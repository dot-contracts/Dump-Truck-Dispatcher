(function () {
    $(function () {

        var _receiptService = abp.services.app.receipt;
        var _dtHelper = abp.helper.dataTables;
        var _receiptId = $("#Id").val();
        var _receipt = null;
        var _receiptLines = null;
        var _receiptLineGridData = null;
        var _isOrderReadonly = false; //temp
        var _permissions = {
            edit: abp.auth.hasPermission('Pages.Orders.Edit'),
            createItems: abp.auth.hasPermission('Pages.Orders.Edit')
        };
        var _features = {
            separateItems: abp.features.isEnabled('App.SeparateMaterialAndFreightItems'),
        };

        var _createOrEditReceiptLineModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Receipts/CreateOrEditReceiptLineModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Receipts/_CreateOrEditReceiptLineModal.js',
            modalClass: 'CreateOrEditReceiptLineModal'
        });

        var _captureOrderAuthorizationModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CaptureOrderAuthorizationModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CaptureOrderAuthorizationModal.js',
            modalClass: 'CaptureOrderAuthorizationModal'
        });

        var _emailOrderReportModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/EmailOrderReportModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_EmailOrderReportModal.js',
            modalClass: 'EmailOrderReportModal'
        });

        function refreshPaymentInfo() {
            var authorizationCaptureDate = _dtHelper.parseUtcDateTime($("#AuthorizationCaptureDateTime").val(), '');
            var authorizationDate = _dtHelper.parseUtcDateTime($("#AuthorizationDateTime").val(), '');
            if (authorizationCaptureDate !== '') {
                $("#OrderPaymentStatus").text("PAID " + authorizationCaptureDate.format('l') + " " + authorizationCaptureDate.format('LT'));
                $("#OrderPaymentStatus").closest('.order-payment-status').show();
                $("#AuthorizeChargeButton, #CaptureAuthorizationButton, #CancelAuthorizationButton, #RefundPaymentButton").hide();
                if (!_isOrderReadonly && _permissions.edit) {
                    $("#RefundPaymentButton").show();
                }
            } else if (authorizationDate !== '') {
                $("#OrderPaymentStatus").text("Authorized " + authorizationDate.format('l') + " " + authorizationDate.format('LT'));
                $("#OrderPaymentStatus").closest('.order-payment-status').show();
                $("#AuthorizeChargeButton, #CaptureAuthorizationButton, #CancelAuthorizationButton, #RefundPaymentButton").hide();
                if (!_isOrderReadonly && _permissions.edit) {
                    $("#CaptureAuthorizationButton, #CancelAuthorizationButton").show();
                }
            } else {
                $("#OrderPaymentStatus").text("Not Authorized");
                $("#OrderPaymentStatus").closest('.order-payment-status').hide();
                $("#AuthorizeChargeButton, #CaptureAuthorizationButton, #CancelAuthorizationButton, #RefundPaymentButton").hide();
                if (!_isOrderReadonly && _permissions.edit) {
                    $("#AuthorizeChargeButton").show();
                }
            }
        }
        refreshPaymentInfo();

        function loseFocusAndAwaitBackgroundTasks(callback) {
            $(':focus').blur();
            var waitingTime = 0;
            abp.ui.setBusy($("#ReceiptForm"));
            function checkBackgroundTasksAndContinue(callback) {
                waitingTime++;
                if (waitingTime > 100) { //100ms * 100, 10 seconds
                    abp.ui.clearBusy($("#ReceiptForm"));
                    abp.message.error('Something went wrong. Please refresh the page and try again.');
                    return;
                }
                if (_recalculateTotalsInProgressCount > 0) {
                    //check again in 100ms
                    setTimeout(function () {
                        checkBackgroundTasksAndContinue(callback);
                    }, 100);
                } else {
                    abp.ui.clearBusy($("#ReceiptForm"));
                    callback && callback();
                }
            }
            //let the blur trigger the background tasks first and only then check for the first time if anything is running
            setTimeout(function () {
                checkBackgroundTasksAndContinue(callback);
            }, 100);
        }

        function isReceiptLineValid(receiptLine) {
            if (designationHasMaterial(receiptLine.designation) && !isQuantityValid(receiptLine.materialQuantity)) {
                return false;
            }
            if (!designationIsMaterialOnly(receiptLine.designation) && !isQuantityValid(receiptLine.freightQuantity)) {
                return false;
            }
            return true;
        }

        function isQuantityValid(val) {
            if (val === null || val === "") {
                return false;
            }
            var num = Number(val);
            if (!isNaN(num) && num >= 0) {
                return true;
            }
            return false;
        }

        function designationHasMaterial(val) {
            return abp.enums.designations.hasMaterial.includes(Number(val));
        }

        function designationIsMaterialOnly(val) {
            return abp.enums.designations.materialOnly.includes(Number(val));
        }

        function saveReceiptAsync(callback) {
            loseFocusAndAwaitBackgroundTasks(function () {
                saveReceiptImmediatelyAsync(callback);
            });
        }

        var saveReceiptImmediatelyAsync = function (callback) {
            var form = $("#ReceiptForm");
            if (!form.valid()) {
                form.showValidateMessage();
                return;
            }

            if (isNewReceipt()) {
                var receiptLinesValid = true;
                _receiptLines && _receiptLines.forEach(function (receiptLine) {
                    if (!isReceiptLineValid(receiptLine)) {
                        receiptLinesValid = false;
                    }
                });
                if (!receiptLinesValid) {
                    abp.message.error('Please check the following: \n' + "Quantity is required on all receipt lines", 'Some of the data is invalid');
                    return;
                }

                if (!_isTotalValid) {
                    abp.message.error('The totals are incorrect. Please make sure the line items do not exceed the maximum allowed amount when summed up');
                    return;
                }
            }

            var receipt = form.serializeFormToObject();
            if (isNewReceipt()) {
                receipt.ReceiptLines = _receiptLines;
            }
            abp.ui.setBusy(form);
            _receiptService.editReceipt(receipt).done(function (data) {
                abp.notify.info('Saved successfully.');
                $("#Id").val(data);
                _receiptId = data;
                history.replaceState({}, "", abp.appPath + 'app/receipts/details/' + data);
                showEditingBlocks();
                $("#ReceiptForm").dirtyForms('setClean');

                $('#AddNewCustomerButton span').addClass('fa-edit').removeClass('fa-plus');

                reloadReceiptLinesGrid();

                if (callback)
                    callback();
            }).always(function () {
                abp.ui.clearBusy(form);
            });
        };

        function isNewReceipt() {
            return _receiptId === '0' || _receiptId === '';
        }

        function isNewOrChangedReceipt() {
            return isNewReceipt() || $("#ReceiptForm").dirtyForms('isDirty');
        }

        function showEditingBlocks() {
            $('.editing-only-block').not(":visible").slideDown();
        }

        if (!_permissions.edit) {
            $("#SaveReceiptButton").hide();

            if (!_permissions.createItems) {
                $("#CreateNewReceiptLineButton").hide();
            }
        }

        $("#DeliveryDate").datepickerInit();
        $("#ReceiptDate").datepickerInit();

        $("#CustomerId").select2Init({
            abpServiceMethod: abp.services.app.customer.getActiveCustomersSelectList,
            showAll: false,
            allowClear: true
        });

        $("#Shift").select2Init({
            showAll: true,
            allowClear: false
        });

        var quoteChildDropdown = abp.helper.ui.initChildDropdown({
            parentDropdown: $("#CustomerId"),
            childDropdown: $("#QuoteId"),
            abpServiceMethod: abp.services.app.quote.getQuotesForCustomer,
            abpServiceData: { hideInactive: true },
            optionCreatedCallback: function (option, val) {
                switch (val.item.status) {
                    case abp.enums.quoteStatus.pending: option.addClass("quote-pending"); break;
                    case abp.enums.quoteStatus.active: option.addClass("quote-active"); break;
                    case abp.enums.quoteStatus.inactive: option.addClass("quote-inactive"); break;
                }
            }
        });

        $("#QuoteId").select2Init({
            showAll: true,
            allowClear: true
        });

        $("#OfficeId").select2Init({
            abpServiceMethod: listCacheSelectLists.office(),
            showAll: true,
            allowClear: false
        });

        function disableTaxControls() {
            var taxCalculationType = abp.setting.getInt('App.Invoice.TaxCalculationType');
            switch (taxCalculationType) {
                case abp.enums.taxCalculationType.noCalculation:
                    $("#SalesTaxRate").prop('readonly', true);
                    break;
                default:
                    $("#SalesTax").prop('readonly', true);
                    break;
            }
        }
        disableTaxControls();

        $("#SalesTaxRate").change(function () {
            if ($(this).val().toString() !== _receipt.SalesTaxRate.toString()) {
                recalculateTotals();
            }
        });

        $("#SalesTax").change(function () {
            if ($(this).val().toString() !== _receipt.SalesTax.toString()) {
                recalculateTotals();
            }
        });

        abp.event.on('app.capturedOrderAuthorizationModal', function (e) {
            var date = _dtHelper.parseDateTimeAsUtc(e.authorizationCaptureDateTime, '');
            $("#AuthorizationCaptureDateTime").val(_dtHelper.renderDateTime(date, ''));
            refreshPaymentInfo();
        });

        function round(num) {
            return abp.utils.round(num);
        }

        function serializeReceipt() {
            _receipt = $("#ReceiptForm").serializeFormToObject();
            _receipt.FreightTotal = Number(_receipt.FreightTotal) || 0;
            _receipt.MaterialTotal = Number(_receipt.MaterialTotal) || 0;
            _receipt.SalesTaxRate = Number(_receipt.SalesTaxRate) || 0;
        }
        serializeReceipt();

        var _isTotalValid = true;
        var _recalculateTotalsInProgressCount = 0;
        function recalculateTotals() {
            _recalculateTotalsInProgressCount++;
            if (isNewReceipt() && _receiptLines) {
                var materialTotal = round(_receiptLines.map(function (x) { return round(x.materialAmount); }).reduce(function (a, b) { return a + b; }, 0)) || 0.00;
                var freightTotal = round(_receiptLines.map(function (x) { return round(x.freightAmount); }).reduce(function (a, b) { return a + b; }, 0)) || 0.00;
                $("#MaterialTotal").val(materialTotal);
                $("#FreightTotal").val(freightTotal);
            }
            serializeReceipt();
            var receiptTaxDetails = {
                Id: _receiptId || 0,
                SalesTaxRate: _receipt.SalesTaxRate || 0,
                SalesTax: _receipt.SalesTax || 0,
                ReceiptLines: _receiptLines
            };
            abp.services.app.receipt.calculateReceiptTotals(receiptTaxDetails).done(function (response) {
                _isTotalValid = true;
                updateReceiptTaxDetails(response);
            }).fail(function () {
                _isTotalValid = false;
            }).always(function () {
                _recalculateTotalsInProgressCount--;
            });
        }

        function updateReceiptTaxDetails(receiptTaxDetails) {
            if (!receiptTaxDetails) {
                return;
            }
            $("#FreightTotal").val(round(receiptTaxDetails.freightTotal).toFixed(2));
            $("#MaterialTotal").val(round(receiptTaxDetails.materialTotal).toFixed(2));
            $("#SalesTaxRate").val(receiptTaxDetails.salesTaxRate);
            $("#SalesTax").val(round(receiptTaxDetails.salesTax).toFixed(2));
            $("#Total").val(round(receiptTaxDetails.codTotal).toFixed(2));
        }

        $("#ReceiptForm").dirtyForms();

        var receiptLinesTable = $('#ReceiptLinesTable');
        var receiptLinesGrid = receiptLinesTable.DataTableInit({
            paging: false,
            info: false,
            ordering: false,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                if (isNewReceipt()) {
                    if (_receiptLineGridData) {
                        callback(_receiptLineGridData);
                        return;
                    }
                    $.extend(abpData, { orderId: $("#OrderId").val() });
                } else {
                    $.extend(abpData, { receiptId: _receiptId });
                }
                _receiptService.getReceiptLines(abpData).done(function (abpResult) {
                    _receiptLineGridData = _dtHelper.fromAbpResult(abpResult);
                    _receiptLines = abpResult.items;
                    recalculateTotals();
                    showEditingBlocks();
                    callback(_receiptLineGridData);
                });
            },
            footerCallback: function (tfoot, data, start, end, display) {
                var materialTotal = $("#MaterialTotal").val();
                var freightTotal = $("#FreightTotal").val();

                let grid = this;
                let setTotalFooterValue = function (columnName, total, visible) {
                    let footerCell = grid.api().column(columnName + ':name').footer();
                    $(footerCell).html(visible ? "Total: " + _dtHelper.renderMoney(total) : '');
                }
                setTotalFooterValue('materialAmount', materialTotal, data.length);
                setTotalFooterValue('freightAmount', freightTotal, data.length);
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
                    data: "lineNumber",
                    title: "Line #"
                },
                {
                    data: "loadAtName",
                    title: "Load At",
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        if (rowData.hasQuoteBasedPricing) {
                            $(cell).addClass("quote-based-pricing");
                        }
                    }
                },
                {
                    data: "deliverToName",
                    title: "Deliver To"
                },
                {
                    data: "freightItemName",
                    title: _features.separateItems ? "Freight Item" : "Item",
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        if (rowData.hasQuoteBasedPricing) {
                            $(cell).addClass("quote-based-pricing");
                        }
                    }
                },
                {
                    data: "materialItemName",
                    title: "Material Item",
                    visible: _features.separateItems,
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        if (rowData.hasQuoteBasedPricing) {
                            $(cell).addClass("quote-based-pricing");
                        }
                    }
                },
                {
                    data: "materialUomName",
                    title: "Material UOM",
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        if (rowData.hasQuoteBasedPricing) {
                            $(cell).addClass("quote-based-pricing");
                        }
                    }
                },
                {
                    data: "freightUomName",
                    title: "Freight UOM",
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        if (rowData.hasQuoteBasedPricing) {
                            $(cell).addClass("quote-based-pricing");
                        }
                    }
                },
                {
                    data: "designationName",
                    title: "Designation"
                },
                {
                    data: "materialRate",
                    title: "Material Rate",
                    render: function (data, type, full, meta) { return _dtHelper.renderMoneyUnrounded(data); },
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        if (rowData.isMaterialRateOverridden) {
                            $(cell).addClass("overridden-price");
                        }
                    }
                },
                {
                    data: "freightRate",
                    title: "Freight Rate",
                    render: function (data, type, full, meta) { return _dtHelper.renderMoneyUnrounded(data); },
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        if (rowData.isFreightRateOverridden) {
                            $(cell).addClass("overridden-price");
                        }
                    }
                },
                {
                    data: "materialQuantity",
                    title: "Material<br>Quantity",
                    width: "68px"
                },
                {
                    data: "freightQuantity",
                    title: "Freight<br>Quantity",
                    width: "68px"
                },
                {
                    data: "materialAmount",
                    name: "materialAmount",
                    render: function (data, type, full, meta) {
                        return _dtHelper.renderMoney(full.materialAmount);
                    },
                    title: "Material",
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        if (rowData.isMaterialAmountOverridden) {
                            $(cell).addClass("overridden-price");
                        }
                    }
                },
                {
                    data: "freightAmount",
                    name: "freightAmount",
                    render: function (data, type, full, meta) {
                        return _dtHelper.renderMoney(full.freightAmount);
                    },
                    title: "Freight",
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        if (rowData.isFreightAmountOverridden) {
                            $(cell).addClass("overridden-price");
                        }
                    }
                },
                {
                    data: null,
                    orderable: false,
                    visible: _permissions.edit, //&& !_isOrderReadonly
                    name: "Actions",
                    className: "actions",
                    width: "10px",
                    responsivePriority: 1,
                    render: function (data, type, full, meta) {
                        //if (!_isOrderReadonly) {
                        return '<div class="dropdown">'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '<ul class="dropdown-menu dropdown-menu-right">'
                            + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                            + ' <li> <a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                            + '</ul>'
                            + '</div>'
                            ;
                        //} else {
                        //    return '';
                        //}
                    }
                }
            ]
        });

        var reloadReceiptLinesGrid = function (callback, resetPaging) {
            resetPaging = resetPaging === undefined ? true : resetPaging;
            receiptLinesGrid.ajax.reload(callback, resetPaging);
        };

        abp.event.on('app.createOrEditReceiptLineModalSaved', function (e) {
            if (isNewReceipt()) {
                recalculateTotals();
                reloadReceiptLinesGrid();
            } else {
                updateReceiptTaxDetails(e.orderTaxDetails);
                $("#ReceiptForm").dirtyForms('setClean');
                reloadReceiptLinesGrid();
            }
        });

        $("#SaveReceiptButton").click(function (e) {
            e.preventDefault();
            saveReceiptAsync();
        });

        $("#PrintReceiptButton").click(function (e) {
            e.preventDefault();
            printReceipt(app.order.getReceiptReportOptions());
        });

        $("#EmailReceiptButton").click(function (e) {
            e.preventDefault();
            saveReceiptAsync(function () {
                _emailOrderReportModal.open({ id: _receiptId, useReceipts: true });
            });
        });

        function printReceipt(additionalOptions) {
            if (isNewOrChangedReceipt()) {
                saveReceiptAsync(function () {
                    openWorkOrderReport(additionalOptions);
                });
            } else {
                openWorkOrderReport(additionalOptions);
            }
        }

        function openWorkOrderReport(additionalOptions) {
            var options = $.extend({ id: _receiptId }, additionalOptions);
            app.openPopup(abp.appPath + 'app/orders/GetWorkOrderReport?' + $.param(options));
        }

        $("#CaptureAuthorizationButton").click(function (e) {
            e.preventDefault();
            saveReceiptAsync(function () {
                _captureOrderAuthorizationModal.open({
                    id: _receiptId
                });
            });
        });

        $("#CreateNewReceiptLineButton").click(function (e) {
            e.preventDefault();
            if (isNewReceipt()) {
                //saveReceiptAsync(function () {
                //    reloadReceiptLinesGrid();
                //    _createOrEditReceiptLineModal.open({ receiptId: _receiptId });
                //});
                var newReceiptLine = {
                    isNew: true,
                    lineNumber: _receiptLines.map(function (x) { return x.lineNumber; }).reduce(function (a, b) { return a > b ? a : b; }, 0) + 1
                };
                _createOrEditReceiptLineModal.open({}).done(function (modal, modalObject) {
                    modalObject.setReceiptLine(newReceiptLine);
                    modalObject.saveCallback = function () {
                        _receiptLines.push(newReceiptLine);
                        recalculateTotals();
                        reloadReceiptLinesGrid();
                    };
                });
            } else {
                _createOrEditReceiptLineModal.open({ receiptId: _receiptId });
            }
        });

        receiptLinesTable.on('click', '.btnEditRow', function (e) {
            e.preventDefault();
            var receiptLine = _dtHelper.getRowData(this);
            if (isNewReceipt()) {
                receiptLine.isNew = false;
                _createOrEditReceiptLineModal.open({}).done(function (modal, modalObject) {
                    modalObject.setReceiptLine(receiptLine);
                    modalObject.saveCallback = function () {
                        recalculateTotals();
                        reloadReceiptLinesGrid();
                    };
                });
            } else {
                _createOrEditReceiptLineModal.open({ id: receiptLine.id });
            }
        });

        receiptLinesTable.on('click', '.btnDeleteRow', async function (e) {
            e.preventDefault();
            var receiptLine = _dtHelper.getRowData(this);
            if (await abp.message.confirm('Are you sure you want to delete the item?')) {
                if (isNewReceipt()) {
                    var index = _receiptLines.indexOf(receiptLine);
                    if (index !== -1) {
                        _receiptLines.splice(index, 1);
                        updateLineNumbers();
                        recalculateTotals();
                        reloadReceiptLinesGrid();
                    }
                } else {
                    _receiptService.deleteReceiptLines({
                        ids: [receiptLine.id]
                    }).done(function (e) {
                        abp.notify.info('Successfully deleted.');
                        updateReceiptTaxDetails(e.orderTaxDetails);
                        $("#ReceiptForm").dirtyForms('setClean');
                        reloadReceiptLinesGrid();
                    });
                }
            }
        });

        function updateLineNumbers() {
            if (!isNewReceipt() || !_receiptLines) {
                return;
            }
            _receiptLines.map((receiptLine, index) => receiptLine.lineNumber = index + 1);
        }

        $('[data-toggle=m-tooltip]').tooltip();
    });
})();
