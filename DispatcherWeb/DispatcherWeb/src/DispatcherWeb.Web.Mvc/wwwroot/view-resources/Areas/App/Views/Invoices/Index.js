(function () {
    $(function () {

        var _invoiceService = abp.services.app.invoice;
        var _ticketService = abp.services.app.ticket;
        //var _quickbooksOnlineService = abp.services.app.quickbooksOnline;
        var _dtHelper = abp.helper.dataTables;
        var _settings = {
            quickbooksIntegrationKind: abp.setting.getInt('App.Invoice.Quickbooks.IntegrationKind'),
            allowInvoiceApprovalFlow: abp.setting.getBoolean('App.Invoice.AllowInvoiceApprovalFlow'),
        };
        var _isQuickbooksIntegrationEnabled = _settings.quickbooksIntegrationKind !== abp.enums.quickbooksIntegrationKind.none;
        var _isFilterReady = false;
        var _isGridInitialized = false;
        var _permissions = {
            invoices: abp.auth.hasPermission('Pages.Invoices'),
            approveInvoices: abp.auth.hasPermission('Pages.Invoices.ApproveInvoices'),
            exportTickets: abp.auth.hasPermission('Pages.Tickets.Export') || abp.auth.hasPermission('CustomerPortal.TicketList.Export'),
        };
        var _features = {
            approveInvoices: abp.features.isEnabled('App.AllowInvoiceApprovalFlow'),
        };
        var _isCustomerPortalUser = !abp.auth.hasPermission('Pages.Invoices')
            && abp.auth.hasPermission('CustomerPortal.Invoices')
            && abp.session.customerId;

        var _emailInvoicePrintOutModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Invoices/EmailInvoicePrintOutModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Invoices/_EmailInvoicePrintOutModal.js',
            modalClass: 'EmailInvoicePrintOutModal'
        });

        var _emailOrPrintApprovedInvoicesModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Invoices/EmailOrPrintApprovedInvoicesModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Invoices/_EmailOrPrintApprovedInvoicesModal.js',
            modalClass: 'EmailOrPrintApprovedInvoicesModal'
        });

        $('[data-toggle="tooltip"]').tooltip();

        app.localStorage.getItem('InvoicesFilter', function (cachedFilter) {
            if (!cachedFilter) {
                cachedFilter = {
                };
            }

            var forceEmptyFilter = $('#BatchIdFilter').val() !== '';
            if (forceEmptyFilter) {
                cachedFilter = {};
            }

            if (_isCustomerPortalUser) {
                cachedFilter.customerId = abp.session.customerId;
                cachedFilter.customerName = abp.session.customerName;
                $("#CustomerIdFilter").prop("disabled", true);
            }

            var dateFilterIsEmpty = false;

            if (!cachedFilter.issueDateStart || cachedFilter.issueDateStart === 'Invalid date') {
                dateFilterIsEmpty = true;
                //still need to init the daterangepicker with real dates first and clear the inputs only after the init.
                cachedFilter.issueDateStart = moment().format("MM/DD/YYYY");
            }

            if (!cachedFilter.issueDateEnd || cachedFilter.issueDateEnd === 'Invalid date') {
                dateFilterIsEmpty = true;
                cachedFilter.issueDateEnd = moment().add(1, 'days').format("MM/DD/YYYY");
            }

            if (forceEmptyFilter) {
                $('#IssueDateStartFilter').val('').change();
                $('#IssueDateEndFilter').val('').change();
                $('#IssueDateFilter').val('').change();
            } else {
                $('#IssueDateStartFilter').val(cachedFilter.issueDateStart);
                $('#IssueDateEndFilter').val(cachedFilter.issueDateEnd);
                $('#IssueDateFilter').val($('#IssueDateStartFilter').val() + ' - ' + $('#IssueDateEndFilter').val());
            }

            $("#IssueDateFilter").daterangepicker({
                autoUpdateInput: false,
                locale: {
                    cancelLabel: 'Clear'
                }
            }, function (start, end, label) {
                $("#IssueDateStartFilter").val(start.format('MM/DD/YYYY'));
                $("#IssueDateEndFilter").val(end.format('MM/DD/YYYY'));
            });

            $("#IssueDateFilter").on('blur', function () {
                var startDate = $("#IssueDateStartFilter").val();
                var endDate = $("#IssueDateEndFilter").val();
                $(this).val(startDate && endDate ? startDate + ' - ' + endDate : '');
            });

            $("#IssueDateFilter").on('apply.daterangepicker', function (ev, picker) {
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
                $("#IssueDateStartFilter").val(picker.startDate.format('MM/DD/YYYY'));
                $("#IssueDateEndFilter").val(picker.endDate.format('MM/DD/YYYY'));
                reloadMainGrid();
            });

            $("#IssueDateFilter").on('cancel.daterangepicker', function (ev, picker) {
                $(this).val('');
                $("#IssueDateStartFilter").val('');
                $("#IssueDateEndFilter").val('');
                reloadMainGrid();
            });

            if (dateFilterIsEmpty) {
                $("#IssueDateFilter").val('');
                $("#IssueDateStartFilter").val('');
                $("#IssueDateEndFilter").val('');
            }

            $("#StatusFilter").select2Init({
                showAll: true,
                allowClear: true
            });
            if (cachedFilter.status) {
                abp.helper.ui.addAndSetDropdownValue($("#StatusFilter"), cachedFilter.status);
            }

            $("#CustomerIdFilter").select2Init({
                abpServiceMethod: abp.services.app.customer.getActiveCustomersSelectList,
                abpServiceParams: { includeInactiveWithInvoices: true },
                showAll: false,
                allowClear: true
            });
            if (cachedFilter.customerId) {
                abp.helper.ui.addAndSetDropdownValue($("#CustomerIdFilter"), cachedFilter.customerId, cachedFilter.customerName);
            }

            $("#OfficeIdFilter").select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: true
            });
            if (cachedFilter.officeId) {
                abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), cachedFilter.officeId, cachedFilter.officeName);
            }

            $("#InvoiceIdFilter").val(cachedFilter.invoiceId);
            //$("#BatchIdFilter").val(cachedFilter.batchId);
            $("#UploadBatchIdFilter").val(cachedFilter.uploadBatchId);
            $("#TicketNumberFilter").val(cachedFilter.ticketNumber);

            _isFilterReady = true;
            if (_isGridInitialized) {
                reloadMainGrid(null, false);
            }
        });

        var invoicesTable = $('#InvoicesTable');
        var invoicesGrid = invoicesTable.DataTableInit({
            stateSave: true,
            stateDuration: 0,
            ajax: function (data, callback, settings) {
                if (!_isGridInitialized) {
                    _isGridInitialized = true;
                }
                if (!_isFilterReady) {
                    callback(_dtHelper.getEmptyResult());
                    return;
                }
                var abpData = _dtHelper.toAbpData(data);
                var filterData = _dtHelper.getFilterData();
                app.localStorage.setItem('InvoicesFilter', filterData);
                $.extend(abpData, _dtHelper.getFilterData());
                _invoiceService.getInvoices(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            order: [[2, 'desc']],
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
                    data: 'id',
                    width: '25px',
                    orderable: false,
                    render: function (data, type, full, meta) {
                        var icon = abp.helper.ui.getEmailDeliveryStatusIcon(full.calculatedEmailDeliveryStatus);
                        if (!icon) {
                            return '';
                        }
                        return $("<div>").append(icon).html();
                    },
                    title: ""
                },
                {
                    data: "id",
                    title: "Inv #",
                    width: "50px"
                },
                {
                    data: "issueDate",
                    render: function (data, type, full, meta) { return _dtHelper.renderUtcDate(full.issueDate); },
                    title: "Issue Date",
                    width: "150px"
                },
                {
                    responsivePriority: 1,
                    data: "customerName",
                    title: "Customer"
                },
                {
                    data: "jobNumberSort",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.jobNumber); },
                    title: "Job Nbr"
                },
                {
                    data: "totalAmount",
                    render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.totalAmount); },
                    title: "Total",
                    width: "150px"
                },
                {
                    data: "status",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.statusName); },
                    title: "Status",
                    width: "150px"
                },
                {
                    data: "quickbooksExportDateTime",
                    render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(full.quickbooksExportDateTime !== null); },
                    visible: _isQuickbooksIntegrationEnabled,
                    title: "Exported",
                    class: "checkmark",
                    width: "20px"
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    responsivePriority: 2,
                    width: '10px',
                    render: function (data, type, full, meta) {
                        return '<div class="dropdown">'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '<ul class="dropdown-menu dropdown-menu-right">'
                            + (_permissions.invoices
                                ? '<li><a class="btnEditRow dropdown-item" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                                : '')
                            + (_permissions.invoices && _permissions.approveInvoices && _settings.allowInvoiceApprovalFlow && full.status !== abp.enums.invoiceStatus.approved
                                ? '<li><a class="btnApproveRow dropdown-item" title="Approve"><i class="fa fa-thumbs-up"></i>Approve</a></li>'
                                : '')
                            + (_isCustomerPortalUser
                                ? '<li><a class="btnEditRow dropdown-item" title="View"><i class="fa fa-edit"></i> View</a></li>'
                                : '')
                            + (_permissions.invoices && _isQuickbooksIntegrationEnabled && full.quickbooksExportDateTime
                                ? '<li><a class="btnUndoInvoiceExportForRow dropdown-item"><i class="fas fa-undo"></i> Change Status to allow exporting</a></li>'
                                : '')
                            + (_permissions.invoices
                                ? '<li><a class="btnDeleteRow dropdown-item" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                                : '')
                            + (_permissions.invoices || _isCustomerPortalUser
                                ? '<li><a class="btnPrintRow dropdown-item" title="Print"><i class="fa fa-print"></i> Print</a></li>'
                                : '')
                            + (_permissions.invoices && (!_features.approveInvoices || !_settings.allowInvoiceApprovalFlow || (_permissions.approveInvoices || full.status === abp.enums.invoiceStatus.approved))
                                ? '<li><a class="btnEmailRow dropdown-item" title="Email"><i class="fa fa-envelope"></i> Email</a></li>'
                                : '')
                            + (_permissions.invoices && full.customerHasMaterialCompany
                                ? '<li><a class="btnSendInvoiceTicketsToCustomerTenantForRow dropdown-item"><i class="fa fa-share"></i> ' + app.localize('SendTicketsToCustomerTenant') + '</a></li>'
                                : '')
                            + (_permissions.exportTickets
                                ? '<li><a class="btnCreateTicketsFileForRow dropdown-item"><i class="fa fa-ticket-alt"></i> ' + app.localize('CreateTicketsFile') + '</a></li>'
                                : '')
                            + (_permissions.invoices || _isCustomerPortalUser
                                ? '<li><a class="btnDownloadTicketImagesForRow dropdown-item"><i class="fa-regular fa-file-image"></i> ' + app.localize('DownloadTicketImages') + '</a></li>'
                                : '')
                            + '</ul>'
                            + '</div>';
                    }
                }
            ]
        });

        function reloadMainGrid(preservePaging) {
            let resetPaging = !preservePaging;
            invoicesGrid.ajax.reload(null, resetPaging);
        }

        $("#CreateNewInvoiceButton").click(function (e) {
            e.preventDefault();
            window.location = abp.appPath + 'app/Invoices/Details/';
        });

        invoicesTable.on('click', '.btnEditRow', function () {
            var invoiceId = _dtHelper.getRowData(this).id;
            window.location = abp.appPath + 'app/Invoices/Details/' + invoiceId;
        });

        invoicesTable.on('click', '.btnApproveRow', async function () {
            var invoice = _dtHelper.getRowData(this);
            invoice.status = abp.enums.invoiceStatus.approved;
            try {
                abp.ui.setBusy();
                await _invoiceService.updateInvoiceStatus(invoice).done(function () {
                    abp.notify.info('Invoice Approved.');
                    let preservePaging = true; //!_dtHelper.getFilterData().status;
                    reloadMainGrid(preservePaging);
                });
            }
            finally {
                abp.ui.clearBusy();
            }
        });

        invoicesTable.on('click', '.btnDeleteRow', async function (e) {
            e.preventDefault();
            var row = _dtHelper.getRowData(this);
            var invoiceId = row.id;
            var isExported = row.quickbooksExportDateTime !== null;
            //if (row.status !== abp.enums.invoiceStatus.draft) {
            //    abp.message.error(app.localize('InvoiceDeleteErrorNotDraft'));
            //    return;
            //}
            if (await abp.message.confirm(
                app.localize(isExported ? 'ExportedInvoiceDeletePrompt' : 'InvoiceDeletePrompt', row.id)
            )) {
                _invoiceService.deleteInvoice({
                    id: invoiceId
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        });

        invoicesTable.on('click', '.btnUndoInvoiceExportForRow', async function (e) {
            e.preventDefault();
            var row = _dtHelper.getRowData(this);
            var invoiceId = row.id;
            if (await abp.message.confirm(
                app.localize('UndoInvoiceExportPrompt')
            )) {
                _invoiceService.undoInvoiceExport({
                    id: invoiceId
                }).done(function () {
                    abp.notify.info('Successfully saved.');
                    let filter = _dtHelper.getFilterData();
                    let preservePaging = true; //!filter.uploadBatchId && !filter.status;
                    reloadMainGrid(preservePaging);
                });
            }
        });

        invoicesTable.on('click', '.btnPrintRow', async function () {
            var invoiceId = _dtHelper.getRowData(this).id;
            await _invoiceService.validateInvoiceStatusChange({
                ids: [invoiceId],
                status: abp.enums.invoiceStatus.printed,
            });
            //setTimeout(() => window.location.reload(), 5000);
            setTimeout(() => reloadMainGrid(), 3000);
            app.openPopup(abp.appPath + 'app/invoices/GetInvoicePrintOut?invoiceId=' + invoiceId);
        });

        invoicesTable.on('click', '.btnEmailRow', function () {
            var invoiceId = _dtHelper.getRowData(this).id;
            _emailInvoicePrintOutModal.open({ id: invoiceId });
        });

        abp.event.on('app.emailInvoicePrintOutModalSent', function () {
            let filter = _dtHelper.getFilterData();
            let preservePaging = true; //!filter.status;
            reloadMainGrid(preservePaging);
        });

        invoicesTable.on('click', '.btnSendInvoiceTicketsToCustomerTenantForRow', async function () {
            var invoiceId = _dtHelper.getRowData(this).id;
            abp.ui.setBusy();
            try {
                await _invoiceService.sendInvoiceTicketsToCustomerTenant({ id: invoiceId });
                abp.notify.info('Sent successfully');
            }
            finally {
                abp.ui.clearBusy();
            }
        });

        invoicesTable.on('click', '.btnCreateTicketsFileForRow', function () {
            var invoiceId = _dtHelper.getRowData(this).id;
            abp.ui.setBusy();
            abp.services.app.ticket
                .getTicketsToCsv({ invoiceId: invoiceId })
                .done(function (result) {
                    app.downloadTempFile(result);
                }).always(function () {
                    abp.ui.clearBusy();
                });
        });

        invoicesTable.on('click', '.btnDownloadTicketImagesForRow', async function () {
            var invoiceId = _dtHelper.getRowData(this).id;
            var button = $(this);
            abp.ui.setBusy(button);
            try {
                if (!await abp.services.app.ticket.invoiceHasTicketPhotos(invoiceId)) {
                    abp.message.info(app.localize('InvoiceDoesntHaveTicketImages'));
                    return;
                }
                const result = await _ticketService.getTicketPhotosForInvoice(invoiceId);
                app.downloadTempFile(result);
            }
            finally {
                abp.ui.clearBusy(button);
            }
        });

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            $("#IssueDateStartFilter").val('');
            $("#IssueDateEndFilter").val('');
            $("#BatchIdFilter").val('');
            if (_isCustomerPortalUser) {
                abp.helper.ui.addAndSetDropdownValue($("#CustomerIdFilter"), abp.session.customerId, abp.session.customerName);
            }
            reloadMainGrid();
        });

        /*
        $("#UpdateQboButton").click(function (e) {
            e.preventDefault();
            let button = $(this);
            abp.ui.setBusy(button);
            _quickbooksOnlineService.uploadInvoices().done(function (result) {
                if (result.errorList.length) {
                    abp.message.warn(result.errorList.join('; \n'), 'Some of the invoices weren\'t uploaded');
                } else if (result.uploadedInvoicesCount) {
                    abp.notify.success('Invoices added to QuickBooks');
                } else {
                    abp.notify.warn('There were no invoices to upload');
                }
            }).always(function () {
                abp.ui.clearBusy(button);
                reloadMainGrid();
            });
        });
        */

        $("#ExportButton").click(async function (e) {
            e.preventDefault();

            let exportOptions = _dtHelper.getFilterData();
            exportOptions.includeExportedInvoices = !await abp.message.confirmWithYesNoCancel(app.localize('NotIncludeExportedInvoicesPrompt'));

            let button = $(this);
            let quickbooksIntegrationKind = abp.setting.getInt('App.Invoice.Quickbooks.IntegrationKind');
            let file = null;
            abp.ui.setBusy(button);
            try {
                switch (quickbooksIntegrationKind) {
                    case abp.enums.quickbooksIntegrationKind.desktop:
                        file = await abp.services.app.quickbooksDesktop.exportInvoicesToIIF(exportOptions);
                        break;
                    case abp.enums.quickbooksIntegrationKind.qboExport:
                    case abp.enums.quickbooksIntegrationKind.transactionProExport:
                    case abp.enums.quickbooksIntegrationKind.sbtCsvExport:
                    case abp.enums.quickbooksIntegrationKind.hollisExport:
                    case abp.enums.quickbooksIntegrationKind.sageExport:
                    case abp.enums.quickbooksIntegrationKind.jandjExport:
                        file = await abp.services.app.quickbooksOnlineExport.exportInvoicesToCsv(exportOptions);
                        break;
                    case abp.enums.quickbooksIntegrationKind.online:
                    case abp.enums.quickbooksIntegrationKind.none:
                    default:
                        break;
                }

                if (file) {
                    app.downloadTempFile(file);
                    reloadMainGrid();
                }
            } finally {
                abp.ui.clearBusy(button);
            }
        });

        $("#EmailOrPrintApprovedInvoicesButton").click(async function (e) {
            e.preventDefault();
            var $button = $(this);
            try {
                $button.prop('disabled', true);
                abp.ui.setBusy($button);
                var hasApprovedInvoices = await _invoiceService.hasApprovedInvoices();
                if (!hasApprovedInvoices.hasApprovedInvoicesToPrint && !hasApprovedInvoices.hasApprovedInvoicesToEmail) {
                    abp.message.warn('There are no invoices to send or print');
                    return;
                }
                if (hasApprovedInvoices.hasApprovedInvoicesToEmail) {
                    abp.ui.clearBusy($button);
                    await app.getModalResultAsync(
                        _emailOrPrintApprovedInvoicesModal.open()
                    );
                }
                if (hasApprovedInvoices.hasApprovedInvoicesToPrint) {
                    //await _invoiceService.ensureCanPrintApprovedInvoices(); //commented out since it's not technically needed yet; we can uncomment it if status validation becomes more complex in the future and the next method starts throwing nonhandled userfriendly exceptions
                    app.openPopup(abp.appPath + 'app/invoices/PrintApprovedInvoices');
                }
                setTimeout(() => reloadMainGrid(), 3000);
            }
            finally {
                $button.prop('disabled', false);
                abp.ui.clearBusy($button);
            }
        });

        $("#PrintDraftInvoicesButton").click(async function (e) {
            e.preventDefault();
            try {
                abp.ui.setBusy();
                const filter = _dtHelper.getFilterData();
                if (!await _invoiceService.hasDraftInvoices(filter)) {
                    abp.message.warn('There are no invoices to print');
                    return;
                }
                app.openPopup(abp.appPath + 'app/invoices/PrintDraftInvoices?' + toQueryString(filter));
            }
            finally {
                abp.ui.clearBusy();
            }
        });

        function toQueryString(obj) {
            return Object.keys(obj).map(key => encodeURIComponent(key) + '=' + encodeURIComponent(obj[key])).join('&');
        }
    });
})();
