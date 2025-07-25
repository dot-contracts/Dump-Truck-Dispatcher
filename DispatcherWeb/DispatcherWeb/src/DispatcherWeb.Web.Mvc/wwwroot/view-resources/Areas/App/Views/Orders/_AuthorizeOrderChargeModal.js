(function ($) {
    app.modals.AuthorizeOrderChargeModal = function () {

        var _modalManager;
        var _orderPaymentService = abp.services.app.orderPayment;
        var _$form = null;
        var _creditCardTokenInput = null;
        var _newCreditCardTempTokenInput = null;
        var _formData = null;
        var _cardForm = null;

        var authorizeOrderChargeAsync = function (callback) {
            _formData = _$form.serializeFormToObject();
            _orderPaymentService.authorizeOrderCharge({
                orderId: _formData.OrderId,
                creditCardToken: _formData.CreditCardToken,
                newCreditCardTempToken: _formData.NewCreditCardTempToken,
                streetAddress: _formData.StreetAddress,
                zipCode: _formData.ZipCode,
                authorizationAmount: _formData.AuthorizationAmount,
                saveCreditCardForFutureUse: _$form.find('#SaveCreditCardForFutureUse[type=checkbox]').prop('checked') //_formData.SaveCreditCardForFutureUse
            }).done(function (data) {
                abp.notify.info('Authorized successfully.');
                _modalManager.close();
                abp.event.trigger('app.authorizedOrderChargeModal', data);
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _creditCardTokenInput = _$form.find('#CreditCardToken');
            _newCreditCardTempTokenInput = _$form.find('#NewCreditCardTempToken');

            var amountInput = _$form.find('#AuthorizationAmount');
            if (amountInput.val() !== "") {
                amountInput.val(abp.utils.round(amountInput.val()).toFixed(2));
            }

            var saveButton = _modalManager.getModal().find('.save-button');
            saveButton.find('span').text('OK');
            saveButton.find('i.fa-save').removeClass('fa-save').addClass('fa-credit-card');

            _modalManager.getModal().on('shown.bs.modal', async function () {
                if (_creditCardTokenInput.val() !== '') {
                    if (await abp.message.confirm(
                        'Do you want to use this card? If you choose ‘Cancel’, you will be able to add a new card.',
                        'This customer has a credit card on file.'
                    )) {
                        _$form.find('#CardFields').slideUp();
                    } else {
                        _creditCardTokenInput.val('');
                    }
                }
            });

            _modalManager.onClose(function () {
                $("#secure-payment-styles-default").remove();
            });

            // Configure account
            GlobalPayments.configure({
                publicApiKey: HeartlandPublicKey
            });

            // Create Form
            _cardForm = GlobalPayments.creditCard.form("#credit-card");
            //hide the submit button
            $(_cardForm.fields.submit.target).hide();

            function refreshIframeHeight() {
                _$form.find('#CardFields iframe').each(function () {
                    $(this).css('height', '51px');
                });
            }

            _cardForm.ready(function () {
                setTimeout(refreshIframeHeight, 0);
                setTimeout(refreshIframeHeight, 200);
            });

            _cardForm.on("token-success", (resp) => {
                abp.log.debug(resp);
                _newCreditCardTempTokenInput.val(resp.paymentReference);
                authorizeOrderChargeAsync();
            });

            _cardForm.on("token-error", (resp) => {
                abp.log.debug(resp);
                _modalManager.setBusy(false);
                var message = '';
                if (resp.error && resp.reasons && resp.reasons.length) {
                    message = resp.reasons[0].message;
                }
                abp.message.error(message, 'There was an error on trying to tokenize the card:');
            });

        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            if (!HeartlandPublicKey) {
                abp.message.error('', 'The Heartland public key is not set for the office.');
                return;
            }

            _modalManager.setBusy(true);

            if (_creditCardTokenInput.val() !== '') {
                //already have a card token
                authorizeOrderChargeAsync();
            } else {
                //need to request a new token first
                // Tell the iframes to tokenize the data
                _cardForm.requestDataFromAll(_cardForm.frames['card-number']);
            }

        };
    };
})(jQuery);
