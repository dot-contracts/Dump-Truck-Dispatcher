(function () {
    $(function () {
        "use strict";

        var _dtHelper = abp.helper.dataTables;
        var _colors = app.colors;
        var _dashboardService = abp.services.app.dashboard;
        var _currencySymbol = _dtHelper.renderText(abp.setting.get('App.General.CurrencySymbol'));

        var _editDashboardSettingsModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Dashboard/EditDashboardSettingsModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Dashboard/_EditDashboardSettingsModal.js',
            modalClass: 'EditDashboardSettingsModal'
        });

        $('#btnModifyDashboard').click(function () {
            _editDashboardSettingsModal.open();
        });

        abp.event.on('app.dasboardSettingsModalSaved', () => {
            abp.ui.setBusy();
            location.reload();
        });

        function initTruckAvailabilityChart() {
            abp.helper.graphs.initDonutChart({
                container: $("#truckAvailabilityContainer"),
                getDataAsync: _dashboardService.getTruckAvailabilityData,
                hasData: (result) => result.available || result.outOfService,
                highlightedDataIndex: 1,
                donutOptionsGetter: data => ({
                    data: [
                        { label: "Available", value: data.available, color: _colors.success },
                        { label: "Out of Service", value: data.outOfService, color: _colors.danger }
                    ],
                }),
            });
        }

        var initServiceStatusChart = function () {
            abp.helper.graphs.initDonutChart({
                container: $("#serviceStatusContainer"),
                getDataAsync: _dashboardService.getTruckServiceStatusData,
                hasData: (result) => result.noData || result.overdue || result.due || result.ok,
                gotDataCallback: (data, hasData) => {
                    $("#serviceStatusContainer a").attr("href", hasData ? "/App/PreventiveMaintenanceSchedule" : "/App/VehicleServices");
                },
                highlightedDataIndex: 0,
                donutOptionsGetter: data => ({
                    data: [
                        { label: "Overdue", value: data.overdue, color: _colors.danger },
                        { label: "Due", value: data.due, color: _colors.warning },
                        { label: "OK", value: data.ok, color: _colors.success },
                        { label: "No Data", value: data.noData, color: _colors.unavailable }
                    ],
                }),
            });
        };

        var initLicensePlateStatusChart = function () {
            abp.helper.graphs.initDonutChart({
                container: $("#licensePlateStatusContainer"),
                getDataAsync: _dashboardService.getTruckLicensePlateStatusData,
                hasData: (result) => result.noData || result.overdue || result.due || result.ok,
                highlightedDataIndex: 0,
                donutOptionsGetter: data => ({
                    data: [
                        { label: "Overdue", value: data.overdue, color: _colors.danger },
                        { label: "Due", value: data.due, color: _colors.warning },
                        { label: "OK", value: data.ok, color: _colors.success },
                        { label: "No Data", value: data.noData, color: _colors.unavailable }
                    ],
                }),
            });
        };

        var initDriverLicenseChart = function () {
            abp.helper.graphs.initDonutChart({
                container: $("#driverLicenseStatusContainer"),
                getDataAsync: _dashboardService.getDriverLicenseStatusData,
                hasData: (result) => result.noData || result.overdue || result.due || result.ok,
                highlightedDataIndex: 0,
                donutOptionsGetter: data => ({
                    data: [
                        { label: "Overdue", value: data.overdue, color: _colors.danger },
                        { label: "Due", value: data.due, color: _colors.warning },
                        { label: "OK", value: data.ok, color: _colors.success },
                        { label: "No Data", value: data.noData, color: _colors.unavailable }
                    ],
                }),
            });
        };

        var initPhysicalStatusChart = function () {
            abp.helper.graphs.initDonutChart({
                container: $("#physicalStatusContainer"),
                getDataAsync: _dashboardService.getDriverPhysicalStatusData,
                hasData: (result) => result.noData || result.overdue || result.due || result.ok,
                highlightedDataIndex: 0,
                donutOptionsGetter: data => ({
                    data: [
                        { label: "Overdue", value: data.overdue, color: _colors.danger },
                        { label: "Due", value: data.due, color: _colors.warning },
                        { label: "OK", value: data.ok, color: _colors.success },
                        { label: "No Data", value: data.noData, color: _colors.unavailable }
                    ],
                }),
            });
        };

        var initDriverMVRStatusChart = function () {
            abp.helper.graphs.initDonutChart({
                container: $("#driverMVRStatusContainer"),
                getDataAsync: _dashboardService.getDriverMVRStatusData,
                hasData: (result) => result.noData || result.overdue || result.due || result.ok,
                highlightedDataIndex: 0,
                donutOptionsGetter: data => ({
                    data: [
                        { label: "Overdue", value: data.overdue, color: _colors.danger },
                        { label: "Due", value: data.due, color: _colors.warning },
                        { label: "OK", value: data.ok, color: _colors.success },
                        { label: "No Data", value: data.noData, color: _colors.unavailable }
                    ],
                }),
            });
        };

        var $truckUtilizationDateRange = $("#TruckUtilizationDateRange")
            .daterangepicker({ showDropDown: true })
            .val(moment().startOf('month').format('MM/DD/YYYY') + ' - ' + moment().endOf('month').format('MM/DD/YYYY'))
            .on('apply.daterangepicker', function (ev, picker) {
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            });
        var $revenueDateRange = $("#RevenueDateRange")
            .daterangepicker({ showDropDown: true })
            .val(moment().startOf('month').format('MM/DD/YYYY') + ' - ' + moment().endOf('month').format('MM/DD/YYYY'))
            .on('apply.daterangepicker', function (ev, picker) {
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            });
        var $revenuePerTruckDateRange = $("#RevenuePerTruckDateRange")
            .daterangepicker({ showDropDown: true })
            .val(moment().startOf('month').format('MM/DD/YYYY') + ' - ' + moment().endOf('month').format('MM/DD/YYYY'))
            .on('apply.daterangepicker', function (ev, picker) {
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            });

        async function initRevenuePerTruckChart() {
            let chart = await abp.helper.graphs.initBarChart({
                container: $("#revenuePerTruckContainer"),
                loadingContainer: $("#revenuePerTruckLoading"),
                getDataAsync: async function () {
                    let requestData = { datePeriod: $('input[name="RevenuePerTruckDateInterval"]:checked').val() };
                    $.extend(requestData, _dtHelper.getDateRangeObject($revenuePerTruckDateRange.val(), 'periodBegin', 'periodEnd'));
                    let result = await _dashboardService.getRevenuePerTruckByDateGraphData(requestData);
                    return result.revenueGraphData;
                },
                barOptions: {
                    xkey: 'period',
                    ykeys: ['freightRevenueValue', 'materialRevenueValue', 'fuelSurchargeValue'],
                    labels: ['Freight Revenue', 'Material Revenue', 'Fuel Surcharge'],
                    //labels: ['Revenue/Truck'],
                    barColors: [app.colors.freight, app.colors.material, app.colors.fuel],
                    stacked: true,
                    preUnits: _currencySymbol
                }
            });

            $("input[name='RevenuePerTruckDateInterval'").change(function () {
                chart.refresh();
            });
            $revenuePerTruckDateRange.on('apply.daterangepicker', function (ev, picker) {
                chart.refresh();
            });
        }

        async function initRevenueChart() {
            let chart = await abp.helper.graphs.initBarChart({
                container: $("#revenueContainer"),
                loadingContainer: $("#revenueLoading"),
                getDataAsync: async function () {
                    let getRevenueDataInput = { datePeriod: $('input[name="RevenueDateInterval"]:checked').val() };
                    $.extend(getRevenueDataInput, _dtHelper.getDateRangeObject($revenueDateRange.val(), 'periodBegin', 'periodEnd'));
                    let result = await _dashboardService.getRevenueByDateGraphData(getRevenueDataInput);
                    return result.revenueGraphData;
                },
                barOptions: {
                    xkey: 'period',
                    ykeys: ['freightRevenueValue', 'materialRevenueValue', 'fuelSurchargeValue'],
                    labels: ['Freight Revenue', 'Material Revenue', 'Fuel Surcharge'],
                    barColors: [app.colors.freight, app.colors.material, app.colors.fuel],
                    stacked: true,
                    preUnits: _currencySymbol
                }
            });

            $("input[name='RevenueDateInterval'").change(function () {
                chart.refresh();
            });
            $revenueDateRange.on('apply.daterangepicker', function (ev, picker) {
                chart.refresh();
            });
        }

        async function initTruckUtilizationChart() {
            let chart = await abp.helper.graphs.initBarChart({
                container: $("#truckUtilizationContainer"),
                loadingContainer: $("#truckUtilizationLoading"),
                getDataAsync: async function () {
                    var getTruckUtilizationDataInput = { datePeriod: $('input[name="TruckUtilizationDateInterval"]:checked').val() };
                    $.extend(getTruckUtilizationDataInput, _dtHelper.getDateRangeObject($truckUtilizationDateRange.val(), 'periodBegin', 'periodEnd'));
                    var result = await _dashboardService.getTruckUtilizationData(getTruckUtilizationDataInput);
                    return result.truckUtilizationData;
                },
                barOptions: {
                    xkey: 'period',
                    ykeys: ['utilizationPercent'],
                    labels: ['Utilization'],
                    postUnits: '%'
                }
            });

            $("input[name='TruckUtilizationDateInterval'").change(function () {
                chart.refresh();
            });
            $truckUtilizationDateRange.on('apply.daterangepicker', function (ev, picker) {
                chart.refresh();
            });
        }



        loadScheduledTrucks();

        initTruckAvailabilityChart();
        initServiceStatusChart();
        initLicensePlateStatusChart();
        initDriverLicenseChart();
        initPhysicalStatusChart();
        initDriverMVRStatusChart();
        initRevenueChartsDateRange();

        loadRevenueCharts();

        initRevenuePerTruckChart();
        initRevenueChart();
        initTruckUtilizationChart();

        loadGettingStarted();

        function loadGettingStarted() {
            if (!abp.setting.getBoolean('App.GettingStarted.ShowGettingStarted')) {
                return;
            }

            $("#gettingStartedContainer").hide();
            $("#gettingStartedContainer").html('');
            $.ajax(abp.appPath + 'app/Dashboard/GettingStarted').then(html => {
                $("#gettingStartedContainer").html(html);
                $("#gettingStartedContainer").show();

                $('.getting-started-checkbox').click(function () {
                    var isChecked = $(this).is(':checked').toString();
                    var settingName = $(this).data('setting-name');
                    _dashboardService.setSettingValue(settingName, isChecked);
                });

                $('#HideGettingStartedBlock').click(function (e) {
                    e.preventDefault();
                    _dashboardService.setSettingValue('App.GettingStarted.ShowGettingStarted', false);
                    $('div.getting-started').closest('.row').slideUp();
                });
                $('div.getting-started').closest('.row').slideDown();
            });
        }

        function loadScheduledTrucks() {
            let container = $("#scheduledTruckCountContainer");
            if (!container.length) {
                return;
            }
            $.ajax(abp.appPath + 'app/Dashboard/ScheduledTruckCount').then(html => {
                container.html(html);
                $("#scheduledTruckCountLoading").hide();
                container.show();
            });
        }

        function initRevenueChartsDateRange() {

            let yesterdayFormatted = moment().subtract(1, 'day').format('MM/DD/YYYY');
            $("#RevenueChartsDateRange").daterangepicker({ showDropDown: true })
                .val(yesterdayFormatted + ' - ' + yesterdayFormatted)
                .on('apply.daterangepicker', function (ev, picker) {
                    $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
                    loadRevenueCharts();
                });
            $("input[name='TicketType']").change(function () {
                loadRevenueCharts();
            });

        }

        function loadRevenueCharts() {
            var revenueChartsContainer = $("#revenueChartsContainer");
            var revenueChartsLoading = $("#revenueChartsLoading");

            if (!revenueChartsContainer.length) {
                return;
            }

            revenueChartsLoading.show();
            revenueChartsContainer.hide();

            let requestData = {
                ticketType: $('input[name="TicketType"]:checked').val()
            };
            $.extend(requestData, _dtHelper.getDateRangeObject($('#RevenueChartsDateRange').val(), 'periodBegin', 'periodEnd'));
            $.ajax({
                url: abp.appPath + 'app/Dashboard/RevenueCharts',
                data: requestData
            }).then(html => {
                revenueChartsContainer.html(html);
                revenueChartsLoading.hide();
                revenueChartsContainer.show();
            }).catch(() => {
                revenueChartsLoading.hide();
            });
        }

    });
})();