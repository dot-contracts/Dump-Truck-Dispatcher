(function ($) {
    app.modals.CreateOrEditCustomerModal = function () {

        var _modalManager;
        var _customerService = abp.services.app.customer;
        var _dtHelper = abp.helper.dataTables;
        var _permissions = {
            merge: abp.auth.hasPermission('Pages.Customers.Merge')
        };
        var _$form = null;
        var _customerId = null;
        var _billingAddressAutocomplete = null;
        var _physicalAddressAutocomplete = null;
        var _billingAddressFields = {
            streetAddress: null,
            streetAddress2: null,
            city: null,
            state: null,
            zipCode: null,
            countryCode: null
        };
        var _physicalAddressFields = {
            streetAddress: null,
            streetAddress2: null,
            city: null,
            state: null,
            zipCode: null,
            countryCode: null
        };
        var _pricingTierDropdown = null;

        var saveCustomerAsync = function (callback) {
            if (!_$form.valid()) {
                return;
            }
            var customer = _$form.serializeFormToObject();

            abp.ui.setBusy(_$form);
            _modalManager.setBusy(true);
            _customerService.editCustomer(customer).done(function (editResult) {
                abp.notify.info('Saved successfully.');
                _customerId = editResult.id;
                _$form.find("#Id").val(_customerId);
                abp.event.trigger('app.createOrEditCustomerModalSaved', {
                    item: editResult
                });
                if (callback)
                    callback(editResult);
            }).always(function () {
                abp.ui.clearBusy(_$form);
                _modalManager.setBusy(false);
            });
        };

        var _runningDuplicateCustomerNameChecks = [];
        function warnIfDuplicateCustomerName(customerName) {
            if (_customerId || !customerName) {
                return Promise.resolve(false);
            }

            //The blur event on the 'Name' input might happen simultaneously with clicking Save button, so we are merging all function calls made for the same customer name, and only do the check and show the prompt once
            var runningCheck = _runningDuplicateCustomerNameChecks.find(x => x.name === customerName);
            if (runningCheck) {
                return runningCheck.promise;
            }

            var promise = warnIfDuplicateCustomerNameInternal(customerName);
            _runningDuplicateCustomerNameChecks.push({
                name: customerName,
                promise: promise
            });

            return promise;
        }

        async function warnIfDuplicateCustomerNameInternal(customerName) {
            try {
                abp.ui.setBusy(_$form);
                let customer = await _customerService.getCustomerIfExistsOrNull({ name: customerName });
                if (!customer) {
                    return false;
                }

                if (await abp.message.confirm(
                    'This customer name already exists in the database. If you continue, you may be adding a duplicate of an existing record. Are you sure you want to continue?',
                )) {
                    return false;
                } else {
                    abp.event.trigger('app.customerNameExists', { item: customer });
                    _modalManager.setResult(customer);
                    _modalManager.close();
                    return true;
                }
            }
            finally {
                abp.ui.clearBusy(_$form);
            }
        }

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
            _$form.find('#InvoiceEmail').rules('add', { regex: app.regex.emails });

            _customerId = _$form.find("#Id").val();
            _billingAddressFields.streetAddress = _$form.find('#BillingAddress1');
            _billingAddressFields.streetAddress2 = _$form.find('#BillingAddress2');
            _billingAddressFields.city = _$form.find('#BillingCity');
            _billingAddressFields.state = _$form.find('#BillingState');
            _billingAddressFields.zipCode = _$form.find('#BillingZipCode');
            _billingAddressFields.countryCode = _$form.find('#BillingCountryCode');
            _physicalAddressFields.streetAddress = _$form.find('#Address1');
            _physicalAddressFields.streetAddress2 = _$form.find('#Address2');
            _physicalAddressFields.city = _$form.find('#City');
            _physicalAddressFields.state = _$form.find('#State');
            _physicalAddressFields.zipCode = _$form.find('#ZipCode');
            _physicalAddressFields.countryCode = _$form.find('#CountryCode');
            _pricingTierDropdown = _$form.find('#PricingTierId');

            _pricingTierDropdown.select2Init({
                abpServiceMethod: abp.services.app.pricingTier.getPricingTiersSelectList,
                showAll: true,
                allowClear: true
            });

            if (abp.features.isEnabled('App.PricingTiersFeature') && !_pricingTierDropdown.val()) {
                abp.services.app.pricingTier.getPricingTiersSelectList({}).done(function (result) {
                    let defaultPricingTier = result.items.find(x => x.isDefault);
                    if (defaultPricingTier) {
                        abp.helper.ui.addAndSetDropdownValue(_pricingTierDropdown, defaultPricingTier.id, defaultPricingTier.name);
                    }
                });
            }

            var _createOrEditCustomerContactModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Customers/CreateOrEditCustomerContactModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Customers/_CreateOrEditCustomerContactModal.js',
                modalClass: 'CreateOrEditCustomerContactModal'
            });

            _$form.find('#Name').on('blur', async function () {
                var customerName = $(this).val();
                await warnIfDuplicateCustomerName(customerName);
            });

            var preferredDeliveryMethodSelect = _modalManager.getModal().find('#PreferredDeliveryMethod');
            preferredDeliveryMethodSelect.select2Init({
                showAll: true,
                allowClear: true
            });

            var termsSelect = _modalManager.getModal().find('#Terms');
            termsSelect.select2Init({
                showAll: true,
                allowClear: true
            });

            var invoicingMethodSelect = _modalManager.getModal().find('#InvoicingMethod');
            invoicingMethodSelect.select2Init({
                showAll: true,
                allowClear: false
            });

            var customerContactsTable = _modalManager.getModal().find('#CustomerContactsTable');
            var customerContactsGrid = customerContactsTable.DataTableInit({
                paging: false,
                serverSide: true,
                processing: true,
                info: false,
                language: {
                    emptyTable: app.localize("NoDataInTheTableClick{0}ToAdd", app.localize("AddNewContact"))
                },
                ajax: function (data, callback, settings) {
                    if (_customerId === '') {
                        callback(_dtHelper.getEmptyResult());
                        return;
                    }
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, { customerId: _customerId });
                    _customerService.getCustomerContacts(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                dataMergeOptions: {
                    enabled: _permissions.merge && _customerId !== '',
                    description: "The selected customer contacts are about to be merged into one entry. Select the contact that you would like them to be merged into. The other contacts will be deleted. There is no undoing this process. If you don't want this to happen, press cancel.",
                    entitiesName: 'contacts',
                    dropdownServiceMethod: _customerService.getCustomerContactsByIdsSelectList,
                    mergeServiceMethod: _customerService.mergeCustomerContacts
                },
                columns: [
                    {
                        width: '20px',
                        className: 'control responsive',
                        orderable: false,
                        render: function () {
                            return '';
                        }
                    },
                    {
                        data: "name",
                        title: "Name"
                    },
                    {
                        data: "title",
                        title: "Title"
                    },
                    {
                        width: "80px",
                        data: "phoneNumber",
                        title: "Phone"
                    },
                    {
                        data: "fax",
                        title: "Fax"
                    },
                    {
                        data: "email",
                        title: "Email"
                    },
                    {
                        data: "isActive",
                        render: function (isActive) { return _dtHelper.renderCheckbox(isActive); },
                        className: "checkmark text-center",
                        title: "Active"
                    },
                    {
                        data: null,
                        orderable: false,
                        autoWidth: false,
                        defaultContent: '',
                        width: "10px",
                        responsivePriority: 1,
                        rowAction: {
                            items: [{
                                text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                                className: "btn btn-sm btn-default",
                                action: function (data) {
                                    _createOrEditCustomerContactModal.open({ id: data.record.id });
                                }
                            }, {
                                text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                                className: "btn btn-sm btn-default",
                                action: function (data) {
                                    deleteCustomerContact(data.record);
                                }
                            }]
                        }
                    }
                ]
            });

            _modalManager.getModal().on('shown.bs.modal', function () {
                customerContactsGrid
                    .columns.adjust()
                    .responsive.recalc();
            });

            var reloadCustomerContactGrid = function () {
                customerContactsGrid.ajax.reload();
            };

            abp.event.on('app.createOrEditCustomerContactModalSaved', function () {
                reloadCustomerContactGrid();
            });

            _modalManager.getModal().find("#CreateNewCustomerContactButton").click(function (e) {
                e.preventDefault();
                if (_customerId === '') {
                    saveCustomerAsync(function () {
                        _createOrEditCustomerContactModal.open({ customerId: _customerId });
                    });
                } else {
                    _createOrEditCustomerContactModal.open({ customerId: _customerId });
                }
            });

            _modalManager.getModal().find("#IsUseSameAddressAsBillingAddress").click(function (e) {
                if ($('#IsUseSameAddressAsBillingAddress').is(':checked')) {
                    $('#Address1').val($('#BillingAddress1').val());
                    $('#Address2').val($('#BillingAddress2').val());
                    $('#City').val($('#BillingCity').val());
                    $('#State').val($('#BillingState').val());
                    $('#ZipCode').val($('#BillingZipCode').val());
                    $('#CountryCode').val($('#BillingCountryCode').val());
                    $('#Address1').prop('disabled', true);
                    $('#Address2').prop('disabled', true);
                    $('#City').prop('disabled', true);
                    $('#State').prop('disabled', true);
                    $('#ZipCode').prop('disabled', true);
                    $('#CountryCode').prop('disabled', true);
                } else {
                    $('#Address1').prop('disabled', false);
                    $('#Address2').prop('disabled', false);
                    $('#City').prop('disabled', false);
                    $('#State').prop('disabled', false);
                    $('#ZipCode').prop('disabled', false);
                    $('#CountryCode').prop('disabled', false);
                }
            });

            async function deleteCustomerContact(record) {
                if (await abp.message.confirm(
                    'Are you sure you want to delete the contact?'
                )) {
                    _customerService.deleteCustomerContact({
                        id: record.id
                    }).done(function () {
                        abp.notify.info('Successfully deleted.');
                        reloadCustomerContactGrid();
                    });
                }
            }

            initAddressControlsAsync();
        };

        this.save = async function () {
            var customer = _$form.serializeFormToObject();
            if (await warnIfDuplicateCustomerName(customer.Name)) {
                return;
            }
            saveCustomerAsync(function (editResult) {
                _modalManager.setResult(editResult);
                _modalManager.close();
            });
        };

        async function initAddressControlsAsync() {
            await abp.maps.waitForGoogleMaps();

            function initAddressAutocomplete(addressFields) {
                var autocompleteControl = addressFields.streetAddress[0];

                var autocomplete = new google.maps.places.Autocomplete(autocompleteControl, {
                    fields: ["address_components", "geometry", "place_id", "name"],
                    types: ["address"],
                });

                autocomplete.addListener("place_changed", () => {
                    let place = autocomplete.getPlace();
                    fillAddressFromPlace(addressFields, place);
                });

                return autocomplete;
            }

            _billingAddressAutocomplete = initAddressAutocomplete(_billingAddressFields);
            _physicalAddressAutocomplete = initAddressAutocomplete(_physicalAddressFields);
        }

        function fillAddressFromPlace(addressFields, place) {
            var address = abp.helper.googlePlacesHelper.parseAddressComponents(place.address_components);

            addressFields.streetAddress.val(address.streetAddress);
            addressFields.city.val(address.city);
            addressFields.state.val(address.state);
            addressFields.zipCode.val(address.zipCode);
            addressFields.countryCode.val(address.countryCode);
        }
    };
})(jQuery);
