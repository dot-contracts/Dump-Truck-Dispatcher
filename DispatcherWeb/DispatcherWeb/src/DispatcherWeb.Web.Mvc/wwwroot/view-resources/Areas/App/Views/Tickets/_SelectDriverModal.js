(function ($) {
    app.modals.SelectDriverModal = function () {

        var _modalManager;
        //var _ticketService = abp.services.app.ticket;
        var _$form = null;
        var _driverInput = null
        var _drivers = [];

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _driverInput = _$form.find('#DriverId');

        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var driverId = Number(_driverInput.val()) || null;
            var driver = _driverInput.getSelectedDropdownOption().data('driver');
            if (!this.saveCallback) {
                return;
            }
            try {
                _modalManager.setBusy(true);
                await this.saveCallback({ driver, driverId });
            }
            finally {
                _modalManager.setBusy(false);
            }
            _modalManager.close();
        };

        this.setDrivers = function (drivers) {
            _drivers = drivers;
            _driverInput.append(
                _drivers
                    .map(d => $('<option>').attr('value', d.id).text(d.name).data('driver', d))
                    .reduce((prev, curr) => prev ? prev.add(curr) : curr)
            );
            _driverInput.select2Init({
                allowClear: false,
                showAll: true,
                width: "100%"
            });
        }

        this.saveCallback = null;
    };
})(jQuery);