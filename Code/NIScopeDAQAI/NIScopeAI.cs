using Jtext103.CFET2.Things.BasicAIModel;
using NationalInstruments;
using NationalInstruments.ModularInstruments.NIScope;
using NationalInstruments.ModularInstruments.SystemServices.TimingServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Jtext103.CFET2.Things.NIScopeDAQAI
{
    public class NIScopeAI : IBasicAI
    {
        //每个采集卡一个实例
        private NIScope scopeSession;

        //同步时钟，所有需要同步的卡只有一个这个实例，所以除了 Master，别的都是 null
        private TClock tClockSession;

        private Status aiState;

        private NIScopeAIStaticConfig _staticConfig;

        #region public AI status
        public Status AIState
        {
            get
            {
                return aiState;
            }
            private set
            {
                if (aiState != value)
                {
                    aiState = value;

                    //状态改变时，产生OnStatusChanged事件
                    OnStatusChanged();
                }
            }
        }

        /// <summary>
        /// 获取上一炮时间
        /// </summary>
        public DateTime LastShotTime { get; private set; }

        /// <summary>
        /// 获取采集相关配置
        /// </summary>
        public BasicAIStaticConfig StaticConfig
        {
            get
            {
                return _staticConfig;
            }
        }

        public string ConfigFilePath { get; internal set; }

        /// <summary>
        /// 采集数据是否需要转置：不需要
        /// </summary>
        public bool DataNeedTransposeWhenSaving
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region 事件相关
        public event EventHandler RaiseAITaskStopEvent;
        public event EventHandler RaiseStatusChangeEvent;
        public event EventHandler<DataArrivalEventArgs> RaiseDataArrivalEvent;

        /// <summary>
        /// 发布 RaiseAITaskStopEvent 事件
        /// </summary>
        protected virtual void OnAITaskStopped()
        {
            LastShotTime = DateTime.UtcNow;
            RaiseAITaskStopEvent?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// 发布 RaiseStatusChangeEvent 事件
        /// </summary>
        protected virtual void OnStatusChanged()
        {
            RaiseStatusChangeEvent?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// 发布 RaiseDataArrivelEvent 事件
        /// </summary>
        protected virtual void OnDataArrival(double[,] data)
        {
            //如果传过来了数据，产生事件
            if (data.GetLength(1) > 0)
            {
                RaiseDataArrivalEvent?.Invoke(this, new DataArrivalEventArgs(data));
            }
        }
        #endregion

        public NIScopeAI()
        {
            LastShotTime = DateTime.UtcNow;
            aiState = Status.Idle;
        }

        /// <summary>
        /// 从配置文件为已经存在的 basicAI 赋初值
        /// </summary>
        /// <param name="configFilePath"></param>
        public void InitAI(string configFilePath)
        {
            _staticConfig = LoadStaticConfig(configFilePath) as NIScopeAIStaticConfig;
        }

        /// <summary>
        /// 实例化新的 AI，可以从配置文件生成有内容的，也可以是空的
        /// </summary>
        /// <param name="configFilePath">给 null 或 "" 表示实例化一个空的，否则给配置文件路径</param>
        /// <returns></returns>
        public BasicAIStaticConfig LoadStaticConfig(string configFilePath)
        {
            if (configFilePath == "" || configFilePath == null)
            {
                return new NIScopeAIStaticConfig();
            }
            ConfigFilePath = configFilePath;
            return new NIScopeAIStaticConfig(configFilePath);
        }

        public void ChangeStaticConfig(BasicAIStaticConfig basicAIStaticConfig)
        {
            _staticConfig = (NIScopeAIStaticConfig)basicAIStaticConfig;
        }

        public bool SaveStaticConfig()
        {
            return _staticConfig.Save(ConfigFilePath);
        }

        /// <summary>
        /// 实例化并启动采集任务
        /// </summary>
        public async void TryArmTask()
        {
            Task.Run(() =>
            {
                RealArm();
            });

        }

        private void RealArm()
        {
            if (AIState != Status.Idle)
            {
                throw new Exception("If you want to arm, the AI state must be 'Idle'!");
            }
            else
            {
                if (scopeSession == null)
                {
                    try
                    {
                        //新建任务
                        scopeSession = new NIScope(_staticConfig.ResourceName, false, false);

                        //配置任务
                        NIScopeAIConfigMapper.MapAndConfigAll(scopeSession, _staticConfig, ref tClockSession);

                        //获取并设置通道数
                        _staticConfig.ChannelCount = scopeSession.Channels.Count;

                        AIState = Status.Ready;

                        if (_staticConfig.TriggerConfig.MasterOrSlave == AITriggerMasterOrSlave.NonSync)
                        {
                            scopeSession.Measurement.Initiate();
                        }
                        // Master 连同所有 Slaves 只需要一个 tClockSession.Initiate()
                        else if (_staticConfig.TriggerConfig.MasterOrSlave == AITriggerMasterOrSlave.Master)
                        {
                            tClockSession.Initiate();
                            TClockDevice.IsMasterReady = true;
                        }
                        //注意，这里从 Slave 是不会 Initiate 的，Slave 必须被 Master 带，也就是 Slave 需要等待 Master 准备就绪才能采集
                        else
                        {
                            while (!TClockDevice.IsSlaveCanAddIntoTDevice)
                            {
                                Thread.Sleep(300);
                            }
                            //等待主卡 Initiate
                            while (!TClockDevice.IsMasterReady)
                            {
                                Thread.Sleep(200);
                            }

                            lock (TClockDevice.Lock)
                            {
                                //表示没有完成，主卡不能 Idle
                                if (TClockDevice.SlaveOver.ContainsKey(_staticConfig.ResourceName))
                                {
                                    TClockDevice.SlaveOver[_staticConfig.ResourceName] = false;
                                }
                                else
                                {
                                    TClockDevice.SlaveOver.Add(_staticConfig.ResourceName, false);
                                }
                            }
                        }

                        //开始读取数据
                        readData(scopeSession);
                    }
                    catch (Exception e)
                    {
                        TryStopTask();
                        Console.WriteLine("炸了");
                        throw new Exception("任务因错误终止！" + e.ToString());
                    }
                }
            }
        }

        //读数据
        private void readData(NIScope scopeSession)
        {
            var channels = ((JArray)_staticConfig.ChannelConfig.ChannelName).ToObject<List<int>>();
            //开新线程等待读数据
            //await Task.Run(() =>
            //{
            int totalReadDataLength = 0;
            PrecisionTimeSpan timeout = new PrecisionTimeSpan(1000000);  //无timeOut          
            AnalogWaveformCollection<double> scopeWaveform = null;

            double[,] readData = new double[_staticConfig.ChannelCount, _staticConfig.ClockConfig.TotalSampleLengthPerChannel];

            //生成 channels
            string channelScope = null;
            var sChannels = ((JArray)_staticConfig.ChannelConfig.ChannelName).ToObject<List<int>>();
            foreach (var s in sChannels)
            {
                channelScope += s + ",";
            }
            channelScope = channelScope.Substring(0, channelScope.Length - 1);

            //一次采完
            //经过测试，发现在默认设置（不设置其它）情况下：
            // 1、如果多次 FetchDouble，则每 2 次的数据不连续（中间断一截）
            // 2、如果设置多个 records，则每 2 个 record 之间的数据不连续（中间断一截）
            //因此暂时只允许一个 record
            scopeWaveform = scopeSession.Channels[channelScope].Measurement.FetchDouble(timeout, _staticConfig.ClockConfig.TotalSampleLengthPerChannel, scopeWaveform);

            //一旦上面这里获取到数据，则表示采集开始
            AIState = Status.Running;

            // i 是读取次数
            // j 是通道计数
            // k 是每一波的每个点
            double[] temp;
            AnalogWaveform<double> waveform;

            for (int i = 0; i < _staticConfig.ClockConfig.TotalSampleLengthPerChannel / _staticConfig.ClockConfig.ReadSamplePerTime; i++)
            {
                //将一个通道的数据拷贝到 data[,] 的一行
                for (int j = 0; j < _staticConfig.ChannelCount; j++)
                {
                    waveform = scopeWaveform[i, j];
                    temp = waveform.GetRawData();
                    for (int k = 0; k < _staticConfig.ClockConfig.ReadSamplePerTime; k++)
                    {
                        readData[j, i * _staticConfig.ClockConfig.ReadSamplePerTime + k] = temp[k];
                    }
                }

                totalReadDataLength += _staticConfig.ClockConfig.ReadSamplePerTime;

            }
            GC.Collect();
            Thread.Sleep(2000);

            //发布数据到达事件
            OnDataArrival(readData);

            //发布停止任务事件
            OnAITaskStopped();

            //等待，让外部保证获取到 Running 状态
            Thread.Sleep(2000);
            //});
        }

        public bool TryStopTask()
        {
            //如果是主卡，判断是否有任何从卡没有完成，否则不能出来
            if (_staticConfig.TriggerConfig.MasterOrSlave == AITriggerMasterOrSlave.Master)
            {
                bool canOut = false;
                while (!canOut)
                {
                    canOut = true;
                    lock (TClockDevice.Lock)
                    {
                        foreach (var s in TClockDevice.SlaveOver)
                        {
                            if (s.Value == false)
                            {
                                canOut = false;
                            }
                        }
                    }
                }
            }
            //如果是从卡，设置让主卡可以出来
            else if (_staticConfig.TriggerConfig.MasterOrSlave == AITriggerMasterOrSlave.Slave)
            {
                lock (TClockDevice.Lock)
                {
                    TClockDevice.SlaveOver[_staticConfig.ResourceName] = true;
                }
            }
            lock (TClockDevice.Lock)
            {
                if (TClockDevice.SyncDevices.Contains(scopeSession))
                {
                    TClockDevice.SyncDevices.Remove(scopeSession);
                }
            }
            try
            {
                if (tClockSession != null)
                {
                    tClockSession = null;
                }
                if (scopeSession != null)
                {
                    scopeSession.Close();
                    scopeSession.Dispose();
                    scopeSession = null;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            AIState = Status.Idle;
            return true;
        }

        #region IDisposable Support
        //todo:研究下面代码实际作用
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    TryStopTask();
                }
                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
