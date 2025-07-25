(function ($) {
    app.modals.CreateOrEditQuoteModal = function () {
        let _modalManager;
        let _quoteService = abp.services.app.quote;
        var _quoteHistoryService = abp.services.app.quoteHistory;
        let _quoteId = null;
        let _dtHelper = abp.helper.dataTables;
        let _permissions = {
            edit: abp.auth.hasPermission('Pages.Quotes.Edit'),
            createItems: abp.auth.hasPermission('Pages.Quotes.Items.Create')
        };
        let _features = {
            separateItems: abp.features.isEnabled('App.SeparateMaterialAndFreightItems')
        };

        let _requireSaveBeforeEditingQuoteLines = false;

        let _$form = null;
        let _$quoteIdInput = null;
        let _$customerInput = null;
        let _$officeInput = null;
        let _$proposalDateInput = null;
        let _$proposalExpiryDateInput = null;
        let _$inactivationDateInput = null;
        let _$quoteSalesPersonIdInput = null;
        let _$quoteFuelSurchargeCalculationIdInput = null;

        let _$baseFuelCostInput = null;
        let _$statusInput = null;
        let _$contactIdInput = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            const _createOrEditQuoteLineModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Quotes/CreateOrEditQuoteLineModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Quotes/_CreateOrEditQuoteLineModal.js',
                modalClass: 'CreateOrEditQuoteLineModal'
            });

            const _createOrEditCustomerModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Customers/CreateOrEditCustomerModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Customers/_CreateOrEditCustomerModal.js',
                modalClass: 'CreateOrEditCustomerModal',
                modalSize: 'lg'
            });

            const _createOrEditCustomerContactModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Customers/CreateOrEditCustomerContactModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Customers/_CreateOrEditCustomerContactModal.js',
                modalClass: 'CreateOrEditCustomerContactModal'
            });

            const _emailQuoteReportModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Quotes/EmailQuoteReportModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Quotes/_EmailQuoteReportModal.js',
                modalClass: 'EmailQuoteReportModal'
            });

            const _viewQuoteDeliveriesModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Quotes/ViewQuoteDeliveriesModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Quotes/_ViewQuoteDeliveriesModal.js',
                modalClass: 'ViewQuoteDeliveriesModal',
                modalSize: 'lg'
            });

            const _createOrEditQuoteModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Quotes/CreateOrEditQuoteModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Quotes/_CreateOrEditQuoteModal.js',
                modalClass: 'CreateOrEditQuoteModal',
                modalSize: 'xxl',
            });

            const _viewQuoteHistoryModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/QuoteHistory/ViewQuoteHistoryModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/QuoteHistory/_ViewQuoteHistoryModal.js',
                modalClass: 'ViewQuoteHistoryModal',
                modalSize: 'lg'
            });

            _$form = _modalManager.getModal().find('form');
            //_$form.validate();

            _$quoteIdInput = _$form.find('#Id');
            _$nameInput = _$form.find('#Name');
            _$customerInput = _$form.find('#QuoteCustomer');
            _$officeInput = _$form.find('#QuoteOffice');
            _$proposalDateInput = _$form.find('#ProposalDate');
            _$proposalExpiryDateInput = _$form.find('#ProposalExpiryDate');
            _$inactivationDateInput = _$form.find('#InactivationDate');
            _$quoteSalesPersonIdInput = _$form.find('#QuoteSalesPersonId');
            _$quoteFuelSurchargeCalculationIdInput = _$form.find('#QuoteFuelSurchargeCalculationId');
            _$baseFuelCostInput = _$form.find('#BaseFuelCost');
            _$statusInput = _$form.find('#Status');
            _$contactIdInput = _$form.find('#ContactId');
            _$taxExemptCheckbox = _$form.find('#QuoteIsTaxExempt');

            function isNewOrChangedQuote() {
                return _quoteId === '' || _$form.find("#QuoteForm").dirtyForms('isDirty');
            }

            if (!_permissions.edit) {
                _modalManager.getModal().find(".save-quote-button").hide();
                _$form.find("#CopyQuoteButton").hide();
                _$form.find("#DeleteSelectedQuoteLinesButton").hide();

                if (!_permissions.createItems) {
                    _$form.find("#CreateNewQuoteLineButton").hide();
                }
            }

            _$proposalDateInput.datepickerInit();
            _$proposalExpiryDateInput.datepickerInit();
            _$inactivationDateInput.datepickerInit({
                useCurrent: false
            });

            _$quoteSalesPersonIdInput.select2Init({
                abpServiceMethod: abp.services.app.user.getSalespersonsSelectList,
                showAll: false,
                allowClear: false
            });

            _$statusInput.select2Init({
                showAll: true,
                allowClear: false
            });

            _$contactIdInput.select2Init({
                showAll: false,
                allowClear: true,
                addItemCallback: async function (newItemName) {
                    var customerId = _$customerInput.val();
                    if (!customerId) {
                        abp.notify.warn("Select a customer first");
                        _$customerInput.focus();
                        return false;
                    }
                    var result = await app.getModalResultAsync(
                        _createOrEditCustomerContactModal.open({ name: newItemName, customerId: customerId })
                    );
                    contactChildDropdown.updateChildDropdown(function () {
                        contactChildDropdown.childDropdown.val(result.Id).change();
                    });
                    return false;
                }
            });

            _$customerInput.select2Init({
                abpServiceMethod: abp.services.app.customer.getActiveCustomersSelectList,
                showAll: false,
                allowClear: false,
                addItemCallback: async function (newItemName) {
                    var result = await app.getModalResultAsync(
                        _createOrEditCustomerModal.open({ name: newItemName })
                    );
                    _$taxExemptCheckbox.prop('checked', result.isTaxExempt);
                    return {
                        id: result.id,
                        name: result.name
                    };
                }
            });

            _$officeInput.select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: false
            });

            _$quoteFuelSurchargeCalculationIdInput.select2Init({
                abpServiceMethod: listCacheSelectLists.fuelSurchargeCalculation(),
                showAll: true,
                allowClear: true
            });

            _$form.find('[data-toggle="tooltip"]').tooltip();

            _quoteId = _$quoteIdInput.val();

            const contactChildDropdown = abp.helper.ui.initChildDropdown({
                parentDropdown: _$customerInput,
                childDropdown: _$contactIdInput,
                abpServiceMethod: abp.services.app.customer.getContactsForCustomer
            });

            _$customerInput.change(function () {
                var dropdownData = _$customerInput.select2('data');
                if (dropdownData && dropdownData.length) {
                    if (dropdownData[0].item) {
                        _$taxExemptCheckbox.prop('checked', dropdownData[0].item.isTaxExempt);
                    }
                }
                _requireSaveBeforeEditingQuoteLines = true;
            });

            _$statusInput.change(function () {
                var option = _$statusInput.getSelectedDropdownOption().val();
                if (option == abp.enums.quoteStatus.pending || option == abp.enums.quoteStatus.active) {
                    _$inactivationDateInput.val('');
                } else {
                    var now = moment(new Date());
                    _$inactivationDateInput.val(moment(now).format('MM/DD/YYYY'));
                }
            });

            _$quoteFuelSurchargeCalculationIdInput.change(function () {
                let dropdownData = _$quoteFuelSurchargeCalculationIdInput.select2('data');
                let selectedOption = dropdownData && dropdownData.length && dropdownData[0];
                let canChangeBaseFuelCost = selectedOption?.item?.canChangeBaseFuelCost || false;
                _$form.find("#BaseFuelCostContainer").toggle(canChangeBaseFuelCost);
                _$baseFuelCostInput.val(selectedOption?.item?.baseFuelCost || 0);
                _$quoteFuelSurchargeCalculationIdInput.removeUnselectedOptions();
            });

            const quoteLinesTable = _$form.find('#QuoteLinesTable');
            const quoteLinesGrid = quoteLinesTable.DataTableInit({
                paging: false,
                info: false,
                ordering: true,
                language: {
                    emptyTable: app.localize("NoDataInTheTableClick{0}ToAdd", app.localize("AddQuoteItem"))
                },
                ajax: function (data, callback, settings) {
                    if (_quoteId === "") {
                        callback(_dtHelper.getEmptyResult());
                        return;
                    }

                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, { quoteId: _quoteId });

                    _quoteService.getQuoteLines(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    })
                },
                footerCallback: function (tfoot, data, start, end, display) {
                    const materialTotal = data.map(x => x.extendedMaterialPrice).reduce((a, b) => a + b, 0);
                    const serviceTotal = data.map(x => x.extendedServicePrice).reduce((a, b) => a + b, 0);

                    let grid = this;
                    let setTotalFooterValue = function (columnName, total, visible) {
                        let footerCell = grid.api().column(columnName + ':name').footer();
                        $(footerCell).html(visible ? "Total: " + _dtHelper.renderMoney(total) : '');
                    }

                    setTotalFooterValue('extendedMaterialPrice', materialTotal, data.length);
                    setTotalFooterValue('extendedServicePrice', serviceTotal, data.length);
                },
                massDeleteOptions: {
                    enabled: _permissions.edit,
                    deleteButton: _$form.find("#DeleteSelectedQuoteLinesButton"),
                    deleteServiceMethod: _quoteService.deleteQuoteLines
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
                        data: "id",
                        visible: false
                    },
                    {
                        responsivePriority: 2,
                        data: "loadAtName",
                        title: "Load At"
                    },
                    {
                        responsivePriority: 2,
                        data: "deliverToName",
                        title: "Deliver To"
                    },
                    {
                        data: "freightItemName",
                        title: _features.separateItems ? "Freight" : "Item"
                    },
                    {
                        data: "materialItemName",
                        title: "Material",
                        visible: _features.separateItems 
                    },
                    {
                        data: "truckCategory",
                        orderable: false,
                        render: function(data, type, full, meta) { return _dtHelper.renderText(full.truckCategory.join(',')); },
                        title: "Truck Category"
                    },
                    {
                        data: "materialUomName",
                        title: "Material UOM"
                    },
                    {
                        data: "freightUomName",
                        title: "Freight UOM"
                    },
                    {
                        data: "designation",
                        render: function (data, type, full, meta) { return _dtHelper.renderText(full.designationName); },
                        title: "Designation",
                        orderable: false
                    },
                    {
                        data: "pricePerUnit",
                        render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.pricePerUnit); },
                        title: "Material Rate"
                    },
                    {
                        data: "freightRate",
                        render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.freightRate); },
                        title: "Freight Rate"
                    },
                    {
                        data: "leaseHaulerRate",
                        render: function (data, type, full, meta) { return _dtHelper.renderMoney(data); },
                        title: "LH Rate",
                        visible: abp.setting.getBoolean('App.LeaseHaulers.ShowLeaseHaulerRateOnQuote')
                    },
                    {
                        data: "materialQuantity",
                        title: "Material<br>Quantity"
                    },
                    {
                        data: "freightQuantity",
                        title: "Freight<br>Quantity"
                    },
                    {
                        data: "extendedMaterialPrice",
                        render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.extendedMaterialPrice); },
                        name: "extendedMaterialPrice",
                        title: "Extended<br>Material Price",
                        orderable: false
                    },
                    {
                        data: "extendedServicePrice",
                        render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.extendedServicePrice); },
                        name: "extendedServicePrice",
                        title: "Extended<br>Freight Price",
                        orderable: false
                    },
                    {
                        data: null,
                        orderable: false,
                        visible: _permissions.edit,
                        width: "10px",
                        className: "actions",
                        responsivePriority: 3,
                        defaultContent: '<div class="dropdown action-button">'
                            + '<ul class="dropdown-menu dropdown-menu-right">'
                            + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                            + '<li><a class="btnShowDeliveriesForRow" title="Show deliveries"><i class="fa fa-truck-loading"></i>Show deliveries</a></li>'
                            + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                            + '</ul>'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '</div>'
                    }
                ]
            });

            const reloadQuoteLinesGrid = () => {
                quoteLinesGrid.ajax.reload();
            };

            _$form.find("#CreateNewQuoteLineButton").click(async function (e) {
                e.preventDefault();

                if (_quoteId === "" || _requireSaveBeforeEditingQuoteLines) {
                    await saveQuoteAsync();
                    reloadQuoteLinesGrid();
                    reloadQuoteHistoryGrid();
                }
                _createOrEditQuoteLineModal.open({ quoteId: _quoteId });
            });

            _modalManager.getModal().find(".save-quote-button").click(async function (e) {
                e.preventDefault();
                await saveQuoteAsync();
                _modalManager.close();
            });

            _modalManager.on('app.createOrEditQuoteLineModalSaved', function () {
                reloadQuoteLinesGrid();
                reloadQuoteHistoryGrid();
            });

            quoteLinesTable.on('click', '.btnEditRow', async function (e) {
                e.preventDefault();
                let quoteLineId = _dtHelper.getRowData(this).id;
                if (_requireSaveBeforeEditingQuoteLines) {
                    await saveQuoteAsync();
                }
                _createOrEditQuoteLineModal.open({ id: quoteLineId });
            });

            quoteLinesTable.on('click', '.btnDeleteRow', async function (e) {
                e.preventDefault();
                let quoteLineId = _dtHelper.getRowData(this).id;
                if (await abp.message.confirm('Are you sure you want to delete the item?')) {
                    _quoteService.deleteQuoteLines({
                        ids: [quoteLineId]
                    }).done(function () {
                        abp.notify.info('Successfully deleted.');
                        reloadQuoteLinesGrid();
                    });
                }
            });

            quoteLinesTable.on('click', '.btnShowDeliveriesForRow', function (e) {
                e.preventDefault();
                var row = _dtHelper.getRowData(this);
                _viewQuoteDeliveriesModal.open({
                    quoteLineId: row.id,
                    quotedMaterialQuantity: row.materialQuantity || 0,
                    quotedFreightQuantity: row.freightQuantity || 0
                });
            });

            abp.helper.ui.initCannedTextLists();
            var quoteHistoryTable = _$form.find('#QuoteHistoryTable');
            var quoteHistoryGrid = quoteHistoryTable.DataTableInit({
                ajax: function (data, callback, settings) {
                    if (_quoteId === '') {
                        callback(_dtHelper.getEmptyResult());
                        return;
                    }
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, { quoteId: _quoteId });
                    _quoteHistoryService.getQuoteHistory(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                columns: [
                    {
                        width: '20px',
                        className: 'control responsive',
                        orderable: false,
                        render: function () {
                            return '';
                        },
                    },
                    {
                        data: "dateTime",
                        render: function (data, type, full, meta) { return _dtHelper.renderUtcDateTime(full.dateTime); },
                        title: "When changed"
                    },
                    {
                        data: "changedByUserName",
                        render: function (data, type, full, meta) { return _dtHelper.renderText(full.changedByUserName); },
                        title: "Changed by"
                    },
                    {
                        data: "changeType",
                        render: function (data, type, full, meta) { return _dtHelper.renderText(full.changeTypeName); },
                        title: "Type of change"
                    },
                    {
                        data: null,
                        orderable: false,
                        name: "Actions",
                        width: "10px",
                        responsivePriority: 1,
                        className: "actions",
                        defaultContent: '<div class="dropdown action-button">'
                            + '<ul class="dropdown-menu dropdown-menu-right">'
                            + '<li><a class="btnShowHistoryDetails"><i class="fa fa-edit"></i> Details</a></li>'
                            + '</ul>'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '</div>'
                    }
                ],
                order: [[1, "desc"]]

            });

            var reloadQuoteHistoryGrid = function () {
                quoteHistoryGrid.ajax.reload();
            };

            quoteHistoryTable.on('click', '.btnShowHistoryDetails', function (e) {
                e.preventDefault();
                var quoteHistoryId = _dtHelper.getRowData(this).id;
                _viewQuoteHistoryModal.open({
                    id: quoteHistoryId,
                    hideGoToQuoteButton: true
                });
            });

            _$form.find("#CopyQuoteButton").click(async function (e) {
                e.preventDefault();
                await saveQuoteAsync();
                try {
                    abp.ui.setBusy();
                    let newQuoteId = await _quoteService.copyQuote({
                        id: _quoteId
                    });
                    _modalManager.close();

                    _createOrEditQuoteModal.open({ id: newQuoteId });
                } finally {
                    abp.ui.clearBusy();
                }
            });

            function openReport() {
                abp.helper.promptForHideLoadAtOnQuote().then(function (hideLoadAt) {
                    app.openPopup(abp.appPath + 'app/quotes/getreport?quoteId=' + _quoteId + '&hideLoadAt=' + hideLoadAt);
                });
            }

            _$form.find("#PrintQuoteButton").click(async function (e) {
                e.preventDefault();
                if (isNewOrChangedQuote() || _$baseFuelCostInput.val() === '') { //force the user to enter the 'base fuel cost' value before allowing to print the quote
                    await saveQuoteAsync();
                }
                openReport();
            });

            _$form.find("#EmailQuoteButton").click(async function (e) {
                e.preventDefault();
                await saveQuoteAsync();
                _emailQuoteReportModal.open({ id: _quoteId });
            });
        }

        this.focusOnDefaultElement = function () {
            if (_quoteId === "") {
                _$customerInput.data('select2').focus();
            }
        };

        const saveQuoteAsync = async () => {
            if (!_$form) {
                throw new Error('Stopping save because form is not available yet.');
            }
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                throw new Error('Stopping save because form is invalid.');
            }

            if (!abp.helper.validateStartEndDates(
                { value: _$form.find("#ProposalDate").val(), title: _$form.find('label[for="ProposalDate"]').text() },
                { value: _$form.find("#ProposalExpiryDate").val(), title: _$form.find('label[for="ProposalExpiryDate"]').text() },
                { value: _$form.find("#InactivationDate").val(), title: _$form.find('label[for="InactivationDate"]').text() }
            )) {
                throw new Error('Stopping save because form is invalid.');
            }

            let quote = _$form.serializeFormToObject();

            try {
                abp.ui.setBusy(_$form);
                _modalManager.setBusy(true);

                let editResult = await _quoteService.editQuote(quote);

                abp.notify.info('Saved successfully.');
                _$quoteIdInput.val(editResult);
                _quoteId = editResult;
                _requireSaveBeforeEditingQuoteLines = false;

                _$form.dirtyForms('setClean');

                abp.event.trigger('app.createOrEditQuoteModalSaved');
            } finally {
                abp.ui.clearBusy(_$form);
                _modalManager.setBusy(false);
            }
        };
    }
})(jQuery);
