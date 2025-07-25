(function ($) {
    app.modals.CreateOrEditOrderModal = function () {

        var _orderAppService = abp.services.app.order;
        var _orderPaymentService = abp.services.app.orderPayment;
        var _dtHelper = abp.helper.dataTables;
        var _orderId = null;
        var _quoteId = null;
        var _order = null;
        var _freightTotal = null;
        var _materialTotal = null;
        var _orderLines = [];
        var _orderLinesGridData = null;
        var _isOrderReadonly = false;
        var _pricingTierId;
        var _customerIsTaxExempt = null;
        var _customerIsCod = null;
        var _permissions = {
            edit: abp.auth.hasPermission('Pages.Orders.Edit')
        };
        var _modalManager;
        var _$modal = null;
        var _$form = null;
        var _quoteDropdown = null;
        var _orderIsTaxExempt = null;
        var _separateMaterialAndFreightItemsFeature = abp.features.isEnabled('App.SeparateMaterialAndFreightItems');

        //Init modals

        var _createOrEditOrderLineModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CreateOrEditOrderLineModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CreateOrEditOrderLineModal.js',
            modalClass: 'CreateOrEditOrderLineModal'
        });

        var _createOrEditOrderModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CreateOrEditOrderModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CreateOrEditOrderModal.js',
            modalClass: 'CreateOrEditOrderModal',
            modalSize: 'xl'
        });

        var _addQuoteBasedOrderLinesModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/AddQuoteBasedOrderLinesModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_AddQuoteBasedOrderLinesModal.js',
            modalClass: 'AddQuoteBasedOrderLinesModal',
            modalSize: 'xl'
        });

        var _createOrEditTicketModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CreateOrEditTicketModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CreateOrEditTicketModal.js',
            modalClass: 'CreateOrEditTicketModal',
            modalSize: 'lg'
        });

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

        var _copyOrderModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CopyOrderModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CopyOrderModal.js',
            modalClass: 'CopyOrderModal'
        });

        var _editInternalNotesModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CreateOrEditOrderInternalNotesModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CreateOrEditOrderInternalNotesModal.js',
            modalClass: 'CreateOrEditOrderInternalNotesModal'
        });

        var _authorizeOrderChargeModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/AuthorizeOrderChargeModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_AuthorizeOrderChargeModal.js',
            modalClass: 'AuthorizeOrderChargeModal'
        });

        var _emailOrderReportModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/EmailOrderReportModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_EmailOrderReportModal.js',
            modalClass: 'EmailOrderReportModal'
        });

        var _selectOrderQuoteModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/SelectOrderQuoteModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_SelectOrderQuoteModal.js',
            modalClass: 'SelectOrderQuoteModal'
        });

        var _printOrderWithDeliveryInfoModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/PrintOrderWithDeliveryInfoModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_PrintOrderWithDeliveryInfoModal.js',
            modalClass: 'PrintOrderWithDeliveryInfoModal'
        });

        var _setOrderLineNoteModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/SetOrderLineNoteModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_SetOrderLineNoteModal.js',
            modalClass: 'SetOrderLineNoteModal'
        });

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$modal = _modalManager.getModal();
            _$form = _$modal.find('form');
            _$form.validate();

            _quoteDropdown = _$form.find("#OrderQuoteId");
            _orderIsTaxExempt = _$form.find("#OrderIsTaxExempt");

            _orderId = _$form.find("#Id").val();
            _quoteId = _quoteDropdown.val();
            _customerIsCod = _$form.find('#CustomerIsCod').val() === 'True';
            _customerIsTaxExempt = _$form.find('#CustomerIsTaxExempt').val() === 'True';

            //Common functions

            function loseFocusAndAwaitBackgroundTasks() {
                $(':focus').blur();
                var waitingTime = 0;
                abp.ui.setBusy(_$form);

                return new Promise((resolve, reject) => {
                    function awaitBackgroundTasksAndResolve(resolve, reject) {
                        waitingTime++;
                        if (waitingTime > 100) { //100ms * 100, 10 seconds
                            abp.ui.clearBusy(_$form);
                            abp.message.error('Something went wrong. Please refresh the page and try again.');
                            reject(new Error('Stopping save because background tasks did not finish in time.'));
                            return;
                        }
                        if (_recalculateTotalsInProgressCount > 0) {
                            //check again in 100ms
                            setTimeout(function () {
                                awaitBackgroundTasksAndResolve(resolve, reject);
                            }, 100);
                        } else {
                            abp.ui.clearBusy(_$form);
                            resolve();
                        }
                    }
                    //let the blur trigger the background tasks first and only then check for the first time if anything is running
                    setTimeout(function () {
                        awaitBackgroundTasksAndResolve(resolve, reject);
                    }, 100);
                });
            }

            async function saveOrderIfNeededAsync() {
                if (isNewOrChangedOrder()) {
                    await saveOrderAsync();
                }
            }

            async function saveOrderAsync() {
                await loseFocusAndAwaitBackgroundTasks();
                await saveOrderImmediatelyAsync();
            }

            async function saveOrderImmediatelyAsync() {
                if (!_$form.valid()) {
                    _$form.showValidateMessage();
                    throw new Error('Stopping save because form is invalid.');
                }

                var order = _$form.serializeFormToObject();
                if (isNewOrder()) {
                    order.OrderLines = _orderLines;
                }

                order.IsPending = _$form.find("#IsPending").prop("checked");
                order.isTaxExempt = _$form.find("#OrderIsTaxExempt").prop("checked");

                try {
                    _modalManager.setBusy(true);
                    abp.ui.setBusy(_$form);

                    await checkForOrderDuplicates(order);

                    let editResult = await _orderAppService.editOrder(order);
                    if (!editResult.completed) {
                        if (!await abp.helper.showTruckWarning(editResult.notAvailableTrucks, 'already scheduled for this date. Do you want to continue with remaining trucks?')) {
                            throw new Error('Stopping save because some trucks are already scheduled for this date and user disagreed to continue');
                        }
                        order.removeNotAvailableTrucks = true;
                        editResult = await _orderAppService.editOrder(order);
                    }

                    abp.notify.info('Saved successfully.');
                    let orderId = editResult.id;
                    $("#Id").val(orderId);
                    _orderId = orderId;
                    showEditingBlocks();
                    _orderLinesGridData = null;
                    reloadOrderLinesGridAsync();
                    updateLastModifiedDates();
                    _$form.dirtyForms('setClean');
                    if (editResult.hasZeroQuantityItems && !abp.setting.getBoolean('App.UserOptions.DontShowZeroQuantityWarning')) {
                        abp.message.warn(app.localize('MissingQtyOrNbrOfTrucksOnOrderLinesWarning'));
                    }
                    checkIfTaxIsRequired();
                    abp.event.trigger('app.createOrEditOrderModalSaved');
                } finally {
                    abp.ui.clearBusy(_$form);
                    _modalManager.setBusy(false);
                }
            }

            function updateLastModifiedDates() {
                if (!_orderId) {
                    return;
                }
                return _orderAppService.getOrderLastModifiedDates(_orderId).done(function (result) {
                    let formatDate = function (val) {
                        return val ? moment(val).utc().format("L LT") : '';
                    };
                    _$form.find("#LastModificationTime").val(formatDate(result.lastModificationTime));
                    _$form.find("#LastModifierName").val(result.lastModifierName);
                    _$form.find("#CreationTime").val(formatDate(result.creationTime));
                    _$form.find("#CreatorName").val(result.creatorName);
                    _$form.find("#CreationTime, #LastModificationTime").closest('.hidden-for-new-entity').show();

                    //_$form.dirtyForms('setClean');
                });
            }

            function checkIfTaxIsRequired() {
                serializeOrder();
                if (!_orderLines.length) {
                    return false;
                }

                var fieldNameForWarning = '';
                var taxCalculationType = abp.setting.getInt('App.Invoice.TaxCalculationType');
                var isFreightTaxable = taxCalculationType === abp.enums.taxCalculationType.freightAndMaterialTotal;
                switch (taxCalculationType) {
                    case abp.enums.taxCalculationType.noCalculation:
                        //if (_order.SalesTax > 0) {
                        //    return false;
                        //}
                        //fieldNameForWarning = 'sales tax';
                        //break;
                        return false;
                    default:
                        if (_order.SalesTaxRate > 0) {
                            return false;
                        }
                        fieldNameForWarning = 'tax rate';
                        break;
                }

                var isTaxRequired = false;
                $.each(_orderLines, function (ind, orderLine) {
                    if (orderLine.materialPrice) { //material total
                        isTaxRequired = true;
                    }
                    if (isFreightTaxable && orderLine.freightPrice) {
                        isTaxRequired = true;
                    }
                });

                if (!_orderIsTaxExempt.is(':checked') && isTaxRequired) {
                    if (isFreightTaxable) {
                        abp.message.warn('Tax is required. Please add the applicable ' + fieldNameForWarning + '.');
                    } else {
                        abp.message.warn('Tax is required since this order includes materials. Please add the applicable ' + fieldNameForWarning + '.');
                    }
                }
                return isTaxRequired;
            }

            function isNewOrder() {
                return _orderId === '';
            }

            function isNewOrChangedOrder() {
                return isNewOrder() || _$form.dirtyForms('isDirty');
            }

            function showEditingBlocks() {
                _$form.find('.editing-only-block').not(":visible").slideDown();
            }

            async function checkForOrderDuplicates(order) {
                if (order.Id !== '') {
                    return;
                }

                let duplicateCount = await _orderAppService.getOrderDuplicateCount({
                    id: order.Id,
                    customerId: order.CustomerId,
                    quoteId: order.QuoteId,
                    deliveryDate: order.DeliveryDate
                });

                if (duplicateCount > 0) {
                    var customerName = _$form.find("#OrderCustomerId").getSelectedDropdownOption().text();
                    if (!await abp.message.confirm(
                        'You already have an order scheduled for ' + order.DeliveryDate + ' for ' + customerName + '. Are you sure you want to save this order?'
                    )) {
                        throw new Error('Stopping save because duplicate order was found and user disagreed to continue.');
                    }
                }
            }

            function serializeOrder() {
                _order = _$form.serializeFormToObject();
                _order.FreightTotal = Number(_order.FreightTotal) || 0;
                _order.MaterialTotal = Number(_order.MaterialTotal) || 0;
                _order.SalesTaxRate = Number(_order.SalesTaxRate) || 0;
                //_order.IsFreightTotalOverridden = _order.IsFreightTotalOverridden === "True";
                //_order.IsMaterialTotalOverridden = _order.IsMaterialTotalOverridden === "True";

            }
            serializeOrder();

            //function refreshTotalsBackground() {
            //    if (_$form.find("#IsFreightTotalOverridden").val() === "True") {
            //        _$form.find("#FreightTotal").addClass("overridden-price");
            //    } else {
            //        _$form.find("#FreightTotal").removeClass("overridden-price");
            //    }

            //    if (_$form.find("#IsMaterialTotalOverridden").val() === "True") {
            //        _$form.find("#MaterialTotal").addClass("overridden-price");
            //    } else {
            //        _$form.find("#MaterialTotal").removeClass("overridden-price");
            //    }
            //}
            //refreshTotalsBackground();

            function updateOrderTaxDetails(orderTaxDetails) {
                _$form.find("#FreightTotal").val(round(orderTaxDetails.freightTotal).toFixed(2));
                _$form.find("#MaterialTotal").val(round(orderTaxDetails.materialTotal).toFixed(2));
                _$form.find("#SalesTaxRate").val(orderTaxDetails.salesTaxRate);
                _$form.find("#SalesTax").val(round(orderTaxDetails.salesTax).toFixed(2));
                _$form.find("#CODTotal").val(round(orderTaxDetails.codTotal).toFixed(2));
            }

            function canEditAnyOrderDirections() {
                return _$form.find("#CanEditAnyOrderDirections").val() === "True";
            }

            var _recalculateTotalsInProgressCount = 0;
            function recalculateTotals() {
                _recalculateTotalsInProgressCount++;
                if (isNewOrder() && _orderLines) {
                    var materialTotal = round(_orderLines.map(x => round(x.materialPrice)).reduce((a, b) => a + b, 0)) || 0.00;
                    var freightTotal = round(_orderLines.map(x => round(x.freightPrice)).reduce((a, b) => a + b, 0)) || 0.00;
                    _$form.find("#MaterialTotal").val(materialTotal);
                    _$form.find("#FreightTotal").val(freightTotal);
                }
                serializeOrder();
                var orderTaxDetails = {
                    Id: _orderId || 0,
                    SalesTaxRate: _order.SalesTaxRate || 0,
                    SalesTax: _order.SalesTax || 0,
                    OrderLines: _orderLines
                };
                abp.services.app.order.calculateOrderTotals(orderTaxDetails).done(function (response) {
                    updateOrderTaxDetails(response);
                }).always(function () {
                    _recalculateTotalsInProgressCount--;
                });
            }

            //update Order MaterialTotal and FreightTotal values
            //be sure to call it after the callback from the grid has been returned
            //function updateMaterialTotal() {
            //    _$form.find("#MaterialTotal").val(_materialTotal.toFixed(2)).change();
            //}

            //function updateFreightTotal() {
            //    _$form.find("#FreightTotal").val(_freightTotal.toFixed(2)).change();
            //}

            //calculate _materialTotal and _freightTotal values from order line data
            function calculateMaterialAndFreightTotal(data) {
                _materialTotal = round(data.map(x => round(x.materialPrice)).reduce((a, b) => a + b, 0)) || 0.00;
                _freightTotal = round(data.map(x => round(x.freightPrice)).reduce((a, b) => a + b, 0)) || 0.00;
            }

            function round(num) {
                return abp.utils.round(num);
            }

            function disableOrderEditForHaulingCompany() {
                _$form.find('input,select,textarea').not('#SalesTaxRate, #OrderSalesTaxEntityId, #OrderFuelSurchargeCalculationId, #BaseFuelCost').attr('disabled', true);
                _$form.find("#CreateNewOrderLineButton").hide();
                _$form.find("#EditInternalNotesButton").closest('.form-group').hide();
            }

            function disableOrderEdit() {
                _isOrderReadonly = true;
                _$form.find('input,select,textarea').attr('disabled', true);
                _$modal.find('#SaveOrderButton').hide();
                _$form.find("#CreateNewOrderLineButton").hide();
                _$form.find("#EditInternalNotesButton").closest('.form-group').hide();
                if (canEditAnyOrderDirections()) {
                    _$form.find("#SaveDirectionsButton").closest('.form-group').show();
                    _$form.find("#Directions").attr('disabled', false);
                }
            }

            function disableTaxControls() {
                var taxCalculationType = abp.setting.getInt('App.Invoice.TaxCalculationType');
                switch (taxCalculationType) {
                    case abp.enums.taxCalculationType.noCalculation:
                        _$form.find("#SalesTaxRate, #OrderSalesTaxEntityId, #SalesTax").closest('.form-group').hide();
                        break;
                    default:
                        _$form.find("#SalesTax").prop('readonly', true);
                        break;
                }
            }
            disableTaxControls();

            function disableSalesTaxRateIfNeeded() {
                if (_$form.find("#OrderSalesTaxEntityId").val()) {
                    _$form.find("#SalesTaxRate").prop('readonly', true);
                }
            }
            disableSalesTaxRateIfNeeded();

            function refreshPaymentInfo() {
                var authorizationCaptureDate = _dtHelper.parseUtcDateTime(_$form.find("#AuthorizationCaptureDateTime").val(), '');
                var authorizationDate = _dtHelper.parseUtcDateTime(_$form.find("#AuthorizationDateTime").val(), '');
                if (authorizationCaptureDate !== '') {
                    _$form.find("#OrderPaymentStatus").text("PAID " + authorizationCaptureDate.format('l') + " " + authorizationCaptureDate.format('LT'));
                    _$form.find("#OrderPaymentStatus").closest('.order-payment-status').show();
                    _$form.find("#AuthorizeChargeButton, #CaptureAuthorizationButton, #CancelAuthorizationButton, #RefundPaymentButton").hide();
                    if (!_isOrderReadonly && _permissions.edit) {
                        _$form.find("#RefundPaymentButton").show();
                    }
                } else if (authorizationDate !== '') {
                    _$form.find("#OrderPaymentStatus").text("Authorized " + authorizationDate.format('l') + " " + authorizationDate.format('LT'));
                    _$form.find("#OrderPaymentStatus").closest('.order-payment-status').show();
                    _$form.find("#AuthorizeChargeButton, #CaptureAuthorizationButton, #CancelAuthorizationButton, #RefundPaymentButton").hide();
                    if (!_isOrderReadonly && _permissions.edit) {
                        _$form.find("#CaptureAuthorizationButton, #CancelAuthorizationButton").show();
                    }
                } else {
                    _$form.find("#OrderPaymentStatus").text("Not Authorized");
                    _$form.find("#OrderPaymentStatus").closest('.order-payment-status').hide();
                    _$form.find("#AuthorizeChargeButton, #CaptureAuthorizationButton, #CancelAuthorizationButton, #RefundPaymentButton").hide();
                    if (!_isOrderReadonly && _permissions.edit) {
                        _$form.find("#AuthorizeChargeButton").show();
                    }
                }
            }

            function setIsTaxExempt(customerIsTaxExempt, quoteIsTaxExempt) {
                var isTaxExempt;
                if (_quoteDropdown.val() !== '') {
                    isTaxExempt = quoteIsTaxExempt;
                    _orderIsTaxExempt.attr('disabled', true);
                } else {
                    isTaxExempt = customerIsTaxExempt;
                    _orderIsTaxExempt.attr('disabled', false);
                }
                _orderIsTaxExempt.prop('checked', isTaxExempt).change();
            }

            //Init field editors
            if (abp.session.officeId !== undefined) {

                if (abp.session.officeId === null || _$form.find("#OrderOfficeId").val() !== abp.session.officeId.toString()) {
                    //disableOrderEdit();
                }
            }

            if (!_permissions.edit) {
                disableOrderEdit();
                _$form.find("#CopyOrderButton").hide();
            } else {
                if (_$form.find("#MaterialCompanyOrderId").val()) {
                    disableOrderEditForHaulingCompany();
                }
            }

            _$form.find("#DeliveryDate").datepickerInit();

            _$form.find("#OrderShift").select2Init({
                showAll: true,
                allowClear: false
            });

            _$form.find("#Time").timepickerInit({ stepping: 1 });

            _$form.find("#OrderOfficeId").select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: false
            });

            _$form.find("#OrderSalesTaxEntityId").select2Init({
                abpServiceMethod: listCacheSelectLists.taxRate(),
                showAll: true,
                allowClear: true
            });

            _$form.find("#OrderCustomerId").select2Init({
                abpServiceMethod: abp.services.app.customer.getActiveCustomersSelectList,
                showAll: false,
                allowClear: true,
                addItemCallback: async function (newItemName) {
                    var result = await app.getModalResultAsync(
                        _createOrEditCustomerModal.open({ name: newItemName })
                    );
                    selectCustomerInControl(result);
                    return false;
                },
            });

            _quoteDropdown.select2Init({
                showAll: true,
                allowClear: true,
            });

            _$form.find("#OrderContactId").select2Init({
                showAll: true,
                allowClear: true,
                addItemCallback: async function (newItemName) {
                    var customerId = _$form.find("#OrderCustomerId").val();
                    if (!customerId) {
                        abp.notify.warn("Select a customer first");
                        _$form.find("#OrderCustomerId").focus();
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

            _$form.find("#OrderPriority").select2Init({
                showAll: true,
                allowClear: false
            });

            var quoteChildDropdown = abp.helper.ui.initChildDropdown({
                parentDropdown: _$form.find("#OrderCustomerId"),
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
            var contactChildDropdown = abp.helper.ui.initChildDropdown({
                parentDropdown: _$form.find("#OrderCustomerId"),
                childDropdown: _$form.find("#OrderContactId"),
                abpServiceMethod: abp.services.app.customer.getContactsForCustomer
            });
            _$form.find("#OrderContactId").change(function () {
                var option = _$form.find("#OrderContactId").getSelectedDropdownOption();
                _$form.find("#ContactPhone").val(option.data("phoneNumber"));
            });
            _$form.find("#OrderCustomerId").change(function () {
                var dropdownData = _$form.find("#OrderCustomerId").select2('data');
                if (dropdownData && dropdownData.length) {
                    if (dropdownData[0].item) {
                        _$form.find("#CustomerAccountNumber").val(dropdownData[0].item.accountNumber);
                        if (dropdownData[0].item.customerIsCod) {
                            _$form.find("#CustomerAccountNumber").val('COD');
                            _$form.find("#CustomerAccountNumber").addClass('cod-account-number');
                        } else {
                            _$form.find("#CustomerAccountNumber").removeClass('cod-account-number');
                        }
                        _pricingTierId = dropdownData[0].item.pricingTierId;
                        _customerIsCod = dropdownData[0].item.customerIsCod;
                        _customerIsTaxExempt = dropdownData[0].item.isTaxExempt;
                    } else {
                        _pricingTierId = null;
                        _customerIsCod = false;
                        _customerIsTaxExempt = false;
                    }
                }
                setIsTaxExempt(_customerIsTaxExempt, false);
            });

            _$form.find("#OrderSalesTaxEntityId").change(function () {
                var dropdownData = $(this).select2('data');
                if (dropdownData && dropdownData.length) {
                    if (dropdownData[0].item) {
                        _$form.find("#SalesTaxRate").val(dropdownData[0].item.rate).change();
                    } else {
                        _$form.find("#SalesTaxRate").val(0).change().prop('readonly', false);
                    }
                }
                disableSalesTaxRateIfNeeded();
            });

            _$form.find("#OrderFuelSurchargeCalculationId").select2Init({
                abpServiceMethod: listCacheSelectLists.fuelSurchargeCalculation(),
                showAll: true,
                allowClear: true
            });
            _$form.find("#OrderFuelSurchargeCalculationId").change(function () {
                let dropdownData = _$form.find("#OrderFuelSurchargeCalculationId").select2('data');
                let selectedOption = dropdownData && dropdownData.length && dropdownData[0];
                let canChangeBaseFuelCost = selectedOption?.item?.canChangeBaseFuelCost || false;
                _$form.find("#BaseFuelCostContainer").toggle(canChangeBaseFuelCost);
                _$form.find("#BaseFuelCost").val(selectedOption?.item?.baseFuelCost || 0);
                _$form.find("#OrderFuelSurchargeCalculationId").removeUnselectedOptions();
            });

            _orderIsTaxExempt.change(function () {
                updateTaxControls();
            });

            function updateTaxControls() {
                var salesTaxEntity = _$form.find("#OrderSalesTaxEntityId");
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

            _modalManager.on('app.selectOrderQuoteModal.requestInput', function (callback) {
                callback(_quoteDropdown);
            });

            function roundTextboxValueIfNumeric(field) {
                var value = round(_$form.find(field).val());
                if (value !== null) {
                    _$form.find(field).val(value.toFixed(2));
                }
            }

            roundTextboxValueIfNumeric("#MaterialTotal");
            roundTextboxValueIfNumeric("#FreightTotal");
            roundTextboxValueIfNumeric("#SalesTax");
            roundTextboxValueIfNumeric("#CODTotal");

            refreshPaymentInfo();

            _$form.dirtyForms();


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
                if (_quoteId !== '') {
                    _$form.find("#OrderContactId").val(option.data('contactId')).change();
                    if (option.data('officeId')) {
                        abp.helper.ui.addAndSetDropdownValue(_$form.find("#OrderOfficeId"), option.data('officeId'), option.data('officeName'));
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
                        abp.helper.ui.addAndSetDropdownValue(_$form.find("#OrderFuelSurchargeCalculationId"), fuelSurchargeCalculationId, option.data('fuelSurchargeCalculationName'));
                        updateInputValue("#BaseFuelCost", option.data('baseFuelCost'));
                    } else {
                        _$form.find("#OrderFuelSurchargeCalculationId").val(null).change();
                        updateInputValue("#BaseFuelCost", 0);
                    }
                    _$form.find("#BaseFuelCostContainer").toggle(option.data('canChangeBaseFuelCost') === true);
                    setIsTaxExempt(option.data('customerIsTaxExempt'), option.data('quoteIsTaxExempt'));
                } else {
                    let defaultFuelSurchargeCalculationId = abp.setting.getInt('App.Fuel.DefaultFuelSurchargeCalculationId');
                    let defaultFuelSurchargeCalculationName = _$form.find("#DefaultFuelSurchargeCalculationName").val();
                    if (defaultFuelSurchargeCalculationId > 0) {
                        abp.helper.ui.addAndSetDropdownValue(_$form.find("#OrderFuelSurchargeCalculationId"), defaultFuelSurchargeCalculationId, defaultFuelSurchargeCalculationName);
                    } else {
                        _$form.find("#OrderFuelSurchargeCalculationId").val(null).change();
                    }
                    updateInputValue("#BaseFuelCost", _$form.find("#DefaultBaseFuelCost").val());
                    _$form.find("#BaseFuelCostContainer").toggle(_$form.find("#DefaultCanChangeBaseFuelCost").val() === 'True');
                    setIsTaxExempt(_customerIsTaxExempt, false);
                }
                _$form.find("#OrderFuelSurchargeCalculationId").prop("disabled", !!_quoteId && !abp.auth.hasPermission('Pages.Orders.EditQuotedValues'));
                serializeOrder();
                if (_quoteId) {
                    try {
                        abp.ui.setBusy();
                        let quoteLinesData = await _orderAppService.getOrderLines({ quoteId: _quoteId });
                        if (quoteLinesData.items.length === 1) {
                            addNewOrderLines(quoteLinesData.items);
                        }
                    }
                    finally {
                        abp.ui.clearBusy();
                    }
                }
            };

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

            //Recalculatable Fields change handling

            _$form.find("#SalesTaxRate").change(function () {
                if ($(this).val().toString() !== _order.SalesTaxRate.toString()) {
                    recalculateTotals();
                }
            });

            _$form.find("#SalesTax").change(function () {
                if ($(this).val().toString() !== _order.SalesTax.toString()) {
                    recalculateTotals();
                }
            });

            //_$form.find("#FreightTotal").change(function () {
            //    if ($(this).val().toString() !== _order.FreightTotal.toString()) {
            //        serializeOrder();
            //        _order.IsFreightTotalOverridden = _order.FreightTotal !== _freightTotal;
            //        _$form.find("#IsFreightTotalOverridden").val(_order.IsFreightTotalOverridden ? "True" : "False");
            //        recalculateTotals();
            //        refreshTotalsBackground();
            //    }
            //});

            //_$form.find("#MaterialTotal").change(function () {
            //    if ($(this).val().toString() !== _order.MaterialTotal.toString()) {
            //        serializeOrder();
            //        _order.IsMaterialTotalOverridden = _order.MaterialTotal !== _materialTotal;
            //        _$form.find("#IsMaterialTotalOverridden").val(_order.IsMaterialTotalOverridden ? "True" : "False");
            //        recalculateTotals();
            //        refreshTotalsBackground();
            //    }
            //});

            //Init OrderLines grid

            var staggeredIcon = ' <span class="far fa-clock staggered-icon pull-right" title="Staggered"></span>';
            var orderLinesTable = _$form.find('#OrderLinesTable');
            var orderLinesGrid = orderLinesTable.DataTableInit({
                paging: false,
                info: false,
                ordering: false,
                language: {
                    emptyTable: app.localize("NoDataInTheTableClick{0}ToAdd", app.localize("AddOrderItem"))
                },
                ajax: function (data, callback, settings) {
                    var abpData = _dtHelper.toAbpData(data);
                    if (isNewOrder()) {
                        if (!_orderLinesGridData) {
                            _orderLinesGridData = _dtHelper.getEmptyResult();
                            _orderLines = _orderLinesGridData.data;
                        }

                        callback(_orderLinesGridData);
                        return;
                    }

                    $.extend(abpData, { orderId: _orderId });
                    _orderAppService.getOrderLines(abpData).done(function (abpResult) {
                        _orderLinesGridData = _dtHelper.fromAbpResult(abpResult);
                        _orderLines = abpResult.items;
                        callback(_orderLinesGridData);
                    });
                },
                footerCallback: function (tfoot, data, start, end, display) {
                    calculateMaterialAndFreightTotal(data);
                    let grid = this;
                    let setTotalFooterValue = function (columnName, total, visible) {
                        let footerCell = grid.api().column(columnName + ':name').footer();
                        $(footerCell).html(visible ? "Total: " + _dtHelper.renderMoney(total) : '');
                    }
                    setTotalFooterValue('materialPrice', _materialTotal, data.length);
                    setTotalFooterValue('freightPrice', _freightTotal, data.length);
                },
                columns: [
                    {
                        width: '20px',
                        className: 'control responsive',
                        orderable: false,
                        render: function () {
                            return '';
                        }
                    },
                    {
                        data: "lineNumber",
                        title: "Line #"
                    },
                    {
                        data: "loadAtName",
                        title: "Load At",
                        createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                            if (rowData.hasQuoteBasedPricing) {
                                $(cell).addClass("quote-based-pricing");
                            }
                        }
                    },
                    {
                        data: "deliverToName",
                        title: "Deliver To",
                        createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                            if (rowData.hasQuoteBasedPricing) {
                                $(cell).addClass("quote-based-pricing");
                            }
                        }
                    },
                    {
                        data: "freightItemName",
                        title: _separateMaterialAndFreightItemsFeature ? "Freight Item" : "Item",
                        createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                            if (rowData.hasQuoteBasedPricing) {
                                $(cell).addClass("quote-based-pricing");
                            }
                        },
                    },
                    {
                        data: "materialItemName",
                        title: "Material Item",
                        createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                            if (rowData.hasQuoteBasedPricing) {
                                $(cell).addClass("quote-based-pricing");
                            }
                        },
                        visible: _separateMaterialAndFreightItemsFeature
                    },
                    {
                        data: "designationName",
                        title: "Designation"
                    },
                    {
                        data: "numberOfTrucks",
                        title: '<i class="fas fa-truck"></i>',
                        titleHoverText: app.localize('RequestedNumberOfTrucks')
                    },
                    {
                        data: "isMultipleLoads",
                        title: '',
                        titleHoverText: app.localize('RunUntilStopped'),
                        render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(data); }
                    },
                    {
                        data: "timeOnJob",
                        render: function (data, type, full, meta) {
                            var timeToDisplay = full.staggeredTimeKind === abp.enums.staggeredTimeKind.setInterval
                                ? full.firstStaggeredTimeOnJob
                                : full.timeOnJob;
                            var isTimeStaggered = full.staggeredTimeKind !== abp.enums.staggeredTimeKind.none;
                            return _dtHelper.renderTime(timeToDisplay, '') + (isTimeStaggered ? staggeredIcon : '');
                        },
                        title: '<i class="far fa-clock"></i>',
                        titleHoverText: app.localize('TimeOnJob')
                    },
                    {
                        data: "note",
                        title: '<i class="fa-regular fa-files"></i>',
                        titleHoverText: app.localize('Note'),
                        orderable: false,
                        className: "checkmark all",
                        render: function (data, type, full, meta) { return ''; },
                        createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                            let icon = $('<i class="fa-regular fa-files directions-icon" data-toggle="tooltip" data-html="true"></i>');
                            if (rowData.note) {
                                icon.prop('title', abp.utils.replaceAll(rowData.note, '\n', '<br>') + '<br><br><b>Click icon to edit comments</b>');
                            } else {
                                icon.addClass('gray');
                                icon.prop('title', '<b>Click icon to add comments</b>');
                            }
                            icon.click(function () {
                                var orderLine = rowData;
                                if (orderLine.id) {
                                    _setOrderLineNoteModal.open({ id: orderLine.id });
                                } else {
                                    let model = {
                                        orderLineId: null,
                                        note: orderLine.note
                                    };
                                    _setOrderLineNoteModal.open({}).done(function (modal, modalObject) {
                                        modalObject.setModel(model);
                                        modalObject.saveCallback = function (model) {
                                            orderLine.note = model.note;
                                        };
                                    });
                                }
                            });
                            $(cell).append(icon);
                        }
                    },
                    {
                        data: "productionPay",
                        title: '<i class="fas fa-dollar-sign"></i>',
                        titleHoverText: app.localize('IsBasedOnProductionPay') + '.',
                        render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(data); }
                    },
                    {
                        data: "materialUomName",
                        title: "Material UOM",
                        createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                            if (rowData.hasQuoteBasedPricing) {
                                $(cell).addClass("quote-based-pricing");
                            }
                        }
                    },
                    {
                        data: "freightUomName",
                        title: "Freight UOM",
                        createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                            if (rowData.hasQuoteBasedPricing) {
                                $(cell).addClass("quote-based-pricing");
                            }
                        }
                    },
                    {
                        data: "materialPricePerUnit",
                        title: "Material Rate",
                        render: function (data, type, full, meta) { return _dtHelper.renderMoneyUnrounded(data); },
                        createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                            if (rowData.isMaterialPricePerUnitOverridden) {
                                $(cell).addClass("overridden-price");
                            }
                        }
                    },
                    {
                        data: "freightPricePerUnit",
                        title: "Freight Rate",
                        render: function (data, type, full, meta) { return _dtHelper.renderMoneyUnrounded(data); },
                        createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                            if (rowData.isFreightPricePerUnitOverridden) {
                                $(cell).addClass("overridden-price");
                            }
                        }
                    },
                    {
                        data: "leaseHaulerRate",
                        render: function (data, type, full, meta) { return _dtHelper.renderMoneyUnrounded(data); },
                        title: "LH Rate",
                        visible: abp.setting.getBoolean('App.LeaseHaulers.ShowLeaseHaulerRateOnOrder')
                    },
                    {
                        data: "materialQuantity",
                        title: "Material</br>Quantity",
                        width: "68px"
                    },
                    {
                        data: "freightQuantity",
                        title: "Freight</br>Quantity",
                        width: "68px"
                    },
                    {
                        data: "materialPrice",
                        name: "materialPrice",
                        render: function (data, type, full, meta) {
                            return _dtHelper.renderMoney(full.materialPrice);
                        },
                        title: "Material",
                        createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                            if (rowData.isMaterialPriceOverridden) {
                                $(cell).addClass("overridden-price");
                            }
                        }
                    },
                    {
                        data: "freightPrice",
                        name: "freightPrice",
                        render: function (data, type, full, meta) {
                            return _dtHelper.renderMoney(full.freightPrice);
                        },
                        title: "Freight",
                        createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                            if (rowData.isFreightPriceOverridden) {
                                $(cell).addClass("overridden-price");
                            }
                        }
                    },
                    {
                        data: null,
                        orderable: false,
                        visible: _permissions.edit, //&& !_isOrderReadonly
                        name: "Actions",
                        className: "actions",
                        width: "10px",
                        responsivePriority: 1,
                        render: function (data, type, full, meta) {
                            if (!_isOrderReadonly) {
                                return '<div class="dropdown">'
                                    + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                                    + '<ul class="dropdown-menu dropdown-menu-right">'
                                    + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                                    + '<li><a class="btnOpenTicketsModalForRow"><i class="fa fa-edit"></i> Tickets</a></li>'
                                    + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                                    + '</ul>'
                                    + '</div>'
                                    ;
                            } else {
                                return '';
                            }
                        }
                    }
                ],
                drawCallback: function (settings) {
                    _$form.find('table [data-toggle="tooltip"]').tooltip();
                }
            });

            _modalManager.getModal().on('shown.bs.modal', function () {
                orderLinesGrid
                    .columns.adjust()
                    .responsive.recalc();
            });

            function reloadOrderLinesGridAsync(resetPaging) {
                return new Promise((resolve) => {
                    resetPaging = resetPaging === undefined ? true : resetPaging;
                    orderLinesGrid.ajax.reload(() => {
                        resolve();
                    }, resetPaging);
                });
            }

            //Handle OrderLine add/edit

            async function handleOrderTaxDetailsExternalChange(e) {
                if (isNewOrder()) {
                    recalculateTotals();
                } else {
                    if (e && e.orderTaxDetails) {
                        await reloadOrderLinesGridAsync();
                        updateOrderTaxDetails(e.orderTaxDetails);
                        _$form.dirtyForms('setClean');
                        checkIfTaxIsRequired();
                    }
                }
            }

            _modalManager.on('app.createOrEditOrderLineModalSaved', function (e) {
                handleOrderTaxDetailsExternalChange(e);
                updateLastModifiedDates();
            });

            _modalManager.on('app.ticketEditedModal', function (e) {
                handleOrderTaxDetailsExternalChange(e);
            });

            _modalManager.on('app.ticketDeletedModal', function (e) {
                handleOrderTaxDetailsExternalChange(e);
            });

            _modalManager.on('app.orderLineNoteModalSaved', function (e) {
                reloadOrderLinesGridAsync();
                updateLastModifiedDates();
            });

            function getNextLineNumber() {
                return _orderLines.map(x => x.lineNumber).reduce((a, b) => a > b ? a : b, 0) + 1;
            }

            function setDefaultNewOrderLineValues(orderLine) {
                orderLine.lineNumber = getNextLineNumber();
                orderLine.quoteId = _quoteId;
                orderLine.canOverrideTotals = true;
                orderLine.pricingTierId = _pricingTierId;
                orderLine.customerIsCod = _customerIsCod;
                orderLine.productionPay = abp.setting.getBoolean('App.TimeAndPay.DefaultToProductionPay');
                return orderLine;
            }

            function addNewOrderLines(orderLines) {
                let nextLineNumber = getNextLineNumber();
                orderLines.forEach(orderLine => {
                    orderLine.lineNumber = nextLineNumber++;
                    if (_orderId) {
                        orderLine.orderId = _orderId;
                    }
                    addNewOrderLineInternal(orderLine);
                });
                recalculateTotals();
                reloadOrderLinesGridAsync();
            }

            function addNewOrderLineInternal(orderLine) {
                if (!_orderLines || !_orderLinesGridData) {
                    console.error('_orderLines or _orderLinesGridData hasn\'t been initialised yet');
                    return;
                }
                _orderLines.push(orderLine);
                _orderLinesGridData.recordsTotal++;
                _orderLinesGridData.recordsFiltered++;
            }

            _$form.find("#CreateNewOrderLineButton").click(async function (e) {
                e.preventDefault();
                if (_quoteId) {
                    _addQuoteBasedOrderLinesModal.open({
                        quoteId: _quoteId,
                    }).done(function (modal, modalObject) {
                        modalObject.setFilter({ quoteId: _quoteId });
                    });
                } else if (isNewOrder()) {
                    var newOrderLine = setDefaultNewOrderLineValues({});
                    _createOrEditOrderLineModal.open({}).done(function (modal, modalObject) {
                        modalObject.setOrderLine(newOrderLine);
                        modalObject.saveCallback = function () {
                            addNewOrderLines([newOrderLine]);
                        };
                    });
                } else {
                    if (isNewOrChangedOrder()) {
                        await saveOrderAsync();
                        reloadOrderLinesGridAsync();
                    }
                    openNewOrderLinePopup();
                }
            });

            function openNewOrderLinePopup() {
                _createOrEditOrderLineModal
                    .open({ orderId: _orderId })
                    .fail(function (failResult) {
                        handleOrderLinePopupError(failResult);
                    });
            }

            _modalManager.on('app.quoteBasedOrderLinesSelectedModal', function (eventData) {
                if (!eventData.selectedLines.length) {
                    return;
                }
                if (isNewOrder()) {
                    addNewOrderLines(eventData.selectedLines);
                } else {
                    abp.ui.setBusy();
                    addNewOrderLines(eventData.selectedLines);
                    _orderAppService.editOrderLines(eventData.selectedLines).done(function () {
                        abp.notify.info('Saved successfully.');
                    }).always(function () {
                        reloadOrderLinesGridAsync();
                        abp.ui.clearBusy();
                    });
                }
            });

            function handleOrderLinePopupError(failResult) {
                if (failResult && failResult.loadResponseObject && failResult.loadResponseObject.userFriendlyException) {
                    var param = failResult.loadResponseObject.userFriendlyException.parameters;
                    if (param && param.Kind === "EntityDeletedException") {
                        if (param.EntityKind === "Order") {
                            _$form.dirtyForms('setClean');
                            location.href = abp.appPath + 'app/orders/';
                        } else if (param.EntityKind === "OrderLine") {
                            reloadOrderLinesGridAsync();
                        }
                    }
                }
            }

            _$modal.find("#SaveOrderButton").click(async function (e) {
                e.preventDefault();
                await saveOrderAsync();
                //reloadOrderLinesGridAsync(); //commented this out since it doesn't look like this is needed - we close the modal on the next step
                _modalManager.close();
            });

            _$form.find("#CopyOrderButton").click(async function (e) {
                e.preventDefault();
                await saveOrderAsync();
                //var date = _$form.find("#DeliveryDate").val();
                _copyOrderModal.open({
                    orderId: _orderId
                });
            });

            _$form.find("#SaveDirectionsButton").click(function (e) {
                e.preventDefault();

                var formData = _$form.serializeFormToObject();

                abp.ui.setBusy(_$form);
                abp.services.app.scheduling.setOrderDirections({
                    orderId: _orderId,
                    directions: formData.Directions
                }).done(function () {
                    abp.notify.info('Saved successfully.');
                    _$form.dirtyForms('setClean');
                }).always(function () {
                    abp.ui.clearBusy(_$form);
                });
            });

            async function printOrder(additionalOptions) {
                if (isNewOrChangedOrder()) {
                    await saveOrderAsync();
                }
                var options = $.extend({ id: _orderId }, additionalOptions);
                app.openPopup(abp.appPath + 'app/orders/GetWorkOrderReport?' + $.param(options));
            }

            _$form.find("#PrintOrderWithNoPricesButton").click(function (e) {
                e.preventDefault();
                printOrder({ hidePrices: true });
            });

            _$form.find("#PrintOrderWithCombinedPrices").click(function (e) {
                e.preventDefault();
                printOrder();
            });

            _$form.find("#PrintOrderWithSeparatePrices").click(function (e) {
                e.preventDefault();
                printOrder(app.order.getOrderWithSeparatePricesReportOptions());
            });

            _$form.find("#PrintOrderForBackOffice").click(function (e) {
                e.preventDefault();
                printOrder(app.order.getBackOfficeReportOptions());
            });

            _$form.find("#PrintOrderWithDeliveryInfo").click(async function (e) {
                e.preventDefault();
                await saveOrderIfNeededAsync();
                _printOrderWithDeliveryInfoModal.open({ id: _orderId });
            });

            _$form.find("#EmailOrderButton").click(async function (e) {
                e.preventDefault();
                await saveOrderAsync();
                _emailOrderReportModal.open({ id: _orderId });
            });

            _$form.find("#EditInternalNotesButton").click(async function (e) {
                e.preventDefault();
                await saveOrderIfNeededAsync();
                _editInternalNotesModal.open({ id: _orderId });
            });

            _$form.find("#AuthorizeChargeButton").click(async function (e) {
                e.preventDefault();
                await saveOrderAsync();
                _authorizeOrderChargeModal.open({ id: _orderId });
            });

            _$form.find("#CancelAuthorizationButton").click(async function (e) {
                e.preventDefault();
                if (!await abp.message.confirm(
                    'Are you sure you want to cancel the authorization?'
                )) {
                    return;
                }

                await saveOrderAsync();
                await _orderPaymentService.cancelOrderAuthorization({ id: _orderId });

                abp.notify.info('Canceled successfully.');
                _$form.find("#AuthorizationDateTime").val('');
                refreshPaymentInfo();
            });

            _$form.find("#RefundPaymentButton").click(async function (e) {
                e.preventDefault();
                if (!await abp.message.confirm(
                    'Are you sure you want to refund the payment?'
                )) {
                    return;
                }

                await saveOrderAsync();
                await _orderPaymentService.refundOrderPayment({ id: _orderId });

                abp.notify.info('Refunded successfully.');
                _$form.find("#AuthorizationDateTime").val('');
                _$form.find("#AuthorizationCaptureDateTime").val('');
                refreshPaymentInfo();
            });

            _modalManager.on('app.authorizedOrderChargeModal', function (e) {
                var date = _dtHelper.parseDateTimeAsUtc(e.authorizationDateTime, '');
                _$form.find("#AuthorizationDateTime").val(_dtHelper.renderDateTime(date, ''));
                refreshPaymentInfo();
            });

            _modalManager.on('app.capturedOrderAuthorizationModal', function (e) {
                var date = _dtHelper.parseDateTimeAsUtc(e.authorizationCaptureDateTime, '');
                _$form.find("#AuthorizationCaptureDateTime").val(_dtHelper.renderDateTime(date, ''));
                refreshPaymentInfo();
            });

            _modalManager.on('app.orderModalCopied', function (e) {
                abp.ui.setBusy();
                _modalManager.close();
                abp.ui.clearBusy();
                _createOrEditOrderModal.open({ id: e.newOrderId });
            });

            _$form.find("#CreateNewReceipt").click(async function (e) {
                e.preventDefault();
                await saveOrderIfNeededAsync();
                abp.ui.setBusy();
                window.location = abp.appPath + 'app/receipts/details?orderId=' + _orderId;
            });

            _$form.find('.openReceiptButton').click(async function (e) {
                e.preventDefault();
                var receiptId = $(this).attr('data-receiptId');
                await saveOrderIfNeededAsync();
                abp.ui.setBusy();
                window.location = abp.appPath + 'app/receipts/details/' + receiptId;
            });

            orderLinesTable.on('click', '.btnEditRow', async function (e) {
                e.preventDefault();
                var orderLine = _dtHelper.getRowData(this);
                if (isNewOrder()) {
                    //orderLine.isNew = false;
                    _createOrEditOrderLineModal.open({
                        designation: orderLine.designation
                    }).done(function (modal, modalObject) {
                        modalObject.setOrderLine(orderLine);
                        modalObject.saveCallback = function () {
                            recalculateTotals();
                            reloadOrderLinesGridAsync();
                        };
                    });
                } else {
                    if (isNewOrChangedOrder()) {
                        await saveOrderAsync();
                        reloadOrderLinesGridAsync();
                    }
                    openEditOrderLinePopup(orderLine.id);
                }
            });

            function openEditOrderLinePopup(orderLineId) {
                _createOrEditOrderLineModal
                    .open({ id: orderLineId })
                    .fail(function (failResult) {
                        handleOrderLinePopupError(failResult);
                    });
            }

            orderLinesTable.on('click', '.btnOpenTicketsModalForRow', async function (e) {
                e.preventDefault();
                let orderLine = _dtHelper.getRowData(this);
                if (isNewOrChangedOrder()) {
                    await saveOrderAsync();
                    let orderLineReloadPromise = reloadOrderLinesGridAsync();
                    if (!orderLine.id) {
                        await orderLineReloadPromise;
                        orderLine = _orderLines.find(x => x.lineNumber === orderLine.lineNumber);
                        if (!orderLine) {
                            return;
                        }
                    }
                }
                openTicketModal(orderLine.id);
            });

            function openTicketModal(orderLineId) {
                _createOrEditTicketModal.open({ orderLineId: orderLineId });
            }

            orderLinesTable.on('click', '.btnDeleteRow', async function (e) {
                e.preventDefault();
                var orderLine = _dtHelper.getRowData(this);
                if (await abp.message.confirm(
                    'Are you sure you want to delete the item?'
                )) {
                    if (isNewOrder()) {
                        var index = _orderLines.indexOf(orderLine);
                        if (index !== -1) {
                            _orderLines.splice(index, 1);
                            _orderLinesGridData.recordsTotal--;
                            _orderLinesGridData.recordsFiltered--;
                            updateLineNumbers();
                            recalculateTotals();
                            reloadOrderLinesGridAsync();
                        }
                    } else {
                        await saveOrderIfNeededAsync();
                        deleteOrderLine(orderLine.id);
                    }
                }
            });

            async function deleteOrderLine(orderLineId) {
                let deleteResult = await _orderAppService.deleteOrderLine({
                    id: orderLineId,
                    orderId: _orderId
                });
                abp.notify.info('Successfully deleted.');
                await reloadOrderLinesGridAsync();
                updateOrderTaxDetails(deleteResult.orderTaxDetails);
                _$form.dirtyForms('setClean');
            }

            function updateLineNumbers() {
                if (!isNewOrder() || !_orderLines) {
                    return;
                }
                _orderLines.map((orderLine, index) => orderLine.lineNumber = index + 1);
            }

            //Handle popup adding

            function selectCustomerInControl(customer) {
                var option = new Option(customer.name, customer.id, true, true);
                $(option).data('data', {
                    id: customer.id,
                    text: customer.name,
                    item: {
                        accountNumber: customer.accountNumber,
                        customerIsCod: customer.customerIsCod,
                        isTaxExempt: customer.isTaxExempt,
                        pricingTierId: customer.pricingTierId,
                    },
                });
                _$form.find("#OrderCustomerId").append(option).trigger('change');
            }

            abp.helper.ui.initCannedTextLists();
        }
    };
})(jQuery);
