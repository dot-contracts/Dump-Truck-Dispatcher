(function () {

    var _dtHelper = abp.helper.dataTables;
    var _leaseHaulerRequestListService = abp.services.app.leaseHaulerRequestList;
    var _leaseHaulerRequestEditService = abp.services.app.leaseHaulerRequestEdit;

    initFilterControls();

    var _createOrEditLeaseHaulerRequestModal = new app.ModalManager({
        viewUrl: abp.appPath + 'app/LeaseHaulerRequests/CreateOrEditLeaseHaulerRequestModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/LeaseHaulerRequests/_CreateOrEditLeaseHaulerRequestModal.js',
        modalClass: 'CreateOrEditLeaseHaulerRequestModal'
    });

    var _sendLeaseHaulerRequestModal = new app.ModalManager({
        viewUrl: abp.appPath + 'app/LeaseHaulerRequests/SendLeaseHaulerRequestModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/LeaseHaulerRequests/_SendLeaseHaulerRequestModal.js',
        modalClass: 'SendLeaseHaulerRequestModal'
    });

    var useShifts = abp.setting.getBoolean('App.General.UseShifts');

    var leaseHaulerRequestTable = $('#LeaseHaulerRequestsTable');
    var leaseHaulerRequestGrid = leaseHaulerRequestTable.DataTableInit({
        stateSave: true,
        stateDuration: 0,

        stateLoadCallback: function (settings, callback) {
            app.localStorage.getItem('leaseHaulerRequests_filter',
                function (result) {
                    var filter = result || {};

                    if (filter.dateRangeFilter) {
                        $('#DateRangeFilter').val(filter.dateRangeFilter);
                    } else {
                        resetDateRangeFilterToDefault();
                    }
                    if (filter.leaseHaulerId) {
                        abp.helper.ui.addAndSetDropdownValue($("#LeaseHaulerIdFilter"),
                            filter.leaseHaulerId,
                            filter.leaseHaulerName);
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

                    app.localStorage.getItem('leaseHaulerRequests_grid',
                        function (result) {
                            callback(JSON.parse(result));
                        });

                });
        },
        stateSaveCallback: function (settings, data) {
            delete data.columns;
            delete data.search;
            app.localStorage.setItem('leaseHaulerRequests_grid', JSON.stringify(data));
            app.localStorage.setItem('leaseHaulerRequests_filter', _dtHelper.getFilterData());
        },
        ajax: function (data, callback, settings) {
            var abpData = _dtHelper.toAbpData(data);
            $.extend(abpData, _dtHelper.getFilterData());
            $.extend(abpData, parseDate(abpData.dateRangeFilter));

            localStorage.setItem('leaseHaulerRequests_filter', JSON.stringify(abpData));

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
                title: 'Date' + (useShifts ? '/Shift' : '')
            },
            {
                data: 'leaseHauler',
                title: 'Lease Hauler'
            },
            {
                data: 'sent',
                render: function (data, type, full, meta) { return _dtHelper.renderDateShortTime(full.sent); },
                title: 'Sent'
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
                width: '80px',
                className: "cell-editable",
                render: function (data, type, full, meta) {
                    return '<input class="form-control" name="Available" type="text" value="' + (full.available === null ? '' : _dtHelper.renderText(full.available)) + '">';
                }
            },
            {
                data: 'approved',
                title: 'Approved',
                width: '80px',
                className: "cell-editable",
                render: function (data, type, full, meta) {
                    return '<input class="form-control" name="Approved" type="text" value="' + (full.approved === null ? '' : _dtHelper.renderText(full.approved)) + '">';
                }
            },
            {
                data: 'scheduled',
                title: 'Scheduled',
                width: '80px'
            },
            {
                responsivePriority: 1,
                data: null,
                orderable: false,
                autoWidth: false,
                width: "10px",
                defaultContent: '',
                rowAction: {
                    items: [{
                        text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                        visible: function () {
                            return true; //_permissions.edit;
                        },
                        action: function (data) {
                            _createOrEditLeaseHaulerRequestModal.open({ leaseHaulerRequestId: data.record.id });
                        }
                    }]
                }
            }
        ]
    });

    leaseHaulerRequestTable.on('draw.dt', function () {
        createInputControlsFocusOutHandler();
    });
    leaseHaulerRequestGrid.on('responsive-display', function (e, datatable, row, showHide, update) {
        if (showHide) {
            createInputControlsFocusOutHandler();
        }
    });

    function createInputControlsFocusOutHandler() {
        createInputFocusOutHandler(leaseHaulerRequestTable.find('input[name="Available"]'), 'available', _leaseHaulerRequestEditService.updateAvailable);
        createInputFocusOutHandler(leaseHaulerRequestTable.find('input[name="Approved"]'), 'approved', _leaseHaulerRequestEditService.updateApproved);
    }

    function createInputFocusOutHandler($inputCtrl, field, serviceMethod) {
        $inputCtrl.off('focusout').on('focusout', function () {
            var $ctrl = $(this);
            var $cell = $ctrl.closest('td');
            var rowData = _dtHelper.getRowData($cell[0]);
            var oldValue = rowData[field];
            var newValue = $ctrl.val();
            if (isNaN(newValue) || parseInt(newValue) < 0 || parseInt(newValue) > 1000 || parseInt(newValue).toString() !== newValue) {
                abp.message.error('Please enter a valid number!');
                $ctrl.val(oldValue);
                return;
            }
            newValue = newValue === '' ? null : parseInt(newValue);
            if (newValue === oldValue) {
                return;
            }
            var available = field === 'available' ? newValue : rowData.available;
            var approved = field === 'approved' ? newValue : rowData.approved;
            if (available !== null && approved !== null && available < approved || available === null && approved !== null) {
                abp.message.error('Approved must be less than or equal to available!');
                $ctrl.val(isNaN(oldValue) || oldValue === null ? '' : oldValue);
                return;
            }

            abp.ui.setBusy($cell);
            var input = {
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

    $('form').submit(function (event) {
        event.preventDefault();
        reloadMainGrid();
    });
    $("#ClearSearchButton").click(function () {
        $(this).closest('form')[0].reset();
        //setUserOffice();
        $('#LeaseHaulerIdFilter').val('').trigger('change');
        $('#ShiftFilter').val(0).trigger("change");
        resetDateRangeFilterToDefault();
        reloadMainGrid();
    });

    function resetDateRangeFilterToDefault() {
        $('#DateRangeFilter').val(moment().add(1, 'days').format("MM/DD/YYYY") + ' - ' + moment().add(1, 'days').format("MM/DD/YYYY"));
    }

    function setUserOffice() {
        abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"),
            abp.session.officeId,
            abp.session.officeName);
    }

    function reloadMainGrid() {
        leaseHaulerRequestGrid.ajax.reload();
    }

    function initFilterControls() {
        $("#DateRangeFilter").daterangepicker({
            locale: {
                cancelLabel: 'Clear'
            },
            showDropDown: true
        }).on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
        }).on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
        });

        $('#LeaseHaulerIdFilter').select2Init({
            abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulersSelectList,
            showAll: false,
            allowClear: true
        });

        $('#ShiftFilter').select2Init({
            showAll: true,
            allowClear: false
        });

        $("#OfficeIdFilter").select2Init({
            abpServiceMethod: listCacheSelectLists.office(),
            showAll: true,
            allowClear: true
        });
    }

    function parseDate(dateRangeString) {
        var dateObject = {};
        var dateStringArray;
        if (dateRangeString) {
            dateStringArray = dateRangeString.split(' - ');
            $.extend(dateObject, { dateBegin: abp.helper.parseDateToJsonWithoutTime(dateStringArray[0]), dateEnd: abp.helper.parseDateToJsonWithoutTime(dateStringArray[1]) });
        }
        return dateObject;
    }

})();
