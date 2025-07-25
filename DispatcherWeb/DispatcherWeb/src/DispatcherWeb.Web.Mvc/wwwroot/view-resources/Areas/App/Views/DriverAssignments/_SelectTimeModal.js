(function ($) {
    app.modals.SelectTimeModal = function () {

        var _modalManager;
        var _$form = null;
        var _saveCallback = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find("#StartTime").timepickerInit({ stepping: 1 });
        };

        this.setSaveCallback = function (val) {
            _saveCallback = val;
        };

        this.save = function () {
            //if (!_$form.valid()) {
            //    _$form.showValidateMessage();
            //    return;
            //}

            var formValues = _$form.serializeFormToObject();
            if (formValues.StartTime === "") {
                formValues.StartTime = null;
            }

            _saveCallback(formValues);
            _modalManager.close();
        };
    };
})(jQuery);
