using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.CFET2.Things.MDSUpload
{
    public class AiRequestUriComposer
    {
        /// <summary>
        /// 根据 channels[x] 信息、本地炮号 localShot 获取采样率
        /// </summary>
        static public string ComposeSampleRateSrcUri(Channel channel, int localShot)
        {
            return channel.SourceAISampleRate + "/" + localShot.ToString();
        }

        /// <summary>
        /// 根据 channel[x]信息、本地炮号 localShot 获取当前通道的数据长度
        /// </summary>
        static public string ComposeLengthSrcUri(Channel channel, int localShot)
        {
            return channel.SourceAILength + "/" + localShot.ToString();
        }

        /// <summary>
        /// 根据 channel[x]信息、本地炮号 localShot 获取采集开始时间（生成时间轴）
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="localShot"></param>
        /// <returns></returns>
        static public string ComposeStartTimeSrcUri(Channel channel, int localShot)
        {
            return channel.SourceAIStartTime + "/" + localShot.ToString();
        }

        /// <summary>
        /// 根据 channels[x] 信息、本地炮号 localShot、上传炮号 mdsPlusShot 和 数据长度 获取数据
        /// </summary>
        static public string ComposeDataSrcUri(Channel channel, int localShot, int length)
        {
            //默认从 0 开始读全部点
            return channel.SourceAIData + "/" + localShot + "/" + 0 + "/" + length.ToString();
        }
    }
}
