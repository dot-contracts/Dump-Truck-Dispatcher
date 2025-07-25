(function ($) {
    app.modals.TripsReportModal = function () {

        var _modalManager;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();
        }

    };
})(jQuery);