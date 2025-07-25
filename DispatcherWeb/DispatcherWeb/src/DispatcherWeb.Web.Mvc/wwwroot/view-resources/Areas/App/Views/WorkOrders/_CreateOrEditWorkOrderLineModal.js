(function ($) {
    app.modals.CreateOrEditWorkOrderLineModal = function () {

        var _modalManager;
        var _workOrderService = abp.services.app.workOrder;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find("#VehicleServiceId").select2Init({
                abpServiceMethod: abp.services.app.vehicleService.getSelectList,
                showAll: false,
                allowClear: true
            });

            let $laborTime = _$form.find("#LaborTime");
            let $laborRate = _$form.find("#LaborRate");
            let $laborCost = _$form.find("#LaborCost");

            $laborTime.change(calculateLaborCost);
            $laborRate.change(calculateLaborCost);
            calculateLaborCost();

            function calculateLaborCost() {
                let laborTime = $laborTime.val();
                let laborRate = $laborRate.val();
                if (!laborTime || !laborRate) {
                    $laborCost.prop('disabled', false);
                    return;
                }
                $laborCost.prop('disabled', true);
                $laborCost.val(abp.utils.round(abp.utils.round(laborTime) * abp.utils.round(laborRate)));
            }
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var workOrderLine = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _workOrderService.saveWorkOrderLine(workOrderLine).done(function (result) {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditWorkOrderLineModalSaved', result);
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);