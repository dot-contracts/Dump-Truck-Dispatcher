(function () {
    $(function () {

        var _orderService = abp.services.app.order;
        var _dtHelper = abp.helper.dataTables;
        var _isFilterReady = false;
        var _isGridInitialized = false;

        //init the filter controls
        app.sessionStorage.getItem('ReceiptsFilter', function (cachedFilter) {
            if (!cachedFilter) {
                cachedFilter = {
                    startDate: null,
                    endDate: null,
                    customerId: "",
                    customerName: "Select a customer",
                    officeId: abp.session.officeId,
                    officeName: abp.session.officeName
                };
            }

            var dateFilterIsEmpty = false;

            if (!cachedFilter.startDate || cachedFilter.startDate === 'Invalid date') {
                dateFilterIsEmpty = true;
                //still need to init the daterangepicker with real dates first and clear the inputs only after the init.
                cachedFilter.startDate = moment().format("MM/DD/YYYY");
            }

            if (!cachedFilter.endDate || cachedFilter.endDate === 'Invalid date') {
                dateFilterIsEmpty = true;
                cachedFilter.endDate = moment().add(1, 'days').format("MM/DD/YYYY");
            }

            $("#DateStartFilter").val(cachedFilter.startDate);
            $("#DateEndFilter").val(cachedFilter.endDate);
            $("#DateFilter").val($("#DateStartFilter").val() + ' - ' + $("#DateEndFilter").val());

            $("#DateFilter").daterangepicker({
                locale: {
                    cancelLabel: 'Clear'
                }
            }, function (start, end, label) {
                $("#DateStartFilter").val(start.format('MM/DD/YYYY'));
                $("#DateEndFilter").val(end.format('MM/DD/YYYY'));
            });

            $("#DateFilter").on('apply.daterangepicker', function (ev, picker) {
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
                $("#DateStartFilter").val(picker.startDate.format('MM/DD/YYYY'));
                $("#DateEndFilter").val(picker.endDate.format('MM/DD/YYYY'));
                reloadMainGrid();
            });

            $("#DateFilter").on('cancel.daterangepicker', function (ev, picker) {
                $(this).val('');
                $("#DateStartFilter").val('');
                $("#DateEndFilter").val('');
                reloadMainGrid();
            });

            if (dateFilterIsEmpty) {
                $("#DateFilter").val('');
                $("#DateStartFilter").val('');
                $("#DateEndFilter").val('');
            }

            $("#OfficeIdFilter").select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: true
            });
            if (cachedFilter.officeId) {
                abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), cachedFilter.officeId, cachedFilter.officeName);
            }

            $("#CustomerIdFilter").select2Init({
                abpServiceMethod: abp.services.app.customer.getCustomersSelectList,
                showAll: false,
                allowClear: true
            });
            if (cachedFilter.customerId) {
                abp.helper.ui.addAndSetDropdownValue($("#CustomerIdFilter"), cachedFilter.customerId, cachedFilter.customerName);
            }

            _isFilterReady = true;
            if (_isGridInitialized) {
                reloadMainGrid(null, false);
            }
        });

        var lastSortingString = "";

        var receiptsTable = $('#ReceiptsTable');
        var receiptsGrid = receiptsTable.DataTableInit({
            stateSave: true,
            stateDuration: 0,
            ajax: function (data, callback, settings) {
                if (!_isGridInitialized) {
                    _isGridInitialized = true;
                }
                if (!_isFilterReady) {
                    callback(_dtHelper.getEmptyResult());
                    return;
                }
                var abpData = _dtHelper.toAbpData(data);
                var filterData = _dtHelper.getFilterData();
                lastSortingString = abpData.sorting;
                app.sessionStorage.setItem('ReceiptsFilter', filterData);
                $.extend(abpData, filterData);
                _orderService.getReceipts(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            order: [[1, 'asc']],
            columns: [
                {
                    width: '20px',
                    className: 'control responsive',
                    orderable: false,
                    render: function () { return ''; }
                },
                {
                    data: "deliveryDate",
                    render: function (data, type, full, meta) { return _dtHelper.renderUtcDate(data); },
                    title: "Delivery Date"
                },
                {
                    data: "customerName",
                    title: "Customer"
                },
                {
                    data: "codTotal",
                    orderable: false,
                    render: function (data, type, full, meta) { return _dtHelper.renderMoney(data); },
                    title: "Total"
                },
                {
                    data: null,
                    responsivePriority: 1,
                    orderable: false,
                    name: "Actions",
                    title: " ",
                    width: "40px",
                    className: "actions",
                    defaultContent: '<div class="dropdown action-button">'
                        + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                        + '<ul class="dropdown-menu">'
                        + '<li><a class="btnEditReceipt" title="Edit Receipt"><i class="fa fa-edit"></i> Edit</a></li>'
                        + '</ul>'
                        + '</div>'
                }
            ]
        });

        function reloadMainGrid(callback, resetPaging) {
            resetPaging = resetPaging === undefined ? true : resetPaging;
            receiptsGrid.ajax.reload(callback, resetPaging);
        }

        //$(".filter").change(function () {
        //    reloadMainGrid();
        //});

        receiptsTable.on('click', '.btnEditReceipt', function () {
            var receiptId = _dtHelper.getRowData(this).receiptId;
            window.location = abp.appPath + 'app/Receipts/Details/' + receiptId;
        });

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            $("#DateStartFilter").val('');
            $("#DateEndFilter").val('');
            //abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), abp.session.officeId, abp.session.officeName);
            reloadMainGrid();
        });

        $("#ExportReceiptsButton").click(function (e) {
            e.preventDefault();
            var request = {
                sorting: lastSortingString
            };
            $.extend(request, _dtHelper.getFilterData());
            _orderService.exportReceiptsToExcel(request).done(function (result) {
                app.downloadTempFile(result);
            });
        });

    });
})();
