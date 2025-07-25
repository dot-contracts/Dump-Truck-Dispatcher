(function ($) {
    app.modals.CreateOrEditCustomerContactModal = function () {

        var _modalManager;
        var _customerService = abp.services.app.customer;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();
            $.validator.addMethod(
                "regex",
                function (value, element, regexp) {
                    var re = new RegExp(regexp, 'i');
                    return this.optional(element) || re.test(value);
                },
                "Please check your input."
            );
            _$form.find('#Email').rules('add', { regex: app.regex.email });
            _$form.find('#PhoneNumber').rules('add', { regex: app.regex.cellPhoneNumber });
            _$form.find('#Fax').rules('add', { regex: app.regex.cellPhoneNumber });
            _$form.find('#Email').change(e => {
                var email = $(e.target).val();
                var customerPortalCheckboxIsVisible = _$form.valid() && email.length > 0;
                _$form.find('#HasCustomerPortalAccessContainer').toggle(customerPortalCheckboxIsVisible);
            });
        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var customerContact = _$form.serializeFormToObject();

            try {

                _modalManager.setBusy(true);
                let duplicateCount = await _customerService.getCustomerContactDuplicateCount({
                    customerId: customerContact.CustomerId,
                    exceptId: customerContact.Id,
                    name: customerContact.Name
                });

                if (duplicateCount) {
                    _modalManager.setBusy(false);
                    if (!await abp.message.confirm(
                        'This customer already has a contact by this name. Are you sure you want to add this contact?'
                    )) {
                        return;
                    }
                }

                _modalManager.setBusy(true);
                let newId = await _customerService.editCustomerContact(customerContact);
                abp.notify.info('Saved successfully.');
                customerContact.Id = newId;
                abp.event.trigger('app.createOrEditCustomerContactModalSaved', {
                    item: customerContact
                });
                _modalManager.setResult(customerContact);
                _modalManager.close();
            }
            finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);
