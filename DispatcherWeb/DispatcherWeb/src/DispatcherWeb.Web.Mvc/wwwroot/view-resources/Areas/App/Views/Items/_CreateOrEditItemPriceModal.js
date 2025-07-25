(function ($) {
    app.modals.CreateOrEditItemPriceModal = function () {

        var _modalManager;
        var _itemService = abp.services.app.item;
        var _$form = null;
        var _materialUomDropdown = null;
        var _freightUomDropdown = null;
        var _designationDropdown = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _materialUomDropdown = _$form.find("#ItemPrice_MaterialUomId");
            _freightUomDropdown = _$form.find("#ItemPrice_FreightUomId");
            _designationDropdown = _$form.find("#ItemPrice_Designation");

            _materialUomDropdown.select2Uom();
            _freightUomDropdown.select2Uom();
            _designationDropdown.select2Init({
                showAll: true,
                allowClear: false
            });

            _designationDropdown.change(function () {
                if (designationHasMaterial()) {
                    enableMaterialFields();
                } else {
                    disableMaterialFields();
                }
                if (designationIsMaterialOnly()) {
                    disableFreightFields();
                } else {
                    enableFreightFields();
                }
            }).change();

            abp.helper.ui.syncUomDropdowns(_materialUomDropdown, _freightUomDropdown, _designationDropdown);
        };

        function designationHasMaterial() {
            var designation = Number(_designationDropdown.val());
            return abp.enums.designations.hasMaterial.includes(designation);
        }
        function designationIsMaterialOnly() {
            return abp.enums.designations.materialOnly.includes(Number(_designationDropdown.val()));
        }

        function disableMaterialFields() {
            _$form.find('#ItemPrice_MaterialUomId').attr('disabled', 'disabled').val('').change();
            _$form.find("label[for=ItemPrice_MaterialUomId]").removeClass('required-label');
            _$form.find('#ItemPrice_MaterialUomId').closest('.form-group').hide();
            _$form.find('#PricePerUnit').attr('disabled', 'disabled').val('');
            _$form.find('#PricePerUnit').closest('.form-group').hide();
        }
        function enableMaterialFields() {
            _$form.find('#ItemPrice_MaterialUomId').removeAttr('disabled');
            _$form.find("label[for=ItemPrice_MaterialUomId]").addClass('required-label');
            _$form.find('#ItemPrice_MaterialUomId').closest('.form-group').show();
            _$form.find('#PricePerUnit').removeAttr('disabled');
            _$form.find('#PricePerUnit').closest('.form-group').show();
        }
        function disableFreightFields() {
            _$form.find('#ItemPrice_FreightUomId').attr('disabled', 'disabled').val('').change();
            _$form.find("label[for=ItemPrice_FreightUomId]").removeClass('required-label');
            _$form.find('#ItemPrice_FreightUomId').closest('.form-group').hide();
            _$form.find('#FreightRate').attr('disabled', 'disabled').val('');
            _$form.find('#FreightRate').closest('.form-group').hide();
        }
        function enableFreightFields() {
            _$form.find('#ItemPrice_FreightUomId').removeAttr('disabled');
            _$form.find("label[for=ItemPrice_FreightUomId]").addClass('required-label');
            _$form.find('#ItemPrice_FreightUomId').closest('.form-group').show();
            _$form.find('#FreightRate').removeAttr('disabled');
            _$form.find('#FreightRate').closest('.form-group').show();
        }

        function validateFields(entity) {
            var isFreightUomValid = true;
            if (!designationIsMaterialOnly()) {
                if (!Number(entity.FreightUomId)) {
                    isFreightUomValid = false;
                }
            }

            var isMaterialUomValid = true;
            if (designationHasMaterial()) {
                if (!Number(entity.MaterialUomId)) {
                    isMaterialUomValid = false;
                }
            }

            if (!isFreightUomValid
                || !isMaterialUomValid) {
                abp.message.error('Please check the following: \n'
                    + (isMaterialUomValid ? '' : '"Material UOM" - This field is required.\n')
                    + (isFreightUomValid ? '' : '"Freight UOM" - This field is required.\n'), 'Some of the data is invalid');
                return false;
            }
            return true;
        }

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var itemPrice = _$form.serializeFormToObject();

            if (!validateFields(itemPrice)) {
                return;
            }

            _modalManager.setBusy(true);
            _itemService.editItemPrice(itemPrice).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditItemPriceModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
