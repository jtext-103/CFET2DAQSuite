using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.CFET2.Things.MDSUpload
{
    class MdsConfigRebuild
    {
        /// <summary>
        /// 用可能带参数{,,}的 Channels 生成完整的不带参数的 Channels
        /// </summary>
        public static Channel[] ChannelRebuild(Channel[] s)
        {
            //数据源的参数
            int[] sourceIndex = new int[5];
            string[] sourceParamS = new string[4];
            int[] sourceParam = new int[4];

            // mds Tag 的参数
            int[] tagIndex = new int[5];
            string[] tagParamS = new string[4];
            int[] tagParam = new int[4];

            //找一遍看有没有带{,,,}的，有一个就变一个
            int count = s.Length;
            for (int i = 0; i < count; i++)
            {
                if (StringOperation.SetParams5Separator4Param(ref sourceIndex, ref sourceParamS, ref sourceParam, s[i].SourceAIData) == -1)
                {
                    continue;
                }
                StringOperation.SetParams5Separator4Param(ref tagIndex, ref tagParamS, ref tagParam, s[i].Tag);

                //重复次数 param[3] 以 source 为准
                tagParamS[3] = sourceParamS[3];
                tagParam[3] = sourceParam[3];

                //保存这个 Channel，并将原数组中的这个 Channel 删除
                Channel cWithParam = new Channel(s[i]);
                for (int j = i; j < count - 1; j++)
                {
                    s[j] = s[j + 1];
                }

                //新建一个更长的 Channel，并将 s 给到前面
                Channel[] newC = new Channel[count + sourceParam[3] - 1];
                for (int j = 0; j < count - 1; j++)
                {
                    newC[j] = s[j];
                }

                //将新生成的 Channel[] 加到 newC 结尾
                string ts;
                for (int j = 0; j < sourceParam[3]; j++)
                {
                    //将 ts 加入结尾
                    //这里曾经出现了 Bug
                    // newC[count - 1 + j] = cWithParam
                    //如果按上述这样写，因为是引用类型，所以 cWithParam 在后面过程中会改变
                    newC[count - 1 + j] = new Channel(cWithParam);

                    //先构建 SourceAiData
                    ts = StringOperation.SetStringByParams(ref sourceIndex, ref sourceParam, cWithParam.SourceAIData, j);

                    //这里曾经出现了 Bug，见上面
                    newC[count - 1 + j].SourceAIData = ts;

                    //再构建 Tag
                    ts = StringOperation.SetStringByParams(ref tagIndex, ref tagParam, cWithParam.Tag, j);

                    newC[count - 1 + j].Tag = ts;
                }

                //更新 Channel[] s
                s = newC;
                count += sourceParam[3] - 1;
                i--;
            }

            return s;
        }
    }
}
