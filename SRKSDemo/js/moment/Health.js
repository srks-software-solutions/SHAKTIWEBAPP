$(document).ready(function () {
    var ChartDet1 = GetChartDet('chartdiv1');
    var ChartDet2 = GetChartDet('chartdiv2');
    var ChartDet3 = GetChartDet('chartdiv3');
    GetData();

    setInterval(GetData, 3000);
    function GetChartDet(divid) {
        var chart = AmCharts.makeChart(divid, {
            "type": "serial",
            "theme": "light",
            "marginTop": 0,
            "marginRight": 0,

            "valueAxes": [{
                "minimum": 0,
                "maximum": 0,
                "autoGridCount": false,
                "gridCount": 10,
                "axisAlpha": 0,
                "title": "",
                "position": "left",

                "guides": [{
                    "value": 10,
                    "lineAlpha": 1,
                    "lineColor": "#880000"
                }, {
                    "value": -10,
                    "lineAlpha": 1,
                    "lineColor": "#880088"
                }],
                "zeroGridAlpha": 0
            }],
            "graphs": [{
                "balloonText": "<div style='margin:5px; font-size:19px;'><span style='font-size:13px;'>[[category]]</span><br>[[value]]</div>",
                "bullet": "round",
                "bulletSize": 8,
                "bulletBorderAlpha": 0,
                "lineThickness": 2,
                "negativeLineColor": "#FFC107",
                "negativeBase": 0,
                "type": "smoothedLine",
                "valueField": "value"
            }, {
                "showBalloon": false,
                "bullet": "round",
                "bulletBorderAlpha": 0,
                "hideBulletsCount": 50,
                "lineColor": "transparent",
                "negativeLineColor": "#D50000",
                "negativeBase": 0,
                "type": "smoothedLine",
                "valueField": "value"
            }],
            "chartScrollbar": {
                "graph": "g1",
                "gridAlpha": 0,
                "color": "#888888",
                "scrollbarHeight": 55,
                "backgroundAlpha": 0,
                "selectedBackgroundAlpha": 0.1,
                "selectedBackgroundColor": "#888888",
                "graphFillAlpha": 0,
                "autoGridCount": true,
                "selectedGraphFillAlpha": 0,
                "graphLineAlpha": 0.2,
                "graphLineColor": "#c2c2c2",
                "selectedGraphLineColor": "#888888",
                "selectedGraphLineAlpha": 1

            },
            "chartCursor": {
                "categoryBalloonDateFormat": "JJ:NN:SS",
                "cursorAlpha": 0,
                "valueLineEnabled": false,
                "valueLineBalloonEnabled": false,
                "title": "Time",
                "valueLineAlpha": 0.5,
                "fullWidth": true
            },
            "dataDateFormat": "HH:NN:SS",
            "categoryField": "Time",
            "categoryAxis": {
                "minPeriod": "ss",
                "title": "Time",
                "parseDates": true,
                "minorGridAlpha": 0.1,
                "minorGridEnabled": true
            },

            "titles": [
                {
                    "id": "Title-1",
                    "size": 15,
                    "text": ""
                }
            ]
        });
        return chart;
    }

    function zoomChart(chardets) {
        chardets.zoomToIndexes(Math.round(chardets.dataProvider.length * 0.4), Math.round(chardets.dataProvider.length * 0.5));
    }
    function GetData() {
        var parameter = $("#SMID_parameters1").val();
        if (parameter !== 0) {
            Getparameters1(parameter);
        }
        var parameter1 = $("#SMID_parameters2").val();
        if (parameter !== 0) {
            Getparameters2(parameter1);
        }
        var parameter2 = $("#SMID_parameters3").val();
        if (parameter != 0) {
            Getparameters3(parameter2);
        }
    }

    function Getparameters1(parameterval) {
        $.get("/MachineHealth/GetParameters", { id: parameterval }, function (msg) {

            data2 = msg;
            if (msg !== '') {
                var data = JSON.parse(msg);
                ChartDet1.dataProvider = data.MachineHealthdet;
                // chart.dataProvider = data2;
                ChartDet1.validateData();
                ChartDet1.valueAxes[0].guides[0].value = data.LSL;
                ChartDet1.graphs[0].negativeBase = data.LSL;
                ChartDet1.graphs[1].negativeBase = data.USL;
                ChartDet1.validateData();
                ChartDet1.valueAxes[0].guides[1].value = data.USL;
                ChartDet1.validateData();
                ChartDet1.valueAxes[0].minimum = data.min;
                ChartDet1.valueAxes[0].maximum = data.max;
                ChartDet1.valueAxes[0].title = data.unit;
                ChartDet1.validateData();
                ChartDet1.addListener("rendered", zoomChart(ChartDet1));
            }

        });
    }

    $("#SMID_parameters1").on("change", function () {
        var parameter = $("#SMID_parameters1").val();
        Getparameters1(parameter);
    });
    function Getparameters2(parameterval1) {

        $.get("/MachineHealth/GetParameters", { id: parameterval1 }, function (msg) {

            data2 = msg;
            if (msg !== '') {
                var data = JSON.parse(msg);
                ChartDet2.dataProvider = data.MachineHealthdet;
                // chart.dataProvider = data2;
                ChartDet2.validateData();
                ChartDet2.valueAxes[0].guides[0].value = data.LSL;
                ChartDet2.valueAxes[0].guides[1].value = data.USL;
                ChartDet2.graphs[0].negativeBase = data.LSL;
                ChartDet2.graphs[1].negativeBase = data.USL;
                ChartDet2.valueAxes[0].minimum = data.min;
                ChartDet2.valueAxes[0].maximum = data.max;
                ChartDet2.valueAxes[0].title = data.unit;
                ChartDet2.validateData();
                ChartDet2.addListener("rendered", zoomChart(ChartDet2));
            }

        });

    }
    $("#SMID_parameters2").on("change", function () {
        var parameter = $("#SMID_parameters2").val();
        Getparameters2(parameter);
    });
    function Getparameters3(parameterval2) {
        $.get("/MachineHealth/GetParameters", { id: parameterval2 }, function (msg) {

            data2 = msg;
            if (msg !== '') {
                var data = JSON.parse(msg);
                ChartDet3.dataProvider = data.MachineHealthdet;
                // chart.dataProvider = data2;
                ChartDet3.validateData();
                ChartDet3.valueAxes[0].guides[0].value = data.LSL;
                ChartDet3.valueAxes[0].guides[1].value = data.USL;
                ChartDet3.graphs[0].negativeBase = data.LSL;
                ChartDet3.graphs[1].negativeBase = data.USL;
                ChartDet3.valueAxes[0].minimum = data.min;
                ChartDet3.valueAxes[0].maximum = data.max;
                ChartDet3.valueAxes[0].title = data.unit;
                ChartDet3.validateData();
                ChartDet3.addListener("rendered", zoomChart(ChartDet3));
            }

        });
    }

    $("#SMID_parameters3").on("change", function () {
        var parameter = $("#SMID_parameters3").val();
        Getparameters3(parameter);
    });

    $("#shop").empty();
    $("#shop").append("<option value=''> Select Shop </option>");
    $("#SMID_parameters1").empty();
    $("#SMID_parameters1").append("<option value=''> Select Parameter Name </option>");
    $("#SMID_parameters2").empty();
    $("#SMID_parameters2").append("<option value=''> Select Parameter Name </option>");
    $("#SMID_parameters3").empty();
    $("#SMID_parameters3").append("<option value=''> Select Parameter Name </option>");
    $("#PlantID").on("change", function (e) {
        var PID = $(this).val();
        $.getJSON("/MachineHealth/FetchShop", { PID: PID }, function (res) {
            $("#shop").empty();
            $("#shop").append("<option value = ''> Select Shop </option>");

            $("#cell").empty();
            $("#cell").append("<option value = ''> Select Cell </option>");

            $("#Machine").empty();
            $("#Machine").append("<option value = ''> Select Machine </option>");

            $.each(res, function (index, item) {
                $("#shop").append("<option value = '" + item.Value + "'>" + item.Text + "</option>");
            });
        });
    });

    $("#cell").empty();
    $("#cell").append("<option value = ''> Select Cell </option>");
    $("#shop").on("change", function (e) {
        var SID = $(this).val();
        $.getJSON("/MachineHealth/Fetchcell", { SID: SID }, function (data) {
            $("#cell").empty();
            $("#cell").append("<option value = ''> Select Cell </option>");

            $("#Machine").empty();
            $("#Machine").append("<option value = ''> Select Machine </option>");
            $.each(data, function (index, item) {
                $("#cell").append("<option value = '" + item.Value + "'>" + item.Text + "</option>");
            });
        });
    });

    $("#Machine").empty();
    $("#Machine").append("<option value = ''> Select Machine </option>");

    $("#cell").on("change", function (e) {
        $("#Machine").empty();
        $("#Machine").append("<option value = ''> Select Machine </option>");
        var CID = $(this).val();
        $.getJSON("/MachineHealth/FetchMachine", { CID: CID }, function (data) {
            $("#Machine").empty();
            $("#Machine").append("<option value = ''> Select Machine </option>");
            $.each(data, function (index, item) {
                $("#Machine").append("<option value = '" + item.Value + "'>" + item.Text + "</option>");
            });
        });
    });

    $("#Machine").on("change", function (e) {
        var MID = $(this).val();
        $.getJSON("/MachineHealth/Fetchsensor", { MID: MID }, function (data) {
            $("#SMID_parameters1").empty();
            $("#SMID_parameters1").append("<option value = ''> Select Parameter Name </option>");
            $("#SMID_parameters2").empty();
            $("#SMID_parameters2").append("<option value=''> Select Parameter Name </option>");
            $("#SMID_parameters3").empty();
            $("#SMID_parameters3").append("<option value=''> Select Parameter Name </option>");
            $.each(data, function (index, item) {
                $("#SMID_parameters1").append("<option value = '" + item.Value + "'>" + item.Text + "</option>");
                $("#SMID_parameters2").append("<option value = '" + item.Value + "'>" + item.Text + "</option>");
                $("#SMID_parameters3").append("<option value = '" + item.Value + "'>" + item.Text + "</option>");

            });
        });

    });

});