using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Jtext103.CFET2.Things.MDSUpload
{
    public class MdsUploadConfig
    {
        /// <summary>
        /// 主程序会一直运行，监听到事件后调用从属程序上传，从属程序上传完毕后自动关掉
        /// Master 为 1
        /// Slave 为 2
        /// </summary>
        public int MasterOrSlave { get; set; }

        /// <summary>
        /// .exe file
        /// </summary>
        public string SlavePath { get; set; }

        /// <summary>
        /// 服务器配置
        /// </summary>
        public Server ServerConfig { get; set; }

        /// <summary>
        /// 监听的事件路径
        /// </summary>
        public string[] EventPaths { get; set; }

        /// <summary>
        /// 监听的事件类型，需要和上面一一匹配
        /// </summary>
        public string[] EventKinds { get; set; }

        /// <summary>
        /// 所有要上传的 mds 通道
        /// </summary>
        public Channel[] ChannelConfigs { get; set; }

        /// <summary>
        /// 将 ChannelConfigs 中的内容做成按 SourceAiData （解析完成{,,,}后）索引
        /// </summary>
        public Dictionary<string, Channel> RealChannelsDic { get; set; }

        public MdsUploadConfig() { }

        public MdsUploadConfig(string path)
        {
            //全部 public 参数反序列化
            JsonConvert.PopulateObject(File.ReadAllText(path, Encoding.Default), this);

            //将带 {,,,} 的 Channels 自动生成为完整格式 
            ChannelConfigs = MdsConfigRebuild.ChannelRebuild(ChannelConfigs);

            //将原始顺序的数组转变成以 SourceAiData 为索引的字典
            RealChannelsDic = new Dictionary<string, Channel>();
            foreach (var channel in ChannelConfigs)
            {
                RealChannelsDic.Add(channel.SourceAIData, channel);
            }
        }
    }
}
