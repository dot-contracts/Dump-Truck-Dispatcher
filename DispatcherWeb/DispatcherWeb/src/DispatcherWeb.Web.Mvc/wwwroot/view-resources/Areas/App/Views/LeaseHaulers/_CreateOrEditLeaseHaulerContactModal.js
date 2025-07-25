(function ($) {
    app.modals.CreateOrEditLeaseHaulerContactModal = function () {

        var _modalManager;
        var _leaseHaulerService = abp.services.app.leaseHauler;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
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
            _$form.find('#Email').rules('add', { regex: app.regex.email });

            setRequiredAttributesAccordingToPreferredFormat();

            var notifyPreferredFormat = _$form.find('#NotifyPreferredFormat');

            notifyPreferredFormat.select2Init({
                showAll: true,
                allowClear: false
            });

            notifyPreferredFormat.change(function () {
                setRequiredAttributesAccordingToPreferredFormat();
            });

            _$form.find('#AllowPortalAccess').change(function () {
                setRequiredAttributesAccordingToPreferredFormat();
            });

            _$form.find('#Email').keyup(function () {
                disableSendInviteButtonIfNeeded();
            });

            _modalManager.getModal().find('#SendInviteButton').click(function (e) {
                e.preventDefault();
                saveLeaseHaulerContactAsync(true);
            });

            function setRequiredAttributesAccordingToPreferredFormat() {
                var preferredFormat = _$form.find('#NotifyPreferredFormat').val();
                var allowPortalAccess = _$form.find('#AllowPortalAccess').is(':checked');
                var sendInviteButton = _$form.find('#SendInviteButton');

                var $emailAddress = _$form.find('#Email');
                if (preferredFormat == 1 /* Email */ || preferredFormat == 3 /* Both */ || allowPortalAccess) {
                    addRequired($emailAddress);
                    sendInviteButton.show();
                    disableSendInviteButtonIfNeeded();
                } else {
                    removeRequired($emailAddress);
                    sendInviteButton.hide();
                }

                var $cellPhoneNumber = _$form.find('#CellPhoneNumber');
                if (preferredFormat == 2 /* SMS */ || preferredFormat == 3 /* Both */) {
                    addRequired($cellPhoneNumber);
                } else {
                    removeRequired($cellPhoneNumber);
                }
            }

            function addRequired($ctrl) {
                $ctrl.attr('required', 'required');
                $ctrl.closest('.form-group').find('label').addClass('required-label');
            }
            function removeRequired($ctrl) {
                $ctrl.removeAttr('required').removeAttr('aria-required');
                $ctrl.closest('.form-group').removeClass('has-error');
                $ctrl.closest('.form-group').find('label').removeClass('required-label');
            }

            function disableSendInviteButtonIfNeeded() {
                var allowPortalAccess = _$form.find('#AllowPortalAccess').is(':checked');
                var emailString = _$form.find('#Email').val();
                var sendInviteButton = _$form.find('#SendInviteButton');

                if (allowPortalAccess && new RegExp(app.regex.email).test(emailString) == true) {
                    sendInviteButton.prop('disabled', false);
                } else {
                    sendInviteButton.prop('disabled', true);
                }
            }

        };

        this.save = async function () {
            await saveLeaseHaulerContactAsync();
        }

        var saveLeaseHaulerContactAsync = async function (sendInviteEmail) {
            if (_$form.find('#AllowPortalAccess').is(':checked') && _$form.find('#Email').val() === '') {
                abp.message.error('An email address is required when giving access to the lease hauler portal.');
                return;
            }

            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var leaseHaulerContact = _$form.serializeFormToObject();
            if (sendInviteEmail) {
                leaseHaulerContact.SendInviteEmail = true;
            }

            if ($('#IsDispatcher').is(':checked') && parseInt(leaseHaulerContact.NotifyPreferredFormat) === 0) {
                abp.message.error('You have checked the "Receives Truck Requests". You must select the "Preferred Format" other than "Neither".', 'Some of the data is invalid');
                return;
            }

            _modalManager.setBusy(true);
            _leaseHaulerService.editLeaseHaulerContact(leaseHaulerContact).done(function (data) {
                if (!sendInviteEmail) {
                    abp.notify.info('Saved successfully.');
                    _modalManager.close();
                } else {
                    abp.notify.info('Sent successfully.');
                }
                leaseHaulerContact.Id = data;
                _$form.find('#Id').val(data);
                abp.event.trigger('app.createOrEditLeaseHaulerContactModalSaved', {
                    item: leaseHaulerContact
                });
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
