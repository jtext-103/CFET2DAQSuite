using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.CFET2.Things.MDSUpload
{
    public class StringOperation
    {   
        /// <summary>
        /// 根据字符串找到分隔符与参数
        /// </summary>
        public static int SetParams5Separator4Param(ref int[] index, ref string[] paramS, ref int[] param, string s)
        {
            //找到所有分隔符位置
            index[0] = s.IndexOf('{');
            if(index[0] < 0)
            {
                return -1;     //没有
            }
            index[1] = s.IndexOf(',');
            index[2] = s.IndexOf(',', index[1] + 1);
            index[3] = s.IndexOf(',', index[2] + 1);
            index[4] = s.IndexOf('}');

            //提取所有参数
            paramS[0] = s.Substring(index[0] + 1, index[1] - 1 - index[0]);
            paramS[1] = s.Substring(index[1] + 1, index[2] - 1 - index[1]);
            paramS[2] = s.Substring(index[2] + 1, index[3] - 1 - index[2]);
            paramS[3] = s.Substring(index[3] + 1, index[4] - 1 - index[3]);
            param[0] = int.Parse(paramS[0]);
            param[1] = int.Parse(paramS[1]);
            param[2] = int.Parse(paramS[2]);
            param[3] = int.Parse(paramS[3]);

            return 0;
        }

        public static string SetStringByParams(ref int[] index, ref int[] param, string s, int time)
        {
            // ts 为要加入的新的字符串
            string ts = s.Substring(0, index[0]);
            string tts = (param[1] + param[2] * time).ToString();
            if (param[0] > tts.Length)
            {
                tts = tts.PadLeft(param[0], '0');
            }
            ts += tts;
            if (index[4] < s.Length)
            {
                ts += s.Substring(index[4] + 1, s.Length - 1 - index[4]);
            }
            return ts;
        }
    }
}
