(function ($) {
    app.modals.CreateOrEditTicketModal = function () {

        var _modalManager;
        var _ticketService = abp.services.app.ticket;
        var _$form = null;
        var _lastTruckCode = null;
        var _orderLineId = null;
        var _validateTrucksAndDrivers = abp.setting.getBoolean('App.General.ValidateDriverAndTruckOnTickets');
        var _allowCounterSales = abp.setting.getBoolean('App.DispatchingAndMessaging.AllowCounterSalesForUser') && abp.setting.getBoolean('App.DispatchingAndMessaging.AllowCounterSalesForTenant');
        var _allowCounterSalesForTenant = abp.setting.getBoolean('App.DispatchingAndMessaging.AllowCounterSalesForTenant');
        var _features = {
            separateItems: abp.features.isEnabled('App.SeparateMaterialAndFreightItems'),
        };

        var _createOrEditLocationModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Locations/CreateOrEditLocationModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Locations/_CreateOrEditLocationModal.js',
            modalClass: 'CreateOrEditLocationModal',
            modalSize: 'lg'
        });

        this.init = function (modalManager) {
            _modalManager = modalManager;

            var modal = _modalManager.getModal();

            _$form = modal.find('form');
            _$form.validate();

            var loadAtDropdown = _$form.find("#LoadAtId");
            var deliverToDropdown = _$form.find("#DeliverToId");

            _orderLineId = Number(_$form.find("#OrderLineId").val()) || null;

            modal.on('shown.bs.modal', function () {
                modal.find('#CustomerId').focus();
                //_$form.find("#TicketDateTime").datepicker('hide');
            });
            _$form.find('#TicketDateTime').datetimepickerInit();

            async function addNewLocation(newItemName) {
                var result = await app.getModalResultAsync(
                    _createOrEditLocationModal.open({ mergeWithDuplicateSilently: true, name: newItemName })
                );
                return {
                    id: result.id,
                    name: result.displayName
                };
            }

            loadAtDropdown.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownLoadSite,
                showAll: false,
                allowClear: true,
                addItemCallback: abp.auth.isGranted('Pages.Locations') ? addNewLocation : null
            });

            deliverToDropdown.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownDeliverySite,
                showAll: false,
                allowClear: true,
                addItemCallback: abp.auth.isGranted('Pages.Locations') ? addNewLocation : null
            });

            _$form.find("select#OfficeId").select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: false
            });

            _$form.find('#CustomerId').select2Init({
                abpServiceMethod: abp.services.app.customer.getActiveCustomersSelectList,
                showAll: false,
                allowClear: true
            });

            _$form.find('#FreightItemId').select2Init({
                abpServiceMethod: listCacheSelectLists.item(),
                abpServiceParamsGetter: (params) => ({
                    types: _features.separateItems ? abp.enums.itemTypes.material : null,
                }),
                showAll: listCache.item.isEnabled,
                allowClear: true
            });

            _$form.find('#MaterialItemId').select2Init({
                abpServiceMethod: listCacheSelectLists.item(),
                abpServiceParamsGetter: (params) => ({
                    types: _features.separateItems ? abp.enums.itemTypes.freight : null,
                }),
                showAll: listCache.item.isEnabled,
                allowClear: true
            });

            _$form.find('#FreightUomId').select2Uom();
            _$form.find('#MaterialUomId').select2Uom();

            let carrierDropdown = _$form.find('#CarrierId');
            carrierDropdown.select2Init({
                abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulersSelectList,
                showAll: false,
                allowClear: true
            }).change(function () {
                updateTruckAndDriverRequiredness();
            });

            let trailerDropdown = _$form.find('#TrailerId');
            trailerDropdown.select2Init({
                abpServiceMethod: abp.services.app.truck.getTrucksSelectList,
                abpServiceParams: {
                    assetType: abp.enums.assetType.trailer,
                },
                showAll: false,
                allowClear: true,
            });

            let driverDropdown = _$form.find('#DriverId');
            let truckCodeInput = _$form.find('#TruckCode');
            let truckOrDriverIsChanging = false;
            truckCodeInput.blur(async function () {
                let truckCode = _$form.find('#TruckCode').val();
                if (truckCode == _lastTruckCode) {
                    return;
                }
                _lastTruckCode = truckCode;

                if (!_validateTrucksAndDrivers) {
                    return;
                }

                let ticketDateTime = _$form.find("#TicketDateTime").val();
                if (!ticketDateTime && !_orderLineId || !truckCode) {
                    return;
                }

                if (truckOrDriverIsChanging) {
                    return;
                }
                let lockedControls = driverDropdown.closest('.form-group')
                    .add(truckCodeInput.closest('.form-group'))
                    .add(trailerDropdown.closest('.form-group'));
                try {
                    truckOrDriverIsChanging = true;
                    abp.ui.setBusy(lockedControls);
                    let ticketDriverAndTrailer = await _ticketService.getDriverAndTrailerForTicketTruck({
                        orderLineId: _orderLineId,
                        orderDate: _orderLineId ? null : moment(ticketDateTime, 'L').format('L'),
                        truckCode: truckCode
                    });

                    if (!ticketDriverAndTrailer.truckCodeIsCorrect) {
                        if (!carrierDropdown.val()) {
                            abp.message.error("Truck Code is invalid");
                        }
                        return;
                    }

                    if (ticketDriverAndTrailer.driverId) {
                        abp.helper.ui.addAndSetDropdownValue(driverDropdown, ticketDriverAndTrailer.driverId, ticketDriverAndTrailer.driverName);
                    }
                    abp.helper.ui.addAndSetDropdownValue(carrierDropdown, ticketDriverAndTrailer.carrierId, ticketDriverAndTrailer.carrierName);
                    if (ticketDriverAndTrailer.trailerId) {
                        abp.helper.ui.addAndSetDropdownValue(trailerDropdown, ticketDriverAndTrailer.trailerId, ticketDriverAndTrailer.trailerTruckCode);
                    }
                }
                finally {
                    abp.ui.clearBusy(lockedControls);
                    truckOrDriverIsChanging = false;
                }
            });

            trailerDropdown.change(async function () {
                let trailerId = $(this).val();
                if (!_validateTrucksAndDrivers) {
                    return;
                }
                let ticketDateTime = _$form.find("#TicketDateTime").val();
                if (!ticketDateTime && !_orderLineId || !trailerId) {
                    return;
                }

                if (truckOrDriverIsChanging) {
                    return;
                }
                let lockedControls = driverDropdown.closest('.form-group')
                    .add(truckCodeInput.closest('.form-group'))
                    .add(trailerDropdown.closest('.form-group'));
                try {
                    truckOrDriverIsChanging = true;
                    abp.ui.setBusy(lockedControls);
                    let ticketTruckAndDriver = await _ticketService.getTruckAndDriverForTicketTrailer({
                        orderLineId: _orderLineId,
                        orderDate: _orderLineId ? null : moment(ticketDateTime, 'L').format('L'),
                        trailerId: trailerId
                    });

                    if (ticketTruckAndDriver.truckId) {
                        truckCodeInput.val(ticketTruckAndDriver.truckCode);
                        abp.helper.ui.addAndSetDropdownValue(carrierDropdown, ticketTruckAndDriver.carrierId, ticketTruckAndDriver.carrierName);
                    }
                    if (ticketTruckAndDriver.driverId) {
                        abp.helper.ui.addAndSetDropdownValue(driverDropdown, ticketTruckAndDriver.driverId, ticketTruckAndDriver.driverName);
                    }
                }
                finally {
                    abp.ui.clearBusy(lockedControls);
                    truckOrDriverIsChanging = false;
                }
            });


            var updateTruckAndDriverRequiredness = function () {
                var carrierId = carrierDropdown.val();
                _$form.find('label[for="TruckCode"],label[for="DriverId"]').toggleClass('required-label', !carrierId && !_allowCounterSalesForTenant);
            };
            updateTruckAndDriverRequiredness();

            driverDropdown.select2Init({
                abpServiceMethod: abp.services.app.driver.getDriversSelectList,
                abpServiceParams: { orderLineId: _validateTrucksAndDrivers ? _orderLineId : null },
                abpServiceParamsGetter: (params, rowData) => ({
                    truckCode: _lastTruckCode
                }),
                showAll: false,
                allowClear: true
            }).change(async function () {
                var driverId = $(this).val();
                if (!_validateTrucksAndDrivers) {
                    return;
                }

                let ticketDateTime = _$form.find("#TicketDateTime").val();
                if (!ticketDateTime && !_orderLineId || !driverId) {
                    return;
                }

                if (truckOrDriverIsChanging) {
                    return;
                }
                let lockedControls = driverDropdown.closest('.form-group')
                    .add(truckCodeInput.closest('.form-group'))
                    .add(trailerDropdown.closest('.form-group'));
                try {
                    truckOrDriverIsChanging = true;
                    abp.ui.setBusy(lockedControls);
                    let ticketTruckAndTrailer = await _ticketService.getTruckAndTrailerForTicketDriver({
                        orderLineId: _orderLineId,
                        orderDate: _orderLineId ? null : moment(ticketDateTime, 'L').format('L'),
                        driverId: driverId
                    });

                    if (ticketTruckAndTrailer.truckId) {
                        truckCodeInput.val(ticketTruckAndTrailer.truckCode);
                        abp.helper.ui.addAndSetDropdownValue(carrierDropdown, ticketTruckAndTrailer.carrierId, ticketTruckAndTrailer.carrierName);
                    }
                    if (ticketTruckAndTrailer.trailerId) {
                        abp.helper.ui.addAndSetDropdownValue(trailerDropdown, ticketTruckAndTrailer.trailerId, ticketTruckAndTrailer.trailerTruckCode);
                    }
                }
                finally {
                    abp.ui.clearBusy(lockedControls);
                    truckOrDriverIsChanging = false;
                }
            });

            if (modal.find("#ReadOnly").val() === "true") {
                modal.find("input, select, textarea, form button").not('#TicketPhoto, #AddTicketPhotoButton').prop('disabled', true);
                modal.find('.save-button').hide();
                modal.find('.close-button').text('Close');
            }

            var ticketId = modal.find("#Id").val();
            var $ticketPhotoId = modal.find("#TicketPhotoId");
            var $ticketPhoto = modal.find("#TicketPhoto");
            var ticketPhotoId = $ticketPhotoId.val();
            var ticketIsInternal = modal.find('#IsInternal').val() === 'True';
            if (ticketId) {
                if (ticketPhotoId) {
                    modal.find("#TicketPhotoBlock").show();
                } else {
                    modal.find("#AddTicketPhotoBlock").show();
                    if (ticketIsInternal) {
                        modal.find('#PrintTicketBlock').show();
                    }
                }
            }
            var addTicketPhotoButton = modal.find("#AddTicketPhotoButton");
            addTicketPhotoButton.click(function (e) {
                e.preventDefault();
                if (!validateTicketPhoto()) {
                    return;
                }
                const file = $ticketPhoto[0].files[0];
                const reader = new FileReader();

                reader.addEventListener("load", function () {
                    _ticketService.addTicketPhoto({
                        ticketId: ticketId,
                        ticketPhoto: reader.result,
                        ticketPhotoFilename: file.name
                    }).done(function (result) {
                        $ticketPhotoId.val(result.ticketPhotoId);
                        ticketPhotoId = result.ticketPhotoId;
                        modal.find("#AddTicketPhotoBlock").hide();
                        modal.find('#PrintTicketBlock').hide();
                        modal.find("#TicketPhotoBlock").show();
                    }).always(function () {
                        abp.ui.clearBusy(addTicketPhotoButton);
                        _modalManager.setBusy(false);
                    });
                }, false);

                abp.ui.setBusy(addTicketPhotoButton);
                _modalManager.setBusy(true);
                reader.readAsDataURL(file);
            });

            function validateTicketPhoto() {
                let files = $ticketPhoto.get(0).files;

                if (!files.length) {
                    showValidationError('No file is selected.');
                    return false;
                }

                let file = files[0];

                //File type check
                let type = '|' + file.type.slice(file.type.lastIndexOf('/') + 1) + '|';
                if ('|jpg|jpeg|png|gif|pdf|'.indexOf(type) === -1) {
                    showValidationError('Invalid file type.');
                    return false;
                }
                //File size check
                if (file.size > 8388608) //8 MB
                {
                    showValidationError('Size of the file exceeds allowed limits.');
                    return false;
                }
                return true;
            }

            function showValidationError(errorMessage) {
                abp.message.error('Please check the following: \n' + errorMessage, 'Some of the data is invalid');
            }

            modal.find('#printTicketButton').click(function (e) {
                e.preventDefault();
                let url = abp.appPath + 'app/Tickets/GetTicketPrintOut?ticketId=' + ticketId;
                app.openPopup(url);
            });

            modal.find("#showTicketPhotoButton").click(function (e) {
                e.preventDefault();
                let url = abp.appPath + 'app/Tickets/GetTicketPhoto/' + ticketId;
                app.openPopup(url);
            });

            $ticketPhoto.on("change", function () {
                var filename = $(this).val().split('\\').pop();
                modal.find("#FileNamePlaceholder").val(filename);
            });
        };

        this.save = async function () {
            let cannotEditReason = _$form.find("#CannotEditReason").val();
            if (cannotEditReason) {
                abp.message.error(cannotEditReason);
                return;
            }

            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var ticket = _$form.serializeFormToObject();

            if (!abp.helper.validateFutureDates(
                { value: $("#TicketDateTime").val(), title: $('label[for="TicketDateTime"]').text() }
            )) {
                return;
            }

            if (ticket.OrderDate) {
                if (moment(ticket.TicketDateTime, "MM/DD/YYYY") > moment(ticket.OrderDate, "MM/DD/YYYY")) {
                    if (!await abp.message.confirm(
                        'You have entered a ticket date that is different from the order date. This may cause your Revenue Report by Order to be different from the tickets for the same date range. Are you sure you want to do this?',
                    )) {
                        return;
                    }
                }
            }

            try {

                _modalManager.setBusy(true);
                if (ticket.IsBilled) {
                    let outOfServiceOrInactive = await _ticketService.checkIfTruckIsOutOfServiceOrInactive(ticket);
                    if (outOfServiceOrInactive) {
                        if (!await abp.message.confirm(
                            'Truck ' + ticket.TruckCode + ' is ' + outOfServiceOrInactive + '. Are you sure you want to add a ticket for this truck?',
                        )) {
                            _modalManager.setBusy(false);
                            return;
                        }
                    }
                }


                //if (ticket.ReceiptLineId) {
                //    abp.message.confirm(
                //        'This ticket is associated with a receipt. If you change this ticket, your ticket and receipt amounts will not match. Are you sure you want to do this?',
                //        function (isConfirmed) {
                //            if (isConfirmed) {
                //                saveCallback(ticket);
                //            }
                //        }
                //    );
                //} else {
                //    saveCallback(ticket);
                //}

                _modalManager.setBusy(true);
                await _ticketService.editTicket(ticket);
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditTicketModalSaved');
            } finally {
                _modalManager.setBusy(false);
            }
        };

    };
})(jQuery);
