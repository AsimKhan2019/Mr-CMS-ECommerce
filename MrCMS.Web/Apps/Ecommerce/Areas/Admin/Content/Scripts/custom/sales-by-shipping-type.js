﻿$(document).ready(function () {

    $(window).resize(updateCharts);
    
    function updateCharts() {
        generateChart();
    };

    generateChart();
});

var chartData = {
    labels: [],
    datasets: [
        {
            fillColor: "rgba(52, 152, 219,0.5)",
            strokeColor: "rgba(52, 152, 219,1.0)",
            pointColor: "rgba(52, 152, 219,1.0)",
            pointStrokeColor: "rgba(52, 152, 219,1.0)",
            data: []
        },
        {
            fillColor: "rgba(43, 191, 105,0.5)",
            strokeColor: "rgba(43, 191, 105,1.0)",
            pointColor: "rgba(43, 191, 105,1.0)",
            pointStrokeColor: "rgba(43, 191, 105,1.0)",
            data: []
        },
        {
            fillColor: "rgba(231, 76, 60,0.5)",
            strokeColor: "rgba(231, 76, 60,1.0)",
            pointColor: "rgba(231, 76, 60,1.0)",
            pointStrokeColor: "rgba(231, 76, 60,1.0)",
            data: []
        }
    ]
};

var dataUrl = "/Admin/Apps/Ecommerce/Report/SalesByShippingType";
var chart = $("#chart").get(0).getContext("2d");
var chartContainer = $("#chart").parent();

function generateChart() {
    $.post(dataUrl, {
        from: $("#From").val(),
        to: $("#To").val()
    }, function (result) {
        chartData.labels = result.ChartLabels;
        $.each(result.MultiChartData, function (index, value) {
            chartData.datasets[index].data = value;
        });
        var nc = $("#chart").attr('width', $(chartContainer).width());
        new Chart(chart).Bar(chartData, lineChartOptions);
    });
};