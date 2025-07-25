var app = app || {};
(function () {

    app.localStorage = app.localStorage || {};

    app.localStorage.setItem = function (key, value) {
        if (!localforage) {
            return;
        }

        localforage.setItem(key, value);
    };

    app.localStorage.getItem = function (key, callback) {
        if (!localforage || !callback) {
            return;
        }

        localforage.getItem(key)
            .then(function (value) {
                callback(value);
            });
    };

    app.localStorage.removeItem = function (key) {
        if (!localforage) {
            return;
        }
        localforage.removeItem(key);
    }

    app.localStorage.clear = function(callback) {
        if (!localforage) {
            return;
        }
        localforage.clear()
            .then(function() {
                if (callback) {
                    callback();
                }
            });
    }

})();
