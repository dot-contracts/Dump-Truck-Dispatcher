(function ($) {
    app.modals.CreateOrEditScheduledReportModal = function () {

        var _modalManager;
        var _scheduledReportService = abp.services.app.scheduledReport;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();
            $.validator.addMethod(
                "regex",
                function (value, element, regexp) {
                    var re = new RegExp(regexp);
                    return this.optional(element) || re.test(value);
                },
                "Please check your input."
            );
            _$form.find('#SendTo').rules('add', { regex: app.regex.emails });

            _$form.find('#ReportType').select2Init({
                showAll: true,
                allowClear: false
            });
            _$form.find('#ReportFormat').select2Init({
                showAll: true,
                allowClear: false
            });
            _$form.find('#SendOnDaysOfWeek').select2Init({
                showAll: true,
                allowClear: false
            });
            _$form.find("#ScheduleTime").timepickerInit({ stepping: 1 });

        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var model = _$form.serializeFormWithMultipleToObject();

            if (!$.isArray(model.SendOnDaysOfWeek)) {
                model.SendOnDaysOfWeek = [model.SendOnDaysOfWeek];
            }
            _modalManager.setBusy(true);
            _scheduledReportService.saveScheduledReport(model)
                .done(function (result) {
                    $('#Id').val(result.id);
                    abp.notify.info('Saved successfully.');
                    abp.event.trigger('app.createOrEditScheduledReportModalSaved');
                    _modalManager.close();
                }).always(function () {
                    _modalManager.setBusy(false);
                });
        };

    };
})(jQuery);