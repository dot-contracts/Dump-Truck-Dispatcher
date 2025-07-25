(function ($) {
    app.modals.SelectDateModal = function () {

        var _modalManager;
        //var _ticketService = abp.services.app.ticket;
        var _$form = null;
        var _dateInput = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _dateInput = _$form.find('#Date');

        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var date = _dateInput.val();
            if (!this.saveCallback) {
                return;
            }
            try {
                _modalManager.setBusy(true);
                await this.saveCallback({ date });
            }
            finally {
                _modalManager.setBusy(false);
            }
            _modalManager.close();
        };

        this.setDate = function (date) {
            _dateInput.val(date).blur().datepicker();
        }

        this.saveCallback = null;
    };
})(jQuery);