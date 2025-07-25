(function ($) {
    app.modals.SetOrderDateModal = function () {

        var _modalManager;
        var _orderService = abp.services.app.order;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find("#Date").datepicker();

            _$form.find('#Shift').select2Init({ allowClear: false });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            var input = {
                orderId: formData.OrderId,
                date: formData.Date,
                shift: formData.Shift,
                keepTrucks: formData.KeepTrucks,
                orderLineId: formData.ChangeType == 1 ? formData.OrderLineId : null
            };
            _orderService.setOrderDate(input).done(function (result) {
                if (!result.completed) {
                    var useShifts = abp.setting.getBoolean('App.General.UseShifts');
                    abp.helper.showTruckWarning(result.notAvailableTrucks,
                        'already scheduled or ' + (result.notAvailableTrucks.length > 1 ? 'have' : 'has') + ' no driver for this date' + (useShifts ? '/shift' : '') + '. Do you want to continue with remaining trucks?',
                        function (isConfirmed) {
                            if (isConfirmed) {
                                input.removeNotAvailableTrucks = true;
                                _orderService.setOrderDate(input).done(function (result) {
                                    notifyAndClose();
                                });
                            }
                        }
                    );
                } else {
                    notifyAndClose();
                }

                function notifyAndClose() {
                    abp.notify.info('Saved successfully.');
                    _modalManager.close();
                    abp.event.trigger('app.orderDateModalSaved');
                }
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);