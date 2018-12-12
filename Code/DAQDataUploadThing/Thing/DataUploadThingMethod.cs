using System;
using System.Collections.Generic;
using System.IO;
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
        /// 上传所有路径下的数据，如果任务不能开始返回-1，成功开始返回0
        /// </summary>
        /// <param name="localPath">源</param>
        /// <param name="serverPath">目标</param>
        /// <returns></returns>
        [Cfet2Method]
        public int UploadAll()
        {
            lock (myStateLock)
            {
                if (UploadState != Status.Idle)
                {
                    return -1;
                }
                UploadState = Status.Running;

                Task.Run(() => Upload(myConfig.LocalDataDirectories, myConfig.ServerDataDirectories, myConfig.AIThings));
                return 0;
            }
        }

        // 最基础的上传的方法，将本地路径中的所有文件复制到服务器路径下
        private void Upload(string[] localDirectories, string[] serverDirectories, string[] aIThings)
        {
            for (int i = 0; i < localDirectories.Length; i++)
            {
                //设置本地上传文件完整路径
                string[] realLocalPaths = SetLocalFilePath(localDirectories[i], aIThings[i]);
                string realServerDirectory = SetServerFileDirectory(serverDirectories[i]);

                switch (myConfig.UploadBehavior)
                {
                    case Behavior.KeepOriginal:
                        {

                        }
                        try
                        {
                            foreach(var p in realLocalPaths)
                            {
                                File.Copy(p, realServerDirectory + GetServerFilename());
                            }
                            
                        }
                        catch (Exception e)
                        {
                            //todo:写入Log永久保存
                            Console.WriteLine("不覆盖拷贝错误! Error Message : {0}", e);
                        }
                        break;
                    case Behavior.RenameOriginal:
                        try
                        {
                            foreach (var p in realLocalPaths)
                            {
                                FileOperator.RenameExistedFile(GetServerFilename(), realServerDirectory);
                                File.Copy(p, realServerDirectory + GetServerFilename());
                            } 
                        }
                        catch (Exception e)
                        {
                            //todo:写入Log永久保存
                            Console.WriteLine("重命名原文件拷贝错误! Error Message : {0}", e);
                        }
                        break;
                    case Behavior.Overwrite:
                        try
                        {
                            foreach (var p in realLocalPaths)
                            {
                                if (File.Exists(realServerDirectory + GetServerFilename()))
                                {
                                    //todo:写入Log永久保存
                                    Console.WriteLine("警告！正覆盖原有数据文件！");
                                }
                                File.Copy(p, realServerDirectory + GetServerFilename(), true);
                            }                     
                        }
                        catch (Exception e)
                        {
                            //todo:写入Log永久保存
                            Console.WriteLine("覆盖拷贝错误! Error Message : {0}", e);
                        }
                        break;
                    default:
                        throw new Exception("UploadBehavior设置错误！");
                }
            }
            //上传完毕
            lock (myStateLock)
            {
                UploadState = Status.Idle;
            }
        }

        //用配置文件中的Local路径得到真正的，也就是最大炮号文件夹的路径
        private string[] SetLocalFilePath(string originDirectory, string aIThings)
        {
            object[] param = null;  
            return (string[])MyHub.TryGetResourceSampleWithUri(aIThings + "/fulldatafilepaths", param).ObjectVal;
        }

        //通过本地采集电脑上配置的ServerDirectories和ShotServer获取最后要上传到Server上的文件夹路径，不带文件名
        //如果Server上的文件夹路径不存在，则在这里自动创建
        private string SetServerFileDirectory(string serverParentDirectory)
        {
            //todo:从ECEIServer上的ShotServerThing获取当前炮号
            int shotNo = 233333333;

            string nowDirectory = serverParentDirectory;
            string newDir;

            //每100分一级文件夹，最多分两级
            int firstLevel = shotNo / 10000;
            if (firstLevel >= 1)
            {
                newDir = nowDirectory + firstLevel + "xxxx";
                if (!Directory.Exists(newDir))
                {
                    Directory.CreateDirectory(newDir);
                }
                nowDirectory = newDir + @"\";
            }

            int secondLevel = shotNo / 100;
            if (secondLevel >= 1)
            {
                newDir = nowDirectory + secondLevel + "xx";
            }
            else
            {
                newDir = nowDirectory + "xx";
            }
            if (!Directory.Exists(newDir))
            {
                Directory.CreateDirectory(newDir);
            }
            nowDirectory = newDir + @"\";
            return nowDirectory;
        }

        //通过ShotServerThing获得最后要存到Server上的文件名，不带路径
        private string GetServerFilename()
        {
            //todo:从ECEIServer上的ShotServerThing获取当前炮号
            int shotNo = 233333333;
            return shotNo.ToString() + ".hdf5";
        }
    }
}
