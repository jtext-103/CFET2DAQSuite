using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.CFET2.Core;
using Jtext103.CFET2.Core.Attributes;

namespace Jtext103.CFET2.Things.DAQDataUploadThing
{
    public partial class DataUpLoadThing : Thing
    {
        /// <summary>
        /// 用来存储 Upload 的当前状态
        /// Idle 为可以上传，Uploading 为正在上传，此时不能调用上传方法
        /// </summary>
        [Cfet2Status]
        public Status UploadState { get; internal set; }

        /// <summary>
        /// 要上传的本地数据文件名
        /// </summary>
        [Cfet2Status]
        public string LocalDataFileName { get; internal set; }

        /// <summary>
        /// 本地路径配置
        /// </summary>
        /// <returns></returns>
        [Cfet2Status]
        public string LocalDataDirectories
        {
            get
            {
                string result = null;
                foreach (var s in myConfig.LocalDataDirectories)
                {
                    result += s + "\n";
                }
                return result.Substring(0, result.Length - 1);
            }
        }

        /// <summary>
        /// 上传路径配置
        /// </summary>
        /// <returns></returns>
        [Cfet2Status]
        public string ServerDataDirectories
        {
            get
            {
                string result = null;
                foreach (var s in myConfig.ServerDataDirectories)
                {
                    result += s + "\n";
                }
                return result.Substring(0, result.Length - 1);
            }
        }

        /// <summary>
        /// 已有文件存在时行为
        /// </summary>
        /// <returns></returns>
        [Cfet2Status]
        public Behavior UploadBehavior()
        {
            return myConfig.UploadBehavior;
        }

        /// <summary>
        /// 监听的所有事件
        /// </summary>
        [Cfet2Status]
        public string Events
        {
            get
            {
                string result = null;
                for (int i = 0; i < myConfig.EventPaths.Length; i++)
                {
                    result += "--Path--: " + myConfig.EventPaths[i] + "\n";
                    result += "--Kind--: " + myConfig.EventKinds[i] + "\n";
                }
                return result.Substring(0, result.Length - 1);
            }
        }
    }
}
