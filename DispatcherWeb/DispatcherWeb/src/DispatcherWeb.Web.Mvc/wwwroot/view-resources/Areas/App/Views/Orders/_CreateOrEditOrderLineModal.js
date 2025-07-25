(function ($) {
    app.modals.CreateOrEditOrderLineModal = function () {

        var _modalManager;
        var _modalOptions = null;
        var _orderAppService = abp.services.app.order;
        var _features = {
            separateItems: abp.features.isEnabled('App.SeparateMaterialAndFreightItems'),
            pricingTiers: abp.features.isEnabled('App.PricingTiersFeature'),
        };
        var _$form = null;
        var _quoteId = null;
        var _quoteLineId = null;
        var _pricingTierId = null;
        var _customerIsCod = null;
        var _pricing = null;
        var _orderLine = null;
        var _initializing = false;
        var _recalculating = false;
        var _saveEventArgs = {
            reloadMaterialTotalIfNotOverridden: false,
            reloadFreightTotalIfNotOverridden: false
        };
        var _loadAtDropdown = null;
        var _deliverToDropdown = null;
        var _useZoneBasedRatesInput = null;
        var _freightItemDropdown = null;
        var _materialItemDropdown = null;
        var _initializeMaterialItemDropdown = null;
        var _materialUomDropdown = null;
        var _initializeMaterialUomDropdown = null;
        var _freightUomDropdown = null;
        var _designationDropdown = null;
        var _vehicleCategoriesDropdown = null;
        var _bedConstructionDropdown = null;
        var _isMaterialPricePerUnitOverriddenInput = null;
        var _isFreightPricePerUnitOverriddenInput = null;
        var _isLeaseHaulerPriceOverriddenInput = null;
        var _materialQuantityInput = null;
        var _freightQuantityInput = null;
        var _travelTimeInput = null;
        var _materialPricePerUnitInput = null;
        var _materialCostRateInput = null;
        var _freightPricePerUnitInput = null;
        var _freightRateToPayDriversInput = null;
        var _isFreightRateToPayDriversOverriddenInput = false;
        var _leaseHaulerRateInput = null;
        var _hourlyDriverPayRateInputs = null;
        var _materialPriceInput = null; //total for the item
        var _freightPriceInput = null; //total for the item
        var _numberOfTrucksInput = null;
        var _timeOnJobInput = null;
        var _isMaterialPriceOverriddenInput = null;
        var _isFreightPriceOverriddenInput = null;
        var _unlockMaterialPriceButton = null;
        var _unlockFreightPriceButton = null;
        var _wasProductionPay = null;

        var _timeOnJobLastValue = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modalOptions = _modalManager.getArgs();

            var createOrEditItemModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Items/CreateOrEditItemModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Items/_CreateOrEditItemModal.js',
                modalClass: 'CreateOrEditItemModal',
                modalSize: 'xl'
            });

            var createOrEditLocationModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Locations/CreateOrEditLocationModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Locations/_CreateOrEditLocationModal.js',
                modalClass: 'CreateOrEditLocationModal',
                modalSize: 'lg'
            });

            var setStaggeredTimesModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Orders/SetStaggeredTimesModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_SetStaggeredTimesModal.js',
                modalClass: 'SetStaggeredTimesModal'
            });

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _timeOnJobInput = _$form.find("#TimeOnJob");

            _numberOfTrucksInput = _$form.find("#NumberOfTrucks");
            _numberOfTrucksInput.change(function () {
                updateStaggeredTimeControls();
            }).change();

            _$form.find("#SetStaggeredTimeButton").click(function () {
                var orderLineId = _$form.find("#Id").val();
                if (orderLineId) {
                    setStaggeredTimesModal.open({ orderLineId });
                } else {
                    _orderLine = _orderLine || {};
                    let model = {
                        orderLineId: null,
                        staggeredTimeKind: Number(_$form.find("#StaggeredTimeKind").val()),
                        staggeredTimeInterval: _orderLine.staggeredTimeInterval,
                        firstStaggeredTimeOnJob: _orderLine.firstStaggeredTimeOnJob
                    };
                    setStaggeredTimesModal.open({}).done(function (modal, modalObject) {
                        modalObject.setModel(model);
                        modalObject.saveCallback = function (model) {
                            _orderLine.updateStaggeredTime = true;
                            _orderLine.staggeredTimeKind = model.staggeredTimeKind;
                            _orderLine.staggeredTimeInterval = model.staggeredTimeInterval;
                            _orderLine.firstStaggeredTimeOnJob = model.firstStaggeredTimeOnJob;
                        };
                    });
                }
            }).tooltip();

            _modalManager.on('app.staggeredTimesSetModal', function (e) {
                _$form.find("#StaggeredTimeKind").val(e.staggeredTimeKind);
                updateTimeOnJobInput();
            });
            updateTimeOnJobInput();


            _quoteId = _$form.find("#QuoteId").val();
            _quoteLineId = _$form.find("#QuoteLineId").val();
            _pricingTierId = _$form.find("#PricingTierId").val();
            _customerIsCod = _$form.find("#CustomerIsCod").val() === "True";

            _loadAtDropdown = _$form.find("#LoadAtId");
            _deliverToDropdown = _$form.find("#DeliverToId");
            _useZoneBasedRatesInput = _$form.find("#UseZoneBasedRates");
            _freightItemDropdown = _$form.find("#FreightItemId");
            _materialItemDropdown = _$form.find("#MaterialItemId");
            _materialUomDropdown = _$form.find("#MaterialUomId");
            _freightUomDropdown = _$form.find("#FreightUomId");
            _designationDropdown = _$form.find("#Designation");
            _vehicleCategoriesDropdown = _$form.find("#VehicleCategories");
            _bedConstructionDropdown = _$form.find("#BedConstruction");
            _isMaterialPricePerUnitOverriddenInput = _$form.find("#IsMaterialPricePerUnitOverridden");
            _isFreightPricePerUnitOverriddenInput = _$form.find("#IsFreightPricePerUnitOverridden");
            _isLeaseHaulerPriceOverriddenInput = _$form.find("#IsLeaseHaulerPriceOverridden");
            _materialQuantityInput = _$form.find("#MaterialQuantity");
            _freightQuantityInput = _$form.find("#FreightQuantity");
            _travelTimeInput = _$form.find("#TravelTime");
            _materialPricePerUnitInput = _$form.find("#MaterialPricePerUnit");
            _materialCostRateInput = _$form.find("#MaterialCostRate");
            _freightPricePerUnitInput = _$form.find("#FreightPricePerUnit");
            _freightRateToPayDriversInput = _$form.find("#FreightRateToPayDrivers");
            _isFreightRateToPayDriversOverriddenInput = _$form.find("#IsFreightRateToPayDriversOverridden");
            _leaseHaulerRateInput = _$form.find("#LeaseHaulerRate");
            _hourlyDriverPayRateInputs = {
                driverPayTimeClassificationDropdown: _$form.find("#DriverPayTimeClassificationId"),
                hourlyDriverPayRateInput: _$form.find("#HourlyDriverPayRate"),
            };
            _materialPriceInput = _$form.find("#MaterialPrice"); //total for item
            _freightPriceInput = _$form.find("#FreightPrice"); //total for item
            _isMaterialPriceOverriddenInput = _$form.find("#IsMaterialPriceOverridden");
            _isFreightPriceOverriddenInput = _$form.find("#IsFreightPriceOverridden");
            _materialPriceInput.val(round(_materialPriceInput.val()).toFixed(2));
            _freightPriceInput.val(round(_freightPriceInput.val()).toFixed(2));

            _timeOnJobLastValue = _timeOnJobInput.val();

            if (!abp.setting.getBoolean('App.LeaseHaulers.ShowLeaseHaulerRateOnOrder')) {
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
                        createOrEditItemModal.open({ name: newItemName })
                    );
                    _$form.find("#IsTaxable, #IsFreightTaxable").val(result.isTaxable ? "True" : "False");
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
                            createOrEditItemModal.open({ name: newItemName })
                        );
                        _$form.find("#IsMaterialTaxable").val(result.isTaxable ? "True" : "False");
                        return {
                            id: result.id,
                            name: result.name
                        };
                    } : undefined
                });
            };

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
            _vehicleCategoriesDropdown.select2Init({
                abpServiceMethod: listCacheSelectLists.vehicleCategory(),
                showAll: true,
                allowClear: true
            });
            _bedConstructionDropdown.select2Init({
                showAll: true,
                allowClear: true
            });

            app.order.initHourlyDriverPayRateInputs(_hourlyDriverPayRateInputs);

            _designationDropdown.change(function () {
                var designationId = !$(this).val() ? _modalOptions?.designation : $(this).val();
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

            _$form.find("#ProductionPay").change(function () {
                updateFreightRateForDriverPayVisibility();
                updateFreightRateToPayDriversValue();
            });

            _$form.find("#RequiresCustomerNotification").change(updateCustomerNotificationControlsVisibility);

            _$form.find('#IsMultipleLoads').change(() => updateRunUntilStopped(true));

            reloadPricing();
            refreshHighlighting();

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

            _freightItemDropdown.change(function () {
                var sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
                var dropdownData = _freightItemDropdown.select2('data');
                if (dropdownData && dropdownData.length && dropdownData[0].item) {
                    _$form.find("#IsTaxable, #IsFreightTaxable").val(dropdownData[0].item.isTaxable ? "True" : "False");
                    _useZoneBasedRatesInput.val(dropdownData[0].item.useZoneBasedRates ? 'True' : 'False');
                }
            });

            _materialItemDropdown.change(function () {
                let sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
                let dropdownData = _materialItemDropdown.select2('data');
                if (dropdownData && dropdownData.length && dropdownData[0].item) {
                    _$form.find("#IsMaterialTaxable").val(dropdownData[0].item.isTaxable ? "True" : "False");
                }
            });

            _materialUomDropdown.change(function () {
                var sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
            });

            _vehicleCategoriesDropdown.change(function () {
                let sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
            });

            _freightUomDropdown.change(function () {
                var sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
                disableProductionPayIfNeeded(true);
                updateFreightRateForDriverPayVisibility();
                updateTravelTimeVisibility();
                updateRunUntilStopped(true);
            });

            abp.helper.ui.syncUomDropdowns(_materialUomDropdown, _freightUomDropdown, _designationDropdown, _materialQuantityInput, _freightQuantityInput);

            _materialQuantityInput.change(function () {
                recalculate($(this));
            });
            _freightQuantityInput.change(function () {
                recalculate($(this));
            });
            _materialPricePerUnitInput.change(function () {
                recalculate($(this));
            });
            _freightPricePerUnitInput.change(function () {
                if (!getIsFreightRateToPayDriversOverridden() || _freightRateToPayDriversInput.is(':hidden')) {
                    _freightRateToPayDriversInput.val(_freightPricePerUnitInput.val());
                }

                recalculate($(this));
            });

            _freightRateToPayDriversInput.change(function () {
                setIsFreightRateToPayDriversOverridden(true);
            });

            _materialPriceInput.change(function () {
                _saveEventArgs.reloadMaterialTotalIfNotOverridden = true;
                setIsMaterialPriceOverridden(true);
            });
            _freightPriceInput.change(function () {
                _saveEventArgs.reloadFreightTotalIfNotOverridden = true;
                setIsFreightPriceOverridden(true);
            });
            initOverrideButtons();
            disableProductionPayIfNeeded(false);
            disableQuoteRelatedFieldsIfNeeded();
            updateCustomerNotificationControlsVisibility();
            updateFreightRateForDriverPayVisibility();
            updateTravelTimeVisibility();
            updateRunUntilStopped();
        };

        function disableQuoteRelatedFieldsIfNeeded() {
            if (_quoteId && !abp.auth.hasPermission('Pages.Orders.EditQuotedValues')) {
                _designationDropdown
                    //.add(_loadAtDropdown)
                    //.add(_deliverToDropdown)
                    .add(_freightItemDropdown)
                    .add(_materialItemDropdown)
                    .add(_freightUomDropdown)
                    .add(_materialUomDropdown)
                    .add(_freightPricePerUnitInput)
                    .add(_materialPricePerUnitInput)
                    //.add(_freightQuantityInput)
                    //.add(_materialQuantityInput)
                    .prop('disabled', true);
            }
        }

        function updateCustomerNotificationControlsVisibility() {
            _$form.find("#CustomerNotificationContactName").closest('.form-group').toggle(_$form.find('#RequiresCustomerNotification').is(':checked'));
            _$form.find("#CustomerNotificationPhoneNumber").closest('.form-group').toggle(_$form.find('#RequiresCustomerNotification').is(':checked'));
        }

        function initOverrideButtons() {
            _unlockMaterialPriceButton = _$form.find("#UnlockMaterialTotalButton");
            _unlockMaterialPriceButton.click(function () {
                setIsMaterialPriceOverridden(!getIsMaterialPriceOverridden());
                refreshTotalFields();
                refreshOverrideButtons();
                refreshHighlighting();
                if (getIsMaterialPriceOverridden()) {
                    _$form.find("#IsMultipleLoads").prop('checked', false).change();
                }
            });
            _unlockFreightPriceButton = _$form.find("#UnlockFreightTotalButton");
            _unlockFreightPriceButton.click(function () {
                setIsFreightPriceOverridden(!getIsFreightPriceOverridden());
                refreshTotalFields();
                refreshOverrideButtons();
                refreshHighlighting();
                if (getIsFreightPriceOverridden()) {
                    _$form.find("#IsMultipleLoads").prop('checked', false).change();
                }
            });
            refreshOverrideButtons();
        }

        function refreshTotalFields() {
            if (getIsMaterialPriceOverridden()) {
                _materialPriceInput.prop('disabled', false);
            } else {
                _materialPriceInput.prop('disabled', true);
                recalculate(_materialQuantityInput);
            }
            if (getIsFreightPriceOverridden()) {
                _freightPriceInput.prop('disabled', false);
            } else {
                _freightPriceInput.prop('disabled', true);
                recalculate(_freightQuantityInput);
            }
        }

        function refreshOverrideButtons() {
            refreshOverrideButton(_unlockMaterialPriceButton, getIsMaterialPriceOverridden());
            refreshOverrideButton(_unlockFreightPriceButton, getIsFreightPriceOverridden());
        }

        function refreshOverrideButton(button, isOverridden) {
            button
                .attr('title', app.localize(isOverridden ? "RemoveOverriddenValue" : "OverrideAutocalculatedValue"))
                .find('.fas').removeClass('fa-unlock fa-lock').addClass(isOverridden ? 'fa-unlock' : 'fa-lock');
        }

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

            abp.services.app.item.getItemPricing(
                getRateParams()
            ).done(function (pricing) {
                _pricing = pricing;
                refreshHighlighting();
                if (callback)
                    callback();
            });
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
                materialUomId: _materialUomDropdown.val(),
                freightUomId: _freightUomDropdown.val(),
                truckCategoryIds: _vehicleCategoriesDropdown.val(),
                pricingTierId: _pricingTierId,
                customerIsCod: _customerIsCod,
                loadAtId: _loadAtDropdown.val(),
                deliverToId: _deliverToDropdown.val(),
                quoteLineId: _quoteLineId,
            };
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

            if (getIsMaterialPricePerUnitOverridden()) {
                _materialPricePerUnitInput.addClass("overridden-price");
            } else {
                _materialPricePerUnitInput.removeClass("overridden-price");
            }

            if (getIsFreightPricePerUnitOverridden()) {
                _freightPricePerUnitInput.addClass("overridden-price");
            } else {
                _freightPricePerUnitInput.removeClass("overridden-price");
            }

            if (getIsMaterialPriceOverridden()) {
                _materialPriceInput.addClass("overridden-price");
            } else {
                _materialPriceInput.removeClass("overridden-price");
            }

            if (getIsFreightPriceOverridden()) {
                _freightPriceInput.addClass("overridden-price");
            } else {
                _freightPriceInput.removeClass("overridden-price");
            }
        }

        function getIsFreightPricePerUnitOverridden() {
            return _isFreightPricePerUnitOverriddenInput.val() === "True";
        }

        function setIsFreightPricePerUnitOverridden(val) {
            _isFreightPricePerUnitOverriddenInput.val(val ? "True" : "False");
        }

        function getIsFreightRateToPayDriversOverridden() {
            return !_freightRateToPayDriversInput.is(':hidden')
                && _isFreightRateToPayDriversOverriddenInput.val() === "True";
        }

        function setIsFreightRateToPayDriversOverridden(val) {
            _isFreightRateToPayDriversOverriddenInput.val(val ? "True" : "False");
        }

        function getIsLeaseHaulerPriceOverridden() {
            return _isLeaseHaulerPriceOverriddenInput.val() === "True";
        }

        function setIsLeaseHaulerPriceOverriden(val) {
            _isLeaseHaulerPriceOverriddenInput.val(val ? "True" : "False");
        }

        function getIsMaterialPricePerUnitOverridden() {
            return _isMaterialPricePerUnitOverriddenInput.val() === "True";
        }

        function setIsMaterialPricePerUnitOverridden(val) {
            _isMaterialPricePerUnitOverriddenInput.val(val ? "True" : "False");
        }

        function getIsFreightPriceOverridden() {
            return _isFreightPriceOverriddenInput.val() === "True";
        }

        function setIsFreightPriceOverridden(val) {
            _isFreightPriceOverriddenInput.val(val ? "True" : "False");
        }

        function getIsMaterialPriceOverridden() {
            return _isMaterialPriceOverriddenInput.val() === "True";
        }

        function setIsMaterialPriceOverridden(val) {
            _isMaterialPriceOverriddenInput.val(val ? "True" : "False");
        }

        function updateTimeOnJobInput() {
            if (Number(_$form.find("#StaggeredTimeKind").val()) > 0) {
                disableTimeOnJobInput();
            } else {
                enableTimeOnJobInput();
            }
        }

        function enableTimeOnJobInput() {
            if (_timeOnJobInput.val() === 'Staggered') {
                _timeOnJobInput.val('');
            }
            _timeOnJobInput.prop('disabled', false).timepickerInit({ stepping: 1 });
        }

        function disableTimeOnJobInput() {
            var timeOnJobTimepicker = _timeOnJobInput.data('DateTimePicker');
            timeOnJobTimepicker && timeOnJobTimepicker.destroy();
            _timeOnJobInput.prop('disabled', true).val('Staggered');
        }

        function updateStaggeredTimeControls() {
            var isStaggeredTimeButtonVisible = Number(_numberOfTrucksInput.val()) > 1;
            _$form.find("#SetStaggeredTimeButton").closest('.input-group-btn').toggle(isStaggeredTimeButtonVisible);
            if (!isStaggeredTimeButtonVisible && Number(_$form.find('#StaggeredTimeKind').val()) > 0) {
                _$form.find('#StaggeredTimeKind').val("0");
                enableTimeOnJobInput();
            }
        }

        function disableProductionPayIfNeeded(forceUncheck) {
            if (!shouldDisableProductionPay()) {
                enableProductionPay();
            } else {
                let productionPayInput = _$form.find('#ProductionPay')

                if (forceUncheck) {
                    productionPayInput.prop('checked', false);
                } else {
                    if (productionPayInput.is(':checked')) {
                        return;
                    }
                }

                disableProductionPay();
            }
        }

        function shouldDisableProductionPay() {
            if (abp.setting.getBoolean('App.TimeAndPay.PreventProductionPayOnHourlyJobs')) {
                return freightUomDropdownHasHours();
            }
            return false;
        }

        function freightUomDropdownHasHours() {
            let freightUom = _freightUomDropdown.getSelectedDropdownOption().text();
            return ['hours', 'hour'].includes((freightUom || '').toLowerCase());
        }

        function disableProductionPay() {
            let productionPayInput = _$form.find('#ProductionPay');
            productionPayInput.prop('disabled', true);
            productionPayInput.closest('label').attr('title', app.localize('PreventProductionPayOnHourlyJobsHint')).tooltip();
        }

        function enableProductionPay() {
            let productionPayInput = _$form.find('#ProductionPay');
            productionPayInput.prop('disabled', false);
            productionPayInput.closest('label').attr('title', '').tooltip('dispose');
        }

        function shouldFreightRateForDriverPayBeVisible() {
            return !designationIsMaterialOnly()
                && abp.setting.getBoolean('App.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate')
                && _$form.find('#ProductionPay').is(':checked');
        }

        function updateFreightRateForDriverPayVisibility() {
            if (shouldFreightRateForDriverPayBeVisible()) {
                _freightRateToPayDriversInput.closest('.form-group').show();
            } else {
                _freightRateToPayDriversInput.val(_freightPricePerUnitInput.val()).closest('.form-group').hide();
                setIsFreightRateToPayDriversOverridden(false);
            }

            updateLoadBasedVisibility();
        }

        function updateLoadBasedVisibility() {
            let freightUom = _freightUomDropdown.getSelectedDropdownOption().text();
            if (shouldFreightRateForDriverPayBeVisible()
                && abp.setting.getBoolean('App.TimeAndPay.AllowLoadBasedRates')
                && !(['hours', 'hour'].includes((freightUom || '').toLowerCase()))
                && _$form.find('#ProductionPay').is(':checked')
            ) {
                _$form.find('#LoadBased').closest('.form-group').show();
            } else {
                _$form.find('#LoadBased').prop('checked', false).closest('.form-group').hide();
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

        function setFreightRateFromPricingIfNeeded(rate, sender) {
            if (getIsFreightPricePerUnitOverridden() || designationIsMaterialOnly()) {
                return;
            }
            //when quantity changes, don't reset the rate from pricing unless the rate was empty
            if ((sender.is(_materialQuantityInput) || sender.is(_freightQuantityInput)) && _freightPricePerUnitInput.val()) {
                return;
            }
            _freightPricePerUnitInput.val(rate);
        }

        function setMaterialRateFromPricingIfNeeded(rate, sender) {
            if (getIsMaterialPricePerUnitOverridden() || !designationHasMaterial()) {
                return;
            }
            //when quantity changes, don't reset the rate from pricing unless the rate was empty
            if ((sender.is(_materialQuantityInput) || sender.is(_freightQuantityInput)) && _materialPricePerUnitInput.val()) {
                return;
            }
            _materialPricePerUnitInput.val(rate);
        }

        function setLeaseHaulerRateFromPricingIfNeeded(rate, sender) {
            if (getIsLeaseHaulerPriceOverridden() || designationIsMaterialOnly()){
                return;
            }
            //when quantity changes, don't reset the rate from pricing unless the rate was empty
            if ((sender.is(_materialQuantityInput) || sender.is(_freightQuantityInput)) && _leaseHaulerRateInput.val()) {
                return;
            }
            _leaseHaulerRateInput.val(rate);
        }

        function recalculate(sender) {
            if (_initializing || _recalculating) {
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
            var materialCostRatePricing = getPricingFieldValue('materialCostRate');

            if (freightRatePricing !== null) {
                if (sender.is(_freightPricePerUnitInput)) {
                    setIsFreightPricePerUnitOverridden(freightRatePricing !== Number(_freightPricePerUnitInput.val()));
                } else {
                    setFreightRateFromPricingIfNeeded(freightRatePricing, sender);
                }
            } else {
                //no freight pricing
                if (!getIsFreightPricePerUnitOverridden() && isSenderFreightDropdown) {
                    _freightPricePerUnitInput.val('');
                }
            }

            if (leaseHaulerPricing !== null) {
                if (sender.is(_leaseHaulerRateInput)) {
                    setIsLeaseHaulerPriceOverriden(leaseHaulerPricing !== Number(_leaseHaulerRateInput.val()));
                } else {
                    setLeaseHaulerRateFromPricingIfNeeded(leaseHaulerPricing, sender);
                }
            } else {
                if (!getIsLeaseHaulerPriceOverridden() && isSenderFreightDropdown && !_quoteLineId) {
                    _leaseHaulerRateInput.val('');
                }
            }

            if (materialRatePricing !== null) {
                if (sender.is(_materialPricePerUnitInput)) {
                    setIsMaterialPricePerUnitOverridden(materialRatePricing !== Number(_materialPricePerUnitInput.val()));
                } else {
                    setMaterialRateFromPricingIfNeeded(materialRatePricing, sender);
                }
            } else {
                //no material pricing
                if (!getIsMaterialPricePerUnitOverridden() && isSenderMaterialDropdown) {
                    _materialPricePerUnitInput.val('');
                }
            }

            if (materialCostRatePricing !== null) {
                _materialCostRateInput.val(materialCostRatePricing);
            }

            updateFreightRateToPayDriversValue();

            var materialPricePerUnit = _materialPricePerUnitInput.val();
            var freightPricePerUnit = _freightPricePerUnitInput.val();
            var materialQuantity = _materialQuantityInput.val();
            var freightQuantity = _freightQuantityInput.val();
            var materialPrice = round(materialPricePerUnit * materialQuantity);
            var freightPrice = round(freightPricePerUnit * freightQuantity);

            if (!getIsMaterialPriceOverridden()) {
                _materialPriceInput.val(materialPrice.toFixed(2));
            }

            if (!getIsFreightPriceOverridden()) {
                _freightPriceInput.val(freightPrice.toFixed(2));
            }
            refreshHighlighting();
            _saveEventArgs.reloadMaterialTotalIfNotOverridden = true;
            _saveEventArgs.reloadFreightTotalIfNotOverridden = true;
            _recalculating = false;
        }

        function updateFreightRateToPayDriversValue() {
            var freightRateToPayDriversPricing = getPricingFieldValue('freightRateToPayDrivers');

            if (_freightRateToPayDriversInput.is(':hidden')) {
                _freightRateToPayDriversInput.val(_freightPricePerUnitInput.val());
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

        function round(num) {
            return abp.utils.round(num);
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

        function disableMaterialFields() {
            _$form.find('#MaterialPricePerUnit').val('').closest('.form-group').hide();
            _$form.find('#MaterialPrice').val('0').closest('.form-group').hide();
        }
        function enableMaterialFields() {
            _$form.find('#MaterialPricePerUnit').closest('.form-group').show();
            _$form.find('#MaterialPrice').closest('.form-group').show();
        }
        function disableFreightFields() {
            _$form.find("label[for=FreightUomId]").removeClass('required-label');
            _$form.find('#FreightPricePerUnit').val('').closest('.form-group').hide();
            _$form.find('#FreightPrice').val('0').closest('.form-group').hide();
            _$form.find('#FreightUomId').val('').change().closest('.form-group').hide();
            _$form.find('#FreightQuantity').val('').closest('.form-group').hide();
        }
        function enableFreightFields() {
            _$form.find("label[for=FreightUomId]").addClass('required-label');
            _$form.find('#FreightPricePerUnit').closest('.form-group').show();
            _$form.find('#FreightPrice').closest('.form-group').show();
            _$form.find('#FreightUomId').closest('.form-group').show();
            _$form.find('#FreightQuantity').closest('.form-group').show();
        }

        function updateProductionPay() {
            let productionPay = _$form.find('#ProductionPay');
            let productionPayContainer = productionPay.closest('.form-group');
            if (designationIsMaterialOnly()) {
                if (_wasProductionPay === null) {
                    _wasProductionPay = productionPay.is(':checked');
                }
                productionPay.prop('checked', false);
                productionPayContainer.hide();
                updateFreightRateForDriverPayVisibility();
            } else {
                if (_wasProductionPay !== null) {
                    if (shouldDisableProductionPay()) {
                        disableProductionPay();
                    } else {
                        productionPay.prop('checked', _wasProductionPay);
                    }
                    _wasProductionPay = null;
                }
                productionPayContainer.show();
                updateFreightRateForDriverPayVisibility();
            }
        }

        function updateRunUntilStopped(ignoreOrderLineId) {
            let runUntilStoppedInput = _$form.find('#IsMultipleLoads');
            var orderLine = _$form.serializeFormToObject();
            orderLine.Id = orderLine.Id ? Number(orderLine.Id) : null;
            if (freightUomDropdownHasHours() && (!orderLine.Id || ignoreOrderLineId || !runUntilStoppedInput.is(':checked'))) {
                runUntilStoppedInput.prop('disabled', true).prop('checked', false);
            } else {
                runUntilStoppedInput.prop('disabled', false);
            }
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

            updateCustomerNotificationControlsVisibility();
        }

        function setModalSize(size) {
            _modalManager.getModal().find('.modal-dialog').removeClass('modal-md').removeClass('modal-lg').addClass('modal-' + size);
        }

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }
            if (!app.order.validatreHourlyDriverPayRateInputs(_hourlyDriverPayRateInputs)) {
                return;
            }

            var orderLine = _$form.serializeFormToObject();
            orderLine.Id = orderLine.Id ? Number(orderLine.Id) : null;
            orderLine.OrderId = orderLine.OrderId ? Number(orderLine.OrderId) : 0;
            orderLine.QuoteId = orderLine.QuoteId ? Number(orderLine.QuoteId) : null;
            orderLine.QuoteLineId = orderLine.QuoteLineId ? Number(orderLine.QuoteLineId) : null;

            if (!parseFloat(orderLine.MaterialQuantity) && parseFloat(orderLine.MaterialPrice)
                || !parseFloat(orderLine.FreightQuantity) && parseFloat(orderLine.FreightPrice)) {
                abp.message.error(app.localize('QuantityIsRequiredWhenTotalIsSpecified'));
                return;
            }

            if (orderLine.RequiresCustomerNotification && (orderLine.CustomerNotificationContactName === "" || orderLine.CustomerNotificationPhoneNumber === "")) {
                abp.message.error('Please check the following: \n'
                    + (orderLine.CustomerNotificationContactName ? '' : '"Contact Name" - This field is required.\n')
                    + (orderLine.CustomerNotificationPhoneNumber ? '' : '"Phone Number" - This field is required.\n'), 'Some of the data is invalid');
                return;
            }

            if (orderLine.CustomerNotificationPhoneNumber) {
                if (!_$form.find("#CustomerNotificationPhoneNumber")[0].checkValidity()) {
                    abp.message.error(app.localize('IncorrectPhoneNumberFormatError'));
                    return;
                }
            }

            if (!validateFields(orderLine)) {
                return;
            }

            if (Number(orderLine.StaggeredTimeKind) !== abp.enums.staggeredTimeKind.none) {
                orderLine.TimeOnJob = null;
            }

            _orderLine = _orderLine || {};
            _orderLine.id = orderLine.Id;
            _orderLine.orderId = orderLine.OrderId;
            _orderLine.quoteId = orderLine.QuoteId;
            _orderLine.quoteLineId = orderLine.QuoteLineId;
            _orderLine.pricingTierId = orderLine.PricingTierId;
            _orderLine.customerIsCod = orderLine.CustomerIsCod === "True";
            _orderLine.useZoneBasedRates = orderLine.UseZoneBasedRates === "True";
            _orderLine.isMaterialPricePerUnitOverridden = orderLine.IsMaterialPricePerUnitOverridden === "True";
            _orderLine.isFreightPricePerUnitOverridden = orderLine.IsFreightPricePerUnitOverridden === "True";
            _orderLine.isFreightRateToPayDriversOverridden = orderLine.IsFreightRateToPayDriversOverridden === "True";
            _orderLine.isLeaseHaulerPriceOverridden = orderLine.IsLeaseHaulerPriceOverridden === "True";
            _orderLine.isMaterialPriceOverridden = orderLine.IsMaterialPriceOverridden === "True";
            _orderLine.isFreightPriceOverridden = orderLine.IsFreightPriceOverridden === "True";
            _orderLine.isTaxable = orderLine.IsTaxable === "True";
            _orderLine.isMaterialTaxable = orderLine.IsMaterialTaxable === "True";
            _orderLine.isFreightTaxable = orderLine.IsFreightTaxable === "True";
            _orderLine.staggeredTimeKind = Number(orderLine.StaggeredTimeKind) || 0;
            _orderLine.lineNumber = Number(orderLine.LineNumber);
            _orderLine.designation = orderLine.Designation;
            _orderLine.designationName = Number(orderLine.Designation) ? _$form.find("#Designation option:selected").text() : null;
            _orderLine.loadAtId = orderLine.LoadAtId;
            _orderLine.loadAtName = Number(orderLine.LoadAtId) ? _$form.find("#LoadAtId option:selected").text() : null;
            _orderLine.deliverToId = orderLine.DeliverToId;
            _orderLine.deliverToName = Number(orderLine.DeliverToId) ? _$form.find("#DeliverToId option:selected").text() : null;
            _orderLine.freightItemId = orderLine.FreightItemId;
            _orderLine.freightItemName = Number(orderLine.FreightItemId) ? _$form.find("#FreightItemId option:selected").text() : null;
            _orderLine.materialItemId = orderLine.MaterialItemId;
            _orderLine.materialItemName = Number(orderLine.MaterialItemId) ? _$form.find("#MaterialItemId option:selected").text() : null;
            _orderLine.materialUomId = orderLine.MaterialUomId;
            _orderLine.materialUomName = Number(orderLine.MaterialUomId) ? _$form.find("#MaterialUomId option:selected").text() : null;
            _orderLine.freightUomId = orderLine.FreightUomId;
            _orderLine.freightUomName = Number(orderLine.FreightUomId) ? _$form.find("#FreightUomId option:selected").text() : null;
            _orderLine.freightUomBaseId = _freightUomDropdown.getUomBaseId();
            _orderLine.materialPricePerUnit = Number(orderLine.MaterialPricePerUnit) || 0;
            _orderLine.materialCostRate = Number(orderLine.MaterialCostRate) || 0;
            _orderLine.freightPricePerUnit = Number(orderLine.FreightPricePerUnit) || 0;
            _orderLine.freightRateToPayDrivers = Number(orderLine.FreightRateToPayDrivers) || 0;
            _orderLine.loadBased = !!orderLine.LoadBased;
            _orderLine.leaseHaulerRate = Number(orderLine.LeaseHaulerRate) || 0;
            _orderLine.driverPayTimeClassificationId = orderLine.DriverPayTimeClassificationId;
            _orderLine.driverPayTimeClassificationName = Number(orderLine.DriverPayTimeClassificationId) ? _hourlyDriverPayRateInputs.driverPayTimeClassificationDropdown.find('option:selected').text() : null;
            _orderLine.hourlyDriverPayRate = Number(orderLine.HourlyDriverPayRate) || 0;
            _orderLine.materialQuantity = Number(orderLine.MaterialQuantity) || 0;
            _orderLine.freightQuantity = Number(orderLine.FreightQuantity) || 0;
            _orderLine.travelTime = Number(orderLine.TravelTime) || 0;
            _orderLine.materialPrice = Number(orderLine.MaterialPrice) || 0;
            _orderLine.freightPrice = Number(orderLine.FreightPrice) || 0;
            _orderLine.numberOfTrucks = Number(orderLine.NumberOfTrucks) || 0;
            _orderLine.isMultipleLoads = !!orderLine.IsMultipleLoads;
            _orderLine.productionPay = !!orderLine.ProductionPay;
            _orderLine.requireTicket = !!orderLine.RequireTicket;
            _orderLine.timeOnJob = orderLine.TimeOnJob;
            _orderLine.jobNumber = orderLine.JobNumber;
            _orderLine.note = orderLine.Note;
            _orderLine.requiresCustomerNotification = !!orderLine.RequiresCustomerNotification;
            _orderLine.customerNotificationContactName = orderLine.CustomerNotificationContactName;
            _orderLine.customerNotificationPhoneNumber = orderLine.CustomerNotificationPhoneNumber;
            _orderLine.vehicleCategories = _$form.find("#VehicleCategories").select2('data').map(x => ({ id: x.id, name: x.text }));
            _orderLine.bedConstruction = orderLine.BedConstruction === "" ? null : Number(orderLine.BedConstruction) || 0;
            _orderLine.bedConstructionName = orderLine.BedConstruction === "" ? null : _$form.find("#BedConstruction option:selected").text();

            try {
                _modalManager.setBusy(true);

                if (_orderLine.id
                    && _orderLine.staggeredTimeKind === abp.enums.staggeredTimeKind.none
                    && _orderLine.timeOnJob !== _timeOnJobLastValue
                    && abp.auth.hasPermission('Pages.Orders.Edit')
                ) {
                    var timeOnJobValidationResult = await _orderAppService.validateOrderLineTimeOnJob({ orderLineId: _orderLine.id });

                    if (!timeOnJobValidationResult.hasOrderLineTrucks) {
                        _orderLine.updateOrderLineTrucksTimeOnJob = false;
                        _orderLine.updateDispatchesTimeOnJob = false;
                    } else {
                        _orderLine.updateOrderLineTrucksTimeOnJob =
                            !timeOnJobValidationResult.hasDisagreeingOrderLineTrucks
                            || !!await abp.message.confirmWithYesNo(app.localize('TimeOnJobDisagreeingTrucksValidationMessage'));

                        _orderLine.updateDispatchesTimeOnJob =
                            _orderLine.updateOrderLineTrucksTimeOnJob
                            && (
                                !timeOnJobValidationResult.hasOpenDispatches
                                || !!await abp.message.confirmWithYesNo(app.localize('TimeOnJobOpenDispatchesValidationMessage'))
                            );
                    }
                }

                if (this.saveCallback) {
                    this.saveCallback(_orderLine);
                }

                if (!orderLine.Id && !orderLine.OrderId) {
                    _modalManager.close();
                    abp.event.trigger('app.createOrEditOrderLineModalSaved', {});
                    return;
                }

                let materialQuantity = orderLine.MaterialQuantity === "" ? null : abp.utils.round(parseFloat(orderLine.MaterialQuantity));
                let freightQuantity = orderLine.FreightQuantity === "" ? null : abp.utils.round(parseFloat(orderLine.FreightQuantity));
                let numberOfTrucks = orderLine.NumberOfTrucks === "" ? null : abp.utils.round(parseFloat(orderLine.NumberOfTrucks));

                if (orderLine.Id && !await abp.scheduling.checkExistingDispatchesBeforeSettingQuantityAndNumberOfTrucksZero(orderLine.Id, materialQuantity, freightQuantity, numberOfTrucks)) {
                    _modalManager.close();
                    return;
                }


                let editResult = await _orderAppService.editOrderLine(_orderLine);

                abp.notify.info('Saved successfully.');
                _$form.find("#Id").val(editResult.orderLineId);
                _modalManager.close();
                abp.event.trigger('app.createOrEditOrderLineModalSaved', editResult);

            } finally {
                _modalManager.setBusy(false);
            }
        }

        function validateFields(orderLine) {
            var isFreightUomValid = true;
            if (!designationIsMaterialOnly()) {
                if (!Number(orderLine.FreightUomId)) {
                    isFreightUomValid = false;
                }
            }

            var isMaterialUomValid = true;
            if (designationHasMaterial()) {
                if (!Number(orderLine.MaterialUomId)) {
                    isMaterialUomValid = false;
                }
            }

            if (_materialQuantityInput.val() !== '' && !Number(orderLine.MaterialUomId)) {
                isMaterialUomValid = false;
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

        this.setOrderLine = function (orderLine) {
            _orderLine = orderLine;
            if (!_$form) {
                return;
            }
            _initializing = true;
            _$form.find("#Id").val(_orderLine.id);
            _$form.find("#OrderId").val(_orderLine.orderId);
            _$form.find("#QuoteId").val(_orderLine.quoteId);
            _$form.find("#QuoteLineId").val(_orderLine.quoteLineId);
            _$form.find("#PricingTierId").val(_orderLine.pricingTierId);
            _$form.find("#CustomerIsCod").val(_orderLine.customerIsCod ? "True" : "False");
            _$form.find("#UseZoneBasedRates").val(_orderLine.useZoneBasedRates ? "True" : "False");
            _$form.find("#IsMaterialPricePerUnitOverridden").val(_orderLine.isMaterialPricePerUnitOverridden ? "True" : "False");
            _$form.find("#IsFreightPricePerUnitOverridden").val(_orderLine.isFreightPricePerUnitOverridden ? "True" : "False");
            _$form.find("#IsFreightRateToPayDriversOverridden").val(_orderLine.isFreightRateToPayDriversOverridden ? "True" : "False");
            _$form.find("#IsLeaseHaulerPriceOverridden").val(_orderLine.isLeaseHaulerPriceOverridden ? "True" : "False");
            _$form.find("#IsMaterialPriceOverridden").val(_orderLine.isMaterialPriceOverridden ? "True" : "False");
            _$form.find("#IsFreightPriceOverridden").val(_orderLine.isFreightPriceOverridden ? "True" : "False");
            _$form.find("#IsTaxable").val(_orderLine.isTaxable ? "True" : "False");
            _$form.find("#IsMaterialTaxable").val(_orderLine.isMaterialTaxable ? "True" : "False");
            _$form.find("#IsFreightTaxable").val(_orderLine.isFreightTaxable ? "True" : "False");
            _$form.find("#StaggeredTimeKind").val(_orderLine.staggeredTimeKind);
            _$form.find("#LineNumber").val(_orderLine.lineNumber);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#Designation"), _orderLine.designation, _orderLine.designationName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#LoadAtId"), _orderLine.loadAtId, _orderLine.loadAtName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#DeliverToId"), _orderLine.deliverToId, _orderLine.deliverToName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#FreightItemId"), _orderLine.freightItemId, _orderLine.freightItemName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#MaterialItemId"), _orderLine.materialItemId, _orderLine.materialItemName);
            _freightUomDropdown.setUomBaseId(_orderLine.freightUomBaseId); //must be set before both material UOM and freight UOM are triggering their change event
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#MaterialUomId"), _orderLine.materialUomId, _orderLine.materialUomName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#FreightUomId"), _orderLine.freightUomId, _orderLine.freightUomName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#BedConstruction"), _orderLine.bedConstruction, _orderLine.bedConstructionName);
            _$form.find("#MaterialPricePerUnit").val(_orderLine.materialPricePerUnit);
            _$form.find("#MaterialCostRate").val(_orderLine.materialCostRate);
            _$form.find("#FreightPricePerUnit").val(_orderLine.freightPricePerUnit);

            _$form.find("#FreightRateToPayDrivers").val(_orderLine.freightRateToPayDrivers);
            _$form.find("#LoadBased").prop('checked', _orderLine.loadBased);
            _$form.find("#LeaseHaulerRate").val(_orderLine.leaseHaulerRate);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#DriverPayTimeClassificationId"), _orderLine.driverPayTimeClassificationId, _orderLine.driverPayTimeClassificationName);
            _$form.find("#HourlyDriverPayRate").val(_orderLine.hourlyDriverPayRate);
            _$form.find("#MaterialQuantity").val(_orderLine.materialQuantity);
            _$form.find("#FreightQuantity").val(_orderLine.freightQuantity);
            _$form.find("#TravelTime").val(_orderLine.travelTime);
            _$form.find("#MaterialPrice").val(_orderLine.materialPrice);
            _$form.find("#FreightPrice").val(_orderLine.freightPrice);
            _$form.find("#NumberOfTrucks").val(_orderLine.numberOfTrucks);
            _$form.find("#IsMultipleLoads").prop('checked', _orderLine.isMultipleLoads);
            _$form.find("#ProductionPay").prop('checked', _orderLine.productionPay);
            _$form.find("#RequireTicket").prop('checked', _orderLine.requireTicket);
            _$form.find("#TimeOnJob").val(_orderLine.timeOnJob);
            _timeOnJobLastValue = _timeOnJobInput.val();
            _$form.find("#JobNumber").val(_orderLine.jobNumber);
            _$form.find("#Note").val(_orderLine.note);
            if (_orderLine.vehicleCategories) {
                abp.helper.ui.addAndSetDropdownValues(_$form.find("#VehicleCategories"), _orderLine.vehicleCategories);
            }

            _quoteId = _$form.find("#QuoteId").val();
            _quoteLineId = _$form.find("#QuoteLineId").val();
            _pricingTierId = _$form.find("#PricingTierId").val();
            _customerIsCod = _$form.find("#CustomerIsCod").val() === "True";

            updateStaggeredTimeControls();
            updateTimeOnJobInput();
            updateProductionPay();
            disableProductionPayIfNeeded(false);
            disableQuoteRelatedFieldsIfNeeded();

            _modalManager.getModal().find('.modal-title').text(orderLine.isNew ? "Add new line" : "Edit line");
            _initializing = false;
            reloadPricing();

            refreshTotalFields();
            refreshOverrideButtons();
            refreshHighlighting();
        };

        this.saveCallback = null;

    };
})(jQuery);
