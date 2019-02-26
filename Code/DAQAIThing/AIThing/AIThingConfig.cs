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
    public partial class AIThing : Thing
    {
        #region Clock
        /// <summary>
        /// 获取采样率
        /// </summary>
        /// <param name="shotNo"></param>
        /// <returns></returns>
        [Cfet2Config(ConfigActions = ConfigAction.Get, Name = "SampleRate")]
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
        /// 设置采样率
        /// </summary>
        /// <param name="sampleRate"></param>
        [Cfet2Config(ConfigActions = ConfigAction.Set, Name = "SampleRate")]
        public void SampleRateSet(double sampleRate)
        {
            basicAI.StaticConfig.ClockConfig.SampleRate = sampleRate;

            //这里需要把ReadSamplePerTime也设置一下
            int gcd = Caculator.GCD((int)sampleRate, basicAI.StaticConfig.ClockConfig.TotalSampleLengthPerChannel);
            if(gcd <= 20000)
            {
                basicAI.StaticConfig.ClockConfig.ReadSamplePerTime = gcd;
            }
            else
            {
                basicAI.StaticConfig.ClockConfig.ReadSamplePerTime = Caculator.GCD(basicAI.StaticConfig.ClockConfig.TotalSampleLengthPerChannel, 20000);
            }
            
            basicAI.ChangeStaticConfig(basicAI.StaticConfig);
        }

        /// <summary>
        /// 获取采样点数
        /// </summary>
        /// <param name="shotNo"></param>
        /// <returns></returns>
        [Cfet2Config(ConfigActions = ConfigAction.Get, Name = "Length")]
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
        /// 设置采样点数
        /// </summary>
        /// <param name="length"></param>
        [Cfet2Config(ConfigActions = ConfigAction.Set, Name = "Length")]
        public void LengthSet(int length)
        {
            basicAI.StaticConfig.ClockConfig.TotalSampleLengthPerChannel = length;
            basicAI.ChangeStaticConfig(basicAI.StaticConfig);
        }

        #endregion

        #region Trigger
        /// <summary>
        /// 获取同步方式
        /// </summary>
        /// <returns></returns>
        [Cfet2Config(ConfigActions = ConfigAction.Get, Name = "SyncType")]
        public AITriggerMasterOrSlave SyncType()
        {
            return basicAI.StaticConfig.TriggerConfig.MasterOrSlave;
        }

        /// <summary>
        /// 设置同步方式
        /// </summary>
        /// <param name="type"></param>
        [Cfet2Config(ConfigActions = ConfigAction.Set, Name = "SyncType")]
        public void SyncTypeSet(AITriggerMasterOrSlave type)
        {
            basicAI.StaticConfig.TriggerConfig.MasterOrSlave = type;
            basicAI.ChangeStaticConfig(basicAI.StaticConfig);
        }

        /// <summary>
        /// 获取触发方式
        /// </summary>
        /// <returns></returns>
        [Cfet2Config(ConfigActions = ConfigAction.Get, Name = "TriggerType")]
        public AITriggerType TriggerType()
        {
            return basicAI.StaticConfig.TriggerConfig.TriggerType;
        }

        /// <summary>
        /// 设置触发方式
        /// </summary>
        /// <returns></returns>
        [Cfet2Config(ConfigActions = ConfigAction.Set, Name = "TriggerType")]
        public void TriggerTypeSet(AITriggerType type)
        {
            basicAI.StaticConfig.TriggerConfig.TriggerType = type;
            basicAI.ChangeStaticConfig(basicAI.StaticConfig);
        }

        /// <summary>
        /// 获取触发通道
        /// </summary>
        [Cfet2Config(ConfigActions = ConfigAction.Get, Name = "TriggerSource")]
        public object TriggerSource()
        {
            return basicAI.StaticConfig.TriggerConfig.TriggerSource;
        }

        /// <summary>
        /// 设置触发通道
        /// </summary>
        [Cfet2Config(ConfigActions = ConfigAction.Set, Name = "TriggerSource")]
        public void TriggerSourceSet(string source)
        {
            basicAI.StaticConfig.TriggerConfig.TriggerSource = source;
            basicAI.ChangeStaticConfig(basicAI.StaticConfig);
        }

        #endregion

        #region Channel
        /// <summary>
        /// 获取通道名
        /// </summary>
        /// <returns></returns>
        [Cfet2Config(ConfigActions = ConfigAction.Get, Name = "ChannelName")]
        public string ChannelName()
        {
            return basicAI.StaticConfig.ChannelConfig.ChannelName;
        }

        [Cfet2Config(ConfigActions = ConfigAction.Set, Name = "ChannelName")]
        public void ChannelNameSet(string channelName)
        {
            //注意这里的替换行为
            channelName = channelName.Replace(".", @"/");
            basicAI.StaticConfig.ChannelConfig.ChannelName = channelName;
            basicAI.ChangeStaticConfig(basicAI.StaticConfig);
        }

        /// <summary>
        /// 获取Channel个数
        /// </summary>
        /// <param name="shotNo"></param>
        /// <returns></returns>
        [Cfet2Config(ConfigActions = ConfigAction.Get, Name = "ChannelCount")]
        public int ChannelCount(int shotNo = -1)
        {
            //0代表当前配置文件中的status配置
            if (shotNo == -1)
            {
                return basicAI.StaticConfig.ChannelCount;
            }
            //其余参数是从已保存的文件中读取
            return tryUpdateStaticConfigDic(shotNo).ChannelCount;
        }

        /// <summary>
        /// 设置Channel个数
        /// </summary>
        /// <param name="channelCount"></param>
        [Cfet2Config(ConfigActions = ConfigAction.Set, Name = "ChannelCount")]
        public void ChannelCountSet(int channelCount)
        {
            basicAI.StaticConfig.ChannelCount = channelCount;
            basicAI.ChangeStaticConfig(basicAI.StaticConfig);
        }

        #endregion

    }
}
