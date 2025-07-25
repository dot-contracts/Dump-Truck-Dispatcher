(function () {
    app.modals.CreateOrEditRoleModal = function () {

        var _modalManager;
        var _roleService = abp.services.app.role;
        var _$roleInformationForm = null;
        var _permissionsTree;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _permissionsTree = new PermissionsTree();
            _permissionsTree.init(_modalManager.getModal().find('.permission-tree'));

            _$roleInformationForm = _modalManager.getModal().find('form[name=RoleInformationsForm]');
            _$roleInformationForm.validate({ ignore: "" });

            var $resetPermissionsButton = _modalManager.getModal().find('#ResetPermissions');
            $resetPermissionsButton.on('click', function () {
                resetRolePesmissions();
            });

            _modalManager.getModal().find('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
                if (e.target.text.trim() === "Permissions") {
                    $resetPermissionsButton.show();
                } else {
                    $resetPermissionsButton.hide();
                }
            });
            if (_modalManager.reopened === true) {
                _modalManager.getModal().find('a[href="#PermissionsTab"]').tab('show');
                _modalManager.reopened = false;
            }
        };

        this.save = function () {
            if (!_$roleInformationForm.valid()) {
                _$roleInformationForm.showValidateMessage();
                return;
            }

            var role = _$roleInformationForm.serializeFormToObject();

            _modalManager.setBusy(true);
            _roleService.createOrUpdateRole({
                role: role,
                grantedPermissionNames: _permissionsTree.getSelectedPermissionNames()
            }).done(function () {
                abp.notify.info(app.localize('SavedSuccessfully'));
                _modalManager.close();
                abp.event.trigger('app.createOrEditRoleModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };

        function resetRolePesmissions() {
            _modalManager.setBusy(true);
            _roleService.restoreDefaultPermissions(_modalManager.getArgs().id).done(function () {
                abp.notify.info("Restored successfully");
                _modalManager.getModal().on('hidden.bs.modal', function (e) {
                    _modalManager.reopened = true;
                    _modalManager.reopen();
                });
                _modalManager.close();
            }).always(function () {
                _modalManager.setBusy(false);
            });

        }

    };
})();