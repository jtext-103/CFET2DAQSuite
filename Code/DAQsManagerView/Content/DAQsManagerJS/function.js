//存储对应 Partial 中的 API 路径
var apiStates = new Array();
var apiArms = new Array();
var apiStops = new Array();
var apiTimes = new Array();

var apiShot = new String();
var apiUpload = new String();

function arm(index) {
    var s = "" + index;
    $.ajax({
        url: apiArms[s],
        type: "put",
        async: true,
        success: function () {
            //alert(s + ' is armed!')
        }
    });
}

function stop(index) {
    var s = "" + index;
    $.ajax({
        url: apiStops[s],
        type: "put",
        async: true,
        success: function () {
            //alert(s + ' is stoped!')
        }
    });
}

//是否自动刷新所有DAQ状态
var autoRefreshStates = true;

//设置是否自动刷新
function refreshStatesAuto(s) {
    $(s).prop('checked') ? autoRefreshStates = true : autoRefreshStates = false;
}

setInterval(function () { refreshStates(false) }, 1000);

//手动刷新参数manual为true，自动刷新参数为false
function refreshStates(manual) {
    if (manual == false && autoRefreshStates == false) return;

    //刷新炮号
    $.ajax({
        url: apiShot,
        type: "get",
        contentType: 'application/json',
        async: false,
        success: function (data) {
            var obj = JSON.parse(data);
            if (obj.ObjectVal == -1) {
                document.getElementById("shotId").innerHTML = "No Shot";
            }
            else {
                document.getElementById("shotId").innerHTML = obj.ObjectVal;
            }   
            document.getElementById("shotId").style.color = '#000000';
        },
        error: function() {
            document.getElementById("shotId").innerHTML = "Offline";
            document.getElementById("shotId").style.color = '#999999';
        }
    });

    //刷新上传状态
    $.ajax({
        url: apiUpload,
        type: "get",
        contentType: 'application/json',
        async: false,
        success: function (data) {
            var obj = JSON.parse(data);
            if (obj.ObjectVal == 0) {
                document.getElementById("uploadId").innerHTML = "Idle";
                document.getElementById("uploadId").style.color = '#FFA500';
            }
            else if (obj.ObjectVal == 2) {
                document.getElementById("uploadId").innerHTML = "Uploading";
                document.getElementById("uploadId").style.color = '#32CD32';
            }
            else {
                document.getElementById("uploadId").innerHTML = "Unknow";
                document.getElementById("uploadId").style.color = '#999999';
            }
            
        },
        error: function () {
            document.getElementById("uploadId").innerHTML = "Offline";
            document.getElementById("uploadId").style.color = '#999999';
        }
    });

    //刷新 PartialView，这里 api 是索引
    for (var api in apiStates) {
        $.ajax({
            url: apiStates[api],
            type: "get",
            contentType: 'application/json',
            async: false,
            success: function (data) {
                var obj = JSON.parse(data);
                var val;
                if (obj.ObjectVal == 0) {
                    val = "Idle";
                    document.getElementById(api + " stateId").style.color = '#FFA500';
                    document.getElementById(api + " timeId").style.color = '#FFA500';
                }
                else if (obj.ObjectVal == 1) {
                    val = "Ready";
                    document.getElementById(api + " stateId").style.color = '#FF6347';
                    document.getElementById(api + " timeId").style.color = '#FF6347';
                }
                else if (obj.ObjectVal == 2) {
                    val = "Running";
                    document.getElementById(api + " stateId").style.color = '#32CD32';
                    document.getElementById(api + " timeId").style.color = '#32CD32';
                }
                else if (obj.ObjectVal == 255) {
                    val = "Error";
                    document.getElementById(api + " stateId").style.color = '#FF0000';
                    document.getElementById(api + " timeId").style.color = '#FF0000';
                }
                else {
                    val = "Unknow";
                    document.getElementById(api + " stateId").style.color = '#999999';
                    document.getElementById(api + " timeId").style.color = '#999999';
                }
                var text = document.getElementById(api + " stateId").innerHTML = val;

                //由于apiStates和times的索引一一对应一模一样，所以可以这么玩
                $.ajax({
                    url: apiTimes[api],
                    type: "get",
                    async: false,
                    success: function (data) {
                        var obj = JSON.parse(data);
                        document.getElementById(api + " timeId").innerHTML = obj.ObjectVal + "s";
                    }
                });
            }
        });
    }
}


