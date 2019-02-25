using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.CFET2.Core;
using Jtext103.CFET2.Core.Attributes;

namespace Jtext103.CFET2.Things.EPCISClient
{
    /// <summary>
    /// 用来设置和读取ECPIS PV的Thing
    /// </summary>
    public class EPCISThing : Thing
    {
        private EPCISOperator epciser;

        public EPCISThing() { }

        public override void TryInit(object initObj)
        {
            epciser = new EPCISOperator((string)initObj);
        }

        public override void Start() { }

        /// <summary>
        /// 获取已经存在的PV值
        /// </summary>
        /// <param name="pvName"></param>
        /// <returns></returns>
        [Cfet2Status]
        public object TryGetPV(string pvName)
        {
            return epciser.TryGetPV(pvName);
        }

        /// <summary>
        /// 对已经存在的PV设置值
        /// </summary>
        /// <param name="pvName"></param>
        /// <param name="value"></param>
        [Cfet2Method]
        public void TrySetPV(string pvName, int value)
        {
            epciser.TrySetPV(pvName, value);
        }

        [Cfet2Status]
        public string[] PreinstallPvNames()
        {
            return epciser.Config.PvNames;
        }

        [Cfet2Status]
        public string ShowPreinstallPvNames()
        {
            string result = null;
            if (epciser.Config.PvNames.Length >= 1)
            {
                foreach (var p in epciser.Config.PvNames)
                {
                    result += p + "\n";
                }
                result = result.Substring(0, result.Length - 1);
            }
            return result;
        }
    }
}
