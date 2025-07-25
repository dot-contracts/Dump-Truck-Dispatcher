(function ($) {
    app.modals.ViewMessageModal = function () {

        var _modalManager;
        var _driverMessageAppService = abp.services.app.driverMessage;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();



        };

    };
})(jQuery);