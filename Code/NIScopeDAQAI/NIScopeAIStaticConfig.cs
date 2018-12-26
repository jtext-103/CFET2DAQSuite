using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.CFET2.Things.BasicAIModel;

namespace Jtext103.CFET2.Things.NIScopeDAQAI
{
    public class NIScopeAIStaticConfig:BasicAIStaticConfig
    {
        /// <summary>
        /// 资源名称（卡名）
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// 使用默认参数初始化配置文件属性
        /// </summary>
        public NIScopeAIStaticConfig()
        {
            ////没有配置文件路径时，初始化生成的模板
            //TriggerConfig = new AITriggerConfiguration()
            //{
            //    TriggerType = AITriggerType.DigitalTrigger,
            //    TriggerSource = "/PXI1Slot3/ai/StartTrigger",
            //    TriggerEdge = Edge.Rising,
            //    MasterOrSlave = AITriggerMasterOrSlave.Slave
            //};
            //ClockConfig = new AIClockConfiguration()
            //{
            //    ClkSource = "",
            //    SampleQuantityMode = AISamplesMode.FiniteSamples,
            //    SampleRate = 1000,
            //    ClkActiveEdge = Edge.Rising,
            //    TotalSampleLengthPerChannel = 1000,
            //    ReadSamplePerTime = 500
            //};
            //ChannelConfig = new AIChannelConfiguration()
            //{
            //    ChannelName = "PXI1Slot4/ai0:3",
            //    TerminalConfigType = AITerminalType.Differential,
            //    MinimumValue = 0,
            //    MaximumValue = 10
            //};
            //StartTime = 0.5;
            //AutoWriteDataToFile = true;
            //ChannelCount = 4;
            //RemainShotsMax = 30;
            //RemainShotsMin = 20;
        }

        /// <summary>
        /// 通过配置文件构造实例
        /// </summary>
        /// <param name="filePath"></param>
        public NIScopeAIStaticConfig(string filePath)
        {
            NIScopeAIStaticConfig config = (NIScopeAIStaticConfig)InitFromConfigFile(filePath);
            ResourceName = config.ResourceName;
            TriggerConfig = config.TriggerConfig;
            ClockConfig = config.ClockConfig;
            ChannelConfig = config.ChannelConfig;
            StartTime = config.StartTime;
            AutoWriteDataToFile = config.AutoWriteDataToFile;
            ChannelCount = config.ChannelCount;
            RemainShotsMax = config.RemainShotsMax;
            RemainShotsMin = config.RemainShotsMin;
        }
    }
}
