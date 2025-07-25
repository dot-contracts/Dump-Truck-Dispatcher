(function ($) {
    app.modals.SetTractorForTrailerModal = function () {

        var _modalManager;
        var _trailerAssignmentService = abp.services.app.trailerAssignment;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();
            
            _$form.find('#TractorId').select2Init({
                abpServiceMethod: abp.services.app.truck.getActiveTractorsSelectList,
                showAll: true,
                allowClear: true
            });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _trailerAssignmentService.setTractorForTrailer({
                date: formData.Date,
                shift: formData.Shift,
                officeId: formData.OfficeId,
                tractorId: formData.TractorId,
                trailerId: formData.TrailerId,
            }).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.defaultDriverForTruckModalSet');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
