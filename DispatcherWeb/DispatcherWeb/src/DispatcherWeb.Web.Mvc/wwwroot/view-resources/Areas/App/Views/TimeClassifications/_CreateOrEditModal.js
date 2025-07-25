(function ($) {
    app.modals.CreateOrEditTimeClassificationModal = function () {

        var _modalManager;
        var _timeClassificationService = abp.services.app.timeClassification;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find("#IsProductionBased").change(function () {
                var isDefaultRateVisible = !$(this).is(':checked');
                var defaultRateInput = _$form.find("#DefaultRate");
                defaultRateInput.closest('.form-group').toggle(isDefaultRateVisible);
                if (!isDefaultRateVisible) {
                    defaultRateInput.val('');
                }
            }).change();
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var timeClassification = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _timeClassificationService.editTimeClassification(timeClassification).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditTimeClassificationModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
