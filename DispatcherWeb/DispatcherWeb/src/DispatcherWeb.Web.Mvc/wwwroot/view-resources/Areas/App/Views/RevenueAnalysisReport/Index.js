(function () {
    'use strict';

    var _dtHelper = abp.helper.dataTables;
    var _currencySymbol = _dtHelper.renderText(abp.setting.get('App.General.CurrencySymbol'));
    var _revenueAnalysisReportService = abp.services.app.revenueAnalysisReport;
    var _dashboardService = abp.services.app.dashboard;
    var _chart = null;
    var _chartData = null;


    $('#DeliveryDateFilter').val(moment().format('MM/DD/YYYY - MM/DD/YYYY'));
    $("#DeliveryDateFilter").daterangepicker({
        //autoUpdateInput: false,
        locale: {
            cancelLabel: 'Clear'
        },
        showDropDown: true
    }).on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
    }).on('cancel.daterangepicker', function (ev, picker) {
        $(this).val('');
    });

    $('#AnalyzeBy').select2Init({
        showAll: true,
        allowClear: false
    });

    $('#OfficeIdFilter').select2Init({
        abpServiceMethod: listCacheSelectLists.office(),
        showAll: true,
        allowClear: true
    });

    $('#HasLeaseHaulerIdFilter').select2Init({
        showAll: true,
        allowClear: true
    });

    $("#ViewButton").click(function () {
        initRevenueChart();
    });

    var revenueAnalysisShowAllLink = $("#revenueAnalysisShowAllLink");
    revenueAnalysisShowAllLink.click(function (e) {
        e.preventDefault();
        $(this).hide();
        if (_chart && _chartData) {
            _chart.draw(_chartData);
        }
    });

    function getTicketLhFilterType(hasLeaseHaulerId) {
        switch (hasLeaseHaulerId) {
            default:
            case '': return abp.enums.ticketType.both;
            case 'true': return abp.enums.ticketType.leaseHaulers;
            case 'false': return abp.enums.ticketType.internalTrucks;
        }
    }

    async function initRevenueChart() {
        let analyzeBy = Number($("#AnalyzeBy").val());
        let officeId = $("#OfficeIdFilter").val();
        let hasLeaseHaulerId = $("#HasLeaseHaulerIdFilter").val();
        let container = $("#revenueAnalysisContainer");
        let loadingContainer = $("#revenueAnalysisLoading");
        let ticketType = getTicketLhFilterType(hasLeaseHaulerId);
        revenueAnalysisShowAllLink.hide();

        if (analyzeBy === abp.enums.analyzeRevenueBy.date) {
            _chart = await abp.helper.graphs.initBarChart({
                container: container,
                loadingContainer: loadingContainer,
                getDataAsync: async function () {
                    let input = {
                        datePeriod: abp.enums.revenueGraphDatePeriod.daily,
                        officeId,
                        hasLeaseHaulerId,
                        ticketType,
                    };
                    $.extend(input, _dtHelper.getDateRangeObject($("#DeliveryDateFilter").val(), 'periodBegin', 'periodEnd'));
                    let result = await _dashboardService.getRevenueByDateGraphData(input);
                    return result.revenueGraphData;
                },
                barOptions: {
                    xkey: 'period',
                    ykeys: ['freightRevenueValue', 'materialRevenueValue', 'fuelSurchargeValue'],
                    labels: ['Freight Revenue', 'Material Revenue', 'Fuel Surcharge'],
                    barColors: [app.colors.freight, app.colors.material, app.colors.fuel],
                    stacked: true,
                    preUnits: _currencySymbol,
                    hoverCallback: function (index, barOptions, content, row) {
                        let finalContent = $(content);
                        let formattedRevenueValue = row.revenueValue.toLocaleString('en-US');
                        return finalContent.add($('<div class="text-body">Total: ' + barOptions.preUnits + formattedRevenueValue + '</div>'));
                    }
                }
            });
        } else {
            _chart = await abp.helper.graphs.initBarChart({
                container: container,
                loadingContainer: loadingContainer,
                getDataAsync: async function () {
                    let input = {
                        analyzeBy,
                        officeId,
                        hasLeaseHaulerId,
                    };
                    $.extend(input, _dtHelper.getDateRangeObject($("#DeliveryDateFilter").val(), 'deliveryDateBegin', 'deliveryDateEnd'));
                    let result = await _revenueAnalysisReportService.getRevenueAnalysis(input);
                    _chartData = result.revenueAnalysisGraphData;
                    if (analyzeBy === abp.enums.analyzeRevenueBy.customer && _chartData.length > 20) {
                        revenueAnalysisShowAllLink.show();
                        return _chartData.slice(0, 20);
                    }
                    return _chartData;
                },
                barOptions: {
                    xkey: 'analysisBy',
                    ykeys: ['freightRevenueValue', 'materialRevenueValue', 'fuelSurchargeValue'],
                    maxXLabelLength: 18,
                    labels: ['Freight Revenue', 'Material Revenue', 'Fuel Surcharge'],
                    barColors: [app.colors.freight, app.colors.material, app.colors.fuel],
                    horizontal: true,
                    stacked: true,
                    preUnits: _currencySymbol,
                    xLabelAngle: 45,
                    hoverCallback: function (index, barOptions, content, row) {
                        let finalContent = $(content);
                        let formattedRevenueValue = row.revenueValue.toLocaleString('en-US');
                        return finalContent.add($('<div class="text-body">Total: ' + barOptions.preUnits + formattedRevenueValue + '</div>'));
                    }
                }
            });
        }
    }

})();
