(function ($) {
    app.modals.CreateOrEditLeaseHaulerTruckModal = function () {

        var _modalManager;
        var _modal;
        var _truckService = abp.services.app.truck;
        var _leaseHaulerService = abp.services.app.leaseHauler;
        var _$form = null;
        var _wasActive = null;
        var _isTruckReadonly = false;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modal = _modalManager.getModal();

            _$form = _modal.find('form');
            _$form.validate({ ignore: "" });

            if (_$form.find('#HaulingCompanyTruckId').val()) {
                _isTruckReadonly = true;
                _modal.find('.modal-footer .save-button').hide();
                _modal.find('.modal-footer .close-button').text('Close');
                _modal.find('input,select,textarea').prop('disabled', true);
            }

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

            var $defaultDriverId = _$form.find("#DefaultDriverId");
            var $currentTrailerId = _$form.find("#CurrentTrailerId");
            var officeDropdown = _$form.find('#OfficeId');
            var vehicleCategoryDropdown = _$form.find("#VehicleCategoryId");

            var canPullTrailerCheckbox = _$form.find('#CanPullTrailer');
            var alwaysShowOnScheduleCheckbox = _$form.find('#AlwaysShowOnSchedule');

            var defaultDriverIdLastValue = $defaultDriverId.val();
            var currentTrailerIdLastValue = $currentTrailerId.val();

            vehicleCategoryDropdown.change(function () {
                var dropdownData = vehicleCategoryDropdown.select2('data');
                let isPowered = null;
                let assetType = null;
                if (dropdownData && dropdownData.length && dropdownData[0].item) {
                    isPowered = dropdownData[0].item.isPowered;
                    assetType = dropdownData[0].item.assetType;
                }
                _$form.find("#VehicleCategoryIsPowered").val(isPowered);
                _$form.find("#VehicleCategoryAssetType").val(assetType);
                _$form.find('#IsApportioned').closest('.form-group').toggle(isPowered === true);
                _$form.find('#BedConstruction').closest('.form-group').toggle([abp.enums.assetType.dumpTruck, abp.enums.assetType.trailer].includes(assetType));
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
            });

            canPullTrailerCheckbox.change(function () {
                let canPullTrailer = canPullTrailerCheckbox.is(':checked');
                var shouldHideCurrentTrailer = !canPullTrailer;
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
                abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulerDriversSelectList,
                abpServiceParams: { leaseHaulerId: _$form.find('#LeaseHaulerId').val() },
                showAll: true,
                allowClear: true
            });

            $currentTrailerId.select2Init({
                abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulerTrucksSelectList,
                abpServiceParams: {
                    leaseHaulerId: _$form.find('#LeaseHaulerId').val(),
                    assetType: abp.enums.assetType.trailer
                },
                showAll: true,
                allowClear: true
            });

            vehicleCategoryDropdown.select2Init({
                abpServiceMethod: listCacheSelectLists.vehicleCategory(),
                showAll: true,
                allowClear: true
            });

            officeDropdown.select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: false
            });

            alwaysShowOnScheduleCheckbox.change(function () {
                alwaysShowOnSchedule = $(this).is(":checked");
                if (!alwaysShowOnSchedule) {
                    officeDropdown.closest('.form-group').hide();
                } else {
                    //abp.helper.ui.addAndSetDropdownValue(officeDropdown, abp.session.officeId, abp.session.officeName);
                    if (abp.features.isEnabled('App.AllowMultiOfficeFeature')) {
                        officeDropdown.closest('.form-group').show();
                    }
                }
            });

            $("#BedConstruction").select2Init({
                showAll: true,
                allowClear: false
            });
        };

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

            try {
                if (truck.CurrentTrailerId !== '' && truck.CanPullTrailer) {
                    _modalManager.setBusy(true);
                    let tractorWithCurrentTrailer = await _truckService.getTractorWithCurrentTrailer({
                        trailerId: truck.CurrentTrailerId,
                        tractorId: truck.Id
                    });
                    if (tractorWithCurrentTrailer) {
                        _modalManager.setBusy(false);
                        let isConfirmed = await abp.message.confirm('Trailer ' + _$form.find('#CurrentTrailer option:selected').text()
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

                _modalManager.setBusy(true);
                let editResult = await _leaseHaulerService.editLeaseHaulerTruck(truck);

                if (editResult.neededBiggerNumberOfTrucks > 0) {
                    if (!await abp.message.confirm(app.localize('ReachedNumberOfTrucks_DoYouWantToUpgrade'))) {
                        _modalManager.setBusy(false);
                        throw new Error('Couldn\'t save because number of trucks limit is reached');
                    }

                    _modalManager.setBusy(true);
                    await _truckService.updateMaxNumberOfTrucksFeatureAndNotifyAdmins({
                        newValue: editResult.neededBiggerNumberOfTrucks
                    });

                    editResult = await _leaseHaulerService.editLeaseHaulerTruck(truck);
                }

                abp.notify.info('Saved successfully.');
                abp.event.trigger('app.createOrEditLeaseHaulerTruckModalSaved');
                if (doneAction) {
                    doneAction();
                }
            } finally {
                _modalManager.setBusy(false);
            }
        }

    };
})(jQuery);
