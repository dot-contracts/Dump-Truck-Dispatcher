var app = app || {};
(function ($) {

    var _loadedScripts = [];

    app.modals = app.modals || {};

    app.getModalResultAsync = function (modalPromise) {
        return new Promise((resolve, reject) => {
            modalPromise.then((modal, modalObject, modalResultPromise) => {
                modal.on('hidden.bs.modal', () => {
                    reject('Modal was closed without saving');
                });
                modalResultPromise.then(resolve, reject);
            }, (e) => {
                reject(e);
            })
        });
    };

    app.ModalManager = (function () {

        var _normalizeOptions = function (options) {
            if (!options.modalId) {
                options.modalId = 'Modal_' + Math.floor(Math.random() * 1000000) + new Date().getTime();
            }
            if (!options.modalSize)
                options.modalSize = 'md';
        };

        function _removeContainer(modalId) {
            var _containerId = modalId + 'Container';
            var _containerSelector = '#' + _containerId;

            var $container = $(_containerSelector);
            if ($container.length) {
                $container.remove();
            }
        }

        function _createContainer(modalId, modalSize) {
            _removeContainer(modalId);

            var _containerId = modalId + 'Container';
            return $('<div id="' + _containerId + '"></div>')
                .append(
                    '<div id="' + modalId + '" class="modal fade" tabindex="-1" role="modal">' +
                    '  <div class="modal-dialog modal-' + modalSize + '">' +
                    '    <div class="modal-content"></div>' +
                    '  </div>' +
                    '</div>'
                ).appendTo('body');
        }

        return function (options) {

            _normalizeOptions(options);

            var _options = options;
            var _$modal = null;
            var _modalId = options.modalId;
            var _modalSelector = '#' + _modalId;
            var _modalObject = null;

            var _publicApi = null;
            var _args = null;
            var _getResultCallback = null;

            var _onCloseCallbacks = [];
            var _onCloseOnceCallbacks = [];

            var _onOpenCallbacks = [];
            var _onOpenOnceCallbacks = [];

            var _modalResultPromise = null;

            function _saveModal() {
                if (_modalObject && _modalObject.save) {
                    _modalObject.save();
                }
            }

            async function _initAndShowModal(openResultDeferred) {
                _$modal = $(_modalSelector);

                _$modal.modal({
                    backdrop: 'static'
                });

                _$modal.on('hidden.bs.modal', function () {
                    _removeContainer(_modalId);

                    for (let i = 0; i < _onCloseCallbacks.length; i++) {
                        _onCloseCallbacks[i]();
                    }

                    for (let i = 0; i < _onCloseOnceCallbacks.length; i++) {
                        _onCloseOnceCallbacks[i]();
                    }

                    if (_modalResultPromise) {
                        _modalResultPromise.reject();
                    }

                    _onCloseOnceCallbacks = [];
                });

                _$modal.on('shown.bs.modal', function () {
                    if (_options.getDefaultFocusElement) {
                        _options.getDefaultFocusElement(_$modal).focus();
                    } else if (_modalObject && _modalObject.focusOnDefaultElement) {
                        _modalObject.focusOnDefaultElement(_$modal);
                    } else {
                        _$modal.find('input:not([type=hidden]):first').focus();
                    }
                    for (let i = 0; i < _onOpenCallbacks.length; i++) {
                        _onOpenCallbacks[i]();
                    }
                    for (let i = 0; i < _onOpenOnceCallbacks.length; i++) {
                        _onOpenOnceCallbacks[i]();
                    }
                    _onOpenOnceCallbacks = [];
                    openResultDeferred.resolveWith(_publicApi, [_$modal, _modalObject, _modalResultPromise]);
                });

                var modalClass = app.modals[options.modalClass];
                if (modalClass) {
                    _modalObject = new modalClass();
                    if (_modalObject.init) {
                        await Promise.resolve(_modalObject.init(_publicApi, _args));
                    }
                }

                _$modal.find('.save-button').click(function () {
                    let button = $(this);
                    if (button.data('is-busy')) {
                        return;
                    }
                    button.data('is-busy', true);
                    setTimeout(() => { button.data('is-busy', false); }, 500);

                    _saveModal();
                });

                _$modal.modal('show');
            }

            var _open = function (args, getResultCallback) {

                _args = args || {};
                _getResultCallback = getResultCallback;
                _modalResultPromise = $.Deferred();

                var openResultDeferred = $.Deferred();

                _loadModal(openResultDeferred);

                return openResultDeferred.promise();
            };

            var _loadModal = async function (openResultDeferred) {
                let container = _createContainer(_modalId, _options.modalSize);
                const clearBusy = abp.ui.withBusy();
                try {
                    try {
                        let modalContent = await $.ajax({
                            url: options.viewUrl,
                            data: _args,
                        });

                        container.find('.modal-content').html(modalContent);

                        if (options.scriptUrl && !_loadedScripts.includes(options.scriptUrl)) {
                            await $.ajax({
                                url: options.scriptUrl,
                                dataType: "script",
                                crossDomain: true,
                            });
                            _loadedScripts.push(options.scriptUrl);
                        }
                    } catch (xhr) {
                        let response = xhr.responseText;
                        let responseObject = null;
                        if (response && response.length && response[0] === '{') {
                            try {
                                responseObject = JSON.parse(response);
                            } catch (e) { /**/ }
                        }

                        clearBusy();
                        if (responseObject && responseObject.userFriendlyException) {
                            var e = responseObject.userFriendlyException;
                            await abp.message.warn(e.details, e.message);
                        } else if (xhr.status === 401) {
                            await abp.message.warn(app.localize('CurrentUserDidNotLoginToTheApplication'));
                            window.location = abp.appPath + 'Account/Login';
                            return;
                        } else {
                            await abp.message.warn(abp.localization.abpWeb('InternalServerError'));
                        }

                        throw xhr;
                    }

                    await _initAndShowModal(openResultDeferred);

                } catch (e) {
                    openResultDeferred.rejectWith(_publicApi, [e]);
                    throw e;
                } finally {
                    clearBusy();
                }
            }

            var _close = function () {
                if (!_$modal) {
                    return;
                }

                _$modal.modal('hide');
            };

            var _onClose = function (onCloseCallback) {
                _onCloseCallbacks.push(onCloseCallback);
            };

            var _onCloseOnce = function (onCloseCallback) {
                _onCloseOnceCallbacks.push(onCloseCallback);
            };

            var _onOpen = function (onOpenCallback) {
                _onOpenCallbacks.push(onOpenCallback);
            };

            var _onOpenOnce = function (onOpenCallback) {
                _onOpenOnceCallbacks.push(onOpenCallback);
            };

            var _on = function (eventName, callback) {
                abp.event.on(eventName, callback);
                _onClose(function () {
                    abp.event.off(eventName, callback);
                });
            };

            function _setBusy(isBusy) {
                if (!_$modal) {
                    return;
                }

                _$modal.find('.modal-footer button').buttonBusy(isBusy);
            }

            _publicApi = {

                open: _open,

                reopen: function () {
                    _open(_args);
                },

                close: _close,

                getModalId: function () {
                    return _modalId;
                },

                getModal: function () {
                    return _$modal;
                },

                getModalContent: function () {
                    return _$modal.find('.modal-content');
                },

                getArgs: function () {
                    return _args;
                },

                getOptions: function () {
                    return _options;
                },

                setBusy: _setBusy,

                setResult: function () {
                    _getResultCallback && _getResultCallback.apply(_publicApi, arguments);
                    _modalResultPromise && _modalResultPromise.resolveWith(_publicApi, arguments);
                },

                onClose: _onClose,

                onCloseOnce: _onCloseOnce,

                onOpen: _onOpen,

                onOpenOnce: _onOpenOnce,

                on: _on
            };

            return _publicApi;

        };
    })();

    //todo move to the helper after merging the refactoring PRs
    abp.ui.withBusy = function (element, acceptableDelay = 100) {
        //acceptableDelay is time in ms during which the busy indicator won't be shown, i.e. for quick enough actions
        let busyTimeout;
        let isBusy = false;

        const start = () => {
            busyTimeout = setTimeout(() => {
                isBusy = true;
                abp.ui.setBusy(element);
            }, acceptableDelay);
        };

        const stop = () => {
            clearTimeout(busyTimeout);
            if (isBusy) {
                abp.ui.clearBusy(element);
            }
        };

        start();
        return stop;
    }

})(jQuery);
