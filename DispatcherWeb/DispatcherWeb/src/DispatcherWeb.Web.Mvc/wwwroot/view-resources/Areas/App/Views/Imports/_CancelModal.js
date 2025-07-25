(function ($) {
    app.modals.CancelModal = function () {

        var _modalManager;

        this.init = function (modalManager) {
            _modalManager = modalManager;

        };

        this.save = function () {
            _modalManager.close();
            abp.event.trigger('app.UploadCanceled');
        };
    };
})(jQuery);