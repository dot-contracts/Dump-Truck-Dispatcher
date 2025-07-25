(function () {
    $(function () {

        var _payStatementService = abp.services.app.payStatement;
        var _dtHelper = abp.helper.dataTables;
        var _payStatementId = $("#Id").val();
        var _allowProductionPay = abp.setting.getBoolean('App.TimeAndPay.AllowProductionPay');
        var _basePayOnHourlyJobRate = abp.setting.getBoolean('App.TimeAndPay.BasePayOnHourlyJobRate');

        $('form').validate();
        $.validator.addMethod(
            "regex",
            function (value, element, regexp) {
                var re = new RegExp(regexp, 'i');
                return this.optional(element) || re.test(value);
            },
            "Please check your input."
        );

        var _printDriverPayStatementModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/DriverPayStatements/PrintDriverPayStatementModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/DriverPayStatements/_PrintDriverPayStatementModal.js',
            modalClass: 'PrintDriverPayStatementModal'
        });

        //var _createOrEditPayStatementItemModal = new app.ModalManager({
        //    viewUrl: abp.appPath + 'app/DriverPayStatements/CreateOrEditPayStatementItemModal',
        //    scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/DriverPayStatements/_CreateOrEditPayStatementItemModal.js',
        //    modalClass: 'CreateOrEditPayStatementItemModal'
        //});

        var $dateBegin = $("#StartDate");
        var $dateEnd = $("#EndDate");
        $dateBegin.datepickerInit();
        $dateEnd.datepickerInit();
        $("#StatementDate").datepickerInit();

        //var savePayStatementAsync = function (callback) {
        //    var form = $("#PayStatemenForm");
        //    if (!form.valid()) {
        //        form.showValidateMessage();
        //        return;
        //    }
        //    if (!abp.helper.validateStartEndDates(
        //        { value: $dateBegin.val(), title: $('label[for="StartDate"]').text() },
        //        { value: $dateEnd.val(), title: $('label[for="EndDate"]').text() }
        //    )) {
        //        return;
        //    }

        //    var payStatement = form.serializeFormToObject();
        //    abp.ui.setBusy(form);
        //    _payStatementService.editPayStatemen(payStatement).done(function (data) {
        //        abp.notify.info('Saved successfully.');
        //        _payStatementId = data.id;
        //        $("#Id").val(_payStatementId);
        //        if (data.endDate) {
        //            $dateEnd.val(_dtHelper.renderUtcDate(data.endDate)).change();
        //        }
        //        history.replaceState({}, "", abp.appPath + 'app/DriverPayStatements/Details/' + _payStatementId);
        //        if (callback)
        //            callback();
        //    }).always(function () {
        //        abp.ui.clearBusy(form);
        //    });
        //};

        function saveItem(item) {
            return _payStatementService.editPayStatementItem(item);
        }

        var payStatementItemsTable = $('#PayStatementItemsTable');
        var payStatementItemsGrid = payStatementItemsTable.DataTableInit({
            paging: false,
            info: false,
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                if (_payStatementId === '') {
                    callback(_dtHelper.getEmptyResult());
                    return;
                }
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, { payStatementId: _payStatementId });
                _payStatementService.getPayStatementItems(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            editable: {
                saveCallback: function (rowData, cell) {
                    return saveItem(rowData);
                }
            },
            order: [[2, "desc"], [3, "asc"], [4, "asc"]],
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
                    data: "id",
                    visible: false
                },
                {
                    data: "itemKind",
                    visible: false
                },
                {
                    data: "driverName",
                    title: "Driver"
                },
                {
                    data: "date",
                    title: "Date",
                    render: function (data, type, full, meta) {
                        switch (full.itemKind) {
                            case abp.enums.payStatementItemKind.time:
                                return _dtHelper.renderUtcDate(data);
                            case abp.enums.payStatementItemKind.ticket:
                                return _dtHelper.renderActualUtcDateShortTime(data);
                        }
                        return '';
                    }
                },
                {
                    data: "item",
                    title: "Item",
                    visible: _allowProductionPay || _basePayOnHourlyJobRate,
                },
                {
                    data: "customerName",
                    title: "Customer",
                    visible: _allowProductionPay || _basePayOnHourlyJobRate,
                },
                {
                    data: "jobNumber",
                    title: "Job #",
                    visible: _allowProductionPay || _basePayOnHourlyJobRate,
                },
                {
                    data: "deliverToName",
                    title: "Deliver To",
                    visible: _allowProductionPay || _basePayOnHourlyJobRate,
                },
                {
                    data: "loadAtName",
                    title: "Load At",
                    visible: _allowProductionPay || _basePayOnHourlyJobRate,
                },
                {
                    data: "timeClassificationName",
                    title: "Time Class",
                    className: "all",
                    editable: {
                        editor: _dtHelper.editors.dropdown,
                        idField: 'timeClassificationId',
                        nameField: 'timeClassificationName',
                        dropdownOptions: {
                            abpServiceMethod: abp.services.app.timeClassification.getTimeClassificationsSelectList,
                            abpServiceParamsGetter: (params, rowData) => ({
                                excludeProductionPay: rowData.itemKind === abp.enums.payStatementItemKind.time
                            }),
                            showAll: true,
                            allowClear: false
                        },
                        editCompleteCallback: function (editResult, rowData, cell) {
                            rowData.isProductionPay = editResult.isProductionPay;
                            refreshDriverRate(rowData, cell);
                            refreshRowTotal(rowData, cell);
                        }
                    }
                },
                {
                    data: "freightRateToPayDrivers",
                    render: function (data, type, full, meta) { return data ? _dtHelper.renderMoney(data) : ''; },
                    title: "Freight<br>Rate",
                    visible: _allowProductionPay
                },
                {
                    data: "driverPayRate",
                    title: "Driver<br>Pay Rate",
                    className: "all driver-pay-rate-cell",
                    editable: {
                        editor: _dtHelper.editors.quantity,
                        maxValue: undefined, //100 for tickets or 1000000 for time
                        validate: function (rowData, newValue) {
                            var maxValue = rowData.isProductionPay ? 100 : 1000000;
                            if (newValue > maxValue) {
                                abp.message.error('Please enter a valid number less than ' + maxValue + '!');
                                return false;
                            }
                            return true;
                        },
                        getDisplayValue: function (rowData, fieldName) {
                            return formatDriverRate(rowData);
                        },
                        editCompleteCallback: function (editResult, rowData, cell) {
                            refreshRowTotal(rowData, cell);
                        }
                    }
                },
                {
                    data: "quantity",
                    title: "Quantity",
                    className: "all",
                    editable: {
                        editor: _dtHelper.editors.quantity,
                        editCompleteCallback: function (editResult, rowData, cell) {
                            refreshRowTotal(rowData, cell);
                        }
                    }
                },
                {
                    data: "total",
                    render: function (data, type, full, meta) { return _dtHelper.renderMoney(data); },
                    name: "total",
                    className: "total-cell",
                    title: "Ext. Amount"
                }
                //{
                //    data: null,
                //    orderable: false,
                //    autoWidth: false,
                //    width: "10px",
                //    responsivePriority: 1,
                //    defaultContent: '<div class="dropdown action-button">'
                //        + '<ul class="dropdown-menu dropdown-menu-right">'
                //        + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                //        + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                //        + '</ul>'
                //        + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                //        + '</div>'
                //}
            ]
        });

        function refreshDriverRate(rowData, updatedCell) {
            if (rowData.isProductionPay && rowData.driverPayRate > 100) {
                rowData.driverPayRate = 0;
            }
            updatedCell.closest('tr').find('td.driver-pay-rate-cell input').val(formatDriverRate(rowData));
        }

        function formatDriverRate(rowData) {
            let val = rowData.driverPayRate;
            return rowData.isProductionPay
                ? val + '%'
                : _dtHelper.renderMoney(val);
        }

        function refreshRowTotal(rowData, updatedCell) {
            rowData.total = getRowTotal(rowData);
            updatedCell.closest('tr').find('td.total-cell').text(_dtHelper.renderMoney(rowData.total));
        }

        function getRowTotal(rowData) {
            switch (rowData.itemKind) {
                case abp.enums.payStatementItemKind.ticket:
                    return rowData.isProductionPay
                        ? abp.utils.round(rowData.quantity * rowData.driverPayRate * (rowData.freightRateToPayDrivers || 0) / 100)
                        : abp.utils.round(rowData.quantity * rowData.driverPayRate);
                case abp.enums.payStatementItemKind.time:
                    return abp.utils.round(rowData.quantity * rowData.driverPayRate);
            }
            return 0;
        }

        var reloadMainGrid = function () {
            payStatementItemsGrid.ajax.reload();
        };

        //$("#SavePayStatementButton").click(function (e) {
        //    e.preventDefault();
        //    savePayStatementAsync(function () {
        //        reloadMainGrid();
        //    });
        //});

        //payStatementItemsTable.on('click', '.btnEditRow', function () {
        //    var payStatementItemId = _dtHelper.getRowData(this).id;
        //    _createOrEditPayStatementItemModal.open({ id: payStatementItemId });
        //});

        //payStatementItemsTable.on('click', '.btnDeleteRow', function () {
        //    var payStatementItemId = _dtHelper.getRowData(this).id;
        //    abp.message.confirm(
        //        'Are you sure you want to delete the item?',
        //        function (isConfirmed) {
        //            if (isConfirmed) {
        //                _payStatementService.deletePayStatementItem({
        //                    id: payStatementItemId
        //                }).done(function () {
        //                    abp.notify.info('Successfully deleted.');
        //                    reloadMainGrid();
        //                });
        //            }
        //        }
        //    );
        //});

        $("#PrintPayStatementButton").click(function (e) {
            e.preventDefault();
            _printDriverPayStatementModal.open({ id: _payStatementId });
        });

        $("#ExportPayStatementButton").click(async function (e) {
            e.preventDefault();
            var button = $(this);
            try {
                abp.ui.setBusy(button);
                var tempFile = await _payStatementService.exportPayStatementToCsv({ id: _payStatementId });
                app.downloadTempFile(tempFile);
            } finally {
                abp.ui.clearBusy(button);
            }
        });

    });
})();
