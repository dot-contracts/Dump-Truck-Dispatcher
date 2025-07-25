(function ($) {
    app.modals.ReassignTrucksModal = function () {

        var _modalManager;
        var _schedulingService = abp.services.app.scheduling;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find('#DestinationOrderLineId').select2Init({
                abpServiceMethod: abp.services.app.scheduling.getOrderLinesToAssignTrucksToSelectList,
                abpServiceParams: { id: _$form.find('#OrderLineId').val() },
                showAll: true,
                dropdownParent: $("#" + _modalManager.getModalId())
            });
            _$form.find('#TruckIds').select2Init({ allowClear: false });
        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();
            var truckIds = _$form.find('#TruckIds').val();

            try {
                _modalManager.setBusy(true);
                let trucksHaveDispatchesResult = await abp.services.app.scheduling.someOrderLineTrucksHaveDispatches({
                    orderLineId: formData.OrderLineId,
                    truckIds: truckIds
                });

                if (trucksHaveDispatchesResult.acknowledgedOrLoaded) {
                    abp.message.warn('One or more truck has an acknowledged or loaded dispatch. You must first complete or cancel the dispatch to reassign this truck.');
                    return;
                }
                if (trucksHaveDispatchesResult.unacknowledged) {
                    var truckText = truckIds.length === 1 ? 'this truck' : 'these trucks';
                    if (!await abp.message.confirm('This order line has open dispatches. If you reassign ' + truckText + ', these dispatches will be removed. Are you sure you want to do this?')) {
                        return;
                    }
                }

                _modalManager.setBusy(true);
                var input = {
                    sourceOrderLineId: formData.OrderLineId,
                    destinationOrderLineId: formData.DestinationOrderLineId,
                    truckIds: truckIds
                };
                await _schedulingService.reassignTrucks(input);

                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.reassignModalSaved');
            }
            finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);