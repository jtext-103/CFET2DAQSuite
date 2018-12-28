using Jtext103.CFET2.Things.BasicAIModel;
using NationalInstruments.ModularInstruments.NIScope;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments;
using NationalInstruments.ModularInstruments;
using NationalInstruments.ModularInstruments.SystemServices.TimingServices;

namespace Jtext103.CFET2.Things.NIScopeDAQAI
{
    public class NIScopeAIConfigMapper
    {
        private static void MapAndConfigTrigger(NIScope scopeSession, AITriggerConfiguration triggerConfiguration, ref TClock tClockSession)
        {
            //设置触发模式
            switch (triggerConfiguration.TriggerType)
            {
                case AITriggerType.Immediate:
                    scopeSession.Trigger.Type = ScopeTriggerType.Immediate;
                    break;
                case AITriggerType.DigitalTrigger:
                    scopeSession.Trigger.Type = ScopeTriggerType.DigitalEdge;
                    break;
                case AITriggerType.AnalogTrigger:
                    scopeSession.Trigger.Type = ScopeTriggerType.Edge;
                    break;
                default:
                    throw new Exception("触发模式 TriggerType 设置错误！");
            }
            //设置触发源
            ScopeTriggerSource triggerSource = triggerConfiguration.TriggerSource.ToString();
            //配置触发沿
            ScopeTriggerSlope triggerSlope;
            switch (triggerConfiguration.TriggerEdge)
            {
                case Edge.Rising:
                    triggerSlope = ScopeTriggerSlope.Positive;
                    break;
                case Edge.Falling:
                    triggerSlope = ScopeTriggerSlope.Negative;
                    break;
                default:
                    throw new Exception("触发沿 TriggerEdge 设置错误！");
            }
            scopeSession.Trigger.EdgeTrigger.Configure(triggerSource, 2.5, triggerSlope, ScopeTriggerCoupling.DC, PrecisionTimeSpan.Zero, PrecisionTimeSpan.Zero);
            //配置触发方式
            //如果不是非同步，则加入到TClock中
            if(triggerConfiguration.MasterOrSlave != AITriggerMasterOrSlave.NonSync)
            {
                TClockDevice.SynchronizableDevices.Add(scopeSession);
            }
            //如果是主卡，则将所有的 SynchronizableDevices 实例化出来，这样写也就决定了只能有一个 Master（ NI 机箱可级联支持双 Master）
            //这一行必须是在所有需要同步的卡 scopeSession 设置完毕之后
            if (triggerConfiguration.MasterOrSlave == AITriggerMasterOrSlave.Master)
            {
                //填充为 16 个，最多支持 16 卡同步
                //todo:测试
                while(TClockDevice.SynchronizableDevices.Count < 16)
                {
                    NIScope emptyScope = null;
                    TClockDevice.SynchronizableDevices.Add(emptyScope);
                }
                ITClockSynchronizableDevice[] scopeSynchronizableDevices = new ITClockSynchronizableDevice[16] {
                    TClockDevice.SynchronizableDevices[0], TClockDevice.SynchronizableDevices[1], TClockDevice.SynchronizableDevices[2], TClockDevice.SynchronizableDevices[3],
                    TClockDevice.SynchronizableDevices[4], TClockDevice.SynchronizableDevices[5], TClockDevice.SynchronizableDevices[6], TClockDevice.SynchronizableDevices[7],
                    TClockDevice.SynchronizableDevices[8], TClockDevice.SynchronizableDevices[9], TClockDevice.SynchronizableDevices[10], TClockDevice.SynchronizableDevices[11],
                    TClockDevice.SynchronizableDevices[12], TClockDevice.SynchronizableDevices[13], TClockDevice.SynchronizableDevices[14], TClockDevice.SynchronizableDevices[15]
                };
                tClockSession = new TClock(scopeSynchronizableDevices);
                tClockSession.ConfigureForHomogeneousTriggers();
                tClockSession.Synchronize();
                tClockSession.Initiate();
            }
        }

        private static void MapAndConfigClock(NIScope scopeSession, AIClockConfiguration clockConfiguration)
        {
            //无时钟源设置
            //采样数量，仅支持有限
            switch(clockConfiguration.SampleQuantityMode)
            {
                case AISamplesMode.FiniteSamples:
                    break;
                default:
                    throw new Exception("采样数量 SampleQuantityMode 设置错误！仅支持有限采样（0）！");
            }
            //无使能时钟沿
            //配置采样率，采样总数，采样次数
            int numberOfRecords = clockConfiguration.TotalSampleLengthPerChannel / clockConfiguration.ReadSamplePerTime;
            scopeSession.Timing.ConfigureTiming(clockConfiguration.SampleRate, clockConfiguration.TotalSampleLengthPerChannel, clockConfiguration.ReadSamplePerTime, numberOfRecords, true);
        }

        private static void MapAndConfigChannel(NIScope scopeSession, AIChannelConfiguration channelConfiguration)
        {
            //todo:检查是否真的是0，1，2……
            var channels = ((JArray)channelConfiguration.ChannelName).ToObject<List<int>>();
            double range = channelConfiguration.MaximumValue - channelConfiguration.MinimumValue;
            foreach (var c in channels)
            {
                //配置信号输入方式
                switch (channelConfiguration.TerminalConfigType)
                {
                    //目前只支持差分
                    case AITerminalType.Differential:
                        scopeSession.Channels[c.ToString()].TerminalConfiguration = ScopeChannelTerminalConfiguration.Differential;
                        break;
                    default:
                        throw new Exception("输入方式 AITerminalType 定义无效！");
                }
                scopeSession.Channels[c.ToString()].Configure(range, 0, ScopeVerticalCoupling.DC, 1.0, true);
            }
        }

        /// <summary>
        /// 配置已经实例化的 scopeSession，包括通道设置、时钟设置以及触发设置
        /// </summary>
        /// <param name="niTask"></param>
        /// <param name="channelConfiguration"></param>
        public static void MapAndConfigAll(NIScope scopeSession, NIScopeAIStaticConfig basicAIConifg, ref TClock tClock)
        {
            scopeSession.Timing.MoreRecordsThanMemoryAllowed = basicAIConifg.MoreRecordsThanMemoryAllowed;
    
            MapAndConfigTrigger(scopeSession, basicAIConifg.TriggerConfig, ref tClock);
            MapAndConfigClock(scopeSession, basicAIConifg.ClockConfig);
            MapAndConfigChannel(scopeSession, basicAIConifg.ChannelConfig);
        }
    }
}
