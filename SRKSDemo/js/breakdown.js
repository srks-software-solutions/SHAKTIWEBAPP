

function breakdownModal1(Machineid) {
    $.ajax({
        type: 'GET',
        data: { Machineid },
        url: '/OperatorEntryModel/BreakdDownReasons',
        success: function (data) {
            var Result = JSON.parse(data);
            var RLength = Result.length;
            if (RLength > 1) {
                $("#BreakDown").html('');
                $("#BreakDown").append('<div class="modal-content" style="width: 70%; position: absolute; left:16%;"><div class="modal-header" style ="background-color: navy; color: white;" >' +
                    '<h5 class="modal-title">Breakdown Reason</h5><button type="button" class="close" data-dismiss="modal" aria-label="Close">' +
                    '<span aria-hidden="true" style="color: white;">×</span></button></div><div class="modal-body"><div class="form-group" style="text-align: center;"><div id="reason1">' +
                    '<p>Reason 1<select style="width: 206px;height: 34px;border-radius: 5px;" class="brkdwnreason1" id="Reject1_' + Machineid + '"></select></div><div id="reason2"><p>Reason 2<select style="width: 206px;height: 34px;border-radius: 5px;" class="brkdwnreason2" id="Reject2_' + Machineid + '">' +
                    '</select></div><div id="reason3"><p>Reason 3<select style="width: 206px;height: 34px;border-radius: 5px;" class="brkdwnreason3" id="Reject3_' + Machineid + '"></select>' +
                    '</div></div><div style="text-align: center; margin-top: 34px;"><button type="button" class="btn btn-success" style="width:22%" data-dismiss="modal" onclick="openLogin1(' + Machineid + ')">Start</button>' +
                    '</div></div></div>');


                $("#Reject1_" + Machineid).html('');
                $("#Reject1_" + Machineid).append(("<option value=0>--Select Breakdown Reason--</option>"))
                for (var i = 0; i < Result.length; i++) {
                    $("#Reject1_" + Machineid).append(("<option value='" + Result[i].BreakdownID + "'>" + Result[i].BreakdownCode + "</option>"))
                }

            }
            else {
                if (Result == "MaintLogin") {
                    $("#Maintances").html('');
                    var css = '';
                    css += '<div class="modal-content" style="width: 70%; position: absolute; left:16%;"><div class="modal-header" style="    background-color: navy; color: white;">';
                    css += '<h5 class="modal-title">Maintenance Login</h5> <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true" style="color: white;">×</span></button></div>';
                    css += '<div class="modal-body"><form class="form-horizontal"><div class="form-group"><div class="form-group"><label class="control-label col-sm-4" for="email">User Name:</label><div class="col-sm-6">';
                    css += '<input type="text" class="form-control"  placeholder="Enter user name" id="UserName_' + Machineid + '"></div></div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Password:</label>';
                    css += '<div class="col-sm-6"><input type="password" class="form-control"  placeholder="Enter password" id="Password_' + Machineid + '"></div></div></form><div style="text-align: center; margin-top: 34px;">';
                    css += '<button type="button" class="btn btn-success" class="btnloginmnt mnt1_' + Machineid + '" style="width:22%" data-toggle="modal" data-target="#machineAcceptanceBtn" onclick="getmaintainancelogin1(' + Machineid + ')">Login</button></div></div>';
                    $(css).appendTo($("#Maintances"));

                }
                else if (Result == "MaintClosureLogin") {
                    $("#Maintances").html('');
                    var css = '';
                    css += '<div class="modal-content" style="width: 70%; position: absolute; left:16%;"><div class="modal-header" style="    background-color: navy; color: white;">';
                    css += '<h5 class="modal-title">Maintenance Login</h5> <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true" style="color: white;">×</span></button></div>';
                    css += '<div class="modal-body"><form class="form-horizontal"><div class="form-group"><div class="form-group"><label class="control-label col-sm-4" for="email">User Name:</label><div class="col-sm-6">';
                    css += '<input type="text" class="form-control"  placeholder="Enter user name" id="UserName_' + Machineid + '"></div></div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Password:</label>';
                    css += '<div class="col-sm-6"><input type="password" class="form-control"  placeholder="Enter password" id="Password_' + Machineid + '"></div></div></form><div style="text-align: center; margin-top: 34px;">';
                    css += '<button type="button" class="btn btn-success" class="btnloginmnt mnt1_' + Machineid + '" style="width:22%" data-toggle="modal" data-target="#maintenaceClousreBtn" onclick="getmaintainancelogin2(' + Machineid + ')">Login</button></div></div>';
                    $(css).appendTo($("#Maintances"));


                }
                else if (Result == "ProdLogin") {
                    $("#ProdLogin").html('');
                    var css = '';
                    css += '<div class="modal-content" style="width: 70%; position: absolute; left:16%;"><div class="modal-header" style = "background-color: navy; color: white;" >' +
                        '<h5 class="modal-title">Production Login</h5><button type="button" class="close" data-dismiss="modal" aria-label="Close">' +
                        '<span aria-hidden="true" style="color: white;">×</span></button></div ><div class="modal-body"><form class="form-horizontal"><div class="form-group">' +
                        '<label class="control-label col-sm-4" for="email">User Name:</label><div class="col-sm-6"><input type="text" class="form-control" id="Pusername_' + Machineid + '" placeholder="Enter user name">' +
                        '</div></div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Password:</label><div class="col-sm-6"><input type="password" class="form-control" id="Ppwd_' + Machineid + '" placeholder="Enter password">' +
                        '</div></div></form><div style="text-align: center; margin-top: 34px;"><button type="button" class="btn btn-success" style="width:22%" data-toggle="modal" data-target="#productionDetailsBtn" onclick="ProdLogin1(' + Machineid + ')">Login</button>' +
                        '</div></div></div>';

                    $(css).appendTo($("#ProdLogin"));

                }
            }
        }
    });
}


function ProdLogin1(id) {
    $("#productionloginBtn").hide();
    var UserName = $("#Pusername_" + id).val();
    var Password = $("#Ppwd_" + id).val();
    var Machineid = id;
    $.ajax({
        type: 'POST',
        data: { UserName, Password, Machineid },
        url: '/OperatorEntryModel/LoginCheckProd1',
        success: function (data) {
            var Result = JSON.parse(data);
            if (Result != "Fail") {
                var css = '';
                css += '<div class="modal-content" style="width: 70%; position: absolute; left:16%;"><div class="modal-header" style="background-color: navy; color: white;">' +
                    '<h5 class="modal-title"> Production Details </h5><button type="button" class="close" data-dismiss="modal" aria-label="Close">' +
                    '<span aria-hidden="true" style="color: white;">×</span></button></div><div class="modal-body"><form class="form-horizontal"><div class="form-group">' +
                    '<label class="control-label col-sm-4" for="email">Machine Name:</label><div class="col-sm-6"><input type="text" class="form-control" value="' + Result.MachineName+'" disabled>' +
                    '</div></div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Reason</label><div class="col-sm-6"><input type="text" class="form-control" value="' + Result.Reason+'" readonly>' +
                    '</div></div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Date & Time</label><div class="col-sm-6"><input type="text" class="form-control" value="' + Result.DateTimeDis +'" readonly>' +
                    '</div></div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Acceptence Time</label><div class="col-sm-6"><input type="text" class="form-control" value="' + Result.AcceptTime +'" readonly>' +
                    '</div></div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Closure Time</label><div class="col-sm-6"><input type="text" class="form-control" value="' + Result.finish +'" readonly>' +
                    '</div></div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Operator Name</label><div class="col-sm-6"><input type="text" class="form-control" value="' + Result.Operatorname +'" readonly>' +
                    '</div></div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Maintenance Operator Name:</label><div class="col-sm-6"><input type="text" class="form-control" value="' + Result.MaintName +'" readonly>' +
                    '</div></div><div class="form-group"><label class="control-label col-sm-4" for="email">Maintenance Remarks</label><div class="col-sm-6">' +
                    '<textarea readonly type="text" class="form-control" row="2">'+Result.result+'</textarea></div></div><div class="form-group"><label class="control-label col-sm-4" for="email">Production Remarks</label>' +
                    '<div class="col-sm-6"><textarea type="text" id="ProdText_' + Machineid+'" class="form-control" row="2" placeholder="Enter the remarks here"></textarea></div></div></form><div style="text-align: center; margin-top: 34px;">' +
                    '<button type="button" class="btn btn-success" style="width:22%" data-dismiss="modal" onclick="ProdAccept1(' + Machineid + ')">Accept</button><button type="button" class="btn btn-success" style="width:22%" data-toggle="modal" data-target="#RejectReasonBtn" onclick="ProdReject1(' + Machineid + ')">Reject</button>' + 
                    '</div></div></div>';
               

                $(css).appendTo($("#ProdDet"));
            }

        }
    });
}

function ProdAccept1(id) {
    var RemartsData = $("#ProdText_" + id).val();
    $.ajax({
        type: 'POST',
        data: { id, RemartsData },
        url: '/OperatorEntryModel/UpdateRemarksProd',
        success: function (data) {
            if (data != 'Fail') {
                window.location.reload(true);
               // BreakRework1(id);
                //breakdownModal(id);
            }
        }
    });
}

function ProdReject1(Mid) {
    $("#productionDetailsBtn").hide();
    var machineid = Mid;
    $("#MaintReject").append('<div class="modal-content" style="width: 70%; position: absolute; left:16%;"><div class="modal-header" style="    background-color: navy; color: white;">' +
        '<h5 class="modal-title">Rejected Reason</h5><button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true" style="color: white;">×</span>' +
        '</button></div><div class="modal-body"><form class="form-horizontal"><div class="form-group" style="text-align: center;"><div id="reason3"><p>Enter the reason<select style="width: 206px;height: 34px;border-radius: 5px;" id="PReject_' + machineid + '">' +
        '</select></div></div></form><div style="text-align: center; margin-top: 34px;"><button type="button" class="btn btn-success" style="width:22%" data-dismiss="modal" onclick="ProdRejectsave1(' + machineid + ');">Save</button></div>' +
        '</div> </div>');

    $.ajax({
        url: '/OperatorEntryModel/ProdReject',
        type: 'GET',
        async: false,
        data: { machineid },
        success: function (data) {
            var Result = JSON.parse(data);
            $("#PReject_" + machineid).html('');
            for (var i = 0; i < Result.length; i++) {
                $("#PReject_" + machineid).append("<option value='" + Result[i].RID + "'>" + Result[i].RejectName + "</option>");
            }
        }
    });
    $('.brkdwn_' + machineid + '').removeAttr('data-target', '#machineAcceptanceBtn');
    $('.brkdwn_' + machineid + '').attr('data-target', '#RejectReasonBtn');
}


function ProdRejectsave1(machineid) {
    var Rid = $("#PReject_" + machineid).val();
    $.ajax({
        url: '/OperatorEntryModel/RejectProduction',
        type: 'GET',
        async: false,
        data: { machineid, Rid },
        success: function (data) {
            if (data == "true") {
                $('.brkdwn_' + machineid + '').removeAttr('data-target', '#RejectReasonBtn');
                $('.brkdwn_' + machineid + '').attr('data-target', '#loginBtn');
                window.location.reload(true);
                //BreakRework1(machineid);

            }
        }
    });
}



function getmaintainancelogin2(id) {
    $('#loginBtn').hide();
    var UserName = $("#UserName_" + id).val();
    var Password = $("#Password_" + id).val();
    var Machineid = id;
    $.ajax({
        type: 'POST',
        data: { UserName, Password, Machineid },
        url: '/OperatorEntryModel/LoginCheckMaint2',
        success: function (data) {
            if (data != 'Fail') {
                // window.location.reload(true);                
                var result = JSON.parse(data);
                $("#MaintClosure").html('');
                $("#MaintClosure").append('<div class="modal-content" style="width: 70%; position: absolute; left:16%;"><div class= "modal-header" style ="background-color: navy; color: white;" >' +
                    '<h5 class="modal-title"> Maintenance Closure </h5><button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true" style="color: white;">×</span>' +
                    '</button></div ><div class="modal-body"><form class="form-horizontal"><div class="form-group"><label class="control-label col-sm-4" for="email">Machine Name:</label>' +
                    '<div class="col-sm-6"><input type="text" class="form-control" disabled value=' + result.MachineName + '></div></div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Reason</label>' +
                    '<div class="col-sm-6"><input type="text" class="form-control" readonly value=' + result.Reason + '></div></div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Date & Time</label>' +
                    '<div class="col-sm-6"><input type="text" class="form-control" readonly value=' + result.DateTimeDis + '></div></div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Acceptence Time</label>' +
                    '<div class="col-sm-6"><input type="text" class="form-control" readonly value=' + result.AcceptTime + '></div> </div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Operator Name</label>' +
                    '<div class="col-sm-6"><input type="text" class="form-control" readonly value=' + result.Operatorname + '></div></div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Maintenance Operator Name:</label>' +
                    '<div class="col-sm-6"><input type="text" class="form-control" readonly value=' + result.MaintName + '></div></div><div class="form-group"><label class="control-label col-sm-4" for="email">Remarks</label>' +
                    '<div class="col-sm-6"><textarea type="text" class="form-control" row="2" placeholder="Enter the remarks here" id="MaintText_' + Machineid + '"></textarea></div></div></form><div style="text-align: center; margin-top: 34px;">' +
                    '<button type="button" class="btn btn-success" style="width:22%" data-dismiss="modal" onclick="MaintFinish1(' + Machineid + ')">Finish</button> </div></div></div>');
            }
            else {
                alert("Relogin");
            }

        }

    });
}

function MaintFinish1(id) {
    var RemartsData = $("#MaintText_" + id).val();
    $.ajax({
        type: 'POST',
        data: { id, RemartsData },
        url: '/OperatorEntryModel/UpdateRemarks1',
        success: function (data) {
            if (data != 'Fail') {
                //breakdownModal(id);
                window.location.reload(true);
               // BreakRework1(id);
                $('.brkdwn1_' + machineid + '').removeAttr('data-target', '#maintenaceClousreBtn');
                $('.brkdwn1_' + machineid + '').attr('data-target', '#loginBtn');
               
            }
        }
    });
}


function getmaintainancelogin1(id) {
    $('#loginBtn').hide();
    var UserName = $("#UserName_" + id).val();
    var Password = $("#Password_" + id).val();
    var Machineid = id;
    $.ajax({
        type: 'POST',
        data: { UserName, Password, Machineid },
        url: '/OperatorEntryModel/LoginCheckMaint1',
        success: function (data) {
            if (data != 'Fail') {
                // window.location.reload(true);                
                var Result = JSON.parse(data);
                $("#MachineAcceptance").html('');
                $("#MachineAcceptance").append('<div class="modal-content" style="width: 70%; position: absolute; left:16%;"><div class= "modal-header" style = "background-color: navy; color: white;" >' +
                    '<h5 class="modal-title">Maintenance Acceptance</h5><button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true" style="color: white;">×</span>' +
                    '</button></div ><div class="modal-body"><form class="form-horizontal"><div class="form-group"><label class="control-label col-sm-4" for="email">Machine Name:</label>' +
                    '<div class="col-sm-6"><input type="text" class="form-control" disabled id="MachineName_' + id + '" value="' + Result.MachineName + '"></div></div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Reason</label>' +
                    '<div class="col-sm-6"><input type="text" class="form-control" readonly id="Reason_' + id + '" value="' + Result.Reason + '"></div></div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Date & Time</label>' +
                    '<div class="col-sm-6"><input type="text" class="form-control" readonly id="datetime_' + id + '" value="' + Result.DateTimeDis + '"></div></div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Operator Name</label>' +
                    '<div class="col-sm-6"><input type="text" class="form-control" readonly id="OperatName_' + id + '" value="' + Result.Operatorname + '"></div></div><div class="form-group"><label class="control-label col-sm-4" for="pwd">Maintenance Operator Name:</label>' +
                    '<div class="col-sm-6"><input type="text" class="form-control" readonly id="MAint_' + id + '" value="' + Result.MaintName + '"></div></div></form><div style="text-align: center; margin-top: 34px;">' +
                    '<button type="button" class="btn btn-success" style="width:22%"  onclick="MaintAccept1(' + id + ')">Accept</button><button type="button" class="btn btn-success" style="width:22%" data-toggle="modal" data-target="#RejectReasonBtn" onclick="MaintReject1(' + id + ')">Reject</button>' +
                    '</div></div></div>');
            }
            else {
                alert("Relogin");
            }

        }

    });

}


function MaintReject1(Mid) {
    $("#machineAcceptanceBtn").hide();
    var machineid = Mid;
    $("#MaintReject").append('<div class="modal-content" style="width: 70%; position: absolute; left:16%;"><div class="modal-header" style="    background-color: navy; color: white;">' +
        '<h5 class="modal-title">Rejected Reason</h5><button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true" style="color: white;">×</span>' +
        '</button></div><div class="modal-body"><form class="form-horizontal"><div class="form-group" style="text-align: center;"><div id="reason3"><p>Enter the reason<select style="width: 206px;height: 34px;border-radius: 5px;" id="MReject_' + machineid + '">' +
        '</select></div></div></form><div style="text-align: center; margin-top: 34px;"><button type="button" class="btn btn-success" style="width:22%" data-dismiss="modal" onclick="MainRejectsave1(' + machineid + ')">Save</button></div>' +
        '</div> </div>');

    $.ajax({
        url: '/OperatorEntryModel/MaintReject',
        type: 'GET',
        async: false,
        data: { machineid },
        success: function (data) {
            var Result = JSON.parse(data);
            $("#MReject_" + machineid).html('');
            for (var i = 0; i < Result.length; i++) {
                $("#MReject_" + machineid).append("<option value='" + Result[i].RID + "'>" + Result[i].RejectName + "</option>");
            }
        }
    });

    $('.brkdwn1_' + machineid + '').removeAttr('data-target', '#machineAcceptanceBtn');
    $('.brkdwn1_' + machineid + '').attr('data-target', '#RejectReasonBtn');
}

function MainRejectsave1(machineid) {
    var rejectid = $("#MReject_" + machineid).val();
    $.ajax({
        url: '/OperatorEntryModel/RejectMaintance1',
        type: 'GET',
        async: false,
        data: { machineid, rejectid },
        success: function (data) {
            if (data == "true") {
                $('.brkdwn1_' + machineid).removeAttr('data-target', '#loginBtn');
                $('.brkdwn1_' + machineid).attr('data-target', '#breakdownBtn');
                window.location.reload(true)
               // BreakRework1(machineid);
            }
        }
    });
}



function MaintAccept1(id) {
    var Machineid = id;
    $.ajax({
        type: 'POST',
        data: { Machineid },
        url: '/OperatorEntryModel/UpdateMaint1',
        success: function (data) {
            if (data != 'Fail') {
                $('#machineAcceptanceBtn').hide();
                $('#loginBtn').hide();
                $('.brkdwn1_' + Machineid).removeAttr('data-target', '#breakdownBtn');
                $('.brkdwn1_' + Machineid).attr('data-target', '#loginBtn');
                window.location.reload(true);
                //BreakRework1(id);
            }
        }
    });
}


function BreakRework1() {
    $.ajax({
        type: "GET",
        data: { },
        url: "/OperatorEntryModel/ReworkBreakdownStartTime1",
        success: function (data) {
            var Result = JSON.parse(data);            
            for (var i = 0; i < Result.length; i++) {
                var ccsData = '';                
                var MachID = Result[i].MachineID;
                var Rstart = Result[i].ReworkStart;
                var Bstart = Result[i].ContentName;
                if (Rstart == 1) {
                    ccsData += '<div class="row"><div class="col-sm-12"> <b class="partDetails">Rework Start Time :' + Result[i].ReworkStartTime + '  </b> </div></div>';
                }
                else if (Rstart == 2) {
                    ccsData += '<div class="row"><div class="col-sm-12"> <b class="partDetails">Rework Finished  </b> </div></div>';
                }
                if (Bstart != null) {
                    ccsData += '<div class="row"><div class="col-sm-12"> <b class="partDetails">'+Bstart+' :' + Result[i].BreakDownStartTime + '  </b> </div></div>';
                }               
                $("#ReBreak1_" + MachID).html('');
                $("#ReBreak1_" + MachID).append(ccsData);
            }
        }
    });
}



function openLogin1(Mid) {

    var r1 = $("#Reject1_" + Mid).val();
    var r2 = $("#Reject2_" + Mid).val();
    var r3 = $("#Reject3_" + Mid).val();
    if (r3 != "0" && r3 != null) {
        var BreakDownID = r3;
        $.ajax({
            type: 'Post',
            data: { BreakDownID, Mid },
            url: '/OperatorEntryModel/BreakDownReasonStore1',
            success: function (data) {
                if (data != "") {
                    window.location.reload(true);
                   // BreakRework1(Mid);
                    $('.brkdwn1_' + Mid + '').removeAttr('data-target', '#breakdownBtn');
                    $('.brkdwn1_' + Mid + '').attr('data-target', '#loginBtn');
                }
                else {
                    alert("Please Start the Wrok order");
                }
            }
        });
    }
    else if (r2 != "0") {
        var BreakDownID = r2;
        $.ajax({
            type: 'Post',
            data: { BreakDownID, Mid },
            url: '/OperatorEntryModel/BreakDownReasonStore1',
            success: function (data) {
                if (data != "") {
                    window.location.reload(true);
                   // BreakRework1(Mid);
                    $('.brkdwn1_' + Mid + '').removeAttr('data-target', '#breakdownBtn');
                    $('.brkdwn1_' + Mid + '').attr('data-target', '#loginBtn');
                }
                else {
                    alert("Please Start the Wrok order");
                }
            }
        });
    }
    else if (r1 != '0') {
        var BreakDownID = r1;
        $.ajax({
            type: 'Post',
            data: { BreakDownID, Mid },
            url: '/OperatorEntryModel/BreakDownReasonStore1',
            success: function (data) {
                if (data != "") {
                    window.location.reload(true);
                    //BreakRework1(Mid);
                    $('.brkdwn1_' + Mid + '').removeAttr('data-target', '#breakdownBtn');
                    $('.brkdwn1_' + Mid + '').attr('data-target', '#loginBtn');
                }
                else {
                    alert("Please Start the Wrok order");
                }
            }
        });

    }

    //breakdownModal(id);

    //$('#breakdownBtn').modal('hide');
    //$('#loginBtn').modal('show');
}