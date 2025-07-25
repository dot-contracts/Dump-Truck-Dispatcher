/*
    OnlineClientManager should be used on messaging app service,
    to implement the already researched 'proper solution'
 */

(function () {
    abp.services.app.chat = abp.services.app.chat || {};
    abp.services.app.friendship = abp.services.app.friendship || {};
    abp.services.driverApp.message = abp.services.driverApp.message || {};
    abp.services.app.notification = abp.services.app.notification || {};

    // action 'getUserChatFriendsWithSettings'
    abp.services.app.chat.getUserChatFriendsWithSettings = function(ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.signalRPath + 'api/services/app/Chat/GetUserChatFriendsWithSettings',
            type: 'GET',
            xhrFields: {
                withCredentials: true
            }
        }, ajaxParams));
    };

    // action 'getUserChatMessages'
    abp.services.app.chat.getUserChatMessages = function (input, ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.signalRPath + 'api/services/app/Chat/GetUserChatMessages' + abp.utils.buildQueryString([{ name: 'tenantId', value: input.tenantId }, { name: 'userId', value: input.userId }, { name: 'minMessageId', value: input.minMessageId }, { name: 'maxResultCount', value: input.maxResultCount }, { name: 'skipCount', value: input.skipCount }]) + '',
            type: 'GET',
            xhrFields: {
                withCredentials: true
            }
        }, ajaxParams));
    };

    // action 'markAllUnreadMessagesOfUserAsRead'
    abp.services.app.chat.markAllUnreadMessagesOfUserAsRead = function (input, ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.signalRPath + 'api/services/app/Chat/MarkAllUnreadMessagesOfUserAsRead',
            type: 'POST',
            data: JSON.stringify(input),
            xhrFields: {
                withCredentials: true
            }
        }, ajaxParams));
    };

    // action 'createFriendshipRequest'
    abp.services.app.friendship.createFriendshipRequest = function(input, ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.signalRPath + 'api/services/app/Friendship/CreateFriendshipRequest',
            type: 'POST',
            data: JSON.stringify(input),
            xhrFields: {
                withCredentials: true
            }
        }, ajaxParams));
    };

    // action 'blockUser'
    abp.services.app.friendship.blockUser = function(input, ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.signalRPath + 'api/services/app/Friendship/BlockUser',
            type: 'POST',
            data: JSON.stringify(input),
            xhrFields: {
                withCredentials: true
            }
        }, ajaxParams));
    };

    // action 'unblockUser'
    abp.services.app.friendship.unblockUser = function(input, ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.signalRPath + 'api/services/app/Friendship/UnblockUser',
            type: 'POST',
            data: JSON.stringify(input),
            xhrFields: {
                withCredentials: true
            }
        }, ajaxParams));
    };

    // action 'acceptFriendshipRequest'
    abp.services.app.friendship.acceptFriendshipRequest = function(input, ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.signalRPath + 'api/services/app/Friendship/AcceptFriendshipRequest',
            type: 'POST',
            data: JSON.stringify(input),
            xhrFields: {
                withCredentials: true
            }
        }, ajaxParams));
    };

    // action 'createFriendshipRequestByUserName'
    abp.services.app.friendship.createFriendshipRequestByUserName = function(input, ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.signalRPath + 'api/services/app/Friendship/CreateFriendshipRequestByUserName',
            type: 'POST',
            data: JSON.stringify(input),
            xhrFields: {
                withCredentials: true
            }
        }, ajaxParams));
    };

    // action 'post'
    abp.services.driverApp.message.post = function(model, ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.signalRPath + 'api/services/driverApp/Message/Post',
            type: 'POST',
            data: JSON.stringify(model),
            xhrFields: {
                withCredentials: true
            }
        }, ajaxParams));
    };

    // action 'markAsRead'
    abp.services.driverApp.message.markAsRead = function(input, ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.signalRPath + 'api/services/driverApp/Message/MarkAsRead',
            type: 'POST',
            data: JSON.stringify(input),
            xhrFields: {
                withCredentials: true
            }
        }, ajaxParams));
    };

    // action 'getUserNotifications'
    abp.services.app.notification.getUserNotifications = function (input, ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.signalRPath + 'api/services/app/Notification/GetUserNotifications' + abp.utils.buildQueryString([{ name: 'state', value: input.state }, { name: 'startDate', value: input.startDate }, { name: 'endDate', value: input.endDate }, { name: 'maxResultCount', value: input.maxResultCount }, { name: 'skipCount', value: input.skipCount }]) + '',
            type: 'GET',
            xhrFields: {
                withCredentials: true
            }
        }, ajaxParams));;
    };

    // action 'getUnreadPriorityNotifications'
    abp.services.app.notification.getUnreadPriorityNotifications = function (ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.signalRPath + 'api/services/app/Notification/GetUnreadPriorityNotifications',
            type: 'GET',
            xhrFields: {
                withCredentials: true
            }
        }, ajaxParams));;
    };

    // action 'setAllNotificationsAsRead'
    abp.services.app.notification.setAllNotificationsAsRead = function (ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.signalRPath + 'api/services/app/Notification/SetAllNotificationsAsRead',
            type: 'POST',
            xhrFields: {
                withCredentials: true
            }
        }, ajaxParams));;
    };

    // action 'setNotificationAsRead'
    abp.services.app.notification.setNotificationAsRead = function (input, ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.signalRPath + 'api/services/app/Notification/SetNotificationAsRead',
            type: 'POST',
            data: JSON.stringify(input),
            xhrFields: {
                withCredentials: true
            }
        }, ajaxParams));;
    };

    // action 'deleteNotification'
    abp.services.app.notification.deleteNotification = function (input, ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.signalRPath + 'api/services/app/Notification/DeleteNotification' + abp.utils.buildQueryString([{ name: 'id', value: input.id }]) + '',
            type: 'DELETE',
            xhrFields: {
                withCredentials: true
            }
        }, ajaxParams));;
    };

    // action 'deleteAllUserNotifications'
    abp.services.app.notification.deleteAllUserNotifications = function (input, ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.signalRPath + 'api/services/app/Notification/DeleteAllUserNotifications' + abp.utils.buildQueryString([{ name: 'state', value: input.state }, { name: 'startDate', value: input.startDate }, { name: 'endDate', value: input.endDate }]) + '',
            type: 'DELETE',
            xhrFields: {
                withCredentials: true
            }
        }, ajaxParams));;
    };
})();
