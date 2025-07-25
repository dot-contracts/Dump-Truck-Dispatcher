(function ($) {
    app.modals.AddEmployeeTimeEntryModal = function () {

        var _modalManager;
        var _modal;
        var _$form = null;

        var _employeeTimeService = abp.services.app.employeeTime;
        var _timeClassificationService = abp.services.app.timeClassification;

        this.init = function (modalManager) {

            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _modal = _modalManager.getModal();

            _modal.find("#StartDate").datepickerInit();
            _modal.find("#TimeClassificationId").select2Init({
                abpServiceMethod: _timeClassificationService.getTimeClassificationsSelectList,
                abpServiceParams: {
                    excludeProductionPay: false,
                    employeeId: _modal.find("#EmployeeId").val(),
                    allowForManualTime: true
                },
                showAll: false,
                allowClear: true
            }).on("change", function (e) {
                _timeClassificationId = $(this).val();
            });
        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            try {
                var timeOffEditData = _$form.serializeFormToObject();
                var employeeTime = {
                    id: null,
                    employeeId: timeOffEditData.EmployeeId,
                    startDateTime: timeOffEditData.StartDate,
                    endDateTime: timeOffEditData.StartDate,
                    manualHourAmount: timeOffEditData.RequestedHours,
                    timeClassificationId: timeOffEditData.TimeClassificationId,
                    timeOffId: timeOffEditData.Id
                };
                _modalManager.setBusy(true);
                await _employeeTimeService.editEmployeeTime(employeeTime);
                abp.notify.info('Employee time entry was saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.addEmployeeTimeEntryModalSaved');
            } finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);
