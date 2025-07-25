(function ($) {
    app.modals.CreateOrEditReceiptLineModal = function () {

        var _modalManager;
        var _receiptAppService = abp.services.app.receipt;
        var _$form = null;
        var _pricing = null;
        var _receiptLine = null;
        var _initializing = false;
        var _saveEventArgs = {
            reloadMaterialTotalIfNotOverridden: false,
            reloadFreightTotalIfNotOverridden: false
        };

        var _loadAtDropdown = null;
        var _deliverToDropdown = null;
        var _freightItemDropdown = null;
        var _materialItemDropdown = null;
        var _initializeMaterialItemDropdown = null;
        var _materialUomDropdown = null;
        var _freightUomDropdown = null;
        var _designationDropdown = null;
        var _materialQuantityInput = null;
        var _freightQuantityInput = null;
        var _isMaterialRateOverriddenInput = null;
        var _isFreightRateOverriddenInput = null;
        var _materialRateInput = null;
        var _freightRateInput = null;
        var _materialAmountInput = null; //total for item
        var _freightAmountInput = null; //total for item
        var _isMaterialAmountOverriddenInput = null;
        var _isFreightAmountOverriddenInput = null;
        var _jobNumberInput = null;
        var _features = {
            separateItems: abp.features.isEnabled('App.SeparateMaterialAndFreightItems'),
        };

        function reloadPricing(callback) {
            if (_initializing) {
                return;
            }
            if (!areRequiredPricingFieldsFilled()) {
                _pricing = null;
                refreshHighlighting();
                if (callback)
                    callback();
                return;
            }
            abp.services.app.item.getItemPricing({
                freightItemId: _freightItemDropdown.val(),
                materialItemId: _materialItemDropdown.val(),
                materialUomId: _materialUomDropdown.val(),
                freightUomId: _freightUomDropdown.val(),
                loadAtId: _loadAtDropdown.val(),
                deliverToId: _deliverToDropdown.val(),
            }).done(function (pricing) {
                _pricing = pricing;
                refreshHighlighting();
                if (callback)
                    callback();
            });
        }

        function areRequiredPricingFieldsFilled() {
            if (_features.separateItems) {
                return !!((_freightItemDropdown.val() && _freightUomDropdown.val()) || (_materialItemDropdown.val() && _materialUomDropdown.val()));
            } else {
                return !!(_freightItemDropdown.val() && (_materialUomDropdown.val() || _freightUomDropdown.val()));
            }
        }

        function refreshHighlighting() {
            if (_pricing && _pricing.quoteBasedPricing) {
                _freightItemDropdown.addClass("quote-based-pricing");
                _materialUomDropdown.addClass("quote-based-pricing");
                _freightUomDropdown.addClass("quote-based-pricing");
            } else {
                _freightItemDropdown.removeClass("quote-based-pricing");
                _materialUomDropdown.removeClass("quote-based-pricing");
                _freightUomDropdown.removeClass("quote-based-pricing");
            }

            if (getIsMaterialRateOverridden()) {
                _materialRateInput.addClass("overridden-price");
            } else {
                _materialRateInput.removeClass("overridden-price");
            }

            if (getIsFreightRateOverridden()) {
                _freightRateInput.addClass("overridden-price");
            } else {
                _freightRateInput.removeClass("overridden-price");
            }

            if (getIsMaterialAmountOverridden()) {
                _materialAmountInput.addClass("overridden-price");
            } else {
                _materialAmountInput.removeClass("overridden-price");
            }

            if (getIsFreightAmountOverridden()) {
                _freightAmountInput.addClass("overridden-price");
            } else {
                _freightAmountInput.removeClass("overridden-price");
            }
        }

        function getIsFreightRateOverridden() {
            return _isFreightRateOverriddenInput.val() === "True";
        }

        function setIsFreightRateOverridden(val) {
            _isFreightRateOverriddenInput.val(val ? "True" : "False");
        }

        function getIsMaterialRateOverridden() {
            return _isMaterialRateOverriddenInput.val() === "True";
        }

        function setIsMaterialRateOverridden(val) {
            _isMaterialRateOverriddenInput.val(val ? "True" : "False");
        }

        function getIsFreightAmountOverridden() {
            return _isFreightAmountOverriddenInput.val() === "True";
        }

        function setIsFreightAmountOverridden(val) {
            _isFreightAmountOverriddenInput.val(val ? "True" : "False");
        }

        function getIsMaterialAmountOverridden() {
            return _isMaterialAmountOverriddenInput.val() === "True";
        }

        function setIsMaterialAmountOverridden(val) {
            _isMaterialAmountOverriddenInput.val(val ? "True" : "False");
        }

        function setFreightRateFromPricingIfNeeded(rate, sender) {
            if (getIsFreightRateOverridden() || designationIsMaterialOnly()) {
                return;
            }
            //when quantity changes, don't reset the rate from pricing unless the rate was empty
            if ((sender.is(_materialQuantityInput) || sender.is(_freightQuantityInput)) && _freightRateInput.val()) {
                return;
            }
            _freightRateInput.val(rate);
        }

        function setMaterialRateFromPricingIfNeeded(rate, sender) {
            if (getIsMaterialRateOverridden() || !designationHasMaterial()) {
                return;
            }
            //when quantity changes, don't reset the rate from pricing unless the rate was empty
            if ((sender.is(_materialQuantityInput) || sender.is(_freightQuantityInput)) && _materialRateInput.val()) {
                return;
            }
            _materialRateInput.val(rate);
        }

        var _recalculating = false;
        function recalculate(sender) {
            if (_initializing || _recalculating) {
                return;
            }
            _recalculating = true;
            if (_pricing && _pricing.hasPricing && _pricing.freightRate !== null) {
                if (sender.is(_freightRateInput)) {
                    setIsFreightRateOverridden(_pricing.freightRate !== Number(_freightRateInput.val())); //_freightRateInput value used to be rouned
                } else {
                    setFreightRateFromPricingIfNeeded(_pricing.freightRate, sender);
                }
            } else {
                //no freight pricing
                if (!getIsFreightRateOverridden() && (sender.is(_freightUomDropdown) || sender.is(_freightItemDropdown))) {
                    _freightRateInput.val('');
                }
            }
            if (_pricing && _pricing.hasPricing && _pricing.pricePerUnit !== null) {
                if (sender.is(_materialRateInput)) {
                    setIsMaterialRateOverridden(_pricing.pricePerUnit !== Number(_materialRateInput.val())); //_materialRateInput used to be rounded
                } else {
                    setMaterialRateFromPricingIfNeeded(_pricing.pricePerUnit, sender);
                }
            } else {
                //no material pricing
                if (!getIsMaterialRateOverridden() && (sender.is(_materialUomDropdown) || sender.is(_features.separateItems ? _materialItemDropdown : _freightItemDropdown))) {
                    _materialRateInput.val('');
                }
            }
            var materialRate = _materialRateInput.val(); //used to be rounded
            var freightRate = _freightRateInput.val(); //used to be rounded
            var materialQuantity = _materialQuantityInput.val(); //quantityInput value used to be rounded
            var freightQuantity = _freightQuantityInput.val();
            var materialAmount = round(materialRate * materialQuantity);
            var freightAmount = round(freightRate * freightQuantity);
            if (!getIsMaterialAmountOverridden()) {
                _materialAmountInput.val(materialAmount.toFixed(2));
            }
            if (!getIsFreightAmountOverridden()) {
                _freightAmountInput.val(freightAmount.toFixed(2));
            }
            refreshHighlighting();
            _saveEventArgs.reloadMaterialTotalIfNotOverridden = true;
            _saveEventArgs.reloadFreightTotalIfNotOverridden = true;
            _recalculating = false;
        }
        function designationHasMaterial() {
            var designation = Number(_designationDropdown.val());
            return abp.enums.designations.hasMaterial.includes(designation);
        }
        function designationIsMaterialOnly() {
            return abp.enums.designations.materialOnly.includes(Number(_designationDropdown.val()));
        }

        function round(num) {
            return abp.utils.round(num);
        }

        function disableMaterialFields() {
            _materialRateInput.attr('disabled', 'disabled').val('0');
            _materialAmountInput.attr('disabled', 'disabled').val('0');
            _materialRateInput.closest('.form-group').hide();
            _materialAmountInput.closest('.form-group').hide();
        }
        function enableMaterialFields() {
            _materialRateInput.removeAttr('disabled');
            _materialAmountInput.removeAttr('disabled');
            _materialRateInput.closest('.form-group').show();
            _materialAmountInput.closest('.form-group').show();
        }
        function disableFreightFields() {
            if (_features.separateItems) {
                _freightItemDropdown.attr('disabled', 'disabled').val('').change();
                _freightItemDropdown.closest('.form-group').hide();
            }
            _freightRateInput.attr('disabled', 'disabled').val('0');
            _freightUomDropdown.attr('disabled', 'disabled').val('').change();
            _freightQuantityInput.attr('disabled', 'disabled').val('');
            _freightAmountInput.attr('disabled', 'disabled').val('0');
            _$form.find("label[for=FreightQuantity]").removeClass('required-label');
            _$form.find("label[for=FreightUomId]").removeClass('required-label');
            _freightRateInput.closest('.form-group').hide();
            _freightUomDropdown.closest('.form-group').hide();
            _freightQuantityInput.closest('.form-group').hide();
            _freightAmountInput.closest('.form-group').hide();
        }
        function enableFreightFields() {
            if (_features.separateItems) {
                _freightItemDropdown.removeAttr('disabled');
                _freightItemDropdown.closest('.form-group').show();
            }
            _freightRateInput.removeAttr('disabled');
            _freightUomDropdown.removeAttr('disabled');
            _freightQuantityInput.removeAttr('disabled');
            _freightAmountInput.removeAttr('disabled');
            _$form.find("label[for=FreightQuantity]").addClass('required-label');
            _$form.find("label[for=FreightUomId]").addClass('required-label');
            _freightRateInput.closest('.form-group').show();
            _freightUomDropdown.closest('.form-group').show();
            _freightQuantityInput.closest('.form-group').show();
            _freightAmountInput.closest('.form-group').show();
        }

        this.init = function (modalManager) {
            _modalManager = modalManager;

            var _createOrEditItemModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Items/CreateOrEditItemModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Items/_CreateOrEditItemModal.js',
                modalClass: 'CreateOrEditItemModal',
                modalSize: 'xl'
            });

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _loadAtDropdown = _$form.find("#LoadAtId");
            _deliverToDropdown = _$form.find("#DeliverToId");
            _freightItemDropdown = _$form.find("#FreightItemId");
            _materialItemDropdown = _$form.find("#MaterialItemId");
            _materialUomDropdown = _$form.find("#MaterialUomId");
            _freightUomDropdown = _$form.find("#FreightUomId");
            _designationDropdown = _$form.find("#Designation");
            _materialQuantityInput = _$form.find("#MaterialQuantity");
            _freightQuantityInput = _$form.find("#FreightQuantity");
            _isMaterialRateOverriddenInput = _$form.find("#IsMaterialRateOverridden");
            _isFreightRateOverriddenInput = _$form.find("#IsFreightRateOverridden");
            _materialRateInput = _$form.find("#MaterialRate");
            _freightRateInput = _$form.find("#FreightRate");
            _materialAmountInput = _$form.find("#MaterialAmount"); //total for item
            _freightAmountInput = _$form.find("#FreightAmount"); //total for item
            _isMaterialAmountOverriddenInput = _$form.find("#IsMaterialAmountOverridden");
            _isFreightAmountOverriddenInput = _$form.find("#IsFreightAmountOverridden");
            _jobNumberInput = _$form.find("#JobNumber");

            _materialAmountInput.val(round(_materialAmountInput.val()).toFixed(2));
            _freightAmountInput.val(round(_freightAmountInput.val()).toFixed(2));

            _loadAtDropdown.select2Init({
                abpServiceMethod: listCacheSelectLists.location(),
                showAll: false,
                allowClear: true
            });

            _deliverToDropdown.select2Init({
                abpServiceMethod: listCacheSelectLists.location(),
                showAll: false,
                allowClear: true

            });

            _freightItemDropdown.select2Init({
                abpServiceMethod: listCacheSelectLists.item(),
                abpServiceParamsGetter: (params) => ({
                    types: _isHaulZoneAndRateFeatureEnabled ? abp.enums.itemTypes.freight : null,
                }),
                showAll: listCache.item.isEnabled,
                allowClear: true,
                addItemCallback: abp.auth.isGranted('Pages.Items') ? async function (newItemName) {
                    var result = await app.getModalResultAsync(
                        _createOrEditItemModal.open({ name: newItemName })
                    );
                    return {
                        id: result.id,
                        name: result.name
                    };
                } : null
            });

            _initializeMaterialItemDropdown = () => {
                if (_materialItemDropdown.data('select2')) {
                    _materialItemDropdown.select2('destroy');
                }
                _materialItemDropdown.select2Init({
                    abpServiceMethod: listCacheSelectLists.item(),
                    abpServiceParamsGetter: (params) => ({
                        types: _isHaulZoneAndRateFeatureEnabled ? abp.enums.itemTypes.material : null,
                    }),
                    showAll: listCache.item.isEnabled,
                    allowClear: designationIsFreightOnly(),
                    addItemCallback: abp.auth.isGranted('Pages.Items') ? async function (newItemName) {
                        var result = await app.getModalResultAsync(
                            _createOrEditItemModal.open({ name: newItemName })
                        );
                        return {
                            id: result.id,
                            name: result.name
                        };
                    } : null
                });
            };

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

                if (_features.separateItems) {
                    let designation = Number(_designationDropdown.val());
                    let isMaterialItemRequired = designation !== abp.enums.designation.freightOnly;
                    _$form.find("#MaterialItemId").attr('required', isMaterialItemRequired);
                    _$form.find("#MaterialItemId").closest('.form-group').find('label').toggleClass('required-label', isMaterialItemRequired);
                }

                _initializeMaterialItemDropdown();
            }).change();

            abp.helper.ui.syncUomDropdowns(_materialUomDropdown, _freightUomDropdown, _designationDropdown, _materialQuantityInput, _freightQuantityInput);

            reloadPricing();
            refreshHighlighting();

            _freightItemDropdown.change(function () {
                var sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
            });
            _materialItemDropdown.change(function () {
                var sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
            });
            _materialUomDropdown.change(function () {
                var sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
            });
            _freightUomDropdown.change(function () {
                var sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
            });

            _materialQuantityInput.change(function () {
                recalculate($(this));
            });
            _freightQuantityInput.change(function () {
                recalculate($(this));
            });
            _materialRateInput.change(function () {
                recalculate($(this));
            });
            _freightRateInput.change(function () {
                recalculate($(this));
            });

            _materialAmountInput.change(function () {
                _saveEventArgs.reloadMaterialTotalIfNotOverridden = true;
                setIsMaterialAmountOverridden(true);
            });
            _freightAmountInput.change(function () {
                _saveEventArgs.reloadFreightTotalIfNotOverridden = true;
                setIsFreightAmountOverridden(true);
            });
        };

        function designationIsFreightOnly() {
            return abp.enums.designations.freightOnly.includes(Number(_designationDropdown.val()));
        }

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var receiptLine = _$form.serializeFormToObject();
            receiptLine.LoadAtName = Number(receiptLine.LoadAtId) ? _$form.find("#LoadAtId option:selected").text() : null;
            receiptLine.DeliverToName = Number(receiptLine.DeliverToId) ? _$form.find("#DeliverToId option:selected").text() : null;
            receiptLine.FreightItemName = Number(receiptLine.FreightItemId) ? _$form.find("#FreightItemId option:selected").text() : null;
            receiptLine.MaterialItemName = Number(receiptLine.MaterialItemId) ? _$form.find("#MaterialItemId option:selected").text() : null;
            receiptLine.MaterialUomName = Number(receiptLine.MaterialUomId) ? _$form.find("#MaterialUomId option:selected").text() : null;
            receiptLine.FreightUomName = Number(receiptLine.FreightUomId) ? _$form.find("#FreightUomId option:selected").text() : null;
            receiptLine.DesignationName = Number(receiptLine.Designation) ? _$form.find("#Designation option:selected").text() : null;
            receiptLine.Id = receiptLine.Id ? Number(receiptLine.Id) : 0;
            receiptLine.ReceiptId = receiptLine.ReceiptId ? Number(receiptLine.ReceiptId) : 0;
            receiptLine.LineNumber = receiptLine.LineNumber ? Number(receiptLine.LineNumber) : 1;
            receiptLine.MaterialRate = receiptLine.MaterialRate || 0;
            receiptLine.MaterialAmount = receiptLine.MaterialAmount || 0;
            receiptLine.FreightRate = receiptLine.FreightRate || 0;
            receiptLine.FreightAmount = receiptLine.FreightAmount || 0;

            if (!validateFields(receiptLine)) {
                return;
            }

            if (receiptLine.Id || receiptLine.ReceiptId) {
                _modalManager.setBusy(true);
                _receiptAppService.editReceiptLine(receiptLine).done(function (e) {
                    abp.notify.info('Saved successfully.');
                    _modalManager.close();
                    abp.event.trigger('app.createOrEditReceiptLineModalSaved', e);
                }).always(function () {
                    _modalManager.setBusy(false);
                });
            } else {
                _receiptLine = _receiptLine || {};
                //$.extend(_receiptLine, receiptLine);
                _receiptLine.id = receiptLine.Id;
                _receiptLine.receiptId = receiptLine.ReceiptId;
                _receiptLine.isMaterialRateOverridden = receiptLine.IsMaterialRateOverridden === "True";
                _receiptLine.isFreightRateOverridden = receiptLine.IsFreightRateOverridden === "True";
                _receiptLine.isMaterialAmountOverridden = receiptLine.IsMaterialAmountOverridden === "True";
                _receiptLine.isFreightAmountOverridden = receiptLine.IsFreightAmountOverridden === "True";
                _receiptLine.lineNumber = receiptLine.LineNumber;
                _receiptLine.loadAtId = receiptLine.LoadAtId;
                _receiptLine.loadAtName = receiptLine.LoadAtName;
                _receiptLine.deliverToId = receiptLine.DeliverToId;
                _receiptLine.deliverToName = receiptLine.DeliverToName;
                _receiptLine.freightItemId = receiptLine.FreightItemId;
                _receiptLine.freightItemName = receiptLine.FreightItemName;
                _receiptLine.materialItemId = receiptLine.MaterialItemId;
                _receiptLine.materialItemName = receiptLine.MaterialItemName;
                _receiptLine.materialUomId = receiptLine.MaterialUomId;
                _receiptLine.materialUomName = receiptLine.MaterialUomName;
                _receiptLine.freightUomId = receiptLine.FreightUomId;
                _receiptLine.freightUomName = receiptLine.FreightUomName;
                _receiptLine.designation = receiptLine.Designation;
                _receiptLine.designationName = receiptLine.DesignationName;
                _receiptLine.materialRate = receiptLine.MaterialRate;
                _receiptLine.freightRate = receiptLine.FreightRate;
                _receiptLine.materialQuantity = receiptLine.MaterialQuantity;
                _receiptLine.freightQuantity = receiptLine.FreightQuantity;
                _receiptLine.materialAmount = receiptLine.MaterialAmount;
                _receiptLine.freightAmount = receiptLine.FreightAmount;
                _receiptLine.jobNumber = receiptLine.JobNumber;
                this.saveCallback && this.saveCallback(_receiptLine);
                _modalManager.close();
                abp.event.trigger('app.createOrEditReceiptLineModalSaved', {});
            }
        };

        function validateFields(receiptLine) {
            var isFreightUomValid = true;
            var isFreightQuantityValid = true;
            if (!designationIsMaterialOnly()) {
                if (!Number(receiptLine.FreightUomId)) {
                    isFreightUomValid = false;
                }
                if (receiptLine.FreightQuantity === '' || receiptLine.FreightQuantity === null || !(Number(receiptLine.FreightQuantity) >= 0)) {
                    isFreightQuantityValid = false;
                }
            }

            var isMaterialUomValid = true;
            var isMaterialQuantityValid = true;
            if (designationHasMaterial()) {
                if (!Number(receiptLine.MaterialUomId)) {
                    isMaterialUomValid = false;
                }
                if (receiptLine.MaterialQuantity === '' || receiptLine.MaterialQuantity === null || !(Number(receiptLine.MaterialQuantity) >= 0)) {
                    isMaterialQuantityValid = false;
                }
            }

            if (!isFreightUomValid
                || !isFreightQuantityValid
                || !isMaterialUomValid
                || !isMaterialQuantityValid) {
                abp.message.error('Please check the following: \n'
                    + (isMaterialUomValid ? '' : '"Material UOM" - This field is required.\n')
                    + (isMaterialQuantityValid ? '' : '"Material Quantity" - This field is required.\n')
                    + (isFreightUomValid ? '' : '"Freight UOM" - This field is required.\n')
                    + (isFreightQuantityValid ? '' : '"Freight Quantity" - This field is required.\n'), 'Some of the data is invalid');
                return false;
            }
            return true;
        }

        this.setReceiptLine = function (receiptLine) {
            _receiptLine = receiptLine;
            if (!_$form) {
                return;
            }
            _initializing = true;
            _$form.find("#Id").val(_receiptLine.id);
            _$form.find("#ReceiptId").val(_receiptLine.receiptId);
            _$form.find("#IsMaterialRateOverridden").val(_receiptLine.isMaterialRateOverridden ? "True" : "False");
            _$form.find("#IsFreightRateOverridden").val(_receiptLine.isFreightRateOverridden ? "True" : "False");
            _$form.find("#IsMaterialAmountOverridden").val(_receiptLine.isMaterialAmountOverridden ? "True" : "False");
            _$form.find("#IsFreightAmountOverridden").val(_receiptLine.isFreightAmountOverridden ? "True" : "False");
            _$form.find("#LineNumber").val(_receiptLine.lineNumber);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#LoadAtId"), _receiptLine.loadAtId, _receiptLine.loadAtName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#DeliverToId"), _receiptLine.deliverToId, _receiptLine.deliverToName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#FreightItemId"), _receiptLine.freightItemId, _receiptLine.freightItemName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#MaterialItemId"), _receiptLine.materialItemId, _receiptLine.materialItemName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#MaterialUomId"), _receiptLine.materialUomId, _receiptLine.materialUomName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#FreightUomId"), _receiptLine.freightUomId, _receiptLine.freightUomName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#Designation"), _receiptLine.designation, _receiptLine.designationName);
            _$form.find("#MaterialRate").val(_receiptLine.materialRate);
            _$form.find("#FreightRate").val(_receiptLine.freightRate);
            _$form.find("#MaterialQuantity").val(_receiptLine.materialQuantity);
            _$form.find("#FreightQuantity").val(_receiptLine.freightQuantity);
            _$form.find("#MaterialAmount").val(_receiptLine.materialAmount);
            _$form.find("#FreightAmount").val(_receiptLine.freightAmount);
            _$form.find("#JobNumber").val(_receiptLine.jobNumber);
            _modalManager.getModal().find('.modal-title').text(receiptLine.isNew ? "Add new line" : "Edit line");
            _initializing = false;
            reloadPricing();
        };

        this.saveCallback = null;

    };
})(jQuery);
