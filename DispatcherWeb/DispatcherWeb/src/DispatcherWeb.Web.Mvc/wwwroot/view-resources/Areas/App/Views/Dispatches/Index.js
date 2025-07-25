(function () {
    $(function () {

        var _dispatchingService = abp.services.app.dispatching;
        var _dtHelper = abp.helper.dataTables;

        initFilterControls();

        var dispatchesTable = $('#DispatchesTable');

        var dispatchesGrid = dispatchesTable.DataTableInit({
            //stateSave: true,
            //stateDuration: 0,
            //stateLoadCallback: function (settings, callback) {
            //	app.localStorage.getItem('dispatches_filter', function (result) {
            //		var filter = result || {};

            //		if (filter.dateFilter) {
            //			$('#DateFilter').val(filter.dateFilter);
            //		}

            //		if (filter.customerId) {
            //			abp.helper.ui.addAndSetDropdownValue($("#CustomerIdFilter"), filter.customerId, filter.customerName);
            //		}

            //		app.localStorage.getItem('dispatches_grid', function (result) {
            //			callback(JSON.parse(result));
            //		});
            //	});
            //},
            //stateSaveCallback: function (settings, data) {
            //	delete data.columns;
            //	delete data.search;
            //	app.localStorage.setItem('dispatches_grid', JSON.stringify(data));
            //	app.localStorage.setItem('dispatches_filter', _dtHelper.getFilterData());
            //},
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                $.extend(abpData, _dtHelper.getDateRangeObject(abpData.dateFilter, 'dateBegin', 'dateEnd'));
                _dispatchingService.getDispatchPagedList(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            order: [[1, 'asc']],
            columns: [
                {
                    width: '20px',
                    className: 'control responsive',
                    orderable: false,
                    render: function () {
                        return '';
                    }
                },
                {
                    data: "truckCode",
                    title: "Truck",
                    responsivePriority: 0
                },
                {
                    data: "driverLastFirstName",
                    title: "Driver",
                    responsivePriority: 1
                },
                {
                    data: "sent",
                    title: "Sent",
                    render: function (data, type, full, meta) { return _dtHelper.renderDateShortTime(data); },
                    responsivePriority: 0
                },
                {
                    data: "acknowledged",
                    title: "Acknowledged",
                    render: function (data, type, full, meta) { return _dtHelper.renderDateShortTime(data); },
                    responsivePriority: 0
                },
                {
                    data: "loaded",
                    title: "Loaded",
                    render: function (data, type, full, meta) { return _dtHelper.renderDateShortTime(data); },
                    responsivePriority: 0
                },
                {
                    data: "delivered",
                    title: "Delivered",
                    render: function (data, type, full, meta) { return _dtHelper.renderDateShortTime(data); },
                    responsivePriority: 0
                },
                {
                    data: "customerName",
                    title: "Customer",
                    responsivePriority: 0
                },
                {
                    data: "loadAtName",
                    title: "Load At",
                    responsivePriority: 0
                },
                {
                    data: "deliverToName",
                    title: "Deliver To",
                    responsivePriority: 0
                },
                {
                    data: "item",
                    title: "Item",
                    responsivePriority: 0
                }

            ]
        });

        function reloadMainGrid() {
            dispatchesGrid.ajax.reload();
        }

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            reloadMainGrid();
        });

        function initFilterControls() {
            var todayString = moment().startOf('day').format('MM/DD/YYYY');
            $("#DateFilter").daterangepicker({
                autoUpdateInput: false,
                locale: {
                    cancelLabel: 'Clear'
                },
                showDropDown: true
            }).on('apply.daterangepicker', function (ev, picker) {
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
                $("#CustomerIdFilter").val('').trigger('change');
                $("#OfficeIdFilter").val('').trigger('change');
            }).on('cancel.daterangepicker', function (ev, picker) {
                $(this).val('');
            });
            if (!$('#OrderLineId').val()) {
                $("#DateFilter").val(todayString + ' - ' + todayString);
            }

            $('#DateFilter').blur(function () {
                if (!moment($(this).val(), 'MM/DD/YYYY').isValid()) {
                    $(this).val(todayString + ' - ' + todayString);
                }
            });
            $("#TruckFilter").select2Init({
                abpServiceMethod: abp.services.app.truck.getTrucksSelectList,
                abpServiceParams: {
                    excludeTrailers: true,
                    includeLeaseHaulerTrucks: true
                },
                showAll: false,
                allowClear: true
            });
            $("#DriverFilter").select2Init({
                abpServiceMethod: abp.services.app.driver.getDriversSelectList,
                abpServiceParams: {
                    includeLeaseHaulerDrivers: true
                },
                showAll: false,
                allowClear: true
            });
            $("#CustomerIdFilter").select2Init({
                abpServiceMethod: abp.services.app.customer.getCustomersWithOrdersSelectList,
                abpServiceParamsGetter: (params) => ({
                    ..._dtHelper.getDateRangeObject($("#DateFilter").val(), 'dateBegin', 'dateEnd')
                }),
                showAll: false,
                allowClear: true
            });
            $("#OfficeIdFilter").select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: true
            });
        }

        $('#ExportDispatchesToCsvButton').click(function () {
            var $button = $(this);
            var abpData = {};
            $.extend(abpData, _dtHelper.getFilterData());
            $.extend(abpData, _dtHelper.getDateRangeObject(abpData.dateFilter, 'dateBegin', 'dateEnd'));
            abp.ui.setBusy($button);
            _dispatchingService
                .getDispatchesToCsv(abpData)
                .done(function (result) {
                    app.downloadTempFile(result);
                }).always(function () {
                    abp.ui.clearBusy($button);
                });
        });

    });
})();


