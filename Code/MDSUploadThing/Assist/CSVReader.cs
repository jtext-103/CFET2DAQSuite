using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Jtext103.CSV;

namespace Jtext103.CFET2.Things.MDSUpload
{
    //这个玩意儿依赖了AIThing
    public class CSVReader
    {
        public static Channel[] SetChannel(string filepath)
        {
            var result = CSVOperator.LoadCSVFile(filepath);

            var channels = new Channel[result.Count - 1];

            //第一行是说明
            for(int i = 1; i < result.Count; i++)
            {
                channels[i - 1] = new Channel();
                channels[i - 1].SourceAIData = result[i][0] + @"/data/" + result[i][1];
                channels[i - 1].SourceAISampleRate = result[i][0] + @"/samplerate";
                channels[i - 1].SourceAILength = result[i][0] + @"/length";
                channels[i - 1].SourceAIStartTime = result[i][0] + @"/starttime";
                channels[i - 1].Tag = result[i][2];
                channels[i - 1].Enable = true;
            }

            return channels;
        }
    }
}
