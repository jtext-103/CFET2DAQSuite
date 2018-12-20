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
            #region Nancy，ViewCopy以及Dic配置
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
            #endregion

            //------------------------------采集卡，每增加一个采集卡要增加以下4行代码
            //这个niAlone每张卡要不一样
            var niAlone = new AIThing();

            //这个除了niAlone不一样其余都一样
            niAlone.basicAI = new NIAI();
            niAlone.DataFileFactory = new HDF5DataFileFactory();

            //这个括号里面的不一样
            MyHub.TryAddThing(niAlone,      //上面的niAlone
                                @"/",       //Thing挂载路径，都一样，不要改！！！
                                "Card0",    //卡名，也就是在网页上看到的卡名称
                                            //下面引号中的要改，前面的是配置文件路径，后面的是采集数据保存到本地的路径
                                new { ConfigFilePath = @"D:\Run\ConfigFile\DAQFamilyBucket\niAlone.txt", DataFileParentDirectory = @"D:\Data\ni\Card0" });

            //注意，这里加了多少个卡，在左边
            //解决方案资源管理器 -> CFET2App -> Views -> ViewSelector.json 中的 childpath 字段中
            //就要加对应多少个卡，且名字和卡名要一样，否则网页上不能显示

            var niMaster = new AIThing();
            niMaster.basicAI = new NIAI();
            niMaster.DataFileFactory = new HDF5DataFileFactory();
            MyHub.TryAddThing(niMaster,
                                @"/",
                                "Card1",
                                new { ConfigFilePath = @"D:\Run\ConfigFile\DAQFamilyBucket\niMaster.txt", DataFileParentDirectory = @"D:\Data\ni\Card1" });

            var niSlave = new AIThing();
            niSlave.basicAI = new NIAI();
            niSlave.DataFileFactory = new HDF5DataFileFactory();
            MyHub.TryAddThing(niSlave,
                                @"/",
                                "Card2",
                                new { ConfigFilePath = @"D:\Run\ConfigFile\DAQFamilyBucket\niSlave.txt", DataFileParentDirectory = @"D:\Data\ni\Card2" });

            //------------------------------采集卡管理，只有一个这个
            var aiManagement = new AIManagementThing();
            MyHub.TryAddThing(aiManagement,
                                @"/",
                                "aimanagement",
                                new
                                {
                                    //有多少个卡就加几个，注意前面是 / 后面是卡名，比如{ "/Card0", "/Card1" },
                                    AllAIThingPaths = new string[] { "/Card0", "/Card1", "/Card2" },
                                    //自动Arm的，如果不想手动触发的就加上，跟上面一行格式一样
                                    AutoArmAIThingPaths = new string[] { "/Card2" }
                                });

            //------------------------------上传文件的，只有一个这个
            var uploader = new DataUpLoadThing();
            //前面的别改，后面的.txt路径是配置文件的完整路径
            MyHub.TryAddThing(uploader, @"/", "uploader", @"D:\Run\ConfigFile\DAQFamilyBucket\DataUploadConfig.txt");

            //说明：
            //一键注释：选中代码并按 Ctrl+K Ctrl+C
            //一键解除注释：选中代码并按 Ctrl+K Ctrl+U
            //退程序不要点右上角的 X ！输入 exit 回车退出
        }
    }
}
