(function ($) {
    app.modals.CreateOrEditPricingTierModal = function () {
        let _modalManager;
        let _pricingTierService = abp.services.app.pricingTier;
        let _$form = null;

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

            var pricingTier = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _pricingTierService.editPricingTier(pricingTier).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditPricingTierModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
