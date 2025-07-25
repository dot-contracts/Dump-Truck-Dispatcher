(function () {
    $(function () {
        var _tenantSettingsService = abp.services.app.tenantSettings;
        //var _quickbooksOnlineService = abp.services.app.quickbooksOnline;
        var _fuelSurchargeCalculationService = abp.services.app.fuelSurchargeCalculation;
        var _initialTimeZone = $('#GeneralSettingsForm [name=Timezone]').val();
        var _usingDefaultTimeZone = $('#GeneralSettingsForm [name=TimezoneForComparison]').val() === abp.setting.values["Abp.Timing.TimeZone"];

        var _testSmsNumberModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Settings/TestSmsNumberModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Settings/_TestSmsNumberModal.js',
            modalClass: 'TestSmsNumberModal'
        });

        var createOrEditItemModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Items/CreateOrEditItemModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Items/_CreateOrEditItemModal.js',
            modalClass: 'CreateOrEditItemModal',
            modalSize: 'xl'
        });

        $('form#GeneralSettingsForm').validate();
        $('form#DispatchingAndMessagingSettingsForm').validate();
        $('form#TicketsSettingsForm').validate();
        $('form#LeaseHaulerSettingsForm').validate();

        $('form#GeotabSettingsForm').validate();
        $('form#SamsaraSettingsForm').validate();
        $('form#IntelliShiftSettingsForm').validate();
        $('form#DtdTrackerSettingsForm').validate();
        $('form#FuelSettingsForm').validate();
        $('form#HaulZoneSettingsForm').validate();
        $('form#QuoteSettingsForm').validate();
        $('form#JobSettingsForm').validate();

        $('form#BillingSettingsForm').validate();
        $.validator.addMethod(
            "regex",
            function (value, element, regexp) {
                var re = new RegExp(regexp, 'i');
                return this.optional(element) || re.test(value);
            },
            "Please check your input."
        );
        $('#SmsPhoneNumber').rules('add', { regex: app.regex.cellPhoneNumber });
        $('#MapBaseUrl').rules('add', { regex: app.regex.url });
        $('#SamsaraBaseUrl').rules('add', { regex: app.regex.url });
        $('#DtdTrackerBaseUrl').rules('add', { regex: app.regex.url });

        $("#TimeTrackingDefaultTimeClassificationId").select2Init({
            abpServiceMethod: abp.services.app.timeClassification.getTimeClassificationsSelectList,
            showAll: true,
            allowClear: true
        });

        $('#DriverIsPaidForLoadBasedOn').select2Init({
            showAll: true,
            allowClear: false
        });

        $('#PayStatementReportOrientation').select2Init({
            showAll: true,
            allowClear: false
        });

        $('#AllowProductionPay').change(refreshProductionPayControls);
        refreshProductionPayControls();
        function refreshProductionPayControls() {
            if ($('#AllowProductionPay').is(':checked')) {
                $('#DefaultToProductionPay').closest('.form-group').show();
                $('#PreventProductionPayOnHourlyJobs').closest('.form-group').show();
                $('#AllowDriverPayRateDifferentFromFreightRate').closest('.form-group').show();
                $('#AllowLoadBasedRates').closest('.form-group').show();
                $('#DriverIsPaidForLoadBasedOn').closest('.form-group').show();
            } else {
                $('#DefaultToProductionPay').prop('checked', false).closest('.form-group').hide();
                $('#PreventProductionPayOnHourlyJobs').prop('checked', false).closest('.form-group').hide();
                $('#AllowDriverPayRateDifferentFromFreightRate').prop('checked', false).closest('.form-group').hide();
                $('#AllowLoadBasedRates').prop('checked', false).closest('.form-group').hide();
                $('#DriverIsPaidForLoadBasedOn').closest('.form-group').hide();
            }
        }

        $('#FulcrumIntegrationIsEnabled').change(refreshFulcrumControls);
        refreshFulcrumControls();
        function refreshFulcrumControls() {
            if ($('#FulcrumIntegrationIsEnabled').is(':checked')) {
                $('#FulcrumCustomerNumber').closest('.form-group').show();
                $('#FulcrumUserName').closest('.form-group').show();
                $('#FulcrumPassword').closest('.form-group').show();
            } else {
                $('#FulcrumCustomerNumber').val('').closest('.form-group').hide();
                $('#FulcrumUserName').val('').closest('.form-group').hide();
                $('#FulcrumPassword').val('').closest('.form-group').hide();
            }
        }

        $('#CalculateMinimumFreightAmount').change(refreshMinimumFreightAmountControls);
        refreshMinimumFreightAmountControls();
        function refreshMinimumFreightAmountControls() {
            if ($('#CalculateMinimumFreightAmount').is(':checked')) {
                $('#MinimumFreightAmountForHours').closest('.form-group').show();
                $('#MinimumFreightAmountForTons').closest('.form-group').show();
            } else {
                $('#MinimumFreightAmountForHours').val('0').closest('.form-group').hide();
                $('#MinimumFreightAmountForTons').val('0').closest('.form-group').hide();
            }
        }

        $("#ItemIdToUseForFuelSurchargeOnInvoice").select2Init({
            abpServiceMethod: listCacheSelectLists.item(),
            allowClear: true,
            showAll: true,
            addItemCallback: abp.auth.isGranted('Pages.Items') ? async function (newItemName) {
                var result = await app.getModalResultAsync(
                    createOrEditItemModal.open({ name: newItemName })
                );
                return {
                    id: result.id,
                    name: result.name
                };
            } : null
        });

        $("#HaulRateCalculationBaseUomIdForCod").select2Uom({
            abpServiceParams: {
                getUomBaseId: true,
                uomBaseIds: abp.enums.uomBases.haulRateCalculation,
            },
        });
        $("#HaulRateCalculationBaseUomId").select2Uom({
            abpServiceParams: {
                getUomBaseId: true,
                uomBaseIds: abp.enums.uomBases.haulRateCalculation,
            },
        });

        $("#GeneralSettingsForm [name=Timezone]").select2Init({
            showAll: true,
            allowClear: true
        });

        function toggleShowFuelSurcharge() {
            let showFuelSurcharge = $('#ShowFuelSurcharge').is(':checked');
            if (showFuelSurcharge) {
                $('#FuelSurchargeContainer').slideDown('fast');
            } else {
                $('#FuelSurchargeContainer').slideUp('fast');
            }
            $('label[for="ItemIdToUseForFuelSurchargeOnInvoice"]').toggleClass('required-label', showFuelSurcharge);
        }

        $("#ShowFuelSurcharge").change(function () {
            toggleShowFuelSurcharge();
        });

        var fuelSurchargeDataList = $("#FuelSurchargeCalculationList").dataListInit({
            listMethod: _fuelSurchargeCalculationService.getFuelSurchargeCalculations,
            editMethod: _fuelSurchargeCalculationService.editFuelSurchargeCalculation,
            deleteMethod: _fuelSurchargeCalculationService.deleteFuelSurchargeCalculation,
            columns: [
                {
                    title: 'Default',
                    data: null,
                    width: '',
                    editor: dataList.editors.checkbox,
                    isEditable: (rowData, isRowEditable) => {
                        return rowData.id ? true : false;
                    },
                    readData: (rowData) => rowData.id && abp.setting.getInt('App.Fuel.DefaultFuelSurchargeCalculationId') === rowData.id,
                    writeData: async (value, rowData) => {
                        let newId = value ? rowData.id : 0;
                        await _fuelSurchargeCalculationService.setDefaultFuelSurchargeCalculationId(newId);
                        abp.setting.values['App.Fuel.DefaultFuelSurchargeCalculationId'] = newId.toString();
                        fuelSurchargeDataList.readValuesFromModel();
                    },
                },
                {
                    title: 'Name',
                    data: 'name',
                    width: '280px',
                    editor: dataList.editors.text,
                    editorOptions: {
                        required: true,
                        maxLength: 50,
                    },
                    isVisible: (rowData) => {
                        return [
                            abp.enums.fuelSurchargeCalculationType.basedOnActualFuelCost,
                            abp.enums.fuelSurchargeCalculationType.simplePercentage,
                        ].includes(rowData.type);
                    },
                },
                {
                    title: 'Base Fuel Cost',
                    data: 'baseFuelCost',
                    width: '100px',
                    editor: dataList.editors.decimal,
                    editorOptions: {
                        allowNull: false,
                        min: 0,
                        max: 99,
                    },
                    isVisible: (rowData) => {
                        return [
                            abp.enums.fuelSurchargeCalculationType.basedOnActualFuelCost,
                        ].includes(rowData.type);
                    },
                },
                {
                    title: 'Can Change',
                    data: 'canChangeBaseFuelCost',
                    width: '',
                    editor: dataList.editors.checkbox,
                    isVisible: (rowData) => {
                        return [
                            abp.enums.fuelSurchargeCalculationType.basedOnActualFuelCost,
                        ].includes(rowData.type);
                    },
                },
                {
                    title: 'Increment',
                    data: 'increment',
                    width: '200px',
                    editor: dataList.editors.decimal,
                    editorOptions: {
                        allowNull: false,
                        min: 0.01,
                        max: 10000,
                    },
                    isVisible: (rowData) => {
                        return [
                            abp.enums.fuelSurchargeCalculationType.basedOnActualFuelCost,
                        ].includes(rowData.type);
                    },
                },
                {
                    title: 'Freight Rate %',
                    data: 'freightRatePercent',
                    width: '100px',
                    editor: dataList.editors.decimal,
                    editorOptions: {
                        allowNull: false,
                        min: 0,
                        max: 100,
                    },
                    isVisible: (rowData) => {
                        return [
                            abp.enums.fuelSurchargeCalculationType.basedOnActualFuelCost,
                            abp.enums.fuelSurchargeCalculationType.simplePercentage,
                        ].includes(rowData.type);
                    },
                },
                {
                    title: 'Credit',
                    data: 'credit',
                    width: '',
                    editor: dataList.editors.checkbox,
                    isVisible: (rowData) => {
                        return [
                            abp.enums.fuelSurchargeCalculationType.basedOnActualFuelCost,
                        ].includes(rowData.type);
                    },
                },
            ]
        });

        $("#AddNewFuelSurchargeCalculation").click(async function () {
            let type = await swal(
                'Calculation Type',
                'Select the button for the type of calculation you want to add',
                {
                    buttons: {
                        [abp.enums.fuelSurchargeCalculationType.simplePercentage]: app.localize("SimplePercentage"),
                        [abp.enums.fuelSurchargeCalculationType.basedOnActualFuelCost]: app.localize("BasedOnActualFuelCost"),
                    },
                }
            );
            if (!type) {
                return;
            }
            fuelSurchargeDataList.addRow({
                type: Number(type),
            });
        });

        //Toggle form based registration options
        var _$selfRegistrationOptions = $('#FormBasedRegistrationSettingsForm')
            .find('input[name=IsNewRegisteredUserActiveByDefault],input[name=UseCaptchaOnRegistration]')
            .closest('.md-checkbox');

        function toggleSelfRegistrationOptions() {
            if ($('#Setting_AllowSelfRegistration').is(':checked')) {
                _$selfRegistrationOptions.slideDown('fast');
            } else {
                _$selfRegistrationOptions.slideUp('fast');
            }
        }

        $('#Setting_AllowSelfRegistration').change(function () {
            toggleSelfRegistrationOptions();
        });

        toggleSelfRegistrationOptions();

        function togglePaymentSettingVisibility() {

            if ($('#PaymentProcessor').val() === abp.enums.paymentProcessor.heartlandConnect.toString()) {
                $('#heartlandSettingsBlock').show();
            } else {
                $('#heartlandSettingsBlock').hide();
            }
        }

        $('#PaymentProcessor').change(togglePaymentSettingVisibility);

        togglePaymentSettingVisibility();

        //Toggle SMTP credentials
        var _$smtpCredentialFormGroups = $('#EmailSmtpSettingsForm')
            .find('input[name=SmtpDomain],input[name=SmtpUserName],input[name=SmtpPassword]')
            .closest('.form-group');

        function toggleSmtpCredentialFormGroups() {
            if ($('#Settings_SmtpUseDefaultCredentials').is(':checked')) {
                _$smtpCredentialFormGroups.slideUp('fast');
            } else {
                _$smtpCredentialFormGroups.slideDown('fast');
            }
        }

        $('#Settings_SmtpUseDefaultCredentials').change(function () {
            toggleSmtpCredentialFormGroups();
        });

        toggleSmtpCredentialFormGroups();

        //Toggle User lockout

        var _$userLockOutSettingsFormItems = $('#UserLockOutSettingsForm')
            .find('input')
            .not('#Setting_UserLockOut_IsEnabled')
            .closest('.form-group');

        function toggleUserLockOutSettingsFormItems() {
            if ($('#Setting_UserLockOut_IsEnabled').is(':checked')) {
                _$userLockOutSettingsFormItems.slideDown('fast');
            } else {
                _$userLockOutSettingsFormItems.slideUp('fast');
            }
        }

        toggleUserLockOutSettingsFormItems();

        $('#Setting_UserLockOut_IsEnabled').change(function () {
            toggleUserLockOutSettingsFormItems();
        });

        //Toggle two factor login

        var _$twoFactorLoginSettingsFormItems = $('#TwoFactorLoginSettingsForm')
            .find('input')
            .not('#Setting_TwoFactorLogin_IsEnabled')
            .closest('.md-checkbox');

        function toggleTwoFactorLoginSettingsFormItems() {
            if ($('#Setting_TwoFactorLogin_IsEnabled').is(':checked')) {
                _$twoFactorLoginSettingsFormItems.slideDown('fast');
            } else {
                _$twoFactorLoginSettingsFormItems.slideUp('fast');
            }
        }

        toggleTwoFactorLoginSettingsFormItems();

        $('#Setting_TwoFactorLogin_IsEnabled').change(function () {
            toggleTwoFactorLoginSettingsFormItems();
        });

        //Security
        $('#Setting_PasswordComplexity_UseDefaultSettings').change(function (val) {
            if ($('#Setting_PasswordComplexity_UseDefaultSettings').is(":checked")) {
                $('#DefaultPasswordComplexitySettingsForm').show();
                $('#PasswordComplexitySettingsForm').hide();
            } else {
                $('#DefaultPasswordComplexitySettingsForm').hide();
                $('#PasswordComplexitySettingsForm').show();
            }
        });

        function getDefaultPasswordComplexitySettings() {
            //note: this is a fix for '$('#DefaultPasswordComplexitySettingsForm').serializeFormToObject()' always returns true for checkboxes if they are disabled.
            var $disabledDefaultPasswordInputs = $('#DefaultPasswordComplexitySettingsForm input:disabled');
            $disabledDefaultPasswordInputs.removeAttr("disabled");
            var defaultPasswordComplexitySettings = $('#DefaultPasswordComplexitySettingsForm').serializeFormToObject();
            $disabledDefaultPasswordInputs.attr("disabled", "disabled");
            return defaultPasswordComplexitySettings;
        }

        //Appearance/Logo
        $('#SettingsLogoUploadForm').ajaxForm({
            beforeSubmit: function (formData, jqForm, options) {

                var $fileInput = $('#SettingsLogoUploadForm input[name=ApplicationLogoImage]');
                var files = $fileInput.get()[0].files;

                if (!files.length) {
                    return false;
                }

                var file = files[0];

                //File type check
                var type = file.type.slice(file.type.lastIndexOf('/') + 1);
                if (!app.allowedLogoTypes.includes(type)) {
                    abp.message.warn(app.localize('File_Invalid_Type_Error'));
                    return false;
                }

                //File size check
                if (file.size > app.maxLogoSize)
                {
                    abp.message.warn(app.localize('File_SizeLimit_Error'));
                    return false;
                }

                return true;
            },
            success: function (response) {
                if (response.success) {
                    refreshLogo(abp.appPath + 'TenantCustomization/GetLogo?id=' + response.result.id);
                    abp.notify.info(app.localize('SavedSuccessfully'));
                } else {
                    abp.message.error(response.error.message);
                }
            }
        });

        $('#SettingsLogoUploadForm button[type=reset]').click(function () {
            _tenantSettingsService.clearLogo().done(function () {
                refreshLogo(abp.appPath + 'Common/Images/app-logo-dump-truck-130x35.gif');
                abp.notify.info(app.localize('ClearedSuccessfully'));
            });
        });

        //Appearance/ReportsLogo
        $('#SettingsReportsLogoUploadForm').ajaxForm({
            beforeSubmit: function (formData, jqForm, options) {

                var $fileInput = $('#SettingsReportsLogoUploadForm input[name=ReportsLogoImage]');
                var files = $fileInput.get()[0].files;

                if (!files.length) {
                    return false;
                }

                var file = files[0];

                //File type check
                var type = file.type.slice(file.type.lastIndexOf('/') + 1);
                if (!app.allowedReportLogoTypes.includes(type)) {
                    abp.message.warn(app.localize('File_Invalid_Type_Error'));
                    return false;
                }

                //File size check
                if (file.size > app.maxReportLogoSize)
                {
                    abp.message.warn(app.localize('File_SizeLimit_Error'));
                    return false;
                }

                return true;
            },
            success: function (response) {
                if (response.success) {
                    abp.notify.info(app.localize('SavedSuccessfully'));
                } else {
                    abp.message.error(response.error.message);
                }
            }
        });

        $('#SettingsReportsLogoUploadForm button[type=reset]').click(function () {
            _tenantSettingsService.clearReportsLogo().done(function () {
                abp.notify.info(app.localize('ClearedSuccessfully'));
            });
        });

        function refreshLogo(url) {
            $('#AppLogo').attr('src', url);
        }

        //Appearance/Custom CSS
        $('#SettingsCustomCssUploadForm').ajaxForm({
            beforeSubmit: function (formData, jqForm, options) {

                var $fileInput = $('#SettingsCustomCssUploadForm input[name=CustomCssFile]');
                var files = $fileInput.get()[0].files;

                if (!files.length) {
                    return false;
                }

                var file = files[0];

                //File type check
                var type = '|' + file.type.slice(file.type.lastIndexOf('/') + 1) + '|';
                if ('|css|'.indexOf(type) === -1) {
                    abp.message.warn(app.localize('File_Invalid_Type_Error'));
                    return false;
                }

                //File size check
                if (parseInt(file.size) > 1048576) //1MB
                {
                    abp.message.warn(app.localize('File_SizeLimit_Error'));
                    return false;
                }

                return true;
            },
            success: function (response) {
                if (response.success) {
                    refreshCustomCss(abp.appPath + 'TenantCustomization/GetCustomCss?id=' + response.result.id);
                    abp.notify.info(app.localize('SavedSuccessfully'));
                } else {
                    abp.message.error(response.error.message);
                }
            }
        });

        $('#SettingsCustomCssUploadForm button[type=reset]').click(function () {
            _tenantSettingsService.clearCustomCss().done(function () {
                refreshCustomCss(null);
                abp.notify.info(app.localize('ClearedSuccessfully'));
            });
        });

        function refreshCustomCss(url) {
            $('#TenantCustomCss').remove();
            if (url) {
                $('head').append('<link id="TenantCustomCss" href="' + url + '" rel="stylesheet"/>');
            }
        }

        $("#DefaultStartTime").timepickerInit({ stepping: 1 });

        $("#PhoneNumberToSendMessagesFromInfo, #DontValidateDriverAndTruckOnTicketsInfo, .tooltip-icon").tooltip();

        var $allowImportingTruxEarnings = $('#AllowImportingTruxEarnings');
        $allowImportingTruxEarnings.change(function () {
            if ($allowImportingTruxEarnings.is(':checked')) {
                $('#TruxCustomerId').attr('required', 'required').closest('.form-group').show();
            } else {
                $('#TruxCustomerId').removeAttr('required').closest('.form-group').hide();
            }
        });

        var $allowImportingLuckStoneEarnings = $('#AllowImportingLuckStoneEarnings');
        $allowImportingLuckStoneEarnings.change(function () {
            let dependantControls = $('#LuckStoneCustomerId, #HaulerRef');
            if ($allowImportingLuckStoneEarnings.is(':checked')) {
                dependantControls.attr('required', 'required').closest('.form-group').show();
            } else {
                dependantControls.removeAttr('required').closest('.form-group').hide();
            }
        });

        var $allowImportingIronSheepdogEarnings = $('#AllowImportingIronSheepdogEarnings');
        $allowImportingIronSheepdogEarnings.change(function () {
            let dependantControls = $('#IronSheepdogCustomerId');
            if ($allowImportingIronSheepdogEarnings.is(':checked')) {
                dependantControls.attr('required', 'required').closest('.form-group').show();
            } else {
                dependantControls.removeAttr('required').closest('.form-group').hide();
            }
        });

        $('#TruxCustomerId').select2Init({
            abpServiceMethod: abp.services.app.customer.getActiveCustomersSelectList,
            allowClear: false
        });

        $('#LuckStoneCustomerId').select2Init({
            abpServiceMethod: abp.services.app.customer.getActiveCustomersSelectList,
            allowClear: false
        });

        $('#IronSheepdogCustomerId').select2Init({
            abpServiceMethod: abp.services.app.customer.getActiveCustomersSelectList,
            allowClear: false
        });

        //Save settings
        $('#SaveAllSettingsButton').click(function () {
            saveSettings();
        });
        async function saveSettings(successCallback, rejectCallback) {
            if (!$('form#GeneralSettingsForm').valid()) {
                $('form#GeneralSettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }
            if ($('form#GeotabSettingsForm').length && !$('form#GeotabSettingsForm').valid()) {
                $('form#GeotabSettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }
            if ($('form#SamsaraSettingsForm').length && !$('form#SamsaraSettingsForm').valid()) {
                $('form#SamsaraSettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }
            if ($('form#IntelliShiftSettingsForm').length && !$('form#IntelliShiftSettingsForm').valid()) {
                $('form#IntelliShiftSettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }
            if ($('form#DtdTrackerSettingsForm').length && !$('form#DtdTrackerSettingsForm').valid()) {
                $('form#DtdTrackerSettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }
            if ($('form#BillingSettingsForm').length && !$('form#BillingSettingsForm').valid()) {
                $('form#BillingSettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }
            if ($('#Setting_UseShifts').is(':checked') && $('#Setting_ShiftName1, #Setting_ShiftName2, #Setting_ShiftName3').filter(function () { return $(this).val(); }).length === 0) {
                abp.message.error('At least one "Shift name" is required when the "Use Shifts" setting is turned on!');
                rejectCallback && rejectCallback();
                return;
            }
            if ($('form#DispatchingAndMessagingSettingsForm').length && !$('form#DispatchingAndMessagingSettingsForm').valid()) {
                $('form#DispatchingAndMessagingSettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }
            if ($('form#TicketsSettingsForm').length && !$('form#TicketsSettingsForm').valid()) {
                $('form#TicketsSettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }
            if ($('form#LeaseHaulerSettingsForm').length && !$('form#LeaseHaulerSettingsForm').valid()) {
                $('form#LeaseHaulerSettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }
            if ($('form#TimeAndPaySettingsForm').length && !$('form#TimeAndPaySettingsForm').valid()) {
                $('form#TimeAndPaySettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }
            $('form#TruxSettingsForm').validate();
            if ($('form#TruxSettingsForm').length && !$('form#TruxSettingsForm').valid()) {
                $('form#TruxSettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }
            $('form#LuckStoneSettingsForm').validate();
            if ($('form#LuckStoneSettingsForm').length && !$('form#LuckStoneSettingsForm').valid()) {
                $('form#LuckStoneSettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }
            $('form#IronSheepdogSettingsForm').validate();
            if ($('form#IronSheepdogSettingsForm').length && !$('form#IronSheepdogSettingsForm').valid()) {
                $('form#IronSheepdogSettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }
            if ($('#FuelSettingsForm').length && !$('#FuelSettingsForm').valid()) {
                $('#FuelSettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }
            if ($('#HaulZoneSettingsForm').length && !$('#HaulZoneSettingsForm').valid()) {
                $('#HaulZoneSettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }
            if ($('#QuoteSettingsForm').length && !$('#QuoteSettingsForm').valid()) {
                $('#QuoteSettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }
            if ($('#JobSettingsForm').length && !$('#JobSettingsForm').valid()) {
                $('#JobSettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }
            if ($('#EmailTemplateSettingsForm').length && !$('#EmailTemplateSettingsForm').valid()) {
                $('#EmailTemplateSettingsForm').showValidateMessage();
                rejectCallback && rejectCallback();
                return;
            }

            //even though ItemIdToUseForFuelSurchargeOnInvoice is always visible, we should only require the value when ShowFuelSurcharge checkbox is checked
            if ($('#ShowFuelSurcharge').is(':checked') && !$("#ItemIdToUseForFuelSurchargeOnInvoice").val()) {
                abp.helper.formatAndShowValidationMessage('"' + app.localize('ItemToUseForFuelSurchargeOnInvoice') + '" - This field is required.');
                return;
            }

            var $passwordCheckCtrl = $('#Setting_PasswordComplexity_UseDefaultSettings');
            var $requiredLengthCtrl = $('#RequiredLength');
            var $settingUselockoutCheckCtrl = $('#Setting_UserLockOut_IsEnabled');
            var $maxFailedAccessCtrl = $('#MaxFailedAccessAttemptsBeforeLockout');
            var $defaultAccountLockoutCtrl = $('#DefaultAccountLockoutSeconds');

            if ($passwordCheckCtrl.is(":not(:checked)")) {
                $requiredLengthCtrl.attr('required', 'required');
                $("#PasswordComplexitySettingsForm").validate({ ignore: '' });
                if (!$('#PasswordComplexitySettingsForm').valid()) {
                    $('#PasswordComplexitySettingsForm').showValidateMessage();
                    rejectCallback && rejectCallback();
                    return;
                }
            }
            else {
                var $requiredLengthPasswordCompexityCtrl = $('#PasswordComplexitySettingsForm [name=RequiredLength]');
                $requiredLengthPasswordCompexityCtrl.removeAttr('required');
            }

            if ($settingUselockoutCheckCtrl.is(':checked')) {
                $maxFailedAccessCtrl.attr('required', 'required');
                $defaultAccountLockoutCtrl.attr('required', 'required');
                if (!$('#UserLockOutSettingsForm').valid()) {
                    rejectCallback && rejectCallback();
                    return;
                }
            }
            else {
                $maxFailedAccessCtrl.removeAttr('required');
                $defaultAccountLockoutCtrl.removeAttr('required');
            }

            var gpsIntegration = $('form#GpsIntegrationSettingsForm').length ? $('#GpsIntegrationSettingsForm').serializeFormToObject() : {};
            gpsIntegration.geotab = $('form#GeotabSettingsForm').length ? $('#GeotabSettingsForm').serializeFormToObject() : {};
            gpsIntegration.samsara = $('form#SamsaraSettingsForm').length ? $('#SamsaraSettingsForm').serializeFormToObject() : {};
            gpsIntegration.intellishift = $('form#IntelliShiftSettingsForm').length ? $('#IntelliShiftSettingsForm').serializeFormToObject() : {};
            gpsIntegration.dtdTracker = $('form#DtdTrackerSettingsForm').length ? $('#DtdTrackerSettingsForm').serializeFormToObject() : {};

            var integration = {
                gps: gpsIntegration,
                fulcrum: $('form#FulcrumSettingsForm').length ? $('#FulcrumSettingsForm').serializeFormToObject() : {},
            };

            _tenantSettingsService.updateAllSettings({
                general: $('#GeneralSettingsForm').serializeFormToObject(),
                userManagement: $.extend($('#FormBasedRegistrationSettingsForm').serializeFormToObject(), $('#OtherSettingsForm').serializeFormToObject()),
                email: $('#EmailSmtpSettingsForm').serializeFormToObject(),
                billing: $('#BillingSettingsForm').serializeFormToObject(),
                sms: $('#SmsSettingsForm').serializeFormToObject(),
                integration: integration,
                payment: $('#PaymentSettingsForm').serializeFormToObject(),
                timeAndPay: $('#TimeAndPaySettingsForm').serializeFormToObject(),
                trux: $('#TruxSettingsForm').serializeFormToObject(),
                luckStone: $('#LuckStoneSettingsForm').serializeFormToObject(),
                ironSheepdog: $('#IronSheepdogSettingsForm').serializeFormToObject(),
                fuel: $('#FuelSettingsForm').serializeFormToObject(),
                haulZone: $('#HaulZoneSettingsForm').serializeFormToObject(),
                quote: $('#QuoteSettingsForm').serializeFormToObject(),
                job: $('#JobSettingsForm').serializeFormToObject(),
                emailTemplate: $('#EmailTemplateSettingsForm').serializeFormToObject(),
                dispatchingAndMessaging: $('form#DispatchingAndMessagingSettingsForm').length ? $('#DispatchingAndMessagingSettingsForm').serializeFormToObject() : {},
                tickets: $('form#TicketsSettingsForm').length ? $('form#TicketsSettingsForm').serializeFormToObject() : {},
                leaseHaulers: $('form#LeaseHaulerSettingsForm').length ? $('form#LeaseHaulerSettingsForm').serializeFormToObject() : {},
                security: {
                    useDefaultPasswordComplexitySettings: $('#Setting_PasswordComplexity_UseDefaultSettings').is(":checked"),
                    passwordComplexity: $('#PasswordComplexitySettingsForm').serializeFormToObject(),
                    defaultPasswordComplexity: getDefaultPasswordComplexitySettings(),
                    userLockOut: $('#UserLockOutSettingsForm').serializeFormToObject(),
                    twoFactorLogin: $('#TwoFactorLoginSettingsForm').serializeFormToObject()
                }
            }).done(function () {
                abp.notify.info(app.localize('SavedSuccessfully'));

                refreshTestPhoneNumberButton();

                var newTimezone = $('#GeneralSettingsForm [name=Timezone]').val();
                if (abp.clock.provider.supportsMultipleTimezone
                    && _usingDefaultTimeZone
                    && _initialTimeZone !== newTimezone
                ) {
                    abp.message.info(app.localize('TimeZoneSettingChangedRefreshPageNotification')).done(function () {
                        if (!successCallback) {
                            window.location.reload();
                        } else {
                            successCallback(true); //requireReload
                        }
                    });
                } else {
                    successCallback && successCallback();
                }
            }).fail(function () {
                rejectCallback && rejectCallback();
            });
        }

        refreshTestPhoneNumberButton();
        function refreshTestPhoneNumberButton() {
            if ($('#SmsPhoneNumber').val()) {
                $('#TestPhoneNumberButton').removeAttr('disabled');
            } else {
                $('#TestPhoneNumberButton').attr('disabled', 'disabled');
            }
        }
        $('#SmsPhoneNumber').on('blur', function () {
            refreshTestPhoneNumberButton();
        });

        $('#SendTestEmailButton').click(function () {
            _tenantSettingsService.sendTestEmail({
                emailAddress: $('#TestEmailAddressInput').val()
            }).done(function () {
                abp.notify.info(app.localize('TestEmailSentSuccessfully'));
            });
        });

        abp.maps.waitForGoogleMaps().then(function () {
            var addressField = $("#DefaultMapLocationAddress");
            var coordsField = $("#DefaultMapLocation");
            var addressAutocomplete = new google.maps.places.Autocomplete(addressField[0], {
                fields: ["formatted_address", "geometry"],
                types: ["address"],
            });

            addressAutocomplete.addListener("place_changed", () => {
                let place = addressAutocomplete.getPlace();
                addressField.val(place.formatted_address);
                if (!place.geometry || !place.geometry.location) {
                    coordsField.val('');
                } else {
                    latLng = place.geometry.location;
                    coordsField.val(latLng.toUrlValue());
                }
            });

            addressField.change(function () {
                if (addressField.val() === '') {
                    coordsField.val('');
                }
            });
        });

        //Toggle User defined field 1
        var _$userDefinedField = $('#Setting_UserDefinedField1');
        _$userDefinedField.on('change', function () {
            _$userDefinedField.val($.trim(_$userDefinedField.val()));
        });

        function toggleUserDefinedField1() {
            if ($('#Setting_UserDefinedField1Checkbox').is(':checked')) {
                _$userDefinedField.show();
                _$userDefinedField.attr('required', 'required');
            } else {
                _$userDefinedField.hide();
                _$userDefinedField.removeAttr('required');
                _$userDefinedField.val('');
            }
        }

        $('#Setting_UserDefinedField1Checkbox').change(function () {
            toggleUserDefinedField1();
        });

        toggleUserDefinedField1();

        //Toggle Tax fields
        var $taxCalculationType = $("#TaxCalculationType");
        $taxCalculationType.select2Init({
            showAll: true,
            allowClear: false
        });

        var $autopopulateDefaultTaxrate = $("#Setting_AutopopulateDefaultTaxRateCheckbox");
        var $defaultTaxRate = $("#Setting_DefaultTaxRate");

        function toggleTaxFields() {
            if ($taxCalculationType.val() === abp.enums.taxCalculationType.noCalculation.toString()) {
                $autopopulateDefaultTaxrate.prop('checked', false);
                $defaultTaxRate.val("0");
                $("#AutopopulateCheckboxBlock").hide();
            } else {
                $("#AutopopulateCheckboxBlock").show();
                if ($autopopulateDefaultTaxrate.is(":checked")) {
                    $("#DefaultTaxRateBlock").show();
                } else {
                    $("#DefaultTaxRateBlock").hide();
                }
            }
        }
        toggleTaxFields();

        $taxCalculationType.change(toggleTaxFields);
        $autopopulateDefaultTaxrate.change(toggleTaxFields);

        //Toggle Shifts
        var _$shiftFields = $('#Setting_ShiftName1, label[for="Setting_ShiftName1"], #Setting_ShiftName2, label[for="Setting_ShiftName2"], #Setting_ShiftName3, label[for="Setting_ShiftName3"]');

        function toggleShifts() {
            if ($('#Setting_UseShifts').is(':checked')) {
                _$shiftFields.show();
            } else {
                _$shiftFields.hide();
                _$shiftFields.val('');
            }
        }

        $('#Setting_UseShifts').change(function () {
            toggleShifts();
        });

        toggleShifts();

        function disableSubmitButton($form) {
            $form.find('button:submit').attr('disabled', true);
        }
        disableSubmitButton($('form#SettingsLogoUploadForm'));
        disableSubmitButton($('form#SettingsCustomCssUploadForm'));
        disableSubmitButton($('form#SettingsReportsLogoUploadForm'));
        $('form#SettingsLogoUploadForm, form#SettingsCustomCssUploadForm, form#SettingsReportsLogoUploadForm').on('reset', function () {
            disableSubmitButton($(this));
        });
        function updateSubmitButtonState(inputFile) {
            var $form = $(inputFile).closest('form');
            if ($(inputFile).val()) {
                $form.find('button:submit').removeAttr('disabled');
            }
            else {
                $form.find('button:submit').attr('disabled', true);
            }
        }

        (function () {
            let logoInput = document.getElementById("ApplicationLogoImage");
            if (!logoInput) {
                return;
            }
            logoInput.onchange = function () {
                var filename = $(this).val().split('\\').pop();
                document.getElementById("uploadApplicationLogoFile").value = filename;
                updateSubmitButtonState(this);
            };
        })();

        (function () {
            document.getElementById("ReportsLogoImage").onchange = function () {
                var filename = $(this).val().split('\\').pop();
                document.getElementById("uploadReportsLogoFile").value = filename;
                updateSubmitButtonState(this);
            };
        })();

        (function () {
            let logoInput = document.getElementById("CustomCssFile");
            if (!logoInput) {
                return;
            }
            logoInput.onchange = function () {
                var filename = $(this).val().split('\\').pop();
                document.getElementById("uploadCustomCssFile").value = filename;
                updateSubmitButtonState(this);
            };
        })();


        // Dispatching & Messaging
        $('#TestPhoneNumberButton, #SendTestMessageButton').click(function () {
            $('#SaveAllSettingsButton').click();
            _testSmsNumberModal.open({ testPurpose: $(this).data('test') });
        });

        var $driverDispatchSms = $('#DriverDispatchSms');
        $('input[name="DispatchVia"], #AllowSmsMessages').change(refereshDispatchViaControls);
        refereshDispatchViaControls();
        function refereshDispatchViaControls() {
            var $parentDiv = $driverDispatchSms.parent('div.form-group');
            var dispatchVia = parseInt($('input[name="DispatchVia"]:checked').val());
            if (dispatchVia !== abp.enums.dispatchVia.driverApplication) {
                $('input[name="SendSmsOnDispatching"][value="1"]').prop('checked', true);
            }
            if (dispatchVia === abp.enums.dispatchVia.simplifiedSms || $('#AllowSmsMessages').is(':checked')) {
                $driverDispatchSms.attr('required', 'required');
                $parentDiv.show();
                $('#SendTestMessageButton').show();
            } else {
                $parentDiv.hide();
                $('#SendTestMessageButton').hide();
                $driverDispatchSms.removeAttr('required').removeAttr('aria-required');
            }
        }
        $('#DriverDispatchSms').on('blur', function () {
            if (!$driverDispatchSms.val()) {
                _tenantSettingsService.getDefaultDriverDispatchSmsTemplate().done(function (defaultTemplate) {
                    $driverDispatchSms.val(defaultTemplate);
                });
            }
        });

        $('#HideTicketControlsInDriverApp').change(refreshDriverAppControls);
        refreshDriverAppControls();
        function refreshDriverAppControls() {
            if ($('#HideTicketControlsInDriverApp').is(':checked')) {
                $(`input[name="RequiredTicketEntry"][value="${abp.enums.requiredTicketEntry.none}"]`).prop('checked', true).closest('.form-group').hide();
                $('#RequireSignature').prop('checked', false).closest('.form-group').hide();
                $('#RequireTicketPhoto').prop('checked', false).closest('.form-group').hide();
                $('#AllowEditingTimeOnHourlyJobs').prop('checked', false).closest('.form-group').hide();
            } else {
                $(`input[name="RequiredTicketEntry"][value="${abp.enums.requiredTicketEntry.none}"]`).closest('.form-group').show();
                $('#RequireSignature').closest('.form-group').show();
                $('#RequireTicketPhoto').closest('.form-group').show();
                $('#AllowEditingTimeOnHourlyJobs').closest('.form-group').show();
            }
        }

        $('#AutoGenerateTicketNumbers').change(refreshDisableTicketNumberCheckboxVisibility);
        refreshDisableTicketNumberCheckboxVisibility();
        function refreshDisableTicketNumberCheckboxVisibility() {
            if ($('#AutoGenerateTicketNumbers').is(':checked')) {
                $('#DisableTicketNumberOnDriverApp').closest('.form-group').show();
            } else {
                $('#DisableTicketNumberOnDriverApp').prop('checked', false).closest('.form-group').hide();
            }
        }

        $('#DefaultLoadAtLocationIdForTenant').select2Init({
            abpServiceMethod: listCacheSelectLists.location(),
            showAll: false,
            allowClear: true
        });

        //#9256
        $('#DispatchingAndMessagingSettingsForm input:radio').change(refreshAdditionalSettings);
        refreshAdditionalSettings();
        function refreshAdditionalSettings() {
            if ($('#radioDriverApplication').is(':checked'))
                $('#AdditionalSettings').show();
            else
                $('#AdditionalSettings').hide();
        }

        $('#InvoiceTemplate').select2Init({
            showAll: true,
            allowClear: false
        });

        $('#QuickbooksIntegrationKind').change(refreshQuickbooksControls);
        $('#QuickbooksIntegrationKind').select2Init({
            showAll: true,
            allowClear: false
        });

        $("#QbdDefaultIncomeAccountType").select2Init({
            showAll: true,
            allowClear: false
        });

        refreshQuickbooksControls();
        function refreshQuickbooksControls() {
            let quickbooksIntegrationKind = parseInt($('#QuickbooksIntegrationKind').val() || '0');
            $('#quickbooksConnectBlock, #quickbooksConnectedBlock, #quickbooksDesktopBlock, #quickbooksTransactionProBlock, #quickbooksGeneralBlock').hide();
            switch (quickbooksIntegrationKind) {
                case abp.enums.quickbooksIntegrationKind.desktop:
                    $('#quickbooksDesktopBlock, #quickbooksGeneralBlock').show();
                    break;
                /*
                case abp.enums.quickbooksIntegrationKind.online:
                    $('#quickbooksGeneralBlock').show();
                    if ($('#IsQuickbooksConnected').val() === 'True') {
                        $('#quickbooksConnectedBlock').show();
                    } else {
                        $('#quickbooksConnectBlock').show();
                    }
                    break;
                */
                case abp.enums.quickbooksIntegrationKind.qboExport:
                    $('#quickbooksGeneralBlock').show();
                    break;
                case abp.enums.quickbooksIntegrationKind.transactionProExport:
                    $('#quickbooksTransactionProBlock, #quickbooksGeneralBlock').show();
                    break;
                case abp.enums.quickbooksIntegrationKind.none:
                default:
                    break;
            }
        }

        /*
        $('#QuickbooksConnectButton').click(function (e) {
            e.preventDefault();
            let button = $('#QuickbooksConnectButton');
            abp.ui.setBusy(button);
            saveSettings(function (reloadRequired) {
                _quickbooksOnlineService.getInitiateAuthUrl().done(function (url) {
                    window.location = url;
                }).fail(function () {
                    abp.ui.clearBusy(button);
                });
            }, function () {
                abp.ui.clearBusy(button);
            });
        });

        $('#QuickbooksVerifyConnectionButton').click(function (e) {
            e.preventDefault();
            let button = $(this);
            abp.ui.setBusy(button);
            _quickbooksOnlineService.getInitiateAuthUrl().done(function () {
                abp.message.success("Verified successfully");
            }).always(function () {
                abp.ui.clearBusy(button);
            });
        });

        $('#QuickbooksDisconnectButton').click(function (e) {
            e.preventDefault();
            let button = $(this);
            abp.ui.setBusy(button);
            saveSettings(function (reloadRequired) {
                _quickbooksOnlineService.revokeToken().done(function () {
                    abp.notify.success("Disconnected successfully");
                    window.location.reload();
                }).fail(function () {
                    abp.ui.clearBusy(button);
                });
            }, function () {
                abp.ui.clearBusy(button);
            });
        });

        $('#QuickbooksOnlineSettingsButton').click(function (e) {
            e.preventDefault();
            let button = $('#QuickbooksOnlineSettingsButton');
            abp.ui.setBusy(button);
            saveSettings(function (reloadRequired) {
                window.location = abp.appPath + 'app/settings/quickbooksonline';
            }, function () {
                abp.ui.clearBusy(button);
            });
        });
        */

        $("#Platform").select2Init({
            showAll: true,
            allowClear: false
        });

        $('#Platform').change(function () {
            $('#SettingsDtdTrackerTab').hide();
            $('#SettingsGeotabTab').hide();
            $('#SettingsSamsaraTab').hide();
            $('#SettingsIntelliShiftTab').hide();

            $("#MapBaseUrl").removeAttr('required');
            $('label[for="MapBaseUrl"]').removeClass('required-label');

            if (Number($("#Platform").val()) === abp.enums.gpsPlatform.dtdTracker) {
                $('#SettingsDtdTrackerTab').show();
            }
            else if (Number($("#Platform").val()) === abp.enums.gpsPlatform.geotab) {
                $('#SettingsGeotabTab').show();
                $("#MapBaseUrl").prop('required', true);
                $('label[for="MapBaseUrl"]').addClass('required-label');
            }
            else if (Number($("#Platform").val()) === abp.enums.gpsPlatform.samsara) {
                $('#SettingsSamsaraTab').show();
            }
            else if (Number($("#Platform").val()) === abp.enums.gpsPlatform.intelliShift) {
                $('#SettingsIntelliShiftTab').show();
            }
        });

        $("#LinkDtdTrackerAccountButton").click(function (e) {
            e.preventDefault();
            let button = $(this);
            abp.ui.setBusy(button);
            saveSettings(function (reloadRequired) {
                window.location = abp.appPath + 'app/settings/LinkDtdTrackerAccount';
            }, function () {
                abp.ui.clearBusy(button);
            });
        });

        $('#LoggingLevel').select2Init({
            showAll: true,
            allowClear: false
        });
    });
})();
