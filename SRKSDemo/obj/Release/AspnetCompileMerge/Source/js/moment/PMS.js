$(document).ready(function () {
    GetPlant();
    var cellid = '';

    function GetPlant() {

        $.get("/PreventiveMaintainanceScheduling/GetPlant", {}, function (msg) {
            var ddlPlant = $("#PlantID");
            ddlPlant.empty().append('<option selected="selected" value="0">--Select Plant--</option>');
            if (msg != '') {
                data = JSON.parse(msg);

                for (var i = 0; i < data.length; i++) {
                    ddlPlant.append('<option  value="' + data[i].PlantID + '">' + data[i].PlantName + '</option>');
                }

            }

        });
    }

    function GetMonth() {
        $.get('/PreventiveMaintainanceScheduling/GetMonth', {}, function (msg) {
            var ddlMonth = $(".Month");
            ddlMonth.empty().append('<option selected="selected" value="0">--Select Month--</option>');
            if (msg != '') {
                data = JSON.parse(msg);

                for (var i = 0; i < data.length; i++) {
                    ddlMonth.append('<option  value="' + data[i].MonthID + '">' + data[i].Text + '</option>');
                }

            }

        });
    }


    function GetWeek() {
        $.get('/PreventiveMaintainanceScheduling/GetWeek', {}, function (msg) {
            var ddlWeek = $(".Week");
            ddlWeek.empty().append('<option selected="selected" value="0">--Select Week--</option>');
            if (msg != '') {
                data = JSON.parse(msg);

                for (var i = 0; i < data.length; i++) {
                    ddlWeek.append('<option  value="' + data[i].WeekID + '">' + data[i].value + '</option>');
                }

            }

        });
    }


    function GetMonthdet(MonthID, Classname) {
        $.get('/PreventiveMaintainanceScheduling/GetMonth', {}, function (msg) {
            var ddlMonth = $('#' + Classname + '');

            ddlMonth.empty().append('<option  value="0">--Select Month--</option>');
            if (msg != '') {
                data = JSON.parse(msg);

                for (var i = 0; i < data.length; i++) {
                    if (data[i].Text == MonthID) {
                        ddlMonth.append('<option selected="selected"  value="' + data[i].MonthID + '">' + data[i].Text + '</option>');
                    }
                    else {
                        ddlMonth.append('<option  value="' + data[i].MonthID + '">' + data[i].Text + '</option>');
                    }
                }

            }

        });
    }

    function GetWeekdet(WeekId, Classname) {
        $.get('/PreventiveMaintainanceScheduling/GetWeek', {}, function (msg) {
            var ddlWeek = $('#' + Classname + '');

            ddlWeek.empty().append('<option  value="0">--Select Week--</option>');
            if (msg != '') {
                data = JSON.parse(msg);

                for (var i = 0; i < data.length; i++) {
                    if (data[i].WeekID == WeekId) {
                        ddlWeek.append('<option selected="selected"  value="' + data[i].WeekID + '">' + data[i].value + '</option>');
                    }
                    else {
                        ddlWeek.append('<option  value="' + data[i].WeekID + '">' + data[i].value + '</option>');
                    }
                }

            }

        });
    }


    $("#PlantID").change(function () {

        var PlantID = $("#PlantID").val();
        $.get("/PreventiveMaintainanceScheduling/GetShop", { PlantID }, function (msg) {
            var ddlshop = $("#shop");
            ddlshop.empty().append('<option selected="selected" value="0">--Select Shop--</option>');
            if (msg != '') {
                data = JSON.parse(msg);

                for (var i = 0; i < data.length; i++) {
                    ddlshop.append('<option  value="' + data[i].ShopID + '">' + data[i].ShopName + '</option>');
                }

            }

        });
    });

    $("#shop").change(function () {
        var ShopID = $("#shop").val();

        $.get("/PreventiveMaintainanceScheduling/GetCell", { ShopID }, function (msg) {
            var ddlCell = $("#cell");
            ddlCell.empty().append('<option selected="selected" value="0">--Select Cell--</option>');
            if (msg != '') {
                data = JSON.parse(msg);

                for (var i = 0; i < data.length; i++) {
                    ddlCell.append('<option  value="' + data[i].CellID + '">' + data[i].CellName + '</option>');
                }

            }

        });
    });

    $("#btnGetDetails").click(function () {
        var PlantID = $("#PlantID").val();

        var ShopID = $("#shop").val();
        var CellID = $("#cell").val();
        cellid = CellID;
        var PlantName = $("#PlantID :selected").text();
        var ShopName = $("#shop :selected").text();
        var CellName = $("#cell :selected").text();
        if (PlantID !== '0' && ShopID !== '0' && CellID !== '0') {
            $.get('/PreventiveMaintainanceScheduling/GetDetails', { CellID }, function (msg) {

                if (msg !== '') {
                    if (msg === 'Saved')
                        window.location.href = '/PreventiveMaintainanceScheduling/Index';
                    else {
                        $("#tblsection").css('display', 'block');
                        $("#divPMS").css('display', 'block');
                        $('.divcreate').css('display', 'none');
                        $("#PlantName").html(PlantName);
                        $('#ShopName').html(ShopName);
                        $('#CellName').html(CellName);
                        var data = JSON.parse(msg);
                        $('.pmsdata').html('');
                        var cssdata = '';

                        for (var i = 0; i < data.length; i++) {
                            var pmslist = data[i].pmsdetailsList;
                            var MachineName = data[i].MachineName;
                            var pmscount = pmslist.length;
                            cssdata += '<tr>';
                            var count = 0;

                            if (pmslist.length > 0) {
                                for (var j = 0; j < pmslist.length; j++) {
                                    count = count + 1;
                                    //if (j == 0) {
                                    //    cssdata += '<td style="color:black; border:1px solid Black">' + pmslist[j].MachineName + '</td>';
                                    //    if (pmslist[j].month != 0 && pmslist[j].week != 0) {

                                    //        var className = 'Monthname_' + pmslist[j].pmid + '';
                                    //        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Month" id=Monthname_' + pmslist[j].pmid + '> </select></td>';
                                    //        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Week" id=WeekName' + pmslist[j].pmid + '> </select></td>';
                                    //        cssdata += '<td style="border:1px solid Black"><button style="background-color: Red; color: white; font-size: 20px;border-radius: 50%; border: 1px; height: 30px; width: 30px;"> - </button></td>';

                                    //        //GetWeek(data[j].month, className);
                                    //    }
                                    //    else {
                                    //        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Month" id=Monthname_' + pmslist[j].pmid + '> </select></td>';
                                    //        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Week" id=WeekName' + pmslist[j].pmid + '> </select></td>';
                                    //        cssdata += '<td style="border:1px solid Black"><button style="background-color: Green; color: white; font-size: 20px;border-radius: 50%; border: 1px; height: 30px; width: 30px;"> + </button></td>';
                                    //    }

                                    //}
                                    if (j === 0) {
                                        cssdata += '<td style="color:black; border:1px solid Black">' + pmslist[j].MachineName + '</td>';
                                        if (pmslist[j].month !== 0 && pmslist[j].week !== 0) {

                                            var className = 'Monthname_' + pmslist[j].pmid + '';
                                            cssdata += '<td style="border:1px solid Black"><select style="color:black" class="monthdropdown" id=Monthname_' + pmslist[j].pmid + '> </select></td>';
                                            cssdata += '<td style="border:1px solid Black"><select style="color:black" class="weekdropdown" id=WeekName_' + pmslist[j].pmid + '> </select></td>';
                                            cssdata += '<td style="border:1px solid Black"><button class="btn-remove" style="background-color: Red; color: white; font-size: 20px;border-radius: 50%; border: 1px; height: 30px; width: 30px;" name=' + pmslist[j].pmid + ' id = month' + pmslist[j].pmid + '> - </button></td>';

                                            //GetWeek(data[j].month, className);
                                        }
                                        else {
                                            cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Month" id=Monthname_' + pmslist[j].pmid + ' name=' + pmslist[j].pmid + '> </select></td>';
                                            cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Week" id=WeekName_' + pmslist[j].pmid + ' name=' + pmslist[j].pmid + '> </select></td>';
                                            cssdata += '<td style="border:1px solid Black"><button class="btn-add" style="background-color: Green; color: white; font-size: 20px;border-radius: 50%; border: 1px; height: 30px; width: 30px;" name=' + pmslist[j].pmid + ' id = month' + pmslist[j].pmid + '> + </button></td>';
                                        }

                                    }
                                    //else if (j > 0) {
                                    //    cssdata += '<td></td>';
                                    //    if (pmslist[j].month != 0 && pmslist[j].week != 0) {

                                    //        var className = 'Monthname_' + pmslist[j].pmid + '';
                                    //        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Month" id=Monthname_' + pmslist[j].pmid + '> </select></td>';
                                    //        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Week" id=WeekName' + pmslist[j].pmid + '> </select></td>';
                                    //        cssdata += '<td style="border:1px solid Black"><button style="background-color: Red; color: white; font-size: 20px;border-radius: 50%; border: 1px; height: 30px; width: 30px;"> - </button></td>';

                                    //        //GetWeek(data[j].month, className);
                                    //    }
                                    //    else {
                                    //        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Month" id=Monthname_' + pmslist[j].pmid + '> </select></td>';
                                    //        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Week" id=WeekName' + pmslist[j].pmid + '> </select></td>';
                                    //        cssdata += '<td style="border:1px solid Black"><button style="background-color: Green; color: white; font-size: 20px;border-radius: 50%; border: 1px; height: 30px; width: 30px;"> + </button></td>';
                                    //    }
                                    //    cssdata += '</tr>';
                                    //}
                                    else if (j > 0) {
                                        cssdata += '<td></td>';
                                        if (pmslist[j].month !== 0 && pmslist[j].week !== 0) {

                                            var className = 'Monthname_' + pmslist[j].pmid + '';
                                            cssdata += '<td style="border:1px solid Black"><select style="color:black" class="monthdropdown" id=Monthname_' + pmslist[j].pmid + '> </select></td>';
                                            cssdata += '<td style="border:1px solid Black"><select style="color:black" class="weekdropdown" id=WeekName_' + pmslist[j].pmid + '> </select></td>';
                                            cssdata += '<td style="border:1px solid Black"><button class="btn-remove" style="background-color: Red; color: white; font-size: 20px;border-radius: 50%; border: 1px; height: 30px; width: 30px;" name=' + pmslist[j].pmid + ' id = month' + pmslist[j].pmid + '> - </button></td>';

                                            //GetWeek(data[j].month, className);
                                        }
                                        else {
                                            cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Month" id=Monthname_' + pmslist[j].pmid + ' name=' + pmslist[j].pmid + '> </select></td>';
                                            cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Week" id=WeekName_' + pmslist[j].pmid + ' name=' + pmslist[j].pmid + '> </select></td>';
                                            cssdata += '<td style="border:1px solid Black"><button class="btn-add" style="background-color: Green; color: white; font-size: 20px;border-radius: 50%; border: 1px; height: 30px; width: 30px;" name=' + pmslist[j].pmid + ' id = month' + pmslist[j].pmid + '> + </button></td>';
                                        }
                                        cssdata += '</tr>';
                                    }

                                    //else {
                                    //    cssdata += '<tr>'
                                    //    cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Month " id=Monthname_' + pmslist[j].MachineID + '> </select></td>';
                                    //    cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Week "id=Weekname_' + pmslist[j].MachineID + '> </select></td>';
                                    //    cssdata += '<td style="border:1px solid Black"><button class="btn-add" style="background-color: Green; color: white; font-size: 20px;border-radius: 50%; border: 1px; height: 30px; width: 30px;  id="month' + pmslist[j].MachineID  > + </button ></td > ';
                                    //    cssdata += '</tr>';
                                    //}

                                    cssdata += '<tr>';
                                    if (count === pmscount && pmslist[j].month !== 0 && pmslist[j].week !== 0) {


                                        cssdata += '<td></td>';
                                        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Month" id=Monthname_' + pmslist[j].MachineID + ' name=' + pmslist[j].MachineID + '> </select></td>';
                                        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Week" id=Weekname_' + pmslist[j].MachineID + ' name=' + pmslist[j].MachineID + '> </select></td>';
                                        cssdata += '<td style="border:1px solid Black"><button class="btn-add1" style="background-color: Green; color: white; font-size: 20px;border-radius: 50%; border: 1px; height: 30px; width: 30px;" name=' + pmslist[j].MachineID + ' id = month' + pmslist[j].MachineID + '> + </button ></td > ';

                                    }
                                    cssdata += '</tr>';
                                }
                            }
                            ////else {
                            ////    cssdata += '<tr>'
                            ////    cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Month Monthname_' + pmslist[j].MachineID + '"> </select></td>';
                            ////    cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Week Weekname_' + pmslist[j].MachineID + '"> </select></td>';
                            ////    cssdata += '<td style="border:1px solid Black"><button style="background-color: Green; color: white; font-size: 20px;border-radius: 50%; border: 1px; height: 30px; width: 30px;"> + </button></td>';
                            ////    cssdata += '</tr>';
                            ////}

                        }

                        $(cssdata).appendTo($('.pmsdata'));

                        GetMonth();
                        GetWeek();

                        GetMonth_Week(data);

                    }
                }
            });
            return false;
        }
        else {
            $("#errorMessage").html('Please Select Required Details');
            return false;
        }
    });

    function GetMonth_Week(data) {
        for (var i = 0; i < data.length; i++) {
            var pmslist = data[i].pmsdetailsList;
            for (var j = 0; j < pmslist.length; j++) {
                if (pmslist[j].month !== 0 && pmslist[j].week !== 0) {
                    var className = 'Monthname_' + pmslist[j].pmid + '';
                    var classNameWeek = 'WeekName_' + pmslist[j].pmid + '';
                    GetMonthdet(pmslist[j].month, className);
                    GetWeekdet(pmslist[j].week, classNameWeek);
                }
            }
        }
    }
    $(document).on('click', '.btn-add', function (e) {
        var id = $(this).attr('id');
        var name = $(this).attr('name');
        var monthValue = $("#Monthname_" + name).val();
        var weekvalue = $("#WeekName_" + name).val();
        if (monthValue !== "" && weekvalue !== "") {
            $.get("/PreventiveMaintainanceScheduling/InsertData", { id: name, monthValue: monthValue, weekvalue: weekvalue }, function (data) {
                if (data !== null && data !== "") {

                }
            });
        }
        GetPMSDetails(cellid);
    });

    function GetPMSDetails(cellid) {
        var CellID = cellid;
        var PlantName = $("#PlantID :selected").text();
        var ShopName = $("#shop :selected").text();
        var CellName = $("#cell :selected").text();
        $.get('/PreventiveMaintainanceScheduling/GetDetails', { CellID }, function (msg) {

            if (msg !== '') {
                if (msg === 'Saved')
                    window.location.href = '/PreventiveMaintainanceScheduling/Index';
                else {
                    $("#tblsection").css('display', 'block');
                    $("#divPMS").css('display', 'block');
                    $('.divcreate').css('display', 'none');
                    $("#PlantName").html(PlantName);
                    $('#ShopName').html(ShopName);
                    $('#CellName').html(CellName);
                    var data = JSON.parse(msg);
                    $('.pmsdata').html('');
                    var cssdata = '';

                    for (var i = 0; i < data.length; i++) {
                        var pmslist = data[i].pmsdetailsList;
                        var MachineName = data[i].MachineName;
                        var pmscount = pmslist.length;
                        cssdata += '<tr>';
                        var count = 0;

                        if (pmslist.length > 0) {
                            for (var j = 0; j < pmslist.length; j++) {
                                count = count + 1;
                                if (j === 0) {
                                    cssdata += '<td style="color:black; border:1px solid Black">' + pmslist[j].MachineName + '</td>';
                                    if (pmslist[j].month !== 0 && pmslist[j].week !== 0) {

                                        var className = 'Monthname_' + pmslist[j].pmid + '';
                                        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="monthdropdown" id=Monthname_' + pmslist[j].pmid + '> </select></td>';
                                        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="weekdropdown" id=WeekName_' + pmslist[j].pmid + '> </select></td>';
                                        cssdata += '<td style="border:1px solid Black"><button class="btn-remove" style="background-color: Red; color: white; font-size: 20px;border-radius: 50%; border: 1px; height: 30px; width: 30px;" name=' + pmslist[j].pmid + ' id = month' + pmslist[j].pmid + '> - </button></td>';

                                        //GetWeek(data[j].month, className);
                                    }
                                    else {
                                        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Month" id=Monthname_' + pmslist[j].pmid + ' name=' + pmslist[j].pmid + '> </select></td>';
                                        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Week" id=WeekName_' + pmslist[j].pmid + ' name=' + pmslist[j].pmid + '> </select></td>';
                                        cssdata += '<td style="border:1px solid Black"><button class="btn-add" style="background-color: Green; color: white; font-size: 20px;border-radius: 50%; border: 1px; height: 30px; width: 30px;" name=' + pmslist[j].pmid + ' id = month' + pmslist[j].pmid + '> + </button></td>';
                                    }

                                }
                                else if (j > 0) {
                                    cssdata += '<td></td>';
                                    if (pmslist[j].month !== 0 && pmslist[j].week !== 0) {

                                        var className = 'Monthname_' + pmslist[j].pmid + '';
                                        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="monthdropdown" id=Monthname_' + pmslist[j].pmid + '> </select></td>';
                                        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="weekdropdown" id=WeekName_' + pmslist[j].pmid + '> </select></td>';
                                        cssdata += '<td style="border:1px solid Black"><button class="btn-remove" style="background-color: Red; color: white; font-size: 20px;border-radius: 50%; border: 1px; height: 30px; width: 30px;" name=' + pmslist[j].pmid + ' id = month' + pmslist[j].pmid + '> - </button></td>';

                                        //GetWeek(data[j].month, className);
                                    }
                                    else {
                                        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Month" id=Monthname_' + pmslist[j].pmid + ' name=' + pmslist[j].pmid + '> </select></td>';
                                        cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Week" id=WeekName_' + pmslist[j].pmid + ' name=' + pmslist[j].pmid + '> </select></td>';
                                        cssdata += '<td style="border:1px solid Black"><button class="btn-add" style="background-color: Green; color: white; font-size: 20px;border-radius: 50%; border: 1px; height: 30px; width: 30px;" name=' + pmslist[j].pmid + ' id = month' + pmslist[j].pmid + '> + </button></td>';
                                    }
                                    cssdata += '</tr>';
                                }

                                cssdata += '<tr>';
                                if (count === pmscount && pmslist[j].month !== 0 && pmslist[j].week !== 0) {


                                    cssdata += '<td></td>';
                                    cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Month" id=Monthname_' + pmslist[j].MachineID + ' name=' + pmslist[j].MachineID + '> </select></td>';
                                    cssdata += '<td style="border:1px solid Black"><select style="color:black" class="Week" id=Weekname_' + pmslist[j].MachineID + ' name=' + pmslist[j].MachineID + '> </select></td>';
                                    cssdata += '<td style="border:1px solid Black"><button class="btn-add1" style="background-color: Green; color: white; font-size: 20px;border-radius: 50%; border: 1px; height: 30px; width: 30px;" name=' + pmslist[j].MachineID + ' id = month' + pmslist[j].MachineID + '> + </button ></td > ';
                                    cssdata += '</tr>';
                                }
                                cssdata += '</tr>';
                            }
                        }

                    }

                    $(cssdata).appendTo($('.pmsdata'));

                    GetMonth();
                    GetWeek();

                    GetMonth_Week(data);

                }
            }
        });

    }
    $(document).on('click', '.btn-remove', function (e) {
        var name = $(this).attr('name');
        $.getJSON("/PreventiveMaintainanceScheduling/DeleteData", { Id: name }, function (res) {

            //GetPMSDetails(cellid);
        });
        GetPMSDetails(cellid);
    });

    $(document).on('click', '.btn-add1', function (e) {
        var name = $(this).attr('name');
        //var name1 = $('.monthdropdown').attr('id');
        //var arry = name1.split('_');
        //monthid = arry[1];
        var monthValue = $("#Monthname_" + name).val();
        var weekvalue = $("#Weekname_" + name).val();
        if (monthValue !== "" && weekvalue !== "") {
            $.get("/PreventiveMaintainanceScheduling/InsertData1", { /*pmid: monthid, */macid: name, monthValue: monthValue, weekvalue: weekvalue }, function (data) {

            });
            //GetPMSDetails(cellid);
        }
        GetPMSDetails(cellid);
    });

    $(document).on('change', ".monthdropdown", function (e) {
        var id = $(this).attr('id');
        var monthValue = $("#" + id).val();
        var arry = id.split('_');
        monthid = arry[1];
        $.getJSON("/PreventiveMaintainanceScheduling/UpdateData", { pmid: monthid, monthValue: monthValue }, function (res) {

        });
        GetPMSDetails(cellid);
    });

    $(document).on('change', ".weekdropdown", function (e) {
        var id = $(this).attr('id');
        var weekvalue = $("#" + id).val();
        var arry = id.split('_');
        weekid = arry[1];
        $.getJSON("/PreventiveMaintainanceScheduling/UpdateData1", { pmid: weekid, weekvalue: weekvalue }, function (res) {

        });
        GetPMSDetails(cellid);
        //console.log('refresh');
        //window.location.reload(true);
    });
    //$(document).on('change', ".monthdropdownList", function (e) {
    //    var id = $(this).attr('id');
    //    var monthValue = $("#" + id).val();
    //    var name = $('.btn-add1').attr('name');
    //    $.getJSON("/PreventiveMaintainanceScheduling/UpdateData", { pmid: name, monthValue: monthValue }, function (res) {

    //    });
    //    //GetPMSDetails(cellid);
    //});

    ////$(document).on('change', ".weekdropdownList", function (e) {
    ////    var id = $(this).attr('id');
    ////    var weekvalue = $("#" + id).val();
    ////    var name = $('.btn-add1').attr('name');
    ////    $.getJSON("/PreventiveMaintainanceScheduling/UpdateData1", { pmid: name, weekvalue: weekvalue }, function (res) {
    ////    });
    ////    //GetPMSDetails(cellid);
    ////});

    //$(document).on('change', ".Month", function (e) {
    //    var id = $(this).attr('id');
    //    var monthValue = $("#" + id).val();
    //    var arry = id.split('_');
    //    monthid = arry[1];
    //    $.getJSON("/PreventiveMaintainanceScheduling/UpdateData", { pmid: monthid, monthValue: monthValue }, function (res) {

    //    });
    //});

    //$(document).on('change', ".Week", function (e) {
    //    var id = $(this).attr('id');
    //    var weekvalue = $("#" + id).val();
    //    var arry = id.split('_');
    //    weekid = arry[1];
    //    $.getJSON("/PreventiveMaintainanceScheduling/UpdateData1", { pmid: weekid, weekvalue: weekvalue }, function (res) {

    //    });
    //});

    $(document).on('click', '.savebtn', function (e) {
        var month = [];
        var week = [];
        var Id = [];
        $.each($(".Week option:selected"), function () {
            week.push($(this).val());
            Id.push($(this).parent()[0].name);
        });
        $.each($(".Month option:selected"), function () {
            month.push($(this).val());
        });
        $.each(Id, function (index) {
            var id = Id[index];
            var mon = month[index];
            var wee = week[index];
            if (mon != 0 && wee != 0) {
                $.post("/PreventiveMaintainanceScheduling/Save", { id: id, mon: mon, wee: wee }, function (res) {
                    //if (res == "Success") {

                    //}
                    //TempData["toaster_success"] = "Data Saved successfully";
                    window.location.href = "/PreventiveMaintainanceScheduling/Index";
                });
            }
            else { }
           
        });
    });

            //var id = Id[index];
            //$.each(month, function (index) {
            //    var mon = month[index];
            //    $.each(week, function (index) {
            //        var wee = week[index];

            //        $.post("/PreventiveMaintainanceScheduling/Save", { id: id, mon: mon, wee: wee }, function (res) {
            //            if (res == "Success") {
                            
            //            }
            //            window.location.href = "/PreventiveMaintainanceScheduling/Index";
            //        });
            //    });
            //});
    //    });
    //});
    
});