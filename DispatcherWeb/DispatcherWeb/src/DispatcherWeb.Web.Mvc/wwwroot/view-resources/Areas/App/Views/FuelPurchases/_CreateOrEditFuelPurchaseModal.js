(function ($) {
    app.modals.CreateOrEditFuelPurchaseModal = function () {

        var _modalManager;
        var _fuelPurchaseService = abp.services.app.fuelPurchase;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find('#Odometer').rules('add', { mileage: true });

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

            if (_$form.data('new')) {
                app.localStorage.getItem('fuelpurchase_edit', function (result) {
                    var cachedValues = result || {
                        fuelDateTime: moment().format("MM/DD/YYYY") + " 12:00 AM",
                        rate: 0,
                        ticketNumer: ''
                    };
                    _$form.find('#FuelDateTime').datetimepicker().val(cachedValues.fuelDateTime);
                    _$form.find('#Rate').val(cachedValues.rate);
                    _$form.find('#TicketNumber').val(cachedValues.ticketNumber);
                });
            } else {
                _$form.find('#FuelDateTime').datetimepicker();
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
            app.localStorage.setItem('fuelpurchase_edit', {
                fuelDateTime: model.FuelDateTime,
                rate: model.Rate,
                ticketNumber: model.TicketNumber
            });

            _modalManager.setBusy(true);
            _fuelPurchaseService.saveFuelPurchase(model).done(function () {
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
                abp.event.trigger('app.createOrEditFuelPurchaseModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        }
    };
})(jQuery);