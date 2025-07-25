(function ($) {
    app.modals.CreateOrEditUserModal = function () {

        var _userService = abp.services.app.user;

        var _modalManager;
        var _modal;
        var _$userInformationForm = null;
        var _passwordComplexityHelper = new app.PasswordComplexityHelper();
        var _organizationTree;
        const _leaseHaulerPortalRoleNames = [
            'LeaseHaulerAdministrator',
            'LeaseHaulerDispatcher',
        ];
        let _originalLeaseHaulerId = null;
        let _leaseHaulerId = null;
        let _showOffice = abp.session.tenantId && abp.features.isEnabled('App.AllowMultiOfficeFeature');

        const _selectLeaseHaulerModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/LeaseHaulers/SelectLeaseHaulerModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/LeaseHaulers/_SelectLeaseHaulerModal.js',
            modalClass: 'SelectLeaseHaulerModal'
        });

        const changeProfilePictureModal = new app.ModalManager({
            viewUrl: abp.appPath + 'App/Profile/ChangePictureModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Profile/_ChangePictureModal.js',
            modalClass: 'ChangeProfilePictureModal'
        });

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modal = _modalManager.getModal();

            _organizationTree = new OrganizationTree();
            _organizationTree.init(_modal.find('.organization-tree'));

            _$userInformationForm = _modal.find('form[name=UserInformationsForm]');

            _leaseHaulerId = _$userInformationForm.find('#LeaseHaulerId').val();
            _originalLeaseHaulerId = _leaseHaulerId;

            _$userInformationForm.validate();
            $.validator.addMethod(
                "regex",
                function (value, element, regexp) {
                    var re = new RegExp(regexp, 'i');
                    return this.optional(element) || re.test(value);
                },
                "Please check your input."
            );
            _modal.find('#EmailAddress').rules('add', { regex: app.regex.email });

            var passwordInputs = _modal.find('input[name=Password],input[name=PasswordRepeat]');
            var passwordInputGroups = passwordInputs.closest('.form-group');

            _passwordComplexityHelper.setPasswordComplexityRules(passwordInputs);

            _modal.find('#EditUser_SetRandomPassword').change(function () {
                if ($(this).is(':checked')) {
                    passwordInputGroups.slideUp('fast');
                    if (!_modalManager.getArgs().id) {
                        passwordInputs.removeAttr('required');
                    }
                } else {
                    passwordInputGroups.slideDown('fast');
                    if (!_modalManager.getArgs().id) {
                        passwordInputs.attr('required', 'required');
                    }
                }
            });

            if (_showOffice) {
                _$userInformationForm.find("#User_OfficeId").select2Init({
                    abpServiceMethod: listCacheSelectLists.office(),
                    showAll: true,
                    allowClear: false
                });
            }

            _modal
                .find('.user-role-checkbox-list input[type=checkbox]')
                .change(async function () {
                    let roleName = $(this).closest('label').text().trim();
                    if (roleName && _leaseHaulerPortalRoleNames.includes(roleName)) {
                        if (isLeaseHaulerPortalRoleAssigned()) {
                            if (!_leaseHaulerId) {
                                if (_originalLeaseHaulerId) {
                                    _leaseHaulerId = _originalLeaseHaulerId;
                                } else {
                                    try {
                                        await selectLeaseHaulerAsync();
                                    } catch {
                                        $(this).prop('checked', false);
                                    }
                                }
                            }
                        } else {
                            _leaseHaulerId = null;
                        }
                    }

                    _modal.find('#assigned-role-count').text(findAssignedRoleNames().length);
                });

            _modal.find('[data-toggle=tooltip]').tooltip();

            $('#ChangePictureLink').click(function (e) {
                e.preventDefault();
                if (_modalManager.getArgs().id) {
                    changeProfilePictureModal.open({ userId: _modalManager.getArgs().id });
                }
            });

        };
        
        function findAssignedRoleNames() {
            var assignedRoleNames = [];

            _modal
                .find('.user-role-checkbox-list input[type=checkbox]')
                .each(function () {
                    if ($(this).is(':checked')) {
                        assignedRoleNames.push($(this).attr('name'));
                    }
                });

            return assignedRoleNames;
        }

        function isLeaseHaulerPortalRoleAssigned() {
            return findAssignedRoleNames().some(roleName => _leaseHaulerPortalRoleNames.includes(roleName));
        }

        async function selectLeaseHaulerAsync() {
            if (!abp.auth.isGranted('Pages.LeaseHauler')) {
                let error = 'Lease Haulers permission is required to assign a lease hauler to a user.'
                abp.notify.error(error);
                throw new Error(error); //this will revert the checkbox to unchecked or stop the save process, depending on the caller
            }
            let selectedLeaseHauler = await app.getModalResultAsync(
                _selectLeaseHaulerModal.open()
            );
            _leaseHaulerId = selectedLeaseHauler.id;
        }

        this.save = async function () {
            if (!_$userInformationForm.valid()) {
                return;
            }

            var assignedRoleNames = findAssignedRoleNames();
            var user = _$userInformationForm.serializeFormToObject();

            if (isLeaseHaulerPortalRoleAssigned()
                && _leaseHaulerId === null
            ) {
                await selectLeaseHaulerAsync();
            }

            if (user.SetRandomPassword) {
                user.Password = null;
            }

            _modalManager.setBusy(true);
            _userService.createOrUpdateUser({
                user: user,
                assignedRoleNames: assignedRoleNames,
                sendActivationEmail: user.SendActivationEmail,
                SetRandomPassword: user.SetRandomPassword,
                organizationUnits: _organizationTree.getSelectedOrganizations(),
                leaseHaulerId: _leaseHaulerId
            }).done(function () {
                abp.notify.info(app.localize('SavedSuccessfully'));
                _modalManager.close();
                abp.event.trigger('app.createOrEditUserModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
