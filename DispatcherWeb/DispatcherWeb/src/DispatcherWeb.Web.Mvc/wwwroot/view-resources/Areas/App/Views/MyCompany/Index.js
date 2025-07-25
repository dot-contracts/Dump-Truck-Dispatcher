(function () {
    $(function () {
        const _leaseHaulerService = abp.services.app.leaseHauler;
        const _insuranceService = abp.services.app.insurance;
        const _dtHelper = abp.helper.dataTables;
        const _permissions = {
            readLeaseHaulerProfile: abp.auth.hasPermission('LeaseHaulerPortal.MyCompany'),
            editLeaseHaulerProfile: abp.auth.hasPermission('LeaseHaulerPortal.MyCompany.Profile'),
            leaseHaulerContacts: abp.auth.hasPermission('LeaseHaulerPortal.MyCompany.Contacts'),
            leaseHaulerDrivers: abp.auth.hasPermission('LeaseHaulerPortal.MyCompany.Drivers'),
            insurance: abp.auth.hasPermission('LeaseHaulerPortal.MyCompany.Insurance'),
            leaseHaulerTrucks: abp.auth.hasPermission('LeaseHaulerPortal.MyCompany.Trucks'),
        };

        let isDataChanged = false;
        let _leaseHaulerId = $('#leaseHaulerId').val();
        let _mailingAddressAutocomplete = null;
        let _physicalAddressAutocomplete = null;
        let _mailingAddressFields = {
            streetAddress: null,
            streetAddress2: null,
            city: null,
            state: null,
            zipCode: null,
            countryCode: null
        };
        let _physicalAddressFields = {
            streetAddress: null,
            streetAddress2: null,
            city: null,
            state: null,
            zipCode: null,
            countryCode: null
        };

        const _sendMessageModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/LeaseHaulers/SendMessageModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/LeaseHaulers/_SendMessageModal.js',
            modalClass: 'SendMessageModal'
        });
        const _createOrEditLeaseHaulerContactModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/LeaseHaulers/CreateOrEditLeaseHaulerContactModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/LeaseHaulers/_CreateOrEditLeaseHaulerContactModal.js',
            modalClass: 'CreateOrEditLeaseHaulerContactModal'
        });
        const _createOrEditLeaseHaulerTruckModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/LeaseHaulers/CreateOrEditLeaseHaulerTruckModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/LeaseHaulers/_CreateOrEditLeaseHaulerTruckModal.js',
            modalClass: 'CreateOrEditLeaseHaulerTruckModal'
        });
        const _createOrEditLeaseHaulerDriverModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/LeaseHaulers/CreateOrEditLeaseHaulerDriverModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/LeaseHaulers/_CreateOrEditLeaseHaulerDriverModal.js',
            modalClass: 'CreateOrEditLeaseHaulerDriverModal'
        });

        let _$form = $('form#LeaseHaulerGeneralSettingsForm');
        _$form.validate();

        _mailingAddressFields.streetAddress = _$form.find('#MailingAddress1');
        _mailingAddressFields.streetAddress2 = _$form.find('#MailingAddress2');
        _mailingAddressFields.city = _$form.find('#MailingCity');
        _mailingAddressFields.state = _$form.find('#MailingState');
        _mailingAddressFields.zipCode = _$form.find('#MailingZipCode');
        _mailingAddressFields.countryCode = _$form.find('#MailingCountryCode');
        _physicalAddressFields.streetAddress = _$form.find('#StreetAddress1');
        _physicalAddressFields.streetAddress2 = _$form.find('#StreetAddress2');
        _physicalAddressFields.city = _$form.find('#City');
        _physicalAddressFields.state = _$form.find('#State');
        _physicalAddressFields.zipCode = _$form.find('#ZipCode');
        _physicalAddressFields.countryCode = _$form.find('#CountryCode');

        _$form.find("#HireDate").datepicker();
        _$form.find("#TerminationDate").datepicker();

        _$form.find('#UseSameAddressAsMailingAddress').click(function () {
            let useSameAddressAsMailingAddress = $(this).is(':checked');
            for (let key in _mailingAddressFields) {
                if (!_mailingAddressFields.hasOwnProperty(key)) {
                    continue;
                }
                let mailingAddressField = _mailingAddressFields[key];
                let physicalAddressField = _physicalAddressFields[key];
                if (useSameAddressAsMailingAddress) {
                    physicalAddressField.val(mailingAddressField.val());
                }
                physicalAddressField.prop('disabled', useSameAddressAsMailingAddress);
            }
        });

        if (!_permissions.editLeaseHaulerProfile) {
            _$form.find('input, select, textarea').prop('readonly', true);
            _$form.find('input[type="checkbox"], select').prop('disabled', true);
            $('.save-profile-btn').hide();
        } else {
            initAddressControlsAsync();
        }

        // form change listener
        const initialFormData = _$form.serialize();
        _$form.on('change', function () {
            const currentFormData = _$form.serialize();
            if (initialFormData !== currentFormData) {
                isDataChanged = true;
            }
        });

        const leaseHaulerContactsTable = $('#LeaseHaulerContactsTable');
        let leaseHaulerContactsGrid;
        if (_permissions.leaseHaulerContacts) {
            leaseHaulerContactsGrid = leaseHaulerContactsTable.DataTableInit({
                serverSide: true,
                processing: true,
                language: {
                    emptyTable: app.localize("NoDataInTheTableClick{0}ToAdd", app.localize("AddNewContact"))
                },
                ajax: function (data, callback, settings) {
                    const abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, { leaseHaulerId: _leaseHaulerId });
                    _leaseHaulerService.getLeaseHaulerContacts(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                paging: false,
                info: false,
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
                        data: "phone",
                        render: function (data, type, full, meta) { return _dtHelper.renderPhone(data); },
                        title: "Office Phone"
                    },
                    {
                        data: "cellPhoneNumber",
                        render: function (data, type, full, meta) { return _dtHelper.renderPhone(data); },
                        title: "Cell Phone"
                    },
                    {
                        data: "email",
                        title: "Email"
                    },
                    {
                        data: null,
                        orderable: false,
                        autoWidth: false,
                        defaultContent: '',
                        width: "10px",
                        responsivePriority: 1,
                        rowAction: {
                            items: [
                                {
                                    text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                                    className: "btn btn-sm btn-default",
                                    action: function (data) {
                                        _createOrEditLeaseHaulerContactModal.open({ id: data.record.id });
                                    }
                                },
                                {
                                    text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                                    className: "btn btn-sm btn-default",
                                    action: function (data) {
                                        deleteLeaseHaulerContact(data.record);
                                    }
                                },
                                {
                                    text: '<i class="fas fa-comments"></i> ' + app.localize('SendSms'),
                                    className: "btn btn-sm btn-default",
                                    visible: function (data) {
                                        return data.record.cellPhoneNumber;
                                    },
                                    action: function (data) {
                                        _sendMessageModal.open({ leaseHaulerId: _leaseHaulerId, leaseHaulerContactId: data.record.id, messageType: 0 });
                                    }
                                },
                                {
                                    text: '<i class="fa fa-phone"></i> ' + app.localize('CallOffice'),
                                    className: "btn btn-sm btn-default",
                                    visible: function (data) {
                                        return data.record.phone;
                                    },
                                    action: function (data) {
                                        window.location = 'tel:' + data.record.phone;
                                    }
                                },
                                {
                                    text: '<i class="fa fa-phone"></i> ' + app.localize('CallCell'),
                                    className: "btn btn-sm btn-default",
                                    visible: function (data) {
                                        return data.record.cellPhoneNumber;
                                    },
                                    action: function (data) {
                                        window.location = 'tel:' + data.record.cellPhoneNumber;
                                    }
                                },
                                {
                                    text: '<i class="fas fa-comments"></i> ' + app.localize('SendEmail'),
                                    className: "btn btn-sm btn-default",
                                    visible: function (data) {
                                        return data.record.email;
                                    },
                                    action: function (data) {
                                        _sendMessageModal.open({ leaseHaulerId: _leaseHaulerId, leaseHaulerContactId: data.record.id, messageType: 1 });
                                    }
                                }
                            ]
                        }
                    }
                ]
            });
        }

        let insuranceDataList;
        if (_permissions.insurance) {
            insuranceDataList = $('#InsuranceDataList').dataListInit({
                listMethod: async () => await _insuranceService.getInsurances(_leaseHaulerId),
                editMethod: _insuranceService.editInsurance,
                deleteMethod: _insuranceService.deleteInsurance,
                localization: {
                    addNewButtonText: $('#AddNewInsurance').text(),
                },
                columns: [
                    {
                        title: 'Active',
                        data: 'isActive',
                        width: '100%',
                        editor: dataList.editors.checkbox,
                    },
                    {
                        title: 'Insurance Type',
                        data: 'insuranceTypeId',
                        width: '265px',
                        editor: dataList.editors.dropdown,
                        editorOptions: {
                            required: true,
                            idField: 'insuranceTypeId',
                            nameField: 'insuranceTypeName',
                            dropdownOptions: {
                                abpServiceMethod: _insuranceService.getInsuranceTypesSelectList,
                                showAll: true,
                                allowClear: false,
                            },
                        },
                        postDraw: (cell) => {
                            let insuranceTypeDropdown = cell.editor.dropdown;
                            insuranceTypeDropdown
                                .on('change', async () => {
                                    let documentType = cell.row.rowData.documentType;

                                    if (insuranceTypeDropdown.data('select2')) {
                                        let dropdownData = insuranceTypeDropdown.select2('data');
                                        if (dropdownData?.length && dropdownData[0].item) {
                                            documentType = dropdownData[0].item.documentType;
                                        }
                                    }

                                    let isInsuranceFieldVisible = documentType === abp.enums.documentType.insurance;
                                    let insuranceFields = [
                                        'brokerName',
                                        'brokerPhone',
                                        'coverageLimit',
                                    ];
                                    for (let fieldName of insuranceFields) {
                                        let editor = cell.row.getCellByDataField(fieldName).editor;
                                        editor.domElement.toggle(isInsuranceFieldVisible);
                                    }
                                });
                        },
                    },
                    {
                        title: 'Issue Date',
                        data: 'issueDate',
                        width: '123px',
                        editor: dataList.editors.datepicker,
                        editorOptions: {
                            required: true,
                        },
                    },
                    {
                        title: 'Expiration Date',
                        data: 'expirationDate',
                        width: '123px',
                        editor: dataList.editors.datepicker,
                        editorOptions: {
                            required: true,
                        },
                    },
                    {
                        title: 'Issued By',
                        data: 'issuedBy',
                        width: '265px',
                        editor: dataList.editors.text,
                        editorOptions: {
                            maxLength: abp.entityStringFieldLengths.insurance.issuedBy,
                        }
                    },
                    {
                        title: 'Issuer Phone',
                        data: 'issuerPhone',
                        width: '265px',
                        placeholder: '',
                        editor: dataList.editors.text,
                        editorOptions: {
                            maxLength: abp.entityStringFieldLengths.insurance.issuerPhone,
                            regex: app.regex.cellPhoneNumber,
                            placeholder: '(format:+15553214321)',
                        },
                    },
                    {
                        title: 'Broker Name',
                        data: 'brokerName',
                        width: '265px',
                        editor: dataList.editors.text,
                        editorOptions: {
                            maxLength: abp.entityStringFieldLengths.insurance.brokerName,
                        }
                    },
                    {
                        title: 'Broker Phone',
                        data: 'brokerPhone',
                        width: '265px',
                        editor: dataList.editors.text,
                        editorOptions: {
                            maxLength: abp.entityStringFieldLengths.insurance.brokerPhone,
                            regex: app.regex.cellPhoneNumber,
                            placeholder: '(format:+15553214321)',
                        },
                    },
                    {
                        title: 'Coverage Limit',
                        data: 'coverageLimit',
                        width: '126px',
                        editor: dataList.editors.decimal,
                        editorOptions: {
                            number: true,
                            min: 0,
                        },
                    },
                    {
                        title: 'Comments',
                        data: 'comments',
                        width: '550px',
                        editor: dataList.editors.textarea,
                    },
                    {
                        title: '',
                        data: 'fileId',
                        editor: dataList.editors.icon,
                        editorOptions: {
                            icon: 'fa-regular fa-file-image',
                            click: (rowData) => {
                                app.openPopup(abp.appPath + 'app/Insurance/GetInsurancePhoto/' + rowData.id);
                            },
                        },
                    },
                ],
                rowActions: [
                    {
                        icon: 'fa fa-upload',
                        text: 'Upload Document',
                        action: pickInsuranceDocumentAsync,
                        isVisible: (rowData) => {
                            return !!(rowData.id && !rowData.fileId);
                        },
                    },
                    {
                        icon: 'fa fa-refresh',
                        text: 'Replace Document',
                        action: pickInsuranceDocumentAsync,
                        isVisible: (rowData) => {
                            return !!(rowData.id && rowData.fileId);
                        },
                    },
                    {
                        icon: 'fa fa-trash',
                        text: 'Delete Document',
                        action: async (rowData) => {
                            if (!await abp.message.confirm('Are you sure you want to delete the document?')) {
                                return;
                            }
                            await _insuranceService.deleteInsurancePhoto({
                                insuranceId: rowData.id
                            });
                            rowData.fileId = null;
                            abp.notify.info('Document deleted.');
                        },
                        isVisible: (rowData) => {
                            return !!(rowData.id && rowData.fileId);
                        },
                    },
                ]
            });
        }

        async function pickInsuranceDocumentAsync(rowData, row) {
            abp.ui.clearBusy(row.panel);
            let file = await abp.helper.pickInsuranceDocumentAsync();
            abp.ui.setBusy(row.panel);
            let result = await _insuranceService.addInsurancePhoto({
                insuranceId: rowData.id,
                ...file,
            });
            rowData.fileId = result.fileId;
            abp.notify.info('Document added');
        }

        const leaseHaulerTrucksTable = $('#LeaseHaulerTrucksTable');
        let leaseHaulerTrucksGrid;
        if (_permissions.leaseHaulerTrucks) {
            leaseHaulerTrucksGrid = leaseHaulerTrucksTable.DataTableInit({
                serverSide: true,
                processing: true,
                language: {
                    emptyTable: app.localize("NoDataInTheTableClick{0}ToAdd", app.localize("AddNewTruck"))
                },
                ajax: function (data, callback, settings) {
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, { leaseHaulerId: _leaseHaulerId });
                    _leaseHaulerService.getLeaseHaulerTrucks(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                paging: false,
                info: false,
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
                        responsivePriority: 1,
                        data: "truckCode",
                        title: app.localize('TruckCode')
                    },
                    {
                        data: "vehicleCategoryName",
                        title: app.localize('Category')
                    },
                    {
                        data: "defaultDriverName",
                        title: app.localize('DefaultDriver')
                    },
                    {
                        data: "isActive",
                        render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(full.isActive); },
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
                            items: [
                                {
                                    text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                                    className: "btn btn-sm btn-default",
                                    action: function (data) {
                                        _createOrEditLeaseHaulerTruckModal.open({ id: data.record.id });
                                    }
                                },
                                {
                                    text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                                    className: "btn btn-sm btn-default",
                                    action: function (data) {
                                        deleteLeaseHaulerTruck(data.record);
                                    }
                                }
                            ]
                        }
                    }
                ]
            });
        }

        const leaseHaulerDriversTable = $('#LeaseHaulerDriversTable');
        let leaseHaulerDriversGrid;
        if (_permissions.leaseHaulerDrivers) {
            leaseHaulerDriversGrid = leaseHaulerDriversTable.DataTableInit({
                serverSide: true,
                processing: true,
                language: {
                    emptyTable: app.localize("NoDataInTheTableClick{0}ToAdd", app.localize("AddNewDriver"))
                },
                ajax: function (data, callback, settings) {
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, { leaseHaulerId: _leaseHaulerId });
                    _leaseHaulerService.getLeaseHaulerDrivers(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                paging: false,
                info: false,
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
                        responsivePriority: 1,
                        data: "firstName",
                        title: "First Name"
                    },
                    {
                        responsivePriority: 3,
                        data: "lastName",
                        title: "Last Name"
                    },
                    {
                        data: "isInactive",
                        render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(full.isInactive); },
                        className: "checkmark text-center",
                        width: "100px",
                        title: "Inactive"
                    },
                    {
                        data: null,
                        orderable: false,
                        autoWidth: false,
                        defaultContent: '',
                        width: "10px",
                        responsivePriority: 1,
                        rowAction: {
                            items: [
                                {
                                    text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                                    className: "btn btn-sm btn-default",
                                    action: function (data) {
                                        _createOrEditLeaseHaulerDriverModal.open({ id: data.record.id });
                                    }
                                },
                                {
                                    text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                                    className: "btn btn-sm btn-default",
                                    action: function (data) {
                                        deleteLeaseHaulerDriver(data.record);
                                    }
                                }
                            ]
                        }
                    }
                ]
            });
        }

        $('a[data-toggle="tab"]').on('shown.bs.tab', function () {
            leaseHaulerContactsGrid?.columns.adjust().responsive.recalc();
            leaseHaulerDriversGrid?.columns.adjust().responsive.recalc();
            leaseHaulerTrucksGrid?.columns.adjust().responsive.recalc();
        });

        const reloadLeaseHaulerContactsGrid = () => {
            _permissions.leaseHaulerContacts && leaseHaulerContactsGrid.ajax.reload();
        };

        const reloadLeaseHaulerTrucksGrid = () => {
            _permissions.leaseHaulerTrucks && leaseHaulerTrucksGrid.ajax.reload();
        };

        const reloadLeaseHaulerDriversGrid = () => {
            _permissions.leaseHaulerDrivers && leaseHaulerDriversGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditLeaseHaulerContactModalSaved', function () {
            reloadLeaseHaulerContactsGrid();
        });

        abp.event.on('app.createOrEditLeaseHaulerTruckModalSaved', function () {
            reloadLeaseHaulerTrucksGrid();
        });

        abp.event.on('app.createOrEditLeaseHaulerDriverModalSaved', function () {
            reloadLeaseHaulerDriversGrid();
        });

        $('#CreateNewLeaseHaulerContactButton').click(function (e) {
            e.preventDefault();
            _createOrEditLeaseHaulerContactModal.open({
                leaseHaulerId: _leaseHaulerId
            });
        });

        $('#CreateNewLeaseHaulerTruckButton').click(function (e) {
            e.preventDefault();
            _createOrEditLeaseHaulerTruckModal.open({
                leaseHaulerId: _leaseHaulerId
            });
        });

        $('#CreateNewLeaseHaulerDriverButton').click(function (e) {
            e.preventDefault();
            _createOrEditLeaseHaulerDriverModal.open({
                leaseHaulerId: _leaseHaulerId
            });
        });

        $('#AddNewInsurance').click(function (e) {
            e.preventDefault();
            insuranceDataList.addRow({
                leaseHaulerId: _leaseHaulerId,
                isActive: true,
            });
        });

        async function deleteLeaseHaulerContact(record) {
            if (await abp.message.confirm(
                'Are you sure you want to delete the contact?'
            )) {
                _leaseHaulerService.deleteLeaseHaulerContact({
                    id: record.id
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadLeaseHaulerContactsGrid();
                });
            }
        }

        async function deleteLeaseHaulerTruck(record) {
            if (await abp.message.confirm(
                'Are you sure you want to delete the truck?'
            )) {
                _leaseHaulerService.deleteLeaseHaulerTruck({
                    id: record.id
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadLeaseHaulerTrucksGrid();
                });
            }
        }

        async function deleteLeaseHaulerDriver(record) {
            if (await abp.message.confirm(
                'Are you sure you want to delete the driver?'
            )) {
                _leaseHaulerService.deleteLeaseHaulerDriver({
                    id: record.id
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadLeaseHaulerDriversGrid();
                });
            }
        }

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

            _mailingAddressAutocomplete = initAddressAutocomplete(_mailingAddressFields);
            _physicalAddressAutocomplete = initAddressAutocomplete(_physicalAddressFields);
        }

        function fillAddressFromPlace(addressFields, place) {
            var address = abp.helper.googlePlacesHelper.parseAddressComponents(place.address_components);
            addressFields.streetAddress.val(address.streetAddress);
            addressFields.city.val(address.city);
            addressFields.state.val(address.state);
            addressFields.zipCode.val(address.zipCode);
            addressFields.countryCode.val(address.countryCode);
            isDataChanged = true;
        }

        $('#SettingsTabPanel > ul > li > a[data-toggle="tab"]').on('click', function (e) {
            let isProfileTab = $(this).attr('href') === '#LeaseHaulerGeneralTab';
            $('.save-profile-btn').toggle(isProfileTab && _permissions.editLeaseHaulerProfile);
        });

        $('.save-profile-btn').click(async function (e) {
            e.preventDefault();

            if (isDataChanged && _permissions.editLeaseHaulerProfile) {
                await saveLeaseHaulerAsync();
            }
        });

        async function saveLeaseHaulerAsync() {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                throw new Error('Stopping save because form is invalid.');
            }

            if (insuranceDataList?.hasUnsavedRows()) {
                abp.message.warn('Please save or cancel the unsaved insurance policies.');
                throw new Error('Stopping save because there are unsaved insurance rows.');
            }

            try {
                abp.ui.setBusy(_$form);

                const leaseHauler = _$form.serializeFormToObject();

                await _leaseHaulerService.editLeaseHauler(leaseHauler);

                isDataChanged = false;
                abp.notify.info('Saved successfully.');
            } finally {
                abp.ui.clearBusy(_$form);
            }
        }
    });
})();
