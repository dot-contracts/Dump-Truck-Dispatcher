(function () {
    $(function () {
        'use strict';

        const _dispatchingService = abp.services.app.dispatching;

        const _addTicketPhotoModal = new app.ModalManager({
            viewUrl: abp.appPath + 'App/Dispatches/AddTicketPhotoModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Dispatches/_AddTicketPhotoModal.js',
            modalClass: 'AddTicketPhotoModal'
        });

        const form = $('#LoadDispatchForm');
        form.submit(async function (e) {
            e.preventDefault();

            if (!form.valid()) {
                form.showValidateMessage();
                return;
            }

            try {
                abp.ui.setBusy(form);
                var formData = form.serializeFormToObject();

                if (!validateTicketFields(formData)) {
                    abp.ui.clearBusy(form);
                    return;
                }

                let position = await abp.helper.getLocationAsync();
                if (position) {
                    formData.sourceLatitude = position.coords.latitude;
                    formData.sourceLongitude = position.coords.longitude;
                }

                await _dispatchingService.loadDispatch(formData);

                abp.notify.info('Saved successfully.');
                window.location = window.location.href.split('?')[0];
            } finally {
                abp.ui.clearBusy(form);
            }
        });

        $("#MaterialItemId").select2Init({
            abpServiceMethod: listCacheSelectLists.item(),
            abpServiceParamsGetter: (params) => ({
                types: abp.enums.itemTypes.material,
            }),
            showAll: listCache.item.isEnabled,
            allowClear: true,
        });

        $('#CreateNewTicketButton').click(function () {
            var createNewTicketOldValue = $('#CreateNewTicket').val() === "True";
            var createNewTicket = !createNewTicketOldValue;
            if (createNewTicket) {
                $('#TicketNumberLabel').removeClass('required-label');
            } else {
                $('#TicketNumberLabel').addClass('required-label');
            }
            $('#TicketNumber').prop('disabled', createNewTicket);
            $('#CreateNewTicket').val(createNewTicket ? "True" : "False");
        });

        $(".TakeTicketPhotoButton").click(async function (e) {
            e.preventDefault();
            let result = await app.getModalResultAsync(
                _addTicketPhotoModal.open()
            );

            $("#TicketPhotoBase64").val(result.ticketPhoto);
            $("#TicketPhotoFilename").val(result.ticketPhotoFilename);
            $("#TicketPhotoSelectedMessage").slideDown(function () { $(this).show(); });
            $(".TakeTicketPhotoButton").prop('disabled', true);
        });

        function validateTicketFields(ticket) {
            var isMaterialQuantityValid = true;

            if ($('#RequireTicket').val() === "True") {
                if ($('#MaterialQuantity').is(':visible')) {
                    if (ticket.MaterialQuantity === '' || ticket.MaterialQuantity === null || !(Number(ticket.MaterialQuantity) > 0)) {
                        isMaterialQuantityValid = false;
                    }
                }
            }

            if (!isMaterialQuantityValid) {
                abp.message.error('Please check the following: \n'
                    + (isMaterialQuantityValid ? '' : '"Material Quantity" - This field is required.\n'), 'Some of the data is invalid');
                return false;
            }

            return true;
        }
    });
})();
