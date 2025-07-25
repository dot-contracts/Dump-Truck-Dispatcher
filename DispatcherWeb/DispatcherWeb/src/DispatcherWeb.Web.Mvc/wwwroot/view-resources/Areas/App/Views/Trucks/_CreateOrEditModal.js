(function ($) {
    app.modals.CreateOrEditTruckModal = function () {

        var _modalManager;
        var _truckService = abp.services.app.truck;
        var _$form = null;
        var _dtHelper = abp.helper.dataTables;
        var _wasActive = null;
        var _enabledDriverAppGps = null;


        var _createOrEditPreventiveMaintenanceModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/PreventiveMaintenanceSchedule/CreateOrEditPreventiveMaintenanceModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/PreventiveMaintenanceSchedule/_CreateOrEditPreventiveMaintenanceModal.js',
            modalClass: 'CreateOrEditPreventiveMaintenanceModal'
        });

        var _createOrEditDriverModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Drivers/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Drivers/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditDriverModal',
        });

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('#DriverdetailsForm');
            _$form.validate({ ignore: "" });
            _$form.find('#CurrentMileage').rules('add', { mileage: true });

            _enabledDriverAppGps = abp.features.isEnabled('App.GpsIntegrationFeature')
                && abp.setting.getBoolean('App.GpsIntegration.DtdTracker.EnableDriverAppGps')
                && abp.setting.getInt('App.GpsIntegration.GpsPlatform') === abp.enums.gpsPlatform.dtdTracker;

            var $reasonCtrl = _$form.find('#Reason');
            var $reasonDiv = $reasonCtrl.parent();

            _$form.find('#IsOutOfService').on('change', function (e) {
                if ($(this).is(':checked')) {
                    $reasonDiv.show('medium');
                    $reasonCtrl.rules('add', { required: true });
                } else {
                    $reasonDiv.hide('medium');
                    $reasonCtrl.rules('remove', 'required');
                }
            }).change();

            var $inactivationDateCtrl = _$form.find('#InactivationDate');
            var $inactivationDateDiv = $inactivationDateCtrl.parent();

            var isActiveCheckbox = _$form.find('#IsActive');
            isActiveCheckbox.on('change', function (e) {
                if ($(this).is(':checked')) {
                    $inactivationDateCtrl.removeAttr('required');
                    $inactivationDateDiv.hide('medium');
                } else {
                    $inactivationDateDiv.show('medium');
                    $inactivationDateCtrl.attr('required', 'required');
                    if (!$inactivationDateCtrl.val()) {
                        $inactivationDateCtrl.val(moment().format('MM/DD/YYYY'));
                    }
                }
            });
            _wasActive = isActiveCheckbox.is(':checked');

            _$form.find('.datepicker').datepickerInit();

            $('#InServiceDate').data('DateTimePicker').minDate(moment('1/1/1980', 'M/D/YYYY')).maxDate(moment('1/1/2100', 'M/D/YYYY'));

            var officeDropdown = _$form.find("#OfficeId");

            officeDropdown.select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: false
            });
            officeDropdown.change(function () {
                setDtdTrackerDeviceTypeToDefaultIfNeeded();
            });

            var $defaultDriverId = _$form.find("#DefaultDriverId");
            var $currentTrailerId = _$form.find("#CurrentTrailerId");
            var vehicleCategoryDropdown = _$form.find("#VehicleCategoryId");
            var canPullTrailerCheckbox = _$form.find('#CanPullTrailer');

            var defaultDriverIdLastValue = $defaultDriverId.val();
            var currentTrailerIdLastValue = $currentTrailerId.val();

            let assetType = Number(_$form.find("#VehicleCategoryAssetType").val());
            if (assetType) {
                let isBedConstructionRequired = [abp.enums.assetType.dumpTruck, abp.enums.assetType.trailer].includes(assetType);
                _$form.find('#BedConstruction').attr('required', isBedConstructionRequired).closest('.form-group').toggle(isBedConstructionRequired);
            }

            vehicleCategoryDropdown.change(function () {
                var dropdownData = vehicleCategoryDropdown.select2('data');
                let isPowered = null;
                let assetType = null;
                if (dropdownData && dropdownData.length && dropdownData[0].item) {
                    isPowered = dropdownData[0].item.isPowered;
                    assetType = dropdownData[0].item.assetType;
                }
                let isBedConstructionRequired = [abp.enums.assetType.dumpTruck, abp.enums.assetType.trailer].includes(assetType);
                _$form.find("#VehicleCategoryIsPowered").val(isPowered);
                _$form.find("#VehicleCategoryAssetType").val(assetType);
                _$form.find('#IsApportioned').closest('.form-group').toggle(isPowered === true);
                _$form.find('#BedConstruction').attr('required', isBedConstructionRequired).closest('.form-group').toggle(isBedConstructionRequired);
                canPullTrailerCheckbox.closest('.form-group').toggle(isPowered === true);
                canPullTrailerCheckbox.prop('checked', assetType === abp.enums.assetType.tractor).change();
                var shouldDisableDefaultDriver = isPowered !== true;
                if (shouldDisableDefaultDriver) {
                    defaultDriverIdLastValue = $defaultDriverId.val();
                    $defaultDriverId.val(null).change();
                } else {
                    $defaultDriverId.val(defaultDriverIdLastValue).change();
                }
                $defaultDriverId.prop('disabled', shouldDisableDefaultDriver);

                vehicleCategoryDropdown.find('option').not(`[value=""],[value="${vehicleCategoryDropdown.val()}"]`).remove();

                setDtdTrackerDeviceTypeToDefaultIfNeeded();
            });

            canPullTrailerCheckbox.change(function () {
                let canPullTrailer = canPullTrailerCheckbox.is(':checked');
                var shouldHideCurrentTrailer = !canPullTrailer; //assetType !== abp.enums.assetType.tractor;
                if (shouldHideCurrentTrailer) {
                    currentTrailerIdLastValue = $currentTrailerId.val();
                    $currentTrailerId.val(null).change();
                    $currentTrailerId.closest('div').slideUp();
                } else {
                    $currentTrailerId.val(currentTrailerIdLastValue).change();
                    $currentTrailerId.closest('div').slideDown();
                }
            });

            $defaultDriverId.select2Init({
                abpServiceMethod: abp.services.app.driver.getDriversSelectList,
                abpServiceParams: {
                    includeLeaseHaulerDrivers: abp.setting.getBoolean('App.LeaseHaulers.AllowSubcontractorsToDriveCompanyOwnedTrucks')
                },
                showAll: false,
                allowClear: true,
                addItemCallback: async function (newItemName) {
                    var result = await app.getModalResultAsync(
                        _createOrEditDriverModal.open({ name: newItemName })
                    );
                    return {
                        id: result.id,
                        name: result.firstName + ' ' + result.lastName
                    };
                },
            });

            $currentTrailerId.select2Init({
                abpServiceMethod: abp.services.app.truck.getActiveTrailersSelectList,
                showAll: true,
                allowClear: true
            });

            vehicleCategoryDropdown.select2Init({
                abpServiceMethod: listCacheSelectLists.vehicleCategory(),
                showAll: true,
                allowClear: false
            });

            $("#FuelType").select2Init({
                showAll: true,
                allowClear: true
            });

            $("#BedConstruction").select2Init({
                showAll: true,
                allowClear: false
            });




            var $modal = _modalManager.getModal();
            $modal.find('#SaveCurrentMileageAndHours').click(function (e) {
                e.preventDefault();
                var $currentMileage = $modal.find('#CurrentMileage');
                var $currentHours = $modal.find('#CurrentHours');
                if (!$currentMileage.valid() || !$currentHours.valid()) {
                    return;
                }
                _modalManager.setBusy(true);
                _truckService.saveCurrentMileageAndHours({ truckId: getTruckId(), currentMileage: $currentMileage.val(), currentHours: $currentHours.val() }).done(function () {
                    abp.notify.info('Saved successfully.');
                    abp.event.trigger('app.currentMileageSaved');
                }).always(function () {
                    _modalManager.setBusy(false);
                });
            });

            var preventiveMaintenanceTable = $modal.find('#PreventiveMaintenanceTable');
            var preventiveMaintenanceGrid = preventiveMaintenanceTable.DataTableInit({
                paging: false,
                info: false,
                ordering: false,
                ajax: function (data, callback, settings) {
                    _truckService.getPreventiveMaintenanceDueByTruck(getTruckId()).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                columns: [
                    {
                        data: "vehicleServiceName",
                        title: "Service	Name"
                    },
                    {
                        data: "dueDate",
                        title: "Due Date",
                        render: function (data, type, full, meta) {
                            return _dtHelper.renderUtcDate(data);
                        }
                    },
                    {
                        data: "dueMileage",
                        title: "Due Mileage"
                    },
                    {
                        data: "dueHour",
                        title: "Due Hours"
                    }
                ]
            });
            $modal.find('#AddServiceReminder').click(function (e) {
                e.preventDefault();
                _createOrEditPreventiveMaintenanceModal.open({ truckId: getTruckId(), truckCode: _$form.find('#TruckCode').val() });
            });
            abp.event.on('app.createOrEditPreventiveMaintenanceModal', function () {
                preventiveMaintenanceGrid.ajax.reload();
            });


            var maintenanceHistoryTable = $modal.find('#MaintenanceHistoryTable');
            var maintenanceHistoryGrid = maintenanceHistoryTable.DataTableInit({
                paging: false,
                info: false,
                ordering: false,
                ajax: function (data, callback, settings) {
                    _truckService.getServiceHistoryByTruck(getTruckId()).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                columns: [
                    {
                        data: "vehicleServiceName",
                        title: "Service"
                    },
                    {
                        data: "completionDate",
                        title: "Date",
                        render: function (data, type, full, meta) {
                            return _dtHelper.renderUtcDate(data);
                        }
                    },
                    {
                        data: "mileage",
                        title: "Mileage"
                    }
                ]
            });

            _$form.find('#File').click(function (e) {
                if (app.showWarningIfFreeVersion()) {
                    e.preventDefault();
                    return;
                }
                if (!_$form.valid()) {
                    e.preventDefault();
                    _$form.showValidateMessage();
                }
            });
            _$form.find('#File').fileupload({
                add: function (e, data) {
                    var truckId = getTruckId();
                    if (!truckId) {
                        saveTruck(function (truckId) {
                            addFile(data, truckId);
                        });
                    } else {
                        addFile(data, truckId);
                    }
                },
                submit: function (e, data) {
                    _modalManager.setBusy(true);
                },
                done: function (e, data) {
                    var id = data.result.result.id;
                    $.get('/app/Trucks/GetFileRow?id=' + id, function (htmlRow) {
                        var $tableBody = $('#FilesTable tbody');
                        $tableBody.append(htmlRow);
                    });

                    _modalManager.setBusy(false);
                }
            });
            function addFile(data, truckId) {
                data.formData = { 'id': truckId };
                data.submit();
            }

            $('#FilesTable').on('click', 'button', function (e) {
                e.preventDefault();
                var $button = $(this);
                var id = $button.data('id');
                abp.ui.setBusy($button);
                _truckService.deleteFile({ id: id })
                    .done(function () {
                        $button.closest('tr').remove();
                    })
                    .always(function () {
                        abp.ui.clearBusy($button);
                    }
                    );
            });


            function getTruckId() {
                return _$form.find('#Id').val();
            }

            var dtdTrackerDeviceTypeInput = _$form.find("#DtdTrackerDeviceTypeId");
            dtdTrackerDeviceTypeInput.select2Init({
                abpServiceMethod: abp.services.app.truckTelematics.getWialonDeviceTypesSelectList,
                showAll: false,
                allowClear: true
            });

            dtdTrackerDeviceTypeInput.change(function () {
                var dropdownData = dtdTrackerDeviceTypeInput.select2('data');
                var serverAddress = '';
                var deviceTypeName = '';
                if (dropdownData && dropdownData.length && dropdownData[0].item) {
                    serverAddress = dropdownData[0].item.serverAddress;
                    deviceTypeName = dropdownData[0].name;
                }
                _$form.find("#DtdTrackerServerAddress").val(serverAddress);
                _$form.find("#DtdTrackerDeviceTypeName").val(deviceTypeName);
                dtdTrackerDeviceTypeInput.removeUnselectedOptions();
            });

            _$form.find("#EnableDriverAppGps").change(function () {
                setDtdTrackerDeviceTypeToDefaultIfNeeded();
            });

            _$form.find('#TruckCode').focusout(function () {
                setDtdTrackerDeviceTypeToDefaultIfNeeded();
            });

            function setDtdTrackerDeviceTypeToDefaultIfNeeded() {
                if (_enabledDriverAppGps
                    && _$form.find("#EnableDriverAppGps").is(':checked')
                    && _$form.find("#VehicleCategoryIsPowered").val() === 'true'
                    && _$form.find('#TruckCode').val()
                    && officeDropdown.val()
                ) {
                    let defaultDtdTrackerDeviceTypeId = abp.setting.get('App.HostManagement.DefaultDtdTrackerDeviceTypeId');
                    let defaultDtdTrackerDeviceTypeName = abp.setting.get('App.HostManagement.DefaultDtdTrackerDeviceTypeName');
                    let defaultDtdTrackerServerAddress = abp.setting.get('App.HostManagement.DefaultDtdTrackerServerAddress');
                    abp.helper.ui.addAndSetDropdownValue(dtdTrackerDeviceTypeInput, defaultDtdTrackerDeviceTypeId, defaultDtdTrackerDeviceTypeName);
                    _$form.find("#DtdTrackerServerAddress").val(defaultDtdTrackerServerAddress);
                    _$form.find("#DtdTrackerDeviceTypeName").val(defaultDtdTrackerDeviceTypeName);

                    let tenantName = app.session.tenant.name;
                    let uniqueIdValue = null;
                    if (abp.features.isEnabled('App.AllowMultiOfficeFeature')) {
                        let officeDropdownData = officeDropdown.select2('data');
                        uniqueIdValue = tenantName + "-" + officeDropdownData[0].text + "-" + _$form.find('#TruckCode').val();
                    } else {
                        uniqueIdValue = tenantName + "-" + _$form.find('#TruckCode').val();
                    }
                    _$form.find("#DtdTrackerUniqueId").val(uniqueIdValue);
                }
            }

            $('.AddDeviceButton').on('click', function (e) {
                e.preventDefault();
                var $button = $(this);
                $button.hide();
                $button.closest('.card-body').find('.mobile-device-controls').addClass('added').show();
            })
        };

        //_$generalTab = _modalManager.getModal().find('#GeneralTab');
        //_$generalTab.validate({ ignore: "" });

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            saveTruck(function () {
                _modalManager.close();
            });
        };

        async function saveTruck(doneAction) {
            var truck = _$form.serializeFormToObject();
            truck.files = getFiles();
            truck.mobileDevices = getMobileDevices();

            try {
                if (truck.CurrentTrailerId !== '' && truck.CanPullTrailer) { //truck.VehicleCategoryAssetType === abp.enums.assetType.tractor.toString()
                    _modalManager.setBusy(true);
                    let tractorWithCurrentTrailer = await _truckService.getTractorWithCurrentTrailer({
                        trailerId: truck.CurrentTrailerId,
                        tractorId: truck.Id
                    });
                    if (tractorWithCurrentTrailer) {
                        _modalManager.setBusy(false);
                        let isConfirmed = await abp.message.confirm('Trailer ' + _$form.find("#CurrentTrailerId option:selected").text()
                            + ' is currently assigned to truck ' + tractorWithCurrentTrailer
                            + '. If you continue with this operation, the trailer will be moved to this new truck. Is this what you want to do?');
                        if (!isConfirmed) {
                            return;
                        }
                    }
                }

                if (truck.VehicleCategoryAssetType === abp.enums.assetType.trailer.toString() && _wasActive && !truck.IsActive) {
                    _modalManager.setBusy(true);
                    let tractorWithCurrentTrailer = await _truckService.getTractorWithCurrentTrailer({
                        trailerId: truck.Id
                    });
                    if (tractorWithCurrentTrailer) {
                        _modalManager.setBusy(false);
                        let isConfirmed = await abp.message.confirm("This trailer is the current trailer on truck "
                            + tractorWithCurrentTrailer
                            + ". Are you sure you want to make this trailer inactive?");
                        if (!isConfirmed) {
                            return;
                        }
                    }
                }

                if (truck.DefaultDriverId) {
                    _modalManager.setBusy(true);
                    let truckWithSameDefaultDriver = await _truckService.getTruckCodeByDefaultDriver({
                        exceptTruckId: truck.Id,
                        defaultDriverId: truck.DefaultDriverId
                    });
                    if (truckWithSameDefaultDriver) {
                        _modalManager.setBusy(false);
                        let isConfirmed = await abp.message.confirm('Driver ' + _$form.find("#DefaultDriverId option:selected").text()
                            + ' is already associated with another truck ' + truckWithSameDefaultDriver
                            + '. Do you want to change the default truck assignment for this driver?');
                        if (!isConfirmed) {
                            return;
                        }
                    }
                }

                _modalManager.setBusy(true);
                let editResult = await _truckService.editTruck(truck);

                if (editResult.neededBiggerNumberOfTrucks > 0) {
                    if (!await abp.message.confirm(app.localize('ReachedNumberOfTrucks_DoYouWantToUpgrade'))) {
                        _modalManager.setBusy(false);
                        throw new Error('Couldn\'t save because number of trucks limit is reached');
                    }

                    _modalManager.setBusy(true);
                    await _truckService.updateMaxNumberOfTrucksFeatureAndNotifyAdmins({
                        newValue: editResult.neededBiggerNumberOfTrucks
                    });

                    editResult = await _truckService.editTruck(truck);
                }

                $('#Id').val(editResult.id);
                abp.notify.info('Saved successfully.');
                var message = '';
                message += editResult.thereAreOrdersInTheFuture ? app.localize('ThereAreOrdersInTheFuture') + '\n' : '';
                message += editResult.thereWereAssociatedOrders ? app.localize('ThereWereOrdersAssociatedWithThisTruck') + '\n' : '';
                message += editResult.thereWereCanceledDispatches ? app.localize('ThereWereCanceledDispatches') + '\n' : '';
                message += editResult.thereWereNotCanceledDispatches ? app.localize('ThereWereNotCanceledDispatches') + '\n' : '';
                message += editResult.thereWereAssociatedTractors ? app.localize('ThereWasTractorAssociatedWithThisTrailer') + '\n' : '';
                if (message) {
                    abp.message.info(message, 'Message');
                }
                abp.event.trigger('app.createOrEditTruckModalSaved');
                if (doneAction) {
                    doneAction(editResult.id);
                }

            } finally {
                _modalManager.setBusy(false);
            }
        }

        function getFiles() {
            var $fileRows = _$form.find('#FilesTable tbody tr');
            var files = [];
            $fileRows.each(function (i) {
                var $row = $(this);
                var file = {
                    Id: $row.data('id'),
                    FileId: $row.data('file-id'),
                    Title: $row.find('td:nth-child(1) input').val(),
                    Description: $row.find('td:nth-child(2) textarea').val()
                };

                files.push(file);
            });
            return files;
        }

        function getMobileDevices() {
            var $mobileDevices = _$form.find('.mobile-device-controls.added');
            var mobileDevices = [];
            $mobileDevices.each(function (i) {
                var mobileDevice = {};
                var $inputs = $(this).find('input');
                $inputs.each(function (e) {
                    var $input = $(this);
                    var inputName = $input.attr('name').replace('MobileDevice-', '');
                    mobileDevice[inputName] = $input.val();
                });
                mobileDevices.push(mobileDevice);
            });
            return mobileDevices;
        }

    };
})(jQuery);
