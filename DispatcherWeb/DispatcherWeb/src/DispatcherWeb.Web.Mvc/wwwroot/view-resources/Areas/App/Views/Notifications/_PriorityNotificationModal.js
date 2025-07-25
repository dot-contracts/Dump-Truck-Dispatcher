(function ($) {
    app.modals.PriorityNotificationModal = function () {

        var _modalManager;
        var _notificationAppServive = abp.services.app.notification;

        function getUiIconColorBySeverity(severity) {
            switch (severity) {
                case abp.notifications.severity.SUCCESS:
                    return 'fa fa-check green';
                case abp.notifications.severity.WARN:
                    return 'fa fa-exclamation-triangle yellow';
                case abp.notifications.severity.ERROR:
                    return 'fa fa-bolt red';
                case abp.notifications.severity.FATAL:
                    return 'fa fa-bomb red';
                case abp.notifications.severity.INFO:
                default:
                    return 'fa fa-info blue';
            }
        }

        this.init = function (modalManager) {
            _modalManager = modalManager;

            var modal = _modalManager.getModal();

            let icon = modal.find('.priority-notification-icon');
            let severity = Number(modal.find('#Severity').val());
            let iconClass = getUiIconColorBySeverity(severity);
            icon.addClass(iconClass);

            var notificationId = modal.find('#Id').val();

            var dismissButton = modal.find('.close-button');
            dismissButton.click(function () {
                _modalManager.setBusy(true);
                _notificationAppServive.setNotificationAsRead({ id: notificationId }).done(function () {
                    abp.notify.info('Dismissed successfully.');
                    _modalManager.close();
                    abp.event.trigger('app.priorityNotificationDismissed');
                }).always(function () {
                    _modalManager.setBusy(false);
                });
            });
        };
    };
})(jQuery);