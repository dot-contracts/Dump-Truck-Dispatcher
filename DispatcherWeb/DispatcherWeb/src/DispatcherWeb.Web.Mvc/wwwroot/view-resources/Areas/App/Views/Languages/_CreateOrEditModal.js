(function ($) {
    app.modals.CreateOrEditLanguageModal = function () {

        var _modalManager;
        var _languageService = abp.services.app.language;
        var _$languageInformationForm = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _modalManager.getModal().find('#LanguageNameEdit').select2Init({
                showAll: false,
                allowClear: false
            });

            _modalManager.getModal().find('#LanguageIconEdit').select2Init({
                showAll: false,
                allowClear: false
            });

            _$languageInformationForm = _modalManager.getModal().find('form[name=LanguageInformationsForm]');
        };

        this.save = function () {
            var language = _$languageInformationForm.serializeFormToObject();

            _modalManager.setBusy(true);
            _languageService.createOrUpdateLanguage({
                language: language
            }).done(function () {
                abp.notify.info(app.localize('SavedSuccessfully'));
                _modalManager.close();
                abp.event.trigger('app.createOrEditLanguageModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);