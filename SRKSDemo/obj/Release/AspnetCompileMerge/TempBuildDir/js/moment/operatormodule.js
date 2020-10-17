$(document).ready(function () {
   

    $.ajax({
        type: 'GET',
        url: '/OperatorEntryModel/GetMachineBaseStart',
        success: function (data) {
            var DataMachStart = JSON.parse(data);
            for (var i = 0; i < DataMachStart.length; i++) {
                if (DataMachStart[i].TrueorFalse == "True") {
                    $("#StartMain_" + DataMachStart[i].MachineID).html('Finish');
                }
            }
        }
    });

    function InsertToDb(id) {
        var machineID = id;
        var Shift = $("#ShiftDet_"+id).val();
        var PartNo = $("#PartVal_" + id).val();
        var OPNO = $("#OPVal_" + id).val();
        var WONo = $("#WOVal_" + id).val();
        var WOQValue = $("#WOQVal_" + id).val();
        var OperatorID=@ViewBag.OperatorIDs;
        $.ajax({
            url:"/OperatorEntryModel/InsertData",
            type: "POST",
            data: { machineID, Shift, PartNo, OPNO, WONo, WOQValue, OperatorID },
            success: function (data) {
                if (data == "Success") {
                    location.reload(true);                        
                    //window.location.reload('/OperatorEntryModel/Index');
                    //$("#FinishMain").show();
                    //$("#holdsub").show();
                    //$("#Idelsub").show();
                    //$("#startsub").hide();
                    //$("#StartMain").hide();
                    //$(".Finishmainclass").show();
                }
            }
        })
    }

    function UpdateToDb(id) {
        var machineID = id;
        var PartNo = $("#PartVal_" + id).val();
        var OPNO = $("#OPVal_" + id).val();
        var WONo = $("#WOVal_" + id).val();
        var WOQValue = $("#WOQVal_" + id).val();
        var OperatorID=@ViewBag.OperatorIDs;
        $.ajax({
            url:"/OperatorEntryModel/UpdateData",
            type: "POST",
            data: { machineID, PartNo, OPNO, WONo, WOQValue, OperatorID },
            success: function (data) {
                if (data == "Success") {
                    location.reload(true);                        
                    //window.location.reload('/OperatorEntryModel/Index');
                    //$("#FinishMain").show();
                    //$("#holdsub").show();
                    //$("#Idelsub").show();
                    //$("#startsub").hide();
                    //$("#StartMain").hide();
                    //$(".Finishmainclass").show();
                }
            }
        })
    }
    function openStartModal(id) {
        $.ajax({
            data: { id },
            url: '/OperatorEntryModel/OperatorEntryDetails',
            type: 'GET',
            success: function (dataDet) {
                $("#WOStart").html('');
                if (dataDet != "FAIL") {
                    var Result = JSON.parse(dataDet);                        
                    for (var i = 0; i < Result.length; i++) {
                        $("#WOStart").append('<div class="modal-body"><div style="margin-left: 116px;"><input type="hidden" class="StartButton" id="NoMach" value="" />' +
                            '<p>Shift   <span style="margin-left: 76px;">:</span><input class="input-box" type="text" id="ShiftDet_' + id + '" value=' + Result[i].ShiftID + ' readonly>' +
                            '</p><p> Part No  <span style="margin-left: 55px;">:</span><input class="input-box" type="text" id="PartVal_' + id + '" value=' + Result[i].PartNo + ' readonly> </p>' +
                            '<p> WO No  <span style="margin-left: 59px;">:</span> <input class="input-box" style="margin-left: 0px;" type="text" id="WOVal_' + id + '" value=' + Result[i].WONO + ' readonly> </p>' +
                            '<p> Operation No  <span style="margin-left: 13px;">:</span> <input class="input-box" style="margin-left: 0px;" type="text" id="OPVal_' + id + '" value=' + Result[i].OperationNo + ' readonly> </p>' +
                            '<p> WO Qty  <span style="margin-left: 55px;">:</span> <input class="input-box" style="margin-left: 0px;" type="text" id="WOQVal_' + id + '"  value=' + Result[i].WOQTY + ' readonly> </p>' +
                            '</div><div style="text-align: center; margin-top: 34px;" id="SubMain_' + id + '">' +
                            '<button type="button" class="btn btn-outline-info" style="width:22%" data-dismiss="modal" id="finishsub_' + id + '" onclick="UpdateToDb(' + id + ');">Finish</button>'+
                            '<p id="Hold"></p></div ></div > ');
                        //$("#HoldDet_" + id).hide();
                        //GetHOLDDet();
                        //GetSHIFTDet();
                        //var shiftval = Result[i].ShiftID;
                        //$("#ShiftDet_" + id).val(shiftval);
                    }
                }
                else {
                    $("#WOStart").append('<div class="modal-body"><div style="margin-left: 116px;"><input type="hidden" class="StartButton" id="NoMach" value="" />' +
                        '<p>Shift   <span style="margin-left: 76px;">:&nbsp;</span><select style="width: 206px;height: 34px;border-radius: 5px;" id="ShiftDet_' + id + '"></select>' +
                        '</p><p> Part No  <span style="margin-left: 55px;">:</span><input class="input-box" type="text" id="PartVal_' + id + '"> </p>' +
                        '<p> WO No  <span style="margin-left: 59px;">:</span> <input class="input-box" style="margin-left: 0px;" type="text" id="WOVal_' + id + '"> </p>' +
                        '<p> Operation No  <span style="margin-left: 13px;">:</span> <input class="input-box" style="margin-left: 0px;" type="text" id="OPVal_' + id + '"> </p>' +
                        '<p> WO Qty  <span style="margin-left: 55px;">:</span> <input class="input-box" style="margin-left: 0px;" type="text" id="WOQVal_' + id + '"> </p>' +
                        '</div><div style="text-align: center; margin-top: 34px;" id="SubMain_"' + id + '>' +
                        '<button type="button" class="btn btn-outline-info" style="width:22%" data-dismiss="modal" id="startsub_"' + id + ' onclick="InsertToDb(' + id + ');">Start</button></div></div>');
                    GetSHIFTDet();
                }

            },
            fail: function (dataDet) {
                    
            }
        });

        //function GetHOLDDet(id) {
        //    $.ajax({
        //        url: '/OperatorEntryModel/GetHoldDet',
        //        type: 'GET',
        //        success: function (data) {
        //            var Mesg = JSON.parse(data);
        //            $("#HoldDet_" + id).html('');
        //            $("#HoldDet_" + id).append(("<option value=0>---Select Hold Code---</option>"))
        //            for (var i = 0; i <= Mesg.length; i++) {
        //                $("#HoldDet_" + id).append(("<option value='" + Mesg[i].HoldCodeID + "'>" + Mesg[i].HoldCode + "</option>"))
        //            }
        //        }
        //    });

        //    $()

        //    $("#HoldDet_" + id).show();
        //}

        function GetSHIFTDet() {
            $.ajax({
                url: '/OperatorEntryModel/GetShiftDet',
                type: 'GET',
                success: function (data) {
                    var Mesg = JSON.parse(data);
                    $("#ShiftDet").html('');
                    $("#ShiftDet_" + id).append(("<option value=0>--Select Shift--</option>"))
                    for (var i = 0; i <= Mesg.length; i++) {
                        $("#ShiftDet_" + id).append(("<option value='" + Mesg[i].ShiftID + "'>" + Mesg[i].ShiftName + "</option>"))
                    }
                }
            });
        }


        $("#NoMach").val(id);
        $('#startBtn').modal('show');
    }

    // function openRejectionModal(){
    //     $('#rejectionBtn').modal('show');
    // }

    function reworkModal() {
        $('#reworkBtn').modal('show');
    }

    function breakdownModal(id) {
        $('#breakdownBtn').modal('show');
    }

    function openLogin() {
        $('#breakdownBtn').modal('hide');
        $('#loginBtn').modal('show');
    }

    function machineAcceptance() {
        $('#loginBtn').modal('hide');
        $('#machineAcceptanceBtn').modal('show');
    }

    function reject() {
        $('#machineAcceptanceBtn').modal('hide');
        $('#RejectReasonBtn').modal('show');
    }

    function machineClosure() {
        $('#machineAcceptanceBtn').modal('hide');
        $('#maintenaceClousreBtn').modal('show');
    }

    function productionLoginModal() {
        $('#maintenaceClousreBtn').modal('hide');
        $('#productionloginBtn').modal('show');
    }

    function productionDetailsModal() {
        $('#productionloginBtn').modal('hide');
        $('#productionDetailsBtn').modal('show');

    }

    function productionRejectionModal() {
        $('#productionDetailsBtn').modal('hide');
        $('#prodRejectReasonBtn').modal('show');

    }
});