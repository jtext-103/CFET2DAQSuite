using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.CFET2.Things.BasicAIModel;
using Newtonsoft.Json;

namespace Jtext103.CFET2.Things.NIScopeDAQAI
{
    public class NIScopeAIStaticConfig : BasicAIStaticConfig
    {
        /// <summary>
        /// 资源名称（卡名）
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// 是否允许采集多于板卡内存的点
        /// </summary>
        public bool MoreRecordsThanMemoryAllowed { get; set; }

        /// <summary>
        /// 使用默认参数初始化配置文件属性
        /// </summary>
        public NIScopeAIStaticConfig()
        {
            TriggerConfig = new AITriggerConfiguration();
            ClockConfig = new AIClockConfiguration();
            ChannelConfig = new AIChannelConfiguration();
        }

        /// <summary>
        /// 通过配置文件构造实例
        /// </summary>
        /// <param name="filePath"></param>
        public NIScopeAIStaticConfig(string filePath)
        {
            NIScopeAIStaticConfig config = (NIScopeAIStaticConfig)InitFromConfigFile(filePath);
            TriggerConfig = config.TriggerConfig;
            ClockConfig = config.ClockConfig;
            ChannelConfig = config.ChannelConfig;
            StartTime = config.StartTime;
            AutoWriteDataToFile = config.AutoWriteDataToFile;
            ChannelCount = config.ChannelCount;
            RemainShotsMax = config.RemainShotsMax;
            RemainShotsMin = config.RemainShotsMin;
            IsOn = config.IsOn;

            ResourceName = config.ResourceName;
            //MoreRecordsThanMemoryAllowed = config.MoreRecordsThanMemoryAllowed;

            //暂时固定这样
            MoreRecordsThanMemoryAllowed = false;

            //这里保证相等，也就是说永远只有 1 个 records
            ClockConfig.ReadSamplePerTime = config.ClockConfig.TotalSampleLengthPerChannel;
        }

        public bool Save(string filePath)
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(filePath, jsonData);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
