(function ($) {
    app.modals.AddOutOfServiceReasonModal = function () {

        var _modalManager;
        var _truckService = abp.services.app.truck;
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

            var model = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _truckService.setTruckIsOutOfService({
                truckId: model.TruckId,
                isOutOfService: true,
                reason: model.Reason,
                date: model.Date
            }).done(function (result) {
                abp.notify.info('Saved successfully.');
                var message = '';
                message += result.thereWereAssociatedOrders ? app.localize('ThereWereOrdersAssociatedWithThisTruck') + '\n' : '';
                message += result.thereWereCanceledDispatches ? app.localize('ThereWereCanceledDispatches') + '\n' : '';
                message += result.thereWereNotCanceledDispatches ? app.localize('ThereWereNotCanceledDispatches') + '\n' : '';
                message += result.thereWereAssociatedTractors ? app.localize('ThereWasTractorAssociatedWithThisTrailer') + '\n' : '';
                if (message) {
                    abp.message.info(message, 'Message');
                }
                _modalManager.close();
                abp.event.trigger('app.addOutOfServiceReasonModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);