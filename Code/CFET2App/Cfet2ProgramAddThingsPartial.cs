using Jtext103.CFET2.CFET2App.ExampleThings;
using Jtext103.CFET2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.CFET2.Things.DAQAIThing;
using Jtext103.CFET2.Things.NiAiLib;
using Jtext103.CFET2.Things.NIScopeDAQAI;
using Jtext103.CFET2.Things.JyAiLib;
using Jtext103.CFET2.Things.DAQDataUploadThing;
using Jtext103.CFET2.Things.MDSUpload;
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
            var nancyCM = new NancyCommunicationModule(new Uri("http://localhost:8001"));
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

            //注意，下面加了多少个卡，在左边：
            //解决方案资源管理器 -> CFET2App -> Views -> ViewSelector.json 中的 childpath 字段中
            //就要加对应多少个卡，且名字和卡名要一样，否则网页上不能显示
            //另外，卡名需要以Card开头，否则页面看不到波形
            //说明在 NI 采集卡中，不同卡是类似的

            #region NI采集卡，若使用NI请展开
            //------------------------------NI采集卡，每增加一个采集卡要增加以下4行代码------------------------------//
            //这个niNonSync每张卡要不一样
            var niNonSync = new AIThing();

            //这个除了niNonSync不一样其余都一样
            niNonSync.basicAI = new NIAI();
            niNonSync.DataFileFactory = new HDF5DataFileFactory();

            //这个括号里面的不一样
            MyHub.TryAddThing(niNonSync,      //上面的niNonSync
                                @"/",       //Thing挂载路径，都一样，不要改！！！
                                "Card0",    //卡名，也就是在网页上看到的卡名称
                                            //下面引号中的要改，前面的是配置文件路径，后面的是采集数据保存到本地的路径
                                new { ConfigFilePath = @"D:\Run\ConfigFile\DAQFamilyBucket\niNonSync.txt", DataFileParentDirectory = @"D:\Data\ni\Card0" });

            //var niMaster = new AIThing();
            //niMaster.basicAI = new NIAI();
            //niMaster.DataFileFactory = new HDF5DataFileFactory();
            //MyHub.TryAddThing(niMaster,
            //                    @"/",
            //                    "Card1",
            //                    new { ConfigFilePath = @"D:\Run\ConfigFile\DAQFamilyBucket\niMaster.txt", DataFileParentDirectory = @"D:\Data\ni\Card1" });

            //var niSlave = new AIThing();
            //niSlave.basicAI = new NIAI();
            //niSlave.DataFileFactory = new HDF5DataFileFactory();
            //MyHub.TryAddThing(niSlave,
            //                    @"/",
            //                    "Card2",
            //                    new { ConfigFilePath = @"D:\Run\ConfigFile\DAQFamilyBucket\niSlave.txt", DataFileParentDirectory = @"D:\Data\ni\Card2" });
            #endregion

            #region JY采集卡，若使用JY请展开
            ////------------------------------JY采集卡，格式和NI一样------------------------------//
            //var jyNonSync = new AIThing();
            //jyNonSync.basicAI = new JYAI();
            //jyNonSync.DataFileFactory = new HDF5DataFileFactory();
            //MyHub.TryAddThing(jyNonSync,      
            //                    @"/",       
            //                    "CardA",           
            //                    new { ConfigFilePath = @"D:\Run\ConfigFile\DAQFamilyBucket\jyNonSync.txt", DataFileParentDirectory = @"D:\Data\jy\CardA" });

            //var jyMaster = new AIThing();
            //jyMaster.basicAI = new JYAI();
            //jyMaster.DataFileFactory = new HDF5DataFileFactory();
            //MyHub.TryAddThing(jyMaster,
            //                    @"/",
            //                    "CardB",
            //                    new { ConfigFilePath = @"D:\Run\ConfigFile\DAQFamilyBucket\jyMaster.txt", DataFileParentDirectory = @"D:\Data\jy\CardB" });

            //var jySlave = new AIThing();
            //jySlave.basicAI = new JYAI();
            //jySlave.DataFileFactory = new HDF5DataFileFactory();
            //MyHub.TryAddThing(jySlave,
            //                    @"/",
            //                    "CardC",
            //                    new { ConfigFilePath = @"D:\Run\ConfigFile\DAQFamilyBucket\jySlave.txt", DataFileParentDirectory = @"D:\Data\jy\CardC" });
            #endregion

            #region NIScope采集卡，若使用NIScope请展开
            //------------------------------NIScope采集卡，每增加一个采集卡要增加以下4行代码------------------------------//
            var scopeNonSync = new AIThing();
            scopeNonSync.basicAI = new NIScopeAI();
            scopeNonSync.DataFileFactory = new HDF5DataFileFactory();
            MyHub.TryAddThing(scopeNonSync,
                                @"/",
                                "Cards0",
                                new { ConfigFilePath = @"D:\Run\ConfigFile\DAQFamilyBucket\scopeNonSync.txt", DataFileParentDirectory = @"D:\Data\ni\Cards0" });

            var scopeSlave = new AIThing();
            scopeSlave.basicAI = new NIScopeAI();
            scopeSlave.DataFileFactory = new HDF5DataFileFactory();
            MyHub.TryAddThing(scopeSlave,
                                @"/",
                                "Cards2",
                                new { ConfigFilePath = @"D:\Run\ConfigFile\DAQFamilyBucket\scopeSlave.txt", DataFileParentDirectory = @"D:\Data\ni\Cards2" });

            var scopeMaster = new AIThing();
            scopeMaster.basicAI = new NIScopeAI();
            scopeMaster.DataFileFactory = new HDF5DataFileFactory();
            MyHub.TryAddThing(scopeMaster,
                                @"/",
                                "Cards1",
                                new { ConfigFilePath = @"D:\Run\ConfigFile\DAQFamilyBucket\scopeMaster.txt", DataFileParentDirectory = @"D:\Data\ni\Cards1" });
            #endregion

            //------------------------------自动 Arm 采集卡与发布上传事件的，只有一个这个------------------------------//
            //它的逻辑是当所有 AllAIThingPaths 中的卡都 Idle 之后自动 Arm 所有 AutoArmAIThingPaths 中的卡，以及发布上传事件
            var aiManagement = new AIManagementThing();
            MyHub.TryAddThing(aiManagement,
                                @"/",
                                "aimanagement",
                                new
                                {
                                    //要判断多少个卡的状态就加几个（比如独立工作的卡就不用加），注意前面是 / 后面是卡名，比如{ "/Card0", "/Card1" },
                                    AllAIThingPaths = new string[] { "/Cards0" },
                                    //AllAIThingPaths = new string[] { "/CardB", "/CardC" },
                                    //自动Arm的，如果不想手动触发的就加上，跟上面一行格式一样
                                    AutoArmAIThingPaths = new string[] { }
                                    //AutoArmAIThingPaths = new string[] { "/CardC" }
                                });

            //------------------------------上传文件的，只有一个这个------------------------------//
            //var uploader = new DataUpLoadThing();
            ////前面的别改，后面的.txt路径是配置文件的完整路径
            //MyHub.TryAddThing(uploader, @"/", "uploader", @"D:\Run\ConfigFile\DAQFamilyBucket\DataUploadConfig.txt");

            //------------------------------上传MDS的，只有一个这个------------------------------//
            //var mdsthing = new MdsThing();
            //如果是 Master 用这行，注释掉下面 Slave
            //MyHub.TryAddThing(mdsthing, @"/", "mds", @"D:\Run\ConfigFile\DAQFamilyBucket\MdsUploadMaster.txt");
            //如果是 Slave 用这行，同时一定要将 AIManagementThing 注释掉！！！
            //MyHub.TryAddThing(mdsthing, @"/", "mds", @"D:\Run\ConfigFile\DAQFamilyBucket\MdsUploadSlave.txt");

            //说明：
            //一键注释：选中代码并按 Ctrl+K Ctrl+C
            //一键解除注释：选中代码并按 Ctrl+K Ctrl+U
            //退程序不要点右上角的 X ！输入 exit 回车退出
        }
    }
}
