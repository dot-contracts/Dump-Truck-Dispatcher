//remove 'd-none' class when 'show' or 'toggle' is called
jQuery.each(['toggle', 'show', 'hide'], function (_i, name) {
    let originalFn = jQuery.fn[name];
    jQuery.fn[name] = function (speed, easing, callback) {
        if (name === 'show' || typeof speed === 'boolean' && speed === true && name === 'toggle') {
            if ($(this).hasClass('d-none')) {
                $(this).removeClass('d-none');
            }
        }
        return originalFn.apply(this, arguments);
    };
});

jQuery.each([
    'slideDown',
    'slideUp',
    'slideToggle',
    'fadeIn',
    'fadeOut',
    'fadeToggle'
], function (_i, name) {
    var originalFn = jQuery.fn[name];
    jQuery.fn[name] = function (speed, easing, callback) {
        if (this.hasClass('d-none')) {
            this.removeClass('d-none').hide();
        }
        return originalFn.apply(this, arguments);
    };
});

if ($?.jstree?.defaults?.core) {
    $.jstree.defaults.core.worker = false;
}

var app = app || {};
(function () {

    var appLocalizationSource = abp.localization.getSource('DispatcherWeb');
    app.localize = function () {
        return appLocalizationSource.apply(this, arguments);
    };

    app.downloadTempFile = async function (file) {
        if (file.warningMessage && file.warningMessage.trim() !== '') {
            abp.message.warn(file.warningMessage);
            return;
        } else if (file.successMessage && file.successMessage.trim() !== '') {
            const skipDialog = abp.setting.getBoolean('App.UserOptions.DoNotShowWaitingForTicketDownload');
            if (skipDialog !== true) {
                await showSuccessDialogWithCheckbox(file.successMessage);
            }
            return;
        }
        location.href = abp.appPath + 'File/DownloadTempFile?fileType=' + encodeURIComponent(file.fileType) + '&fileToken=' + encodeURIComponent(file.fileToken) + '&fileName=' + encodeURIComponent(file.fileName);
    };

    showSuccessDialogWithCheckbox = async function (successMessage) {
        return swal({
            icon: "success",
            text: successMessage,
            content: createCheckboxElement(),
            buttons: {
                confirm: {
                    text: "OK",
                    closeModal: true
                }
            }
        }).then(async () => {
            const checkbox = document.getElementById('dont-show-checkbox');
            if (checkbox && checkbox.checked) {
                await abp.services.app.profile.setDoNotShowWaitingForTicketDownload(true);
                window.location.reload();
            }
        });
    };
    function createCheckboxElement() {
        const div = document.createElement('div');
        div.innerHTML = `
        <label class="dont-show-checkbox-label">
            <input type="checkbox" id="dont-show-checkbox" class="dont-show-checkbox">
            Don't show me this message in the future
        </label>`;
        return div;
    }

    app.downloadReportFile = function (file) {
        var url = abp.appPath + 'DownloadReportFile?fileType=' + encodeURIComponent(file.fileType) + '&fileToken=' + encodeURIComponent(file.fileToken) + '&fileName=' + encodeURIComponent(file.fileName);
        if (file.fileType === "application/pdf") {
            app.openPopup(url);
        } else {
            location.href = url;
        }
    };

    app.openPopup = async function (url) {
        var popupWindow = window.open(url, '_blank');
        if (isPopupOpen(popupWindow)) {
            popupWindow.focus();
            return popupWindow;
        }

        var hostname = window.location.hostname;

        if (!await abp.message.warn(
            `Pop-ups are blocked by your browser or another popup blocker. If you want to avoid an extra click on this report, you can set your popup blocker to allow popups from ${hostname}`,
            'Popup Blocked',
            {
                button: ['Download Document'],
            }
        )) {
            return undefined;
        }

        popupWindow = window.open(url, '_blank');
        if (isPopupOpen(popupWindow)) {
            popupWindow.focus();
            return popupWindow;
        }

        await abp.message.error(`Pop-ups are still blocked by your browser or another popup blocker. Please allow popups from ${hostname}`);
        return undefined;
    };

    function isPopupOpen(popupWindow) {
        return popupWindow
            && typeof popupWindow.closed !== 'undefined'
            && !popupWindow.closed;
    }

    app.createDateRangePickerOptions = function (extraOptions) {
        extraOptions = extraOptions ||
        {
            allowFutureDate: false
        };

        var options = {
            locale: {
                format: 'L',
                applyLabel: app.localize('Apply'),
                cancelLabel: app.localize('Cancel'),
                customRangeLabel: app.localize('CustomRange')
            },
            min: moment('2015-05-01'),
            minDate: moment('2015-05-01'),
            opens: 'left',
            ranges: {}
        };

        if (!extraOptions.allowFutureDate) {
            options.max = moment();
            options.maxDate = moment();
        }

        options.ranges[app.localize('Today')] = [moment().startOf('day'), moment().endOf('day')];
        options.ranges[app.localize('Yesterday')] = [moment().subtract(1, 'days').startOf('day'), moment().subtract(1, 'days').endOf('day')];
        options.ranges[app.localize('Last7Days')] = [moment().subtract(6, 'days').startOf('day'), moment().endOf('day')];
        options.ranges[app.localize('Last30Days')] = [moment().subtract(29, 'days').startOf('day'), moment().endOf('day')];
        options.ranges[app.localize('ThisMonth')] = [moment().startOf('month'), moment().endOf('month')];
        options.ranges[app.localize('LastMonth')] = [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')];

        return options;
    };

    app.getUserProfilePicturePath = function (profilePictureId) {
        return profilePictureId
            ? abp.appPath + 'Profile/GetProfilePictureById?id=' + profilePictureId
            : abp.appPath + 'Common/Images/default-profile-picture.png';
    };

    app.getUserProfilePicturePath = function () {
        return abp.appPath + 'Profile/GetProfilePicture?v=' + new Date().valueOf();
    };

    app.getShownLinkedUserName = function (linkedUser) {
        if (!abp.multiTenancy.isEnabled) {
            return linkedUser.username;
        } else {
            if (linkedUser.tenancyName) {
                return linkedUser.tenancyName + '\\' + linkedUser.username;
            } else {
                return '.\\' + linkedUser.username;
            }
        }
    };

    app.notification ??= {};

    app.notification.getUiIconBySeverity = function (severity) {
        switch (severity) {
            case abp.notifications.severity.SUCCESS:
                return 'fa fa-check';
            case abp.notifications.severity.WARN:
                return 'fa fa-exclamation-triangle';
            case abp.notifications.severity.ERROR:
                return 'fa fa-bolt';
            case abp.notifications.severity.FATAL:
                return 'fa fa-bomb';
            case abp.notifications.severity.INFO:
            default:
                return 'fa fa-info';
        }
    };


    app.notification.getIconFontClassBySeverity = function (severity) {
        switch (severity) {
            case abp.notifications.severity.SUCCESS:
                return ' text-success';
            case abp.notifications.severity.WARN:
                return ' text-warning';
            case abp.notifications.severity.ERROR:
                return ' text-danger';
            case abp.notifications.severity.FATAL:
                return ' text-danger';
            case abp.notifications.severity.INFO:
            default:
                return ' text-info';
        }
    };

    app.changeNotifyPosition = function (positionClass) {
        if (!toastr) {
            return;
        }

        //commented out because it was clearing priority notifications
        //toastr.clear();
        toastr.options.positionClass = positionClass;
    };

    app.waitUntilElementIsReady = function (selector, callback, checkPeriod) {
        if (!$) {
            return;
        }

        var elementCount = selector.split(',').length;

        if (!checkPeriod) {
            checkPeriod = 100;
        }

        var checkExist = setInterval(function () {
            if ($(selector).length >= elementCount) {
                clearInterval(checkExist);
                callback();
            }
        }, checkPeriod);
    };

    app.calculateTimeDifference = function (fromTime, toTime, period) {
        if (!moment) {
            return null;
        }

        var from = moment(fromTime);
        var to = moment(toTime);
        return to.diff(from, period);
    };

    app.formatTimeDifference = function (startTime, endTime) {
        const duration = moment.duration(moment(endTime).diff(moment(startTime)));
        const minutes = duration.minutes();
        const hours = duration.hours();
        const days = Math.floor(duration.asDays());
        let result = '';
        if (days) {
            result += days + 'd ';
        }
        if (hours) {
            result += hours + 'h ';
        }
        if (minutes) {
            result += minutes + 'm ';
        }
        return result;
    };

    app.sleepAsync = function sleepAsync(ms) {
        return new Promise(resolve => {
            setTimeout(resolve, ms);
        });
    };

    app.htmlUtils = {
        htmlEncodeText: function (value) {
            return $("<div/>").text(value).html();
        },

        htmlDecodeText: function (value) {
            return $("<div/>").html(value).text();
        },

        htmlEncodeJson: function (jsonObject) {
            return JSON.parse(app.htmlUtils.htmlEncodeText(JSON.stringify(jsonObject)));
        },

        htmlDecodeJson: function (jsonObject) {
            return JSON.parse(app.htmlUtils.htmlDecodeText(JSON.stringify(jsonObject)));
        }
    };

    app.guid = function () {
        //uuidv4:
        return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        );
    };

    app.showWarningIfFreeVersion = function () {
        if (abp.features.isEnabled('App.PaidFunctionalityFeature')) {
            return false;
        }

        abp.message.warn(app.localize('UpgradeToAccessThisFunctionality'));

        return true;
    };

    app.isRTL = function () {
        return (
            document.documentElement.getAttribute('dir') === 'rtl'
            || document.documentElement.getAttribute('direction') === 'rtl'
        );
    };

})();
(function ($) {

    window.app = app || {};

    app.localStorage ??= {};
    app.localStorage.getItemAsync = function (key) {
        return new Promise(resolve => {
            app.localStorage.getItem(key, value => {
                resolve(value);
            });
        });
    };

    app.sessionStorage ??= {};
    app.sessionStorage.getItem = function (key, callback) {
        app.localStorage.getItem('sessionStorageObject',
            function (sessionObject) {
                sessionObject = sessionObject || getDefaultSessionStorageObject();
                callback && callback(sessionObject[key]);
            });
    };

    app.sessionStorage.setItem = function (key, value) {
        app.localStorage.getItem('sessionStorageObject',
            function (sessionObject) {
                sessionObject = sessionObject || getDefaultSessionStorageObject();
                sessionObject[key] = value;
                app.localStorage.setItem('sessionStorageObject', sessionObject);
            });
    };

    function getDefaultSessionStorageObject() {
        return {
            //quoteBaseFuelCost: ''

        };
    }

    app.sessionStorage.clear = function () {
        app.localStorage.setItem('sessionStorageObject', getDefaultSessionStorageObject());
    };

    if (!String.prototype.startsWith) {
        String.prototype.startsWith = function (search, pos) {
            return this.substring(!pos || pos < 0 ? 0 : +pos, search.length) === search;
        };
    }


    abp.helper ??= {};
    abp.helper.reports = function () {
        var _reportService;

        function setReportService(reportService) {
            _reportService = reportService;
        }
        function getReportService() {
            return _reportService;
        }

        var _formDataHandler;

        function setFormDataHandler(formDataHandler) {
            _formDataHandler = formDataHandler;
        }
        function executeFormDataHandler(formData) {
            if (_formDataHandler) {
                return _formDataHandler(formData);
            }
            return formData;
        }

        return {
            setReportService: setReportService,
            getReportService: getReportService,
            setFormDataHandler: setFormDataHandler,
            executeFormDataHandler: executeFormDataHandler
        };
    }();

    abp.helper.trimEndChar = function (str, char) {
        var re = new RegExp(char + "+$", "g");
        return str.replace(re, '');
    };


    abp.helper.getLocationAsync = function () {
        return new Promise(resolve => {
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(onSuccess, onError);
            } else {
                resolve(null);
            }

            function onSuccess(position) {
                resolve(position);
            }

            function onError(error) {
                abp.notify.error('Location error: ' + error.message);
                resolve(null);
            }
        });

    };

    abp.helper.getShiftName = function (shift) {
        switch (shift) {
            case abp.enums.shifts.shift1:
                return abp.setting.get('App.General.ShiftName1');
            case abp.enums.shifts.shift2:
                return abp.setting.get('App.General.ShiftName2');
            case abp.enums.shifts.shift3:
                return abp.setting.get('App.General.ShiftName3');
            case abp.enums.shifts.noShift:
                return "[No Shift]";
        }
        return '';
    };

    abp.helper.getDefaultStartTime = function (date) {
        date = date ? moment(date).clone() : moment();
        var time = moment(abp.setting.get('App.DispatchingAndMessaging.DefaultStartTime') + 'Z');
        return date.set({
            hour: time.hour(),
            minute: time.minute(),
            second: time.second()
        });
    };

    abp.helper.convertLocalDateTimeToUtc = function (localDateTime) {
        return localDateTime ? moment(localDateTime, 'YYYY-MM-DDTHH:mm:ss').utc().format('YYYY-MM-DDTHH:mm:ss[Z]') : localDateTime;
    };

    abp.helper.getQueryStringParameter = function (name) {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(name);
    };

})(jQuery);
(function ($) {

    $(document).on('select2:open', () => {
        document.querySelector('.select2-container--open .select2-search__field').focus();
    });

    jQuery.fn.select2Uom = function (additionalOptions) {
        additionalOptions ??= {};
        var options = {
            abpServiceMethod: listCacheSelectLists.uom(),
            showAll: true,
            allowClear: false
        };
        options = $.extend(true, {}, options, additionalOptions);
        return $(this).select2Init(options).change(function () {
            var data = $(this).select2('data');
            if (data?.length && data[0].item) {
                $(this).setUomBaseId(data[0].item.uomBaseId);
            }
        });
    };

    jQuery.fn.getUomBaseId = function () {
        var value = $(this).data('uombaseid');
        return value ? Number(value) : null;
    };

    jQuery.fn.setUomBaseId = function (value) {
        $(this).data('uombaseid', value);
    };

    if (abp.ui?.block) {
        let abpBlock = abp.ui.block;
        abp.ui.block = function (elm) {
            if (elm
                && $(elm).is(':button')
                && !$(elm).is(':disabled')
            ) {
                $(elm).prop('disabled', true);
                $(elm).data('disabled-by-blockUI', true);
            }
            abpBlock(elm);
        };
    }

    if (abp.ui?.unblock) {
        let abpUnblock = abp.ui.unblock;
        abp.ui.unblock = function (elm) {
            if (elm
                && $(elm).is(':button')
                && $(elm).is(':disabled')
                && $(elm).data('disabled-by-blockUI') === true
            ) {
                $(elm).prop('disabled', false);
                $(elm).data('disabled-by-blockUI', false);
            }
            abpUnblock(elm);
        };
    }

    abp.helper ??= {};
    abp.helper.ui ??= {};

    abp.helper.ui.initChildDropdown = function initChildDropdown(options) {
        var parentDropdown = options.parentDropdown;
        var childDropdown = options.childDropdown;
        var abpServiceMethod = options.abpServiceMethod;
        var abpServiceData = options.abpServiceData || {};
        var optionCreatedCallback = options.optionCreatedCallback;
        var onChildDropdownUpdatedCallbacks = [];
        var updateChildDropdown = function (callback, updateOptions) {
            updateOptions ??= {};
            var parentValue = parentDropdown.val();
            abp.ui.setBusy(childDropdown);
            abpServiceMethod($.extend({}, abpServiceData, { id: parentValue }))
                .done(function (data) {
                    var oldChildValue = childDropdown.val();
                    var oldChildOption = childDropdown.find('option[value="' + oldChildValue + '"]').clone();
                    var childPlaceholder = childDropdown.find('option[value=""]').clone();
                    var setPlaceholderText = function (placeholderText) {
                        childPlaceholder.text(placeholderText);
                        if (childDropdown.data("select2")) {
                            childDropdown.data("placeholder", placeholderText);
                            childDropdown.data("select2").selection.placeholder.text = placeholderText;
                            childDropdown.trigger('change.select2');
                        }
                    };
                    childDropdown.empty();
                    childDropdown.append(childPlaceholder);
                    if (data.items.length) {
                        $.each(data.items, function (ind, val) {
                            var option = $('<option></option>').text(val.name).attr('value', val.id);

                            if (val.item) {
                                for (var itemProperty in val.item) {
                                    if (val.item.hasOwnProperty(itemProperty)) {
                                        option.data(itemProperty, val.item[itemProperty]);
                                    }
                                }
                            }
                            if (optionCreatedCallback)
                                optionCreatedCallback(option, val);
                            option.appendTo(childDropdown);
                        });
                        if (oldChildValue !== '') {
                            if (childDropdown.find('option[value="' + oldChildValue + '"]').length === 0) {
                                childDropdown.append(oldChildOption);
                            }
                            childDropdown.val(oldChildValue); //.change();
                        }
                        setPlaceholderText(childPlaceholder.data('placeholder-default') || 'Select an option');
                    } else {
                        if (oldChildValue !== '') {
                            childDropdown.append(oldChildOption);
                        }
                        setPlaceholderText(childPlaceholder.data(parentValue ? 'placeholder-no-items' : 'placeholder-no-parent') || 'No options available');
                    }
                    if (callback) {
                        callback(data);
                    }
                    if (onChildDropdownUpdatedCallbacks.length && !updateOptions.skipCallbacks) {
                        $.each(onChildDropdownUpdatedCallbacks, function (ind, c) {
                            c && c(data);
                        });
                    }
                })
                .always(function () {
                    abp.ui.clearBusy(childDropdown);
                });
        };
        updateChildDropdown(null, { skipCallbacks: true });

        parentDropdown.change(function () {
            if (childDropdown.val() !== '') {
                childDropdown.val('').change();
            }
            updateChildDropdown();
        });

        var onChildDropdownUpdated = function (callbackToAdd) {
            onChildDropdownUpdatedCallbacks.push(callbackToAdd);
        };

        return {
            parentDropdown: parentDropdown,
            childDropdown: childDropdown,
            updateChildDropdown: updateChildDropdown,
            onChildDropdownUpdated: onChildDropdownUpdated
        };
    };

    abp.helper.ui.syncNumericInputValueIfOtherIsEmptyOrZero = function (source, target) {
        if (target.val() !== '' && Number(target.val()) !== 0
            || !source.val()
            || Number(source.val()) === 0
        ) {
            return;
        }
        target.val(source.val());
    };

    abp.helper.ui.syncDropdownValueIfOtherIsNull = function syncDropdownValueIfOtherIsNull(source, target, successCallback) {
        if (target.val() || !source.val()) {
            return;
        }
        var option = abp.helper.ui.getDropdownValueAndLabel(source);
        abp.helper.ui.addAndSetDropdownValue(target, option.value, option.label);
        successCallback && successCallback(source, target, option);
    };

    abp.helper.ui.syncUomDropdowns = function syncUomDropdowns(materialUomDropdown, freightUomDropdown, designationDropdown, materialQuantityInput, freightQuantityInput) {
        var designationIsFreightAndMaterial = function () {
            return designationDropdown ? abp.enums.designations.freightAndMaterial.includes(Number(designationDropdown.val())) : true;
        };
        var designationIsFreightOnly = function () {
            return designationDropdown ? abp.enums.designations.freightOnly.includes(Number(designationDropdown.val())) : true;
        };
        var uomsAreSame = function () {
            return materialUomDropdown.val() === freightUomDropdown.val() && materialUomDropdown.val();
        };
        var shouldSyncQuantityControls = function () {
            return designationIsFreightAndMaterial() && uomsAreSame()
                || designationIsFreightOnly(); //&& abp.features.isEnabled('App.SeparateMaterialAndFreightItems');
        };
        var shouldSyncUomControls = function () {
            return designationIsFreightAndMaterial()
                || designationIsFreightOnly(); //&& abp.features.isEnabled('App.SeparateMaterialAndFreightItems');
        };
        var isHoursUomSelected = function (dropdown) {
            var uom = abp.helper.ui.getDropdownValueAndLabel(dropdown);
            return uom.label === 'Hours';
        };
        var getMinimumFreightAmount = function () {
            if (abp.setting.getBoolean('App.Invoice.CalculateMinimumFreightAmount') === false) {
                return 0;
            }
            var uom = abp.helper.ui.getDropdownValueAndLabel(freightUomDropdown);
            switch (uom.label) {
                case 'Tons':
                    return Number(abp.setting.get('App.Invoice.MinimumFreightAmountForTons'));
                case 'Hours':
                    return Number(abp.setting.get('App.Invoice.MinimumFreightAmountForHours'));
            }
            return 0;
        };
        var syncUomBaseId = function (source, target) {
            var sourceUomBaseId = source.getUomBaseId();
            target.setUomBaseId(sourceUomBaseId);
        };
        materialUomDropdown.change(function () {
            if (shouldSyncUomControls()) {
                abp.helper.ui.syncDropdownValueIfOtherIsNull(materialUomDropdown, freightUomDropdown, syncUomBaseId);
            }
        });
        freightUomDropdown.change(function () {
            if (shouldSyncUomControls() && !isHoursUomSelected(freightUomDropdown)) {
                abp.helper.ui.syncDropdownValueIfOtherIsNull(freightUomDropdown, materialUomDropdown, syncUomBaseId);
            }
        });
        if (materialQuantityInput && freightQuantityInput) {
            materialQuantityInput.change(function () {
                if (shouldSyncQuantityControls()) {
                    let minimumFreightAmount = getMinimumFreightAmount();
                    if (minimumFreightAmount > 0
                        && materialQuantityInput.val()
                        && Number(materialQuantityInput.val()) < minimumFreightAmount
                    ) {
                        //When the material rate is set to a value less than the minimum (regardless of the current value of the freight quantity), the freight quantity should be set to the minimum freight quantity for that UOM
                        freightQuantityInput.val(minimumFreightAmount);
                    } else {
                        abp.helper.ui.syncNumericInputValueIfOtherIsEmptyOrZero(materialQuantityInput, freightQuantityInput);
                    }
                }
            });
            freightQuantityInput.change(function () {
                if (shouldSyncQuantityControls()) {
                    abp.helper.ui.syncNumericInputValueIfOtherIsEmptyOrZero(freightQuantityInput, materialQuantityInput);
                }
            });
            freightQuantityInput.blur(async function () {
                //If the user changes the freight quantity to a value below the minimum for the freight uom, the user should get a warning on exiting "This value is less than the minimum. Are you sure you want to change this value?". If they choose yes, they are able to exit the freight quantity control. If they choose no, the focus remains in the freight quantity. In other words, we'll allow them to change it to below the minimum but will warn them.
                let minimumFreightAmount = getMinimumFreightAmount();
                if (minimumFreightAmount > 0
                    && freightQuantityInput.val()
                    && Number(freightQuantityInput.val()) < minimumFreightAmount
                ) {
                    if (await abp.message.confirmWithYesNo('This value is less than the minimum. Are you sure you want to change this value?', ' ')) {
                        return;
                    }
                    freightQuantityInput.focus();
                }
            });
        }
    };

    abp.ui.initDesignationDropdown = function (designationDropdown, value) {
        designationDropdown.find('option').remove();
        designationDropdown.append($('<option></option>').text('Select a designation').attr('value', ''));

        let historicalDesignations = [
            abp.enums.designation.backhaulFreightOnly,
            abp.enums.designation.backhaulFreightAndMaterial,
            abp.enums.designation.disposal,
            abp.enums.designation.backHaulFreightAndDisposal,
            abp.enums.designation.straightHaulFreightAndDisposal,
        ].filter(d => Number(value) === d || !abp.features.isEnabled('App.SeparateMaterialAndFreightItems'));

        let designations = [
            abp.enums.designation.freightOnly,
            abp.enums.designation.materialOnly,
            abp.enums.designation.freightAndMaterial,
            ...historicalDesignations,
        ];

        designationDropdown.append(
            designations
                .map(d => $('<option>').attr('value', d).text(abp.enums.designationName[d]))
                .reduce((prev, curr) => prev ? prev.add(curr) : curr, $())
        );

        if (value) {
            designationDropdown.val(value);
        } else {
            designationDropdown.val('');
        }

        designationDropdown.select2Init({
            showAll: true,
            allowClear: false,
        });
    };

    abp.helper.ui.getEmailDeliveryStatusIcon = function getEmailDeliveryStatusIcon(status) {
        switch (status) {
            default:
                return null; //$('<span>');
            case abp.enums.emailDeliveryStatus.notProcessed:
            case abp.enums.emailDeliveryStatus.processed:
            case abp.enums.emailDeliveryStatus.deferred:
                return $('<span class="fa-stack" title="Sending">' +
                    '<i class="fa fa-envelope fa-stack-1x fa-lg"></i>' +
                    '<i class="fa fa-clock fa-stack-1x email-delivery-status-sending-clock"></i>' +
                    '</span>');
            case abp.enums.emailDeliveryStatus.dropped:
            case abp.enums.emailDeliveryStatus.bounced:
                return $('<span class="fa-stack" title="Delivery failed">' +
                    '<i class="fa fa-envelope fa-stack-1x fa-lg"></i>' +
                    '<i class="fa fa-times fa-stack-1x text-danger email-delivery-status-failed-times"></i>' +
                    '</span>');
            case abp.enums.emailDeliveryStatus.delivered:
                return $('<i class="fa fa-envelope fa-lg icon-center" title="Delivered"></i>');
            //    '<span class="fa-stack">' +
            //      '<i class="fa fa-envelope-open fa-stack-1x fa-lg"></i>' +
            //      '<i class="fa fa-check fa-stack-1x text-success email-delivery-status-delivered-check"></i>' +
            //    '</span>'
            case abp.enums.emailDeliveryStatus.opened:
                return $('<i class="fa fa-envelope-open fa-lg icon-center" title="Opened"></i>');
        }
    };

    abp.helper.ui.nonbillableFreightIcon = `
        <span class="fa-stack" title="Nonbillable Freight">
            <i class="fas fa-truck fa-stack-1x fa-1x"></i>
            <i class="fas fa-ban fa-stack-2x fa-70p color-red line-height-inherit"></i>
        </span>
    `;

    abp.helper.ui.nonbillableMaterialIcon = `
        <span class="fa-stack" title="Nonbillable Material">
            <i class="fas fa-dolly fa-stack-1x fa-1x"></i>f
            <i class="fas fa-ban fa-stack-1x fa-70p color-red line-height-inherit"></i>
        </span>
    `;

    abp.helper.ui.initCannedTextLists = function initCannedTextLists() {
        $('.insert-canned-text-list').each(function () {
            var list = $(this);
            if (list.hasClass('initialized')) {
                return;
            }
            list.addClass('initialized');
            if (!abp.auth.hasPermission('Pages.Misc.SelectLists.CannedTexts')) {
                list.closest('.btn-group').hide();
                return;
            }
            list.find('li[data-id="loading"]').show();
            abp.services.app.cannedText.getCannedTextsSelectList({}).done(function (data) {
                if (data.items.length > 0) {
                    list.find('li[data-id="no-items"]').hide();
                    $.each(data.items, function (ind, item) {
                        var li = $('<li></li>').attr('data-id', item.id).appendTo(list);
                        li.append($('<a href="#"></a>').text(item.name));
                    });
                } else {
                    list.find('li[data-id="no-items"]').show();
                }
                list.find('li[data-id="loading"]').hide();
            });
            var target = $('#' + list.attr('data-target-id'));
            list.on('click', 'li', function (e) {
                e.preventDefault();
                var cannedTextId = $(this).attr('data-id');
                if (cannedTextId === "no-items" || cannedTextId === "loading" || cannedTextId === undefined || cannedTextId === '') {
                    return;
                }
                //abp.ui.setBusy(target);
                abp.services.app.cannedText.getCannedTextForEdit({ id: cannedTextId }).done(function (data) {
                    target.replaceSelectedText(data.text);
                }).always(function () {
                    //abp.ui.clearBusy(target);
                });
            });
        });
    };
    $(function () {
        abp.helper.ui.initCannedTextLists();
    });

    abp.helper.ui.addAndSetDropdownValues = function addAndSetDropdownValues(dropdown, array) {
        let values = [];
        array.forEach(i => {
            $('<option></option>').text(i.name).attr('value', i.id).appendTo(dropdown);
            values.push(i.id);
        });
        dropdown.val(values).change();
    };

    jQuery.fn.replaceSelectedText = function (newText) {
        var start = $(this).prop('selectionStart');
        var end = $(this).prop('selectionEnd');
        var value = $(this).val();
        var newValue = value.substring(0, start) + newText + value.substring(end);
        $(this).val(newValue);
        $(this).prop('selectionStart', start + newText.length);
        $(this).prop('selectionEnd', start + newText.length);
        $(this).focus();
    };



    abp.message ??= {};
    abp.message.confirmWithOptions = function (userOpts, callback) {
        //use userOpts.text and userOpts.title
        var opts = $.extend(
            {},
            abp.libs.sweetAlert.config.default,
            abp.libs.sweetAlert.config.confirm,
            userOpts
        );

        return $.Deferred(function ($dfd) {
            swal(opts)
                .then((isConfirmed) => {
                    callback && callback(isConfirmed);
                    $dfd.resolve(isConfirmed);
                });
        });
    };

    abp.message.confirmWithYesNoCancel = function (message, title, options) {
        return abp.message.confirm(message, title, undefined, {
            buttons: {
                cancel: {
                    text: 'Cancel',
                    value: null,
                    visible: true,
                },
                no: {
                    text: 'No',
                    value: false,
                    className: 'swal-button--cancel',
                },
                yes: {
                    text: 'Yes',
                    value: true,
                    className: 'swal-button--confirm',
                },
            },
            ...options,
        }).then(r => {
            if (r === null) {
                throw new Error('The user closed Yes/No prompt without answering.');
            }
            return r;
        });
    };

    abp.message.confirmWithYesNo = function (message, title, options) {
        return abp.message.confirm(message, title, undefined, {
            buttons: {
                no: {
                    text: 'No',
                    value: false,
                    className: 'swal-button--cancel',
                },
                yes: {
                    text: 'Yes',
                    value: true,
                    className: 'swal-button--confirm',
                },
            },
            ...options,
        }).then(r => {
            if (r === null) {
                throw new Error('The user closed Yes/No prompt without answering.');
            }
            return r;
        });
    };

    abp.message.confirmWithDangerYesNo = function (message, title, options) {
        return abp.message.confirm(message, title, undefined, {
            buttons: ['No', 'Yes'],
            dangerMode: true,
            ...options,
        });
    };

    function storeNavbarCollapsedState(isCollapsed) {
        app.localStorage.setItem('isNavbarCollapsed', isCollapsed);
    }

    $("body").on('expanded.pushMenu', function () {
        storeNavbarCollapsedState(false);
    });
    $("body").on('collapsed.pushMenu', function () {
        storeNavbarCollapsedState(true);
    });

    abp.events ??= {};
    abp.event.once = function (eventName, callback) {
        var handler = function () {
            abp.event.off(eventName, handler);
            callback();
        };
        abp.event.on(eventName, handler);
    };


    //XSS in notifications
    abp.notifications.showUiNotifyForUserNotification = function (userNotification, options) {
        var message = abp.notifications.getFormattedMessageFromUserNotification(userNotification);
        message = abp.helper.dataTables.renderText(message);
        var uiNotifyFunc = abp.notifications.getUiNotifyFuncBySeverity(userNotification.notification.severity);
        uiNotifyFunc(message, undefined, options);
    };

})(jQuery);
