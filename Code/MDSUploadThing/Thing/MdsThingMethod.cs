using Jtext103.CFET2.Core;
using Jtext103.CFET2.Core.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using MDSplusVBC;
using System.Threading;
using System.Diagnostics;

namespace Jtext103.CFET2.Things.MDSUpload
{
    public partial class MdsThing : Thing
    {
        //存储从 cfet 获取的采样率
        private double sampleRate;

        //存储从 cfet 获取的每通道数据长度
        private int length;

        //存储从 cfet 获取的开始时间
        private double startTime;

        //存储从 cfet 获取的数据
        private double[] data;

        //锁，控制只能有一个人调用上传方法
        object myStateLock = new object();

        /// <summary>
        /// upload 1/all channels in 1 shot
        /// </summary>
        private void Upload(int localShot, int mdsShot, string localChannelDataSourceUri)
        {
            MDSplus mds = new MDSplus();
            mds.Connect(myConfig.ServerConfig.Host);
            mds.MdsOpen(myConfig.ServerConfig.Tree, mdsShot);

            int status = 0;

            //如果没有传入 channelNo 的获取路径
            if (localChannelDataSourceUri == null)
            {
                foreach (var s in myConfig.RealChannelsDic)
                {
                    //利用短路，可以同时判断 null 和 false
                    if (s.Value.Enable == false)
                    {
                        continue;
                    }
                    status = 0;

                    //获取采样率
                    sampleRate = Convert.ToDouble(MyHub.TryGetResourceSampleWithUri(AiRequestUriComposer.ComposeSampleRateSrcUri(s.Value, localShot)).ObjectVal);
                    //获取长度
                    length = Convert.ToInt32(MyHub.TryGetResourceSampleWithUri(AiRequestUriComposer.ComposeLengthSrcUri(s.Value, localShot)).ObjectVal);
                    //获取开始时间
                    startTime = Convert.ToDouble(MyHub.TryGetResourceSampleWithUri(AiRequestUriComposer.ComposeStartTimeSrcUri(s.Value, localShot)).ObjectVal);
                    //获取数据
                    data = (double[])MyHub.TryGetResourceSampleWithUri(AiRequestUriComposer.ComposeDataSrcUri(s.Value, localShot, length)).ObjectVal;

                    //利用数据的 double数组、BUILD_SIGNAL(开始时间，总时间，1/采样率) 上传一个 Mds 的 signal 类型数据
                    mds.MdsPut("\\" + s.Value.Tag, "BUILD_SIGNAL($1,*,MAKE_DIM(*,$2 : $3 : $4))",
                                data, startTime, length * 1.0 / sampleRate + startTime, 1.0 / sampleRate,
                                ref status);
                }
            }

            //必定手动上传
            else
            {
                try
                {
                    if (myConfig.RealChannelsDic[localChannelDataSourceUri] != null && myConfig.RealChannelsDic[localChannelDataSourceUri].Enable == true)
                    {
                        string uri = AiRequestUriComposer.ComposeSampleRateSrcUri(myConfig.RealChannelsDic[localChannelDataSourceUri], localShot);
                        sampleRate = Convert.ToDouble(MyHub.TryGetResourceSampleWithUri(uri).ObjectVal);
                        length = Convert.ToInt32(MyHub.TryGetResourceSampleWithUri(AiRequestUriComposer.ComposeLengthSrcUri(myConfig.RealChannelsDic[localChannelDataSourceUri], localShot)).ObjectVal);
                        startTime = Convert.ToDouble(MyHub.TryGetResourceSampleWithUri(AiRequestUriComposer.ComposeStartTimeSrcUri(myConfig.RealChannelsDic[localChannelDataSourceUri], localShot)).ObjectVal);
                        data = (double[])MyHub.TryGetResourceSampleWithUri(AiRequestUriComposer.ComposeDataSrcUri(myConfig.RealChannelsDic[localChannelDataSourceUri], localShot, length)).ObjectVal;

                        mds.MdsPut("\\" + myConfig.RealChannelsDic[localChannelDataSourceUri].Tag, "BUILD_SIGNAL($1,*,MAKE_DIM(*,$2 : $3 : $4))",
                                    data, startTime, length * 1.0 / sampleRate + startTime, 1.0 / sampleRate,
                                    ref status);
                    }
                }
                catch
                {
                    // unlock after finished
                    lock (myStateLock)
                    {
                        State = Status.Idle;
                    }
                    logger.Error("手动上传错误！数据源:" + localChannelDataSourceUri);
                    throw new Exception("手动上传错误！请检查相关参数设置！");
                }
            }
            
            System.Diagnostics.Debug.WriteLine("Upload finished!", DateTime.Now.ToLocalTime().ToString("HH:mm:ss.fff"));
            logger.Info("上传成功！");
            mds.DisConnect();

            // unlock after finished
            lock (myStateLock)
            {
                State = Status.Idle;
            }

            if(myConfig.MasterOrSlave == 2)
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        /// <summary>
        /// 上传单个卡的单个通道，拆分卡和通道方便传输
        /// 最好不要用这个方法！！！因为手动调用不会使用 Slave 上传，上传后 MDS 相关资源（内存）不会释放！！！
        /// </summary>
        [Cfet2Method]
        public int StartUpload(int localShot, int mdsShot, string localChannelDataSourceUri)
        {
            // lock before start
            lock (myStateLock)
            {
                if (State != Status.Idle)
                {
                    return -1;
                }
                State = Status.Uploading;
            }
            Task.Run(() => Upload(localShot, mdsShot, localChannelDataSourceUri));
            return 0;
        }

        /// <summary>
        /// 上传所有配置文件中 Enable 的卡和通道
        /// 最好不要用这个方法！！！因为手动调用不会使用 Slave 上传，上传后 MDS 相关资源（内存）不会释放！！！
        /// </summary>
        [Cfet2Method]
        public int StartUploadAll(int localShot, int mdsShot)
        {
            // lock before start
            lock (myStateLock)
            {
                if (State != Status.Idle)
                {
                    return -1;
                }
                State = Status.Uploading;
            }
            Task.Run(() => Upload(localShot, mdsShot, null));
            return 0;
        }

        // for test
        //static int testShotNo = 1054410;

        /// <summary>
        /// 用 0 和 0 参数上传最近一炮的全部卡全部通道数据
        /// </summary>
        [Cfet2Method]
        public int StartUploadAllAuto()
        {
            return StartUploadAll(0, 0);
        }
    }
}
