(function ($) {
    app.modals.SendOrderLineToHaulingCompanyModal = function () {

        var _modalManager;
        var _schedulingService = abp.services.app.scheduling;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find("#LeaseHaulerId").select2Init({
                abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulersSelectList,
                abpServiceParams: {
                    hasHaulingCompanyTenantId: true
                },
                showAll: true,
                allowClear: false
            });

        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _schedulingService.sendOrderLineToHaulingCompany(formData).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.orderLineSentToHaulingCompany');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
