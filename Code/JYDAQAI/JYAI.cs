using Jtext103.CFET2.Things.BasicAIModel;
using JYPXI62022;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jtext103.CFET2.Things.JyAiLib
{
    public class JYAI : IBasicAI, IDisposable
    {
        private JYPXI62022AITask aiTask;

        #region public AI status

        private Status _aiState;

        public Status AIState
        {
            get
            {
                return _aiState;
            }
            private set
            {
                if (_aiState != value)
                {
                    _aiState = value;
                    //状态改变时，产生OnStatusChanged事件
                    OnStatusChanged();
                }
            }
        }

        public DateTime LastShotTime { get; private set; }

        public BasicAIStaticConfig StaticConfig
        {
            get
            {
                return _staticConfig;
            }
        }

        /// <summary>
        /// JY的数据需要转置
        /// </summary>
        public bool DataNeedTransposeWhenSaving
        {
            get
            {
                return true;
            }
        }

        public string ConfigFilePath => throw new NotImplementedException();

        /// <summary>
        /// 对应配置文件，所有AI基本属性均从该配置文件中获取
        /// </summary>
        private JYAIStaticConfig _staticConfig;

        #endregion

        #region event

        public event EventHandler RaiseAITaskStopEvent;

        public event EventHandler RaiseStatusChangeEvent;

        public event EventHandler<DataArrivalEventArgs> RaiseDataArrivalEvent;

        /// <summary>
        /// invoke RaiseAITaskStopEvent
        /// </summary>
        protected virtual void OnAITaskStopped()
        {
            LastShotTime = DateTime.UtcNow;
            RaiseAITaskStopEvent?.Invoke(this, new EventArgs());
            //相当于下面的
            //if (StopEventHandler != null)
            //{
            //    StopEventHandler(this, new EventArgs());
            //}
        }

        /// <summary>
        /// invoke RaiseStatusChangeEvent
        /// </summary>
        protected virtual void OnStatusChanged()
        {
            RaiseStatusChangeEvent?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// invoke RaiseDataArrivelEvent
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

        public JYAI()
        {
            AIState = Status.Idle;
            LastShotTime = DateTime.UtcNow;
        }

        /// <summary>
        /// 使用JYAIStaticConfig配置
        /// </summary>
        /// <param name="configFilePath"></param>
        public void InitAI(string configFilePath)
        {
            _staticConfig = LoadStaticConfig(configFilePath) as JYAIStaticConfig;
        }

        /// <summary>
        /// 进入ERROR状态
        /// 状态改为ERROR并停止任务
        /// </summary>
        private void goError()
        {
            AIState = Status.Error;
        }

        /// <summary>
        /// 启动采集
        /// </summary>
        public void TryArmTask()
        {
            if (AIState != Status.Idle)
            {
                throw new Exception("If you want to arm, the AI state must be 'Idle'!");
            }
            else
            {
                if (aiTask == null)
                {
                    try
                    {
                        //新建任务
                        aiTask = new JYPXI62022AITask(_staticConfig.BoardNum);

                        //配置任务
                        JYAIConfigMapper.MapAndConfigAll(aiTask, _staticConfig);

                        //获取并设置通道数
                        _staticConfig.ChannelCount = aiTask.Channels.Count();

                        //开始任务
                        aiTask.Start();
                        
                        //idle -> ready
                        AIState = Status.Ready;

                        //读取数据
                        int channelCount = _staticConfig.ChannelCount;
                        int readSamplePerTime = _staticConfig.ClockConfig.ReadSamplePerTime;
                        ReadData(aiTask, channelCount, readSamplePerTime);
                    }
                    catch (Exception ex)
                    {
                        goError();
                        throw ex;
                    }
                }
            }
        }
        
        private async Task ReadData(JYPXI62022AITask aiTask, int channelCount, int readSamplePerTime)
        {
            //开新线程等待读数据
            await Task.Run(() =>
            {
                int totalReadDataLength = 0;
                //只要任务没结束，则一直循环等待
                do
                {
                    //每次读到的数据
                    //数据格式为“readData[每通道数据个数][通道个数]”，因此需要转置
                    double[,] readData = new double[readSamplePerTime, channelCount];
                    aiTask.ReadData(ref readData, readSamplePerTime, -1);
                    OnDataArrival(readData);
                    totalReadDataLength += readSamplePerTime;
                    //第一次读到数据时会改变任务状态
                    if (AIState == Status.Ready)
                    {
                        //ready -> running
                        AIState = Status.Running;
                    }
                    //当读够数据则停止
                    if (totalReadDataLength >= aiTask.SamplesToAcquire)
                    {
                        OnAITaskStopped();
                        break;
                    }
                    //等待1/3每次读取间隔时间
                    Thread.Sleep(Convert.ToInt32(readSamplePerTime * 1000 / aiTask.SampleRate / 3));
                }
                //while (aiTask.WaitUntilDone(0));
                while (true);
            });
        }

        /// <summary>
        /// 停止采集
        /// </summary>
        /// <returns></returns>
        public bool TryStopTask()
        {
            if (aiTask != null)
            {
                aiTask.Stop();
                aiTask = null;
                AIState = Status.Idle;
                return true;
            }
            else
            {
                return false;
            }
        }

        public BasicAIStaticConfig LoadStaticConfig(string configFilePath)
        {
            if (configFilePath == "" || configFilePath == null)
            {
                return new JYAIStaticConfig();
            }
            return new JYAIStaticConfig(configFilePath);
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    TryStopTask();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~AI() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        void IDisposable.Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }

        #endregion

        public void ChangeStaticConfig(BasicAIStaticConfig basicAIStaticConfig)
        {
            throw new NotImplementedException();
        }

        public bool SaveStaticConfig()
        {
            throw new NotImplementedException();
        }
    }
}
