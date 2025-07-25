(function () {
    $(function () {
        var _$notificationsTable = $('#NotificationsTable');
        var _notificationService = abp.services.app.notification;
        var _dtHelper = abp.helper.dataTables;

        var _$targetValueFilterSelectionCombobox = $('#TargetValueFilterSelectionCombobox');
        _$targetValueFilterSelectionCombobox.select2Init({
            showAll: true,
            allowClear: true
        });

        var _appUserNotificationHelper = new app.UserNotificationHelper();

        var createNotificationReadButton = function ($td, record) {
            var $span = $('<span/>');
            $span.css('width', '90px');
            var $button = $("<button/>")
                .addClass("btn btn-sm btn-primary")
                .attr("title", app.localize('SetAsUnread'))
                .click(function (e) {
                    e.preventDefault();
                    setNotificationAsRead(record, function () {
                        $button.find('i')
                            .removeClass('fa-circle')
                            .addClass('fa-check');
                        $td.closest("tr").addClass("notification-read");
                    });
                }).appendTo($span);

            var $buttonDelete = $("<button/>")
                .addClass("btn btn-sm btn-danger")
                .attr("title", app.localize('Delete'))
                .click(function () {
                    deleteNotification(record);
                }).appendTo($span);
            $('<i class="fa fa-remove" >').appendTo($buttonDelete);

            var $i = $('<i class="fa" >').appendTo($button);
            var notificationState = _appUserNotificationHelper.format(record).state;

            if (notificationState === 'READ') {
                $i.addClass('fa-check');
            }

            if (notificationState === 'UNREAD') {
                $i.addClass('fa-circle');
                $button.attr("title", app.localize('SetAsRead'));
            }

            $td.append($span);
        };

        var dataTable = _$notificationsTable.DataTableInit({
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = $.extend(_dtHelper.toAbpData(data), {
                    state: _$targetValueFilterSelectionCombobox.val()
                });
                _notificationService.getUserNotifications(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            columns: [
                {
                    data: null,
                    className: 'control responsive',
                    orderable: false,
                    render: function () {
                        return '';
                    }
                },
                {
                    data: null,
                    orderable: false,
                    defaultContent: '',
                    width: '95px',
                    createdCell: function (td, cellData, rowData, rowIndex, colIndex) {
                        createNotificationReadButton($(td), rowData);
                    }
                },
                {
                    orderable: false,
                    data: "notification",
                    render: function (notification, type, row, meta) {
                        var $container;
                        var formattedRecord = _appUserNotificationHelper.format(row, false);
                        formattedRecord.text = _dtHelper.renderText(formattedRecord.text);
                        var rowClass = getRowClass(formattedRecord);

                        if (formattedRecord.url && formattedRecord.url !== '#') {
                            formattedRecord.url = _dtHelper.renderText(formattedRecord.url);
                            $container = $('<a title="' + formattedRecord.text + '" href="' + formattedRecord.url + '" class="' + rowClass + '">' + formattedRecord.text + '</a>');
                        } else {
                            $container = $('<span title="' + formattedRecord.text + '" class="' + rowClass + '">' + formattedRecord.text + '</span>');
                        }

                        return $container[0].outerHTML;
                    }
                },
                {
                    orderable: false,
                    data: null, //"creationTime" might have been causing #11723
                    render: function (data, type, row, meta) {
                        var creationTime = row.creationTime;
                        var formattedRecord = _appUserNotificationHelper.format(row);
                        var rowClass = getRowClass(formattedRecord);
                        var $container = $('<span title="' + moment(creationTime).format("llll") + '" class="' + rowClass + '">' + formattedRecord.timeAgo + '</span> &nbsp;');
                        return $container[0].outerHTML;
                    }
                }
            ]
        });

        async function deleteNotification(notification) {
            if (await abp.message.confirm(
                app.localize('NotificationDeleteWarningMessage')
            )) {
                _notificationService.deleteNotification({
                    id: notification.id
                }).done(function () {
                    getNotifications();
                    abp.notify.success(app.localize('SuccessfullyDeleted'));
                });
            }
        };

        function getRowClass(formattedRecord) {
            return formattedRecord.state === 'READ' ? 'notification-read' : '';
        }

        function getNotifications() {
            dataTable.ajax.reload();
        }

        function setNotificationAsRead(userNotification, callback) {
            _appUserNotificationHelper.setAsRead(userNotification.id, function () {
                if (callback) {
                    callback();
                    getNotifications();
                }
            });
        }

        function setAllNotificationsAsRead() {
            _appUserNotificationHelper.setAllAsRead(function () {
                getNotifications();
            });
        };

        function openNotificationSettingsModal() {
            _appUserNotificationHelper.openSettingsModal();
        };

        _$targetValueFilterSelectionCombobox.change(function () {
            getNotifications();
        });

        $('#RefreshNotificationTableButton').click(function (e) {
            e.preventDefault();
            getNotifications();
        });

        $('#btnOpenNotificationSettingsModal').click(function (e) {
            openNotificationSettingsModal();
        });

        $('#btnSetAllNotificationsAsRead').click(function (e) {
            e.preventDefault();
            setAllNotificationsAsRead();
        });

    });
})();
