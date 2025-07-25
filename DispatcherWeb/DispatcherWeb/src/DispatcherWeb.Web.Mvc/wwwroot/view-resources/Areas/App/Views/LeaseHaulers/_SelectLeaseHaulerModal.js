(function () {
    app.modals.SelectLeaseHaulerModal = function () {

        let _modalManager;
        let _$modal = null;
        let _$form = null;
        var _leaseHaulerDropdown = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$modal = _modalManager.getModal();
            _$form = _$modal.find('form');
            _leaseHaulerDropdown = _$form.find('#SelectLeaseHaulerModal_LeaseHaulerId');

            _$form.validate();

            // make sure to hide the cancel button
            //AG: Don't they have other ways to close the modal, like clicking outside the modal or pressing the escape key?
            //_$modal.find('.close-button').hide();

            const okButton = _$modal.find('.save-button');
            okButton.find('span').text('OK');
            okButton.find('.fa').remove();

            _leaseHaulerDropdown.select2Init({
                abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulersSelectList,
                showAll: true,
                allowClear: false
            });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            let id = _leaseHaulerDropdown.val();

            if (!id) {
                return;
            }

            let name = _leaseHaulerDropdown.getSelectedDropdownOption().text();

            _modalManager.setResult({
                id,
                name
            });
            _modalManager.close();
        };
    };
})();
