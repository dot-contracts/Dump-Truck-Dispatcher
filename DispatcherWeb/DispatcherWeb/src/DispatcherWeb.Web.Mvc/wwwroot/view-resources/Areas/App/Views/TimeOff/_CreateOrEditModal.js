(function ($) {
    app.modals.CreateOrEditTimeOffModal = function () {

        var _modalManager;
        var _timeOffService = abp.services.app.timeOff;
        var _driverAssignmentService = abp.services.app.driverAssignment;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            var modal = _modalManager.getModal();

            modal.find("#StartDate").datepickerInit();
            modal.find("#EndDate").datepickerInit();

            modal.find("#DriverId").select2Init({
                abpServiceMethod: _timeOffService.getDriversSelectList,
                showAll: false,
                allowClear: true
            });
        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var timeOff = _$form.serializeFormToObject();

            if (timeOff.EndDate && !abp.helper.validateStartEndDates(
                { value: timeOff.StartDate, title: _$form.find('label[for="StartDate"]').text() },
                { value: timeOff.EndDate, title: _$form.find('label[for="EndDate"]').text() }
            )) {
                return;
            }

            try {
                _modalManager.setBusy(true);
                let openDispatchesResult = await _driverAssignmentService.thereAreOpenDispatchesForDriverOnDate({
                    driverId: timeOff.DriverId,
                    startDate: timeOff.StartDate,
                    endDate: timeOff.EndDate
                });

                if (openDispatchesResult.thereAreAcknowledgedDispatches) {
                    if (!await abp.message.confirm('There are acknowledged dispatches for this truck. Unacknowledged dispatches will be canceled, but you will need to deal with the acknowledged dispatch. Are you sure you want to do this?')) {
                        return;
                    }
                } else if (openDispatchesResult.thereAreUnacknowledgedDispatches) {
                    if (!await abp.message.confirm('There are open dispatches for this truck. Removing the driver will cancel these dispatches. Are you sure you want to do this?')) {
                        return;
                    }
                }

                _modalManager.setBusy(true);
                let editResult = await _timeOffService.editTimeOff(timeOff);
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                if (editResult.truckWasRemovedFromOrders) {
                    abp.message.info('There were orders associated with this truck. The truck has been removed from those orders.');
                }
                abp.event.trigger('app.createOrEditTimeOffModalSaved');
            } finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);
