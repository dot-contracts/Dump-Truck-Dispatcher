(function ($) {
    app.modals.SendLeaseHaulerRequestModal = function () {

        var _modalManager;
        var _leaseHaulerRequestSendAppService = abp.services.app.leaseHaulerRequestSend;
        var _insuranceAppService = abp.services.app.insurance;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find('#Date').datepickerInit();
            _$form.find('#Shift').select2Init({ allowClear: false });
            _$form.find('#LeaseHaulerIds').select2Init({
                abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulersSelectList,
                allowClear: false
            });
            _$form.find('#OfficeId').select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: false
            });
            abp.helper.ui.addAndSetDropdownValue(_$form.find('#OfficeId'),
                abp.session.officeId,
                abp.session.officeName);

            abp.helper.ui.initCannedTextLists();
            _$form.find('#LeaseHaulerIds').on('select2:select', function (e) {
                var leaseHaulerId = e.params.data.id;
                if (leaseHaulerId) {
                    checkInsuranceExpiry(leaseHaulerId);
                }
            });

        };

        async function checkInsuranceExpiry(leaseHaulerId) {
            const insurances = await _insuranceAppService.getActiveInsurances(leaseHaulerId);

            if (!insurances.length) {
                const isConfirmed = await abp.message.confirmWithOptions({
                    text: "This lease hauler doesn't have insurance. Are you sure you want to use this lease hauler?",
                    title: "Are you sure?",
                    buttons: ['No', 'Yes']
                });

                if (!isConfirmed) {
                    removeLeaseHauler(leaseHaulerId);
                    return;
                }
                return;
            }

            const expiredInsurance = abp.helper.getExpiredInsuranceTypes(insurances);
            if (expiredInsurance) {
                const isConfirmed = await abp.message.confirmWithOptions({
                    text: "Insurance or certification expired. Are you sure you want to use this lease hauler?",
                    title: "Are you sure?",
                    buttons: ['No', 'Yes']
                });

                if (!isConfirmed) {
                    removeLeaseHauler(leaseHaulerId);
                    return;
                }
            }
        }

        function removeLeaseHauler(leaseHaulerId) {
            const selectedItems = _$form.find('#LeaseHaulerIds').val();
            const index = selectedItems.indexOf(leaseHaulerId);
            if (index !== -1) {
                selectedItems.splice(index, 1);
                _$form.find('#LeaseHaulerIds').val(selectedItems).trigger('change');
            }
        }

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormWithMultipleToObject();
            if (!$.isArray(formData.LeaseHaulerIds)) {
                formData.LeaseHaulerIds = [formData.LeaseHaulerIds];
            }

            _modalManager.setBusy(true);
            _leaseHaulerRequestSendAppService.sendRequests(formData).done(function (result) {
                if (result) {
                    abp.notify.info('Sent successfully.');
                } else {
                    abp.notify.warn('Some requests were not been sent.');
                }
                _modalManager.close();
                abp.event.trigger('app.sendLeaseHaulerRequestModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
