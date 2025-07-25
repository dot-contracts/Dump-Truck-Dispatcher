(function () {
    app.modals.NotificationSettingsModal = function () {

        var _notificationAppService = abp.services.app.notification;

        var _modalManager;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form[name=NotificationSettingsForm]');
            _$form.validate();

            _$form.find('#NotificationSettigs_ReceiveNotifications').on('change', function () {
                var receiveNotifications = $(this).is(":checked");
                var notificationCount = _$form.find("div.notification").length;

                if (notificationCount) {
                    _$form.find('.notification-types-header').removeClass('d-none');
                } else {
                    _$form.find('.notification-types-header').addClass('d-none');
                }

                if (notificationCount && !receiveNotifications) {
                    _$form.find('.disable-info').removeClass('d-none');
                } else {
                    _$form.find('.disable-info').addClass('d-none');
                }

                if (!receiveNotifications) {
                    _$form.find('div.notification input[type=checkbox]').attr('disabled', 'disabled');
                } else {
                    _$form.find('div.notification input[type=checkbox]').removeAttr('disabled');
                }
            });
        };

        function _findNotificationSubscriptions() {
            var notifications = [];
            $.each(_$form.find('.notification input[type=checkbox]'), function (index, item) {
                notifications.push({
                    name: $(item).attr("id").replace('Notification_', ''),
                    isSubscribed: $(item).is(":checked")
                });
            });
            return notifications;
        }

        this.save = function () {
            if (!_$form.valid()) {
                return;
            }

            var notificationSettings = _$form.serializeFormToObject();
            notificationSettings.receiveNotifications = _$form.find('#NotificationSettigs_ReceiveNotifications')[0].checked;
            notificationSettings.notifications = _findNotificationSubscriptions();

            _modalManager.setBusy(true);
            _notificationAppService.updateNotificationSettings(notificationSettings)
                .done(function () {
                    abp.notify.info(app.localize('SavedSuccessfully'));
                    _modalManager.close();
                }).always(function () {
                    _modalManager.setBusy(false);
                });
        };
    };
})();