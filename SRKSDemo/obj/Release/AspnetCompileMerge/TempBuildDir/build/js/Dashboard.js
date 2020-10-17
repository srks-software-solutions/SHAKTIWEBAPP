$(document).ready(function () {
    
   // var date = today.toLocaleString();
    setInterval(gettime, 1000);

    function gettime() {
        var currentdate = new Date();
        var datetime =  currentdate.getFullYear() + "/"
               + (currentdate.getMonth() + 1) + "/"
               + currentdate.getDate() + "  "
               + currentdate.getHours() + ":"
               + currentdate.getMinutes() + ":"
               + currentdate.getSeconds();
        var dt = new Date();
        var time =dt.getHours() + ":" + dt.getMinutes() + ":" + dt.getSeconds();
        $('#DateTime').html(datetime);
    }
    GetMachine();
    setInterval(GetMachine, 60000);    

    var ctx = document.getElementById("canvasDoughnut1");
    var chart = new CanvasJS.Chart("chartContainer", {
        animationEnabled: true,
        theme: "dark2",
        backgroundColor: "#4b5360",
        title: {
            text: "OEE Trend"
        },
        axisX: {
            stripLines: [
                {
                    color: "#d8d8d8",

                    labelFontColor: "#a8a8a8"
                }
            ]
        },
        axisY: {
            includeZero: false,

        },
        data: [{
       

        }]
    });
    function doughnut(poweron,running,idle,brkdown) {
        var Mdata = {
            labels: [
           "POWERON",
            "RUNNING",
           "IDLE",
           "BreaK Down"

            ],
            datasets: [{
                data: [poweron,running, idle, brkdown],
                backgroundColor: [
                    "#0000CD",
                    "#006400",
                    "#FFFF00",
                    "#FF0000"

                ],
                hoverBackgroundColor: [
                     "#0000CD",
                    "#006400",
                    "#FFFF00",
                    "#FF0000"

                ]

            }]
        };
        var canvasDoughnut = new Chart(ctx, {
            type: 'doughnut',
            tooltipFillColor: "rgba(11, 22, 3, 0.55)",
            data: Mdata,
            options: {
                cutoutpercentage:20,
                animation: {
                    animationScale: true
                }
            }
        });
        //canvasDoughnut.data = Mdata;
        //canvasDoughnut.render();
    }

    function GetMachine() {
       
        $.get("/Dashboard/GetCurrentMachineStatus", {}, function (msg) {
            if (msg != '') {
                var data = JSON.parse(msg);
                $('#lblavail').html(data.Availability + " %");
                $('#lblqual').html(data.Quality + " %");
                $('#lblPerf').html(data.Performance + " %");
                var value = data.OEE;
                chart2.arrows[0].setValue(value);
                chart2.axes[0].setTopText(value + " %");
                // adjust darker band to new value
                chart2.axes[0].bands[1].setEndValue(value);
                var machinetime = data.Machinetimes;
                var IDLELOSSES = data.TopIDLELosses;
                var BRKLOSSES = data.TopbrkdwnLosses;
                var TopLosses = data.TopLosses;
                //var percent1 = parseInt(IDLELOSSES[0].LossPercent);
                //var percent2 = parseInt(IDLELOSSES[1].LossPercent);
                //var percent3 = parseInt(IDLELOSSES[2].LossPercent);
                //var percent4 = parseInt(IDLELOSSES[3].LossPercent);
                //var LossName1 = "";
                //var LossName2 = "";
                //var LossName3 = "";
                //var LossName4 = "";
                //for (var i = 0; i < IDLELOSSES.length; i++) {
                //    if (i == 0) {
                //        percent1 = parseInt(IDLELOSSES[0].LossPercent);
                //        LossName1 = IDLELOSSES[0].LossName;
                //    }
                //    else if (i == 1) {
                //        percent2 = parseInt(IDLELOSSES[1].LossPercent);
                //        LossName2 = IDLELOSSES[1].LossName;
                //    }
                //    else if (i == 2) {
                //        percent3 = parseInt(IDLELOSSES[2].LossPercent);
                //        LossName3 = BRKLOSSES[2].LossName;
                //    }
                //    else if (i == 3) {
                //        percent4 = parseInt(IDLELOSSES[3].LossPercent);
                //        LossName4 = IDLELOSSES[3].LossName;

                //    }

                //}

                getabnormality(TopLosses);
                getIDLE(IDLELOSSES);
                getaBrkDown(BRKLOSSES);
                var poweron = parseInt(machinetime.PowerON / 60);
                var running = parseInt(machinetime.RunningTime / 60);
                var idle = parseInt(machinetime.IDLETime / 60);
                var brkdowntime = parseInt(machinetime.BreakDownTime / 60);
                $("#pon").html(parseInt(poweron));
                $("#ponperc").html(parseInt(machinetime.PowerONPerc) + " %");

                $("#running").html(parseInt(running));
                $("#runningPerc").html(parseInt(machinetime.RunningTimePerc) + " %");

                $("#idle").html(parseInt(idle));
                $("#idleperc").html(parseInt(machinetime.IDLETimePerc) + " %");

                $("#brkdwn").html(parseInt(brkdowntime));
                $("#brkdwnPerc").html(parseInt(machinetime.BreakDownTimePerc) + " %");

                // doughnut
                //MachineData.push( parseInt( machinetime.PowerONPerc));
                //MachineData.push(parseInt(machinetime.RunningTimePerc));
                //MachineData.push(parseInt(machinetime.IDLETimePerc));
                //MachineData.push(parseInt(machinetime.BreakDownTimePerc));
                //Mdata.datasets[0].data = MachineData;

                doughnut(parseInt(machinetime.PowerONPerc), parseInt(machinetime.RunningTimePerc), parseInt(machinetime.IDLETimePerc), parseInt(machinetime.BreakDownTimePerc));
                //$("#IDLELoss1").html(LossName1);
                //$("#divIDLELoss1").css("width", percent1.toString() + "%");
                //$("#divIDLELoss1").attr("aria-valuenow", percent1);
                //$("#divIDLELoss1").html(percent1 + " %");
                //$("#IDLELoss2").html(LossName2);
                //$("#divIDLELoss2").css("width", percent2 + "%");
                //$("#divIDLELoss2").attr("aria-valuenow", percent2);
                //$("#divIDLELoss2").html(percent2.toString() + " %");
                //$("#IDLELoss3").html(LossName3);

                //$("#divIDLELoss3").css("width", percent3.toString() + "%");
                //$("#divIDLELoss3").attr("aria-valuenow", percent3);
                //$("#divIDLELoss3").html(percent3.toString() + " %");
                //$("#IDLELoss4").html(LossName4);
                //$("#divIDLELoss4").css("width", percent4.toString() + "%");
                //$("#divIDLELoss4").attr("aria-valuenow", percent4);
                //$("#divIDLELoss4").html(percent4.toString() + " %");
                //var BRKpercent1 = 0;
                //var BRKpercent2 = 0;
                //var BRKpercent3 = 0;
                //var BRKpercent4 = 0;
                //var BRKLossName1 = "";
                //var BRKLossName2 = "";
                //var BRKLossName3 = "";
                //var BRKLossName4 = "";
                //for (var i = 0; i < BRKLOSSES.length; i++) {
                //    if (i == 0) {
                //        BRKpercent1 = parseInt(BRKLOSSES[0].LossPercent);
                //        BRKLossName1 = BRKLOSSES[0].LossName;
                //    }
                //    else if (i == 1) {
                //        BRKpercent2 = parseInt(BRKLOSSES[1].LossPercent);
                //        BRKLossName2 = BRKLOSSES[1].LossName;
                //    }
                //    else if (i == 2) {
                //        BRKpercent3 = parseInt(BRKLOSSES[2].LossPercent);
                //        BRKLossName3 = BRKLOSSES[2].LossName;
                //    }
                //    else if (i == 3) {
                //        BRKpercent4 = parseInt(BRKLOSSES[3].LossPercent);
                //        BRKLossName4 = BRKLOSSES[3].LossName;

                //    }

                //}
                //$("#BRKLoss1").html(BRKLossName1);
                //$("#divBRKLoss1").css("width", BRKpercent1.toString() + "%");
                //$("#divBRKLoss1").attr("aria-valuenow", BRKpercent1);
                //$("#divBRKLoss1").html(BRKpercent1 + " %");
                //$("#BRKLoss2").html(BRKLossName2);
                //$("#divBRKLoss2").css("width", BRKpercent2 + "%");
                //$("#divBRKLoss2").attr("aria-valuenow", BRKpercent2);
                //$("#divBRKLoss2").html(BRKpercent2.toString() + " %");
                //$("#BRKLoss3").html(BRKLossName3);

                //$("#divBRKLoss3").css("width", BRKpercent3.toString() + "%");
                //$("#divBRKLoss3").attr("aria-valuenow", BRKpercent3);
                //$("#divBRKLoss3").html(BRKpercent3.toString() + " %");
                //$("#BRKLoss4").html(BRKLossName4);
                //$("#divBRKLoss4").css("width", BRKpercent4.toString() + "%");
                //$("#divBRKLoss4").attr("aria-valuenow", BRKpercent4);
                //$("#divBRKLoss4").html(BRKpercent4.toString() + " %");


            }

        });
    }

    $.get("/Dashboard/DayWiseOEE", {}, function (msg) {

        if(msg!='')
        {
            var data = JSON.parse(msg);
            //chart.datasets[0].data = data;
           
            chart.options.data[0] = data;
            chart.render();
        }
    })

    function getabnormality(losses) {
        var cssdata = "";
        $('.abnormality').html('');
        for (var i = 0; i < losses.length && i<4; i++) {
            var Duration=parseInt(losses[i].Duration);
            cssdata += '<h6 style="color:white;" >' + losses[i].LossName + '</h6><div class="progress" ><div class="progress-bar progress-bar-warning" role="progressbar" aria-valuenow="' + Duration + '" aria-valuemin="0" aria-valuemax="100"  style="width:' + Duration.toString() + '%">';
            cssdata += '' + Duration.toString() + ' %</div></div>';
        }
        $(cssdata).appendTo($('.abnormality'));

    }

    function getaBrkDown(losses) {
        var cssdata = "";
        $('.BRKDWNLOSSES').html('');
        for (var i = 0; i < losses.length && i < 4; i++) {
            var Duration = parseInt(losses[i].LossPercent);
            cssdata += '<h6 style="color:white;">' + losses[i].LossName + '</h6><div class="progress " ><div class="progress-bar progress-bar-warning" role="progressbar" aria-valuenow="' + Duration + '" aria-valuemin="0" aria-valuemax="100"  style="width:' + Duration.toString() + '%">';
            cssdata += '' + Duration.toString() + ' %</div></div>';
        }
        $(cssdata).appendTo($('.BRKDWNLOSSES'));

    }

    function getIDLE(losses) {
        var cssdata = "";
        $('.IDLELOSS').html('');
        for (var i = 0; i < losses.length && i < 4; i++) {
            var Duration = parseInt(losses[i].LossPercent);
            cssdata += '<h6 style="color:white;">' + losses[i].LossName + '</h6><div class="progress " ><div class="progress-bar progress-bar-warning" role="progressbar" aria-valuenow="' + Duration + '" aria-valuemin="0" aria-valuemax="100"  style="width:' + Duration.toString() + '%">';
            cssdata += '' + Duration.toString() + ' %</div></div>';
        }
        $(cssdata).appendTo($('.IDLELOSS'));

    }

    $.get("/Dashboard/GetPreviousMachineStatus", {}, function (msg) {
        if (msg != '') {
            var data = JSON.parse(msg);
            $('#divAvail').html(data.Availability + " %");
            $('#divQuality').html(data.Quality + " %");
            $('#divPerfrom').html(data.Performance + " %");
            $('#divOEE').html(data.OEE + " %");
            $('#divAvail').css("width", data.Availability + "%");
            $('#divQuality').css("width", data.Quality + "%");
            $('#divPerfrom').css("width", data.Performance + "%");
            $('#divOEE').css("width", data.OEE + "%");
        }

    });

    var chart2 = AmCharts.makeChart("chartdiv2", {
        "hideCredits":true,
        "theme": "light",
        "type": "gauge",
        "marginLeft": -22,
        "marginRight": -22,
        "axes": [{
            "topTextFontSize": 15,
            "topTextYOffset": 50,
            "axisColor": "#FFFF00",
            "axisThickness": 1,
            "endValue": 100,
            "gridInside": true,
            "inside": true,
            "radius": "50%",
            "valueInterval": 20,
            "tickColor": "#FFFF00",
            "startAngle": -90,
            "endAngle": 90,
            "unit": "%",
            "bandOutlineAlpha": 0,
            "bands": [{
                "color": "#FFFF00",
                "endValue": 100,
                "innerRadius": "105%",
                "radius": "170%",
                "gradientRatio": [0.5, 0, -0.5],
                "startValue": 0
            }, {
                "color": "#FFFF00",
                "endValue": 0,
                "innerRadius": "105%",
                "radius": "170%",
                "gradientRatio": [0.5, 0, -0.5],
                "startValue": 0
            }]
        }],
        "arrows": [{
            "alpha": 1,
            "innerRadius": "25%",
            "nailRadius": 0,
            "radius": "170%",
            "color":"#FFFF"
        }]
    });
   

});