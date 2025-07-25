(function ($) {
    app.modals.SelectTrailerModal = function () {

        var _modalManager;
        var _truckService = abp.services.app.truck;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            var modalArgs = modalManager.getArgs();

            _$form = _modalManager.getModal().find('form');

            if (modalArgs.optional) {
                _$form.find('#TrailerId').removeAttr('required').closest('.form-group').find('.required-label').removeClass('required-label');
            }

            _$form.validate();

            var leaseHaulerId = _$form.find('#LeaseHaulerId');
            var vehicleCategoryDropdown = _$form.find('#VehicleCategoryId');
            var bedConstructionDropdown = _$form.find('#BedConstruction');
            var makeDropdown = _$form.find('#Make');
            var modelDropdown = _$form.find('#Model');

            _$form.find('#ModalSubtitle').text(modalArgs.modalSubtitle || '');
            _$form.find("#Message").text(modalArgs.message || '');

            vehicleCategoryDropdown.select2Init({
                abpServiceMethod: listCacheSelectLists.vehicleCategory(),
                abpServiceParams: {
                    assetType: abp.enums.assetType.trailer
                },
                showAll: true,
                allowClear: true
            });

            vehicleCategoryDropdown.change(updateCategoryControls);
            bedConstructionDropdown.change(clearTrailerInputIfNeeded);
            makeDropdown.change(refreshMakeControlsVisibillity);
            modelDropdown.change(clearTrailerInputIfNeeded);

            refreshCategoryControlsVisibillity();

            async function updateCategoryControls() {
                try {
                    abp.ui.setBusy(bedConstructionDropdown.closest('.form-group'));
                    let bedConstructions = await _truckService.getBedConstructions({
                        vehicleCategoryId: vehicleCategoryDropdown.val()
                    });
                    bedConstructionDropdown.val(null).change();
                    bedConstructionDropdown.removeUnselectedOptions();
                    bedConstructions.forEach(x => {
                        bedConstructionDropdown.append($('<option>').attr('value', x.id).text(x.name));
                    });
                }
                finally {
                    abp.ui.clearBusy(bedConstructionDropdown.closest('.form-group'));
                }

                refreshCategoryControlsVisibillity();
                clearTrailerInputIfNeeded();
            }

            function refreshCategoryControlsVisibillity() {
                if (vehicleCategoryDropdown.val() == '') {
                    bedConstructionDropdown.val('').change().closest('.form-group').hide();
                    makeDropdown.val('').change().closest('.form-group').hide();
                } else {
                    bedConstructionDropdown.closest('.form-group').show();
                    makeDropdown.closest('.form-group').show();
                }
            }

            function refreshMakeControlsVisibillity() {
                if (makeDropdown.val() == '') {
                    modelDropdown.val('').change().closest('.form-group').hide();
                } else {
                    modelDropdown.closest('.form-group').show();
                }
                clearTrailerInputIfNeeded();
            }

            async function clearTrailerInputIfNeeded() {
                var trailerId = _$form.find('#TrailerId').val();
                if (!trailerId) {
                    return;
                }

                var matchingTrailers = await abp.services.app.truck.getActiveTrailersSelectList({
                    vehicleCategoryId: vehicleCategoryDropdown.val(),
                    bedConstruction: bedConstructionDropdown.val(),
                    make: makeDropdown.val(),
                    model: modelDropdown.val(),
                    id: trailerId
                });
                if (!matchingTrailers.items.length) {
                    _$form.find('#TrailerId').val('').change();
                }
            }

            bedConstructionDropdown.select2Init({
                showAll: true,
                allowClear: true
            });
            
            makeDropdown.select2Init({
                abpServiceMethod: abp.services.app.truck.getMakesSelectList,
                abpServiceParamsGetter: (params) => ({
                    vehicleCategoryId: vehicleCategoryDropdown.val()
                }),
                showAll: true,
                allowClear: true
            });

            modelDropdown.select2Init({
                abpServiceMethod: abp.services.app.truck.getModelsSelectList,
                abpServiceParamsGetter: (params) => ({
                    vehicleCategoryId: vehicleCategoryDropdown.val(),
                    make: makeDropdown.val()
                }),
                showAll: true,
                allowClear: true
            });
            
            _$form.find('#TrailerId').select2Init({
                abpServiceMethod: abp.services.app.truck.getActiveTrailersSelectList,
                abpServiceParams: {
                    leaseHaulerId: leaseHaulerId.val()
                },
                abpServiceParamsGetter: (params) => ({
                    vehicleCategoryId: vehicleCategoryDropdown.val(),
                    bedConstruction: bedConstructionDropdown.val(),
                    make: makeDropdown.val(),
                    model: modelDropdown.val()
                }),
                showAll: true,
                allowClear: true
            });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            let result = formData.TrailerId ? {
                id: Number(formData.TrailerId),
                truckCode: _$form.find('#TrailerId').getSelectedDropdownOption().text()
            } : null;

            if (result) {
                var select2Data = _$form.find('#TrailerId').select2('data');
                if (select2Data.length && select2Data[0].item) {
                    result.vehicleCategory = {
                        id: select2Data[0].item.vehicleCategoryId
                    };
                } else {
                    result.vehicleCategory = {
                        id: _modalManager.getArgs().trailerVehicleCategoryId
                    };
                }
            }

            _modalManager.setResult(result);
            _modalManager.close();
        };
    };
})(jQuery);
