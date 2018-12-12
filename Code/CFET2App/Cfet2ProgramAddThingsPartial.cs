using Jtext103.CFET2.CFET2App.ExampleThings;
using Jtext103.CFET2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.CFET2.CFET2App
{
    partial  class Cfet2Program : CFET2Host
    {
        private void AddThings()
        {
            PcMonitorThing pc = new PcMonitorThing();
            MyHub.TryAddThing(pc, "/", "pc");
        }
    }
}
