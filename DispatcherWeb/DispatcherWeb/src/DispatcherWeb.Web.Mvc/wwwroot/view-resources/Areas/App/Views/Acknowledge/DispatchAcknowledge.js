(function () {
    $(function () {
        'use strict';

        var _dispatchingService = abp.services.app.dispatching;

        $('form').submit(function (e) {
            e.preventDefault();

            var form = $(this);
            if (!form.valid()) {
                form.showValidateMessage();
                return;
            }
            save();

            function save() {
                abp.ui.setBusy(form);
                _dispatchingService.acknowledgeDispatch({ DispatchId: form.find('#DispatchId').val() })
                    .done(function () {
                        abp.notify.info('Saved successfully.');
                        window.location = window.location.href.split('?')[0];
                    })
                    .always(function () {
                        abp.ui.clearBusy(form);
                    });
            }

        });

    });
})();