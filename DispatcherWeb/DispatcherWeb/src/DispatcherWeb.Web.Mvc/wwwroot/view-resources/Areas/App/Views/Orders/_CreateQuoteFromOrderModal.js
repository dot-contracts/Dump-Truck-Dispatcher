(function ($) {
    app.modals.CreateQuoteFromOrderModal = function () {

        var _modalManager;
        var _quoteService = abp.services.app.quote;
        var _$form = null;

        var _createOrEditQuoteModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Quotes/CreateOrEditQuoteModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Quotes/_CreateOrEditQuoteModal.js',
            modalClass: 'CreateOrEditQuoteModal',
            modalSize: 'xxl',
        });

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _modalManager.getModal().find('.save-button').text('OK');

            _$form = _modalManager.getModal().find('form');
            _$form.validate();
        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            try {
                _modalManager.setBusy(true);

                let newQuoteId = await _quoteService.createQuoteFromOrder(formData);
                abp.notify.info('Created successfully.');
                _modalManager.close();
                abp.event.trigger('app.quoteCreatedFromOrderModal', {
                    newQuoteId: newQuoteId
                });
                if (await abp.message.confirmWithOptions({
                    text: '',
                    title: 'The new quote has been created. Do you want us to open the quote for you?',
                    buttons: ['No', 'Yes']
                })) {
                    _createOrEditQuoteModal.open({ id: newQuoteId });
                }
            }
            finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);
