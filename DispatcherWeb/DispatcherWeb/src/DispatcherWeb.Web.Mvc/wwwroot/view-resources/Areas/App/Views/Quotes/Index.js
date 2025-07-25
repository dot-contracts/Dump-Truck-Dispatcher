(function () {
    $(function () {

        var _quoteService = abp.services.app.quote;
        var _dtHelper = abp.helper.dataTables;
        var _permissions = {
            edit: abp.auth.isGranted('Pages.Quotes.Edit')
        };
        var _isFilterReady = false;
        var _isGridInitialized = false;

        var _viewEmailHistoryModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Emails/ViewEmailHistoryModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Emails/_ViewEmailHistoryModal.js',
            modalClass: 'ViewEmailHistoryModal',
            modalSize: 'lg'
        });

        var _createOrEditQuoteModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Quotes/CreateOrEditQuoteModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Quotes/_CreateOrEditQuoteModal.js',
            modalClass: 'CreateOrEditQuoteModal',
            modalSize: 'xxl',
        });

        let quoteIdToOpen = abp.helper.getQueryStringParameter('id');
        if (quoteIdToOpen) {
            _createOrEditQuoteModal.open({ id: quoteIdToOpen });
        }

        app.localStorage.getItem('QuotesFilter', function (cachedFilter) {
            if (!cachedFilter) {
                cachedFilter = {
                };
            }

            $("#CustomerIdFilter").select2Init({
                abpServiceMethod: abp.services.app.customer.getCustomersSelectList,
                showAll: false,
                allowClear: true
            });
            $("#SalesPersonIdFilter").select2Init({
                abpServiceMethod: abp.services.app.quote.getQuoteSalesrepSelectList,
                showAll: false,
                allowClear: true
            });
            $("#StatusFilter").select2Init({
                showAll: true,
                allowClear: true
            });
            if (cachedFilter.status) {
                abp.helper.ui.addAndSetDropdownValue($("#StatusFilter"), cachedFilter.status);
            }

            if (cachedFilter.salesPersonId) {
                abp.helper.ui.addAndSetDropdownValue($("#SalesPersonIdFilter"), cachedFilter.salesPersonId, cachedFilter.salesPersonName);
            }
            if (cachedFilter.customerId) {
                abp.helper.ui.addAndSetDropdownValue($("#CustomerIdFilter"), cachedFilter.customerId, cachedFilter.customerName);
            }

            $("#QuoteIdFilter").val(cachedFilter.quoteId);
            $("#MiscFilter").val(cachedFilter.misc);

            _isFilterReady = true;
            if (_isGridInitialized) {
                reloadMainGrid(null, false);
            }
        });


        var quotesTable = $('#QuotesTable');

        var quotesGrid = quotesTable.DataTableInit({
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
                app.localStorage.setItem('QuotesFilter', filterData);
                $.extend(abpData, filterData);
                _quoteService.getQuotes(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            order: [[1, 'asc']],
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
                        icon.addClass('clickable').addClass('email-delivery-status');
                        return $("<div>").append(icon).html();
                    },
                    title: ""
                },
                {
                    data: "id",
                    title: "Id"
                },
                {
                    responsivePriority: 1,
                    data: "quoteName",
                    title: "Quote Name"
                },
                {
                    data: "customerName",
                    title: "Customer Name"
                },
                {
                    data: "quoteDate",
                    render: function (data, type, full, meta) { return _dtHelper.renderUtcDate(full.quoteDate); },
                    title: "Date"
                },
                {
                    data: "description",
                    title: "Description"
                },
                {
                    data: "status",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.statusName); },
                    title: "Status",
                },
                {
                    data: "salesPersonName",
                    title: "Sales rep"
                },
                {
                    data: "contactName",
                    title: "Contact Name"
                },
                {
                    data: "poNumber",
                    title: "PO Number"
                },
                {
                    responsivePriority: 2,
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    width: "10px",
                    render: function (data, type, full, meta) {
                        return '<div class="dropdown">'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '<ul class="dropdown-menu dropdown-menu-right">'
                            + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'

                            + (_permissions.edit ? (full.status !== abp.enums.quoteStatus.inactive ?
                                '<li><a class="btnInactiveRow" title="Inactive"><i class="fa fa-minus-circle"></i> Inactivate</a></li>' :
                                '<li><a class="btnreactiveRow" title="Re-active"><i class="fa fa-plus-circle"></i> Re-activate</a></li>') : '')

                            + (_permissions.edit ? '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>' : '')
                            + '</ul>'
                            + '</div>'
                            ;
                    }
                }
            ]
        });

        var reloadMainGrid = function () {
            quotesGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditQuoteModalSaved', function () {
            reloadMainGrid();
        });

        quotesTable.on('click', '.btnEditRow', function () {
            var quoteId = _dtHelper.getRowData(this).id;
            _createOrEditQuoteModal.open({ id: quoteId });
        });

        quotesTable.on('click', '.btnDeleteRow', async function () {
            var quoteId = _dtHelper.getRowData(this).id;
            if (await abp.message.confirm('Are you sure you want to delete the quote?')) {
                _quoteService.deleteQuote({
                    id: quoteId
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        });

        quotesTable.on('click', '.btnInactiveRow', async function () {
            var quoteId = _dtHelper.getRowData(this).id;
            if (await abp.message.confirm('Are you sure you want to inactivate the quote?')) {
                _quoteService.inactivateQuote({
                    id: quoteId
                }).done(function () {
                    abp.notify.info('Successfully inactivated.');
                    reloadMainGrid();
                });
            }
        });

        quotesTable.on('click', '.btnreactiveRow', async function () {
            var quoteId = _dtHelper.getRowData(this).id;
            if (await abp.message.confirm('Are you sure you want to activate the quote?')) {
                _quoteService.activateQuote({
                    id: quoteId
                }).done(function () {
                    abp.notify.info('Successfully activated.');
                    reloadMainGrid();
                });
            }
        });

        quotesTable.on('click', '.email-delivery-status', function () {
            var quoteId = _dtHelper.getRowData(this).id;
            _viewEmailHistoryModal.open({ quoteId: quoteId });
        });

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            reloadMainGrid();
        });

        $("#CreateNewQuoteButton").click(function (e) {
            e.preventDefault();
            _createOrEditQuoteModal.open();
        });

    });
})();
