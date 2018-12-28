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
        private TClock tClock;

        private Status aiState;

        private NIScopeAIStaticConfig staticConfig;

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
                    //todo: OnStatusChanged();
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
                return staticConfig;
            }
        }

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
            staticConfig = LoadStaticConfig(configFilePath) as NIScopeAIStaticConfig;
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
            return new NIScopeAIStaticConfig(configFilePath);
        }

        /// <summary>
        /// 实例化并启动采集任务
        /// </summary>
        public void TryArmTask()
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
                        scopeSession = new NIScope(staticConfig.ResourceName, false, false);

                        //配置任务
                        NIScopeAIConfigMapper.MapAndConfigAll(scopeSession, staticConfig, ref tClock);

                        //获取并设置通道数
                        staticConfig.ChannelCount = scopeSession.Channels.Count;

                        //开始读取数据
                        readData(scopeSession);
                    }
                    catch(Exception e)
                    {
                        throw new Exception(e.ToString());
                    }
                }
            }
        }

        //读数据
        private async void readData(NIScope scopeSession)
        {
            var channels = ((JArray)staticConfig.ChannelConfig.ChannelName).ToObject<List<int>>();
            //开新线程等待读数据
            await Task.Run(() =>
            {
                int totalReadDataLength = 0;
                PrecisionTimeSpan timeout = new PrecisionTimeSpan(-1);  //无timeOut

                //只要任务没结束，则一直循环等待
                do
                {
                    double[,] readData = new double[staticConfig.ChannelCount, staticConfig.ClockConfig.ReadSamplePerTime];
                    //读取数据
                    //todo:看看能不能直接所有通道读出来
                    AnalogWaveformCollection<double> scopeWaveform = null;
                    for(int i = 0; i < channels.Count; i++)
                    {
                        scopeWaveform = scopeSession.Channels[channels[i].ToString()].Measurement.FetchDouble(timeout, staticConfig.ClockConfig.ReadSamplePerTime, scopeWaveform);
                        //将一个通道的数据拷贝到 data[,] 的一行
                        //todo:用不这么费事的办法
                        for(int j = 0; j < staticConfig.ClockConfig.ReadSamplePerTime; j++)
                        {
                            //todo:Test
                            readData[i, j] = scopeWaveform[0].Samples[i].Value;
                        }

                    }
                    //发布数据到达事件
                    OnDataArrival(readData);
                    totalReadDataLength += staticConfig.ClockConfig.ReadSamplePerTime;
                    //第一次读到数据时会改变任务状态
                    if (AIState == Status.Ready)
                    {
                        //ready -> running
                        AIState = Status.Running;
                    }
                    //当读够数据则停止
                    if (totalReadDataLength >= staticConfig.ClockConfig.TotalSampleLengthPerChannel)
                    {
                        //发布停止任务事件
                        OnAITaskStopped();
                        break;
                    }
                    //等待1/3每次读取间隔时间
                    Thread.Sleep(Convert.ToInt32(staticConfig.ClockConfig.ReadSamplePerTime * 1000 / staticConfig.ClockConfig.SampleRate / 3));
                }
                while (true);
            });
        }

        public bool TryStopTask()
        {
            if (scopeSession != null)
            {
                try
                {
                    scopeSession.Close();
                    scopeSession = null;
                    scopeSession.Dispose();
                }
                catch (Exception e)
                {
                    throw new Exception(e.ToString());
                }
                return true;
            }
            return false;
        }

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
    }
}
