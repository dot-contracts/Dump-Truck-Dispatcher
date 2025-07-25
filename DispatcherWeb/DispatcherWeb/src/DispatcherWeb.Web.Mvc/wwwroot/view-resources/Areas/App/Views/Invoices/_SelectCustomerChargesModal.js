(function ($) {
    app.modals.SelectCustomerChargesModal = function () {

        var _modalManager;
        var _invoiceService = abp.services.app.invoice;
        var _dtHelper = abp.helper.dataTables;
        var _features = {
            charges: abp.features.isEnabled('App.Charges'),
        };
        var _parentFilter = null;
        var _localFilter = {};
        var _customerInvoicingMethod = null;
        var _selectedJobNumbers = null;
        var _customerChargesGrid = null;
        var _selectionColumn = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            let saveButton = _modalManager.getModal().find('.save-button');
            saveButton.find('span').text('Add selected charges');
            saveButton.find('i').removeClass('fa-save').addClass('fa-plus');

            var customerChargesTable = _modalManager.getModal().find('#CustomerChargesTable');
            _customerChargesGrid = customerChargesTable.DataTableInit({
                paging: false,
                serverSide: true,
                processing: true,
                info: false,
                selectionColumnOptions: _selectionColumn = {
                    preserveSelection: true,
                    isHeaderCheckboxVisible: () => {
                        if (!_parentFilter || !_features.charges) {
                            return true;
                        }
                        let filter = getFilter();
                        let gridData = _customerChargesGrid.rows().data().toArray();
                        let isJobNumberUnique = isValueDistinct(x => x.jobNumber, gridData, filter.jobNumbers);
                        let isTaxRateUnique = isValueDistinct(x => x.salesTaxRate, gridData, filter.salesTaxRates);
                        let isTaxEntityUnique = isValueDistinct(x => x.salesTaxEntityId, gridData, filter.salesTaxEntityIds);
                        return gridData.length > 0
                            && (
                                _customerInvoicingMethod !== abp.enums.invoicingMethod.separateTicketsByJobNumber
                                || isJobNumberUnique
                            )
                            && isTaxRateUnique
                            && isTaxEntityUnique;
                    },
                    selectionChangedCallbacks: [
                        async () => {
                            if (!_features.charges) {
                                return;
                            }
                            let selectedRows = _selectionColumn.getSelectedRows();
                            let reloadGrid = false;

                            if (_customerInvoicingMethod === abp.enums.invoicingMethod.separateTicketsByJobNumber) {
                                if (!_localFilter.jobNumbers?.length
                                    && selectedRows.length
                                ) {
                                    _localFilter.jobNumbers = [selectedRows[0].jobNumber];
                                    reloadGrid = true;
                                } else if (_localFilter.jobNumbers?.length
                                    && !selectedRows.length
                                ) {
                                    _localFilter.jobNumbers = null;
                                    reloadGrid = true;
                                }
                            }

                            if (!_localFilter.salesTaxRates?.length
                                && selectedRows.length
                            ) {
                                _localFilter.salesTaxRates = [selectedRows[0].salesTaxRate];
                                reloadGrid = true;
                            } else if (_localFilter.salesTaxRates?.length
                                && !selectedRows.length
                            ) {
                                _localFilter.salesTaxRates = null;
                                reloadGrid = true;
                            }

                            if (!_localFilter.salesTaxEntityIds?.length
                                && selectedRows.length
                            ) {
                                _localFilter.salesTaxEntityIds = [selectedRows[0].salesTaxEntityId];
                                reloadGrid = true;
                            } else if (_localFilter.salesTaxEntityIds?.length
                                && !selectedRows.length
                            ) {
                                _localFilter.salesTaxEntityIds = null;
                                reloadGrid = true;
                            }

                            if (reloadGrid) {
                                await reloadCustomerChargesGrid();
                            }
                        },
                    ],
                },
                ajax: async function (data, callback, settings) {
                    if (!_parentFilter) {
                        callback(abp.helper.dataTables.getEmptyResult());
                        return;
                    }
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, getFilter());
                    _invoiceService.getCustomerCharges(abpData).done(function (abpResult) {
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
                        data: "chargeDate",
                        render: function (data, type, full, meta) {
                            return (_dtHelper.renderUtcDate(full.chargeDate) || '');
                        },
                        title: "Date",
                    },
                    {
                        data: "jobNumber",
                        title: "Job Nbr",
                    },
                    {
                        data: "itemName",
                        title: "Item",
                        width: "50px",
                    },
                    {
                        data: "unitOfMeasureName",
                        title: "UOM",
                    },
                    {
                        data: "rate",
                        title: "Rate",
                    },
                    {
                        data: "quantity",
                        title: "Qty",
                    },
                    {
                        data: "chargeAmount",
                        title: "Charge",
                    },
                    {
                        data: "description",
                        title: "Description",
                    }
                ]
            });

            _modalManager.getModal().on('shown.bs.modal', function () {
                _customerChargesGrid
                    .columns.adjust()
                    .responsive.recalc();
            });
        };

        function reloadCustomerChargesGrid() {
            return new Promise(resolve => {
                _customerChargesGrid?.ajax.reload(() => resolve());
            });
        }

        function getFilter() {
            const result = {
                ..._parentFilter,
                jobNumbers: combineAndDistinctOptionalArrays(
                    _parentFilter.jobNumbers,
                    _localFilter.jobNumbers
                ),
                salesTaxRates: combineAndDistinctOptionalArrays(
                    _parentFilter.salesTaxRates,
                    _localFilter.salesTaxRates
                ),
                salesTaxEntityIds: combineAndDistinctOptionalArrays(
                    _parentFilter.salesTaxEntityIds,
                    _localFilter.salesTaxEntityIds
                ),
            };
            return result;
        }

        function combineAndDistinctOptionalArrays(a, b) {
            return [...(new Set([
                ...(a || []),
                ...(b || []),
            ]))];
        }

        function isValueDistinct(selector, gridArray, filterArray) {
            if (!gridArray.length) {
                return true;
            }
            const firstRow = gridArray[0];
            return gridArray.every(row => selector(firstRow) === selector(row))
                && (!filterArray?.length || filterArray.includes(selector(firstRow)));
        }

        this.setFilter = function (filter, customerInvoicingMethod, selectedJobNumbers) {
            _parentFilter = filter;
            _customerInvoicingMethod = customerInvoicingMethod;
            _selectedJobNumbers = selectedJobNumbers;
            reloadCustomerChargesGrid();
        };

        this.save = async function () {
            var selectedRows = _selectionColumn.getSelectedRows();

            if (!_features.charges
                && selectedRows?.length
                && _customerInvoicingMethod === abp.enums.invoicingMethod.separateTicketsByJobNumber
            ) {
                let newJobNumbers = [];
                selectedRows.map(r => r.jobNumber).filter(j => j).forEach(j => {
                    if (!newJobNumbers.includes(j)) {
                        newJobNumbers.push(j);
                    }
                });
                let showJobNumberWarning = newJobNumbers.length > 1;
                if (!showJobNumberWarning && _selectedJobNumbers && _selectedJobNumbers.length) {
                    if (newJobNumbers.filter(n => !_selectedJobNumbers.includes(n)).length) {
                        showJobNumberWarning = true;
                    }
                }
                if (showJobNumberWarning) {
                    if (!await abp.message.confirm('This customer wants separate tickets per job number and you have selected some charges with different job numbers. Are you sure you want to save this invoice with multiple job numbers?')) {
                        return;
                    }
                }
            }

            abp.event.trigger('app.customerChargesSelectedModal', {
                selectedCharges: selectedRows
            });
            _modalManager.close();
        };
    };
})(jQuery);
