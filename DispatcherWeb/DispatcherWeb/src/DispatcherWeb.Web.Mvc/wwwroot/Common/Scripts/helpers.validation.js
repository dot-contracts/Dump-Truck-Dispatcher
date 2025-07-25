(function () {

    abp.helper ??= {};
    abp.scheduling ??= {};
    window.app = window.app ?? {};
    app.regex ??= {};

    //app.regex.email = '^[A-Z0-9._%+-]+@([A-Z0-9-]+\\.)+[A-Z]{2,63}$';
    //app.regex.emails = '^([A-Z0-9._%+-]+@([A-Z0-9-]+\\.)+([A-Z]{2,63})[;, ]*)+$';
    app.regex.email = '^(?:[a-z0-9!#$%&\'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&\'*+/=?^_`{|}~-]+)*|"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?|\\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])$';
    //escaped ^(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|"(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?|\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])$
    app.regex.emails = '^(?:(?:[a-z0-9!#$%&\'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&\'*+/=?^_`{|}~-]+)*|"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?|\\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])[;, ]*)+$';
    //escaped ^(?:(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|"(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?|\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])[;, ]*)+$
    app.regex.latitude = "^[-+]?([1-8]?\\d(\\.\\d+)?|90(\\.0+)?)$";
    app.regex.longitude = "^\\s*[-+]?(180(\\.0+)?|((1[0-7]\\d)|([1-9]?\\d))(\\.\\d+)?)$";
    app.regex.cellPhoneNumber = '^\\+?1?[-.\\s]?(?:\\(?\\d{3}\\)?[-.\\s]?)?\\d{3}[-.\\s]?\\d{4}$';
    app.regex.url = '^https?:\\/\\/(www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{2,256}\\.[a-z]{2,6}\\b([-a-zA-Z0-9@:%_\\+.,~#?!&\\/\\/=]*)$';
    app.regex.mileage = '^\\d{0,17}(\\.\\d{0,1})?$';


    abp.scheduling.checkExistingDispatchesBeforeRemovingTruck = function (orderLineTruckId, truckCode, removeCallback, cancelCallback, doneCallback) {
        var removeMarkAsDoneOrCancel = function (handleMultiple) {
            switch (handleMultiple) {
                case "done":
                    doneCallback();
                    break;
                case "remove":
                    removeCallback();
                    break;
                default:
                    cancelCallback && cancelCallback();
                    return;
            }
        };

        var buttons = {
            cancel: "Cancel",
            remove: "Remove",
            done: "Mark as Done"
        };

        abp.services.app.scheduling.hasDispatches({
            orderLineTruckId: orderLineTruckId
        }).done(function (result) {
            if (result.acknowledgedOrLoaded) {
                abp.message.error(app.localize('TruckHasDispatch_YouMustCancelItFirstToRemoveTruck', truckCode));
                cancelCallback && cancelCallback();
            } else if (result.unacknowledged) {
                swal('There are open dispatches associated with this order line and truck. Removing this truck from the order will remove the dispatches this truck is assigned for this order line. Are you sure you want to do this?', { buttons: buttons })
                    .then(removeMarkAsDoneOrCancel);
            } else {
                swal('Do you want to remove this truck or mark its work as done on this order?', { buttons: buttons })
                    .then(removeMarkAsDoneOrCancel);
            }
        }).fail(function () {
            cancelCallback && cancelCallback();
        });

    };

    abp.scheduling.checkExistingDispatchesBeforeRemovingTrucks = function (orderLineId, removeCallback, cancelCallback, doneCallback) {
        var removeMarkAsDoneOrCancel = function (handleMultiple) {
            switch (handleMultiple) {
                case "done":
                    doneCallback();
                    break;
                case "remove":
                    removeCallback();
                    break;
                default:
                    cancelCallback && cancelCallback();
                    return;
            }
        };

        var buttons = {
            cancel: "Cancel",
            remove: "Remove",
            done: "Mark as Done"
        };

        abp.services.app.scheduling.orderLineHasDispatches({
            orderLineId: orderLineId
        }).done(function (result) {
            if (result.some(r => r.acknowledgedOrLoaded)) {
                var truckCode = result.find(r => r.acknowledgedOrLoaded).truckCode;
                abp.message.error(app.localize('TruckHasDispatch_YouMustCancelItFirstToRemoveTruck', truckCode));
                cancelCallback && cancelCallback();
            } else if (result.some(r => r.unacknowledged)) {
                swal('There are open dispatches associated with this order line. Removing these trucks from the order will remove the dispatches these trucks are assigned for this order line. Are you sure you want to do this?', { buttons: buttons })
                    .then(removeMarkAsDoneOrCancel);
            } else {
                swal('Do you want to remove these trucks or mark their work as done on this order?', { buttons: buttons })
                    .then(removeMarkAsDoneOrCancel);
            }
        }).fail(function () {
            cancelCallback && cancelCallback();
        });

    };

    abp.scheduling.checkExistingDispatchesBeforeSettingQuantityAndNumberOfTrucksZero = async function (orderLineId, materialQuantity, freightQuantity, numberOfTrucks) {
        if (materialQuantity !== 0 && materialQuantity !== null || freightQuantity !== 0 && freightQuantity !== null
            || numberOfTrucks !== 0 && numberOfTrucks !== null) {
            return true;
        }
        try {
            let result = await abp.services.app.scheduling.orderLineHasDispatches({
                orderLineId: orderLineId
            });
            if (result.some((r) => r.acknowledgedOrLoaded)) {
                abp.message.warn('This order line has a dispatch in progress so the quantity and requested trucks can’t both be zero. You’ll need to cancel the dispatch before you can make this change.');
                return false;
            }
            if (result.some((r) => r.unacknowledged)) {
                if (!await abp.message.confirm(
                    'This order line has open dispatches. If you set both the quantity and requested trucks to zero, these dispatches will be removed. Are you sure you want to do this?'
                )) {
                    return false;
                }
            }
            return true;
        } catch {
            return false;
        }
    };

    abp.helper.showValidateMessage = function ($form) {
        if (!$form.is('form')) {
            return;
        }
        var validator = $form.validate();
        if (!validator.numberOfInvalids())
            return;

        var errorMessage = '';
        for (var i = 0; i < validator.errorList.length; i++) {
            var invalidElement = validator.errorList[i].element;
            var $formGroup = $(invalidElement).closest('div.form-group');
            var $label = $formGroup.find('label[for="' + invalidElement.id + '"]');
            if ($label.length === 0) {
                $label = $formGroup.find('label');
            }
            var labelText = $label.length === 0 ? '' : $label.text().trim();
            if (labelText.slice(-1) === ':') {
                labelText = labelText.slice(0, -1);
            }
            errorMessage += '"' + labelText + '"' + ' - ' + validator.errorList[i].message + '\n';
        }

        abp.helper.formatAndShowValidationMessage(errorMessage);
    };

    abp.helper.showTruckWarning = function (trucks, message, confirmCallback) {
        var s = trucks.length > 1 ? 's' : '';
        var is = trucks.length > 1 ? 'are' : 'is';
        var trucksString = trucks.join(', ');
        return abp.message.confirmWithOptions({
            text: 'Truck' + s + ' ' + trucksString + ' ' + is + ' ' + message,
            title: ' ',
            buttons: ['No', 'Yes']
        },
            confirmCallback
        );
    };

    jQuery.fn.showValidateMessage = function () {
        abp.helper.showValidateMessage($(this));
    };

    abp.helper.formatAndShowValidationMessage = function (errorMessage) {
        abp.message.error('Please check the following: \n' + errorMessage, 'Some of the data is invalid');
    };

    abp.helper.getExpiredInsuranceTypes = function getExpiredInsuranceTypes(insurances) {
        // with grouping by type:
        //var validInsurances = new Map();
        //for (const insurance of insurances) {
        //    const isValid = !moment(insurance.expirationDate, 'YYYY-MM-DD').isBefore(moment());
        //    const key = insurance.insuranceTypeName;
        //    validInsurances.set(key, validInsurances.get(key) || isValid);
        //}
        //return Array.from(validInsurances.entries())
        //    .filter(([insuranceTypeName, isValid]) => !isValid)
        //    .map(([insuranceTypeName, isValid]) => insuranceTypeName)
        //    .join(', ');

        // with no grouping by type:
        return insurances
            .filter(x => moment(x.expirationDate, 'YYYY-MM-DD').isBefore(moment()))
            .map(x => x.insuranceTypeName)
            .join(', ');
    };
    abp.helper.validateStartEndDates = function () {
        if (arguments.length < 2) {
            return true;
        }
        var startDateIndex = 0;
        for (var i = 1; i < arguments.length; i++) {
            var startDate = arguments[startDateIndex].value;
            var endDate = arguments[i].value;
            if (startDate && endDate) {
                if (!(startDate instanceof Date)) {
                    startDate = new Date(startDate);
                }
                if (!(endDate instanceof Date)) {
                    endDate = new Date(endDate);
                }
                if (startDate > endDate) {
                    abp.message.error('The "' + abp.helper.trimEndChar(arguments[startDateIndex].title, ':') + '" cannot be after the "' + abp.helper.trimEndChar(arguments[i].title, ':') + '"');
                    return false;
                }
            } else if (startDate) {
                continue;
            }
            startDateIndex = i;
        }
        return true;
    };

    abp.helper.validateDatePickersIsNotInFuture = function ($datePickers) {
        var result = true;
        var today = new Date();
        var errorMessage = '';
        $datePickers.each(function (index, element) {
            var $ctrl = $(element);
            var selectedDate = new Date($ctrl.val());
            if (selectedDate > today) {
                result = false;
                var $label = $ctrl.closest('div.form-group').find('label');
                var labelText = abp.helper.trimEndChar($label.length === 0 ? '' : $label.text(), ':');
                errorMessage += 'The "' + labelText + '"' + ' cannot be later than today \n';
            }
        });
        if (errorMessage) {
            abp.helper.formatAndShowValidationMessage(errorMessage);
        }
        return result;
    };
    jQuery.fn.validateDatePickersIsNotInFuture = function () {
        return abp.helper.validateDatePickersIsNotInFuture($(this));
    };

    abp.helper.validateFutureDates = function () {
        if (arguments.length < 1) {
            return true;
        }
        var startDate = arguments[0].value;
        startDate = new Date(startDate);
        var today = new Date();
        if (startDate >= today) {
            abp.message.error('"' + abp.helper.trimEndChar(arguments[0].title, ':') + '" cannot be later than todays date. "');
            return false;
        }
        return true;
    };

    abp.helper.validateTodayDates = function () {
        if (arguments.length < 1) {
            return true;
        }
        var startDate = arguments[0].value;
        startDate = new Date(startDate);
        startDate.setHours(0, 0, 0, 0);
        var today = new Date();
        today.setHours(0, 0, 0, 0);

        if (Date.parse(startDate) !== Date.parse(today)) {
            abp.message.error('"' + abp.helper.trimEndChar(arguments[0].title, ':') + '" cannot be other than todays date. "');
            return false;
        }
        return true;
    };

    abp.helper.checkGreaterNumber = function () {
        if (arguments.length < 2) {
            return true;
        }
        var startDateIndex = 0;
        for (var i = 1; i < arguments.length; i++) {
            var firstNumber = arguments[startDateIndex].value;
            var secondNumber = arguments[i].value;

            if (parseInt(firstNumber) && parseInt(secondNumber)) {
                if (parseInt(firstNumber) > parseInt(secondNumber)) {
                    abp.message.error('The "' + abp.helper.trimEndChar(arguments[startDateIndex].title, ':') + '" should not be greater than "' + abp.helper.trimEndChar(arguments[i].title, ':') + '"');
                    return false;
                }
            } else if (parseInt(firstNumber)) {
                continue;
            }
            startDateIndex = i;
        }
        return true;
    };

    abp.helper.isPositiveIntegerString = function (str, maxLength) {
        var times = '';
        if (maxLength) {
            times = '{0,' + maxLength + '}';
        } else {
            times = '*';
        }
        var r = new RegExp('^([1-9]\\d' + times + ')$');
        return r.test(str);
    };

    $(function () {
        $.validator.addMethod(
            "mileage",
            function (value, element) {
                var re = new RegExp(app.regex.mileage, 'i');
                return this.optional(element) || re.test(value);
            },
            "The value must be a number with a maximum one decimal place."
        );
    });

})();
