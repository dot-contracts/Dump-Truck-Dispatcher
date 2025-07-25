// Duplicating functionality. See the localStorage.localForage.js

//var app = app || {};
//(function () {

//    app.localStorage = app.localStorage || {};

//    app.localStorage.setItem = function (key, value) {
//        if (!localStorage) {
//            return;
//        }

//        localStorage.setItem(key, JSON.stringify(value));
//    };

//    app.localStorage.getItem = function (key, callback) {
//        console.log('getItem');
//        if (!localStorage) {
//            console.log('null');
//            return null;
//        }

//        var value = localStorage.getItem(key);
//        console.log('getItem', key, value);
//        if (callback) {
//            callback(value);
//        } else {
//            return value;
//        }
//    };

//    app.localStorage.removeItem = function(key) {
//        if (!localStorage) {
//            return;
//        }
//        console.log('remove', key);
//        localStorage.removeItem(key);
//    }

//})();
