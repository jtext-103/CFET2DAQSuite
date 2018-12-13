﻿using Jtext103.CFET2.Core;
using Jtext103.CFET2.Core.Attributes;
using Jtext103.CFET2.Core.Extension;
using System;
using System.Collections.Generic;
using Jtext103.CFET2.Things.BasicAIModel;
using Jtext103.CFET2.Things.ShotDirOperate;
using Jtext103.CFET2.Core.Log;
using System.Threading;
using JTextDAQDataFileOperator.Interface;

namespace Jtext103.CFET2.Things.DAQAIThing
{
    public partial class  AIThing : Thing
    {
        /// <summary>
        /// AI任务状态
        /// 这个AIState和IBasicAI下的AIState不是一个，只是初始化都是Idle，然后每次触发StatusChangedHandler都会同步改变
        /// </summary>
        [Cfet2Status]
        public Status AIState
        {
            get
            {
                //return basicAI.AIState;
                return _aiState;
            }
            private set
            {
                if (_aiState != value)
                {
                    _aiState = value;
                    lastTicks = DateTime.Now.Ticks;
                    MyHub.EventHub.Publish(Path, "StateChanged", _aiState);
                    System.Diagnostics.Debug.WriteLine(GetPathFor("AIState") + " = " + _aiState.ToString() + " time: " + DateTime.Now.ToLocalTime().ToString("HH:mm:ss.fff"));
                    logger.Info(GetPathFor("AIState") + " = " + _aiState.ToString("G") + ".");
                }
            }
        }

        /// <summary>
        /// 当前AIStatus持续时间
        /// </summary>
        /// <returns></returns>
        [Cfet2Status]
        public long Time()
        {
            return (DateTime.Now.Ticks - lastTicks) / 10000000;
        }

        /// <summary>
        /// 从文件中读取指定炮、指定通道、指定数目的数据
        /// </summary>
        /// <param name="shotNo"></param>
        /// <param name="channelNo"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [Cfet2Status]
        public double[] Data(int shotNo, int channelNo, int start, int length)
        {
            return DataComplex(shotNo, channelNo, start, length, 1);
        }

        [Cfet2Status]
        public double[] DataComplex(int shotNo, int channelNo, int start, int end, double stepD)
        {
            try
            {
                int step = (int)Math.Ceiling(stepD);
                int length = (end - start) / step;

                string fileDirectory;
                if (shotNo < 0)
                {
                    throw new Exception("读取数据炮号应大于或等于0！");
                }
                else if (shotNo == 0)
                {
                    fileDirectory = DataFileParentDirectory + "\\" + CurrentShotNo.ToString() + "\\";
                }
                else
                {
                    fileDirectory = DataFileParentDirectory + "\\" + shotNo.ToString() + "\\";

                }
                dataReader = DataFileFactory.GetReader(fileDirectory, dataFileName);
                double[] data = dataReader.LoadDataFromFileComplexRam(channelNo, (ulong)start, (ulong)step, (ulong)length, 1);
                return data;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 保存的数据文件名
        /// </summary>
        [Cfet2Status]
        public string[] FullDataFilePaths { get; private set; }

        /// <summary>
        /// 所有通道的最新数据点
        /// </summary>
        [Cfet2Status]
        public double[] LatestData
        {
            get
            {
                return latestData;
            }
        }

        /// <summary>
        /// （已经放电完成的）最新炮的炮号
        /// </summary>
        [Cfet2Status]
        public int CurrentShotNo
        {
            get
            {
                int currentShotNo = ShotDirOperator.FindMaxCompletedShotNo(DataFileParentDirectory);
                if (currentShotNo == 0)
                {
                    //throw new Exception("没有保存任何数据，无法获得（已经放电完成的）上一炮炮号！");
                    return -1;
                }
                else
                {
                    return currentShotNo;
                }
            }
        }

        /// <summary>
        /// 存储当前炮数据及配置文件的文件夹路径
        /// </summary>
        [Cfet2Status]
        public string CurrentFileDirectiory { get; private set; }

        /// <summary>
        /// 最新一炮开始时间
        /// </summary>
        [Cfet2Status]
        public DateTime LastShotTime
        {
            get
            {
                return basicAI.LastShotTime;
            }
        }

        /// <summary>
        /// 采集存储文件夹名称
        /// </summary>
        /// <returns></returns>
        [Cfet2Status]
        public string Name()
        {
            var index = DataFileParentDirectory.LastIndexOf(@"\");
            return DataFileParentDirectory.Substring(index + 1, DataFileParentDirectory.Length  - 1 - index);
        }

        /// <summary>
        /// 当前卡是否是主卡
        /// </summary>
        /// <returns></returns>
        [Cfet2Status]
        public bool IsMaster()
        {
            if(basicAI.StaticConfig.TriggerConfig.MasterOrSlave == AITriggerMasterOrSlave.Master)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 触发方式
        /// </summary>
        [Cfet2Status]
        public AITriggerType TriggerType()
        {
            return basicAI.StaticConfig.TriggerConfig.TriggerType;
        }

        /// <summary>
        /// 采样率
        /// </summary>
        [Cfet2Status]
        public double SampleRate(int shotNo = -1)
        {
            //-1代表当前配置文件中的status配置
            if (shotNo == -1)
            {
                return basicAI.StaticConfig.ClockConfig.SampleRate;
            }
            //其余参数是从已保存的文件中读取
            return tryUpdateStaticConfigDic(shotNo).ClockConfig.SampleRate;
        }

        /// <summary>
        /// 每通道采样数（仅用于有限采样）
        /// </summary>
        [Cfet2Status]
        public int Length(int shotNo = -1)
        {
            //-1代表当前配置文件中的status配置
            if (shotNo == -1)
            {
                return basicAI.StaticConfig.ClockConfig.TotalSampleLengthPerChannel;
            }
            //其余参数是从已保存的文件中读取
            return tryUpdateStaticConfigDic(shotNo).ClockConfig.TotalSampleLengthPerChannel;
        }

        /// <summary>
        /// Channel个数
        /// </summary>
        /// <param name="shotNo"></param>
        /// <returns></returns>
        [Cfet2Status]
        public int ChannelCount(int shotNo = -1)
        {
            //-1代表当前配置文件中的status配置
            if (shotNo == -1)
            {
                return basicAI.StaticConfig.ChannelCount;
            }
            //其余参数是从已保存的文件中读取
            return tryUpdateStaticConfigDic(shotNo).ChannelCount;
        }

        /// <summary>
        /// 采集开始参考时间
        /// </summary>
        /// <param name="shotNo"></param>
        /// <returns></returns>
        [Cfet2Status]
        public double StartTime(int shotNo = -1)
        {
            //-1代表当前配置文件中的status配置
            if (shotNo == -1)
            {
                return basicAI.StaticConfig.StartTime;
            }
            //其余参数是从已保存的文件中读取
            return tryUpdateStaticConfigDic(shotNo).StartTime;
        }

        /// <summary>
        /// 存放所有数据文件的根文件夹路径
        /// </summary>
        [Cfet2Status]
        public string DataFileParentDirectory { get; set; }

        /// <summary>
        /// 超过这个文件数开始自动删除
        /// </summary>
        [Cfet2Status]
        public uint RemainShotsMax()
        {
            return basicAI.StaticConfig.RemainShotsMax;
        }

        /// <summary>
        /// 自动删除数据时保留文件个数
        /// </summary>
        [Cfet2Status]
        public uint RemainShotMin()
        {
            return basicAI.StaticConfig.RemainShotsMin;
        }

        #region status not used fequently

        /// <summary>
        /// 终端输入方式（差分、单端）
        /// </summary>
        [Cfet2Status]
        public AITerminalType TerminalConfigType()
        {
            return basicAI.StaticConfig.ChannelConfig.TerminalConfigType;
        }

        /// <summary>
        /// 输入最小值
        /// </summary>
        [Cfet2Status]
        public double MinimumValue()
        {
            return basicAI.StaticConfig.ChannelConfig.MinimumValue;
        }

        /// <summary>
        /// 输入最大值
        /// </summary>
        [Cfet2Status]
        public double MaximumValue()
        {
            return basicAI.StaticConfig.ChannelConfig.MaximumValue;
        }

        /// <summary>
        /// 触发通道（仅用于外部触发）
        /// </summary>
        [Cfet2Status]
        public object TriggerSource()
        {
            return basicAI.StaticConfig.TriggerConfig.TriggerSource;
        }

        /// <summary>
        /// 触发边沿
        /// </summary>
        [Cfet2Status]
        public Edge TriggerEdge()
        {
            return basicAI.StaticConfig.TriggerConfig.TriggerEdge;
        }

        /// <summary>
        /// 时钟源（使用内部时钟时，为空字符串）
        /// </summary>
        [Cfet2Status]
        public object ClkSource()
        {
            return basicAI.StaticConfig.ClockConfig.ClkSource;
        }

        /// <summary>
        /// 时钟边沿
        /// </summary>
        [Cfet2Status]
        public Edge ClkActiveEdge()
        {
            return basicAI.StaticConfig.ClockConfig.ClkActiveEdge;
        }

        /// <summary>
        /// 采样方式（有限，无限，单点）
        /// </summary>
        [Cfet2Status]
        public AISamplesMode SampleQuantityMode(int shotNo = -1)
        {
            return basicAI.StaticConfig.ClockConfig.SampleQuantityMode;
        }

        /// <summary>
        /// 每次读取数据个数，保证该值能被“每通道采样数”整除
        /// </summary>
        [Cfet2Status]
        public int ReadSamplePerTime()
        {
            return basicAI.StaticConfig.ClockConfig.ReadSamplePerTime;
        }

        #endregion

    }
}