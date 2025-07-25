(function ($) {
    app.modals.CreateOrEditTaxRateModal = function () {
        var _modalManager;
        var _taxRateService = abp.services.app.taxRate;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var taxRate = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _taxRateService.editTaxRate(taxRate).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditTaxRateModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
