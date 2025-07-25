(function ($) {
    app.modals.EmailOrPrintApprovedInvoicesModal = function () {

        var _modalManager;
        var _invoiceAppService = abp.services.app.invoice;
        var _$form = null;
        var _saveButton;

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
            _$form.find('#CC').rules('add', { regex: app.regex.emails });

            _saveButton = _modalManager.getModal().find('.save-button');
            _saveButton.find('span').text('Send');
            _saveButton.find('i.fa-save').removeClass('fa-save').addClass('fa-envelope-o');
        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            try {
                _modalManager.setBusy(true);
                var result = await _invoiceAppService.hasApprovedInvoices();
                if (result.hasApprovedInvoicesToEmail) {
                    await _invoiceAppService.enqueueEmailApprovedInvoicesJob(formData);
                    abp.notify.info('Emails are scheduled to be sent.');
                    abp.event.trigger('app.emailOrPrintApprovedInvoicesModalSaved');
                } else {
                    abp.message.warn('There are no invoices to send');
                }
                _modalManager.setResult({});
                _modalManager.close();
            } finally {
                _modalManager.setBusy(false);
            }
        };

    };

})(jQuery);
