function generateTimeSeriesWithEqualInterval(ratings, titles, timeAdd) {
    var seriesData = [];
    for (var i = 0; i < ratings.length; i++) {
        seriesData.push({
            x: i,
            y: ratings[i],
            title: titles[i], 
            time: timeAdd[i] 
        });
    }
    return seriesData;
}

var optionsAreaChart = {
    chart: {
        height: 320,
        type: "area",
        stacked: true,
        toolbar: { show: false },
        events: {
            selection: function (e, a) {
                console.log(new Date(a.xaxis.min));
                
            },
            click: function(event, chartContext, config) {
                var dataPoint = chartContext.w.config.series[config.seriesIndex].data[config.dataPointIndex];
                console.log("Clicked on point:", dataPoint);
                var title = dataPoint.title; 
                var url = '/MainPage/Index?title=' + encodeURIComponent(title);
                window.location.href = url;
            }
        },
    },
    colors: ["#3f51b5", "#CED4DC"],
    stroke: { width: [2], curve: "smooth" },
    fill: { type: "gradient", gradient: { opacityFrom: 0.3, opacityTo: 0.9 } },
    legend: { position: "top", horizontalAlign: "center" },
    xaxis: {
        type: "numeric",
    },
    yaxis: {
        title: {
            text: "grades",
            offsetX: -20,
            style: {
                color: undefined,
                fontSize: "13px",
                cssClass: "apexcharts-yaxis-title",
            },
        },
    },
    tooltip: {
        enabled: true,
        custom: function({ series, seriesIndex, dataPointIndex, w }) {
            var dataPoint = w.config.series[seriesIndex].data[dataPointIndex];
            return '<input value="" class="apexcharts-tooltip" style="display: block; background: #0a3622; position:relative; width: 100px; min-width: 200px; height: 10px; min-height: 10px; padding: 10px; border-radius: 5px;">' +
                '<div style="color: maroon; margin-bottom: 5px;">Title: <strong style="color: blueviolet;">' + dataPoint.title + '</strong></div>' +
                '<div>Date: <strong>' + dataPoint.time + '</strong></div>'+
                '<div>Grade: <strong>' + dataPoint.y + '</strong></div>' +
                '</input>';
        }
    },



};
$.ajax({
    url: '/Statistic/GetChartDataRating',
    type: 'GET',
    success: function (data1) {
        if (data1) {
            var newData = {
                myRating: generateTimeSeriesWithEqualInterval(data1.mygrade, data1.title,data1.timeAdd),
                overallRating: generateTimeSeriesWithEqualInterval(data1.movieNightGrade, data1.title,data1.timeAdd)
            };

            optionsAreaChart.series = [
                { name: "my rating", data: newData.myRating },
                { name: "overall rating", data: newData.overallRating }
            ];

            var chartArea = new ApexCharts(document.querySelector("#apex-area"), optionsAreaChart);
            chartArea.render(); 

        }
    },
    error: function () {
        
    }
});

var options = {
    chart: {
        height: 320,
        type: "area",
        stacked: true,
        toolbar: { show: false },
        events: {
            selection: function (e, a) {
                console.log(new Date(a.xaxis.min));
            },
        },
    },
    colors: ["#3f51b5", "#CED4DC"],
    dataLabels: { enabled: false },
    stroke: { width: [2], curve: "smooth" },
    fill: { type: "gradient", gradient: { opacityFrom: 0.3, opacityTo: 0.9 } },
    legend: { position: "top", horizontalAlign: "center" },
    xaxis: { type: "datetime" },
    yaxis: {
        title: {
            text: "Recent Signups",
            offsetX: -20,
            style: {
                color: undefined,
                fontSize: "13px",
                cssClass: "apexcharts-yaxis-title",
            },
        },
    },
};



var chart; 


options = {
    chart: { height: 320, type: "donut" },
    series: [44, 55, 41, 15],
    legend: {
        show: !0,
        position: "bottom",
        horizontalAlign: "center",
        verticalAlign: "middle",
        floating: !1,
        fontSize: "14px",
        offsetX: 0,
        offsetY: -10,
    },
    labels: ["Series 1", "Series 2", "Series 3", "Series 4"],
    colors: ["#3f51b5", "#009688", "#00bcd4", "#d1dee4"],
    responsive: [
        {
            breakpoint: 600,
            options: { chart: { height: 210 }, legend: { show: !1 } },
        },
    ],
};






(chart = new ApexCharts(
    document.querySelector("#apex-pie-2"),
    options,
)).render()
options = {
    chart: { height: 320, type: "donut" },
    series: [44, 55, 41, 15],
    legend: {
        show: !0,
        position: "bottom",
        horizontalAlign: "center",
        verticalAlign: "middle",
        floating: !1,
        fontSize: "14px",
        offsetX: 0,
        offsetY: -10,
    },
    labels: ["Series 1", "Series 2", "Series 3", "Series 4"],
    colors: ["#3f51b5", "#009688", "#00bcd4", "#d1dee4"],
    responsive: [
        {
            breakpoint: 600,
            options: { chart: { height: 210 }, legend: { show: !1 } },
        },
    ],
};
(chart = new ApexCharts(
    document.querySelector("#apex-pie-3"),
    options,
)).render()
   
