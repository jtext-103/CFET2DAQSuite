﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.ModularInstruments;
using NationalInstruments.ModularInstruments.NIScope;

namespace Jtext103.CFET2.Things.NIScopeDAQAI
{
    //在每个 NIScopeSlave 初始化的时候，将其 scopeSession 加入进来
    public static class TClockDevice
    {
        public static List<NIScope> SyncDevices;

        public static object Lock = new object();

        public static bool IsSlaveCanAddIntoTDevice = false;

        public static bool IsMasterReady = false;

        public static Dictionary<string, bool> SlaveOver = new Dictionary<string, bool>();
    }
}