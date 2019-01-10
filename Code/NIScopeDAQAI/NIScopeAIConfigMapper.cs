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
using System.Threading;

namespace Jtext103.CFET2.Things.NIScopeDAQAI
{
    public class NIScopeAIConfigMapper
    {
        private static void MapAndConfigTrigger(NIScope scopeSession, AITriggerConfiguration triggerConfiguration, ref TClock tClockSession)
        {
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

            //设置触发源
            ScopeTriggerSource triggerSource = triggerConfiguration.TriggerSource.ToString();

            //设置触发模式
            switch (triggerConfiguration.TriggerType)
            {
                case AITriggerType.Immediate:
                    scopeSession.Trigger.Type = ScopeTriggerType.Immediate;
                    scopeSession.Trigger.ConfigureTriggerImmediate();
                    break;
                case AITriggerType.DigitalTrigger:
                    scopeSession.Trigger.Type = ScopeTriggerType.DigitalEdge;
                    scopeSession.Trigger.ConfigureTriggerDigital(triggerSource, triggerSlope, PrecisionTimeSpan.Zero, PrecisionTimeSpan.Zero);
                    break;
                case AITriggerType.AnalogTrigger:
                    scopeSession.Trigger.Type = ScopeTriggerType.Edge;
                    scopeSession.Trigger.EdgeTrigger.Configure(triggerSource, 2.5, triggerSlope, ScopeTriggerCoupling.DC, PrecisionTimeSpan.Zero, PrecisionTimeSpan.Zero);
                    break;
                default:
                    throw new Exception("触发模式 TriggerType 设置错误！");
            }

            //如果不是非同步
            //加入到TClock中
            if (triggerConfiguration.MasterOrSlave != AITriggerMasterOrSlave.NonSync)
            {
                lock (TClockDevice.Lock)
                {
                    if (TClockDevice.SyncDevices == null)
                    {
                        TClockDevice.SyncDevices = new List<NIScope>();
                    }
                    TClockDevice.SyncDevices.Add(scopeSession);
                }
            }

            //如果是主卡，则将所有的 SynchronizableDevices 实例化出来，这样写也就决定了只能有一个 Master（ NI 机箱可级联支持双 Master）
            //这一行必须是在所有需要同步的卡 scopeSession 设置完毕之后
            if (triggerConfiguration.MasterOrSlave == AITriggerMasterOrSlave.Master)
            {
                TClockDevice.IsMasterReady = false;
                TClockDevice.IsSlaveCanAddIntoTDevice = true;

                //因为主卡可能会和从卡同时被 Arm，因此先等待所有从卡设备加入 TClockDevice
                Thread.Sleep(1000);

                TClockDevice.IsSlaveCanAddIntoTDevice = false;

                ITClockSynchronizableDevice[] scopeSynchronizableDevices = new ITClockSynchronizableDevice[TClockDevice.SyncDevices.Count];
                for (int i = 0; i < TClockDevice.SyncDevices.Count; i++)
                {
                    scopeSynchronizableDevices[i] = TClockDevice.SyncDevices[i];
                }
                tClockSession = new TClock(scopeSynchronizableDevices);
                tClockSession.ConfigureForHomogeneousTriggers();
                tClockSession.Synchronize();
            }
        }

        private static void MapAndConfigClock(NIScope scopeSession, AIClockConfiguration clockConfiguration)
        {
            //无时钟源设置
            //采样数量，仅支持有限
            switch (clockConfiguration.SampleQuantityMode)
            {
                case AISamplesMode.FiniteSamples:
                    break;
                default:
                    throw new Exception("采样数量 SampleQuantityMode 设置错误！仅支持有限采样（0）！");
            }
            //无使能时钟沿
            //配置采样率，采样总数，采样次数
            int records = clockConfiguration.TotalSampleLengthPerChannel / clockConfiguration.ReadSamplePerTime;
            scopeSession.Timing.ConfigureTiming(clockConfiguration.SampleRate, clockConfiguration.TotalSampleLengthPerChannel, 0.0, records, true);
        }

        private static void MapAndConfigChannel(NIScope scopeSession, AIChannelConfiguration channelConfiguration)
        {
            //todo:检查是否真的是0，1，2……
            var channels = ((JArray)channelConfiguration.ChannelName).ToObject<List<int>>();
            double range = channelConfiguration.MaximumValue - channelConfiguration.MinimumValue;
            foreach (var c in channels)
            {
                //Todo:信号输入方式无法配置
                //配置信号输入方式
                switch (channelConfiguration.TerminalConfigType)
                {
                    //目前只支持差分
                    case AITerminalType.Differential:
                        scopeSession.Channels[c.ToString()].Configure(range, 0, ScopeVerticalCoupling.DC, 1.0, true);
                        break;
                    default:
                        throw new Exception("输入方式 AITerminalType 定义无效！");
                }
            }
        }

        /// <summary>
        /// 配置已经实例化的 scopeSession，包括通道设置、时钟设置以及触发设置
        /// </summary>
        /// <param name="niTask"></param>
        /// <param name="channelConfiguration"></param>
        public static void MapAndConfigAll(NIScope scopeSession, NIScopeAIStaticConfig basicAIConifg, ref TClock tClockSession)
        {
            if (basicAIConifg.MoreRecordsThanMemoryAllowed == true)
            {
                throw new Exception("暂时不支持超内存采样！");
            }
            scopeSession.Timing.MoreRecordsThanMemoryAllowed = basicAIConifg.MoreRecordsThanMemoryAllowed;

            MapAndConfigTrigger(scopeSession, basicAIConifg.TriggerConfig, ref tClockSession);
            MapAndConfigClock(scopeSession, basicAIConifg.ClockConfig);
            MapAndConfigChannel(scopeSession, basicAIConifg.ChannelConfig);
        }
    }
}
