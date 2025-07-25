(function ($) {

    app.modals.DuplicateDispatchMessageModal = function () {

        var _modalManager;
        var _dispatchingService = abp.services.app.dispatching;
        var _$form = null;
        var _$truckDriversSelect;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find("#NumberOfDispatches")
                .on("keydown", function (e) {
                    if (e.key === ".") {
                        e.preventDefault();
                        return false;
                    }
                })
                .on("blur", function (e) {
                    if (!$(this).val()) {
                        $(this).val('1');
                        _$form.valid();
                    }
                });
        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            var dispatchTruckStatus = await _dispatchingService.getDispatchTruckStatus(formData.DispatchId);
            if (!dispatchTruckStatus) {
                if (!await swal(
                    'The truck on this dispatch is either out of service or has no driver. Are you sure you want to duplicate this dispatch?',
                    {
                        buttons: ['No', 'Yes']
                    }
                )) {
                    return;
                }
            }

            _modalManager.setBusy(true);
            _dispatchingService.duplicateDispatch(formData).done(function () {
                abp.notify.info('Duplicated successfully.');
                _modalManager.close();
                abp.event.trigger('app.duplicateDispatchMessageModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };

    };
})(jQuery);