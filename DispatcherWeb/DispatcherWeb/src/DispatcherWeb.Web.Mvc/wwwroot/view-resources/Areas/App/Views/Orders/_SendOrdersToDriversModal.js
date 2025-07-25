(function ($) {
    app.modals.SendOrdersToDriversModal = function () {

        var _modalManager;
        var _$form = null;
        var _dateInput = null;
        var _officeIdsInput = null;
        var _dispatchingService = abp.services.app.dispatching;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            var saveButton = _modalManager.getModal().find('.save-button');
            saveButton.find('span').text('Send');
            saveButton.find('i.fa-save').removeClass('fa-save').addClass('fa-envelope');

            _modalManager.getModal().on('shown.bs.modal', function () {
                saveButton.focus();
            });

            _dateInput = _$form.find("#DeliveryDate");
            _dateInput.datepickerInit();

            _$form.find('#Shift').select2Init({ allowClear: false });

            _officeIdsInput = _$form.find("#OfficeIds");
            _officeIdsInput.select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: false
            });

            let selectedOffices = _modalManager.getArgs().selectedOffices;
            if (selectedOffices?.length) {
                abp.helper.ui.addAndSetDropdownValues(_officeIdsInput, selectedOffices);
            }

            $('#SendOnlyFirstOrderCheckbox').closest('.form-group').find('label i').tooltip();
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }
            var formData = _$form.serializeFormToObject();
            var officeIds = _officeIdsInput.val();
            var sendOnlyFirstOrder = false;
            var $sendOnlyFirstOrder = _$form.find('#SendOnlyFirstOrderCheckbox');
            if ($sendOnlyFirstOrder.length && $sendOnlyFirstOrder.is(':checked')) {
                sendOnlyFirstOrder = true;
            }
            var $createAllDispatches = _$form.find('#CreateAllDispatchesCheckbox');
            if ($createAllDispatches.length && !$createAllDispatches.is(':checked')) {
                sendOnlyFirstOrder = true;
            }

            _modalManager.setBusy(true);
            _dispatchingService.sendOrdersToDrivers({
                deliveryDate: formData.DeliveryDate,
                shift: formData.Shift,
                officeIds: officeIds,
                sendOnlyFirstOrder: sendOnlyFirstOrder
            }).done(function () {
                abp.notify.info('Dispatches being created.');
                _modalManager.close();
            }).always(function () {
                _modalManager.setBusy(false);
            });

        };
    };
})(jQuery);
