(function ($) {
    app.modals.SetDispatchTimeOnJobModal = function () {

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
                var result = await _dispatchingAppService.setDispatchTimeOnJob(formData);

                abp.notify.info('Time on Job set successfully.');
                _modalManager.close();
                abp.event.trigger('app.setDispatchTimeOnJobModalSaved', result);
            } finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);
