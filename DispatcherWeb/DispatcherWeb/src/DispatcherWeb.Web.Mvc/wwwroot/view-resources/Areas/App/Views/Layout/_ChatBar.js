var chatService = abp.services.app.chat;
var friendshipService = abp.services.app.friendship;

var chat = {
    friends: [],
    tenantToTenantChatAllowed: abp.features.isEnabled('App.ChatFeature.TenantToTenant'),
    tenantToHostChatAllowed: abp.features.isEnabled('App.ChatFeature.TenantToHost'),
    serverClientTimeDifference: 0,
    selectedUser: null,
    isOpen: false,
    pageSize: 25,
    suppressScrollInvokedLoading: false,
    animationDelay: 200,
    boundUiEvents: false,

    getFriendOrNull: function (userId, tenantId) {
        return chat.friends.find(u =>
            u.friendUserId === parseInt(userId)
            && u.friendTenantId === (tenantId ? parseInt(tenantId) : null)
        ) ?? null;
    },

    getFixedMessageTime: function (messageTime) {
        return moment(messageTime).add(-1 * chat.serverClientTimeDifference, 'seconds').format('YYYY-MM-DDTHH:mm:ssZ');
    },

    getFriendsAndSettings: function (callBack) {
        chatService.getUserChatFriendsWithSettings().done(function (result) {
            chat.friends = result.friends;
            chat.serverClientTimeDifference = app.calculateTimeDifference(abp.clock.now(), result.serverTime, 'seconds');

            chat.triggerUnreadMessageCountChangeEvent();
            chat.renderFriendLists(chat.friends);
            callBack();
        });
    },

    loadLastState: function () {
        app.localStorage.getItem('app.chat.isOpen', function (isOpen) {
            chat.isOpen = isOpen;
            chat.adjustNotifyPosition();

            app.localStorage.getItem('app.chat.pinned', function (pinned) {
                chat.pinned = pinned;
                var $sidebarPinner = $('a.page-quick-sidebar-pinner');
                $sidebarPinner.css("background-image", "url(" + abp.appPath + "Common/Images/" + (chat.pinned ? 'pinned' : 'unpinned') + ".png)");
            });

            if (chat.isOpen) {

                $('body, #m_quick_sidebar').addClass('m-quick-sidebar--on').promise().done(function () {
                    $('#m_quick_sidebar .m-quick-sidebar__content').removeClass("m--hide");
                    app.localStorage.getItem('app.chat.selectedUser', function (user) {
                        if (user) {
                            chat.showMessagesPanel();
                            chat.selectFriend(user.friendUserId, user.friendTenantId);
                        } else {
                            chat.showFriendsPanel();
                        }
                    });
                });
            }
        });
    },

    changeChatPanelIsOpenOnLocalStorage: function () {
        app.localStorage.setItem('app.chat.isOpen', chat.isOpen);
    },

    changeChatUserOnLocalStorage: function () {
        app.localStorage.setItem('app.chat.selectedUser', chat.selectedUser);
    },

    changeChatPanelPinnedOnLocalStorage: function () {
        app.localStorage.setItem('app.chat.pinned', chat.pinned);
    },

    changeChatPanelPinned: function (pinned) {
        chat.pinned = pinned;
        var $sidebarPinner = $(".page-quick-sidebar-pinner");
        $sidebarPinner.css("background-image", "url(" + abp.appPath + "Common/Images/" + (chat.pinned ? 'pinned' : 'unpinned') + ".png)");

        chat.changeChatPanelPinnedOnLocalStorage();
    },

    //Friends
    selectFriend: async function (friendUserId, friendTenantId) {
        var user = chat.getFriendOrNull(friendUserId, friendTenantId);
        chat.selectedUser = user;
        chat.changeChatUserOnLocalStorage();
        chat.user.setSelectedUserOnlineStatus(user.isOnline);

        chat.destroyConversationScrollbar();
        chat.showMessagesPanel();
        $('#UserChatMessages').empty();
        if (user.messages?.length) {
            var renderedMessages = chat.renderMessages(user.messages);
            $('#UserChatMessages').html(renderedMessages);
            $(".timeago").timeago();
        }

        if (chat.selectedUser.friendProfilePictureId) {
            var tenantId = chat.selectedUser.friendTenantId ? chat.selectedUser.friendTenantId : '';
            $('#selectedChatUserImage').attr('src', abp.appPath + 'Profile/GetFriendProfilePictureById?id=' + chat.selectedUser.friendProfilePictureId + '&userId=' + chat.selectedUser.friendUserId + '&tenantId=' + tenantId);
        } else {
            $('#selectedChatUserImage').attr('src', abp.appPath + 'Common/Images/default-profile-picture.png');
        }

        $('#selectedChatUserName').text(chat.user.getShownUserName(chat.selectedUser));
        $('#ChatMessage').val('');

        if (chat.selectedUser.state !== app.consts.friendshipState.blocked) {
            $('#liBanChatUser, #ChatMessageWrapper').show();
            $('#liUnbanChatUser, #UnblockUserButton').hide();
            $('#ChatMessage').removeAttr("disabled");
            $('#ChatMessage').focus();
            $('#btnSendChatMessenger').removeAttr("disabled");
        } else {
            $('#liBanChatUser, #ChatMessageWrapper').hide();
            $('#liUnbanChatUser, #UnblockUserButton').show();
            $('#ChatMessage').attr("disabled", "disabled");
            $('#btnSendChatMessenger').attr("disabled", "disabled");
        }

        if (!user.messagesLoaded) {
            await chat.user.loadMessages(user);
            user.messagesLoaded = true;
        } else {
            chat.user.markAllUnreadMessagesOfUserAsRead(chat.selectedUser);
        }

        await app.sleepAsync(chat.animationDelay);
        chat.scrollToBottom();
        chat.initConversationScrollbar();
    },

    initConversationScrollbar: async function () {
        chat.destroyConversationScrollbar();

        setTimeout(async () => {
            let messagesFitOnScreen = true;
            while (messagesFitOnScreen) {
                messagesFitOnScreen = await chat.loadMoreMessagesIfNeeded();
            }

            const messageContainer = $('#UserChatMessages');
            messageContainer.on('scroll', chat.loadMoreMessagesIfNeeded);
        }, 300);
    },

    destroyConversationScrollbar: function () {
        const messageContainer = $('#UserChatMessages');
        messageContainer.off('scroll', chat.loadMoreMessagesIfNeeded);
    },

    loadMoreMessagesIfNeeded: async function () {
        if (chat.suppressScrollInvokedLoading) {
            return;
        }
        const user = chat.selectedUser;
        if (!user) {
            return false;
        }
        const messageContainer = $('#UserChatMessages');
        const currentPosition = messageContainer.scrollTop();
        if (currentPosition < 1
            && !chat.user.loadingPreviousUserMessages
            && !user.allPreviousMessagesLoaded
            && user.messages?.length > 0
        ) {
            user.position = rememberScrollPosition();
            //user.skipCount = (user.skipCount || 0) + chat.pageSize;
            let loadResult;

            chat.suppressScrollInvokedLoading = true;
            abp.ui.setBusy(messageContainer);
            try {
                loadResult = await chat.user.loadMessages(user);
            } finally {
                abp.ui.clearBusy(messageContainer);
                restoreScrollAfterLoading();
                chat.suppressScrollInvokedLoading = false;
            }

            return loadResult.items.length > 0;
        }
        return false;
    },

    showMessagesPanel: function () {
        $('.m-messenger-friends').hide();
        $('.m-messenger-conversation').show(function () {
            //chat.scrollToBottom();
            //chat.initConversationScrollbar();
        });
        $('#m_quick_sidebar_back').removeClass("d-none");
    },

    showFriendsPanel: function () {
        $('.m-messenger-friends').show();
        $('.m-messenger-conversation').hide();
        $('#m_quick_sidebar_back').addClass("d-none");
    },

    changeFriendState: function (user, state) {
        var friend = chat.getFriendOrNull(user.friendUserId, user.friendTenantId);
        if (!friend) {
            return;
        }

        friend.state = state;
        chat.renderFriendLists(chat.friends);
    },

    getFormattedFriends: function (friends) {
        friends.forEach(function (friend) {
            friend.profilePicturePath = chat.getFriendProfilePicturePath(friend);
            friend.shownUserName = chat.user.getShownUserName(friend);
        });
        return friends;
    },

    renderFriendList: function (friends, $element) {
        var template = $('#UserFriendTemplate').html();
        Mustache.parse(template);

        var rendered = Mustache.render(template, friends);
        $element.html(rendered);
    },

    renderFriendLists: function (friends) {
        friends = chat.getFormattedFriends(friends);

        var acceptedFriends = friends.filter(u => u.state === app.consts.friendshipState.accepted);
        chat.renderFriendList(acceptedFriends, $('#friendListFriends'));

        var blockedFriends = friends.filter(u => u.state === app.consts.friendshipState.blocked);
        chat.renderFriendList(blockedFriends, $('#friendListBlockeds'));

        if (acceptedFriends.length) {
            $('#EmptyFriendListInfo').hide();
        } else {
            $('#EmptyFriendListInfo').show();
        }

        if (blockedFriends.length) {
            $('#EmptyBlockedFriendListInfo').hide();
            $('#friendListBlockeds').show();
        } else {
            $('#EmptyBlockedFriendListInfo').show();
            $('#friendListBlockeds').hide();
        }
    },

    sortFriends: function (friends) {
        friends.sort((a, b) => {
            const minDate = '0000-01-01T00:00:00Z';
            const timeA = a.lastMessageCreationTime ?? minDate;
            const timeB = b.lastMessageCreationTime ?? minDate;

            return -1 * timeA.localeCompare(timeB);
        });
    },

    getFriendProfilePicturePath: function (friend) {
        if (!friend.friendProfilePictureId) {
            return abp.appPath + 'Common/Images/default-profile-picture.png';
        }

        var tenantId = friend.friendTenantId ? friend.friendTenantId : '';
        return abp.appPath + 'Profile/GetFriendProfilePictureById?id=' + friend.friendProfilePictureId + '&userId=' + friend.friendUserId + '&tenantId=' + tenantId;
    },

    //Messages
    sendMessage: async function () {
        sendFiles();
        if (!$('form[name=\'chatMessageForm\']').valid() || chat.selectedUser.state === app.consts.friendshipState.blocked) {
            return;
        }

        $('#btnSendChatMessenger').attr('disabled', 'disabled');

        try {
            await app.chat.sendMessage({
                targetUserId: chat.selectedUser.friendUserId,
                message: $('#ChatMessage').val(),
            });

            $('#ChatMessage').val('');
        } finally {
            $('#btnSendChatMessenger').removeAttr('disabled');
        }
    },

    getFormattedMessages: function (messages) {
        $.each(messages, function (index, message) {
            message.creationTime = chat.getFixedMessageTime(message.creationTime);
            message.cssClass = message.side === app.chat.side.sender ? 'm-messenger__message--out' : 'm-messenger__message--in';
            message.isIn = message.side !== app.chat.side.sender;

            if (message.side === app.chat.side.sender) {
                message.shownUserName = `${app.session.user.surname}, ${app.session.user.name}`;
            } else {
                message.shownUserName = chat.user.getShownUserName(chat.selectedUser, message);
            }

            message.profilePicturePath = message.side === app.chat.side.sender ?
                (!app.session.user.profilePictureId ? (abp.appPath + 'Common/Images/default-profile-picture.png') : (abp.appPath + 'Profile/GetProfilePictureById?id=' + app.session.user.profilePictureId)) :
                chat.getFriendProfilePicturePath(chat.selectedUser);

            var readStateClass = message.receiverReadState === app.chat.readState.read ? ' m--font-info' : ' m--font-secondary';
            message.readStateCheck = message.side === app.chat.side.sender ? '<i class="read-state-check fa fa-check' + readStateClass + '" aria-hidden="true"></i>' : '';
            var fileUrl = "";
            if (message.message.startsWith('[image]')) {
                var image = JSON.parse(message.message.substring('[image]'.length));
                fileUrl = abp.appPath + 'App/Chat/GetImage?id=' + message.id + '&contentType=' + image.contentType;
                var uploadedImageMsg = '<a href="' + fileUrl + '" target="_blank"><img src="' + fileUrl + '" class="chat-image-preview"></a>';

                message.formattedMessage = uploadedImageMsg;
            } else if (message.message.startsWith('[file]')) {
                var file = JSON.parse(message.message.substring('[file]'.length));
                fileUrl = abp.appPath + 'App/Chat/GetFile?id=' + message.id + '&contentType=' + file.contentType;
                var uploadedFileMsg = '<a href="' + fileUrl + '" target="_blank" class="chat-file-preview"><i class="fa-regular fa-file"></i> ' + file.name + ' <i class="fa fa-download pull-right"></i></a>';

                message.formattedMessage = uploadedFileMsg;
            } else if (message.message.startsWith('[link]')) {
                var linkMessage = JSON.parse(message.message.substring('[file]'.length));

                message.formattedMessage = '<a href="' + linkMessage.message + '" target="_blank" class="chat-link-message"><i class="fa fa-link"></i> ' + linkMessage.message + '</a>';
            } else {
                message.formattedMessage = Mustache.escape(message.message);
            }
        });

        return messages;
    },

    renderMessages: function (messages) {
        messages = chat.getFormattedMessages(messages);

        var template = $('#UserChatMessageTemplate').html();
        Mustache.parse(template);

        return Mustache.render(template, messages);
    },

    scrollToBottom: function () {
        const chatBox = document.getElementById("UserChatMessages");
        chatBox.style.scrollBehavior = 'auto';
        chatBox.scrollTop = chatBox.scrollHeight;
        chatBox.style.scrollBehavior = 'smooth';
    },

    //Events & UI

    adjustNotifyPosition: function () {
        if (chat.isOpen) {
            app.changeNotifyPosition('toast-chat-open');
        } else {
            app.changeNotifyPosition('toast-bottom-right');
        }
    },

    triggerUnreadMessageCountChangeEvent: function () {
        var totalUnreadMessageCount = 0;
        if (chat && chat.friends) {
            totalUnreadMessageCount = chat.friends.reduce(function (memo, friend) { return memo + friend.unreadMessageCount; }, 0);
        }

        abp.event.trigger('app.chat.unreadMessageCountChanged', totalUnreadMessageCount);
    },

    bindUiEvents: function () {
        if (this.boundUiEvents) {
            return;
        }
        this.boundUiEvents = true;

        var quickSidebarOffCanvas = new mOffcanvas('m_quick_sidebar', {
            baseClass: 'm-quick-sidebar',
            closeBy: 'm_quick_sidebar_close',
            toggleBy: 'm_quick_sidebar_toggle'
        });

        // run once on first time dropdown shown
        quickSidebarOffCanvas.one('afterShow', function () {
            mApp.block($('#m_quick_sidebar'));

            setTimeout(function () {
                mApp.unblock($('#m_quick_sidebar'));
                $('#m_quick_sidebar').find('.m-quick-sidebar__content').removeClass('m--hide');
            }, 1000);
        });

        $('#m_quick_sidebar').on('click', '#m_quick_sidebar_back', function () {
            chat.selectedUser = null;
            chat.changeChatUserOnLocalStorage();
        });

        $('#m_quick_sidebar_toggle').on('click', function () {
            chat.isOpen = $('body').hasClass('m-quick-sidebar--on');
            chat.adjustNotifyPosition();
            chat.changeChatPanelIsOpenOnLocalStorage();
            chat.user.markAllUnreadMessagesOfUserAsRead(chat.selectedUser);
        });

        $('.m-messenger-friends').on('click', '.m-list-search__result-item', function () {
            var friendUserId = $(this).attr('data-friend-user-id');
            var friendTenantId = $(this).attr('data-friend-tenant-id');
            chat.selectFriend(friendUserId, friendTenantId);
        });

        $('#m_quick_sidebar_back').on('click', function () {
            chat.showFriendsPanel();
        });

        $('#liBanChatUser a').click(function () {
            chat.user.block(chat.selectedUser);
        });
        $('#liUnbanChatUser, #UnblockUserButton').click(function () {
            chat.user.unblock(chat.selectedUser);
        });

        let resizeTimeout = null;
        $(window).on('resize', function () {
            resizeTimeout && clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(() => {
                setChatContainerHeight();
                chat.initConversationScrollbar();
            }, 1000);
        });

        $('#SearchChatUserButton').click(async function () {
            $(this).prop('disabled', true);
            try {
                await chat.user.search();
            } finally {
                $(this).prop('disabled', false);
            }
        });

        $('#ChatUserSearchUserName, #ChatUserSearchTenancyName').keypress(function (e) {
            if (e.which === 13) {
                chat.user.search();
            }
        });


        $('#ChatMessage').keypress(function (e) {
            if (e.which === 13) {
                e.preventDefault();
                // Check if the button is disabled
                if ($('#btnSendChatMessenger').attr('disabled')) {
                    return;
                }
                chat.sendMessage();
            }
        });

        $('#btnSendChatMessenger').click(function () {
            // Check if the button is disabled
            if ($(this).attr('disabled')) {
                return;
            }
            chat.sendMessage();
        });


        $('#ChatUserSearchUserName').on('keyup', function () {
            var userName = $(this).val();

            var friends = chat.friends.filter(function (friend) {
                return chat.user.getShownUserName(friend).toLowerCase().indexOf(userName.toLowerCase()) >= 0;
            });

            chat.renderFriendLists(friends);
        });

        $('div.m-quick-sidebar').on('mouseleave', function (e) {
            if (chat.pinned || $(e.target).attr("data-toggle") === 'm-popover') { // don't hide chat panel when mouse is on popover notification
                return;
            }

            $('body, #m_quick_sidebar').removeClass('m-quick-sidebar--on');
            var quickSidebarOffCanvas = new mOffcanvas('m_quick_sidebar');
            quickSidebarOffCanvas.hide();
            chat.isOpen = false;
            chat.adjustNotifyPosition();
            chat.changeChatPanelIsOpenOnLocalStorage();
        });

        $('form[name=\'chatMessageForm\']').validate({
            invalidHandler: function () {
                $('#btnSendChatMessenger').attr('disabled', 'disabled');
            },
            errorPlacement: function () {

            },
            success: function () {
                $('#btnSendChatMessenger').removeAttr('disabled');
            }
        });

        $('.page-quick-sidebar-pinner').click(function () {
            chat.changeChatPanelPinned(!chat.pinned);
        });
    },

    registerEvents: function () {

        abp.event.on('app.chat.messageReceived', function (message) {
            var user = chat.getFriendOrNull(message.targetUserId, message.targetTenantId);

            if (user) {
                user.messages = user.messages || [];
                user.messages.push(message);
                user.lastMessageCreationTime = message.creationTime;

                if (message.side === app.chat.side.receiver) {
                    user.unreadMessageCount += 1;
                    message.readState = app.chat.readState.unread;
                    chat.user.changeUnreadMessageCount(user.friendTenantId, user.friendUserId, user.unreadMessageCount);
                    chat.triggerUnreadMessageCountChangeEvent();

                    if (chat.isOpen && chat.selectedUser !== null && user.friendTenantId === chat.selectedUser.friendTenantId && user.friendUserId === chat.selectedUser.friendUserId) {
                        chat.user.markAllUnreadMessagesOfUserAsRead(chat.selectedUser);
                    } else {
                        abp.notify.info(
                            abp.utils.formatString('{0}: {1}', chat.user.getShownUserName(user, message), abp.utils.truncateString(message.message, 100)),
                            null,
                            {
                                onclick: function () {
                                    if (!$('body').hasClass('m-quick-sidebar--on')) {
                                        $('body').addClass('m-quick-sidebar--on');
                                        chat.isOpen = true;
                                        chat.changeChatPanelIsOpenOnLocalStorage();
                                    }

                                    chat.showMessagesPanel();

                                    chat.selectFriend(user.friendUserId, user.friendTenantId);
                                    chat.changeChatPanelPinned(true);
                                }
                            }
                        );
                    }
                }

                if (chat.selectedUser !== null && user.friendUserId === chat.selectedUser.friendUserId && user.friendTenantId === chat.selectedUser.friendTenantId) {
                    var renderedMessage = chat.renderMessages([message]);
                    $('#UserChatMessages').append(renderedMessage);
                    $(".timeago").timeago();
                }

                chat.sortFriends(chat.friends);
                chat.renderFriendLists(chat.friends);

                chat.scrollToBottom();
            }
        });

        abp.event.on('app.chat.friendshipRequestReceived', function (friend, isOwnRequest) {
            if (!isOwnRequest) {
                abp.notify.info(abp.utils.formatString(app.localize('UserSendYouAFriendshipRequest'), chat.user.getShownUserName(friend)));
            }

            if (!chat.friends.some(u => u.userId === friend.friendUserId && u.tenantId === friend.friendTenantId)) {
                chat.friends.push(friend);
                chat.renderFriendLists(chat.friends);
            }
        });

        abp.event.on('app.chat.userConnectionStateChanged', function (data) {
            chat.user.setFriendOnlineStatus(data.friend.userId, data.friend.tenantId, data.isConnected);
        });

        abp.event.on('app.chat.userStateChanged', function (data) {
            var user = chat.getFriendOrNull(data.friend.userId, data.friend.tenantId);
            if (!user) {
                return;
            }

            user.state = data.state;
            chat.renderFriendLists(chat.friends);
        });

        abp.event.on('app.chat.allUnreadMessagesOfUserRead', function (data) {
            var user = chat.getFriendOrNull(data.friend.userId, data.friend.tenantId);
            if (!user) {
                return;
            }

            user.unreadMessageCount = 0;
            chat.user.changeUnreadMessageCount(user.friendTenantId, user.friendUserId, user.unreadMessageCount);
            chat.triggerUnreadMessageCountChangeEvent();
        });

        abp.event.on('app.chat.readStateChange', function (data) {
            var user = chat.getFriendOrNull(data.friend.userId, data.friend.tenantId);
            if (!user) {
                return;
            }

            $.each(user.messages,
                function (index, message) {
                    message.receiverReadState = app.chat.readState.read;
                });

            if (chat.selectedUser && chat.selectedUser.friendUserId === data.friend.userId) {
                $('.read-state-check').not('.m--font-info').addClass('m--font-info');
            }
        });

        abp.event.on('app.chat.connected', function () {
            $('#chat_is_connecting_icon').hide();
            $('#m_quick_sidebar_toggle').removeClass('d-none');
            chat.getFriendsAndSettings(function () {
                chat.bindUiEvents();
                chat.loadLastState();
            });
        });
    },

    init: function () {
        chat.registerEvents();
        chat.interTenantChatAllowed = abp.features.isEnabled('App.ChatFeature.TenantToTenant')
            || abp.features.isEnabled('App.ChatFeature.TenantToHost')
            || !app.session.tenant;
    },

    user: {
        loadingPreviousUserMessages: false,
        userNameFilter: '',

        getShownUserName: function (friend, message) {
            let name = `${friend.friendLastName}, ${friend.friendFirstName}`;
            if (message) {
                if (message.targetTruckCode) {
                    name += ` (Truck ${message.targetTruckCode}`;
                    if (message.targetTruckLeaseHaulerName) {
                        name += ` - ${message.targetTruckLeaseHaulerName}`;
                    }
                    name += `)`;
                }
            }
            return name;
        },

        block: function (user) {
            friendshipService.blockUser({
                userId: user.friendUserId,
                tenantId: user.friendTenantId
            }).done(function () {
                chat.changeFriendState(user, app.consts.friendshipState.blocked);
                abp.notify.info(app.localize('UserBlocked'));

                $('#ChatMessage').attr("disabled", "disabled");
                $('#btnSendChatMessenger').attr("disabled", "disabled");
                $('#liBanChatUser, #ChatMessageWrapper').hide();
                $('#liUnbanChatUser, #UnblockUserButton').show();
            });
        },

        unblock: function (user) {
            friendshipService.unblockUser({
                userId: user.friendUserId,
                tenantId: user.friendTenantId
            }).done(function () {
                chat.changeFriendState(user, app.consts.friendshipState.accepted);
                abp.notify.info(app.localize('UserUnblocked'));

                $('#ChatMessage').removeAttr("disabled");
                $('#ChatMessage').focus();
                $('#btnSendChatMessenger').removeAttr("disabled");
                $('#liBanChatUser, #ChatMessageWrapper').show();
                $('#liUnbanChatUser, #UnblockUserButton').hide();
            });
        },

        markAllUnreadMessagesOfUserAsRead: function (user) {
            if (!user || !chat.isOpen) {
                return;
            }

            var unreadMessageIds = user.messages.filter(m => m.readState === app.chat.readState.unread).map(m => m.id);
            if (!unreadMessageIds.length) {
                return;
            }

            chatService.markAllUnreadMessagesOfUserAsRead({
                tenantId: user.friendTenantId,
                userId: user.friendUserId
            }).done(function () {
                $.each(user.messages,
                    function (index, message) {
                        if (unreadMessageIds.indexOf(message.id) >= 0) {
                            message.readState = app.chat.readState.read;
                        }
                    });
            });
        },

        changeUnreadMessageCount: function (tenantId, userId, messageCount) {
            if (!tenantId) {
                tenantId = '';
            }
            var $userItems = $('a.m-list-search__result-item[data-friend-tenant-id="' + tenantId + '"][data-friend-user-id="' + userId + '"]');
            if ($userItems) {
                var $item = $($userItems[0]).find('span.m-badge');
                $item.text(messageCount);

                if (messageCount) {
                    $item.removeClass('d-none');
                } else {
                    $item.addClass('d-none');
                }
            }
        },

        loadMessages: async function (user, callback) {
            chat.user.loadingPreviousUserMessages = true;

            var minMessageId = null;
            if (user.messages?.length) {
                minMessageId = Math.min(...user.messages.map(m => m.id));
            }

            let result = await chatService.getUserChatMessages({
                tenantId: user.friendTenantId,
                userId: user.friendUserId,
                minMessageId: minMessageId,
                //skipCount: user.skipCount || 0,
                maxResultCount: chat.pageSize,
            });

            if (!user.messages) {
                user.messages = [];
            }

            if (!result.items.length) {
                user.allPreviousMessagesLoaded = true;
            }

            user.messages = result.items.concat(user.messages);
            chat.user.markAllUnreadMessagesOfUserAsRead(user);

            var renderedMessages = chat.renderMessages(user.messages);
            $('#UserChatMessages').html(renderedMessages);

            if (user.messages.length > 0) {
                $(".timeago").timeago();
            }

            chat.user.loadingPreviousUserMessages = false;
            setChatContainerHeight();

            if (callback) {
                callback();
            }

            return result;
        },

        openSearchModal: async function (tenantId) {
            var lookupModal = app.modals.LookupModal.create({
                title: app.localize('SelectAUser'),
                serviceMethod: abp.services.app.commonLookup.findUsers,
                filterText: $('#ChatUserSearchUserName').val(),
                extraFilters: { tenantId: tenantId }
            });

            await lookupModal.open({}, function (selectedItem) {
                var userId = selectedItem.value;
                friendshipService.createFriendshipRequest({
                    userId: userId,
                    tenantId: app.session.tenant?.id
                }).done(function () {
                    $('#ChatUserSearchUserName').val('');
                    setChatContainerHeight();
                });
            });
        },

        search: async function () {
            var userNameValue = $('#ChatUserSearchUserName').val();
            var tenancyName = '';
            var userName = '';

            if (userNameValue.indexOf('\\') === -1) {
                userName = userNameValue;
            } else {
                var tenancyAndUserNames = userNameValue.split('\\');
                tenancyName = tenancyAndUserNames[0];
                userName = tenancyAndUserNames[1];
            }

            if (!tenancyName || !chat.interTenantChatAllowed) {
                await chat.user.openSearchModal(app.session.tenant?.id, userName);
            } else {
                await friendshipService.createFriendshipRequestByUserName({
                    tenancyName: tenancyName,
                    userName: userName
                }).done(function () {
                    $('#ChatUserSearchUserName').val('');
                });
            }
        },

        setFriendOnlineStatus: function (userId, tenantId, isOnline) {
            var user = chat.getFriendOrNull(userId, tenantId);
            if (!user) {
                return;
            }

            user.isOnline = isOnline;

            var statusClass = 'contact-status1 ' + (isOnline ? 'online' : 'offline');
            var $userItems = $('a.m-list-search__result-item[data-friend-tenant-id="' + (tenantId ? tenantId : '') + '"][data-friend-user-id="' + userId + '"]');
            if ($userItems) {
                $($userItems[0]).find('.contact-status1').attr('class', statusClass);
            }

            if (chat.selectedUser
                && tenantId === chat.selectedUser.friendTenantId
                && userId === chat.selectedUser.friendUserId) {

                chat.user.setSelectedUserOnlineStatus(isOnline);
            }
        },

        setSelectedUserOnlineStatus: function (isOnline) {
            if (chat.selectedUser) {
                var statusClass = 'contact-status2 ' + (isOnline ? 'online' : 'offline');
                $('#selectedChatUserStatus').attr('class', statusClass);
            }
        }
    }
};

chat.init();

function sendFiles() {
    var files = $('#SendFileHolder div.send-file');
    files.each(function () {
        sendFile($(this));
    });
}
async function sendFile($fileDiv) {
    var message = $fileDiv.data('message');
    await app.chat.sendMessage({
        targetUserId: chat.selectedUser.friendUserId,
        message: message,
    });
    $fileDiv.remove();
}

function setChatContainerHeight() {
    var height = $('#m_quick_sidebar').outerHeight(true) - $(".selected-chat-user").outerHeight(true) - $('#chatMessageForm').outerHeight(true) - 150;
    $('#UserChatMessages').css('height', height + 'px');
    $('#UserChatMessages').css('overflow-y', 'scroll');
}

function rememberScrollPosition() {
    const messageContainer = document.querySelector('#UserChatMessages');
    const messages = messageContainer.querySelectorAll('.m-messenger__message');

    // Find the topmost visible message
    let topMessage = null;
    for (let message of messages) {
        const rect = message.getBoundingClientRect();
        if (rect.top >= 0 && rect.bottom <= window.innerHeight) {
            topMessage = message;
            break;
        }
    }

    if (!topMessage) return null;

    // Calculate the offset of the topmost visible message
    const topMessageOffset = topMessage.offsetTop - messageContainer.scrollTop;
    return { topMessageId: topMessage.id, topMessageOffset };
}

function restoreScrollAfterLoading() {
    const position = chat.selectedUser?.position;
    if (!position) {
        return;
    }
    const { topMessageId, topMessageOffset } = position;

    const messageContainer = document.querySelector('#UserChatMessages');
    const topMessage = document.getElementById(topMessageId);
    if (!topMessage) {
        return;
    }

    messageContainer.style.scrollBehavior = 'auto';
    messageContainer.scrollTop = topMessage.offsetTop - topMessageOffset;
    messageContainer.style.scrollBehavior = 'smooth';
}

(function ($) {
    $(function () {
        // Change this to the location of your server-side upload handler:
        var url = abp.appPath + 'App/Chat/UploadFile';

        setChatContainerHeight();

        //image upload
        $('#chatImageUpload').fileupload({
            url: url,
            dataType: 'json',
            maxFileSize: 999000,
            add: function (e, data) {
                var uploadErrors = [];
                if (data.originalFiles[0]['size'] > 10000000) {
                    uploadErrors.push('File size is too big (the maximum size is 10MB)');
                }
                if (uploadErrors.length > 0) {
                    abp.message.error(uploadErrors.join("\n"));
                } else {
                    data.submit();
                }
            },
            done: function (e, response) {
                var jsonResult = response.result;

                var chatMessage = '[image]{"id":"' + jsonResult.result.id + '", "name":"' + jsonResult.result.name + '", "contentType":"' + jsonResult.result.contentType + '"}';
                //$('#ChatMessage').val('[image]{"id":"' + jsonResult.result.id + '", "name":"' + jsonResult.result.name + '", "contentType":"' + jsonResult.result.contentType + '"}');
                //chat.sendMessage();
                addFile(jsonResult.result.id, jsonResult.result.name, jsonResult.result.size, chatMessage);
                $('.chat-progress-bar').hide();
            },
            progressall: function (e, data) {
                var progress = parseInt(data.loaded / data.total * 100, 10);
                $('.chat-progress-bar').show();
                $('#chatFileUploadProgress .progress-bar').css(
                    'width',
                    progress + '%'
                );
            }
        }).prop('disabled', !$.support.fileInput)
            .parent()
            .addClass($.support.fileInput ? undefined : 'disabled');

        //file upload
        $('#chatFileUpload').fileupload({
            url: url,
            dataType: 'json',
            maxFileSize: 999000,
            add: function (e, data) {
                var uploadErrors = [];
                if (data.originalFiles[0]['size'] > 10000000) {
                    uploadErrors.push('File size is too big (the maximum size is 10MB)');
                }
                if (uploadErrors.length > 0) {
                    abp.message.error(uploadErrors.join("\n"));
                } else {
                    data.submit();
                }
            },
            done: function (e, response) {
                var jsonResult = response.result;

                var chatMessage = '[file]{"id":"' + jsonResult.result.id + '", "name":"' + jsonResult.result.name + '", "contentType":"' + jsonResult.result.contentType + '"}';
                //$('#ChatMessage').val('[file]{"id":"' + jsonResult.result.id + '", "name":"' + jsonResult.result.name + '", "contentType":"' + jsonResult.result.contentType + '"}');
                //chat.sendMessage();
                addFile(jsonResult.result.id, jsonResult.result.name, jsonResult.result.size, chatMessage);
                $('.chat-progress-bar').hide();
            },
            progressall: function (e, data) {
                var progress = parseInt(data.loaded / data.total * 100, 10);
                $('.chat-progress-bar').show();
                $('#chatFileUploadProgress .progress-bar').css(
                    'width',
                    progress + '%'
                );
            }
        }).prop('disabled', !$.support.fileInput)
            .parent()
            .addClass($.support.fileInput ? undefined : 'disabled');

        function addFile(fileId, fileName, fileSize, chatMessage) {
            var template = $('#SendFileTemplate').html();
            Mustache.parse(template);
            var file = {
                fileId: fileId,
                fileName: fileName,
                fileSize: Math.round(fileSize / 1024) + 'KB',
                chatMessage: chatMessage
            };
            var sendFileHtml = Mustache.render(template, file);
            $('#SendFileHolder').append(sendFileHtml);
            $(window).trigger('resize');
        }

        $('#btnLinkShare').click(function () {
            $('#chatDropdownToggle').dropdown('toggle');
            $('#ChatMessage').val('[link]{"message":"' + window.location.href + '"}');
            chat.sendMessage();
        });

        $('.fileinput-button').click(function () {
            $('#chatDropdownToggle').dropdown('toggle');
        });

        $('#SendFileHolder').on('click', 'a', function (e) {
            // Remove file
            e.preventDefault();
            var $sendFileRow = $(this).parents('.send-file');
            var fileId = $sendFileRow.data('file-id');
            abp.ajax({
                url: $(this).attr('href') + '/?fileId=' + fileId
            }).done(function () {
                $sendFileRow.remove();
            });
        });


    });
})(jQuery);
