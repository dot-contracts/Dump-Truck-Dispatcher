(function ($) {
    app.modals.CreateOrEditQuoteLineModal = function () {

        var _modalManager;
        var _customerPricingTierId;
        var _customerIsCod;
        var _quoteAppService = abp.services.app.quote;
        var _features = {
            separateItems: abp.features.isEnabled('App.SeparateMaterialAndFreightItems'),
            pricingTiers: abp.features.isEnabled('App.PricingTiersFeature'),
        };
        var _$form = null;
        var _inputPrefix = "quote-service";
        var _designationDropdown = null;
        var _wasProductionPay = null;
        var _pricing = null;
        var _recalculating = false;

        var _useZoneBasedRatesInput = null;
        var _freightItemDropdown = null;
        var _materialItemDropdown = null;
        var _initializeMaterialItemDropdown = null;
        var _materialUomDropdown = null;
        var _initializeMaterialUomDropdown = null;
        var _materialQuantityInput = null;
        var _materialRateInput = null;
        var _materialCostRateInput = null;
        var _freightRateInput = null;
        var _freightRateToPayDriversInput = null;
        var _isMaterialPricePerUnitOverriddenInput = null;
        var _isFreightRateOverriddenInput = null;
        var _isLeaseHaulerRateOverriddenInput = null;
        var _isFreightRateToPayDriversOverriddenInput = false;
        var _freightUomDropdown = null;
        var _freightQuantityInput = null;
        var _travelTimeInput = null;
        var _vehicleCategoriesDropdown = null;
        var _deliverToDropdown = null;
        var _jobNumberInput = null;
        var _bedConstructionDropdown = null;
        var _loadAtDropdown = null;
        var _leaseHaulerRateInput = null;
        var _hourlyDriverPayRateInputs = null;
        var _runUntilStoppedInput = null;
        var _productionPayInput = null;
        var _loadBasedCheckbox = null;
        var _freightTotalInput = null;
        var _materialTotalInput = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            var createOrEditItemModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Items/CreateOrEditItemModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Items/_CreateOrEditItemModal.js',
                modalClass: 'CreateOrEditItemModal',
                modalSize: 'md' //the rates are hidden on this view and we don't need the extra width
            });

            var createOrEditLocationModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Locations/CreateOrEditLocationModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Locations/_CreateOrEditLocationModal.js',
                modalClass: 'CreateOrEditLocationModal',
                modalSize: 'lg'
            });

            _$form = _modalManager.getModal().find('form');
            _$form.find('input, select').each(function () {
                let currentId = $(this).attr('id');
                if (currentId) {
                    const newId = `${_inputPrefix}-${currentId}`;
                    $(this).attr('id', newId);
                    $(this).closest('form').find('label[for="' + currentId + '"]').attr('for', newId);
                }
            });
            _$form.validate();

            _customerPricingTierId = abp.utils.parseStringToNullableNumber(_$form.find(`#${_inputPrefix}-CustomerPricingTierId`).val());
            _customerIsCod = _$form.find(`#${_inputPrefix}-CustomerIsCod`).val() === 'True';

            _loadAtDropdown = _$form.find(`#${_inputPrefix}-LoadAtId`);
            _deliverToDropdown = _$form.find(`#${_inputPrefix}-DeliverToId`);
            _useZoneBasedRatesInput = _$form.find(`#${_inputPrefix}-UseZoneBasedRates`);
            _freightItemDropdown = _$form.find(`#${_inputPrefix}-FreightItemId`);
            _materialItemDropdown = _$form.find(`#${_inputPrefix}-MaterialItemId`);
            _materialUomDropdown = _$form.find(`#${_inputPrefix}-MaterialUomId`);
            _freightUomDropdown = _$form.find(`#${_inputPrefix}-FreightUomId`);

            _designationDropdown = _$form.find(`#${_inputPrefix}-Designation`);
            _vehicleCategoriesDropdown = _$form.find(`#${_inputPrefix}-VehicleCategories`);
            _bedConstructionDropdown = _$form.find(`#${_inputPrefix}-BedConstruction`);
            _materialQuantityInput = _$form.find(`#${_inputPrefix}-MaterialQuantity`);
            _freightQuantityInput = _$form.find(`#${_inputPrefix}-FreightQuantity`);
            _travelTimeInput = _$form.find(`#${_inputPrefix}-TravelTime`);
            _materialRateInput = _$form.find(`#${_inputPrefix}-PricePerUnit`);
            _materialCostRateInput = _$form.find(`#${_inputPrefix}-MaterialCostRate`);
            _isMaterialPricePerUnitOverriddenInput = _$form.find(`#${_inputPrefix}-IsPricePerUnitOverridden`);
            _freightRateInput = _$form.find(`#${_inputPrefix}-FreightRate`);
            _isFreightRateOverriddenInput = _$form.find(`#${_inputPrefix}-IsFreightRateOverridden`);
            _freightRateToPayDriversInput = _$form.find(`#${_inputPrefix}-FreightRateToPayDrivers`);
            _isFreightRateToPayDriversOverriddenInput = _$form.find(`#${_inputPrefix}-IsFreightRateToPayDriversOverridden`);
            _leaseHaulerRateInput = _$form.find(`#${_inputPrefix}-LeaseHaulerRate`);
            _isLeaseHaulerRateOverriddenInput = _$form.find(`#${_inputPrefix}-IsLeaseHaulerRateOverridden`);
            _hourlyDriverPayRateInputs = {
                driverPayTimeClassificationDropdown: _$form.find(`#${_inputPrefix}-DriverPayTimeClassificationId`),
                hourlyDriverPayRateInput: _$form.find(`#${_inputPrefix}-HourlyDriverPayRate`),
            };
            _runUntilStoppedInput = _$form.find(`#${_inputPrefix}-IsMultipleLoads`);
            _jobNumberInput = _$form.find(`#${_inputPrefix}-JobNumber`);
            _productionPayInput = _$form.find(`#${_inputPrefix}-ProductionPay`);
            _loadBasedCheckbox = _$form.find(`#${_inputPrefix}-LoadBased`);
            _freightTotalInput = _$form.find(`#${_inputPrefix}-FreightPriceAmount`);
            _materialTotalInput = _$form.find(`#${_inputPrefix}-MaterialPriceAmount`);

            if (!abp.setting.getBoolean('App.LeaseHaulers.ShowLeaseHaulerRateOnQuote')) {
                _leaseHaulerRateInput.closest('.form-group').hide();
            }

            async function addNewLocation(newItemName) {
                var result = await app.getModalResultAsync(
                    createOrEditLocationModal.open({ mergeWithDuplicateSilently: true, name: newItemName })
                );
                return {
                    id: result.id,
                    name: result.displayName
                };
            }

            _loadAtDropdown.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownLoadSite,
                addItemCallback: abp.auth.hasPermission('Pages.Locations') ? addNewLocation : undefined,
                showAll: true, //todo should be dynamic if possible (false for old functionality, true for haul zone rates)
                rateParamsGetter: getRateParams,
            });
            _deliverToDropdown.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownDeliverySite,
                addItemCallback: abp.auth.hasPermission('Pages.Locations') ? addNewLocation : undefined,
            });
            _freightItemDropdown.select2Init({
                abpServiceMethod: listCacheSelectLists.item(),
                abpServiceParamsGetter: (params) => ({
                    types: _features.separateItems ? abp.enums.itemTypes.freight : null,
                }),
                showAll: listCache.item.isEnabled,
                allowClear: false,
                addItemCallback: abp.auth.hasPermission('Pages.Items') ? async function (newItemName) {
                    var result = await app.getModalResultAsync(
                        createOrEditItemModal.open({
                            name: newItemName,
                            hideRate: true,
                        })
                    );
                    _useZoneBasedRatesInput.val(result.useZoneBasedRates ? 'True' : 'False');
                    return {
                        id: result.id,
                        name: result.name
                    };
                } : undefined
            });
            _initializeMaterialItemDropdown = () => {
                if (_materialItemDropdown.data('select2')) {
                    _materialItemDropdown.select2('destroy');
                }
                _materialItemDropdown.select2Init({
                    abpServiceMethod: listCacheSelectLists.item(),
                    abpServiceParamsGetter: (params) => ({
                        types: _features.separateItems ? abp.enums.itemTypes.material : null,
                    }),
                    showAll: listCache.item.isEnabled,
                    allowClear: designationIsFreightOnly(),
                    addItemCallback: abp.auth.hasPermission('Pages.Items') ? async function (newItemName) {
                        var result = await app.getModalResultAsync(
                            createOrEditItemModal.open({
                                name: newItemName,
                                hideRate: true,
                            })
                        );
                        return {
                            id: result.id,
                            name: result.name
                        };
                    } : undefined
                });
            };

            _vehicleCategoriesDropdown.select2Init({
                abpServiceMethod: listCacheSelectLists.vehicleCategory(),
                showAll: true,
                allowClear: true
            });
            _initializeMaterialUomDropdown = () => {
                if (_materialUomDropdown.data('select2')) {
                    _materialUomDropdown.select2('destroy');
                }
                _materialUomDropdown.select2Uom({
                    allowClear: false, //designationIsFreightOnly(),
                });
            };
            _freightUomDropdown.select2Uom();
            _designationDropdown.select2Init({
                showAll: true,
                allowClear: false
            });
            _bedConstructionDropdown.select2Init({
                showAll: true,
                allowClear: true
            });

            app.order.initHourlyDriverPayRateInputs(_hourlyDriverPayRateInputs);

            abp.helper.ui.syncUomDropdowns(_materialUomDropdown, _freightUomDropdown, _designationDropdown, _materialQuantityInput, _freightQuantityInput);

            _designationDropdown.change(function () {
                var designationId = $(this).val();
                if (_features.separateItems) {
                    showFieldControlsBasedOnDesignation(designationId);
                } else {
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
                }
                _materialUomDropdown.attr('required', true);
                _materialUomDropdown.closest('.form-group').find('label').toggleClass('required-label', true);

                _initializeMaterialItemDropdown();
                _initializeMaterialUomDropdown();

                updateProductionPay();
                updateFreightRateForDriverPayVisibility();
            }).change();

            _freightUomDropdown.change(function () {
                disableProductionPayIfNeeded(true);
                updateFreightRateForDriverPayVisibility();
                updateTravelTimeVisibility();
                var sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
            });

            _freightItemDropdown.change(function () {
                var dropdownData = _freightItemDropdown.select2('data');
                if (dropdownData?.length && dropdownData[0].item) {
                    _useZoneBasedRatesInput.val(dropdownData[0].item.useZoneBasedRates ? 'True' : 'False');
                }
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

            _vehicleCategoriesDropdown.change(function () {
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

            _loadAtDropdown.change(function () {
                var sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
            });

            _deliverToDropdown.change(function () {
                var sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
            });

            _materialQuantityInput.change(function () {
                recalculate($(this));
            });

            _materialRateInput.change(function () {
                recalculate($(this));
            });

            _freightQuantityInput.change(function () {
                recalculate($(this));
            });

            _freightRateInput.change(function () {
                if (!getIsFreightRateToPayDriversOverridden() || _freightRateToPayDriversInput.is(':hidden')) {
                    _freightRateToPayDriversInput.val(_freightRateInput.val());
                }

                recalculate($(this));
            });

            _freightRateToPayDriversInput.change(function () {
                setIsFreightRateToPayDriversOverridden(true);
            });

            _productionPayInput.change(function () {
                updateFreightRateForDriverPayVisibility();
                updateFreightRateToPayDriversValue();
            });

            disableProductionPayIfNeeded(false);
            updateFreightRateForDriverPayVisibility();
            updateTravelTimeVisibility();
            reloadPricing();
            refreshHighlighting();

            if (_features.separateItems) {
                _materialTotalInput.val(calculateTotal(_materialQuantityInput.val(), _materialRateInput.val()));
                _freightTotalInput.val(calculateTotal(_freightQuantityInput.val(), _freightRateInput.val()));
            }
        };

        function setFreightRateFromPricingIfNeeded(rate, sender) {
            if (getIsFreightPricePerUnitOverridden() || designationIsMaterialOnly()) {
                return;
            }
            //when quantity changes, don't reset the rate from pricing unless the rate was empty
            if ((sender.is(_materialQuantityInput) || sender.is(_freightQuantityInput)) && _freightRateInput.val()) {
                return;
            }
            _freightRateInput.val(rate);
        }

        function setMaterialRateFromPricingIfNeeded(rate, sender) {
            if (getIsMaterialPricePerUnitOverridden() || !designationHasMaterial()) {
                return;
            }
            //when quantity changes, don't reset the rate from pricing unless the rate was empty
            if ((sender.is(_materialQuantityInput) || sender.is(_freightQuantityInput)) && _materialRateInput.val()) {
                return;
            }
            _materialRateInput.val(rate);
        }

        function setLeaseHaulerRateFromPricingIfNeeded(rate, sender) {
            if (getIsLeaseHaulerRateOverridden() || designationIsMaterialOnly()) {
                return;
            }
            //when quantity changes, don't reset the rate from pricing unless the rate was empty
            if ((sender.is(_materialQuantityInput) || sender.is(_freightQuantityInput)) && _leaseHaulerRateInput.val()) {
                return;
            }
            _leaseHaulerRateInput.val(rate);
        }

        function recalculate(sender) {
            if (_recalculating) {
                return;
            }
            _recalculating = true;

            let isSenderMaterialDropdown = !_features.pricingTiers
                ? sender.is(_materialUomDropdown) || sender.is(_freightItemDropdown) || sender.is(_loadAtDropdown) || sender.is(_deliverToDropdown)
                : _features.separateItems
                    ? sender.is(_materialUomDropdown) || sender.is(_materialItemDropdown) || sender.is(_loadAtDropdown) || sender.is(_deliverToDropdown)
                    : sender.is(_materialUomDropdown) || sender.is(_freightItemDropdown) || sender.is(_loadAtDropdown) || sender.is(_deliverToDropdown);

            let isSenderFreightDropdown = !_features.pricingTiers
                ? sender.is(_freightUomDropdown) || sender.is(_freightItemDropdown) || sender.is(_loadAtDropdown) || sender.is(_deliverToDropdown)
                : _features.separateItems
                    ? sender.is(_freightUomDropdown) || sender.is(_freightItemDropdown) || sender.is(_vehicleCategoriesDropdown)
                    : sender.is(_freightItemDropdown) || sender.is(_loadAtDropdown) || sender.is(_deliverToDropdown);

            //if (_pricing && _pricing.isMultiplePriceObject && isSenderFreightDropdown) {
            //    abp.message.info('Multiple truck categories were found, so we were unable to set a default price.', 'Unable to set a default price');
            //}

            var freightRatePricing = getPricingFieldValue('freightRate');
            var materialRatePricing = getPricingFieldValue('pricePerUnit');
            var leaseHaulerPricing = getPricingFieldValue('leaseHaulerRate');

            if (freightRatePricing !== null) {
                if (sender.is(_freightRateInput)) {
                    setIsFreightPricePerUnitOverridden(freightRatePricing !== Number(_freightRateInput.val()));
                } else {
                    setFreightRateFromPricingIfNeeded(freightRatePricing, sender);
                }
            } else {
                //no freight pricing
                if (!getIsFreightPricePerUnitOverridden() && isSenderFreightDropdown) {
                    _freightRateInput.val('');
                }
            }

            if (leaseHaulerPricing !== null) {
                if (sender.is(_leaseHaulerRateInput)) {
                    setIsLeaseHaulerRateOverridden(leaseHaulerPricing !== Number(_leaseHaulerRateInput.val()));
                } else {
                    setLeaseHaulerRateFromPricingIfNeeded(leaseHaulerPricing, sender);
                }
            } else {
                if (!getIsLeaseHaulerRateOverridden() && isSenderFreightDropdown) {
                    _leaseHaulerRateInput.val('');
                }
            }

            if (materialRatePricing !== null) {
                if (sender.is(_materialRateInput)) {
                    setIsMaterialPricePerUnitOverridden(materialRatePricing !== Number(_materialRateInput.val()));
                } else {
                    setMaterialRateFromPricingIfNeeded(materialRatePricing, sender);
                }
            } else {
                //no material pricing
                if (!getIsMaterialPricePerUnitOverridden() && isSenderMaterialDropdown) {
                    _materialRateInput.val('');
                }
            }

            _materialCostRateInput.val(_pricing?.materialCostRate);

            updateFreightRateToPayDriversValue();

            var materialPricePerUnit = _materialRateInput.val();
            var freightPricePerUnit = _freightRateInput.val();
            var materialQuantity = _materialQuantityInput.val();
            var freightQuantity = _freightQuantityInput.val();
            var materialPrice = abp.utils.round(materialPricePerUnit * materialQuantity);
            var freightPrice = abp.utils.round(freightPricePerUnit * freightQuantity);

            if (!getIsMaterialPriceOverridden()) {
                _materialTotalInput.val(materialPrice.toFixed(2));
            }

            if (!getIsFreightPriceOverridden()) {
                _freightTotalInput.val(freightPrice.toFixed(2));
            }
            refreshHighlighting();
            _recalculating = false;
        }

        function updateFreightRateToPayDriversValue() {
            var freightRateToPayDriversPricing = getPricingFieldValue('freightRateToPayDrivers');

            if (_freightRateToPayDriversInput.is(':hidden')) {
                _freightRateToPayDriversInput.val(_freightRateInput.val());
                setIsFreightRateToPayDriversOverridden(false);
            } else {
                if (!getIsFreightRateToPayDriversOverridden() && freightRateToPayDriversPricing !== null) {
                    _freightRateToPayDriversInput.val(freightRateToPayDriversPricing);
                }
            }
        }

        function getPricingFieldValue(fieldName) {
            return _pricing?.quoteBasedPricing && _pricing.quoteBasedPricing[fieldName] !== null ? _pricing.quoteBasedPricing[fieldName]
                : _pricing?.hasPricing && _pricing[fieldName] !== null ? _pricing[fieldName]
                    : null;
        }

        function designationHasMaterial() {
            var designation = Number(_designationDropdown.val());
            return abp.enums.designations.hasMaterial.includes(designation);
        }

        function designationIsFreightOnly() {
            return abp.enums.designations.freightOnly.includes(Number(_designationDropdown.val()));
        }

        function designationIsMaterialOnly() {
            return abp.enums.designations.materialOnly.includes(Number(_designationDropdown.val()));
        }

        function reloadPricing(callback) {
            if (!areRequiredPricingFieldsFilled()) {
                _pricing = null;
                refreshHighlighting();
                if (callback)
                    callback();
                return;
            }

            abp.services.app.item.getItemPricing(
                getRateParams()
            ).done(function (pricing) {
                _pricing = pricing;
                refreshHighlighting();
                if (callback)
                    callback();
            });
        }

        function refreshHighlighting() {
            _materialRateInput.toggleClass("overridden-price", getIsMaterialPricePerUnitOverridden());
            _freightRateInput.toggleClass("overridden-price", getIsFreightPricePerUnitOverridden());
        }

        function getIsFreightPricePerUnitOverridden() {
            return _isFreightRateOverriddenInput.val() === "True";
        }

        function setIsFreightPricePerUnitOverridden(val) {
            _isFreightRateOverriddenInput.val(val ? "True" : "False");
        }

        function getIsFreightRateToPayDriversOverridden() {
            return !_freightRateToPayDriversInput.is(':hidden')
                && _isFreightRateToPayDriversOverriddenInput.val() === "True";
        }

        function setIsFreightRateToPayDriversOverridden(val) {
            _isFreightRateToPayDriversOverriddenInput.val(val ? "True" : "False");
        }

        function getIsLeaseHaulerRateOverridden() {
            return _isLeaseHaulerRateOverriddenInput.val() === "True";
        }

        function setIsLeaseHaulerRateOverridden(val) {
            _isLeaseHaulerRateOverriddenInput.val(val ? "True" : "False");
        }

        function getIsMaterialPricePerUnitOverridden() {
            return _isMaterialPricePerUnitOverriddenInput.val() === "True";
        }

        function setIsMaterialPricePerUnitOverridden(val) {
            _isMaterialPricePerUnitOverriddenInput.val(val ? "True" : "False");
        }

        function getIsFreightPriceOverridden() {
            return false;
            //return _isFreightPriceOverriddenInput.val() === "True";
        }

        function setIsFreightPriceOverridden(val) {
            //_isFreightPriceOverriddenInput.val(val ? "True" : "False");
        }

        function getIsMaterialPriceOverridden() {
            return false;
            //return _isMaterialPriceOverriddenInput.val() === "True";
        }

        function setIsMaterialPriceOverridden(val) {
            //_isMaterialPriceOverriddenInput.val(val ? "True" : "False");
        }

        function areRequiredPricingFieldsFilled() {
            if (_features.separateItems) {
                return _freightItemDropdown.val() && _freightUomDropdown.val()
                    || _materialItemDropdown.val() && _materialUomDropdown.val();
            } else {
                return _freightItemDropdown.val()
                    && (_materialUomDropdown.val() || _freightUomDropdown.val());
            }
        }

        function getRateParams() {
            return {
                useZoneBasedRates: _useZoneBasedRatesInput.val() === 'True',
                designation: _designationDropdown.val(),
                freightItemId: _freightItemDropdown.val(),
                materialItemId: _materialItemDropdown.val(),
                freightUomId: _freightUomDropdown.val(),
                materialUomId: _materialUomDropdown.val(),
                truckCategoryIds: _vehicleCategoriesDropdown.val(),
                pricingTierId: _customerPricingTierId,
                customerIsCod: _customerIsCod,
                loadAtId: _loadAtDropdown.val(),
                deliverToId: _deliverToDropdown.val(),
            };
        }

        function showFieldControlsBasedOnDesignation(designationId) {
            let designation = Number(designationId);

            setModalSize(designation === abp.enums.designation.materialOnly || !designation ? 'md' : 'lg');

            if (!designation) {
                _$form.find('[data-visible-for-designation]').hide();
                return;
            }

            _$form.find('[data-visible-for-designation]').show();
            if (designation === abp.enums.designation.materialOnly) {
                _$form.find('[data-visible-for-designation="freight"]').hide();
            } else if (designation === abp.enums.designation.freightOnly) {
                _$form.find('[data-visible-for-designation="material"]').hide();
            }

            let isMaterialRequired = designation !== abp.enums.designation.freightOnly;
            let materialControls = _materialItemDropdown.add(
                //_materialUomDropdown //material UOM is always required
            );
            materialControls.attr('required', isMaterialRequired);
            materialControls.closest('.form-group').find('label').toggleClass('required-label', isMaterialRequired);
        }

        function setModalSize(size) {
            _modalManager.getModal().find('.modal-dialog').removeClass('modal-md, modal-lg').addClass('modal-' + size);
        }

        function calculateTotal(quantity, rate) {
            quantity = quantity ? quantity : 0;
            rate = rate ? rate : 0;

            var calculatedPriceAmount = parseFloat(quantity) * parseFloat(rate);
            return abp.helper.dataTables.renderMoney(calculatedPriceAmount);
        }

        function disableMaterialFields() {
            _materialRateInput.attr('disabled', 'disabled').val('0');
            _materialRateInput.closest('.form-group').hide();
        }

        function enableMaterialFields() {
            _materialRateInput.removeAttr('disabled');
            _materialRateInput.closest('.form-group').show();
        }

        function disableFreightFields() {
            _freightRateInput.attr('disabled', 'disabled').val('0');
            _freightUomDropdown.attr('disabled', 'disabled').val('').change();
            _freightQuantityInput.attr('disabled', 'disabled').val('');
            _freightRateInput.closest('.form-group').hide();
            _freightUomDropdown.closest('.form-group').hide();
            _freightQuantityInput.closest('.form-group').hide();
        }

        function enableFreightFields() {
            _freightRateInput.removeAttr('disabled');
            _freightUomDropdown.removeAttr('disabled');
            _freightQuantityInput.removeAttr('disabled');
            _freightRateInput.closest('.form-group').show();
            _freightUomDropdown.closest('.form-group').show();
            _freightQuantityInput.closest('.form-group').show();
        }

        function disableProductionPayIfNeeded(forceUncheck) {
            if (!shouldDisableProductionPay()) {
                enableProductionPay();
            } else {
                if (forceUncheck) {
                    _productionPayInput.prop('checked', false);
                } else {
                    if (_productionPayInput.is(':checked')) {
                        return;
                    }
                }

                disableProductionPay();
            }
        }

        function shouldDisableProductionPay() {
            if (abp.setting.getBoolean('App.TimeAndPay.PreventProductionPayOnHourlyJobs')) {
                let freightUom = _freightUomDropdown.getSelectedDropdownOption().text();
                if (['hours', 'hour'].includes((freightUom || '').toLowerCase())) {
                    return true;
                }
            }
            return false;
        }

        function disableProductionPay() {
            _productionPayInput.prop('disabled', true);
            _productionPayInput.closest('label').attr('title', app.localize('PreventProductionPayOnHourlyJobsHint')).tooltip();
        }

        function enableProductionPay() {
            _productionPayInput.prop('disabled', false);
            _productionPayInput.closest('label').attr('title', '').tooltip('dispose');
        }

        function shouldFreightRateForDriverPayBeVisible() {
            return !designationIsMaterialOnly()
                && abp.setting.getBoolean('App.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate')
                && _productionPayInput.is(':checked');
        }

        function updateFreightRateForDriverPayVisibility() {
            if (shouldFreightRateForDriverPayBeVisible()) {
                _freightRateToPayDriversInput.closest('.form-group').show();
            } else {
                _freightRateToPayDriversInput.val(_freightRateInput.val()).change().closest('.form-group').hide();
                setIsFreightRateToPayDriversOverridden(false);
            }

            updateLoadBasedVisibility();
        }

        function updateLoadBasedVisibility() {
            let freightUom = _freightUomDropdown.getSelectedDropdownOption().text();
            if (shouldFreightRateForDriverPayBeVisible()
                && abp.setting.getBoolean('App.TimeAndPay.AllowLoadBasedRates')
                && !(['hours', 'hour'].includes((freightUom || '').toLowerCase()))
                && _productionPayInput.is(':checked')
            ) {
                _loadBasedCheckbox.closest('.form-group').show();
            } else {
                _loadBasedCheckbox.prop('checked', false).closest('.form-group').hide();
            }
        }

        function updateTravelTimeVisibility() {
            let freightUomBaseId = _freightUomDropdown.getUomBaseId();
            let visible = freightUomBaseId === abp.enums.uomBase.hours
                && abp.features.isEnabled('App.IncludeTravelTime');
            _travelTimeInput.closest('.form-group').toggle(visible);
            if (!visible) {
                _travelTimeInput.val('');
            }
        }

        function updateProductionPay() {
            let productionPayContainer = _productionPayInput.closest('.form-group');
            if (designationIsMaterialOnly()) {
                if (_wasProductionPay === null) {
                    _wasProductionPay = _productionPayInput.is(':checked');
                }
                _productionPayInput.prop('checked', false);
                productionPayContainer.hide();
                updateFreightRateForDriverPayVisibility();
            } else {
                if (_wasProductionPay !== null) {
                    if (shouldDisableProductionPay()) {
                        disableProductionPay();
                    } else {
                        _productionPayInput.prop('checked', _wasProductionPay);
                    }
                    _wasProductionPay = null;
                }
                productionPayContainer.show();
                updateFreightRateForDriverPayVisibility();
            }
        }

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            if (!app.order.validatreHourlyDriverPayRateInputs(_hourlyDriverPayRateInputs)) {
                return;
            }

            var quoteLine = _$form.serializeFormToObject();
            quoteLine.VehicleCategories = _vehicleCategoriesDropdown.select2('data').map(x => ({
                id: x.id,
                name: x.name
            }));

            if (_features.separateItems && _materialQuantityInput.val() !== '' && !Number(quoteLine.MaterialUomId)) {
                abp.message.error('Please check the following: \n'
                    + '"Material UOM" - This field is required.\n');
                return;
            }

            _modalManager.setBusy(true);
            _quoteAppService.editQuoteLine(quoteLine).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditQuoteLineModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };

    };
})(jQuery);
