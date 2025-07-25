(function ($) {
    app.modals.CreateOrEditOrderInternalNotesModal = function () {

        var _modalManager;
        var _orderService = abp.services.app.order;
        var _$form = null;

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

            var formData = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _orderService.setOrderInternalNotes({
                orderId: formData.OrderId,
                internalNotes: formData.InternalNotes
            }).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.orderInternalNotesSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
