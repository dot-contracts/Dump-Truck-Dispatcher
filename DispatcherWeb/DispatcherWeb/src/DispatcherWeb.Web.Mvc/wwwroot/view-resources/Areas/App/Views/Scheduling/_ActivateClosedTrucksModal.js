(function ($) {
    app.modals.ActivateClosedTrucksModal = function () {

        var _modalManager;
        var _schedulingService = abp.services.app.scheduling;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find("#TruckIds").select2Init({ allowClear: false });

        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();
            _modalManager.setBusy(true);
            _schedulingService.activateClosedTrucks({
                orderLineId: formData.OrderLineId,
                truckIds: _$form.find("#TruckIds").val()
            }).done(function (result) {
                if (result) {
                    abp.notify.info('Activated successfully.');
                } else {
                    abp.notify.warn('Some trucks were not activated because they are fully utilized.');
                }
                _modalManager.close();
                abp.event.trigger('app.activateClosedTrucksModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });

        };
    };
})(jQuery);
