(function ($) {
    app.modals.CreateOrEditVehicleUsageModal = function () {

        var _modalManager;
        var _vehicleUsageService = abp.services.app.vehicleUsage;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _modalManager.getModal().on('shown.bs.modal', function () {
                _modalManager.getModal().find('#TruckId').select2('focus');
            });

            var $truckSelect = _$form.find("#TruckId");
            $truckSelect.select2Init({
                abpServiceMethod: abp.services.app.truck.getTrucksSelectList,
                abpServiceParams: {
                    allOffices: true,
                    officeId: _$form.find('#OfficeId').val()
                },
                showAll: false,
                allowClear: true
            });

            var readingTypeDropDown = _readingTypeDropDown = _$form.find("#ReadingType");
            readingTypeDropDown.select2Init({
                showAll: true,
                allowClear: false
            });

            readingTypeDropDown.change(function () {

                var milage = _$form.find("#Mileage");
                var engineHours = _$form.find("#EngineHours");

                if (milage.val() === 0) {
                    milage.val('');
                }
                if (engineHours.val() === 0) {
                    engineHours.val('');
                }

                if (getSelectedReadingType() === 0) {
                    milage.closest('.form-group').show();
                    engineHours.closest('.form-group').hide();
                    milage.closest('.form-group').find('label').toggleClass('required-label', true);
                } else {
                    milage.closest('.form-group').hide();
                    engineHours.closest('.form-group').show();
                    engineHours.closest('.form-group').find('label').toggleClass('required-label', true);
                }
            }).change();

            if (_$form.data('new')) {
                app.localStorage.getItem('vehicleusage_edit', function (result) {
                    var cachedValues = result ||
                    {
                        readingDateTime: moment().format("MM/DD/YYYY") + " 12:00 AM",
                        rate: 0,
                        ticketNumer: ''
                    };
                    _$form.find('#ReadingDateTime').datetimepicker().val(cachedValues.readingDateTime);
                    _$form.find('#ReadingDateTime').val(cachedValues.readingDateTime);
                    _$form.find('#EngineHours').val(cachedValues.engineHours);
                    _$form.find('#Mileage').val(cachedValues.mileage);
                    abp.helper.ui.addAndSetDropdownValue(_readingTypeDropDown, cachedValues.readingType);
                });
            } else {
                _$form.find('#ReadingDateTime').datetimepicker();
            }

            _modalManager.getModal().find('.save-new-button').click(function (e) {
                e.preventDefault();
                saveData(true);
            });
        };

        this.save = function () {
            saveData();
        };

        function saveData(saveAndNew = false) {

            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var model = _$form.serializeFormToObject();

            app.localStorage.setItem('vehicleusage_edit', { readingDateTime: model.ReadingDateTime, readingType: model.ReadingType, mileage: model.Mileage, engineHours: model.EngineHours });

            if (Number(model.ReadingType) === 0) {
                model.Reading = model.Mileage;
            }
            else {
                model.Reading = model.EngineHours;
            }

            _modalManager.setBusy(true);
            _vehicleUsageService.saveVehicleUsage(model).done(function () {
                abp.notify.info('Saved successfully.');
                if (saveAndNew) {
                    _modalManager.getModal().on('hidden.bs.modal', function (e) {
                        _modalManager.open({
                            id: null,
                            officeId: _$form.find('#OfficeId').val()
                        });
                    });
                }
                _modalManager.close();
                abp.event.trigger('app.createOrEditVehicleUsageModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        }

        function getSelectedReadingType() {
            return Number(_readingTypeDropDown.val());
        }
    };
})(jQuery);