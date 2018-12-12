using Jtext103.CFET2.Things.BasicAIModel;
using JYPXI62022;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.CFET2.Things.JyAiLib
{
    /// <summary>
    /// 从配置文件中读取属性，配置简仪AI任务
    /// </summary>
    public static class JYAIConfigMapper
    {
        /// <summary>
        /// 使用AIChannelConfiguration进行简仪采集卡通道配置
        /// </summary>
        /// <param name="jyTask">需要配置的简仪采集卡任务</param>
        /// <param name="channelConfiguration">通道配置</param>
        public static void MapAndConfigChannel(JYPXI62022AITask jyTask, AIChannelConfiguration channelConfiguration)
        {
            if (channelConfiguration.TerminalConfigType != AITerminalType.Differential)
            {
                throw new Exception("该简仪采集卡只能配置为差分输入！");
            }
            //简仪采集卡通道用int表示，应该是一个int的集合
            var channels = (IList<int>)channelConfiguration.ChannelName;
            for (int i = 0; i < channels.Count(); i++)
            {
                jyTask.AddChannel(channels[i], channelConfiguration.MinimumValue, channelConfiguration.MaximumValue);
            }
        }

        /// <summary>
        /// 使用AIClockConfiguration进行简仪采集卡时钟配置
        /// </summary>
        /// <param name="jyTask">需要配置的简仪采集卡任务</param>
        /// <param name="clockConfiguration">时钟配置</param>
        public static void MapAndConfigClock(JYPXI62022AITask jyTask, AIClockConfiguration clockConfiguration)
        {
            //if (Enum.Parse(typeof(AIClockSource), clockConfiguration.ClkSource.ToString()).Equals(AIClockSource.Internal))
            if (Enum.ToObject(typeof(AIClockSource), clockConfiguration.ClkSource).Equals(AIClockSource.Internal))
            {
                //用内部时钟
                jyTask.ClockSource = AIClockSource.Internal;
            }
            else
            {
                //ClockSource具体怎么改，只能找简仪要范例
                throw new Exception("我还不知道怎么用外部时钟，咨询简仪吧！");
            }
            //采样率
            jyTask.SampleRate = clockConfiguration.SampleRate;
            //采样方式（有限、无限、单点）
            switch (clockConfiguration.SampleQuantityMode)
            {
                case AISamplesMode.ContinuousSamples:
                    jyTask.Mode = AIMode.Continuous;
                    break;
                case AISamplesMode.FiniteSamples:
                    jyTask.Mode = AIMode.Finite;
                    //每通道采样数
                    jyTask.SamplesToAcquire = clockConfiguration.TotalSampleLengthPerChannel;
                    break;
                case AISamplesMode.HardwareTimedSinglePoint:
                    jyTask.Mode = AIMode.Single;
                    break;
                default:
                    throw new Exception("该简仪采集卡采样方式配置错误！");
            }
            //时钟边沿
            switch (clockConfiguration.ClkActiveEdge)
            {
                case Edge.Falling:
                    jyTask.ClockEdge = AIClockEdge.Falling;
                    break;
                case Edge.Rising:
                    jyTask.ClockEdge = AIClockEdge.Rising;
                    break;
                default:
                    throw new Exception("时钟边沿配置错误！");
            }
        }

        /// <summary>
        /// 使用AITriggerConfiguration进行简仪采集卡触发及多卡同步配置
        /// </summary>
        /// <param name="jyTask">需要配置的简仪采集卡任务</param>
        /// <param name="triggerConfiguration">触发配置</param>
        public static void MapAndConfigTrigger(JYPXI62022AITask jyTask, AITriggerConfiguration triggerConfiguration)
        {
            jyTask.Trigger.Mode = AITriggerMode.Start;
            jyTask.Trigger.ReTriggerCount = 0;
            jyTask.Trigger.PreTriggerSamples = 0;
            jyTask.Trigger.Delay = 0;

            switch (triggerConfiguration.TriggerType)
            {
                case BasicAIModel.AITriggerType.Immediate:
                    //无触发
                    jyTask.Trigger.Type = JYPXI62022.AITriggerType.Immediate;
                    break;
                case BasicAIModel.AITriggerType.DigitalTrigger:
                    //外部数字触发
                    jyTask.Trigger.Type = JYPXI62022.AITriggerType.Digital;
                    //触发边沿
                    switch (triggerConfiguration.TriggerEdge)
                    {
                        case Edge.Falling:
                            jyTask.Trigger.Digital.Edge = AIDigitalTriggerEdge.Falling;
                            break;
                        case Edge.Rising:
                            jyTask.Trigger.Digital.Edge = AIDigitalTriggerEdge.Rising;
                            break;
                        default:
                            throw new Exception("触发边沿配置错误！");
                    }
                    //触发源
                    jyTask.Trigger.Digital.Source = (AIDigitalTriggerSource)Enum.ToObject(typeof(AIDigitalTriggerSource), triggerConfiguration.TriggerSource);
                    break;
                case BasicAIModel.AITriggerType.AnalogTrigger:
                    throw new Exception("该简仪采集卡无法使用模拟触发！");
                default:
                    throw new Exception("触发方式配置错误！");
            }
            //主从卡不同，配置不同
            switch (triggerConfiguration.MasterOrSlave)
            {
                case AITriggerMasterOrSlave.NonSync:
                    //不需要设置主从
                    jyTask.Sync.Topology = SyncTopology.Independent;                    
                    break;
                case AITriggerMasterOrSlave.Master:
                    jyTask.Sync.Topology = SyncTopology.Master;
                    //主卡需要触发路由
                    //SSI的意思就是背板某条触发总线，驱动底层自动map
                    jyTask.Sync.TriggerRouting = SyncTriggerRouting.SSI;
                    jyTask.Sync.TimeBaseRouting = SyncTimeBaseRouting.SSI;
                    break;
                case AITriggerMasterOrSlave.Slave:
                    jyTask.Sync.Topology = SyncTopology.Slave;                    
                    //从卡不需要配置触发路由
                    jyTask.Sync.TriggerRouting = SyncTriggerRouting.SSI;
                    jyTask.Sync.TimeBaseRouting = SyncTimeBaseRouting.SSI;
                    //覆盖之前设置的触发属性，应为digitial触发，触发源SSI
                    jyTask.Trigger.Type = JYPXI62022.AITriggerType.Digital;
                    jyTask.Trigger.Digital.Edge = AIDigitalTriggerEdge.Rising;
                    jyTask.Trigger.Digital.Source = AIDigitalTriggerSource.SSI;
                    break;
                default:
                    throw new Exception("该简仪采集卡触发主从设置错误！");
            }

        }

        /// <summary>
        /// 配置简仪采集卡AI任务触发、同步、通道、时钟等各项属性
        /// </summary>
        /// <param name="jyTask"></param>
        /// <param name="basicAIConifg"></param>
        public static void MapAndConfigAll(JYPXI62022AITask jyTask, BasicAIStaticConfig basicAIConifg)
        {
            MapAndConfigChannel(jyTask, basicAIConifg.ChannelConfig);
            MapAndConfigClock(jyTask, basicAIConifg.ClockConfig);
            MapAndConfigTrigger(jyTask, basicAIConifg.TriggerConfig);
        }
    }
}
