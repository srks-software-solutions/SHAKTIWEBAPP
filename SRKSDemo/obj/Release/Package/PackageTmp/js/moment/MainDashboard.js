$(document).ready(function () {
    
    MachineUtiliztion();
    var oeeData = [];
    var oeeData1 = [];
    var oeeData2 = [];
    var oeeData3 = [];
    //var targetData = [];
    //var actualData = [];
    var msg='';
    var contributingFactor = [];
    var contributingFactorData = [];
    var timeval = '';
    var datetimeval = '';
    var currentDate = new Date();
    var day = currentDate.getDate();
    var month = currentDate.getMonth() + 1;
    var year = currentDate.getFullYear();
    datetimeval = day + "-" + month + "-" + year;
    LossContributingFactor();

    //setTimeout(pagereload, 5 * 60 * 1000);   //5min

    function pagereload() {
        window.location.reload(true);
    }

    function checkTime(i) {
        return (i < 10) ? "0" + i : i;
    }

    function startTime() {
        var today = new Date(),
            h = checkTime(today.getHours()),
            m = checkTime(today.getMinutes()),
            s = checkTime(today.getSeconds());
        timeval = h + ":" + m + ":" + s;
        t = setTimeout(function () {
            startTime()
        }, 500);
    }

    function MachineUtiliztion() {
        $.get("/Dashboard/GetMachineUtilization", {}, function (msg) {
            if (msg != '') {
                var data = JSON.parse(msg);
                var machineUtilization = data.MachineUtilizationModels;
                var Alarmsdet = data.AlarmLists;
                var cssdata = '';
                startTime();
                $("#divUtilization").html('');
                $("#DateTimeVal").html(datetimeval + ' ' + timeval);
              
                for (var i = 0; i < machineUtilization.length; i++) {
                    cssdata += '<div class="col-sm-1 dash-new-padding"><div class="dash-new-bg">';
                    cssdata += '<div class="dash-new-title"> <label>' + machineUtilization[i].MachineName + '</label></div>';
                    cssdata += '<div class="dash-new-body"> <label>' + machineUtilization[i].MachineUtiization + '</label></div></div></div>';
                   // cssdata += '<div class="dash-new-footer"> <label>' + datetimeval + ' ' + timeval + '</label></div>';
                }
                $(cssdata).appendTo($("#divUtilization"));

                OEE();
                getAlarms(Alarmsdet);
            }

        });

    }

    function getAlarms(Alarmsdet) {
        $('#alrmdet').html('');
        var cssdata = '';
        if (Alarmsdet != '') {
            for (var i = 0; i < Alarmsdet.length; i++) {
                var num = i + 1;
                cssdata += '<tr><td>' + num + '</td>';
                cssdata += '<td><label>' + Alarmsdet[i].MachineID + '</label></td>';
                cssdata += '<td><label>' + Alarmsdet[i].AlarmNumber + '</label></td>';
                cssdata += '<td><label>' + Alarmsdet[i].AlarmMessage + '</label></td>';
                cssdata += '<td><label>' + Alarmsdet[i].AxisNumber + '</label></td>';
                cssdata += '<td><label>' + Alarmsdet[i].AlarmDateTime + '</label></td></tr>';
            }
        }
        else
            cssdata += '<tr><td colspan="6" style="color:red">No records found </td>';
        $(cssdata).appendTo($('#alrmdet'));
    }

    function GetAlarmsbyId(cellid) {
        $.get("/Dashboard/GetAlarmsById", { cellid }, function (msg) {
            $('#alrmdet').html('');
            var data = JSON.parse(msg);
            var cssdata = '';
            if (msg != '') {
                //var data = data.AlarmLists;
                for (var i = 0; i < data.length; i++) {
                    var num = i + 1;
                    cssdata += '<tr><td>' + num + '</td>';
                    cssdata += '<td><label>' + data[i].MachineID + '</label></td>';
                    cssdata += '<td><label>' + data[i].AlarmNumber + '</label></td>';
                    cssdata += '<td><label>' + data[i].AlarmMessage + '</label></td>';
                    cssdata += '<td><label>' + data[i].AxisNumber + '</label></td>';
                    cssdata += '<td><label>' + data[i].AlarmDateTime + '</label></td></tr>';
                }
            }
            else
                cssdata += '<tr><td colspan="6" style="color:red">No records found </td>';
            $(cssdata).appendTo($('#alrmdet'));
        });
    }

    $(document).on('click', '.clscell', function (e) {
        var cellid = this.id;
        var cellName = this.text;
        cellid = cellid.split('_')[1];
        $("#CellNames").html(cellName);
        //TargetActual(cellid);
        //  LineChat(cellid);
        GetActualPareto(cellid);
        GetAlarmsbyId(cellid);
    });

    //var ctx = document.getElementById("myChart").getContext('2d');
 //   var ctx1 = document.getElementById("myChart1").getContext('2d');
   var ctx2 = document.getElementById("myChart2").getContext('2d');
    //var myChart = new Chart(ctx, {
    //    type: 'bar',
    //    data: {
           
    //        labels: ["Availability", "Perfomance", "Quality", "OEE"],
    //        datasets: []
    //    },
    //    options: {
    //        responsive: true,
    //        maintainAspectRatio: true,
    //        legend: {
    //            position: 'bottom',
    //            display: true,
    //            backgroundColor: 'rgba(254,192,131,1)'
    //        },
    //        tooltips: {
    //            bodySpacing: 4,
    //            mode: "nearest",
    //            intersect: false,
    //            position: "nearest",
    //            xPadding: 10,
    //            yPadding: 10,
    //            caretPadding: 10
    //        },
    //        scales: {
    //            xAxes: [{
    //                stacked: false,
    //            }],
    //            yAxes: [{
    //                stacked: false,
    //                ticks: {
    //                    beginAtZero: true
    //                }
    //            }]
    //        }
    //    }

    //});

    //var myChart1 = new Chart(ctx1, {
    //    type: 'bar',

    //    data: {
    //        //labels: ["6:00-7:00", "7:00-8:00", "8:00-9:00", "9:00-10:00", "10:00-11:00", "11:00-12:00", "12:00-13:00", "13:00-14:00"],
    //        datasets: [{
    //            label: '# Target',

    //            fillColor: "rgba(220,220,220,0.5)",
    //            backgroundColor: "rgba(46, 44, 211, 0.7)",
    //            highlightFill: "rgba(220,220,220,0.75)",
    //            highlightStroke: "rgba(220,220,220,1)"

    //        },
    //        {
    //            label: '# Actual',
    //            fillColor: "rgba(0,0,0,0.5)",
    //            backgroundColor: "rgba(215, 44, 44, 0.7)",
    //            highlightFill: "rgba(0,0,0,0.5)",
    //            highlightStroke: "rgba(0,0,0,0.5)"
    //        }]
    //    },
    //    options: {
    //        responsive: true,
    //        maintainAspectRatio: true,
    //        legend: {
    //            position: 'bottom'
    //        },
    //        tooltips: {
    //            bodySpacing: 4,
    //            mode: "nearest",
    //            intersect: 0,
    //            position: "nearest",
    //            xPadding: 10,
    //            yPadding: 10,
    //            caretPadding: 10
    //        },
    //        scales: {
    //            xAxes: [{
    //                stacked: false,
    //            }],
    //            yAxes: [{
    //                stacked: false,
    //                ticks: {
    //                    beginAtZero: true
    //                }
    //            }]
    //        },
    //        options: {
    //            responsive: true,
    //            maintainAspectRatio: true,
    //            legend: {
    //                position: 'bottom'
    //            },
    //            tooltips: {
    //                bodySpacing: 4,
    //                mode: "nearest",
    //                intersect: 0,
    //                position: "nearest",
    //                xPadding: 10,
    //                yPadding: 10,
    //                caretPadding: 10
    //            },
    //            scales: {
    //                xAxes: [{
    //                    stacked: false,
    //                }],
    //                yAxes: [{
    //                    stacked: false,
    //                    ticks: {
    //                        beginAtZero: true
    //                    }
    //                }]
    //            }
    //        }
    //    }
    //});

    var myChart2 = new Chart(ctx2, {
        type: 'bar',
        data: {
            labels: [],
            datasets: []
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            legend: {
                position: 'bottom',
                display: true
            },
            tooltips: {
                bodySpacing: 4,
                mode: "nearest",
                intersect: false,
                position: "nearest",
                xPadding: 10,
                yPadding: 10,
                caretPadding: 10
            },
            scales: {
                xAxes: [{
                    stacked: false,
                }],
                yAxes: [{
                    stacked: false,
                    ticks: {
                        beginAtZero: true
                    }
                }]
            }
        }

    });
    function OEE() {
        $('#loading').css('display', 'block');
        $.get('/Dashboard/OEEs', {}, function (msg) {
            //var data1 = myChart;
            var OEEdata = JSON.parse(msg);
            if (OEEdata !== '') {
                for (var i = 0; i < OEEdata.length; i++) {
                    var data = '<tr class="child"><td class="dash-news-border"><a class="clscell" href="#" id="cellid_' + OEEdata[i].CellID + '">' + OEEdata[i].CellName + '</a></td>';//<td>' + OEEdata[i].Target + '</td><td>' + OEEdata[i].Actual + '</td>';
                    data += '<td>' + OEEdata[i].data[3] + '</td><td>' + OEEdata[i].data[0] + '</td>';
                    data += '<td>' + OEEdata[i].data[1] + '</td><td>' + OEEdata[i].data[2] + '</td></tr>';
                    $('#oeeTable').append(data);
                    var cellid = OEEdata[OEEdata.length - 1].CellID;
                    // GetTargetActual(cellid);
                    $("#CellNames").html(OEEdata[OEEdata.length - 1].CellName);
                    $(".CellDisplay").html(OEEdata[OEEdata.length - 1].CellName);
                    if (i === 0) {
                       // GetActualPareto(cellid);
                        //GetLosses(cellid);
                    }
                    //LineChat(cellid);
                    //myChart.data.datasets[i] = OEEdata[i];
                    //myChart.data.datasets[i].label = OEEdata[i].CellName;
                    //myChart.data.datasets[i].indexLabel= '{label}{y}';
                    ////myChart.data.data[0].indexLabel = "{label} {y}"
                    //myChart.data.datasets[i].label.backgroundColor = 'rgba(151,187,205,0.5)';
                    //myChart.update();
                    $('#loading').css('display', 'none');
                }
            }
        });
    }



    //function GetTargetActual(cellID) {
    //    var cellid = cellID;

    //    targetData = [];
    //    actualData = [];
    //    $.get("/Dashboard/TargetAcualsDet", { cellid }, function (msg) {
    //        var data = JSON.parse(msg);
    //        if (data !== '') {
    //            for (var i = 0; i < data.length; i++) {
    //                $("#target").text(data[i].Target);
    //                $("#actual").text(data[i].Actual);
    //                targetData = data[i].Target;
    //                actualData = data[i].Actual;
    //                myChart1.data.labels = data[i].Timings;
    //                // myChart1.title = data[i].CellName;
    //                $("#CellNames").html(data[i].CellName);
    //            }
    //            myChart1.data.datasets[0].data = targetData;
    //            myChart1.data.datasets[1].data = actualData;

    //            myChart1.update();
    //        }
    //    });
    //}

    //function TargetActual() {
    //    $.get("/Dashboard/GetTarget_Actual", {}, function (msg) {
    //        var data = JSON.parse(msg);
    //        if (data !== '') {
    //            for (var i = 0; i < data.length; i++) {
    //                $("#target").text(data[i].Target);
    //                $("#actual").text(data[i].Actual);
    //                targetData=data[i].Target;
    //                actualData = data[i].Actual;
    //                myChart1.data.labels = data[i].Timings;
    //               // myChart1.title = data[i].CellName;

    //            }
    //            myChart1.data.datasets[0].data = targetData;
    //            myChart1.data.datasets[1].data = actualData;

    //            myChart1.update();
    //        }
    //    });
    //}

    //Commented By Ashwini

    //function TargetActual(cellID) {
    //    var cellid = cellID;

    //    targetData = [];
    //    actualData = [];
    //    $.post("/Dashboard/GetTarget_Actual", { cellid }, function (msg) {
    //        var data = JSON.parse(msg);
    //        if (data !== '') {
    //            for (var i = 0; i < data.length; i++) {
    //                //$("#target").text(data[i].Target);
    //                $("#actual").text(data[i].Actual);
    //                targetData = data[i].Target;
    //                actualData = data[i].Actual;
    //                myChart1.data.labels = data[i].Timings;
    //                // myChart1.title = data[i].CellName;
    //                $("#CellNames").html(data[i].CellName);
    //            }
    //            myChart1.data.datasets[0].data = targetData;
    //            myChart1.data.datasets[1].data = actualData;

    //            myChart1.update();
    //        }
    //    });
    //}

    function LossContributingFactor() {
        $('#loading').css('display', 'block');
        $.get('/Dashboard/ContributingFactorLosses', {}, function (msg) {
            var lossdata = JSON.parse(msg);
            if (lossdata !== '') {
                for (var i = 0; i < lossdata.length; i++) {
                    var Cellname = "";
                    myChart2.data.datasets[i] = lossdata[i];
                    if (lossdata[i].CellName != null) {
                        Cellname = lossdata[i].CellName;
                    }
                    myChart2.data.datasets[i].label = Cellname;
                    myChart2.data.labels = lossdata[i].LossName;

                    myChart2.update();
                    $('#loading').css('display', 'none');
                }
                //myChart2.data.labels = contributingFactor;
                //myChart2.data.datasets[0].data = contributingFactorData;
                //myChart2.update();
            }
        });
    }

    //function LineChat(cellID) {
    //    var cellid = cellID;

    //    targetData = [];
    //    actualData = [];
    //    $.get("/Dashboard/GetTarget_Actual_Line", { cellid }, function (msg) {
    //        var data = JSON.parse(msg);

    //        chart.options.data = JSON.parse(msg);
    //        chart.render();
    //    });
    //}

    //var chart = new CanvasJS.Chart("chartContainer",
    //       {

    //           title: {
    //               text: "Target vs Actual"
    //           },
    //           axisX: {
    //               valueFormatString: "MMM",
    //               interval: 1,
    //               intervalType: "month"
    //           },
    //           axisY: {
    //               includeZero: false

    //           },
               //data: [
               //{
               //    type: "line",

               //    dataPoints: [
               //    { x: new Date(2012, 00, 1), y: 450 },
               //    { x: new Date(2012, 01, 1), y: 414 },
               //      { x: new Date(2012, 02, 1), y: 520, indexLabel: "highest", markerColor: "red", markerType: "triangle" },
               //    { x: new Date(2012, 03, 1), y: 460 },
               //    { x: new Date(2012, 04, 1), y: 450 },
               //    { x: new Date(2012, 05, 1), y: 500 },
               //    { x: new Date(2012, 06, 1), y: 480 },
               //    { x: new Date(2012, 07, 1), y: 480 },
               //    { x: new Date(2012, 08, 1), y: 410, indexLabel: "lowest", markerColor: "DarkSlateGrey", markerType: "cross" },
               //    { x: new Date(2012, 09, 1), y: 500 },
               //    { x: new Date(2012, 10, 1), y: 480 },
               //    { x: new Date(2012, 11, 1), y: 510 }
               //    ]
               //}
    //           //]
    //       });

    //chart.render();



    //var chart1 = new CanvasJS.Chart("chartContainer", {
    //    title: {
    //        text: "Plan vs Actual"
    //    },
        
    //    axisY: {
    //        title: "Quantity",
    //        lineColor: "#4F81BC",
    //        tickColor: "#4F81BC",
    //        labelFontColor: "#4F81BC",

    //    },
      
    //    axisY2: {
    //        title: "Actual Quantity",
    //        lineColor: "#C0504E",
    //        tickColor: "#C0504E",
    //        labelFontColor: "#C0504E"
    //    },
    //    toolTip: {
    //        shared: true
    //    },
    //    legend: {
    //        cursor: "pointer",
    //        itemclick: toggleDataSeries
    //    },
       
    //});
    //chart1.render();
   
    function toggleDataSeries(e) {
        if (typeof (e.dataSeries.visible) === "undefined" || e.dataSeries.visible) {
            e.dataSeries.visible = false;
        } else {
            e.dataSeries.visible = true;
        }
        e.chart.render();
    }





    //function GetActualPareto(cellID) {
    //    var cellid = cellID;
    //    var dps = [];
    //    $.get("/Dashboard/GetTarget_Actual_Data", { cellid }, function (msg) {

    //        var data = JSON.parse(msg);
    //        var target = data[0].dataPointsTarget;
    //        chart1.options.data = data;          
    //        chart1.render();                    
    //        createPareto1(target);
    //        createPareto(target);
    //    });
    //}


    //function createPareto(target) {
    //    var dps = [];
    //    var yValue, yTotal = 0, yAdd = 0;
    //    var count = target.length;
    //    count = (count + 1) * 18;
    //    for (var j = 0; j < target.length; j++)
    //        yTotal += target[j].y;

    //    for (var i = 0; i < target.length; i++) {
    //        yValue = target[i].y;
    //        yAdd = yValue + yAdd;
    //        dps.push({ label: chart1.data[0].dataPoints[i].label, y: yAdd });
    //    }
       
    //    chart1.addTo("data", { type: "spline", name: "Actual Cummulative", indexLabel: "{y}", indexLabelFontColor: "#C24642", showInLegend: true, dataPoints: dps });
    //    chart1.data[2].set("axisYType", "secondary", false);
    //    chart1.axisY[0].set("maximum", yTotal);
    //    chart1.axisY2[0].set("maximum", count);
    //}

    //function createPareto1(target) {
       
    //    chart1.addTo("data", { type: "column", name: "Actual", indexLabel: "{y}", showInLegend: true, dataPoints: target });
    //    chart1.data[1].set("axisYType", "primary", false);
    //    //chart1.axisY[0].set("maximum", count);
    //    //chart1.axisY2[0].set("maximum", yTotal);
    //}



    //function GetLosses(cellID) {
    //    var cellid = cellID;
    //    var dps = [];
    //    $.get("/Dashboard/ContributingFactorLossesByCell", { cellid }, function (msg) {

    //        var data = JSON.parse(msg);           
    //        chartLoss.options.data = data;
    //        chartLoss.render();
            
    //    });
    //}

    var chartLoss = new CanvasJS.Chart("chartContainer2", {
        title: {
            text: "Losses"
        },
        axisY: {
            title: "Duration in Hours",
            lineColor: "#4F81BC",
            tickColor: "#4F81BC",
            labelFontColor: "#4F81BC"
        },
        //axisY2: {
        //    title: "Percent",
        //    suffix: "%",
        //    lineColor: "#C0504E",
        //    tickColor: "#C0504E",
        //    labelFontColor: "#C0504E"
        //},
        data: [{
            type: "column",
            dataPoints: []
               
            

        }]
    });
   // chart.render();
  //  createPareto();

    //function createPareto() {
    //    var dps = [];
    //    var yValue, yTotal = 0, yPercent = 0;

    //    for (var i = 0; i < chart.data[0].dataPoints.length; i++)
    //        yTotal += chart.data[0].dataPoints[i].y;

    //    for (var i = 0; i < chart.data[0].dataPoints.length; i++) {
    //        yValue = chart.data[0].dataPoints[i].y;
    //        yPercent += (yValue / yTotal * 100);
    //        dps.push({ label: chart.data[0].dataPoints[i].label, y: yPercent });
    //    }

    //    chart.addTo("data", { type: "line", yValueFormatString: "0.##\"%\"", dataPoints: dps });
    //    chart.data[2].set("axisYType", "secondary", false);
    //    chart.axisY[0].set("maximum", yTotal);
    //    chart.axisY2[0].set("maximum", 100);
    //}

});