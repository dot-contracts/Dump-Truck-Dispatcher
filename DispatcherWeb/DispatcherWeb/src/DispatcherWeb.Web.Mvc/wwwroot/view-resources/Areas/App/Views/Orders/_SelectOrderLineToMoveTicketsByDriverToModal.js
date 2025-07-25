(function ($) {
    app.modals.SelectOrderLineToMoveTicketsByDriverToModal = function () {

        var _modalManager;
        var _$form = null;
        var _orderLineDropdown = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _orderLineDropdown = _$form.find('#SelectOrderLineModal_OrderLineId');

            _$form.validate();

            let deliveryDateInput = _$form.find('#DeliveryDate');
            deliveryDateInput.datepickerInit();

            let customerDropdown = _$form.find('#SelectOrderLineModal_CustomerId');
            customerDropdown.select2Init({
                abpServiceMethod: abp.services.app.customer.getActiveCustomersSelectList,
                showAll: false,
                allowClear: false
            });

            _orderLineDropdown.select2Init({
                abpServiceMethod: abp.services.app.order.getOrderLinesSelectListToMoveTicketsByDriverTo,
                abpServiceParamsGetter: (params) => ({
                    deliveryDate: deliveryDateInput.val(),
                    customerId: customerDropdown.val(),
                }),
                showAll: true,
                allowClear: false,
                //todo add formatter
            });
        };

        this.focusOnDefaultElement = function () {
            _orderLineDropdown.focus();
        }

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            let id = _orderLineDropdown.val();
            if (!id) {
                return;
            }

            var item = _orderLineDropdown.select2('data')?.[0]?.item;
            if (!item) {
                return;
            }

            _modalManager.setResult(item);
            _modalManager.close();
        };
    };
})(jQuery);
