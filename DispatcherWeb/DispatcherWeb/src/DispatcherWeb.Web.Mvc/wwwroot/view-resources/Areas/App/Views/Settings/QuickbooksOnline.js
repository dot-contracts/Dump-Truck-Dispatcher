/*
(function () {
    $(function () {
        var _quickbooksOnlineService = abp.services.app.quickbooksOnline;

        $('#QboSettingsForm').validate();

        $('#SaveButton').click(function (e) {
            e.preventDefault();
            saveSettings();
        });
        function saveSettings() {
            var form = $('#QboSettingsForm');
            if (!form.valid()) {
                form.showValidateMessage();
                return;
            }

            var qboSettings = form.serializeFormToObject();
            qboSettings.DefaultIncomeAccountName = $('#DefaultIncomeAccountId').getSelectedDropdownOption().text();

            abp.ui.setBusy(form);
            _quickbooksOnlineService.setQuickbooksOnlineSettings(qboSettings).done(function (data) {
                abp.notify.info('Saved successfully.');
                window.location = abp.appPath + 'app/settings';
            }).fail(function () {
                abp.ui.clearBusy(form);
            });
        }

    });
})();
*/