var app = app || {};
(function ($) {

    app.UserNotificationHelper = (function () {

        return function () {

            /* Message Extracting based on Notification Data Type ********/

            //add your custom notification data types here...

            abp.notifications.messageFormatters['DispatcherWeb.Infrastructure.Notifications.ImportCompletedNotificationData'] = function (importCompletedNotification) {
                return "Import completed. Click to go to Import Results page.";
            };

            /* Example:
            abp.notifications.messageFormatters['DispatcherWeb.MyNotificationDataType'] = function(userNotification) {
                return ...; //format and return message here
            };
            */

            var _notificationService = abp.services.app.notification;
            var _audioNotification = null;

            /* Converter functions ***************************************/

            function getUrl(userNotification) {
                switch (userNotification.notification.notificationName) {
                    case 'App.NewTenantRegistered':
                        return '/App/tenants?filterText=' + userNotification.notification.data.properties.tenancyName;
                    //case 'App.GdprDataPrepared':
                    //    return '/File/DownloadBinaryFile?id=' + userNotification.notification.data.properties.binaryObjectId + '&contentType=application/zip&fileName=collectedData.zip';
                        //Add your custom notification names to navigate to a URL when user clicks to a notification.
                    case 'App.ImportCompleted':
                        return userNotification.notification.data.url;
                    case 'App.DownloadFileCompleted':
                        return `/App/TempFile/DownloadTempFile?tempFileId=${userNotification.notification.data.properties.tempFileId}`;
                }

                //No url for this notification
                return null;
            }

            function showPriorityNotification(userNotification) {
                let dismiss = function () {
                    _notificationService.setNotificationAsRead({ id: userNotification.id }).done(function () {
                        abp.event.trigger('app.priorityNotificationDismissed', userNotification);
                    });
                };
                abp.notifications.showUiNotifyForUserNotification(userNotification, {
                    onclick: function () {
                        dismiss();
                        var url = getUrl(userNotification);
                        if (url) {
                            location.href = url;
                        }
                    },
                    timeOut: 0,
                    extendedTimeOut: 0,
                    closeButton: true,
                    onCloseClick: dismiss
                });
            }

            /* PUBLIC functions ******************************************/

            var format = function (userNotification, truncateText) {
                var formatted = {
                    userNotificationId: userNotification.id,
                    text: abp.notifications.getFormattedMessageFromUserNotification(userNotification),
                    time: moment(userNotification.notification.creationTime).format("YYYY-MM-DD HH:mm:ss"),
                    icon: app.notification.getUiIconBySeverity(userNotification.notification.severity),
                    state: abp.notifications.getUserNotificationStateAsString(userNotification.state),
                    data: userNotification.notification.data,
                    url: getUrl(userNotification),
                    isUnread: userNotification.state === abp.notifications.userNotificationState.UNREAD,
                    timeAgo: moment(userNotification.notification.creationTime).fromNow(),
                };

                if (truncateText || truncateText === undefined) {
                    formatted.text = abp.utils.truncateStringWithPostfix(formatted.text, 100);
                }
                
                return formatted;
            };

            var show = function (userNotification) {
                if (userNotification.notification.notificationName === 'App.PriorityNotification') {
                    showPriorityNotification(userNotification);
                } else {
                    //Application notification
                    abp.notifications.showUiNotifyForUserNotification(userNotification, {
                        'onclick': function () {
                            //Take action when user clicks to live toastr notification
                            var url = getUrl(userNotification);
                            if (url) {
                                location.href = url;
                            }
                        }
                    });
                }

                if (abp.setting.getBoolean('App.UserOptions.PlaySoundForNotifications')) {
                    if (!_audioNotification) {
                        _audioNotification = new Audio('/Common/Sounds/notification.mp3');
                    }
                    _audioNotification.play();
                }

                //Desktop notification
                Push.create("Dump Truck Dispatcher", {
                    body: format(userNotification).text,
                    icon: abp.appPath + 'Common/Images/app-logo-small.svg',
                    timeout: 6000,
                    onClick: function () {
                        window.focus();
                        this.close();
                    }
                }).catch((e) => {
                    //an exception will be thrown if the notifications permission is denied. we catch the exception to avoid showing it as an error in the console.
                    console.log(e);
                });
            };

            var setAllAsRead = function (callback) {
                _notificationService.setAllNotificationsAsRead().done(function () {
                    abp.event.trigger('app.notifications.refresh');
                    callback && callback();
                });
            };

            var setAsRead = function (userNotificationId, callback) {
                _notificationService.setNotificationAsRead({
                    id: userNotificationId
                }).done(function () {
                    abp.event.trigger('app.notifications.read', userNotificationId);
                    callback && callback(userNotificationId);
                });
            };

            var openSettingsModal = function () {
                new app.ModalManager({
                    viewUrl: abp.appPath + 'App/Notifications/SettingsModal',
                    scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Notifications/_SettingsModal.js',
                    modalClass: 'NotificationSettingsModal'
                }).open();
            };

            var showPriorityNotifications = function () {
                abp.services.app.notification.getUnreadPriorityNotifications().done(function (result) {
                    result.items.forEach(function (userNotification) {
                        showPriorityNotification(userNotification);
                    });
                });
            };

            /* Expose public API *****************************************/

            return {
                format: format,
                show: show,
                setAllAsRead: setAllAsRead,
                setAsRead: setAsRead,
                openSettingsModal: openSettingsModal,
                showPriorityNotifications: showPriorityNotifications
            };

        };

    })();

})(jQuery);
