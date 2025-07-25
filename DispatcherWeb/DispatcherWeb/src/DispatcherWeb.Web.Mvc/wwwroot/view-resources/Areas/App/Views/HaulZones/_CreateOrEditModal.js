(function ($) {
    app.modals.CreateOrEditHaulZoneModal = function () {

        var _modalManager;
        var _haulZoneService = abp.services.app.haulZone;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            const modal = _modalManager.getModal();
            _$form = modal.find('form[name="HaulZoneForm"]');
            _$form.validate();

            let uomBaseIds = [...new Set([
                abp.setting.getInt('HaulZone.HaulRateCalculation.BaseUomIdForCod'),
                abp.setting.getInt('HaulZone.HaulRateCalculation.BaseUomId'),
            ])];
            if (uomBaseIds.length === 1 && uomBaseIds[0] === 0) {
                abp.message.warn('Please set the haul zone UOMs in the settings.');
            }

            _$form.find("#UnitOfMeasureId").select2Uom({
                abpServiceParams: {
                    uomBaseIds,
                },
            });
        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            let haulZone = _$form.serializeFormToObject();
            haulZone.IsActive = _$form.find("#IsActive").is(":checked");

            try {
                _modalManager.setBusy(true);
                let result = await _haulZoneService.editHaulZone(haulZone);

                abp.notify.info('Saved successfully.');
                _modalManager.setResult(result);
                _modalManager.close();
                abp.event.trigger('app.createOrEditHaulZoneModalSaved', result);
            } finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);
