(function ($) {
    app.modals.AddTicketPhotoModal = function () {

        var _modalManager;
        var _$form = null;
        var _$fileInput = null;
        var _$saveButton = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            //_$form.validate();

            _$saveButton = _modalManager.getModal().find('.save-button');
            _$saveButton.prop('disabled', true);

            _$fileInput = _$form.find('#TicketPhoto');
            _$fileInput.change(function () {
                _$saveButton.prop('disabled', !_$fileInput.val());
            });
        };

        this.save = function () {
            _modalManager.setBusy(true);
            try {
                if (!abp.helper.validateTicketPhoto(_$fileInput)) {
                    return;
                }

                const file = _$fileInput[0].files[0];
                const reader = new FileReader();

                reader.addEventListener("load", function () {
                    _modalManager.setResult({
                        ticketPhoto: reader.result,
                        ticketPhotoFilename: file.name
                    });
                    _modalManager.close();
                }, false);

                reader.readAsDataURL(file);

            } finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);
