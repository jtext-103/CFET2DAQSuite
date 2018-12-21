using Jtext103.CFET2.Things.BasicAIModel;
using JYPXI62022;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.CFET2.Things.JyAiLib
{
    public class JYAIStaticConfig : BasicAIStaticConfig
    {
        /// <summary>
        /// 简仪板卡号
        /// 命名规则是pxi机箱内简仪板卡从左到右编号0，1，2。。。忽略其他板卡及空插槽
        /// </summary>
        public int BoardNum { get; set; }

        /// <summary>
        /// 使用默认参数初始化配置文件属性
        /// 部署到全新系统时使用该方法产生默认配置文件，并可在此基础上手动修改
        /// 其余情况务必使用有参构造函数通过配置文件构造该类实例
        /// </summary>
        public JYAIStaticConfig()
        {
            //TriggerConfig = new AITriggerConfiguration()
            //{
            //    //一般就用Immediate和Digital
            //    TriggerType = BasicAIModel.AITriggerType.Immediate,
            //    //SSI的意思就是背板某条触发总线，驱动底层自动map
            //    TriggerSource = AIDigitalTriggerSource.SSI,
            //    TriggerEdge = Edge.Rising,
            //    //对于主卡和非非同步卡，以上设置都有效
            //    //对于从卡，TriggerType、TriggerSource和TriggerEdge设置都无效，会分别自动设置为Digital、Rising和SSI
            //    MasterOrSlave = AITriggerMasterOrSlave.NonSync
            //};
            //ClockConfig = new AIClockConfiguration()
            //{
            //    ClkSource = AIClockSource.Internal,
            //    SampleQuantityMode = AISamplesMode.FiniteSamples,
            //    ClkActiveEdge = Edge.Rising,
            //    SampleRate = 1000,
            //    TotalSampleLengthPerChannel = 1000,
            //    ReadSamplePerTime = 500
            //};
            //ChannelConfig = new AIChannelConfiguration()
            //{
            //    ChannelName = new int[] { 0, 1, 2, 3 },
            //    TerminalConfigType = AITerminalType.Differential,
            //    MinimumValue = 0,
            //    MaximumValue = 10
            //};
            //AutoWriteDataToFile = true;
            //BoardNum = 0;
        }

        /// <summary>
        /// 通过配置文件构造实例
        /// </summary>
        /// <param name="filePath"></param>
        public JYAIStaticConfig(string filePath)
        {
            JYAIStaticConfig config = (JYAIStaticConfig)InitFromConfigFile(filePath);
            BoardNum = config.BoardNum;
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
