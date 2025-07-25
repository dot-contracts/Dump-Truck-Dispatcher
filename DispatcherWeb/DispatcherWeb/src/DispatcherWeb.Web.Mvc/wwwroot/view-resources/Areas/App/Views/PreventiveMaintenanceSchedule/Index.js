(function () {
    $(function () {

        var _preventiveMaintenanceService = abp.services.app.preventiveMaintenance;
        var _workOrderService = abp.services.app.workOrder;
        var _dtHelper = abp.helper.dataTables;
        var _permissions = {
            edit: abp.auth.isGranted('Pages.PreventiveMaintenanceSchedule.Edit')
        }
        let _preventiveMaintenanceSelectionColumn = {};

        var _createOrEditPreventiveMaintenanceModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/PreventiveMaintenanceSchedule/CreateOrEditPreventiveMaintenanceModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/PreventiveMaintenanceSchedule/_CreateOrEditPreventiveMaintenanceModal.js',
            modalClass: 'CreateOrEditPreventiveMaintenanceModal'
        });

        initFilterControls();

        var preventiveMaintenanceTable = $('#PreventiveMaintenanceTable');

        var preventiveMaintenanceGrid = preventiveMaintenanceTable.DataTableInit({
            stateSave: true,
            stateDuration: 0,
            stateLoadCallback: function (settings, callback) {
                app.localStorage.getItem('preventivemaintenance_filter', function (result) {
                    if ($('form').data('disable-load-state')) {
                        callback();
                        return;
                    }
                    var filter = result || {};

                    if (filter.dueDateFilter) {
                        $('#DueDateFilter').val(filter.dueDateFilter);
                    } else {
                        $('#DueDateFilter').val('');
                    }
                    if (filter.truckCode) {
                        $('#TruckFilter').val(filter.truckCode);
                    }
                    if (filter.status) {
                        $('#StatusFilter').val(filter.status);
                    }
                    if (filter.vehicleServiceId) {
                        abp.helper.ui.addAndSetDropdownValue($("#VehicleServiceFilter"), filter.vehicleServiceId, filter.vehicleServiceName);
                    }
                    app.localStorage.getItem('preventivemaintenance_grid', function (result) {
                        callback(JSON.parse(result));
                    });
                });
            },
            stateSaveCallback: function (settings, data) {
                delete data.columns;
                delete data.search;
                app.localStorage.setItem('preventivemaintenance_grid', JSON.stringify(data));
                app.localStorage.setItem('preventivemaintenance_filter', _dtHelper.getFilterData());
            },
            selectionColumnOptions: _preventiveMaintenanceSelectionColumn,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                $.extend(abpData, _dtHelper.getDateRangeObject(abpData.dueDateFilter, 'dueDateBegin', 'dueDateEnd'));
                if (abpData.dueDateFilter
                    && (!moment.utc(abpData.dueDateBegin, 'YYYY-MM-DDT00:00:00').isValid()
                        || !moment.utc(abpData.dueDateEnd, 'YYYY-MM-DDT00:00:00').isValid())
                ) {
                    abp.message.error('The Due Date is invalid!');
                    return;
                }
                _preventiveMaintenanceService.getPreventiveMaintenancePagedList(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
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
                    responsivePriority: 1,
                    data: "truckCode",
                    title: "Truck"
                },
                {
                    data: "currentMileage",
                    render: function (data, type, full, meta) { return _dtHelper.renderMileage(data); },
                    title: "Curr Miles"
                },
                {
                    data: "vehicleServiceName",
                    title: "Service"
                },
                {
                    data: "dueDate",
                    title: "Due date",
                    render: function (data, type, full, meta) {
                        var result = $("<span>");
                        result.append(formatDueDate(data));
                        return result.html();
                    }
                },
                {
                    data: "dueMileage",
                    title: "Due Mileage",
                    render: function (data, type, full, meta) {
                        var result = $("<span>");
                        result.append(formatDueMileage(data, full.currentMileage));
                        return result.html();
                    }
                },
                {
                    data: "daysUntilDue",
                    title: "Days<br/> until due",
                    render: function (data, type, full, meta) {
                        var result = $("<span>");
                        result.append(getDaysUntilDueVal(data, full.dueDate, full.warningDate));
                        return result.html();
                    }
                },
                {
                    data: "milesUntilDue",
                    title: "Miles<br/> until due",
                    render: function (data, type, full, meta) {
                        var result = $("<span>");
                        result.append(getMilesUntilDueVal(data, full.dueMileage, full.warningMileage, full.currentMileage));
                        return result.html();
                    }
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    width: '10px',
                    responsivePriority: 2,
                    render: function (data, type, full, meta) {
                        return _permissions.edit ?
                            '<div class="dropdown">'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '<ul class="dropdown-menu dropdown-menu-right">'
                            + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                            + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                            + '</ul>'
                            + '</div>' : '';
                    }
                }
            ],
            drawCallback: function (settings) {
                $('input[type="checkbox"][name="select_all"]').show();
                if ($('.checkboxes').length <= 0) {
                    $('input[type="checkbox"][name="select_all"]').hide();
                }
            }
        });

        preventiveMaintenanceTable.on('click', '.btnDeleteRow', function () {
            var record = _dtHelper.getRowData(this);
            deletePreventiveMaintenance(record);
        });

        preventiveMaintenanceTable.on('click', '.btnEditRow', function () {
            var preventiveMaintenanceId = _dtHelper.getRowData(this).id;
            _createOrEditPreventiveMaintenanceModal.open({ id: preventiveMaintenanceId });
        });

        _preventiveMaintenanceSelectionColumn.selectionChangedCallbacks.push(function (selectedRowsCount) {
            $('#CreateWorkOrders').prop('disabled', selectedRowsCount < 1);
        });


        function formatDueDate(dueDate) {
            if (!dueDate) {
                return '';
            }
            var dueDateString = _dtHelper.renderUtcDate(dueDate);
            dueDate = new Date(dueDate);
            var today = new Date();
            if (today > dueDate) {
                return '<span class="text-danger">' + dueDateString + '</span>';
            }
            return dueDateString;
        }
        function formatDueMileage(dueMileage, currentMileage) {
            if (!dueMileage) {
                return '';
            }
            var formattedMileage = _dtHelper.renderMileage(dueMileage);
            if (currentMileage > dueMileage) {
                return '<span class="text-danger">' + formattedMileage + '</span>';
            }
            return formattedMileage;
        }

        function getDaysUntilDueVal(daysUntilDue, dueDate, warningDate) {
            if (daysUntilDue === null) {
                return '';
            }

            dueDate = new Date(dueDate);
            warningDate = new Date(warningDate);
            var today = new Date();
            if (today > dueDate) {
                return '<span class="untildue-overdue">' + daysUntilDue + '</span>';
            } else if (today > warningDate) {
                return '<span class="untildue-warning">' + daysUntilDue + '</span>';
            } else {
                return '<span class="untildue-notreached">' + daysUntilDue + '</span>';
            }
        }
        function getMilesUntilDueVal(milesUntilDue, dueMileage, warningMileage, currentMileage) {
            if (dueMileage === null) {
                return '';
            }
            var formattedMilesUntilDue = _dtHelper.renderMileage(milesUntilDue);
            if (currentMileage > dueMileage) {
                return '<span class="untildue-overdue">' + formattedMilesUntilDue + '</span>';
            } else if (currentMileage > warningMileage) {
                return '<span class="untildue-warning">' + formattedMilesUntilDue + '</span>';
            } else {
                return '<span class="untildue-notreached">' + formattedMilesUntilDue + '</span>';
            }
        }

        function initFilterControls() {
            $("#DueDateFilter").daterangepicker({
                autoUpdateInput: false,
                locale: {
                    cancelLabel: 'Clear'
                }
            }).on('apply.daterangepicker', function (ev, picker) {
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            }).on('cancel.daterangepicker', function (ev, picker) {
                $(this).val('');
            });

            $("#StatusFilter").select2Init({
                showAll: true,
                allowClear: false
            });

            $("#VehicleServiceFilter").select2Init({
                abpServiceMethod: abp.services.app.vehicleService.getSelectList,
                showAll: false,
                allowClear: true
            });
        }


        var reloadMainGrid = function () {
            preventiveMaintenanceGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditPreventiveMaintenanceModal', function () {
            reloadMainGrid();
        });

        $("#AddPreventiveMaintenance").click(function (e) {
            e.preventDefault();
            _createOrEditPreventiveMaintenanceModal.open();
        });

        async function deletePreventiveMaintenance(record) {
            if (await abp.message.confirm('Are you sure you want to delete the PM?')) {
                _preventiveMaintenanceService.deletePreventiveMaintenance({
                    id: record.id
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        }

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $('#StatusFilter').val(1);
            $('#Filter_DueForService').val(false);
            $('#Filter_OfficeId').val('');
            $(".filter").change();
            reloadMainGrid();
        });

        $('#CreateWorkOrders').click(function (e) {
            e.preventDefault();
            _workOrderService.createWorkOrdersFromPreventiveMaintenance({
                preventiveMaintenanceIds: _preventiveMaintenanceSelectionColumn.getSelectedRowsIds()
            })
            .done(function () {
                abp.notify.info('New work orders have been created.');
            });
        });

    });
})();
