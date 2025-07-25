(function ($) {
    app.modals.EditOutOfServiceModal = function () {

        var _modalManager;
        var _dashboardService = abp.services.app.dashboard;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find('.datepicker').datetimepicker();

        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();


            if (!abp.helper.validateFutureDates(
                { value: $("#OutOfServiceDate").val(), title: $('label[for="OutOfServiceDate"]').text() }
            )) {
                return;
            }


            var data = {
                outOfHistoryId: formData.OutOfHistoryId,
                outOfServiceDate: formData.OutOfServiceDate,
                reason: formData.Reason
            };
            data.outOfServiceDate = new Date(data.outOfServiceDate);
            _modalManager.setBusy(true);
            _dashboardService.editOutOfService(data).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.editOutOfServiceModalSaved', data);
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);