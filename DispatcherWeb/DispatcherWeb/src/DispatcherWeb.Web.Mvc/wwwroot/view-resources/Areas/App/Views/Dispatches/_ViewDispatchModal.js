(function ($) {
    app.modals.ViewDispatchModal = function () {

        var _modalManager;
        var _dispatchingAppService = abp.services.app.dispatching;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find("#TimeOnJob").timepickerInit({ stepping: 1 });


        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            try {
                _modalManager.setBusy(true);
                await _dispatchingAppService.editDispatch(formData);

                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.editDispatchModalSaved');
            } finally {
                _modalManager.setBusy(false);
            }
        };

    };
})(jQuery);
