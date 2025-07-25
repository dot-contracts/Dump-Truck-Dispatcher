(function ($) {
    app.modals.AssignDriverForTruckModal = function () {

        var _modalManager;
        var _driverAssignmentService = abp.services.app.driverAssignment;
        var _leaseHaulerRequestEditService = abp.services.app.leaseHaulerRequestEdit;
        var _$form = null;
        var _originalDriverId = null;
        var _leaseHaulerId = null;
        var _alwaysShowOnSchedule = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _$form = _modalManager.getModal().find('form');
            var modalArgs = modalManager.getArgs();

            var allowSubcontractorsToDriveCompanyOwnedTrucks = abp.setting.getBoolean('App.LeaseHaulers.AllowSubcontractorsToDriveCompanyOwnedTrucks');
            var allowSchedulingTrucksWithoutDrivers = abp.setting.getBoolean('App.DispatchingAndMessaging.AllowSchedulingTrucksWithoutDrivers');

            if (allowSchedulingTrucksWithoutDrivers) {
                _$form.find('#DriverId').removeAttr('required');
                _$form.find('label[for="DriverId"').removeClass('required-label');
            }

            _$form.validate();
                        
            _$form.find("#Message").text(modalArgs.message || '');

            var driverIdDropdown = _$form.find("#DriverId");
            _alwaysShowOnSchedule = _$form.find('#AlwaysShowOnSchedule').val() === 'true';
            _leaseHaulerId = Number(_$form.find("#LeaseHaulerId").val()) || null;
            if (_leaseHaulerId) {
                driverIdDropdown.select2Init({
                    abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulerDriversSelectList,
                    abpServiceParams: {
                        leaseHaulerId: _leaseHaulerId,
                    },
                    allowClear: false,
                    showAll: true
                });
            } else {
                driverIdDropdown.select2Init({
                    abpServiceMethod: abp.services.app.driver.getDriversSelectList,
                    abpServiceParams: {
                        officeId: allowSubcontractorsToDriveCompanyOwnedTrucks ? null : _$form.find('#OfficeId').val(),
                        includeLeaseHaulerDrivers: allowSubcontractorsToDriveCompanyOwnedTrucks,
                    },
                    showAll: true
                });
            }

            _originalDriverId = driverIdDropdown.val();

        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var input = _$form.serializeFormToObject();

            try {
                _modalManager.setBusy(true);
                var isPastDate = moment(input.Date, 'MM/DD/YYYY') < moment().startOf('day');

                if (_originalDriverId) {
                    var validationResult = await _driverAssignmentService.hasOrderLineTrucks({
                        driverId: _originalDriverId,
                        truckId: input.TruckId,
                        officeId: input.OfficeId,
                        date: input.Date,
                        shift: input.Shift
                    });

                    if (_leaseHaulerId && !_alwaysShowOnSchedule) {
                        if (validationResult.hasOrderLineTrucks && validationResult.hasOpenDispatches) {
                            abp.message.error(app.localize("CannotChangeDriverBecauseOfDispatchesError"));
                            return;
                        }
                    } else {
                        if (isPastDate) {
                            input.CreateNewDriverAssignment = true;
                        } else if (validationResult.hasOrderLineTrucks) {
                            _modalManager.setBusy(false);
                            var userResponse = await swal(
                                app.localize("DriverAlreadyScheduledForTruck{0}Prompt_YesToReplace_NoToCreateNew", input.TruckCode),
                                {
                                    buttons: {
                                        no: "No",
                                        yes: "Yes"
                                    }
                                }
                            );
                            _modalManager.setBusy(true);
                            if (userResponse === 'no') {
                                input.CreateNewDriverAssignment = true;
                            } else {
                                if (validationResult.hasOpenDispatches) {
                                    abp.message.error(app.localize("CannotChangeDriverBecauseOfDispatchesError"));
                                    return;
                                }
                            }
                        }
                    }
                }
                if (_leaseHaulerId && !_alwaysShowOnSchedule) {
                    await _leaseHaulerRequestEditService.setDriverForLeaseHaulerTruck(input);
                } else {
                    await _driverAssignmentService.setDriverForTruck(input);
                }
                abp.notify.info('Saved successfully.');
                _modalManager.setResult({
                    driverId: Number(input.DriverId) || null,
                    driverName: _$form.find("#DriverId").getSelectedDropdownOption().text()
                });
                _modalManager.close();
                abp.event.trigger('app.assignDriverForTruckModalSaved');
            }
            finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);
