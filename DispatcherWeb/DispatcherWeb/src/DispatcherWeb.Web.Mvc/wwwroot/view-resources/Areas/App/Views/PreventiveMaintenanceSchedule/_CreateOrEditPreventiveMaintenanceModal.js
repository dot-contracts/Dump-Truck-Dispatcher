(function ($) {
    app.modals.CreateOrEditPreventiveMaintenanceModal = function () {

        var _modalManager;
        var _preventiveMaintenanceService = abp.services.app.preventiveMaintenance;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find('#LastMileage').rules('add', { mileage: true });
            _$form.find('#DueMileage').rules('add', { mileage: true });
            _$form.find('#WarningMileage').rules('add', { mileage: true });

            _$form.find('#LastDate').datepickerInit();
            _$form.find('#DueDate').datepickerInit();
            _$form.find('#WarningDate').datepickerInit();

            var $truckSelect = _$form.find('#TruckId').select2Init({
                abpServiceMethod: abp.services.app.truck.getTrucksSelectList,
                abpServiceParams: {
                    allOffices: true,
                    inServiceOnly: true
                },
                showAll: false,
                allowClear: true
            });

            var $vehicleServiceSelect = _$form.find("#VehicleServiceId").select2Init({
                abpServiceMethod: abp.services.app.vehicleService.getSelectList,
                showAll: false,
                allowClear: true
            });

            _$form.find('#VehicleServiceId, #TruckId').on('change', function () {
                fillDefaultValues();
            });

            function fillDefaultValues() {
                var vehicleServiceId = $vehicleServiceSelect.val();
                var truckId = $truckSelect.val();
                if (vehicleServiceId && truckId) {
                    _preventiveMaintenanceService.getDefaultValues(vehicleServiceId, truckId).done(function (data) {
                        setDateDefaultValue(_$form.find('#LastDate'), data.lastDate);
                        setIntDefaultValue(_$form.find('#LastMileage'), data.lastMileage);
                        setDateDefaultValue(_$form.find('#DueDate'), data.dueDate);
                        setIntDefaultValue(_$form.find('#DueMileage'), data.dueMileage);
                        setDateDefaultValue(_$form.find('#WarningDate'), data.warningDate);
                        setIntDefaultValue(_$form.find('#WarningMileage'), data.warningMileage);
                        setIntDefaultValue(_$form.find('#LastHour'), data.lastHour);
                        setIntDefaultValue(_$form.find('#DueHour'), data.dueHour);
                        setIntDefaultValue(_$form.find('#WarningHour'), data.warningHour);
                    });
                }
            }
            function setDateDefaultValue($ctrl, value) {
                if (value) {
                    $ctrl.val(moment(value).utc().format('L'));
                }
            }
            function setIntDefaultValue($ctrl, value) {
                if (value && (!$ctrl.val() || abp.utils.round($ctrl.val()) === 0)) {
                    $ctrl.val(value);
                }
            }

            _modalManager.getModal().on('shown.bs.modal', function () {
                _$form.find('#select2-TruckId-container').parent().focus();
                //_$form.find("#LastDate").daterangepicker('hide');
            });

        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }
            var preventiveMaintenance = _$form.serializeFormToObject();

            if (!abp.helper.validateStartEndDates(
                { value: $("#WarningDate").val(), title: $('label[for="WarningDate"]').text() },
                { value: $("#DueDate").val(), title: $('label[for="DueDate"]').text() }

            )) {
                return;
            }

            if (!abp.helper.validateFutureDates(
                { value: $("#LastDate").val(), title: $('label[for="LastDate"]').text() }
            )) {
                return;
            }

            if (!preventiveMaintenance.DueDate && !preventiveMaintenance.DueMileage && !preventiveMaintenance.DueHour) {
                abp.message.warn('Either "Due Date" or "Due Mileage" or "Due Hours" are required.', 'Validation error');
                return;
            }
            if (!preventiveMaintenance.WarningDate && !preventiveMaintenance.WarningMileage && !preventiveMaintenance.WarningHour) {
                abp.message.warn('Either "Warning Date" or "Warning Mileage" or "Warning Hours" are required.', 'Validation error');
                return;
            }
            if (preventiveMaintenance.DueDate && !preventiveMaintenance.WarningDate || !preventiveMaintenance.DueDate && preventiveMaintenance.WarningDate) {
                showMessageBothAreRequired('Date');
                return;
            }
            if (preventiveMaintenance.DueMileage && !preventiveMaintenance.WarningMileage || !preventiveMaintenance.DueMileage && preventiveMaintenance.WarningMileage) {
                showMessageBothAreRequired('Mileage');
                return;
            }
            if (preventiveMaintenance.DueHour && !preventiveMaintenance.WarningHour || !preventiveMaintenance.DueHour && preventiveMaintenance.WarningHour) {
                showMessageBothAreRequired('Hour');
                return;
            }

            if (preventiveMaintenance.DueMileage && preventiveMaintenance.WarningMileage && parseFloat(preventiveMaintenance.DueMileage) < parseFloat(preventiveMaintenance.WarningMileage)) {
                abp.message.warn('The "Warning Mileage" should not be greater than the "Due Mileage".', 'Validation error');
                return;
            }

            if (preventiveMaintenance.DueHour && preventiveMaintenance.WarningHour && parseFloat(preventiveMaintenance.DueHour) < parseFloat(preventiveMaintenance.WarningHour)) {
                abp.message.warn('The "Warning Hour" should not be greater than the "Due Hour".', 'Validation error');
                return;
            }

            _modalManager.setBusy(true);
            _preventiveMaintenanceService.save(preventiveMaintenance).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditPreventiveMaintenanceModal');
            }).always(function () {
                _modalManager.setBusy(false);
            });

            function showMessageBothAreRequired(name) {
                abp.message.warn('Both "Due ' + name + '" and "Warning ' + name + '" are required.', 'Validation error');
            }

        };

    };
})(jQuery);
