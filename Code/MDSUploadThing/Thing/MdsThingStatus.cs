using Jtext103.CFET2.Core;
using Jtext103.CFET2.Core.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDSplusVBC;
using Jtext103.CFET2.Core.Event;

namespace Jtext103.CFET2.Things.MDSUpload
{
    public partial class MdsThing : Thing
    {
        /// <summary>
        /// 用来存储 Upload 的当前状态
        /// Idle 为可以上传，Uploading 为正在上传，此时不能调用上传方法
        /// </summary>
        [Cfet2Status]
        public Status State { get; internal set; }

        [Cfet2Status]
        public string SlavePath
        {
            get
            {
                if(myConfig.MasterOrSlave == 1)
                {
                    return myConfig.SlavePath;
                }
                return "Only for master";
            }
        }

        [Cfet2Status]
        public string Host
        {
            get
            {
                return myConfig.ServerConfig.Host;
            }
        }

        [Cfet2Status]
        public string Tree
        {
            get
            {
                return myConfig.ServerConfig.Tree;
            }
        }

        /// <summary>
        /// 显示所有的事件和状态
        /// </summary>
        [Cfet2Status]
        public string ShowEvents
        {
            get
            {
                string result;
                if (myConfig.MasterOrSlave == 2)
                {
                    return "Only for master";
                }
                result = null;
                foreach (var s in myConfig.EventPaths)
                {
                    // 8 为 /AIState 的长度
                    result += s + "\n";
                }
                return result.Substring(0, result.Length - 1);
            }
        }

        /// <summary>
        /// 显示配置文件中所有通道
        /// </summary>
        [Cfet2Status]
        public string ShowSources
        {
            get
            {
                string result = null;
                foreach (var s in myConfig.RealChannelsDic)
                {
                    result += s.Key.ToString() + "\n";
                }
                return result.Substring(0, result.Length - 1);
            }
        }

        /// <summary>
        /// 显示配置文件中所有上传Tag
        /// </summary>
        [Cfet2Status]
        public string ShowTags
        {
            get
            {
                string result = null;
                foreach (var s in myConfig.RealChannelsDic)
                {
                    result += s.Value.Tag.ToString() + "\n";
                }
                return result.Substring(0, result.Length - 1);
            }
        }
    }
}
