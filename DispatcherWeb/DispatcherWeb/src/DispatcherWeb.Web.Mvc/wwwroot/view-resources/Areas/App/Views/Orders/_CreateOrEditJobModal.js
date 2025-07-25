(function ($) {
    app.modals.CreateOrEditJobModal = function () {

        var _modalManager;
        var _orderAppService = abp.services.app.order;
        var _$form = null;
        var _quoteId = null;
        var _quoteLineId = null;
        var _pricing = null;
        var _orderLine = null;
        var _orderId = null;
        var _model = null;
        var _initializing = false;
        var _recalculating = false;
        var _permissions = {
            edit: abp.auth.hasPermission('Pages.Orders.Edit'),
            viewLeaseHaulerJob: abp.auth.hasPermission('LeaseHaulerPortal.Jobs.View'),
            editLeaseHaulerJob: abp.auth.hasPermission('LeaseHaulerPortal.Jobs.Edit'),
            leaseHaulerTruckRequest: abp.auth.hasPermission('LeaseHaulerPortal.Truck.Request'),
            counterSales: abp.auth.hasPermission('Pages.CounterSales'),
        };
        var _features = {
            separateItems: abp.features.isEnabled('App.SeparateMaterialAndFreightItems'),
            pricingTiers : abp.features.isEnabled('App.PricingTiersFeature'),
        };
        var _allowCounterSales = abp.setting.getBoolean('App.DispatchingAndMessaging.AllowCounterSalesForUser') && abp.setting.getBoolean('App.DispatchingAndMessaging.AllowCounterSalesForTenant');
        var _saveEventArgs = {
            reloadMaterialTotalIfNotOverridden: false,
            reloadFreightTotalIfNotOverridden: false
        };
        var _quoteDropdown = null;
        var _deliveryDateDropdown = null;
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
        var _materialPriceInput = null; //total for the item
        var _freightPriceInput = null; //total for the item
        var _numberOfTrucksInput = null;
        var _timeOnJobInput = null;
        var _isMaterialPriceOverriddenInput = null;
        var _isFreightPriceOverriddenInput = null;
        var _unlockMaterialPriceButton = null;
        var _unlockFreightPriceButton = null;
        var _wasProductionPay = null;
        var _orderIsTaxExempt = null;
        var _pricingTierId = null;
        var _customerIsCod = null;
        var _customerIsTaxExempt = null;
        var _timeOnJobLastValue = null;
        var _leaseHaulerRateInput = null;
        var _hourlyDriverPayRateInputs = null;

        var _createOrEditCustomerModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Customers/CreateOrEditCustomerModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Customers/_CreateOrEditCustomerModal.js',
            modalClass: 'CreateOrEditCustomerModal',
            modalSize: 'lg'
        });

        var _createOrEditCustomerContactModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Customers/CreateOrEditCustomerContactModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Customers/_CreateOrEditCustomerContactModal.js',
            modalClass: 'CreateOrEditCustomerContactModal'
        });

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

        var _selectOrderQuoteModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/SelectOrderQuoteModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_SelectOrderQuoteModal.js',
            modalClass: 'SelectOrderQuoteModal'
        });

        var _addQuoteBasedOrderLinesModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/AddQuoteBasedOrderLinesModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_AddQuoteBasedOrderLinesModal.js',
            modalClass: 'AddQuoteBasedOrderLinesModal',
            modalSize: 'lg'
        });

        var _editChargesModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Charges/EditChargesModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Charges/_EditChargesModal.js',
            modalClass: 'EditChargesModal',
            modalSize: 'xl',
        });

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _timeOnJobInput = _$form.find("#TimeOnJob");

            _numberOfTrucksInput = _$form.find("#NumberOfTrucks");
            _numberOfTrucksInput.change(function () {
                updateStaggeredTimeControls();
            }).change();

            _pricingTierId = _$form.find("#PricingTierId").val();
            _customerIsCod = _$form.find("#CustomerIsCod").val() === 'True';

            _$form.find("#SetStaggeredTimeButton").click(function () {
                var orderLineId = _$form.find("#OrderLineId").val();
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
            _orderId = _$form.find("#OrderId").val();
            _customerIsTaxExempt = _$form.find("#CustomerIsTaxExempt").val() === 'True';
            _orderIsTaxExempt = _$form.find("#JobIsTaxExempt");

            _quoteDropdown = _$form.find("#QuoteId");
            _deliveryDateDropdown = _$form.find("#DeliveryDate");
            _loadAtDropdown = _$form.find("#LoadAtId");
            _deliverToDropdown = _$form.find("#DeliverToId");
            _useZoneBasedRatesInput = _$form.find("#UseZoneBasedRates");
            _freightItemDropdown = _$form.find("#JobFreightItemId");
            _materialItemDropdown = _$form.find("#JobMaterialItemId");
            _materialUomDropdown = _$form.find("#MaterialUomId");
            _freightUomDropdown = _$form.find("#FreightUomId");
            _designationDropdown = _$form.find("#JobDesignation");
            _customerDropdown = _$form.find("#JobCustomerId");
            _contactDropdown = _$form.find("#JobContactId");
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

            _timeOnJobLastValue = _timeOnJobInput.val();

            _materialPriceInput = _$form.find("#MaterialPrice"); //total for item
            _freightPriceInput = _$form.find("#FreightPrice"); //total for item

            _isMaterialPriceOverriddenInput = _$form.find("#IsMaterialPriceOverridden");
            _isFreightPriceOverriddenInput = _$form.find("#IsFreightPriceOverridden");

            if (_materialPriceInput.val() && !isNaN(_materialPriceInput.val())) {
                _materialPriceInput.val(round(_materialPriceInput.val()).toFixed(2));
            }
            if (_freightPriceInput.val() && !isNaN(_freightPriceInput.val())) {
                _freightPriceInput.val(round(_freightPriceInput.val()).toFixed(2));
            }
            if (_leaseHaulerRateInput.val() && !isNaN(_leaseHaulerRateInput.val())) {
                _leaseHaulerRateInput.val(abp.utils.round(_leaseHaulerRateInput.val()).toFixed(2));
            }

            if (!abp.setting.getBoolean('App.LeaseHaulers.ShowLeaseHaulerRateOnOrder')) {
                _leaseHaulerRateInput.closest('.form-group').hide();
            }

            //Init field editors

            _deliveryDateDropdown.datepickerInit();

            _$form.find("#JobShift").select2Init({
                showAll: true,
                allowClear: false
            });

            let quoteChildDropdown;
            let contactChildDropdown;
            if (_permissions.edit) {
                _$form.find("#JobSalesTaxEntityId").select2Init({
                    abpServiceMethod: listCacheSelectLists.taxRate(),
                    showAll: true,
                    allowClear: true
                });

                _$form.find("#JobOfficeId").select2Init({
                    abpServiceMethod: listCacheSelectLists.office(),
                    showAll: true,
                    allowClear: false
                });

                _customerDropdown.select2Init({
                    abpServiceMethod: abp.services.app.customer.getActiveCustomersSelectList,
                    showAll: false,
                    allowClear: true,
                    addItemCallback: async function (newItemName) {
                        var result = await app.getModalResultAsync(
                            _createOrEditCustomerModal.open({ name: newItemName })
                        );
                        selectCustomerInControl(result);
                        return false;
                    }
                });

                _contactDropdown.select2Init({
                    showAll: true,
                    allowClear: true,
                    addItemCallback: async function (newItemName) {
                        var customerId = _customerDropdown.val();
                        if (!customerId) {
                            abp.notify.warn("Select a customer first");
                            _customerDropdown.focus();
                            return false;
                        }
                        var result = await app.getModalResultAsync(
                            _createOrEditCustomerContactModal.open({ name: newItemName, customerId: customerId })
                        );
                        contactChildDropdown.updateChildDropdown(function () {
                            contactChildDropdown.childDropdown.val(result.Id).change();
                        });
                        return false;
                    }
                });

                quoteChildDropdown = abp.helper.ui.initChildDropdown({
                    parentDropdown: _customerDropdown,
                    childDropdown: _quoteDropdown,
                    abpServiceMethod: abp.services.app.quote.getQuotesForCustomer,
                    abpServiceData: { hideInactive: true },
                    optionCreatedCallback: function (option, val) {
                        switch (val.item.status) {
                            case abp.enums.quoteStatus.pending: option.addClass("quote-pending"); break;
                            case abp.enums.quoteStatus.active: option.addClass("quote-active"); break;
                            case abp.enums.quoteStatus.inactive: option.addClass("quote-inactive"); break;
                        }
                    }
                });

                quoteChildDropdown.onChildDropdownUpdated(async function (data) {
                    let hasActiveOrPendingQuotes = data.items.some(val =>
                        val.item.status === abp.enums.quoteStatus.pending || val.item.status === abp.enums.quoteStatus.active
                    );

                    if (hasActiveOrPendingQuotes) {
                        let selectedQuoteId = await app.getModalResultAsync(
                            _selectOrderQuoteModal.open()
                        );
                        _quoteDropdown.val(selectedQuoteId).change();
                    }
                });

                contactChildDropdown = abp.helper.ui.initChildDropdown({
                    parentDropdown: _customerDropdown,
                    childDropdown: _contactDropdown,
                    abpServiceMethod: abp.services.app.customer.getContactsForCustomer
                });

                _$form.find("#JobFuelSurchargeCalculationId").select2Init({
                    abpServiceMethod: listCacheSelectLists.fuelSurchargeCalculation(),
                    showAll: true,
                    allowClear: true
                });

                _$form.find("#JobSalesTaxEntityId").change(function () {
                    var dropdownData = $(this).select2('data');
                    if (dropdownData && dropdownData.length) {
                        if (dropdownData[0].item) {
                            let rate = dropdownData[0].item.rate;
                            _$form.find("#SalesTaxRate").val(rate).change();
                        } else {
                            _$form.find("#SalesTaxRate").val(0).change().prop('readonly', false);
                        }
                    }
                    disableSalesTaxRateIfNeeded();
                });

                _$form.find('#SalesTaxRate').on('change', function () {
                    recalculateTotals();
                });

                _$form.find("#EditChargesButton").click(async function () {
                    await saveJobAsync();
                    _editChargesModal.open({
                        orderLineId: _$form.find("#OrderLineId").val(),
                    });
                });
            }

            function selectCustomerInControl(customer) {
                var option = new Option(customer.name, customer.id, true, true);
                $(option).data('data', {
                    id: customer.id,
                    text: customer.name,
                    item: {
                        accountNumber: customer.accountNumber,
                        customerIsCod: customer.customerIsCod,
                        pricingTierId: customer.pricingTierId,
                        isTaxExempt: customer.isTaxExempt,
                    }
                });
                _customerDropdown.append(option).trigger('change');
            }

            _$form.find("#JobPriority").select2Init({
                showAll: true,
                allowClear: false
            });

            _$form.find("#AutoGenerateTicketNumber").change(updateTicketNumberVisibility);

            _$form.find("#ProductionPay").change(function () {
                updateFreightRateForDriverPayVisibility();
                updateFreightRateToPayDriversValue();
            });

            _$form.find("#RequiresCustomerNotification").change(updateCustomerNotificationControlsVisibility);

            _$form.find('#IsMultipleLoads').change(() => updateRunUntilStopped(true));

            _contactDropdown.change(function () {
                var option = _contactDropdown.getSelectedDropdownOption();
                _$form.find("#ContactPhone").val(option.data("phoneNumber"));
            });

            _customerDropdown.change(function () {
                var dropdownData = _customerDropdown.select2('data');
                if (dropdownData && dropdownData.length) {
                    if (dropdownData[0].item) {
                        _$form.find("#CustomerAccountNumber").val(dropdownData[0].item.accountNumber);
                        if (dropdownData[0].item.customerIsCod) {
                            _$form.find("#CustomerAccountNumber").val('COD');
                            _$form.find("#CustomerAccountNumber").addClass('cod-account-number');
                            _modalManager.getModal().find('.subtitle').text('COD');
                        } else {
                            _$form.find("#CustomerAccountNumber").removeClass('cod-account-number');
                            _modalManager.getModal().find('.subtitle').text('');
                        }
                        _pricingTierId = dropdownData[0].item.pricingTierId;
                        _customerIsCod = dropdownData[0].item.customerIsCod;
                        _customerIsTaxExempt = dropdownData[0].item.isTaxExempt;

                        //Logic for populating on predefined values (Load At, Item, Material UOM)
                        if (areRequiredPricingFieldsFilled()) {
                            var sender = $(this);
                            reloadPricing(function () {
                                recalculate(sender);
                            });
                        }
                    } else {
                        _pricingTierId = null;
                        _customerIsCod = false;
                        _customerIsTaxExempt = false;
                        _modalManager.getModal().find('.subtitle').text('');
                    }
                }
                setIsTaxExempt(_customerIsTaxExempt, false);
                showSelectOrderLineButton();
            });

            _$form.find("#JobFuelSurchargeCalculationId").change(function () {
                let dropdownData = _$form.find("#JobFuelSurchargeCalculationId").select2('data');
                let selectedOption = dropdownData && dropdownData.length && dropdownData[0];
                let canChangeBaseFuelCost = selectedOption?.item?.canChangeBaseFuelCost || false;
                _$form.find("#BaseFuelCostContainer").toggle(canChangeBaseFuelCost);
                _$form.find("#BaseFuelCost").val(selectedOption?.item?.baseFuelCost || 0);
                _$form.find("#JobFuelSurchargeCalculationId").removeUnselectedOptions();
            });

            _modalManager.on('app.selectOrderQuoteModal.requestInput', function (callback) {
                callback(_quoteDropdown);
            });

            _orderIsTaxExempt.change(function () {
                updateTaxControls();
            });

            function updateTaxControls() {
                var salesTaxEntity = _$form.find("#JobSalesTaxEntityId");
                var salesTaxRate = _$form.find("#SalesTaxRate");
                if (_orderIsTaxExempt.is(':checked')) {
                    salesTaxEntity.val('').change().attr('disabled', true);
                    salesTaxRate.val(0).change().prop('readonly', true);
                } else {
                    salesTaxEntity.attr('disabled', false);
                    if (salesTaxEntity.val() === '') {
                        salesTaxRate.prop('readonly', false);
                    }
                }
            }
            updateTaxControls();

            //Quote change handling

            function updateInputValueIfSourceIsNotNull(input, sourceValue) {
                if (sourceValue !== null && sourceValue !== '') {
                    _$form.find(input).val(sourceValue).change();
                }
            }

            function updateInputValue(input, sourceValue) {
                _$form.find(input).val(sourceValue).change();
            }

            var handleQuoteChangeAsync = async function (quoteId, option) {
                if (quoteId === _quoteId) {
                    return;
                }
                _quoteId = quoteId;

                disableQuoteRelatedFieldsIfNeeded();

                if (_quoteId !== '') {
                    _contactDropdown.val(option.data('contactId')).change();
                    if (option.data('officeId')) {
                        abp.helper.ui.addAndSetDropdownValue(_$form.find("#JobOfficeId"), option.data('officeId'), option.data('officeName'));
                    }
                }
                updateInputValue("#PONumber", option.data('poNumber'));
                updateInputValue("#SpectrumNumber", option.data('spectrumNumber'));
                updateInputValue("#Directions", option.data('directions'));
                if (abp.session.officeCopyChargeTo) {
                    updateInputValue("#ChargeTo", option.data('chargeTo'));
                }

                if (_quoteId !== '') {
                    let fuelSurchargeCalculationId = option.data('fuelSurchargeCalculationId');
                    if (fuelSurchargeCalculationId) {
                        abp.helper.ui.addAndSetDropdownValue(_$form.find("#JobFuelSurchargeCalculationId"), fuelSurchargeCalculationId, option.data('fuelSurchargeCalculationName'));
                        updateInputValue("#BaseFuelCost", option.data('baseFuelCost'));
                    } else {
                        _$form.find("#JobFuelSurchargeCalculationId").val(null).change();
                        updateInputValue("#BaseFuelCost", 0);
                    }
                    _$form.find("#BaseFuelCostContainer").toggle(option.data('canChangeBaseFuelCost') === true);
                    setIsTaxExempt(option.data('customerIsTaxExempt'), option.data('quoteIsTaxExempt'));
                } else {
                    let defaultFuelSurchargeCalculationId = abp.setting.getInt('App.Fuel.DefaultFuelSurchargeCalculationId');
                    let defaultFuelSurchargeCalculationName = _$form.find("#DefaultFuelSurchargeCalculationName").val();
                    if (defaultFuelSurchargeCalculationId > 0) {
                        abp.helper.ui.addAndSetDropdownValue(_$form.find("#JobFuelSurchargeCalculationId"), defaultFuelSurchargeCalculationId, defaultFuelSurchargeCalculationName);
                    } else {
                        _$form.find("#JobFuelSurchargeCalculationId").val(null).change();
                    }
                    updateInputValue("#BaseFuelCost", _$form.find("#DefaultBaseFuelCost").val());
                    _$form.find("#BaseFuelCostContainer").toggle(_$form.find("#DefaultCanChangeBaseFuelCost").val() === 'True');
                    setIsTaxExempt(_customerIsTaxExempt, false);
                }
                _$form.find("#JobFuelSurchargeCalculationId").prop("disabled", !!_quoteId && !abp.auth.hasPermission('Pages.Orders.EditQuotedValues'));
                if (_quoteId) {
                    try {
                        abp.ui.setBusy();
                        let quoteLinesData = await _orderAppService.getOrderLines({ quoteId: _quoteId });
                        if (quoteLinesData.items.length === 1) {
                            setOrderLine(quoteLinesData.items[0]);
                        } else if (quoteLinesData.items.length > 1) {
                            openAddQuoteBasedOrderLinesModal();
                        }
                    }
                    finally {
                        abp.ui.clearBusy();
                    }
                }
            };

            function openAddQuoteBasedOrderLinesModal() {
                _addQuoteBasedOrderLinesModal.open({
                    titleText: "Select the desired line item",
                    saveButtonText: "Add Item",
                    limitSelectionToSingleOrderLine: true,
                    quoteId: _quoteId,
                }).done(function (modal, modalObject) {
                    modalObject.setFilter({ quoteId: _quoteId });
                    showSelectOrderLineButton();
                });
            }

            _$form.find("#OpenQuoteBasedOrderLinesModalButton").click(function () {
                openAddQuoteBasedOrderLinesModal();
            });

            var quoteIdChanging = false;
            _quoteDropdown.change(async function () {
                if (quoteIdChanging) {
                    return;
                }
                var newQuoteId = _quoteDropdown.val();
                var option = _quoteDropdown.getSelectedDropdownOption();
                if (option.data('status') === abp.enums.quoteStatus.pending) {
                    if (await abp.message.confirm(
                        "This quote is 'Pending'. If you add this quote to the order it will be changed to 'Active'. If you do not want the quote to be activated, select 'Cancel'."
                    )) {
                        option.data('status', abp.enums.quoteStatus.active);
                        await handleQuoteChangeAsync(newQuoteId, option);
                        abp.services.app.quote.setQuoteStatus({ id: _quoteId, status: abp.enums.quoteStatus.active });
                    } else {
                        quoteIdChanging = true;
                        //check if the old value is still in the dropdown, and set the quote to "" if not
                        if (_quoteDropdown.getDropdownOption(_quoteId).length) {
                            _quoteDropdown.val(_quoteId).change();
                        } else {
                            _quoteDropdown.val('').change();
                        }
                        quoteIdChanging = false;
                    }
                } else {
                    await handleQuoteChangeAsync(newQuoteId, option);
                }
            });

            _modalManager.on('app.quoteBasedOrderLinesSelectedModal', function (eventData) {
                if (!eventData.selectedLines.length) {
                    return;
                }
                if (isNewOrder()) {
                    setOrderLine(eventData.selectedLines[0]);
                } else {
                    //abp.ui.setBusy();
                    setOrderLine(eventData.selectedLines[0]);
                    //saving those will add more order lines to the job instead of replacing the single one, so saving is commented out for now.
                    //this way, they will save the main modal and will replace the values in the single existing orderline
                    //_orderAppService.editOrderLines(eventData.selectedLines).done(function () {
                    //    abp.notify.info('Saved successfully.');
                    //}).always(function () {
                    //    abp.ui.clearBusy();
                    //});
                }
            });

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

            if (isNewOrder()) {
                setDefaultValueForProductionPay();
            }

            _designationDropdown.change(function () {
                setDefaultValuesForCounterSaleDesignationIfNeeded();
                updateSaveButtonsVisibility();

                if (!_features.separateItems) {
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
                updateDesignationRelatedFieldsVisibility();

                _initializeMaterialItemDropdown();
                _initializeMaterialUomDropdown();
                updateProductionPay();
                updateFreightRateForDriverPayVisibility();
            }).change();

            reloadPricing();
            refreshHighlighting();

            _materialItemDropdown.change(function () {
                var sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
                var dropdownData = _materialItemDropdown.select2('data');

                if (dropdownData && dropdownData.length && dropdownData[0].item) {
                    _$form.find("#IsMaterialTaxable").val(dropdownData[0].item.isTaxable ? "True" : "False");
                }
            });

            _freightItemDropdown.change(function () {
                let sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
                let dropdownData = _freightItemDropdown.select2('data');
                if (dropdownData && dropdownData.length && dropdownData[0].item) {
                    _$form.find("#IsTaxable, #IsFreightTaxable").val(dropdownData[0].item.isTaxable ? "True" : "False");
                    _useZoneBasedRatesInput.val(dropdownData[0].item.useZoneBasedRates ? 'True' : 'False');
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
                recalculateTotals();
            });
            _freightPriceInput.change(function () {
                _saveEventArgs.reloadFreightTotalIfNotOverridden = true;
                setIsFreightPriceOverridden(true);
                recalculateTotals();
            });

            _modalManager.getModal().find(".save-job-button").click(async function (e) {
                e.preventDefault();
                await saveJobAsync();
                _modalManager.close();
            });

            _modalManager.getModal().find("#SaveAndPrintButton").click(async function (e) {
                e.preventDefault();
                let saveResult = await saveJobAsync();
                if (!saveResult.ticketId) {
                    abp.message.error(app.localize('TicketNumberIsRequired'));
                    return;
                }
                _$form.find("#TicketId").val(saveResult.ticketId);
                app.openPopup(abp.appPath + 'app/tickets/GetTicketPrintOut?ticketId=' + saveResult.ticketId);
                _modalManager.close();
            });

            recalculateTotals();
            initOverrideButtons();
            disableProductionPayIfNeeded(false);
            disableQuoteRelatedFieldsIfNeeded();
            disableFieldsIfEditingJob();
            updateDesignationRelatedFieldsVisibility();
            if (isNewOrder()) {
                setDefaultValuesForCounterSaleDesignationIfNeeded();
            }
            updateSaveButtonsVisibility();
            disableJobEditIfNeeded();
            disableTaxControls();
            disableSalesTaxRateIfNeeded();
            updateCustomerNotificationControlsVisibility();
            updateFreightRateForDriverPayVisibility();
            updateTravelTimeVisibility();
            updateRunUntilStopped();
        };

        this.focusOnDefaultElement = function () {
            var focusFieldId = _$form.find('#FocusFieldId').val();
            if (focusFieldId !== '') {
                _$form.find('#' + focusFieldId).focus();
            }
        }

        function setIsTaxExempt(customerIsTaxExempt, quoteIsTaxExempt) {
            var isTaxExempt;
            if (_quoteId !== '') {
                isTaxExempt = quoteIsTaxExempt;
            } else {
                isTaxExempt = customerIsTaxExempt;
            }
            _orderIsTaxExempt.prop('checked', isTaxExempt).change();
        }

        function disableOrderEditForLeaseHaulerPortalUser() {
            let controlsToDisable = [
                '#DeliveryDate',
                '#JobCustomerId',
                '#QuoteId',
                '#JobContactId',
                '#ContactPhone',
                '#JobShift',
                '#JobOfficeId',
                '#JobNumber',
                '#JobDesignation',
                '#LoadAtId',
                '#DeliverToId',
                '#JobFreightItemId',
                '#JobMaterialItemId',
                '#VehicleCategories',
                '#BedConstruction',
                '#FreightUomId',
                '#MaterialUomId',
                '#ProductionPay',
                '#FreightQuantity',
                '#MaterialQuantity',
                '#LeaseHaulerRate',
                '#NumberOfTrucks',
                '#IsMultipleLoads',
                '#DefaultFuelSurchargeCalculationName',
            ];

            if (_permissions.leaseHaulerTruckRequest) {
                controlsToDisable.push('#TimeOnJob');
                controlsToDisable.push('#ChargeTo');
                controlsToDisable.push('#JobPriority');
                controlsToDisable.push('#Note');
                controlsToDisable.push('#RequiresCustomerNotification');
                controlsToDisable.push('#CustomerNotificationContactName');
                controlsToDisable.push('#CustomerNotificationPhoneNumber');
                controlsToDisable.push('#JobFuelSurchargeCalculationId');
                controlsToDisable.push('#BaseFuelCost');
                controlsToDisable.push('#AutoGenerateTicketNumber');
                controlsToDisable.push('#TicketNumber');
            }

            _$form.find('input,select,textarea')
                .filter(controlsToDisable.join(', '))
                .attr('disabled', true);
        }

        function disableOrderEditForHaulingCompany() {
            _$form.find('input,select,textarea').not('#SalesTaxRate, #JobSalesTaxEntityId, #JobFuelSurchargeCalculationId, #BaseFuelCost, .order-line-field').attr('disabled', true);
        }

        function disableJobEditIfNeeded() {
            if (!_permissions.edit) {
                if (_permissions.editLeaseHaulerJob || _permissions.leaseHaulerTruckRequest) {
                    //only disable some fields
                    disableOrderEditForLeaseHaulerPortalUser();

                    if (_permissions.leaseHaulerTruckRequest) {
                        _modalManager.getModal().find('.save-job-button').hide();
                    }
                } else {
                    //otherwise disable all fields
                    _$form.find('input,select,textarea,button').attr('disabled', true);
                    _modalManager.getModal().find('.save-button-dropdown').hide();
                }
            } else {
                //even with full 'edit' permissions, for a Hauling Company tenant viewing Material Company order some fields should be disabled
                if (_$form.find("#MaterialCompanyOrderId").val()) {
                    disableOrderEditForHaulingCompany();
                }
            }
        }

        function disableTaxControls() {
            var taxCalculationType = abp.setting.getInt('App.Invoice.TaxCalculationType');
            if (taxCalculationType === abp.enums.taxCalculationType.noCalculation
                && !_features.separateItems
            ) {
                _$form.find("#SalesTaxRate, #JobSalesTaxEntityId").parent().hide();
            } else {
                //an older control used to enter the total tax amount manually when "No Calculation" type was used
                //deprecated on new views
                _$form.find("#SalesTax").parent().hide();
            }
        }

        function disableSalesTaxRateIfNeeded() {
            if (_$form.find("#JobSalesTaxEntityId").val()) {
                _$form.find("#SalesTaxRate").prop('readonly', true);
            }
        }

        function isNewOrder() {
            return _orderId === '';
        }

        function setOrderLine(orderLine) {
            if (_orderId) {
                orderLine.orderId = _orderId;
            }

            _orderLine = orderLine;
            if (!_$form) {
                return;
            }

            _initializing = true;
            //_$form.find("#Id").val(_orderLine.id);
            //_$form.find("#OrderId").val(_orderLine.orderId);
            //_$form.find("#QuoteId").val(_orderLine.quoteId);
            _$form.find("#QuoteLineId").val(_orderLine.quoteLineId);
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
            //_$form.find("#LineNumber").val(_orderLine.lineNumber);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#JobDesignation"), _orderLine.designation, _orderLine.designationName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#LoadAtId"), _orderLine.loadAtId, _orderLine.loadAtName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#DeliverToId"), _orderLine.deliverToId, _orderLine.deliverToName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#JobFreightItemId"), _orderLine.freightItemId, _orderLine.freightItemName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#JobMaterialItemId"), _orderLine.materialItemId, _orderLine.materialItemName);
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

            //_quoteId = _$form.find("#QuoteId").val();
            _quoteLineId = _$form.find("#QuoteLineId").val();

            updateStaggeredTimeControls();
            updateTimeOnJobInput();
            updateProductionPay();
            disableProductionPayIfNeeded(false);
            disableQuoteRelatedFieldsIfNeeded();
            showSelectOrderLineButton();

            _initializing = false;
            reloadPricing();

            refreshTotalFields();
            refreshOverrideButtons();
            refreshHighlighting();
        }

        function setDefaultValueForProductionPay() {
            _$form.find("#ProductionPay").prop('checked', abp.setting.getBoolean('App.TimeAndPay.DefaultToProductionPay'));
        }

        function showSelectOrderLineButton() {
            if (_quoteId && !_quoteLineId) {
                _$form.find("#JobDesignation").parent().hide();
                _$form.find("#OpenQuoteBasedOrderLinesModalButton").parent().show();
            } else {
                _$form.find("#JobDesignation").parent().show();
                _$form.find("#OpenQuoteBasedOrderLinesModalButton").parent().hide();
            }
        }

        function disableQuoteRelatedFieldsIfNeeded() {
            var isQuoteSet = !!_quoteId;
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
                .prop('disabled', isQuoteSet && !abp.auth.hasPermission('Pages.Orders.EditQuotedValues'));
        }

        function disableFieldsIfEditingJob() {
            if (!isNewOrder()) {
                _customerDropdown
                    //.add(_designationDropdown)
                    .prop('disabled', true);
            }
        }

        function updateDesignationRelatedFieldsVisibility() {
            var designationRelatedFields = _$form.find("#DesignationRelatedFields");
            if (_designationDropdown.val()) {
                updateControlsVisibility();
                if (_features.separateItems) {
                    var designation = Number(_designationDropdown.val());

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

                _materialUomDropdown.attr('required', true);
                _materialUomDropdown.closest('.form-group').find('label').toggleClass('required-label', true);

                designationRelatedFields.show();
            } else {
                designationRelatedFields.hide();
            }
        }

        function updateControlsVisibility() {
            var designation = Number(_designationDropdown.val());
            var designationIsCounterSale = designation === abp.enums.designation.materialOnly && _allowCounterSales && _permissions.counterSales;
            _deliverToDropdown.closest('.form-group').toggle(!designationIsCounterSale);
            _$form.find("#LeaseHaulerRate").closest('.form-group').toggle(!designationIsCounterSale);
            _$form.find("#NumberOfTrucks").closest('.form-group').toggle(!designationIsCounterSale);
            _$form.find("#IsMultipleLoads").closest('.form-group').toggle(!designationIsCounterSale);
            _$form.find("#TimeOnJob").closest('.form-group').toggle(!designationIsCounterSale);
            _$form.find("#ChargeTo").closest('.form-group').toggle(!designationIsCounterSale);
            _$form.find("#JobPriority").closest('.form-group').toggle(!designationIsCounterSale);
            _$form.find("#ProductionPay").closest('.form-group').toggle(!designationIsMaterialOnly());
            _$form.find("#AutoGenerateTicketNumber").closest('.form-group').toggle(designationIsCounterSale);
            _$form.find("#TicketNumber").closest('.form-group').toggle(designationIsCounterSale && !_$form.find('#AutoGenerateTicketNumber').is(':checked'));
        }

        function setDefaultValuesForCounterSaleDesignationIfNeeded() {
            var designation = Number(_designationDropdown.val());
            if (designation !== abp.enums.designation.materialOnly || !_allowCounterSales || !_permissions.counterSales) {
                return;
            }
            if (_$form.find("#DefaultLoadAtLocationId").val()) {
                abp.helper.ui.addAndSetDropdownValue(_$form.find("#LoadAtId"), _$form.find("#DefaultLoadAtLocationId").val(), _$form.find("#DefaultLoadAtLocationName").val());
            }
            if (_$form.find("#DefaultMaterialItemId").val()) {
                abp.helper.ui.addAndSetDropdownValue(
                    _$form.find(_features.separateItems ? "#JobMaterialItemId" : "#JobFreightItemId"),
                    _$form.find("#DefaultMaterialItemId").val(),
                    _$form.find("#DefaultMaterialItemName").val()
                );
            }
            if (_$form.find("#DefaultMaterialUomId").val()) {
                abp.helper.ui.addAndSetDropdownValue(_$form.find("#MaterialUomId"), _$form.find("#DefaultMaterialUomId").val(), _$form.find("#DefaultMaterialUomName").val());
            }
        }

        function updateTicketNumberVisibility() {
            _$form.find("#TicketNumber").closest('.form-group').toggle(!_$form.find('#AutoGenerateTicketNumber').is(':checked'));
        }

        function updateCustomerNotificationControlsVisibility() {
            _$form.find("#CustomerNotificationContactName").closest('.form-group').toggle(_$form.find('#RequiresCustomerNotification').is(':checked'));
            _$form.find("#CustomerNotificationPhoneNumber").closest('.form-group').toggle(_$form.find('#RequiresCustomerNotification').is(':checked'));
        }

        function updateSaveButtonsVisibility() {
            var designation = Number(_designationDropdown.val());
            var designationIsCounterSale = designation === abp.enums.designation.materialOnly && _allowCounterSales && _permissions.counterSales;
            _modalManager.getModal().find(".save-button-container").toggle(!designationIsCounterSale);
            _modalManager.getModal().find(".save-and-print-buttons-container").toggle(designationIsCounterSale);
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
            if (!_permissions.edit) {
                return;
            }
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
            recalculateTotals();
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

        function recalculateTotals() {
            let rate = _$form.find('#SalesTaxRate').val();
            let isTaxable = _$form.find("#IsTaxable").val() !== "False";
            let isMaterialTaxable = _$form.find("#IsMaterialTaxable").val() !== "False";
            let isFreightTaxable = _$form.find("#IsFreightTaxable").val() !== "False";
            let materialExtendedAmount = _$form.find("#MaterialPrice").val();
            let freightExtendedAmount = _$form.find("#FreightPrice").val();
            let calcResult = abp.helper.calculateOrderLineTotal(materialExtendedAmount, freightExtendedAmount, isTaxable, rate, isMaterialTaxable, isFreightTaxable);
            _$form.find("#OrderLineSalesTax").val(calcResult.tax).change();
            _$form.find("#OrderLineTotal").val(calcResult.total).change();
        }

        function round(num) {
            return abp.utils.round(num);
        }

        function designationHasMaterial() {
            var designation = Number(_designationDropdown.val());
            return abp.enums.designations.hasMaterial.includes(designation);
        }

        function designationIsMaterialOnly() {
            return abp.enums.designations.materialOnly.includes(Number(_designationDropdown.val()));
        }

        function designationIsFreightOnly() {
            return abp.enums.designations.freightOnly.includes(Number(_designationDropdown.val()));
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

        async function checkForOrderDuplicates(model) {
            if (model.orderId) {
                return;
            }

            let duplicateCount = await _orderAppService.getOrderDuplicateCount({
                id: model.orderId,
                customerId: model.customerId,
                quoteId: model.quoteId,
                deliveryDate: model.deliveryDate
            });


            if (duplicateCount > 0) {
                var customerName = _customerDropdown.getSelectedDropdownOption().text();
                if (!await abp.message.confirm(
                    'You already have an order scheduled for ' + model.deliveryDate + ' for ' + customerName + '. Are you sure you want to save this order?'
                )) {
                    throw new Error('Stopping save because duplicate order was found and user disagreed to continue.');
                }
            }
        }

        function hasMissingQuantityOrNumberOfTrucks(model) {
            var designation = Number(_designationDropdown.val());
            var designationIsCounterSale = designation === abp.enums.designation.materialOnly && _allowCounterSales;
            if (designationIsCounterSale) {
                if (model.materialQuantity || model.freightQuantity) {
                    return false;
                }
            } else {
                if (model.materialQuantity || model.freightQuantity || model.numberOfTrucks) {
                    return false;
                }
            }
            return true;
        }

        function updateRunUntilStopped(ignoreOrderLineId) {
            let runUntilStoppedInput = _$form.find('#IsMultipleLoads');
            var model = _$form.serializeFormToObject();
            model.OrderLineId = model.OrderLineId ? Number(model.OrderLineId) : null;
            if (freightUomDropdownHasHours() && (!model.OrderLineId || ignoreOrderLineId || !runUntilStoppedInput.is(':checked'))) {
                runUntilStoppedInput.prop('disabled', true).prop('checked', false);
            } else {
                runUntilStoppedInput.prop('disabled', false);
            }
        }

        var saveJobAsync = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                throw new Error('Stopping save because form is invalid.');
            }
            if (!app.order.validatreHourlyDriverPayRateInputs(_hourlyDriverPayRateInputs)) {
                throw new Error('Stopping save because form is invalid.');
            }

            var model = _$form.serializeFormToObject();
            model.OrderId = model.OrderId ? Number(model.OrderId) : null;
            model.OrderLineId = model.OrderLineId ? Number(model.OrderLineId) : null;
            model.QuoteId = model.QuoteId ? Number(model.QuoteId) : null;
            model.QuoteLineId = model.QuoteLineId ? Number(model.QuoteLineId) : null;

            if (!parseFloat(model.MaterialQuantity) && parseFloat(model.MaterialPrice)
                || !parseFloat(model.FreightQuantity) && parseFloat(model.FreightPrice)
            ) {
                abp.message.error(app.localize('QuantityIsRequiredWhenTotalIsSpecified'));
                throw new Error('Stopping save because form is invalid.');
            }

            if (model.RequiresCustomerNotification && (model.CustomerNotificationContactName === "" || model.CustomerNotificationPhoneNumber === "")) {
                abp.message.error('Please check the following: \n'
                    + (model.CustomerNotificationContactName ? '' : '"Contact Name" - This field is required.\n')
                    + (model.CustomerNotificationPhoneNumber ? '' : '"Phone Number" - This field is required.\n'), 'Some of the data is invalid');
                throw new Error('Stopping save because form is invalid.');
            }

            if (model.CustomerNotificationPhoneNumber) {
                if (!_$form.find("#CustomerNotificationPhoneNumber")[0].checkValidity()) {
                    abp.message.error(app.localize('IncorrectPhoneNumberFormatError'));
                    throw new Error('Stopping save because form is invalid.');
                }
            }

            if (!validateFields(model)) {
                throw new Error('Stopping save because form is invalid.');
            }

            if (Number(model.StaggeredTimeKind) !== abp.enums.staggeredTimeKind.none) {
                model.TimeOnJob = null;
            }

            _model = _model || {};
            _model.orderId = model.OrderId;
            _model.orderLineId = model.OrderLineId;
            _model.quoteId = model.QuoteId;
            _model.quoteLineId = model.QuoteLineId;
            _model.isMaterialPricePerUnitOverridden = model.IsMaterialPricePerUnitOverridden === "True";
            _model.isFreightPricePerUnitOverridden = model.IsFreightPricePerUnitOverridden === "True";
            _model.isFreightRateToPayDriversOverridden = model.IsFreightRateToPayDriversOverridden === "True";
            _model.isLeaseHaulerPriceOverridden = model.IsLeaseHaulerPriceOverridden === "True";
            _model.isMaterialPriceOverridden = model.IsMaterialPriceOverridden === "True";
            _model.isFreightPriceOverridden = model.IsFreightPriceOverridden === "True";
            _model.isTaxable = model.IsTaxable === "True";
            _model.isTaxExempt = !!model.IsTaxExempt;
            _model.staggeredTimeKind = Number(model.StaggeredTimeKind) || 0;
            //_model.lineNumber = Number(model.LineNumber); //?
            _model.deliveryDate = model.DeliveryDate;
            _model.customerId = model.CustomerId;
            _model.chargeTo = model.ChargeTo;
            _model.poNumber = model.PONumber;
            _model.spectrumNumber = model.SpectrumNumber;
            _model.directions = model.Directions;
            _model.salesTaxRate = model.SalesTaxRate;
            _model.salesTaxEntityId = model.SalesTaxEntityId;
            _model.salesTax = model.SalesTax;
            _model.priority = model.Priority;
            _model.shift = model.Shift;
            _model.officeId = model.OfficeId;
            _model.contactId = model.ContactId;
            _model.designation = model.Designation;
            _model.designationName = Number(model.Designation) ? _$form.find("#JobDesignation option:selected").text() : null;
            _model.loadAtId = model.LoadAtId;
            _model.loadAtName = Number(model.LoadAtId) ? _$form.find("#LoadAtId option:selected").text() : null;
            _model.deliverToId = model.DeliverToId;
            _model.deliverToName = Number(model.DeliverToId) ? _$form.find("#DeliverToId option:selected").text() : null;
            _model.freightItemId = model.FreightItemId;
            _model.freightItemName = Number(model.FreightItemId) ? _$form.find("#JobFreightItemId option:selected").text() : null;
            _model.materialItemId = model.MaterialItemId;
            _model.materialItemName = Number(model.MaterialItemId) ? _$form.find("#JobMaterialItemId option:selected").text() : null;
            _model.materialUomId = model.MaterialUomId;
            _model.materialUomName = Number(model.MaterialUomId) ? _$form.find("#MaterialUomId option:selected").text() : null;
            _model.freightUomId = model.FreightUomId;
            _model.freightUomName = Number(model.FreightUomId) ? _$form.find("#FreightUomId option:selected").text() : null;
            _model.freightUomBaseId = _freightUomDropdown.getUomBaseId();
            _model.materialPricePerUnit = Number(model.MaterialPricePerUnit) || 0;
            _model.materialCostRate = Number(model.MaterialCostRate) || 0;
            _model.freightPricePerUnit = Number(model.FreightPricePerUnit) || 0;
            _model.freightRateToPayDrivers = Number(model.FreightRateToPayDrivers) || 0;
            _model.loadBased = !!model.LoadBased;
            _model.leaseHaulerRate = Number(model.LeaseHaulerRate) || 0;
            _model.driverPayTimeClassificationId = model.DriverPayTimeClassificationId;
            _model.driverPayTimeClassificationName = Number(model.DriverPayTimeClassificationId) ? _hourlyDriverPayRateInputs.driverPayTimeClassificationDropdown.find('option:selected').text() : null;
            _model.hourlyDriverPayRate = Number(model.HourlyDriverPayRate) || 0;
            _model.materialQuantity = Number(model.MaterialQuantity) || 0;
            _model.freightQuantity = Number(model.FreightQuantity) || 0;
            _model.travelTime = Number(model.TravelTime) || 0;
            _model.materialPrice = Number(model.MaterialPrice) || 0;
            _model.freightPrice = Number(model.FreightPrice) || 0;
            _model.numberOfTrucks = Number(model.NumberOfTrucks) || 0;
            _model.isMultipleLoads = !!model.IsMultipleLoads;
            _model.productionPay = !!model.ProductionPay;
            _model.timeOnJob = model.TimeOnJob;
            _model.jobNumber = model.JobNumber;
            _model.note = model.Note;
            _model.autoGenerateTicketNumber = !!model.AutoGenerateTicketNumber;
            _model.ticketId = model.TicketId;
            _model.ticketNumber = model.TicketNumber;
            _model.fuelSurchargeCalculationId = model.FuelSurchargeCalculationId;
            _model.baseFuelCost = model.BaseFuelCost;
            _model.requiresCustomerNotification = !!model.RequiresCustomerNotification;
            _model.requireTicket = !!model.RequireTicket;
            _model.customerNotificationContactName = model.CustomerNotificationContactName;
            _model.customerNotificationPhoneNumber = model.CustomerNotificationPhoneNumber;
            _model.vehicleCategories = _$form.find("#VehicleCategories").select2('data').map(x => ({ id: x.id, name: x.text }));
            _model.bedConstruction = model.BedConstruction === "" ? null : Number(model.BedConstruction) || 0;
            _model.bedConstructionName = model.BedConstruction === "" ? null : _$form.find("#BedConstruction option:selected").text();

            let materialQuantity = model.MaterialQuantity === "" ? null : abp.utils.round(parseFloat(model.MaterialQuantity));
            let freightQuantity = model.FreightQuantity === "" ? null : abp.utils.round(parseFloat(model.FreightQuantity));
            let numberOfTrucks = model.NumberOfTrucks === "" ? null : abp.utils.round(parseFloat(model.NumberOfTrucks));

            if (model.OrderLineId && !await abp.scheduling.checkExistingDispatchesBeforeSettingQuantityAndNumberOfTrucksZero(model.OrderLineId, materialQuantity, freightQuantity, numberOfTrucks)) {
                _modalManager.close();
                throw new Error('Stopping save because existing dispatches were found and user disagreed to continue.');
            }

            if (hasMissingQuantityOrNumberOfTrucks(_model) && !abp.setting.getBoolean('App.UserOptions.DontShowZeroQuantityWarning')) {
                if (!await abp.message.confirm(app.localize('MissingQtyOrNbrOfTrucksOnJobConfirmation'))) {
                    throw new Error('Stopping save because missing quantity or number of trucks on job is missing and user disagreed to continue.');
                }
            }

            try {
                _modalManager.setBusy(true);

                if (!isNewOrder()
                    && _model.orderLineId
                    && _model.staggeredTimeKind === abp.enums.staggeredTimeKind.none
                    && _model.timeOnJob != _timeOnJobLastValue
                    && _permissions.edit
                ) {
                    var timeOnJobValidationResult = await _orderAppService.validateOrderLineTimeOnJob({ orderLineId: _model.orderLineId });

                    if (!timeOnJobValidationResult.hasOrderLineTrucks) {
                        _model.updateOrderLineTrucksTimeOnJob = false;
                        _model.updateDispatchesTimeOnJob = false;
                    } else {
                        _model.updateOrderLineTrucksTimeOnJob =
                            !timeOnJobValidationResult.hasDisagreeingOrderLineTrucks
                            || !!await abp.message.confirmWithYesNo(app.localize('TimeOnJobDisagreeingTrucksValidationMessage'));

                        _model.updateDispatchesTimeOnJob =
                            _model.updateOrderLineTrucksTimeOnJob
                            && (
                                !timeOnJobValidationResult.hasOpenDispatches
                                || !!await abp.message.confirmWithYesNo(app.localize('TimeOnJobOpenDispatchesValidationMessage'))
                            );
                    }
                }

                await checkForOrderDuplicates(_model);

                let editResult = await _orderAppService.editJob(_model);
                if (!editResult.completed) {
                    if (!await abp.helper.showTruckWarning(editResult.notAvailableTrucks, 'already scheduled for this date. Do you want to continue with remaining trucks?')) {
                        throw new Error('Stopping save because some trucks are already scheduled for this date and user disagreed to continue');
                    }
                    _model.removeNotAvailableTrucks = true;
                    editResult = await _orderAppService.editJob(_model);
                }

                _$form.find("#OrderLineId").val(editResult.orderLineId);
                _$form.find("#OrderId").val(editResult.id);
                _$form.find("#TicketId").val(editResult.ticketId);

                abp.notify.info('Saved successfully.');
                abp.event.trigger('app.createOrEditJobModalSaved', editResult);

                return editResult;

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

    };
})(jQuery);
