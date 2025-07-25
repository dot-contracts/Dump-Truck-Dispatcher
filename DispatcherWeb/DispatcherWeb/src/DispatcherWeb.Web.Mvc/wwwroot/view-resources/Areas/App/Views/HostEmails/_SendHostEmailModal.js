(function ($) {
    app.modals.SendHostEmailModal = function () {

        var _modalManager;
        var _hostEmailService = abp.services.app.hostEmail;
        var _$form = null;
        var _$editionsDropdown = null;
        var _$statusDropdown = null;
        var _$tenantsDropdown = null;
        var _$rolesDropdown = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$editionsDropdown = _$form.find("#EditionIds");
            _$statusDropdown = _$form.find("#ActiveFilter");
            _$tenantsDropdown = _$form.find("#TenantIds");
            _$rolesDropdown = _$form.find("#RoleNames");

            _$editionsDropdown.select2Init({
                abpServiceMethod: abp.services.app.edition.getEditionsSelectList,
                showAll: true,
                allowClear: true
            }).change(() => {
                _$tenantsDropdown.val(null).change();
            });

            _$statusDropdown.select2Init({
                showAll: true,
                allowClear: true
            }).change(() => {
                _$tenantsDropdown.val(null).change();
            });

            _$tenantsDropdown.select2Init({
                abpServiceMethod: abp.services.app.tenant.getTenantsSelectList,
                abpServiceParamsGetter: (params) => ({
                    editionIds: _$editionsDropdown.val(),
                    activeFilter: _$statusDropdown.val()
                }),
                showAll: false,
                allowClear: true
            });

            _$form.find("#Type").select2Init({
                showAll: true,
                allowClear: false
            });

            _$rolesDropdown.select2Init({
                showAll: true,
                allowClear: true
            });

        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var model = _$form.serializeFormWithMultipleToObject();
            model.EditionIds = _$editionsDropdown.val();
            model.ActiveFilter = _$statusDropdown.val();
            model.TenantIds = _$tenantsDropdown.val();
            model.RoleNames = _$rolesDropdown.val();

            _modalManager.setBusy(true);
            _hostEmailService.sendHostEmail(model).done(function () {
                abp.notify.info('Your email was scheduled to be sent.');
                _modalManager.close();
                abp.event.trigger('app.sendHostEmailModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
