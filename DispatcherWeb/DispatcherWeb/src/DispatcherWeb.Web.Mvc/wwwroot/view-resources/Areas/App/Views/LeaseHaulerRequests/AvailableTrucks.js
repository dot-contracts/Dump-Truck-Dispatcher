(function () {
    $(function () {

        var _leaseHaulerRequestEditService = abp.services.app.leaseHaulerRequestEdit;

        var saveAvailableTrucksAsync = function (callback) {
            var form = $("#AvailableTrucksForm");
            if (!form.valid()) {
                form.showValidateMessage();
                return;
            }
            var model = form.serializeFormToObject();
            abp.ui.setBusy(form);
            _leaseHaulerRequestEditService.editAvailableTrucks(model).done(function () {
                abp.notify.info('Saved successfully.');
                if ($('#HasAvailableBeenSent').val() !== 'True' && window.thankYouForTrucksTemplate) {
                    var message = window.thankYouForTrucksTemplate.replace("{NumberOfTrucks}", model.Available);
                    abp.message.success(message)
                        .done(function () {
                            location.reload();
                        });
                }
                if (callback)
                    callback();
            }).always(function () {
                abp.ui.clearBusy(form);
            });
        };

        $("#SaveAvailableTrucksButton").click(function (e) {
            e.preventDefault();
            saveAvailableTrucksAsync();
        });

        $leaseHaulerTruckSelect = $('.lease-hauler-truck-select');
        $leaseHaulerTruckSelect.each(function () {
            var $select = $(this);
            $select.select2Init({
                abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulerTrucksSelectList,
                abpServiceParams: { leaseHaulerId: $('#LeaseHaulerId').val() },
                showAll: true
            });

            $select.change(async function () {
                if ($select.attr('data-truck-isinuse') === 'True' && $select.attr('data-truck-originalid') !== $select.val()) {
                    if (await abp.message.confirm(
                        'This truck has already been scheduled and will be removed from the schedule if you click Yes.'
                    )) {
                        $select.removeAttr('data-truck-isinuse').removeAttr('data-truck-originalid');
                    } else {
                        $select.val($select.attr('data-truck-originalid')).change();
                    }
                }
            });
        });

        $('.lease-hauler-driver-select').each(function () {
            $(this).select2Init({
                abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulerDriversSelectList,
                abpServiceParams: { leaseHaulerId: $('#LeaseHaulerId').val() },
                showAll: true
            });
        });


    });
})();