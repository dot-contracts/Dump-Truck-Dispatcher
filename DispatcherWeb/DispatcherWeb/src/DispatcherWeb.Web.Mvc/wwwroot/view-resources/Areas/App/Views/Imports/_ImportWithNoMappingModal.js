(function ($) {
    app.modals.ImportWithNoMappingModal = function () {

        var _modalManager;
        var _$form;
        var _importScheduleService = abp.services.app.importSchedule;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$importButton = _modalManager.getModal().find('.save-button');
            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _modalManager.getModal().find('.close-button').click(function (e) {
                e.preventDefault();
                var model = _$form.serializeFormToObject();
                abp.ajax({
                    url: $(this).data('delete-url'),
                    data: { blobName: model.BlobName },
                    contentType: 'application/x-www-form-urlencoded; charset=UTF-8'
                });
                _modalManager.close();
            });

        };

        this.save = function () {
            if (!_$form.valid()) {
                return;
            }
            var model = _$form.serializeFormToObject();
            _modalManager.setBusy(true);
            _importScheduleService.scheduleImport(model).done(function () {
                abp.notify.info("The file is scheduled for importing. You will receive a notification on completion.");
                _modalManager.close();
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };

    };

})(jQuery);