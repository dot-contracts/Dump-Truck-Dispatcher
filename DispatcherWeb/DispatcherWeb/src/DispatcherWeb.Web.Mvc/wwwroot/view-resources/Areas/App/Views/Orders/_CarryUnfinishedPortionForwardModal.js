(function ($) {
    app.modals.CarryUnfinishedPortionForwardModal = function () {

        var _modalManager;
        var _orderService = abp.services.app.order;
        var _schedulingService = abp.services.app.scheduling;
        var _$form = null;
        var _dateBeginPicker = null;
        var _$shiftSelect;
        let _hasMultipleOrderLinesPromise = Promise.resolve(false);
        let _modalArgs = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _modalArgs = _modalManager.getArgs();

            // Only a single order line can be carried forward per spec. Uncomment to allow multiple order lines
            //if (_modalArgs.orderId && _modalArgs.orderLineId) {
            //    setHasMultipleOrderLinesFlag();
            //}

            var $dateBegin = _$form.find('#DateBegin');
            $dateBegin.datepickerInit();
            _dateBeginPicker = $dateBegin.data('DateTimePicker');

            _$shiftSelect = _$form.find('#Shift').select2Init({ allowClear: false });
        };

        // Only a single order line can be carried forward per spec. Uncomment to allow multiple order lines
        //async function setHasMultipleOrderLinesFlag() {
        //    _hasMultipleOrderLinesPromise = _orderService.doesOrderHaveOtherOrderLines(_modalArgs.orderId, _modalArgs.orderLineId);
        //}

        this.save = async function () {
            try {
                _modalManager.setBusy(true);
                if (!_$form.valid()) {
                    _$form.showValidateMessage();
                    return;
                }

                var today = moment().startOf('day');
                var selectedDate = _dateBeginPicker.date();
                var minDate = moment('1971-01-01', 'YYYY-MM-DD');
                var maxDate = moment('2100-01-01', 'YYYY-MM-DD');
                if (selectedDate > maxDate || selectedDate < minDate) {
                    abp.message.error('Please correct the date', 'Some of the data is invalid');
                    return;
                }

                if (selectedDate < today) {
                    if (!await abp.message.confirm("You are creating an order on a previous date. Are you sure you want to do this?")) {
                        return;
                    }
                }

                var formData = _$form.serializeFormToObject();
                // Set DateEnd to be the same as DateBegin for this modal
                formData.DateEnd = formData.DateBegin;

                // Only a single order line can be carried forward per spec. Uncomment to allow multiple order lines
                //if (_modalArgs.orderId && _modalArgs.orderLineId) {
                //    let hasMultipleOrderLines = await _hasMultipleOrderLinesPromise;
                //    if (hasMultipleOrderLines) {
                //        let multipleOrderLinesResponse = await swal(
                //            "You have selected to carry forward an order with multiple line items. Select the button below for how you want to handle this operation.",
                //            {
                //                buttons: {
                //                    cancel: "Cancel",
                //                    single: "Single line item",
                //                    all: "All line items"
                //                }
                //            }
                //        );
                //        switch (multipleOrderLinesResponse) {
                //            case "single":
                //                break;
                //            case "all":
                //                formData.OrderLineId = null;
                //                break;
                //            default:
                //                return;
                //        }
                //    } else {
                //        formData.OrderLineId = null;
                //    }
                //}

                var shiftsArray = _$shiftSelect.length ? [_$shiftSelect.val()] : undefined;

                var newOrderIds = await _orderService.copyOrder({
                    orderId: formData.OrderId,
                    orderLineId: formData.OrderLineId,
                    dateBegin: formData.DateBegin,
                    dateEnd: formData.DateEnd,
                    shifts: shiftsArray,
                    copyTrucks: formData.CopyOrderTrucks,
                    carryUnfinishedPortionForward: true,
                });

                if (formData.CopyOrderTrucks) {
                    _modalManager.setBusy(true);
                    var copyOrderTrucksInput = {
                        originalOrderId: formData.OrderId,
                        newOrderIds: newOrderIds,
                        orderLineId: formData.OrderLineId,
                        proceedOnConflict: false,
                    };
                    var truckCopyResult = await _schedulingService.copyOrdersTrucks(copyOrderTrucksInput);

                    if (!truckCopyResult.completed) {
                        var s = truckCopyResult.conflictingTrucks.length > 1 ? 's' : '';
                        var is = truckCopyResult.conflictingTrucks.length > 1 ? 'are' : 'is';
                        var conflictingTrucks = truckCopyResult.conflictingTrucks.join(', ');
                        _modalManager.setBusy(false);
                        if (await abp.message.confirmWithOptions({
                            text: 'Truck' + s + ' ' + conflictingTrucks + ' ' + is + ' already scheduled on an order and can’t be copied. Do you want to continue with copying the remaining trucks?',
                            title: ' ',
                            buttons: ['No', 'Yes']
                        })) {
                            _modalManager.setBusy(true);
                            copyOrderTrucksInput.proceedOnConflict = true;
                            await _schedulingService.copyOrdersTrucks(copyOrderTrucksInput);
                        }
                        _modalManager.setBusy(true);
                    } else {
                        if (truckCopyResult.someTrucksAreNotCopied) {
                            abp.message.info('Something prevented some of the trucks from being copied with the order.');
                        }
                    }

                    await _orderService.recalculateStaggeredTimeForOrders({
                        orderIds: newOrderIds
                    });
                }

                abp.notify.info('Saved successfully.');
                _modalManager.setBusy(false);
                _modalManager.close();
                abp.event.trigger('app.orderModalCopied', {
                    newOrderId: newOrderIds[0]
                });
            }
            finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);
