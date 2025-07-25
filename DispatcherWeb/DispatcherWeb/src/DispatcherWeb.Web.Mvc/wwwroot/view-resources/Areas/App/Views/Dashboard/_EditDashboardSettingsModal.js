(function () {
    app.modals.EditDashboardSettingsModal = function () {

        var _modalManager;
        var _dashboardService = abp.services.app.dashboard;

        this.init = function (modalManager) {
            _modalManager = modalManager;
        };

        this.save = function () {
            let enabledSettingNames = $(".dashboard-setting-checkbox:checked").map((i, x) => $(x).attr('name')).get();

            _modalManager.setBusy(true);
            _dashboardService.saveDashboardSettings(enabledSettingNames).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.dasboardSettingsModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };

    };
})();