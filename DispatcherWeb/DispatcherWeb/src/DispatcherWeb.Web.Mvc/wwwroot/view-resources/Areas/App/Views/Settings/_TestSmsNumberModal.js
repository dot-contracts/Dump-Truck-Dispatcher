(function ($) {
    app.modals.TestSmsNumberModal = function () {

        var _modalManager;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modalManager.getOptions().getDefaultFocusElement = function (modal) { return modal.find('#PhoneNumber'); };

            _$form = _modalManager.getModal().find('form');

            _$form.validate({
                rules: {
                    CountryCode: {
                        required: true,
                        digits: true
                    },
                    PhoneNumber: {
                        required: true,
                        digits: true
                    }
                }
            });

            _$form.find('#PhoneNumber').on('blur', function (e) {
                var value = $(this).val();
                $(this).val(value.replace(/[^0-9]/g, ""));
                _$form.valid();
            });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var model = _$form.serializeFormToObject();

            var testMethod;
            switch (model.TestPurpose) {
                case 'number':
                    testMethod = abp.services.app.driverMessage.testSmsNumber;
                    break;
                case 'message':
                    testMethod = abp.services.app.dispatching.testDriverDispatchSmsTemplate;
                    break;
                default:
                    return;
            }

            _modalManager.setBusy(true);
            testMethod(model).done(function () {
                abp.notify.info('Sent successfully.');
                _modalManager.close();
                abp.event.trigger('app.testSmsNumber');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);