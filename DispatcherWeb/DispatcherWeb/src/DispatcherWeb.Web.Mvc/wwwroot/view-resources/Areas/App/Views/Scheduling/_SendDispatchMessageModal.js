(function ($) {

    app.modals.SendDispatchMessageModal = function () {

        var _modal;
        var _modalManager;
        var _dispatchingService = abp.services.app.dispatching;
        var _$form = null;
        var _$orderLineTruckSelect;
        var _orderLineId = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modal = _modalManager.getModal();
            _$form = _modal.find('form');
            _$form.validate({
                ignore: []
            });

            var dispatchVia = abp.setting.getInt('App.DispatchingAndMessaging.DispatchVia');
            var isDispatchViaSimplifiedSmsEnabled = dispatchVia === abp.enums.dispatchVia.simplifiedSms;
            _orderLineId = _$form.find("#OrderLineId").val();

            $.validator.addMethod(
                "regex",
                function (value, element, regexp) {
                    var re = new RegExp(regexp, 'i');
                    return this.optional(element) || re.test(value);
                },
                "Please check your input."
            );

            _$orderLineTruckSelect = _$form.find('#OrderLineTruckIds').select2Init({
                multiple: true,
                allowClear: true,
                templateSelection: formatOrderLineTruck,
                templateResult: formatOrderLineTruck
            });

            function formatOrderLineTruck(item) {
                if (!isDispatchViaSimplifiedSmsEnabled || !item.id || $(item.element).data('hascontactinfo')) {
                    return item.text;
                }
                var $result = $('<span class="driver-without-phone">').text(item.text).add('<i title="The designated driver does not have an SMS number or email address configured." class="fas fa-exclamation-circle"></i>');
                return $result;
            }

            function setAddDispatchBasedOnTimeOnJobVisibility(visible, checkedValue) {
                var checkbox = _$form.find("#AddDispatchBasedOnTime");
                checkbox.prop('checked', checkedValue || false);
                checkbox.closest('.form-group').toggle(visible);
            }

            _$orderLineTruckSelect.change(function () {
                var orderLineTruckIds = _$orderLineTruckSelect.val();
                if (!orderLineTruckIds.length) {
                    setAddDispatchBasedOnTimeOnJobVisibility(false);
                    return;
                }
                _dispatchingService.canAddDispatchBasedOnTime({
                    orderLineId: _orderLineId,
                    orderLineTruckIds: orderLineTruckIds
                }).done(function (result) {
                    if (JSON.stringify(_$orderLineTruckSelect.val()) !== JSON.stringify(orderLineTruckIds)) {
                        return;
                    }
                    setAddDispatchBasedOnTimeOnJobVisibility(result, true); //Default dispatch based on time on job visibility to True
                });
            });

            var messageInput = _$form.find("#Message");
            var messageLengthLabel = _$form.find("#MessageLength");
            var clearMessageBtn = _$form.find("#ClearMessageBtn");
            messageInput.on('input', function () {
                messageLengthLabel.text(app.localize('{0}chars', (messageInput.val() || "").length));
            });
            clearMessageBtn.on('click', function (e) {
                messageInput.val('').trigger('input');
            });

            _$form.find("#NumberOfDispatches").on("keydown", function (e) {
                if (e.key === ".") {
                    e.preventDefault();
                    return false;
                }
            }).on("blur", function (e) {
                if (!$(this).val()) {
                    $(this).val('1').trigger('input');
                    _$form.valid();
                }
            }).on('input', function () {
                if (Number($(this).val()) === 1 && !freightUomDropdownHasHours()) {
                    _$form.find("#IsMultipleLoads").prop('disabled', false);
                } else {
                    _$form.find("#IsMultipleLoads").prop('disabled', true).prop('checked', false);
                }
            });

            _$form.find('#IsMultipleLoads').change(updateRunUntilStopped);
            _$form.find('#UseTruckCapacityForLoadQuantity').change(toggleLoadMaterialQuantityControl);

            function updateRunUntilStopped() {
                let runUntilStoppedInput = _$form.find('#IsMultipleLoads');
                if (freightUomDropdownHasHours() && !runUntilStoppedInput.is(':checked')) {
                    runUntilStoppedInput.prop('disabled', true).prop('checked', false);
                } else {
                    runUntilStoppedInput.prop('disabled', false);
                }
            }

            function freightUomDropdownHasHours() {
                let freightUom = _$form.find('#FreightUom').val();
                return ['hours', 'hour'].includes((freightUom || '').toLowerCase());
            }

            function toggleLoadMaterialQuantityControl() {
                let useTruckCapacityForLoadQuantity = _$form.find('#UseTruckCapacityForLoadQuantity').is(':checked');
                _$form.find('#LoadMaterialQuantity').closest('.form-group').toggle(!useTruckCapacityForLoadQuantity);
            }

            var smsIcon = '<a id="ToggleMessageInputVisibility" class="modal-header-button-icon" href="#" title="Show SMS text"><i class="fa fa-file fa-3x" aria-hidden="true"></i></a>';
            _modal.find('div.modal-header').append(smsIcon);

            _modal.find('#ToggleMessageInputVisibility').on('click', function (e) {
                e.preventDefault();
                _modal.find('#Message').closest('.form-group').toggle();
            });

            updateRunUntilStopped();
            toggleLoadMaterialQuantityControl();
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                var maxMessageLength = parseInt(_$form.find('#Message').attr('maxlength'));
                if (maxMessageLength && _$form.find('#Message').val().length > maxMessageLength) {
                    _modal.find('#Message').closest('.form-group').show();
                }
                return;
            }
            if (!_$orderLineTruckSelect.val() || !_$orderLineTruckSelect.val().length) {
                abp.message.error('You must select the truck you want the dispatch sent to.', 'None of the trucks is selected');
                return;
            }

            var formData = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _dispatchingService.sendDispatchMessage({
                orderLineId: formData.OrderLineId,
                message: formData.Message,
                orderLineTruckIds: _$orderLineTruckSelect.val(),
                numberOfDispatches: formData.NumberOfDispatches,
                isMultipleLoads: formData.IsMultipleLoads,
                addDispatchBasedOnTime: formData.AddDispatchBasedOnTime,
                useTruckCapacityForLoadQuantity: formData.UseTruckCapacityForLoadQuantity,
                loadMaterialQuantity: formData.LoadMaterialQuantity,
            }).done(function () {
                abp.notify.info(app.localize('DispatchesBeingCreated'));
                _modalManager.close();
                abp.event.trigger('app.sendDispatchMessageModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };

    };
})(jQuery);
