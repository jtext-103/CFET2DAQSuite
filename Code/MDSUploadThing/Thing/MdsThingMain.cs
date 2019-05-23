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
using Jtext103.CFET2.Core.Log;
using System.Diagnostics;
using System.Threading;

namespace Jtext103.CFET2.Things.MDSUpload
{
    public partial class MdsThing : Thing
    {
        private ICfet2Logger logger;

        //存储 Model
        private MdsUploadConfig myConfig;

        //事件成员
        private Token token;

        public override void TryInit(object configFilePath)
        {
            myConfig = new MdsUploadConfig((string[])configFilePath);
            if (myConfig != null)
            {
                State = Status.Idle;
            }
            else
            {
                State = Status.Error;
            }
            logger = Cfet2LogManager.GetLogger("MdsUploadLog");

            // 当为 Master 时，订阅上传触发事件
            if (myConfig.MasterOrSlave == 1)
            {
                for (int i = 0; i < myConfig.EventPaths.Count(); i++)
                {
                    token = MyHub.EventHub.Subscribe(new EventFilter(myConfig.EventPaths[i], myConfig.EventKinds[i]), handler);
                }
            }  
        }

        /// <summary>
        /// Only useful when this MDSThing is Slave
        /// </summary>
        public override void Start()
        {
            if(myConfig.MasterOrSlave == 2)
            {
                System.Diagnostics.Debug.WriteLine("Acitved by Master, start uploading...", DateTime.Now.ToLocalTime().ToString("HH:mm:ss.fff"));
                logger.Info("MdsSlave开始上传");
                StartUploadAllAuto();
            }    
        }

        //锁，控制只有一个进程调用 事件处理程序
        object myCheckLock = new object();

        /// <summary>
        /// Actived when all things ready
        /// Only useful when this MDSThing is Master
        /// </summary>
        /// <param name="e">not used</param>
        private void handler(EventArg e)
        {
            lock(myCheckLock)
            {
                System.Diagnostics.Debug.WriteLine("Starting uploading all...", DateTime.Now.ToLocalTime().ToString("HH:mm:ss.fff"));
                //启动 Slave 程序
                Process.Start(myConfig.SlavePath);
                State = Status.Running;
                Thread.Sleep(5000);
                State = Status.Idle;
            }
        }
    }
}
