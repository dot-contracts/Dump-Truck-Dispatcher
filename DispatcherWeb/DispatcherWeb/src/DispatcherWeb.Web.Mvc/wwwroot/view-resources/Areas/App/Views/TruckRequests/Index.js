(function () {
    const _dtHelper = abp.helper.dataTables;
    const _leaseHaulerRequestListService = abp.services.app.leaseHaulerRequestList;
    const _leaseHaulerRequestEditService = abp.services.app.leaseHaulerRequestEdit;
    const _permissions = {
        viewLeaseHaulerRequests: abp.auth.hasPermission('Pages.LeaseHaulerRequests'),
        editLeaseHaulerRequests: abp.auth.hasPermission('Pages.LeaseHaulerRequests.Edit'),
        leaseHaulerTruckRequests: abp.auth.hasPermission('LeaseHaulerPortal.Truck.Request'),
    };

    initFilterControls();

    const _createOrEditLeaseHaulerRequestModal = new app.ModalManager({
        viewUrl: abp.appPath + 'app/LeaseHaulerRequests/CreateOrEditLeaseHaulerRequestModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/LeaseHaulerRequests/_CreateOrEditLeaseHaulerRequestModal.js',
        modalClass: 'CreateOrEditLeaseHaulerRequestModal'
    });

    const _editTruckRequestModal = new app.ModalManager({
        viewUrl: abp.appPath + 'app/TruckRequests/EditTruckRequestDetailModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/TruckRequests/_EditTruckRequestDetailModal.js',
        modalClass: 'EditTruckRequestDetailModal'
    });

    const _sendLeaseHaulerRequestModal = new app.ModalManager({
        viewUrl: abp.appPath + 'app/LeaseHaulerRequests/SendLeaseHaulerRequestModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/LeaseHaulerRequests/_SendLeaseHaulerRequestModal.js',
        modalClass: 'SendLeaseHaulerRequestModal'
    });

    const useShifts = abp.setting.getBoolean('App.General.UseShifts');

    const truckRequestTable = $('#TruckRequestsTable');
    const truckRequestGrid = truckRequestTable.DataTableInit({
        stateSave: true,
        stateDuration: 0,
        stateLoadCallback: function (settings, callback) {
            app.localStorage.getItem('truckRequests_filter',
                function (result) {
                    var filter = result || {};

                    if (filter.dateRangeFilter) {
                        $('#DateRangeFilter').val(filter.dateRangeFilter);
                    } else {
                        resetDateRangeFilterToDefault();
                    }
                    if (_permissions.viewLeaseHaulerRequests) {
                        if (filter.leaseHaulerId) {
                            abp.helper.ui.addAndSetDropdownValue($("#LeaseHaulerIdFilter"),
                                filter.leaseHaulerId,
                                filter.leaseHaulerName);
                        }
                    }
                    if (filter.shift) {
                        $('#ShiftFilter').val(filter.shift).trigger('change');
                    }
                    if (filter.officeId) {
                        abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"),
                            filter.officeId,
                            filter.officeName);
                    } else {
                        setUserOffice();
                    }

                    app.localStorage.getItem('truckRequests_grid',
                        function (result) {
                            callback(JSON.parse(result));
                        });

                });
        },
        stateSaveCallback: function (settings, data) {
            delete data.columns;
            delete data.search;
            app.localStorage.setItem('truckRequests_grid', JSON.stringify(data));
            app.localStorage.setItem('truckRequests_filter', _dtHelper.getFilterData());
        },
        ajax: function (data, callback, settings) {
            var abpData = _dtHelper.toAbpData(data);
            $.extend(abpData, _dtHelper.getFilterData());
            $.extend(abpData, parseDate(abpData.dateRangeFilter));
            if (_permissions.leaseHaulerTruckRequests) {
                abpData.leaseHaulerId = abp.session.leaseHaulerId;
            }

            localStorage.setItem('truckRequests_filter', JSON.stringify(abpData));

            _leaseHaulerRequestListService.getLeaseHaulerRequestPagedList(abpData).done(function (abpResult) {
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
                data: 'date',
                render: function (data, type, full, meta) {
                    return _dtHelper.renderUtcDate(full.date) + (useShifts ? ' ' + _dtHelper.renderText(full.shift) : '');
                },
                title: 'Date' + (useShifts ? '/Shift' : ''),
                width: '50px'
            },
            {
                data: 'leaseHauler',
                title: 'Lease Hauler',
                visible: _permissions.viewLeaseHaulerRequests
            },
            {
                data: 'sent',
                render: function (data, type, full, meta) { return _dtHelper.renderDateShortTime(full.sent); },
                title: 'Sent',
                width: '100px'
            },
            {
                data: 'message',
                title: 'Request Info',
                responsivePriority: 1
            },
            {
                data: 'comments',
                title: 'Comments',
            },
            {
                data: 'available',
                title: 'Available',
                width: '50px',
                className: _permissions.editLeaseHaulerRequests || _permissions.leaseHaulerTruckRequests ? "cell-editable" : "",
                render: function (data, type, full, meta) {
                    if (_permissions.editLeaseHaulerRequests) {
                        return '<input class="form-control" name="Available" type="text" value="' + (full.available === null ? '' : _dtHelper.renderText(full.available)) + '">';
                    } else if (_permissions.leaseHaulerTruckRequests) {
                        if (full.approved && full.approved > 0) {
                            return '<span class="p-3">' + data + '</span>';
                        }
                        return '<input class="form-control" name="Available" type="text" value="' + (full.available === null ? '' : _dtHelper.renderText(full.available)) + '">';
                    } else {
                        return '';
                    }
                }
            },
            {
                data: 'approved',
                title: 'Approved',
                width: '50px',
                className: _permissions.editLeaseHaulerRequests ? "cell-editable" : "",
                render: function (data, type, full, meta) {
                    if (_permissions.editLeaseHaulerRequests) {
                        return '<input class="form-control" name="Approved" type="text" value="' + (full.approved === null ? '' : _dtHelper.renderText(full.approved)) + '">';
                    }
                    return data;
                }
            },
            {
                data: 'scheduled',
                title: 'Scheduled',
                width: '50px'
            },
            {
                data: null,
                orderable: false,
                title: '',
                width: '10px',
                render: function (data, type, full, meta) {
                    return (data.specifiedTrucks ?? null) < (data.approved ?? null) ? '<i class="fa fa-asterisk text-danger"></i>' : '';
                }
            },
            {
                data: null,
                orderable: false,
                title: '',
                width: "10px",
                className: 'actions all',
                render: function (data, type, full, meta) {
                    return '<div class="dropdown">' +
                        '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>' +
                        '<ul class="dropdown-menu dropdown-menu-right">' +
                        '<li><a class="btnEditRow dropdown-item"><i class="fa fa-edit"></i> Edit</a></li>' +
                        '</ul></div>';
                }
            }
        ]
    });

    truckRequestTable.on('draw.dt', function () {
        createInputControlsFocusOutHandler();
    });
    truckRequestTable.on('click', '.btnEditRow', function () {
        var requestId = _dtHelper.getRowData(this).id;
        _editTruckRequestModal.open({ leaseHaulerRequestId: requestId });
    });
    truckRequestGrid.on('responsive-display', function (e, datatable, row, showHide, update) {
        if (showHide) {
            createInputControlsFocusOutHandler();
        }
    });

    $('#CreateNewButton').click(function (e) {
        _createOrEditLeaseHaulerRequestModal.open();
    });

    $('#SendRequestsButton').click(function (e) {
        _sendLeaseHaulerRequestModal.open();
    });

    abp.event.on('app.createOrEditLeaseHaulerRequestModalSaved', function () {
        reloadMainGrid();
    });
    abp.event.on('app.sendLeaseHaulerRequestModalSaved', function () {
        reloadMainGrid();
    });

    $('form').submit(function (e) {
        e.preventDefault();
        reloadMainGrid();
    });
    $("#ClearSearchButton").click(function () {
        $(this).closest('form')[0].reset();
        if (abp.session.officeId) {
            setUserOffice();
        } else {
            $("#OfficeIdFilter").val("").trigger("change");
        }
        $("#ShiftFilter").val(0).trigger("change");
        resetDateRangeFilterToDefault();
        reloadMainGrid();
    });

    function initFilterControls() {
        $("#DateRangeFilter").daterangepicker({
            locale: {
                cancelLabel: 'Clear'
            },
            showDropDown: true
        }).on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
        }).on('cancel.daterangepicker', function () {
            $(this).val('');
        });

        $('#LeaseHaulerIdFilter').select2Init({
            abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulersSelectList,
            showAll: false,
            allowClear: true
        });

        $("#ShiftFilter").select2Init({
            showAll: true,
            allowClear: false
        });

        $("#OfficeIdFilter").select2Init({
            abpServiceMethod: listCacheSelectLists.office(),
            showAll: true,
            allowClear: true
        });
    }

    function resetDateRangeFilterToDefault() {
        $("#DateRangeFilter").val(moment().add(1, 'days').format("MM/DD/YYYY") + ' - ' + moment().add(1, 'days').format("MM/DD/YYYY"));
    }

    function setUserOffice() {
        abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"),
            abp.session.officeId,
            abp.session.officeName);
    }

    function parseDate(dateRangeString) {
        const dateObject = {};
        let dateStringArray;
        if (dateRangeString) {
            dateStringArray = dateRangeString.split(' - ');
            $.extend(dateObject, { dateBegin: abp.helper.parseDateToJsonWithoutTime(dateStringArray[0]), dateEnd: abp.helper.parseDateToJsonWithoutTime(dateStringArray[1]) });
        }
        return dateObject;
    }

    function createInputControlsFocusOutHandler() {
        createInputFocusOutHandler(truckRequestTable.find('input[name="Available"]'), 'available', _leaseHaulerRequestEditService.updateAvailable);
        createInputFocusOutHandler(truckRequestTable.find('input[name="Approved"]'), 'approved', _leaseHaulerRequestEditService.updateApproved);
    }

    function createInputFocusOutHandler($inputCtrl, field, serviceMethod) {
        $inputCtrl.off('focusout').on('focusout', function () {
            const $ctrl = $(this);
            const $cell = $ctrl.closest('td');
            const rowData = _dtHelper.getRowData($cell[0]);
            const oldValue = rowData[field];
            let newValue = $ctrl.val();
            if (isNaN(newValue) || parseInt(newValue) < 0 || parseInt(newValue) > 1000 || parseInt(newValue).toString() !== newValue) {
                abp.message.error('Please enter a valid number!');
                $ctrl.val(oldValue);
                return;
            }
            newValue = newValue === '' ? null : parseInt(newValue);
            if (newValue === oldValue) {
                return;
            }
            const available = field === 'available' ? newValue : rowData.available;
            const approved = field === 'approved' ? newValue : rowData.approved;
            if (available !== null && approved !== null && available < approved || available === null && approved !== null) {
                abp.message.error('Approved must be less than or equal to available!');
                $ctrl.val(isNaN(oldValue) || oldValue === null ? '' : oldValue);
                return;
            }

            abp.ui.setBusy($cell);
            const input = {
                id: rowData.id,
                value: newValue
            };
            serviceMethod(
                input
            ).done(function () {
                rowData[field] = newValue;
                abp.notify.info('Saved successfully.');
            }).always(function () {
                abp.ui.clearBusy($cell);
            }).catch(function () {
                $ctrl.val(isNaN(oldValue) || oldValue === null ? '' : oldValue);
            });
        });
    }

    function reloadMainGrid() {
        truckRequestGrid.ajax.reload();
    }
})();
