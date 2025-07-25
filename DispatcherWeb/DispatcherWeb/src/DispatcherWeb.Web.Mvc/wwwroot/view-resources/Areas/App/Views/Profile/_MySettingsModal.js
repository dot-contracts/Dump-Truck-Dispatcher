(function () {
    app.modals.MySettingsModal = function () {

        var _profileService = abp.services.app.profile;
        var _initialTimezone = null;
        var _initialOptionsFormValues = null;

        var _modalManager;
        var _$form = null;
        var _$optionsForm = null;
        var _features = {
            separateItems: abp.features.isEnabled('App.SeparateMaterialAndFreightItems'),
        };

        this.init = function (modalManager) {
            _modalManager = modalManager;
            var $modal = _modalManager.getModal();

            _$form = $modal.find('form[name=MySettingsModalForm]');
            _$form.validate();
            _$optionsForm = $modal.find('form[name=OptionsModalForm]');

            _initialTimezone = _$form.find("[name='Timezone']").val();
            _initialOptionsFormValues = getOptionsFormValues();

            $.validator.addMethod(
                "regex",
                function (value, element, regexp) {
                    var re = new RegExp(regexp, 'i');
                    return this.optional(element) || re.test(value);
                },
                "Please check your input."
            );

            _$form.find('#PhoneNumber').rules('add', { regex: app.regex.cellPhoneNumber });

            var $btnEnableGoogleAuthenticator = $modal.find('#btnEnableGoogleAuthenticator');

            $btnEnableGoogleAuthenticator.click(function () {
                _profileService.updateGoogleAuthenticatorKey()
                    .done(function (result) {
                        $modal.find('.google-authenticator-enable').toggle();
                        $modal.find('img').attr('src', result.qrCodeSetupImageUrl);
                    }).always(function () {
                        _modalManager.setBusy(false);
                    });
            });

            var $SmsVerification = $modal.find('#btnSmsVerification');
            var smsVerificationModal = new app.ModalManager({
                viewUrl: abp.appPath + 'App/Profile/SmsVerificationModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Profile/_SmsVerificationModal.js',
                modalClass: 'SmsVerificationModal'
            });

            $SmsVerification.click(function () {
                _profileService.sendVerificationSms()
                    .done(function () {
                        smsVerificationModal.open({}, function () {
                            $('#SpanSmsVerificationVerified').show();
                            $('#SpanSmsVerificationUnverified').hide();
                            _$form.find(".tooltips").tooltip();
                        });
                    });
            });

            _$form.find(".tooltips").tooltip();

            $('#AllowCounterSalesForUser').change(refreshCounterSalesControls);
            refreshCounterSalesControls();
            function refreshCounterSalesControls() {
                if ($('#AllowCounterSalesForUser').is(':checked')) {
                    $('#DefaultDesignationToMaterialOnlyForUser').closest('.mb-1').show();
                    $('#DefaultLoadAtLocationIdForUser').closest('.form-group').show();
                    $('#DefaultMaterialItemId').closest('.form-group').show();
                    $('#DefaultMaterialUomId').closest('.form-group').show();
                    $('#DefaultAutoGenerateTicketNumberForUser').closest('.mb-1').show();
                } else {
                    $('#DefaultDesignationToMaterialOnlyForUser').prop('checked', false).closest('.mb-1').hide();
                    $('#DefaultLoadAtLocationIdForUser').val('').change().closest('.form-group').hide();
                    $('#DefaultMaterialItemId').val('').change().closest('.form-group').hide();
                    $('#DefaultMaterialUomId').val('').change().closest('.form-group').hide();
                    $('#DefaultAutoGenerateTicketNumberForUser').prop('checked', false).closest('.mb-1').hide();
                }
            }

            _$optionsForm.find('#DefaultLoadAtLocationIdForUser').select2Init({
                abpServiceMethod: listCacheSelectLists.location(),
                showAll: false,
                allowClear: true
            });

            _$optionsForm.find('#DefaultMaterialItemId').select2Init({
                abpServiceMethod: listCacheSelectLists.item(),
                abpServiceParamsGetter: (params) => ({
                    types: _features.separateItems ? abp.enums.itemTypes.material : null,
                }),
                showAll: listCache.item.isEnabled,
                allowClear: true
            });

            _$optionsForm.find('#DefaultMaterialUomId').select2Uom({
                showAll: true,
                allowClear: true
            });
        };

        function getOptionsFormValues() {
            var options = _$optionsForm.serializeFormToObject();
            var hostEmailPreferenceCheckboxes = _$optionsForm.find(".HostEmailPreferenceCheckbox:checked");
            var hostEmailPreference = hostEmailPreferenceCheckboxes.map((_, x) => Number($(x).val())).toArray().reduce((a, b) => a | b, 0);
            options.HostEmailPreference = hostEmailPreference;
            return options;
        }

        this.save = function () {
            if (!_$form.valid()) {
                return;
            }

            var profile = _$form.serializeFormToObject();
            profile.Options = getOptionsFormValues();

            _modalManager.setBusy(true);
            _profileService.updateCurrentUserProfile(profile)
                .done(function () {
                    $('#HeaderCurrentUserName').text(profile.UserName);
                    abp.notify.info(app.localize('SavedSuccessfully'));
                    _modalManager.close();

                    var newTimezone = _$form.find("[name='Timezone']").val();

                    if (abp.clock.provider.supportsMultipleTimezone && _initialTimezone !== newTimezone) {
                        abp.message.info(app.localize('TimeZoneSettingChangedRefreshPageNotification')).done(function () {
                            window.location.reload();
                        });
                    }

                    if (JSON.stringify(profile.Options) !== JSON.stringify(_initialOptionsFormValues)) {
                        abp.message.info(app.localize('OptionsChangedRefreshPageNotification')).done(function () {
                            window.location.reload();
                        });
                    }

                }).always(function () {
                    _modalManager.setBusy(false);
                });
        };
    };
})();
