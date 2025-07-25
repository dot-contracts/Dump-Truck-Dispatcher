(function ($) {

    window.app = app || {};
    app.order ??= {};
    app.order.getBackOfficeReportOptions = function (userOptions) {
        return $.extend({
            splitRateColumn: true,
            showPaymentStatus: true,
            showSpectrumNumber: true,
            showOfficeName: true,
            useActualAmount: true
        }, userOptions);
    };

    app.order.getOrderWithSeparatePricesReportOptions = function (userOptions) {
        return $.extend({
            splitRateColumn: true,
            showPaymentStatus: true,
            showSpectrumNumber: true,
            showOfficeName: true
        }, userOptions);
    };

    app.order.getOrdersWithDeliveryInfoReportOptions = function (userOptions) {
        return $.extend({
            showPaymentStatus: true,
            showSpectrumNumber: true,
            showOfficeName: true,
            useActualAmount: true,
            showDeliveryInfo: true
        }, userOptions);
    };

    app.order.getReceiptReportOptions = function (userOptions) {
        return $.extend({
            //splitRateColumn: true,
            showPaymentStatus: true,
            showSpectrumNumber: true,
            showOfficeName: true,
            //useActualAmount: true,
            useReceipts: true
        }, userOptions);
    };

    app.order.initHourlyDriverPayRateInputs = function (inputs) {
        //inputs must have the following fields:
        //driverPayTimeClassificationDropdown
        //hourlyDriverPayRateInput

        if (!abp.setting.getBoolean('App.TimeAndPay.BasePayOnHourlyJobRate')) {
            inputs.driverPayTimeClassificationDropdown.closest('.form-group').hide();
            inputs.hourlyDriverPayRateInput.closest('.form-group').hide();
            return;
        }

        inputs.driverPayTimeClassificationDropdown.select2Init({
            abpServiceMethod: abp.services.app.timeClassification.getTimeClassificationsSelectList,
            showAll: true,
            allowClear: true,
        }).change(function () {
            if (!(Number(inputs.hourlyDriverPayRateInput.val()) > 0)) {
                var dropdownData = inputs.driverPayTimeClassificationDropdown.select2('data');
                if (dropdownData?.length && dropdownData[0].item) {
                    inputs.hourlyDriverPayRateInput.val(dropdownData[0].item.defaultRate);
                }
            }
        });

        var useDriverSpecificHourlyJobRate = abp.setting.getBoolean('App.TimeAndPay.UseDriverSpecificHourlyJobRate');
        inputs.hourlyDriverPayRateInput.closest('.form-group').toggle(!useDriverSpecificHourlyJobRate).find('label').toggleClass('required-label', !useDriverSpecificHourlyJobRate);
        inputs.driverPayTimeClassificationDropdown.closest('.form-group').find('label').toggleClass('required-label', useDriverSpecificHourlyJobRate);
    };

    app.order.validatreHourlyDriverPayRateInputs = function (inputs) {
        if (!abp.setting.getBoolean('App.TimeAndPay.BasePayOnHourlyJobRate')) {
            return true;
        }

        var useDriverSpecificHourlyJobRate = abp.setting.getBoolean('App.TimeAndPay.UseDriverSpecificHourlyJobRate');
        if (useDriverSpecificHourlyJobRate) {
            if (inputs.driverPayTimeClassificationDropdown.is(':visible')
                && !inputs.driverPayTimeClassificationDropdown.val()
            ) {
                abp.message.error('Please check the following: \n'
                    + '"Driver pay time code" - This field is required.\n');
                return false;
            }
        } else {
            if (inputs.hourlyDriverPayRateInput.is(':visible')
                && !(Number(inputs.hourlyDriverPayRateInput.val()) > 0)
            ) {
                abp.message.error('Please check the following: \n'
                    + '"Hourly driver pay rate" - This field is required.\n');
                return false;
            }
        }

        //if not empty, should be positive number
        if (inputs.hourlyDriverPayRateInput.val() && !(Number(inputs.hourlyDriverPayRateInput.val()) >= 0)) {
            abp.message.error('Please check the following: \n'
                + '"Hourly driver pay rate" - This field must be a positive number.\n');
            return false;
        }

        return true;
    };


    abp.dispatches ??= {};
    abp.dispatches.cancel = function (acknowledged, dispatchId, doneCallback) {
        var confirmMessage = acknowledged ?
            'This dispatch is already being processed. Are you sure you want to cancel it?' :
            'Are you sure you want to cancel this dispatch?';
        swal(
            confirmMessage,
            {
                buttons: {
                    no: "No",
                    yes: "Yes"
                }
            }
        ).then(function (answer) {
            if (answer === 'yes') {
                abp.services.app.dispatching.cancelDispatch({ dispatchId: dispatchId, cancelAllDispatchesForDriver: false })
                    .done(function () {
                        abp.notify.info('Canceled successfully.');
                        if (acknowledged) {
                            abp.message.info('You should call or radio the driver to be sure they know the dispatch has been cancelled.');
                        }
                        if (doneCallback) {
                            doneCallback();
                        }
                    });

            }
        });
    };


    abp.helper ??= {};
    abp.helper.promptForHideLoadAtOnQuote = function () {
        return new Promise((resolve) => {
            if (!abp.setting.getBoolean("App.General.PromptForDisplayingQuarryInfoOnQuotes")) {
                resolve(false);
                return;
            }
            abp.message.confirmWithOptions({
                text: app.localize('DoYouWantToHideLoadAtColumn'),
                title: ' ',
                buttons: ['No', 'Yes']
            },
                function (isConfirmed) {
                    resolve(isConfirmed || false);
                }
            );
        });
    };

    abp.helper.calculateOrderLineTotal = function (materialAmount, freightAmount, isTaxable, salesTaxRate, isMaterialTaxable, isFreightTaxable) {
        var taxCalculationType = abp.setting.getInt('App.Invoice.TaxCalculationType');
        var separateItems = abp.features.isEnabled('App.SeparateMaterialAndFreightItems');

        materialAmount = abp.utils.round(materialAmount);
        freightAmount = abp.utils.round(freightAmount);
        var taxRate = salesTaxRate / 100;
        var salesTax = 0;
        var orderLineTotal = 0;
        var taxableTotal = 0;
        let subtotal = materialAmount + freightAmount;

        if (!separateItems) {
            switch (taxCalculationType) {
                case abp.enums.taxCalculationType.freightAndMaterialTotal:
                    taxableTotal = materialAmount + freightAmount;
                    break;

                case abp.enums.taxCalculationType.materialLineItemsTotal:
                    taxableTotal = materialAmount > 0 ? materialAmount + freightAmount : 0;
                    break;

                case abp.enums.taxCalculationType.materialTotal:
                    taxableTotal = materialAmount;
                    break;

                case abp.enums.taxCalculationType.noCalculation:
                    taxRate = 0;
                    salesTax = 0;
                    break;
            }

            if (!isTaxable || taxableTotal < 0) {
                taxableTotal = 0;
            }

            switch (taxCalculationType) {
                case abp.enums.taxCalculationType.freightAndMaterialTotal:
                case abp.enums.taxCalculationType.materialLineItemsTotal:
                case abp.enums.taxCalculationType.materialTotal:
                    salesTax = taxableTotal * taxRate;
                    orderLineTotal = subtotal + taxableTotal * taxRate;
                    break;

                case abp.enums.taxCalculationType.noCalculation:
                    //salesTax = abp.utils.round(salesTax);
                    //orderLineTotal = abp.utils.round(subtotal + salesTax);
                    orderLineTotal = subtotal;
                    break;
            }

            //var totalsToCheck = new[] { order.FreightTotal, order.MaterialTotal, order.SalesTax, order.CODTotal };
            //var maxValue = AppConsts.MaxDecimalDatabaseLength;
            //if (totalsToCheck.Any(x => x > maxValue))
            //{
            //    throw new UserFriendlyException("The value is too big", "One or more totals exceeded the maximum allowed value. Please decrease some of the values so that the total doesn't exceed " + maxValue);
            //}

            return {
                subtotal: subtotal,
                total: orderLineTotal,
                tax: salesTax
            };
        } else {
            if (isFreightTaxable && freightAmount > 0) {
                taxableTotal += freightAmount;
            }
            if (isMaterialTaxable && materialAmount > 0) {
                taxableTotal += materialAmount;
            }

            salesTax = taxableTotal * taxRate;
            orderLineTotal = subtotal + taxableTotal * taxRate;

            return {
                subtotal: subtotal,
                total: orderLineTotal,
                tax: salesTax
            };
        }
    };

    abp.helper.getVisibleTicketControls = function (orderLine) {
        //orderLine must have the following fields:
        //designation
        //materialItemId
        //freightUomId
        //materialUomId

        let visibility = {
            freightItem: false,
            materialItem: false,
            quantity: false,
            freightQuantity: false,
            materialQuantity: false,
            freightUom: false,
            materialUom: false,
        };

        let separateItems = abp.features.isEnabled('App.SeparateMaterialAndFreightItems');

        visibility.quantity = false; //deprecated
        visibility.freightItem = !separateItems; //if separateItems == true, this will always be populated from order line, otherwise should always be visible
        visibility.materialItem = separateItems && orderLine.designation === abp.enums.designation.freightOnly && !orderLine.materialItemId; //only visible when material item is optional and is not specified
        visibility.freightQuantity = orderLine.designation !== abp.enums.designation.materialOnly && orderLine.freightUomId !== orderLine.materialUomId && orderLine.materialUomId; //only visible when UOMs don't match (and freight is available)
        visibility.materialQuantity = true; //always visible

        visibility.materialUom = visibility.materialQuantity; //UOM visibility will always match the visibility of the respective quantity control
        visibility.freightUom = visibility.freightQuantity;

        return visibility;
    };

})(jQuery);
