(function ($) {
    app.modals.SendMessageModal = function () {

        var _modalManager;
        var _driverMessageAppService = abp.services.app.driverMessage;
        var _driverAppService = abp.services.app.driver;
        var _$form = null;
        var _$driversDropdown = null;
        var _$officesDropdown = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$driversDropdown = _$form.find("#DriverIds");
            _$officesDropdown = _$form.find("#OfficeIds");
            var orderLineId = _$form.find('#OrderLineId').val();
            if (orderLineId) {
                _$driversDropdown.select2Init({
                    showAll: true,
                    allowClear: false
                });
            } else {
                _$driversDropdown.select2Init({
                    abpServiceMethod: abp.services.app.driver.getDriversSelectList,
                    abpServiceParams: {
                        orderNotifyPreferredFormats: abp.enums.orderNotifyPreferredFormats.notNeither,
                    },
                    showAll: true,
                    allowClear: false
                });
            }
            _$officesDropdown.select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: false
            });

            _$officesDropdown.change(function () {
                var officeIds = $(this).val();
                _$driversDropdown.prop('disabled', officeIds.length > 0);
                _$driversDropdown.closest('.form-group').find('label').toggleClass('required-label', officeIds.length === 0);
            });

            _$driversDropdown.change(function () {
                var driverIds = $(this).val();
                _$officesDropdown.prop('disabled', driverIds.length > 0);
                _$officesDropdown.closest('.form-group').find('label').toggleClass('required-label', driverIds.length === 0);
            }).change();

        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var model = _$form.serializeFormWithMultipleToObject();
            model.DriverIds = _$driversDropdown.val();
            model.OfficeIds = _$officesDropdown.val();

            if (!model.DriverIds.length && !model.OfficeIds.length) {
                abp.message.error('Please check the following: \n'
                    + ('"Drivers" - This field is required.\n'), 'Some of the data is invalid');
                return;
            }

            //if (!$.isArray(model.DriverIds)) {
            //    model.DriverIds = [model.DriverIds];
            //}
            //if (model.DriverIds.length === 0) {
            //    delete model.DriverIds;
            //}

            _modalManager.setBusy(true);
            _driverMessageAppService.sendMessage(model).done(function () {
                abp.notify.info('Your message was scheduled to be sent.');
                _modalManager.close();
                abp.event.trigger('app.sendMessageModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
