(function () {
    $(function () {

        var _invoiceService = abp.services.app.invoice;
        var _dtHelper = abp.helper.dataTables;
        var _invoiceId = $("#Id").val();
        var _invoiceLines = null;
        var _invoiceLineGridData = null;
        var _customerInvoicingMethod = $('#CustomerInvoicingMethod').val() ? Number($('#CustomerInvoicingMethod').val()) : null;
        var showFuelSurchargeOnInvoice = Number($("#ShowFuelSurchargeOnInvoice").val());
        var showBottomFuelSurchargeLine = showFuelSurchargeOnInvoice === abp.enums.showFuelSurchargeOnInvoiceEnum.singleLineItemAtTheBottom;
        var showFuelSurchargeLinePerTicket = showFuelSurchargeOnInvoice === abp.enums.showFuelSurchargeOnInvoiceEnum.lineItemPerTicket;
        var form = $("#InvoiceForm");
        var _isCustomerPortalUser = !abp.auth.hasPermission('Pages.Invoices')
            && abp.auth.hasPermission('CustomerPortal.Invoices')
            && abp.session.customerId;
        var _permissions = {
            invoices: abp.auth.hasPermission('Pages.Invoices')
        };
        var _features = {
            charges: abp.features.isEnabled('App.Charges'),
            separateItems: abp.features.isEnabled('App.SeparateMaterialAndFreightItems'),
        };

        $('form').validate();
        $.validator.addMethod(
            "regex",
            function (value, element, regexp) {
                var re = new RegExp(regexp, 'i');
                return this.optional(element) || re.test(value);
            },
            "Please check your input."
        );
        $('#EmailAddress').rules('add', { regex: app.regex.emails });

        var _selectCustomerTicketsModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Invoices/SelectCustomerTicketsModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Invoices/_SelectCustomerTicketsModal.js',
            modalClass: 'SelectCustomerTicketsModal',
            modalSize: 'lg'
        });

        var _selectCustomerChargesModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Invoices/SelectCustomerChargesModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Invoices/_SelectCustomerChargesModal.js',
            modalClass: 'SelectCustomerChargesModal',
            modalSize: 'lg'
        });

        var _createOrEditTicketModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Tickets/CreateOrEditTicketModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Tickets/_CreateOrEditTicketModal.js',
            modalClass: 'CreateOrEditTicketModal'
        });

        var _emailInvoicePrintOutModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Invoices/EmailInvoicePrintOutModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Invoices/_EmailInvoicePrintOutModal.js',
            modalClass: 'EmailInvoicePrintOutModal'
        });

        $("#IssueDate").datepickerInit();
        $("#IssueDate").on('dp.change', function () {
            calculateDueDate();
        });
        $("#DueDate").datepickerInit();

        async function calculateDueDate() {
            var issueDate = $("#IssueDate").val();
            if (!issueDate) {
                return;
            }
            var terms = $("#Terms").val();
            var dueDate = await abp.services.app.invoice.calculateDueDate({
                issueDate: issueDate,
                terms: terms
            });
            if (dueDate) {
                $("#DueDate").val(moment(dueDate).utc().format('L'));
            }
        }

        var saveInvoiceAsync = function (callback) {
            if (!form.valid()) {
                form.showValidateMessage();
                return;
            }

            removeEmptyInvoiceLines();

            var invoice = form.serializeFormToObject();
            invoice.InvoiceLines = _invoiceLines;
            abp.ui.setBusy(form);
            _invoiceService.editInvoice(invoice).done(function (id) {
                abp.notify.info('Saved successfully.');
                _invoiceId = id;
                $("#Id").val(_invoiceId);
                history.replaceState({}, "", abp.appPath + 'app/invoices/details/' + _invoiceId);
                $("#InvoiceForm").dirtyForms('setClean');
                if (callback)
                    callback();
            }).always(function () {
                abp.ui.clearBusy(form);
            });
        };

        $("#CustomerId").select2Init({
            abpServiceMethod: abp.services.app.invoice.getActiveCustomersSelectList,
            showAll: false,
            allowClear: false
        });

        $("#CustomerId").change(function () {
            let dropdown = $("#CustomerId");
            let dropdownData = dropdown.select2('data');
            if (dropdownData && dropdownData.length) {
                if (dropdownData[0].item) {
                    let i = dropdownData[0].item;
                    $("#EmailAddress").val(i.invoiceEmail);
                    $("#BillingAddress").val(i.fullAddress);
                    $("#Terms").val(i.terms).change();
                    _customerInvoicingMethod = i.invoicingMethod;
                    $("#CustomerInvoicingMethod").val(i.invoicingMethod);
                }
                $(this).removeUnselectedOptions();
            }

            refreshAddUnbilledTicketsVisibility();
            refreshJobNumberVisibilityAndText();
        });
        refreshAddUnbilledTicketsVisibility();

        $("#SalesTaxEntityId").change(function () {
            let dropdown = $(this);
            let dropdownData = dropdown.select2('data');
            if (dropdownData && dropdownData.length) {
                if (dropdownData[0].item) {
                    $("#TaxRate").val(dropdownData[0].item.rate).change();
                } else {
                    $("#TaxRate").val(0).change().prop('disabled', false);
                }
            }
            disableSalesTaxRateIfNeeded();
        });

        function refreshAddUnbilledTicketsVisibility() {
            $("#AddUnbilledTicketsButton, #AddUnbilledChargesButton").hide();
            if (_isCustomerPortalUser) {
                return;
            }
            var customerId = $("#CustomerId").val();
            if (customerId && _invoiceLines) {
                let ticketFilter = getCustomerTicketFilter(customerId);
                let chargeFilter = getCustomerChargesFilter(customerId);
                _invoiceService.getCustomerHasTickets(ticketFilter).done(function (hasTickets) {
                    if (!hasTickets || $("#CustomerId").val() !== customerId) {
                        return;
                    }
                    $("#AddUnbilledTicketsButton").show();
                });
                if (chargeFilter.orderLineIds?.length) {
                    _invoiceService.getCustomerHasCharges(chargeFilter).done(function (hasCharges) {
                        if (!hasCharges || $("#CustomerId").val() !== customerId) {
                            return;
                        }
                        $("#AddUnbilledChargesButton").show();
                    });
                }
            }
        }

        function refreshJobNumberVisibilityAndText() {
            //$('#JobNumberBlock h4').text(getSelectedJobNumbers().join('; '));
            //$('#JobNumberBlock').toggle(_customerInvoicingMethod === abp.enums.invoicingMethod.separateTicketsByJobNumber);
            var jobNumbers = getSelectedJobNumbers().filter(x => x).join('; ');
            if (jobNumbers) {
                let maxLength = Number($("#JobNumber").attr('maxlength'));
                jobNumbers = abp.utils.truncate(jobNumbers, maxLength, true);
                $("#JobNumber").val(jobNumbers);
            }
            var poNumbers = getSelectedPoNumbers().join('; ');
            if (poNumbers) {
                let maxLength = Number($("#PoNumber").attr('maxlength'));
                jobNumbers = abp.utils.truncate(jobNumbers, maxLength, true);
                $("#PoNumber").val(poNumbers);
            }
        }

        $("#OfficeId").select2Init({
            abpServiceMethod: listCacheSelectLists.office(),
            showAll: true,
            allowClear: false,
        }).change(function () {
            refreshAddUnbilledTicketsVisibility();
        });

        $("#Terms").select2Init({
            showAll: true,
            allowClear: true
        });
        $("#Terms").change(function () {
            calculateDueDate();
        });

        $("#SalesTaxEntityId").select2Init({
            abpServiceMethod: listCacheSelectLists.taxRate(),
            showAll: true,
            allowClear: true
        });

        function disableInvoiceEdit() {
            $('input,select,textarea').attr('disabled', true);
            $('#AddInvoiceLineButton').hide();
            $('#AddUnbilledTicketsButton').hide();
            $('.SaveInvoiceButton').closest('.btn-group').hide();
            $('#PrintInvoiceButton').show();
        }

        if (_isCustomerPortalUser) {
            disableInvoiceEdit();
        } else if (_permissions.invoices) {
            $('#AddInvoiceLineButton').show();
        }

        function disableCustomerDropdownIfNeeded() {
            if (_invoiceLines && _invoiceLines.filter(x => x.ticketId || x.chargeId).length) {
                $("#CustomerId").prop('disabled', true);
            }
        }

        function disableSalesTaxRateIfNeeded() {
            if ($("#SalesTaxEntityId").val()) {
                $("#TaxRate").prop('disabled', true);
            }
        }
        disableSalesTaxRateIfNeeded();

        function toggleAllTaxControlsIfNeeded() {
            if (!_features.charges) {
                return;
            }
            let allDisabled = false;
            if (_invoiceLines) {
                let taxRates = getSelectedTaxRates(true);
                let taxEntities = getSelectedTaxEntities(true);
                allDisabled = taxRates.length || taxEntities.length;
            }
            let taxRateDisabled = !!$('#SalesTaxEntityId').val();

            $('#SalesTaxEntityId').prop('disabled', allDisabled);
            $('#TaxRate').prop('disabled', allDisabled || taxRateDisabled);
        }

        function setFormDirty() {
            var dirtyFormsField = $("#DirtyFormsField");
            var i = Number(dirtyFormsField.val());
            dirtyFormsField.val(++i).change();
        }

        function recalculateLineNumbers() {
            if (!_invoiceLines) {
                return;
            }
            let i = 1;
            _invoiceLines.forEach(x => x.lineNumber = i++)
        }

        function round(num) {
            return abp.utils.round(num);
        }

        function serializeInvoice() {
            //_receipt = $("#InvoiceForm").serializeFormToObject();
            //_receipt.FreightTotal = Number(_receipt.FreightTotal) || 0;
            //_receipt.MaterialTotal = Number(_receipt.MaterialTotal) || 0;
            //_receipt.SalesTaxRate = Number(_receipt.SalesTaxRate) || 0;
        }
        serializeInvoice();

        $('#TaxRate').change(function () {
            recalculateTotals();
            reloadInvoiceLinesGrid();
        });

        //var _recalculateTotalsInProgressCount = 0;
        function recalculateTotals(sender) {
            let senderRowData = sender ? _dtHelper.getRowData(sender) : null;
            if (!_invoiceLines) {
                return;
            }
            let totalTax = 0;
            let subtotal = 0;
            let totalAmount = 0;
            let taxRate = round($('#TaxRate').val());
            _invoiceLines.forEach(function (invoiceLine) {
                if (invoiceLine.chargeId && invoiceLine.useMaterialQuantity === true && invoiceLine.orderLineId) {
                    let materialQuantitySum = _invoiceLines
                        .filter(x => x.ticketId && x.orderLineId === invoiceLine.orderLineId)
                        .reduce((sum, x) => sum + (x.materialQuantity || 0), 0);
                    invoiceLine.materialQuantity = materialQuantitySum || 0;
                    invoiceLine.freightQuantity = materialQuantitySum || 0;
                }
                if (!invoiceLine.ticketId) {
                    invoiceLine.materialExtendedAmount = abp.utils.round((invoiceLine.materialQuantity || 0) * (invoiceLine.materialRate || 0));
                    invoiceLine.freightExtendedAmount = abp.utils.round((invoiceLine.freightQuantity || 0) * (invoiceLine.freightRate || 0));
                }
                let taxableMaterialAmount = invoiceLine.materialExtendedAmount;
                let taxableFreightAmount = invoiceLine.freightExtendedAmount;
                if (invoiceLine.chargeId) {
                    //for charges, the entire amount needs to be taxed per spec, so we'll pass the entire amount as material amount to the tax calculator
                    taxableMaterialAmount += taxableFreightAmount;
                    taxableFreightAmount = 0;
                }

                let isFreightTaxable = invoiceLine.isFreightTaxable !== false; //isTaxable == null should be processed as true
                let isMaterialTaxable = invoiceLine.isMaterialTaxable !== false;
                let calcResult = abp.helper.calculateOrderLineTotal(taxableMaterialAmount, taxableFreightAmount, isFreightTaxable, taxRate, isMaterialTaxable, isFreightTaxable);
                invoiceLine.tax = calcResult.tax;
                invoiceLine.subtotal = calcResult.subtotal || 0;
                invoiceLine.extendedAmount = calcResult.total || 0;
                if (senderRowData === invoiceLine) {
                    if (!invoiceLine.ticketId) {
                        $(sender).closest('tr').find('.freight-total-cell').text(invoiceLine.freightExtendedAmount);
                        $(sender).closest('tr').find('.material-total-cell').text(invoiceLine.materialExtendedAmount);
                        $(sender).closest('tr').find('.description-cell textarea').val(invoiceLine.description);
                    }
                    $(sender).closest('tr').find('.total-cell').text(_dtHelper.renderMoney(invoiceLine.subtotal));
                    if (senderRowData.isFreightRateOverridden) {
                        $(sender).closest('tr').find('.freight-rate-cell').addClass("overridden-price");
                    }
                }
                subtotal += invoiceLine.subtotal || 0;
                totalTax += invoiceLine.tax || 0;
                totalAmount += invoiceLine.extendedAmount || 0;
            });
            $(".Subtotal").text(abp.helper.dataTables.renderMoney(abp.utils.round(subtotal)));
            $(".TaxAmount").text(abp.helper.dataTables.renderMoney(abp.utils.round(totalTax)));
            $(".BalanceDue").text(abp.helper.dataTables.renderMoney(abp.utils.round(totalAmount)));
        }

        $("#InvoiceForm").dirtyForms(); //{ ignoreSelector: '' }

        //function getTicketFromRowData(rowData) {
        //    return {
        //        id: rowData.id,
        //        orderLineId: rowData.orderLineId,
        //        ticketNumber: rowData.ticketNumber,
        //        ticketDateTime: rowData.ticketDateTime,
        //        materialQuantity: rowData.materialQuantity,
        //        freightQuantity: rowData.freightQuantity,
        //        truckId: rowData.truckId,
        //        truckCode: rowData.truck //api is expecting 'truckCode' on edit, but sends 'truck' in grid data
        //    };
        //}

        function addEmptyRowIfNeeded() {
            if (!_invoiceLines.filter(x => !x.childInvoiceLineKind).length) {
                addEmptyInvoiceLine();
            }
        }

        var invoiceLinesTable = $('#InvoiceLinesTable');
        var invoiceLinesGrid = invoiceLinesTable.DataTableInit({
            paging: false,
            info: false,
            serverSide: true,
            ordering: false,
            processing: true,
            ajax: function (data, callback, settings) {
                if (_invoiceLineGridData) {
                    callback(_invoiceLineGridData);
                    return;
                }
                if (_invoiceId) {
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, { invoiceId: _invoiceId });
                    _invoiceService.getInvoiceLines(abpData).done(function (abpResult) {
                        _invoiceLineGridData = _dtHelper.fromAbpResult(abpResult);
                        _invoiceLines = abpResult.items;
                        addEmptyRowIfNeeded();
                        disableCustomerDropdownIfNeeded();
                        refreshAddUnbilledTicketsVisibility();
                        //refreshJobNumberVisibilityAndText();
                        toggleAllTaxControlsIfNeeded();
                        //recalculateTotals();
                        callback(_invoiceLineGridData);
                    });
                } else {
                    _invoiceLineGridData = _dtHelper.getEmptyResult();
                    _invoiceLines = _invoiceLineGridData.data;
                    addEmptyRowIfNeeded();
                    refreshAddUnbilledTicketsVisibility();
                    //refreshJobNumberVisibilityAndText();
                    callback(_invoiceLineGridData);
                }

            },
            editable: {
                saveCallback: function (rowData, cell) {
                    setFormDirty();
                    recalculateTotals(cell);
                },
                isReadOnly: (rowData) => !!rowData.childInvoiceLineKind,
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
                    data: "ticketNumber",
                    title: "Ticket #"
                    //className: "cell-text-wrap all",
                },
                {
                    data: "truckCode",
                    render: function (data, type, full, meta) {
                        return (full.leaseHaulerName ? _dtHelper.renderText(full.leaseHaulerName) + ' ' : '')
                            + (_dtHelper.renderText(full.truckCode) || '');
                    },
                    title: "Truck"
                },
                {
                    data: "jobNumber",
                    title: "Job #",
                    editable: {
                        editor: _dtHelper.editors.text,
                        maxLength: abp.entityStringFieldLengths.orderLine.jobNumber,
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return isRowReadOnly || rowData.ticketId || rowData.chargeId;
                        },
                        editCompleteCallback: function (editResult, rowData, $cell) {
                            refreshJobNumberVisibilityAndText();
                        }
                    }
                },
                {
                    data: "deliveryDateTime",
                    render: function (data, type, full, meta) { return _dtHelper.renderDate(full.deliveryDateTime); }, //utc datetime
                    title: "Date",
                    editable: {
                        editor: _dtHelper.editors.date,
                        convertDataToDisplayValue: function (data) {
                            return moment(data).format('L'); //parse as utc datetime and convert to local date
                        },
                        convertDisplayValueToData: function (displayValue, rowData) {
                            return moment(displayValue, 'L').utc().format('YYYY-MM-DDTHH:mm:ss[Z]'); //parse local date and convert to utc datetime
                        },
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return isRowReadOnly || !rowData.chargeId;
                        },
                    },
                },
                {
                    data: "freightItemName",
                    title: _features.separateItems ? "Freight Item" : "Item",
                    className: "all item-cell",
                    editable: {
                        editor: _dtHelper.editors.dropdown,
                        idField: 'freightItemId',
                        nameField: 'freightItemName',
                        dropdownOptions: {
                            abpServiceMethod: listCacheSelectLists.item(),
                            abpServiceParamsGetter: (params) => ({
                                types: _features.separateItems ? abp.enums.itemTypes.freight : null,
                            }),
                            showAll: listCache.item.isEnabled,
                            allowClear: false
                        },
                        editStartingCallback: function editStartingCallback(rowData, cell, selectedOption) {
                            if (selectedOption && selectedOption.item) {
                                rowData.isFreightTaxable = selectedOption.item.isTaxable;
                                if (!rowData.chargeId) {
                                    rowData.description = selectedOption.name;
                                }
                            }
                        },
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return isRowReadOnly || rowData.ticketId;
                        }
                    }
                },
                {
                    data: "materialItemName",
                    title: "Material Item",
                    className: "all item-cell",
                    visible: _features.separateItems,
                    editable: {
                        editor: _dtHelper.editors.dropdown,
                        idField: 'materialItemId',
                        nameField: 'materialItemName',
                        dropdownOptions: {
                            abpServiceMethod: listCacheSelectLists.item(),
                            abpServiceParamsGetter: (params) => ({
                                types: _features.separateItems ? abp.enums.itemTypes.material : null,
                            }),
                            showAll: listCache.item.isEnabled,
                            allowClear: false
                        },
                        editStartingCallback: function editStartingCallback(rowData, cell, selectedOption) {
                            if (selectedOption && selectedOption.item) {
                                rowData.isMaterialTaxable = selectedOption.item.isTaxable;
                            }
                        },
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return isRowReadOnly || rowData.ticketId || rowData.chargeId;
                        }
                    }
                },
                {
                    data: "description",
                    title: "Description",
                    width: '240px',
                    className: "all description-cell",
                    editable: {
                        editor: _dtHelper.editors.textarea,
                        maxLength: 1000,
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return isRowReadOnly || rowData.ticketId;
                        }
                    }
                },
                {
                    data: "freightQuantity",
                    title: "Freight Qty",
                    className: "all quantity-cell",
                    editable: {
                        editor: _dtHelper.editors.decimal,
                        maxValue: app.consts.maxQuantity,
                        minValue: 0,
                        allowNull: false,
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return isRowReadOnly || rowData.ticketId;
                        },
                        validate: async function (rowData, newValue) {
                            if (rowData.chargeId && rowData.useMaterialQuantity === true) {
                                if (!await abp.message.confirm('This charge was calculated from the related tickets. Are you sure you want to override this?')) {
                                    return false;
                                }
                                rowData.useMaterialQuantity = false;
                            }
                            return true;
                        }
                    }
                },
                {
                    data: "materialQuantity",
                    title: "Material Qty",
                    className: "all quantity-cell",
                    editable: {
                        editor: _dtHelper.editors.decimal,
                        maxValue: app.consts.maxQuantity,
                        minValue: 0,
                        allowNull: false,
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return isRowReadOnly || rowData.ticketId;
                        }
                    }
                },
                {
                    data: "materialRate",
                    title: "Material<br>Rate",
                    className: "all material-rate-cell",
                    editable: {
                        editor: _dtHelper.editors.decimal,
                        maxValue: app.consts.maxDecimal,
                        minValue: 0,
                        allowNull: true,
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return isRowReadOnly || rowData.ticketId;
                        }
                    }
                },
                {
                    data: "freightRate",
                    title: "Freight<br>Rate",
                    className: "all freight-rate-cell",
                    editable: {
                        editor: _dtHelper.editors.decimal,
                        maxValue: app.consts.maxDecimal,
                        minValue: 0,
                        allowNull: true,
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return isRowReadOnly && !rowData.childInvoiceLineKind || rowData.ticketId;
                        },
                        validate: function (rowData, newValue) {
                            if (rowData.childInvoiceLineKind) {
                                rowData.isFreightRateOverridden = true;
                            }
                            return true;
                        }
                    },
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        if (rowData.isFreightRateOverridden) {
                            $(cell).addClass("overridden-price");
                        }
                    }
                },
                {
                    data: "materialExtendedAmount",
                    title: "Material",
                    className: "all material-total-cell",
                    editable: {
                        editor: _dtHelper.editors.decimal,
                        maxValue: app.consts.maxDecimal,
                        minValue: 0,
                        allowNull: false,
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return true; //isRowReadOnly || !rowData.ticketId;
                        }
                    }
                },
                {
                    data: "freightExtendedAmount",
                    title: "Freight",
                    className: "all freight-total-cell",
                    editable: {
                        editor: _dtHelper.editors.decimal,
                        maxValue: 1000000,
                        minValue: 0,
                        allowNull: false,
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return true; //isRowReadOnly || !rowData.ticketId;
                        }
                    }
                },
                //{
                //    data: "fuelSurcharge",
                //    title: "Fuel",
                //    visible: abp.setting.getBoolean('App.Fuel.ShowFuelSurcharge')
                //},
                {
                    title: _features.separateItems ? "Freight Tax" : "Tax",
                    data: null,
                    className: "all checkmark text-center tax-cell",
                    editable: {
                        editor: _dtHelper.editors.checkbox,
                        fieldName: 'isFreightTaxable',
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return isRowReadOnly && !rowData.childInvoiceLineKind;
                        },
                    }
                },
                {
                    title: "Material Tax",
                    data: null,
                    className: "all checkmark text-center tax-cell",
                    visible: _features.separateItems,
                    editable: {
                        editor: _dtHelper.editors.checkbox,
                        fieldName: 'isMaterialTaxable',
                        isReadOnly: function (rowData, isRowReadOnly) {
                            return isRowReadOnly && !rowData.childInvoiceLineKind;
                        },
                    }
                },
                {
                    data: "subtotal",
                    class: "total-cell",
                    render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.subtotal); },
                    title: "Total"
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    width: "10px",
                    responsivePriority: 1,
                    render: function (data, type, full, meta) {
                        return '<div class="dropdown action-button">'
                            + '<ul class="dropdown-menu dropdown-menu-right">'
                            + (!_isCustomerPortalUser ? '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>' : '')
                            + (full.ticketId ? '<li><a class="btnViewAssociatedTicket" title="View associated ticket"><i class="fa fa-edit"></i> View associated ticket</a></li>' : '')
                            + '</ul>'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '</div>';
                    }

                }
            ]
        });

        var reloadInvoiceLinesGrid = function () {
            invoiceLinesGrid.ajax.reload();
        };

        //function getNextLineNumber() {
        //    return _invoiceLines.map(function (x) { return x.lineNumber; }).reduce((a, b) => a > b ? a : b, 0) + 1;
        //}

        $("#AddInvoiceLineButton").click(function (e) {
            e.preventDefault();
            addEmptyInvoiceLine();
            reloadInvoiceLinesGrid();
            setFormDirty();
        });

        function getBottomFuelSurchargeLine() {
            if (!_invoiceLines) {
                return null;
            }
            return _invoiceLines.find(x => x.childInvoiceLineKind === abp.enums.childInvoiceLineKind.bottomFuelSurchargeLine);
        }

        function addInvoiceLineInternal(line) {
            if (showBottomFuelSurchargeLine) {
                let bottomFuelLine = getBottomFuelSurchargeLine();
                let bottomFuelSurchargeLineIndex = _invoiceLines.indexOf(bottomFuelLine);
                if (bottomFuelSurchargeLineIndex !== -1) {
                    _invoiceLines.splice(bottomFuelSurchargeLineIndex, 0, line);
                    setAmountToFuelSurchargeLine(bottomFuelLine, bottomFuelLine.extendedAmount + line.fuelSurcharge);
                    if (bottomFuelLine.extendedAmount === 0) {
                        _invoiceLines.splice(_invoiceLines.indexOf(bottomFuelLine), 1);
                    }
                } else {
                    _invoiceLines.push(line);
                    bottomFuelLine = getNewBottomFuelSurchargeLine();
                    if (bottomFuelLine.extendedAmount !== 0) {
                        _invoiceLines.push(bottomFuelLine);
                    }
                }
            } else if (showFuelSurchargeLinePerTicket) {
                _invoiceLines.push(line);
                if (line.fuelSurcharge) {
                    _invoiceLines.push(getNewTicketFuelSurchargeLine(line));
                }
            } else {
                _invoiceLines.push(line);
            }
        }

        function getFuelItem() {
            var fuelItem = {
                id: Number($("#FuelItemId").val()),
                name: $("#FuelItemName").val(),
                isTaxable: $("#FuelItemIsTaxable").val().toLowerCase() === 'true',
            };
            if (!fuelItem.id) {
                abp.message.error(app.localize('PleaseSelectItemToUseForFuelSurchargeOnInvoiceInSettings')).then(() => {
                    location = abp.appPath + '/Settings';
                });
                throw new Error('FuelItemId is not set');
            }
            return fuelItem;
        }

        function setItemToFuelSurchargeLine(fuelLine) {
            let fuelItem = getFuelItem();
            fuelLine.description = fuelItem.name;
            fuelLine.freightItemId = fuelItem.id;
            fuelLine.freightItemName = fuelItem.name;
            fuelLine.isFreightTaxable = fuelItem.isTaxable;
        }

        function setAmountToFuelSurchargeLine(fuelLine, fuelAmount) {
            fuelLine.freightQuantity = 1;
            fuelLine.materialQuantity = 0;
            if (fuelLine.isFreightRateOverridden) {
                return;
            }
            fuelLine.freightRate = fuelAmount;
            fuelLine.freightExtendedAmount = fuelAmount;
            fuelLine.subtotal = fuelAmount;
            fuelLine.extendedAmount = fuelAmount;
        }

        function getNewBottomFuelSurchargeLine() {
            if (!showBottomFuelSurchargeLine) {
                return null;
            }
            let fuelLine = getEmptyInvoiceLine();
            setItemToFuelSurchargeLine(fuelLine);
            setAmountToFuelSurchargeLine(fuelLine, _invoiceLines.map(x => x.fuelSurcharge).reduce((a, b) => a + b, 0));
            fuelLine.deliveryDateTime = null;
            fuelLine.childInvoiceLineKind = abp.enums.childInvoiceLineKind.bottomFuelSurchargeLine;
            return fuelLine;
        }

        function getNewTicketFuelSurchargeLine(invoiceLine) {
            if (!showFuelSurchargeLinePerTicket) {
                return null;
            }
            invoiceLine.guid = invoiceLine.guid || app.guid();
            let fuelLine = getEmptyInvoiceLine();
            setItemToFuelSurchargeLine(fuelLine);
            setAmountToFuelSurchargeLine(fuelLine, invoiceLine.fuelSurcharge);
            fuelLine.deliveryDateTime = invoiceLine.deliveryDateTime;
            fuelLine.parentInvoiceLineGuid = invoiceLine.guid;
            fuelLine.childInvoiceLineKind = abp.enums.childInvoiceLineKind.fuelSurchargeLinePerTicket;
            return fuelLine;
        }

        function getEmptyInvoiceLine() {
            return {
                //isNew: true,
                id: 0,
                lineNumber: 0, //getNextLineNumber(),
                carrierId: null,
                carrierName: null,
                deliveryDateTime: null,
                description: null,
                freightItemId: null,
                freightItemName: null,
                materialItemId: null,
                materialItemName: null,
                freightQuantity: 0,
                materialQuantity: 0,
                freightRate: null,
                isFreightRateOverridden: false,
                materialRate: null,
                subtotal: 0,
                extendedAmount: 0,
                freightExtendedAmount: 0,
                leaseHaulerName: null,
                materialExtendedAmount: 0,
                tax: 0,
                isFreightTaxable: true,
                isMaterialTaxable: true,
                ticketId: null,
                chargeId: null,
                orderLineId: null,
                ticketNumber: null,
                truckCode: null,
                jobNumber: null,
                customerId: null,
                salesTaxRate: null,
                salesTaxEntityId: null,
                salesTaxEntityName: null,
                fuelSurcharge: 0,
                guid: null,
                parentInvoiceLineGuid: null,
                parentInvoiceLineId: null,
                childInvoiceLineKind: null
            };
        }

        function addEmptyInvoiceLine() {
            var newInvoiceLine = getEmptyInvoiceLine();
            addInvoiceLineInternal(newInvoiceLine);
            recalculateLineNumbers();
            //disableCustomerDropdownIfNeeded();
            //recalculateTotals();
        }

        $(".SaveInvoiceButton").click(function (e) {
            e.preventDefault();
            saveInvoiceAsync(function () {
                //reloadInvoiceLinesGrid();
                setTimeout(() => abp.ui.setBusy(form), 100);
                location.reload();
            });
        });

        $("#MarkReadyForExportButton").click(function (e) {
            e.preventDefault();
            $("#Status").val(abp.enums.invoiceStatus.readyForExport);
            saveInvoiceAsync(function () {
                //reloadInvoiceLinesGrid();
                setTimeout(() => abp.ui.setBusy(form), 100);
                location.reload();
            });
        });

        $("#SaveAndPrintButton").click(function (e) {
            e.preventDefault();
            saveInvoiceAsync(function () {
                printInvoice();
            });
        });

        $("#PrintInvoiceButton").click(function (e) {
            e.preventDefault();
            printInvoice();
        });

        async function printInvoice() {
            await _invoiceService.validateInvoiceStatusChange({
                ids: [_invoiceId],
                status: abp.enums.invoiceStatus.printed,
            });
            app.openPopup(abp.appPath + 'app/invoices/GetInvoicePrintOut?invoiceId=' + _invoiceId);
            setTimeout(() => abp.ui.setBusy(form), 100);
            setTimeout(() => location.reload(), 3000);
        }

        $("#SaveAndSendButton").click(function (e) {
            e.preventDefault();
            saveInvoiceAsync(function () {
                _emailInvoicePrintOutModal.open({ id: _invoiceId });
            });
        });

        abp.event.on('app.emailInvoicePrintOutModalSent', function (e) {
            abp.ui.setBusy(form);
            location.reload();
        });

        function getCustomerTicketFilter(customerId) {
            let filter = getCustomerBaseFilter(customerId);

            filter.isVerified = true;
            filter.hasRevenue = true;

            if (_invoiceLines) {
                let excludeTicketIds = _invoiceLines.filter(x => x.ticketId !== null).map(x => x.ticketId);
                if (excludeTicketIds.length) {
                    filter.excludeTicketIds = excludeTicketIds;
                }
            }

            return filter;
        }

        function getCustomerChargesFilter(customerId) {
            let filter = getCustomerBaseFilter(customerId);

            if (_invoiceLines) {
                let excludeChargeIds = _invoiceLines.filter(x => x.chargeId !== null).map(x => x.chargeId);
                if (excludeChargeIds.length) {
                    filter.excludeChargeIds = excludeChargeIds;
                }
                let orderLineIds = _invoiceLines.filter(x => x.orderLineId !== null).map(x => x.orderLineId);
                if (orderLineIds.length) {
                    filter.orderLineIds = orderLineIds;
                }
            }

            return filter;
        }

        function getCustomerBaseFilter(customerId) {
            let filter = {
                customerId: customerId,
                isBilled: false,
                hasInvoiceLineId: false,
                jobNumbers: _features.charges && _customerInvoicingMethod === abp.enums.invoicingMethod.separateTicketsByJobNumber
                    ? getSelectedJobNumbers()
                    : null,
                orderLineIds: _features.charges && _customerInvoicingMethod === abp.enums.invoicingMethod.separateTicketsByJob
                    ? getSelectedOrderLineIds()
                    : null,
                salesTaxRates: _features.charges ? getSelectedTaxRates() : [],
                salesTaxEntityIds: _features.charges ? getSelectedTaxEntities().map(x => x.id) : [],
            };
            if (abp.setting.getBoolean('App.General.SplitBillingByOffices')) {
                filter.officeId = $("#OfficeId").val();
            }
            return filter;
        }

        function isCustomerBaseFilterValid(customerId) {
            if (!_features.charges) {
                return true;
            }
            let filter = getCustomerBaseFilter(customerId);
            if (filter.jobNumbers?.length > 1 && _customerInvoicingMethod === abp.enums.invoicingMethod.separateTicketsByJobNumber
                || filter.orderLineIds?.length > 1 && _customerInvoicingMethod === abp.enums.invoicingMethod.separateTicketsByJob
                || filter.salesTaxRates?.length > 1
                || filter.salesTaxEntityIds?.length > 1
            ) {
                return false;
            }
            return true;
        }

        function showInvalidCustomerBaseFilterWarning() {
            abp.message.warn('This invoice has tickets or charges with multiple tax codes. It can’t be edited. You can delete this invoice and recreate it if it is not correct.');
        }

        function getSelectedJobNumbers() {
            return [...new Set(
                _invoiceLines?.filter(l => l.jobNumber || l.ticketId || l.chargeId).map(l => l.jobNumber) || []
            )];
        }

        function getSelectedOrderLineIds() {
            return [...new Set(
                _invoiceLines?.filter(l => l.orderLineId || l.ticketId || l.chargeId).map(l => l.orderLineId) || []
            )];
        }

        function getSelectedTaxRates(excludeInvoiceDetails = false) {
            let distinctResult = new Set(
                _invoiceLines.filter(l => l.ticketId || l.chargeId && l.orderLineId).map(l => l.salesTaxRate)
            );

            if (!excludeInvoiceDetails) {
                let invoiceDetailsTaxRate = abp.utils.parseStringToNullableNumber($("#TaxRate").val());
                if (invoiceDetailsTaxRate !== null) {
                    distinctResult.add(invoiceDetailsTaxRate);
                }
            }

            return [...distinctResult];
        }

        function getSelectedTaxEntities(excludeInvoiceDetails = false) {
            let result = [];
            if (_invoiceLines) {
                _invoiceLines
                    .filter(l => l.ticketId || l.chargeId && l.orderLineId)
                    .map(l => ({
                        id: l.salesTaxEntityId,
                        name: l.salesTaxEntityName,
                    }))
                    .forEach(l => {
                        if (!result.find(x => x.id === l.id)) {
                            result.push(l);
                        }
                    });
            }

            if (!excludeInvoiceDetails) {
                let selectedTaxEntityId = Number($("#SalesTaxEntityId").val());
                if (selectedTaxEntityId) {
                    let selectedTaxEntityName = $("#SalesTaxEntityId option:selected").text();
                    if (!result.find(x => x.id === selectedTaxEntityId)) {
                        result.push({
                            id: selectedTaxEntityId,
                            name: selectedTaxEntityName,
                        });
                    }
                }
            }

            return result;
        }

        function getSelectedPoNumbers() {
            let poNumbers = [];
            if (_invoiceLines) {
                _invoiceLines.map(l => l.poNumber).filter(l => l).forEach(j => {
                    if (!poNumbers.includes(j)) {
                        poNumbers.push(j);
                    }
                });
            }
            return poNumbers;
        }

        $("#AddUnbilledTicketsButton").click(function (e) {
            e.preventDefault();
            var customerId = $("#CustomerId").val();
            if (!customerId) {
                //shouldn't happen in production as the button would be hidden when there's no customer
                abp.message.warn('Please select the customer first');
                return;
            }
            if (!isCustomerBaseFilterValid(customerId)) {
                showInvalidCustomerBaseFilterWarning();
                return;
            }
            _selectCustomerTicketsModal.open().then(($modal, modal) => {
                let filter = getCustomerTicketFilter(customerId);
                let selectedJobNumbers = getSelectedJobNumbers();
                modal.setFilter(filter, _customerInvoicingMethod, selectedJobNumbers);
            });
        });

        abp.event.on('app.customerTicketsSelectedModal', function (e) {
            //let nextLineNumber = getNextLineNumber();
            let newItems = e.selectedTickets.map(x => ({
                id: 0,
                lineNumber: 0, //nextLineNumber++,
                carrierId: x.carrierId,
                carrierName: x.carrierName,
                deliveryDateTime: x.ticketDateTime,
                description: x.description,
                freightItemId: x.freightItemId,
                freightItemName: x.freightItemName,
                materialItemId: x.materialItemId,
                materialItemName: x.materialItemName,
                freightQuantity: x.freightQuantity,
                materialQuantity: x.materialQuantity,
                freightRate: x.freightRate,
                materialRate: x.materialRate,
                subtotal: x.subtotal,
                extendedAmount: x.total,
                freightExtendedAmount: x.freightTotal,
                leaseHaulerName: x.leaseHaulerName,
                materialExtendedAmount: x.materialTotal,
                tax: x.tax,
                isFreightTaxable: x.isFreightTaxable,
                isMaterialTaxable: x.isMaterialTaxable,
                ticketId: x.id,
                chargeId: null,
                orderLineId: x.orderLineId,
                ticketNumber: x.ticketNumber,
                truckCode: x.truckCode,
                jobNumber: x.jobNumber,
                customerId: x.customerId,
                salesTaxRate: x.salesTaxRate,
                salesTaxEntityId: x.salesTaxEntityId,
                salesTaxEntityName: x.salesTaxEntityName,
                poNumber: x.poNumber,
                fuelSurcharge: x.fuelSurcharge,
                guid: null,
                parentInvoiceLineGuid: null,
                parentInvoiceLineId: null,
                childInvoiceLineKind: null
            }));
            if (_features.charges
                && newItems.length
            ) {
                if (newItems[0].salesTaxEntityId) {
                    abp.helper.ui.addAndSetDropdownValue($("#SalesTaxEntityId"), newItems[0].salesTaxEntityId, newItems[0].salesTaxEntityName);
                } else {
                    $("#SalesTaxEntityId").val(null).change();
                }
                $("#TaxRate").val(newItems[0].salesTaxRate);
            }
            newItems.forEach(x => addInvoiceLineInternal(x));
            recalculateLineNumbers();
            setFormDirty();
            disableCustomerDropdownIfNeeded();
            recalculateTotals();
            reloadInvoiceLinesGrid();
            refreshAddUnbilledTicketsVisibility();
            refreshJobNumberVisibilityAndText();
            toggleAllTaxControlsIfNeeded();
            if (e.selectedTickets.length) {
                $("#OfficeId").prop('disabled', true);
            }
            setTimeout(function () {
                //fixes the issue with colspan being applied to the first cell sometimes for some reason
                reloadInvoiceLinesGrid();
            }, 1000);
        });

        $("#AddUnbilledChargesButton").click(function (e) {
            e.preventDefault();
            var customerId = $("#CustomerId").val();
            if (!customerId) {
                abp.message.warn('Please select the customer first');
                return;
            }
            if (!isCustomerBaseFilterValid(customerId)) {
                showInvalidCustomerBaseFilterWarning();
                return;
            }
            _selectCustomerChargesModal.open().then(($modal, modal) => {
                let filter = getCustomerChargesFilter(customerId);
                let selectedJobNumbers = getSelectedJobNumbers();
                modal.setFilter(filter, _customerInvoicingMethod, selectedJobNumbers);
            });
        });

        abp.event.on('app.customerChargesSelectedModal', function (e) {
            //let nextLineNumber = getNextLineNumber();
            let newItems = e.selectedCharges.map(x => ({
                id: 0,
                lineNumber: 0, //nextLineNumber++,
                carrierId: null,
                carrierName: null,
                deliveryDateTime: abp.helper.convertLocalDateTimeToUtc(x.chargeDate),
                description: x.description,
                freightItemId: x.itemId,
                freightItemName: x.itemName,
                materialItemId: null,
                materialItemName: null,
                freightQuantity: x.quantity,
                materialQuantity: 0,
                freightRate: x.freightRate,
                materialRate: x.materialRate,
                subtotal: x.subtotal,
                extendedAmount: x.total,
                freightExtendedAmount: x.freightTotal,
                leaseHaulerName: null,
                materialExtendedAmount: x.materialTotal,
                tax: x.tax,
                freightIsTaxable: x.isTaxable,
                materialIsTaxable: false,
                ticketId: null,
                chargeId: x.id,
                orderLineId: x.orderLineId,
                ticketNumber: null,
                truckCode: null,
                jobNumber: x.jobNumber,
                customerId: x.customerId,
                salesTaxRate: x.salesTaxRate,
                salesTaxEntityId: x.salesTaxEntityId,
                salesTaxEntityName: x.salesTaxEntityName,
                poNumber: x.poNumber,
                fuelSurcharge: 0,
                guid: null,
                parentInvoiceLineGuid: null,
                parentInvoiceLineId: null,
                childInvoiceLineKind: null
            }));
            if (_features.charges
                && newItems.length
            ) {
                if (newItems[0].salesTaxEntityId) {
                    abp.helper.ui.addAndSetDropdownValue($("#SalesTaxEntityId"), newItems[0].salesTaxEntityId, newItems[0].salesTaxEntityName);
                } else {
                    $("#SalesTaxEntityId").val(null).change();
                }
                $("#TaxRate").val(newItems[0].salesTaxRate);
            }
            newItems.forEach(x => addInvoiceLineInternal(x));
            recalculateLineNumbers();
            setFormDirty();
            disableCustomerDropdownIfNeeded();
            recalculateTotals();
            reloadInvoiceLinesGrid();
            refreshAddUnbilledTicketsVisibility();
            refreshJobNumberVisibilityAndText();
            toggleAllTaxControlsIfNeeded();
            if (e.selectedCharges.length) {
                $("#OfficeId").prop('disabled', true);
            }
            setTimeout(function () {
                //fixes the issue with colspan being applied to the first cell sometimes for some reason
                reloadInvoiceLinesGrid();
            }, 1000);
        });

        function getInvoiceStatus() {
            return Number($("#Status").val());
        }

        function isDraftInvoice() {
            return getInvoiceStatus() === abp.enums.invoiceStatus.draft;
        }

        invoiceLinesTable.on('click', '.btnViewAssociatedTicket', function (e) {
            e.preventDefault();
            var invoiceLine = _dtHelper.getRowData(this);
            if (invoiceLine.ticketId) {
                _createOrEditTicketModal.open({ id: invoiceLine.ticketId, readOnly: true });
            } else {
                abp.message.warn('The row doesn\'t have a ticket associated with it');
                return;
            }
        });

        function isInvoiceLineEmpty(invoiceLine) {
            return !invoiceLine.description
                && !invoiceLine.freightItemId
                && !invoiceLine.materialItemId
                && !invoiceLine.chargeId
                && !invoiceLine.ticketId
                && !invoiceLine.freightRate
                && !invoiceLine.materialRate
                && !invoiceLine.freightExtendedAmount
                && !invoiceLine.materialExtendedAmount;
        }

        function removeEmptyInvoiceLines() {
            if (!_invoiceLines) {
                return;
            }
            for (var i = 0; i < _invoiceLines.length; i++) {
                if (isInvoiceLineEmpty(_invoiceLines[i])) {
                    _invoiceLines.splice(i, 1);
                    i--;
                }
            }
            recalculateLineNumbers();
            reloadInvoiceLinesGrid();
        }

        invoiceLinesTable.on('click', '.btnDeleteRow', async function (e) {
            e.preventDefault();
            var invoiceLine = _dtHelper.getRowData(this);
            if (await abp.message.confirm(
                isDraftInvoice()
                    ? 'You are about to delete this line item. Are you sure you want to do this?'
                    : 'You are about to delete this line item and it has already been sent to the customer. Are you sure you want to do this?')
            ) {
                var index = _invoiceLines.indexOf(invoiceLine);
                if (index !== -1) {
                    _invoiceLines.splice(index, 1);
                    if (showBottomFuelSurchargeLine) {
                        let bottomFuelLine = getBottomFuelSurchargeLine();
                        if (bottomFuelLine) {
                            setAmountToFuelSurchargeLine(bottomFuelLine, bottomFuelLine.extendedAmount - invoiceLine.fuelSurcharge);
                            if (bottomFuelLine.extendedAmount === 0) {
                                _invoiceLines.splice(_invoiceLines.indexOf(bottomFuelLine), 1);
                            }
                        }
                    } else if (showFuelSurchargeLinePerTicket) {
                        let childRows = _invoiceLines.filter(x => x.parentInvoiceLineGuid && x.parentInvoiceLineGuid === invoiceLine.guid
                            || x.parentInvoiceLineId && x.parentInvoiceLineId === invoiceLine.id);
                        for (let childRow of childRows) {
                            let childRowIndex = _invoiceLines.indexOf(childRow);
                            _invoiceLines.splice(childRowIndex, 1);
                        }
                    }
                    addEmptyRowIfNeeded();
                    setFormDirty();
                    recalculateLineNumbers();
                    recalculateTotals();
                    refreshAddUnbilledTicketsVisibility();
                    refreshJobNumberVisibilityAndText();
                    toggleAllTaxControlsIfNeeded();
                    reloadInvoiceLinesGrid();
                }
            }
        });
        $("#ApproveInvoiceButton").click(function (e) {
            e.preventDefault();
            $('#Status').val(abp.enums.invoiceStatus.approved);
            saveInvoiceAsync(function () {
                abp.notify.info('Invoice Approved.');
                setTimeout(() => location.reload(), 1000);
            });
        });

        //$('#Status').on('change', enableOrDisableButtonsDependingOnInvoiceStatus);

        //enableOrDisableButtonsDependingOnInvoiceStatus();
        //function enableOrDisableButtonsDependingOnInvoiceStatus() {
        //    var $statusCtrl = $('#Status');
        //    $('#CreateNewInvoiceTicketButton, #CreateNewQuoteButton').prop('disabled', $statusCtrl.val() == $statusCtrl.data('inactive-status'));
        //}

    });
})();
