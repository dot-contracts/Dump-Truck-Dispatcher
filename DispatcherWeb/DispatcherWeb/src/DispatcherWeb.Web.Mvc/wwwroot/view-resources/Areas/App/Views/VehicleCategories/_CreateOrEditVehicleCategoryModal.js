(function ($) {
    app.modals.CreateOrEditVehicleCategoryModal = function () {

        var _modalManager;
        var _vehicleCategoryService = abp.services.app.vehicleCategory;

        var _modal = null;
        var _$form = null;

        var _idFieldHiddenInput = null;
        var _nameFieldInput = null;
        var _assetTypeInput = null;
        var _isPoweredInput = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modal = _modalManager.getModal();
            _$form = _modal.find('form');
            _$form.validate();

            _idFieldHiddenInput = _$form.find("#Id");
            _nameFieldInput = _$form.find('#Name');
            _assetTypeInput = _$form.find('#AssetType');
            _isPoweredInput = _$form.find('#IsPowered');

            _$form.find("#AssetType").select2Init({
                showAll: true,
                allowClear: false
            });

        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                throw new Error("Form is not valid");
            }

            var vehicleCategory = _$form.serializeFormToObject();

            try {
                abp.ui.setBusy(_$form);
                _modalManager.setBusy(true);
                var result = await _vehicleCategoryService.editVehicleCategory(vehicleCategory);
                abp.notify.info('Saved successfully.');
                abp.event.trigger('app.CreateOrEditVehicleCategoryModalSaved', {
                    item: result
                });
                _modalManager.close();
            } finally {
                abp.ui.clearBusy(_$form);
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);
