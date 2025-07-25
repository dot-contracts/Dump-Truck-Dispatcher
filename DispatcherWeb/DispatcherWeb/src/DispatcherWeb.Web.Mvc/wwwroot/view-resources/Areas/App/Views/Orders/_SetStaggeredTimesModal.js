(function ($) {
    app.modals.SetStaggeredTimesModal = function () {

        var _modalManager;
        var _orderAppService = abp.services.app.order;
        var _$form = null;
        var _model = null;
        var _initializing = false;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find('#FirstStaggeredTimeOnJob').timepickerInit({ stepping: 1 });

            _$form.find('[name="StaggeredTimeKind"]').change(function () {
                var staggeredTimeKind = Number(_$form.find('[name="StaggeredTimeKind"]:checked').val());
                $("#StaggeredTimeInterval, #FirstStaggeredTimeOnJob").closest('.form-group').toggle(staggeredTimeKind === abp.enums.staggeredTimeKind.setInterval);
            });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var model = _$form.serializeFormToObject();

            _model = _model || {};
            _model.orderLineId = model.OrderLineId ? Number(model.OrderLineId) : null;
            _model.staggeredTimeKind = Number(model.StaggeredTimeKind);
            _model.staggeredTimeInterval = Number(model.StaggeredTimeInterval); //int
            _model.firstStaggeredTimeOnJob = model.FirstStaggeredTimeOnJob;

            if (this.saveCallback) {
                this.saveCallback(_model);
            }

            if (model.OrderLineId) {
                _modalManager.setBusy(true);
                _orderAppService.setStaggeredTimes(model).done(function (e) {
                    abp.notify.info('Saved successfully.');
                    _modalManager.close();
                    abp.event.trigger('app.staggeredTimesSetModal', e);
                }).always(function () {
                    _modalManager.setBusy(false);
                });
            } else {
                _modalManager.close();
                abp.event.trigger('app.staggeredTimesSetModal', _model);
            }

        };

        this.setModel = function (model) {
            _model = model;
            if (!_$form) {
                return;
            }
            _initializing = true;
            _$form.find('#OrderLineId').val(_model.orderLineId);
            _$form.find('input[name="StaggeredTimeKind"]').filter('[value=' + _model.staggeredTimeKind + ']').prop('checked', true).change();
            _$form.find('#StaggeredTimeInterval').val(_model.staggeredTimeInterval);
            _$form.find('#FirstStaggeredTimeOnJob').val(_model.firstStaggeredTimeOnJob).change();
            _initializing = false;
        };

        this.saveCallback = null;
    };
})(jQuery);
