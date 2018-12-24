using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.CFET2.Things.MDSUpload
{
    public class Channel
    {
        /// <summary>
        /// One channel for one mds node
        /// 注意，这些名字要跟 Json.txt 文件中的名字一模一样（不区分大小写），否则会有错误
        /// </summary>
        public string SourceAIData { get; set; }
        public string SourceAISampleRate { get; set; }
        public string SourceAILength { get; set; }
        public string SourceAIStartTime { get; set; }
        public string Tag { get; set; }
        public bool Enable { get; set; }

        public Channel() { }

        public Channel(Channel channel)
        {
            SourceAIData = channel.SourceAIData;
            SourceAISampleRate = channel.SourceAISampleRate;
            SourceAILength = channel.SourceAILength;
            SourceAIStartTime = channel.SourceAIStartTime;
            Tag = channel.Tag;
            Enable = channel.Enable;
        }   
    }
}
