(function ($) {
    app.modals.SelectOfficeModal = function () {

        var _modalManager;
        var _$form = null;
        var _officeDropdown = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _officeDropdown = _$form.find('#SelectOfficeModal_OfficeId');

            _$form.validate();

            _officeDropdown.select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: false
            });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            let id = _officeDropdown.val();

            if (!id) {
                return;
            }

            let name = _officeDropdown.getSelectedDropdownOption().text();

            _modalManager.setResult({
                id,
                name
            });
            _modalManager.close();
        };
    };
})(jQuery);
