(function ($) {
    app.modals.SetOrderLineNoteModal = function () {

        var _modalManager;
        var _schedulingService = abp.services.app.scheduling;
        var _$form = null;
        var _model = null;
        var _initializing = false;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var model = _$form.serializeFormToObject();

            _model = _model || {};
            _model.orderLineId = model.OrderLineId ? Number(model.OrderLineId) : null;
            _model.note = model.Note;

            if (this.saveCallback) {
                this.saveCallback(_model);
            }

            if (model.OrderLineId) {
                _modalManager.setBusy(true);
                _schedulingService.setOrderLineNote({
                    orderLineId: model.OrderLineId,
                    note: model.Note
                }).done(function () {
                    abp.notify.info('Saved successfully.');
                    _modalManager.close();
                    abp.event.trigger('app.orderLineNoteModalSaved');
                }).always(function () {
                    _modalManager.setBusy(false);
                });
            } else {
                _modalManager.close();
                abp.event.trigger('app.orderLineNoteModalSaved');
            }
        };

        this.setModel = function (model) {
            _model = model;
            if (!_$form) {
                return;
            }
            _initializing = true;
            _$form.find('#OrderLineId').val(_model.orderLineId);
            _$form.find('#Note').val(_model.note);
            _initializing = false;
        };

        this.saveCallback = null;
    };
})(jQuery);
