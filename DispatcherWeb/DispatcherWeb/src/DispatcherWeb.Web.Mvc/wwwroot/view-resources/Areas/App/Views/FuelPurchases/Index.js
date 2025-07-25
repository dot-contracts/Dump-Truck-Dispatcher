(function () {
    $(function () {

        var _fuelPurchaseService = abp.services.app.fuelPurchase;
        var _dtHelper = abp.helper.dataTables;
        var _canEdit = abp.auth.hasPermission('Pages.FuelPurchases.Edit');

        var _createOrEditFuelPurchaseModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/FuelPurchases/CreateOrEditFuelPurchaseModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/FuelPurchases/_CreateOrEditFuelPurchaseModal.js',
            modalClass: 'CreateOrEditFuelPurchaseModal',
        });


        initFilterControls();

        var fuelPurchasesTable = $('#FuelPurchasesTable');
        var fuelPurchasesGrid = fuelPurchasesTable.DataTableInit({
            stateSave: true,
            stateDuration: 0,
            stateLoadCallback: function (settings, callback) {
                app.localStorage.getItem('fuelpurchases_filter', function (result) {
                    var filter = result || {};

                    setOffice(filter);
                    if (filter.fuelDateTime) {
                        $('#FuelDateTimeFilter').val(filter.fuelDateTime);
                    } else {
                        $('#FuelDateTimeFilter').val('');
                    }
                    if (filter.truckId) {
                        abp.helper.ui.addAndSetDropdownValue($("#TruckFilter"), filter.truckId, filter.truckCode);
                    }

                    app.localStorage.getItem('fuelpurchases_grid', function (result) {
                        callback(JSON.parse(result));
                    });
                });
            },
            stateSaveCallback: function (settings, data) {
                delete data.columns;
                delete data.search;
                app.localStorage.setItem('fuelpurchases_grid', JSON.stringify(data));
                app.localStorage.setItem('fuelpurchases_filter', _dtHelper.getFilterData());
            },
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                $.extend(abpData, _dtHelper.getDateRangeObject(abpData.fuelDateTime, 'fuelDateTimeBegin', 'fuelDateTimeEnd'));
                _fuelPurchaseService.getFuelPurchasePagedList(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            order: [[0, 'asc']],
            columns: [
                {
                    width: '20px',
                    className: 'control responsive',
                    orderable: false,
                    render: function () {
                        return '';
                    },
                },
                {
                    data: "truckCode",
                    title: "Truck"
                },
                {
                    data: "fuelDateTime",
                    title: "Fuel Date Time",
                    render: function (data, type, full, meta) { return _dtHelper.renderDateTime(data); }
                },
                {
                    data: "amount",
                    title: "Amount"
                },
                {
                    data: "rate",
                    title: "Rate"
                },
                {
                    data: "odometer",
                    render: function (data, type, full, meta) { return _dtHelper.renderMileage(data); },
                    title: "Odometer"
                },
                {
                    data: "ticketNumber",
                    title: "TicketNumber"
                },
                {
                    name: "Actions",
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    responsivePriority: 1,
                    width: '10px',
                    rowAction: {
                        items: [{
                            text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                            visible: function (data) {
                                return _canEdit;
                            },
                            action: function (data) {
                                var fuelPurchaseId = data.record.id;
                                _createOrEditFuelPurchaseModal.open({ id: fuelPurchaseId, officeId: $('#OfficeIdFilter').val() });
                            }
                        }, {
                            text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                            visible: function (data) {
                                return _canEdit;
                            },
                            action: async function (data) {
                                var fuelPurchaseId = data.record.id;
                                if (await abp.message.confirm(
                                    'Are you sure you want to delete the Fuel entry?'
                                )) {
                                    _fuelPurchaseService.deleteFuelPurchase({
                                        id: fuelPurchaseId
                                    }).done(function () {
                                        abp.notify.info('Successfully deleted.');
                                        reloadMainGrid();
                                    });
                                }
                            }
                        }]
                    }
                }

            ]
        });

        function initFilterControls() {
            $("#OfficeIdFilter").select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: true
            });
            $("#TruckFilter").select2Init({
                abpServiceMethod: abp.services.app.truck.getTrucksSelectList,
                abpServiceParamsGetter: (params) => ({
                    officeId: $('#OfficeIdFilter').val(),
                    allOffices: true
                }),
                showAll: false,
                allowClear: true
            });
            $("#FuelDateTimeFilter").daterangepicker({
                locale: {
                    cancelLabel: 'Clear'
                }
            }).on('apply.daterangepicker', function (ev, picker) {
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            }).on('cancel.daterangepicker', function (ev, picker) {
                $(this).val('');
            });
        }

        $('#OfficeIdFilter').on('change', function () {
            $("#TruckFilter").val('').change();
        });


        var reloadMainGrid = function () {
            fuelPurchasesGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditFuelPurchaseModalSaved', function () {
            reloadMainGrid();
        });

        $("#CreateNewFuelPurchaseButton").click(function (e) {
            e.preventDefault();
            _createOrEditFuelPurchaseModal.open({ id: null, officeId: $('#OfficeIdFilter').val() });
        });

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            //setOffice({});
            $(".filter").change();
            reloadMainGrid();
        });

        function setOffice(filter) {
            if (!filter.officeId) {
                filter.officeId = abp.session.officeId;
                filter.officeName = abp.session.officeName;
            }

            if (filter.officeId) {
                abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), filter.officeId, filter.officeName);
            }

        }

    });
})();
