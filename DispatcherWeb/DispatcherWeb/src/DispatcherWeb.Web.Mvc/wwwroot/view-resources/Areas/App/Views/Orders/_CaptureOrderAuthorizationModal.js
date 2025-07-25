(function ($) {
    app.modals.CaptureOrderAuthorizationModal = function () {

        var _modalManager;
        var _orderPaymentService = abp.services.app.orderPayment;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            var saveButton = _modalManager.getModal().find('.save-button');
            saveButton.find('span').text('OK');
            saveButton.find('i.fa-save').removeClass('fa-save').addClass('fa-credit-card');

        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _orderPaymentService.captureOrderAuthorization({
                receiptId: formData.ReceiptId,
                actualAmount: formData.ActualAmount
            }).done(function (data) {
                abp.notify.info('Finalized successfully.');
                _modalManager.close();
                abp.event.trigger('app.capturedOrderAuthorizationModal', data);
            }).always(function () {
                _modalManager.setBusy(false);
            });

        };
    };
})(jQuery);
