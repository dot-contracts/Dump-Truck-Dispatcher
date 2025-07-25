var abp = abp || {};
(function () {

    // Check if SignalR is defined
    if (!signalR) {
        return;
    }

    // Create namespaces
    abp.signalr = abp.signalr || {};
    abp.signalr.hubs = abp.signalr.hubs || {};
    abp.signalr.reconnectTime = abp.signalr.reconnectTime || 5000;
    abp.signalr.maxTries = abp.signalr.maxTries || 8;
    abp.signalr.increaseReconnectTime = abp.signalr.increaseReconnectTime || function (time) {
        return time * 2;
    };

    // Configure the connection for abp.signalr.hubs.common
    function configureConnection(connection) {
        // Set the common hub
        abp.signalr.hubs.common = connection;

        let tries = 1;
        let reconnectTime = abp.signalr.reconnectTime;

        // Reconnect loop
        function tryReconnect() {
            if (tries <= abp.signalr.maxTries) {
                connection.start()
                    .then(function () {
                        reconnectTime = abp.signalr.reconnectTime;
                        tries = 1;
                        abp.event.trigger('abp.signalr.reconnected');
                        register(connection);
                    }).catch(function () {
                        tries += 1;
                        reconnectTime = abp.signalr.increaseReconnectTime(reconnectTime);
                        setTimeout(function () {
                            tryReconnect();
                        }, reconnectTime);
                    });
            }
        }

        // Reconnect if hub disconnects
        connection.onclose(function (e) {
            if (e) {
                abp.log.debug('Connection closed with error: ' + e);
            } else {
                //abp.log.debug('Disconnected');
            }

            stopHeartbeat();

            if (!abp.signalr.autoReconnect) {
                return;
            }

            abp.event.trigger('abp.signalr.disconnected');
            tryReconnect();
        });

        // Register to get notifications
        connection.on('getNotification', function (notification) {
            abp.event.trigger('abp.notifications.received', notification);
        });

        connection.on('syncRequest', function (syncRequest) {
            abp.event.trigger('abp.signalR.receivedSyncRequest', syncRequest);
        });

        connection.on('listCacheSyncRequest', function (listCacheSyncRequest) {
            abp.event.trigger('abp.signalR.receivedListCacheSyncRequest', listCacheSyncRequest);
        });

        connection.on('debugMessage', function (debugMessage) {
            abp.log.log(debugMessage.message, debugMessage.logLevel);
            abp.event.trigger('abp.signalR.debugMessage', debugMessage);
        });

        registerChatEvents(connection);
    }

    // Connect to the server for abp.signalr.hubs.common
    function connect() {
        var url = abp.signalr.url || (abp.appPath + 'signalr');

        // Start the connection
        startConnection(url, configureConnection)
            .then(function (connection) {
                abp.event.trigger('abp.signalr.connected');
                // Call the Register method on the hub
                register(connection);
            })
            .catch(function (error) {
                abp.log.debug(error.message);
            });
    }

    async function register(connection) {
        await connection.invoke('register');
        startHeartbeat();
        abp.event.trigger('abp.signalr.registered');
        abp.event.trigger('app.chat.connected');
    }

    var heartbeatIntervalId;

    function startHeartbeat() {
        if (!abp.signalr.heartbeatInterval) {
            return;
        }

        heartbeat();
        heartbeatIntervalId = setInterval(heartbeat, abp.signalr.heartbeatInterval * 1000);
    }

    function stopHeartbeat() {
        if (heartbeatIntervalId) {
            clearInterval(heartbeatIntervalId);
            heartbeatIntervalId = undefined;
        }
    }

    async function heartbeat() {
        try {
            if (!abp.signalr.hubs.common) {
                return;
            }
            await abp.signalr.hubs.common.invoke('heartbeat');
        } catch (e) {
            console.error('Error in heartbeat', e);
        }
    };

    function registerChatEvents(connection) {
        connection.on('getChatMessage', function (message) {
            abp.event.trigger('app.chat.messageReceived', message);
        });

        connection.on('getAllFriends', function (friends) {
            abp.event.trigger('abp.chat.friendListChanged', friends);
        });

        connection.on('getFriendshipRequest', function (friendData, isOwnRequest) {
            abp.event.trigger('app.chat.friendshipRequestReceived', friendData, isOwnRequest);
        });

        connection.on('getUserConnectNotification', function (friend, isConnected) {
            abp.event.trigger('app.chat.userConnectionStateChanged',
                {
                    friend: friend,
                    isConnected: isConnected
                });
        });

        connection.on('getUserStateChange', function (friend, state) {
            abp.event.trigger('app.chat.userStateChanged',
                {
                    friend: friend,
                    state: state
                });
        });

        connection.on('getallUnreadMessagesOfUserRead', function (friend) {
            abp.event.trigger('app.chat.allUnreadMessagesOfUserRead',
                {
                    friend: friend
                });
        });

        connection.on('getReadStateChange', function (friend) {
            abp.event.trigger('app.chat.readStateChange',
                {
                    friend: friend
                });
        });
    }

    abp.signalr.onConnect = function (callback) {
        if (abp.signalr.hubs.common?.state?.toLowerCase() === 'connected') {
            callback();
        }
        abp.event.on('abp.signalr.registered', () => {
            callback();
        });
    };

    abp.signalr.subscribeToSyncRequests = function subscribeToSyncRequests() {
        abp.signalr.onConnect(() => {
            abp.signalr.hubs.common.invoke('SubscribeToSyncRequestsWithFilter', {});
        });
    };

    var subscribedToListCacheSyncRequests = false;
    abp.signalr.subscribeToListCacheSyncRequests = function subscribeToListCacheSyncRequests() {
        if (subscribedToListCacheSyncRequests) {
            return;
        }
        subscribedToListCacheSyncRequests = true;

        abp.signalr.onConnect(() => {
            abp.signalr.hubs.common.invoke('SubscribeToListCacheSyncRequests');
        });
    };

    // Starts a connection with transport fallback - if the connection cannot be started using
    // the webSockets transport the function will fallback to the serverSentEvents transport and
    // if this does not work it will try longPolling. If the connection cannot be started using
    // any of the available transports the function will return a rejected Promise.
    function startConnection(url, configureConnection) {
        if (abp.signalr.remoteServiceBaseUrl) {
            url = abp.signalr.remoteServiceBaseUrl + url;
        }

        // Add query string: https://github.com/aspnet/SignalR/issues/680
        if (abp.signalr.qs) {
            url += (url.indexOf('?') == -1 ? '?' : '&') + abp.signalr.qs;
        }

        return function start(transport) {
            var connection = new signalR.HubConnectionBuilder()
                .withUrl(url, transport)
                .build();

            if (configureConnection && typeof configureConnection === 'function') {
                configureConnection(connection);
            }

            return connection.start()
                .then(function () {
                    return connection;
                })
                .catch(function (error) {
                    if (transport !== signalR.HttpTransportType.LongPolling) {
                        return start(transport + 1);
                    }

                    return Promise.reject(error);
                });
        }(signalR.HttpTransportType.WebSockets);
    }

    abp.signalr.autoConnect = abp.signalr.autoConnect === undefined ? true : abp.signalr.autoConnect;
    abp.signalr.autoReconnect = abp.signalr.autoReconnect === undefined ? true : abp.signalr.autoReconnect;
    abp.signalr.connect = abp.signalr.connect || connect;
    abp.signalr.startConnection = abp.signalr.startConnection || startConnection;

    if (abp.signalr.autoConnect && !abp.signalr.hubs.common) {
        abp.signalr.connect();
    }
})();
