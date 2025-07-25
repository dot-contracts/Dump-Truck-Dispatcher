(function ($) {
    app.modals.EmailInvoicePrintOutModal = function () {

        var _modalManager;
        var _invoiceAppService = abp.services.app.invoice;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();
            $.validator.addMethod(
                "regex",
                function (value, element, regexp) {
                    var re = new RegExp(regexp, 'i');
                    return this.optional(element) || re.test(value);
                },
                "Please check your input."
            );
            _$form.find('#From').rules('add', { regex: app.regex.email });
            _$form.find('#To').rules('add', { regex: app.regex.emails });
            _$form.find('#CC').rules('add', { regex: app.regex.emails });

            var saveButton = _modalManager.getModal().find('.save-button');
            saveButton.find('span').text('Send');
            saveButton.find('i.fa-save').removeClass('fa-save').addClass('fa-envelope-o');
        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            try {
                _modalManager.setBusy(true);
                var result = await _invoiceAppService.emailInvoicePrintOut(formData);
                if (result.success) {
                    abp.notify.info('Sent successfully.');
                    _modalManager.close();
                    abp.event.trigger('app.emailInvoicePrintOutModalSent');
                } else if (result.fromEmailAddressIsNotVerifiedError) {
                    var supportRequestLink = abp.setting.get('App.HostManagement.SupportRequestLink');
                    abp.message.error(
                        'This domain and email address are not verified. You must be verified to use this email functionality. If you want to use this functionality, please send a <a href="'
                            + supportRequestLink + '" target="blank">support request</a>.',
                        'Error',
                        {
                            isHtml: true,
                        }
                    );
                }
            } finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);
