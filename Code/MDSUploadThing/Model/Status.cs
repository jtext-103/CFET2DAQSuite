using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.CFET2.Things.MDSUpload
{
    /// <summary>
    /// Upload 任务状态
    /// </summary>
    public enum Status
    {
        Idle = 0,
        Ready = 1,
        Running = 2,
        Uploading = 3,
        Error = 255
    }
}
