(function ($) {
    app.modals.CreateOrEditLeaseHaulerDriverModal = function () {

        var _modalManager;
        var _modal;
        var _driverService = abp.services.app.driver;
        var _leaseHaulerService = abp.services.app.leaseHauler;
        var _passwordComplexityHelper = new app.PasswordComplexityHelper();
        var _$form = null;
        var _wasActive = null;
        var _isDriverReadonly = false;
        var _dispatchViaDriverApp = abp.setting.getInt('App.DispatchingAndMessaging.DispatchVia') === abp.enums.dispatchVia.driverApplication;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modal = _modalManager.getModal();

            _$form = _modal.find('form');

            if (_$form.find('#HaulingCompanyDriverId').val()) {
                _isDriverReadonly = true;
                _modal.find('.modal-footer .save-button').hide();
                _modal.find('.modal-footer .close-button').text('Close');
                _modal.find('input,select,textarea').prop('disabled', true);
            }

            _$form.validate();
            $.validator.addMethod(
                "regex",
                function (value, element, regexp) {
                    var re = new RegExp(regexp, 'i');
                    return this.optional(element) || re.test(value);
                },
                "Please check your input."
            );
            _$form.find('#CellPhoneNumber').rules('add', { regex: app.regex.cellPhoneNumber });
            var emailInput = _$form.find('#EmailAddress');
            emailInput.rules('add', { regex: app.regex.email });

            var passwordInputs = _modalManager.getModal().find('input[name=Password],input[name=PasswordRepeat]');
            var passwordInputGroups = passwordInputs.closest('.form-group');

            _passwordComplexityHelper.setPasswordComplexityRules(passwordInputs);

            _$form.find('#SetRandomPassword').change(function () {
                let userId = _$form.find("#UserId").val();
                if ($(this).is(':checked')) {
                    passwordInputGroups.slideUp('fast');
                    if (!userId) {
                        passwordInputs.removeAttr('required');
                        passwordInputs.closest('.form-group').find('label').removeClass('required-label');
                    }
                } else {
                    passwordInputGroups.slideDown('fast');
                    if (!userId) {
                        passwordInputs.attr('required', 'required');
                        passwordInputs.closest('.form-group').find('label').addClass('required-label');
                    }
                }
            }).change();

            emailInput.change(function () {
                if (!emailInput.val()) {
                    $("#EnableForDriverApplication").prop('checked', false).change().closest('.form-group').hide();
                } else {
                    $("#EnableForDriverApplication").closest('.form-group').show();
                }
            });

            _wasActive = getIsActiveValue();

            setRequiredAttributesAccordingToPreferredFormat();

            _$form.find("#OrderNotifyPreferredFormat").select2Init({
                showAll: true,
                allowClear: false
            });

            _$form.find("#OfficeId").select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: false
            });

            _$form.find('#EnableForDriverApplication').change(function () {
                setRequiredAttributesAccordingToPreferredFormat();
            });

            function setRequiredAttributesAccordingToPreferredFormat() {
                var enableForDriverApplicaiton = _$form.find('#EnableForDriverApplication').is(':checked');

                var $emailAddress = _$form.find('#EmailAddress');
                var $userDetails = _$form.find('#UserDetails');
                setRequired($emailAddress, enableForDriverApplicaiton);
                $userDetails.toggle(enableForDriverApplicaiton);
            }

            function setRequired($ctrl, isRequired) {
                $ctrl.attr('required', isRequired ? 'required' : null);
                $ctrl.closest('.form-group').find('label').toggleClass('required-label', isRequired);
                if (!isRequired) {
                    $ctrl.removeAttr('aria-required');
                    $ctrl.closest('.form-group').removeClass('has-error');
                }
            }
        };




        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var driver = _$form.serializeFormToObject();
            var isActive = getIsActiveValue();
            driver.DriverIsActive = isActive;
            driver.EnableForDriverApplication = _$form.find('#EnableForDriverApplication').is(':checked');
            driver.SetRandomPassword = _$form.find('#SetRandomPassword').is(':checked');
            driver.ShouldChangePasswordOnNextLogin = _$form.find('#ShouldChangePasswordOnNextLogin').is(':checked');
            driver.SendActivationEmail = _$form.find('#SendActivationEmail').is(':checked');

            if (driver.SetRandomPassword === "true") {
                driver.Password = null;
            }
            try {
                _modalManager.setBusy(true);

                if (isActive && _dispatchViaDriverApp) {
                    if (driver.EnableForDriverApplication) {
                        var thereAreActiveDriversWithSameEmail = await _driverService.thereAreActiveDriversWithSameEmail({
                            email: driver.EmailAddress,
                            exceptDriverId: driver.Id
                        });
                        if (thereAreActiveDriversWithSameEmail) {
                            _modalManager.setBusy(false);
                            if (!await abp.message.confirm(
                                app.localize('ThereAreActiveDriversWithSameEmailPrompt')
                            )) {
                                return;
                            }
                            _modalManager.setBusy(true);
                        }
                    }
                }

                _modalManager.setBusy(true);
                await _leaseHaulerService.editLeaseHaulerDriver(driver);

                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditLeaseHaulerDriverModalSaved');
            }
            finally {
                _modalManager.setBusy(false);
            }
        };

        function getIsActiveValue() {
            return _$form.find("#DriverIsActive").is(":checked");
        }
    };
})(jQuery);
