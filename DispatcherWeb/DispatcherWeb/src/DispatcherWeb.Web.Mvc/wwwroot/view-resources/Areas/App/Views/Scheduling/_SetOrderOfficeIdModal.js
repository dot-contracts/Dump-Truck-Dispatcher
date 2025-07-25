(function ($) {
    app.modals.SetOrderOfficeIdModal = function () {

        var _modalManager;
        var _orderService = abp.services.app.order;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find("#OfficeId").select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: false
            });

        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            if (formData.OfficeId === formData.OriginalOfficeId) {
                abp.message.error('The order cannot be transferred to the office that is already associated with the order');
                return;
            }

            _modalManager.setBusy(true);
            _orderService.setOrderOfficeId({
                orderId: formData.OrderId,
                officeId: formData.OfficeId,
                orderLineId: formData.TransferType == 1 ? formData.OrderLineId : null
            }).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.orderOfficeIdModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
