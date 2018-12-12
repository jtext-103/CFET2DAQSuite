namespace Jtext103.CFET2.Things.DAQDataUploadThing
{
    /// <summary>
    /// 任务状态
    /// </summary>
    public enum Status
    {
        //空闲
        Idle = 0,
        //正在上传
        Running = 1,

        Error = 255
    }
}
