var app = app || {};
app.chat = app.chat || {};
(function () {

    if (!signalR) {
        return;
    }

    app.chat.sendMessage = async function (messageData) {

        var chatHub = abp.signalr.hubs.common;

        if (chatHub?.state !== signalR.HubConnectionState.Connected) {
            callback && callback();
            abp.notify.warn(app.localize('ChatIsNotConnectedWarning'));
            return;
        }

        try {
            let result = await chatHub.invoke('sendMessage', messageData);

            if (result) {
                abp.notify.warn(result);
            }
        } catch (e) {
            abp.notify.warn(abp.localization.abpWeb('InternalServerError'));
            throw e;
        }
    };

})();
