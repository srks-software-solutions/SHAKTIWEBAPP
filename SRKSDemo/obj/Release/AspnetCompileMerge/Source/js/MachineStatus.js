function MachineChart(msg) {
    var Result = JSON.parse(msg);
    for (var i = 0; i < Result.length; i++) {
        var Mid = Result[i].MachineID;
        var IdleT = Result[i].IdleDuration;
        var ProdT = Result[i].ProductionDuration;
        var BreakdT = Result[i].BreakdownDuration;
        var PowerroffT = Result[i].PoweroffDuration;
        var TotalDuration = Result[i].TotalDuration;
        Highcharts.chart('StackChart_' + Mid, {
            chart: {
                type: 'bar'
            },
            title: {
                text: '',
                style: {
                    display: 'none'
                }
            },
            xAxis: {
                categories: ['Start']
            },
            yAxis: {
                //min: 1,
                max: TotalDuration,
                title: {
                    text: 'Duration In Minutes(' + TotalDuration + ' Min)'
                },
                labels: {
                    enabled: false
                }

            },
            legend: { enabled: false },
            plotOptions: {
                series: {
                    stacking: 'x'
                }
            },
            series: [{
                name: 'Production',
                data: [ProdT],
                color: "GREEN"
            },
            {
                name: 'Idle',
                data: [IdleT],
                color: "YELLOW"
            },
            {
                name: 'PowerOFF',
                data: [PowerroffT],
                color: "BLUE"
            },
            {
                name: 'BreakDown',
                data: [BreakdT],
                color: "RED"
            }


            ]
        });
    }
}