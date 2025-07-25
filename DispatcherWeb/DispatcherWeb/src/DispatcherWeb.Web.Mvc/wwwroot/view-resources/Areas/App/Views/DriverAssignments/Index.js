'use strict';
(function () {
    $(function () {

        var _driverAssignmentService = abp.services.app.driverAssignment;
        var _dtHelper = abp.helper.dataTables;
        var _date, _shift, _officeId;
        var allowSubcontractorsToDriveCompanyOwnedTrucks = abp.setting.getBoolean('App.LeaseHaulers.AllowSubcontractorsToDriveCompanyOwnedTrucks');

        $("#DateFilter").val(moment().format("MM/DD/YYYY"));

        $("#DateFilter").daterangepicker({
            singleDatePicker: true,
            locale: {
                format: "MM/DD/YYYY"
            }
        });

        $('#DateFilter').on('change', function () {
            if (isPastDate()) {
                $('#NotifyDrivers').hide();
            } else {
                $('#NotifyDrivers').show();
            }
            refreshButtons();
        });

        function isPastDate() {
            var isPastDate = moment($("#DateFilter").val(), 'MM/DD/YYYY') < moment().startOf('day');
            return isPastDate;
        }

        var $shiftSelect = $('#ShiftFilter').select2Init({
            showAll: true,
            allowClear: false
        });
        $shiftSelect.on('change', function () {
            _shift = Number($shiftSelect.val());
            refreshButtons();
        });
        function refreshButtons() {
            if (isPastDate() || _shift === abp.enums.shifts.noShift) {
                $('#AddUnscheduledTrucks').attr("disabled", "disabled");
            } else {
                $('#AddUnscheduledTrucks').removeAttr("disabled");
            }
            if (!thereAreAssignments() || _shift === abp.enums.shifts.noShift) {
                $('#AddDefaultStartTime, #PrintDriverAssignment, #NotifyDrivers').attr('disabled', 'disabled');
            } else {
                $('#AddDefaultStartTime, #PrintDriverAssignment, #NotifyDrivers').removeAttr('disabled');
                disableNotifyDriversButtonIfThereAreNoDriversToNotify();
            }
            function disableNotifyDriversButtonIfThereAreNoDriversToNotify() {
                if (!$("#DateFilter").val() /* It's empty when the clear button is pressed */) {
                    return;
                }
                var formData = $('form').serializeFormToObject();
                abp.ui.setBusy($('#NotifyDrivers'));
                abp.services.app.notifyDrivers.thereAreDriversToNotify(formData)
                    .done(function (result) {
                        if (!result) {
                            $('#NotifyDrivers').attr('disabled', 'disabled');
                        }
                    })
                    .always(function () {
                        abp.ui.clearBusy($('#NotifyDrivers'));
                    });
            }
        }

        updateDateShiftFields();

        $("#OfficeIdFilter").select2Init({
            abpServiceMethod: listCacheSelectLists.office(),
            showAll: true,
            allowClear: true
        });
        abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), abp.session.officeId, abp.session.officeName);

        $('#OfficeIdFilter').change(function () {
            _officeId = $(this).val();
        })

        var driverAssignmentsTable = $('#DriverAssignmentsTable');
        var driverAssignmentsGrid = driverAssignmentsTable.DataTableInit({
            //paging: false,
            //info: false,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _driverAssignmentService.getDriverAssignments(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                    refreshButtons();
                });
            },
            editable: {
                saveCallback: async function (rowData, cell) {
                    await editDriverAssignment(rowData);
                },
                isReadOnly: function (rowData) {
                    return isPastDate();
                }
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
                    data: "truckCode",
                    title: "Truck Number",
                    width: "20px"
                },
                {
                    data: "driverName",
                    title: "Driver",
                    width: "200px",
                    className: "all",
                    editable: {
                        editor: _dtHelper.editors.dropdown,
                        idField: 'driverId',
                        nameField: 'driverName',
                        dropdownOptions: {
                            abpServiceMethod: abp.services.app.driver.getDriversSelectList,
                            abpServiceParamsGetter: (params, rowData) => ({
                                officeId: allowSubcontractorsToDriveCompanyOwnedTrucks || rowData.truckLeaseHaulerId ? null : _officeId,
                                includeLeaseHaulerDrivers: allowSubcontractorsToDriveCompanyOwnedTrucks || !!rowData.truckLeaseHaulerId,
                                leaseHaulerId: rowData.truckLeaseHaulerId,
                            }),
                            showAll: true,
                            allowClear: true
                        },
                        validate: async (rowData, newValue, cell) => {
                            if (!rowData.driverId) {
                                return true;
                            }
                            var validationResult = await _driverAssignmentService.hasOrderLineTrucks({
                                driverId: rowData.driverId,
                                truckId: rowData.truckId,
                                ..._dtHelper.getFilterData()
                            });
                            if (validationResult.hasOrderLineTrucks) {
                                abp.ui.clearBusy(cell);
                                if (!newValue) {
                                    abp.message.warn(app.localize('CannotRemoveDriverBecauseOfOrderLineTrucksError'));
                                    return false;
                                } else {
                                    let userResponse = await swal(
                                        app.localize("DriverAlreadyScheduledForTruck{0}Prompt_YesToReplace_NoToCreateNew", rowData.truckCode),
                                        {
                                            buttons: {
                                                no: "No",
                                                yes: "Yes"
                                            }
                                        }
                                    );
                                    abp.ui.setBusy(cell);
                                    if (userResponse === 'no') {
                                        rowData.id = 0;
                                        rowData.startTime = null;
                                    } else {
                                        if (validationResult.hasOpenDispatches) {
                                            abp.message.error(app.localize("CannotEditDriverAssignmentBecauseOfDispatchesError"));
                                            return false;
                                        }
                                    }
                                }
                            }
                            return true;
                        }
                    }
                },
                {
                    data: "firstTimeOnJob",
                    render: function (data, type, full, meta) {
                        return _dtHelper.renderTime(full.firstTimeOnJob, '') + (full.loadAtName ? ' at ' + _dtHelper.renderText(full.loadAtName) : '');
                    },
                    title: app.localize("FirstTimeOnJob"),
                    width: "20px",
                },
                {
                    data: "startTime",
                    title: "Start Time",
                    render: (data, type, full, meta) => _dtHelper.renderTime(full.startTime, ''),
                    width: "20px",
                    editable: {
                        editor: _dtHelper.editors.time
                    }
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    width: "10px",
                    defaultContent: '',
                    rowAction: {
                        items: [{
                            text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                            action: async function (data) {
                                var rowData = data.record;
                                rowData.isDelete = true;
                                rowData.driverId = null;
                                await editDriverAssignment(rowData);
                            }
                        }]
                    }
                },
            ]
        });

        async function editDriverAssignment(rowData) {
            try {
                var result = await _driverAssignmentService.editDriverAssignment(rowData);
                if (rowData.id && !result.reloadRequired) {
                    refreshButtons();
                } else {
                    setTimeout(() => {
                        reloadMainGrid();
                    }, 100);
                }
                return result;
            } catch {
                reloadMainGrid();
            }
        }

        function thereAreAssignments() {
            var gridData = driverAssignmentsGrid.rows().data();
            return gridData.toArray().some(x => x.driverId !== null);
        }

        function reloadMainGrid() {
            driverAssignmentsGrid.ajax.reload();
        }

        $("#PrintDriverAssignment").click(function (e) {
            e.preventDefault();
            if (!isFilterValid()) {
                return;
            }
            app.openPopup(abp.appPath + 'app/driverassignments/getreport?' + $("#filterForm").serialize());
        });

        $('#AddDefaultStartTime').click(function (e) {
            e.preventDefault();
            var formData = $('form').serializeFormToObject();
            abp.ui.setBusy($('#AddDefaultStartTime'));
            selectStartTimeModal(function (startTimeResult) {
                formData.DefaultStartTime = startTimeResult.StartTime;
                abp.ui.setBusy($('#AddDefaultStartTime'));
                _driverAssignmentService.addDefaultStartTime(formData)
                    .done(function () {
                        abp.notify.info('Start Time values added.');
                        reloadMainGrid();
                    })
                    .always(function () {
                        abp.ui.clearBusy($('#AddDefaultStartTime'));
                    });
            }).always(function () {
                abp.ui.clearBusy($('#AddDefaultStartTime'));
            });

        });

        $('#AddUnscheduledTrucks').click(function (e) {
            e.preventDefault();
            if (!isFilterValid()) {
                return;
            }
            var formData = $('form').serializeFormToObject();
            abp.ui.setBusy($('#AddUnscheduledTrucks'));
            selectStartTimeModal(function (startTimeResult) {
                formData.DefaultStartTime = startTimeResult.StartTime;
                abp.ui.setBusy($('#AddUnscheduledTrucks'));
                _driverAssignmentService.addUnscheduledTrucks(formData)
                    .done(function (result) {
                        abp.notify.info('Created ' + result + ' driver assignments.');
                        reloadMainGrid();
                    })
                    .always(function () {
                        abp.ui.clearBusy($('#AddUnscheduledTrucks'));
                    });
            }).always(function () {
                abp.ui.clearBusy($('#AddUnscheduledTrucks'));
            });
        });

        $('#NotifyDrivers').click(function (e) {
            e.preventDefault();
            if (!isFilterValid()) {
                return;
            }
            var formData = $('form').serializeFormToObject();
            abp.ui.setBusy($('#NotifyDrivers'));
            abp.services.app.notifyDrivers.notifyDrivers(formData)
                .done(function () {
                    abp.notify.info('Sending notifications to the drivers.');
                })
                .always(function () {
                    abp.ui.clearBusy($('#NotifyDrivers'));
                });
        });

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            updateDateShiftFields();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            $("#DateFilter").val(moment().format("MM/DD/YYYY"));
            reloadMainGrid();
        });

        function updateDateShiftFields() {
            _date = $('#DateFilter').val();
            _shift = $('#ShiftFilter').val();
            _officeId = $('#OfficeIdFilter').val();
        }

        function isFilterValid() {
            var $form = $('form');
            if (!$form.valid()) {
                $form.showValidateMessage();
                return false;
            }
            return true;
        }

        function selectStartTimeModal(callback) {
            var selectTimeModal = new app.ModalManager({
                viewUrl: abp.appPath + 'App/DriverAssignments/SelectTimeModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/DriverAssignments/_SelectTimeModal.js',
                modalClass: 'SelectTimeModal'
            });
            return selectTimeModal.open().then((modalManager, modalObject) => {
                modalObject.setSaveCallback(callback);
            });
        }

    });
})();
