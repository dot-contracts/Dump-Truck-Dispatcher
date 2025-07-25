(function ($) {
    app.modals.ChangeDriverForOrderLineTruckModal = function () {
        var _modalManager;
        var _driverAssignmentService = abp.services.app.driverAssignment;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            let isExternal = _$form.find("#IsExternal").val() === "True";

            _$form.find("#DriverId").select2Init({
                abpServiceMethod: isExternal
                    ? abp.services.app.leaseHauler.getLeaseHaulerDriversSelectList
                    : abp.services.app.driver.getDriversSelectList,
                abpServiceParams: isExternal
                    ? { leaseHaulerId: _$form.find('#LeaseHaulerId').val() }
                    : undefined,
                showAll: true,
            });
        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var input = _$form.serializeFormToObject();
            input.ReplaceExistingDriver = false;

            if (input.LeaseHaulerId) {
                input.ReplaceExistingDriver = true;
            } else if (input.HasTicketsOrLoads === "False") {
                var answer = await swal(
                    "Do you want to replace the driver assigned to this truck for the day/shift, or do you want to add an additional driver to the truck for the day/shift?",
                    {
                        buttons: {
                            replaceExisting: "Replace Existing",
                            addAnother: "Add Another"
                        }
                    }
                );

                if (!answer) {
                    return;
                }

                if (answer === "replaceExisting") {
                    input.ReplaceExistingDriver = true;
                }
            }

            try {
                _modalManager.setBusy(true);
                await _driverAssignmentService.changeDriverForOrderLineTruck(input);

                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.changeDriverForOrderLineTruckModalSaved');
            }
            finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);