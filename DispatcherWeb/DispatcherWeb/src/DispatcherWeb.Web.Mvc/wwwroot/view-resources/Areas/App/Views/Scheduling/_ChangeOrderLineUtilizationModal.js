(function ($) {
    app.modals.ChangeOrderLineUtilizationModal = function () {

        var _modalManager;
        var _schedulingService = abp.services.app.scheduling;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find(".tooltip-icon").tooltip();
        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            if (formData.Utilization > 0) {
                saveUtilization();
            } else {
                abp.scheduling.checkExistingDispatchesBeforeRemovingTrucks(
                    formData.OrderLineId,
                    //remove callback
                    function () {
                        saveUtilization();
                    },
                    //cancel callback
                    function () { },
                    //done callback
                    function () {
                        markAsDone();
                    }
                );
            }

            function saveUtilization() {
                _modalManager.setBusy(true);
                _schedulingService.changeOrderLineUtilization({
                    orderLineId: formData.OrderLineId,
                    utilization: formData.Utilization
                }).done(function () {
                    abp.notify.info('Saved successfully.');
                    _modalManager.close();
                    abp.event.trigger('app.truckUtilizationModalSaved');
                }).always(function () {
                    _modalManager.setBusy(false);
                });
            }

            function markAsDone() {
                _schedulingService.deleteOrderLineTrucks({
                    orderLineId: formData.OrderLineId,
                    markAsDone: true
                }).done(function () {
                    abp.notify.info('Mark as done successfully.');
                    _modalManager.close();
                    abp.event.trigger('app.truckUtilizationModalSaved');
                }).always(function () {
                    _modalManager.setBusy(false);
                });

            }
        };
    };
})(jQuery);
