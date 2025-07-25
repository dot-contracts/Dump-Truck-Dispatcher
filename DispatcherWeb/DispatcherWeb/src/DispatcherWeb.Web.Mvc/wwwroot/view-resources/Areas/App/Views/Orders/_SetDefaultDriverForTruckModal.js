(function ($) {
    app.modals.SetDefaultDriverForTruckModal = function () {

        var _modalManager;
        var _driverAssignmentService = abp.services.app.driverAssignment;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            var dateInput = _$form.find("#Date");
            var startDateInput = _$form.find("#StartDate");
            var endDateInput = _$form.find("#EndDate");
            var dateToday = new Date();

            dateInput.val(startDateInput.val() + ' - ' + endDateInput.val());

            dateInput.daterangepicker({
                autoUpdateInput: false,
                minDate: dateToday,
                locale: {
                    cancelLabel: 'Cancel'
                }
            }, function (start, end, label) {
                startDateInput.val(start.format('MM/DD/YYYY'));
                endDateInput.val(end.format('MM/DD/YYYY'));
            });

            dateInput.on('apply.daterangepicker', function (ev, picker) {
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
                startDateInput.val(picker.startDate.format('MM/DD/YYYY'));
                endDateInput.val(picker.endDate.format('MM/DD/YYYY'));
            });

            dateInput.on('cancel.daterangepicker', function (ev, picker) {
            });

            _$form.find('#Shift').select2Init({ allowClear: false });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _driverAssignmentService.setDefaultDriverForTruck({
                truckId: formData.TruckId,
                startDate: formData.StartDate,
                endDate: formData.EndDate,
                shift: formData.Shift
            }).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.defaultDriverForTruckModalSet');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
