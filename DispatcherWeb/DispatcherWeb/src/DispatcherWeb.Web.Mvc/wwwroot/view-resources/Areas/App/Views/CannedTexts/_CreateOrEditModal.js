(function ($) {
    app.modals.CreateOrEditCannedTextModal = function () {

        var _modalManager;
        var _cannedTextService = abp.services.app.cannedText;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find("#OfficeId").select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: false
            });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var record = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _cannedTextService.editCannedText(record).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditCannedTextModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
