$(document).ready(function () {
    Highcharts.chart('StackChart', {
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
            min: 1,
            max: 480,
            title: {
                text: 'Duration In Minutes(480 Min)'
            },
            labels: {
                enabled: false
            }
            
        },        
        legend: { enabled: false },
        //legend: {
        //    //reversed: false,
        //    display: false
        //},
        plotOptions: {
            series: {
                stacking: 'x'
            }
        },
        series: [{
            name: 'Idle',
            data: [8],
            color: "YELLOW"
        }, {
                name: 'PowerOFF',
                data: [5],
                color: "BLUE"
            },
            {
            name: 'Production',
            data: [15],
            color: "GREEN"
        }, {
            name: 'BreakDown',
            data: [10],
            color: "RED"
        },
           {
            name: 'PowerOFF',
            data: [5],
            color: "BLUE"
        },
            {
                name: 'BreakDown',
                data: [25],
                color: "RED"
        },
            {
                name: 'PowerOFF',
                data: [5],
                color: "BLUE"
            },
        ]
    });
});