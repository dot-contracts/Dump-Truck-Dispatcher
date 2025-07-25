(function () {
    $(function () {
        'use strict';

        var _dispatchingService = abp.services.app.dispatching;

        const form = $('#CompleteDispatchForm');
        form.submit(function (e) {
            e.preventDefault();

            if (!form.valid()) {
                form.showValidateMessage();
                return;
            }

            var formData = form.serializeFormToObject();

            if (formData.IsMultipleLoads === 'True') {
                $('#ContinueMultiloadPopup').modal('show');
            } else {
                save(formData);
            }
        });

        $('#ContinueMultiloadNoButton').click(function (e) {
            e.preventDefault();
            var formData = form.serializeFormToObject();
            formData.ContinueMultiload = false;
            save(formData);
        });

        $('#ContinueMultiloadYesButton').click(function (e) {
            e.preventDefault();
            var formData = form.serializeFormToObject();
            formData.ContinueMultiload = true;
            save(formData);
        });

        async function save(formData) {
            try {
                abp.ui.setBusy(form);
                let position = await abp.helper.getLocationAsync();
                if (position) {
                    formData.destinationLatitude = position.coords.latitude;
                    formData.destinationLongitude = position.coords.longitude;
                }

                let result = await _dispatchingService.completeDispatch(formData);

                if (result.isCanceled) {
                    abp.notify.info('The Dispatch is canceled by dispatcher.');
                    window.location.reload();
                } else if (result.isCompleted) {
                    abp.notify.info('The Dispatch is already completed.');
                    window.location.reload();
                } else if (result.notFound) {
                    abp.notify.info('The Dispatch was not found.');
                    window.location.reload();
                } else {
                    abp.notify.info('Saved successfully.');
                    var redirectUrl = abp.appPath.slice(0, -1);
                    if (result.nextDispatchId) {
                        redirectUrl += result.nextDispatchId;
                    } else {
                        redirectUrl += form.data('url-completed');
                    }
                    window.location = redirectUrl;
                }
            } finally {
                abp.ui.clearBusy(form);
            }
        }

    });
})();

