using Jtext103.CFET2.CFET2App.ExampleThings;
using Jtext103.CFET2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.CFET2.Things.DAQAIThing;
using Jtext103.CFET2.Things.NiAiLib;
using Jtext103.CFET2.Things.DAQDataUploadThing;
using JTextDAQDataFileOperator.HDF5;
using Jtext103.CFET2.NancyHttpCommunicationModule;
using Jtext103.CFET2.Things.DicServer;
using ViewCopy;

namespace Jtext103.CFET2.CFET2App
{
    partial  class Cfet2Program : CFET2Host
    {
        private void AddThings()
        {
            //nancy HTTP
            var nancyCM = new NancyCommunicationModule(new Uri("http://localhost:8000"));
            MyHub.TryAddCommunicationModule(nancyCM);

            //拷贝视图文件夹
            var myViewsCopyer = new ViewCopyer();
            myViewsCopyer.StartCopy();
            var myContentCopyer = new ViewCopyer(null, "Content");
            myContentCopyer.StartCopy();

            //Dic
            var dic = new DicServerThing();
            MyHub.TryAddThing(dic, "/", "Dic", @"D:\Run\ConfigFile\DAQFamilyBucket\Dic.txt");

            //采集卡
            var niMaster = new AIThing();
            
            niMaster.basicAI = new NIAI();
            niMaster.DataFileFactory = new HDF5DataFileFactory();

            MyHub.TryAddThing(niMaster,
                                @"/",
                                "Card0",
                                new { ConfigFilePath = @"D:\Run\ConfigFile\DAQFamilyBucket\niMaster.txt", DataFileParentDirectory = @"D:\Data\ni\Card0" });

            //采集卡管理
            var aiManagement = new AIManagementThing();
            MyHub.TryAddThing(aiManagement,
                                @"/",
                                "aimanagement",
                                new
                                {
                                    AllAIThingPaths = new string[] { "/Card0" },
                                    AutoArmAIThingPaths = new string[] {  }
                                });

            //上传文件
            var uploader = new DataUpLoadThing();
            MyHub.TryAddThing(uploader, @"/", "uploader", @"D:\Run\ConfigFile\DAQFamilyBucket\DataUploadConfig.txt");
        }
    }
}
