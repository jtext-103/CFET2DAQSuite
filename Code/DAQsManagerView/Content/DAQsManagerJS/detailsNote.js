
var noteDic = {
    "IsOn": "采集卡是否能开始采集，具体来说就是是否能Arm，true 为是 false 为否",
    "SampleRate": "采样率，注意不能超过采集卡最大采样率，否则会报错导致采集卡无法继续使用",
    "Length": "总采样点数<br/><br/>NI-Scope：注意总通道总点数不能超过板卡内存",
    "ChannelName": "通道名<br/><br/>NI-6 series：比如 Dev1/ai0:3 表示有4个通道<br/><br/>JY-6 series：比如 0,1,2,3 表示有4个通道<br/><br/>NI-Scope：比如 0,1,2,3,4,5,6,7 表示有8个通道",
    "ChannelCount": "通道数量，必须和ChannelName中的通道数相同！",
    "TriggerType": "触发方式<br/><br/>NI-6 series：0：立即触发（Ready了马上就变Running）；1：数字触发（作为从卡）；2：模拟触发（外部，作为主卡）<br/><br/>JY-6 series：0：立即触发；1：数字触发<br/><br/>NI-Scope：0：立即触发；1：数字边沿触发；2：边沿触发",
    "TriggerSource": "触发源<br/><br/>NI-6 series：如果TriggerType是0则为空；如果是主卡 /PXI1Slot2/APFI0 或类似；如果是从卡 /PXI1Slot2/ai/StartTrigger 或类似<br/><br/>JY-6 series：3：背板触发；9：外部数字触发<br/><br/>NI-Scope：触发类型为 0 时设置为 0；触发类型为 1 时设置为 \"VAL_PFI_1\"；触发类型为 2 时可设置为 0-7",
    "Delay": "触发后延时多久开始采集，立即触发此项无效",
    "SyncType": "同步方式，0：无主从；1：主卡；2：从卡"
}