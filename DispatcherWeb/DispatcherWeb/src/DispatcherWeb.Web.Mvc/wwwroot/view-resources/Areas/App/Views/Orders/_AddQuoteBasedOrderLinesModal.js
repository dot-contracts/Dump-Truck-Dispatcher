(function ($) {
    app.modals.AddQuoteBasedOrderLinesModal = function () {

        var _modalManager;
        var _orderAppService = abp.services.app.order;
        let _quoteService = abp.services.app.quote;
        var _dtHelper = abp.helper.dataTables;
        var _filter = null;
        var _grid = null;
        var _gridOptions = null;
        var _modalOptions = null;
        var _loadAtDropdown = null;
        var _deliverToDropdown = null;
        var _itemDropdown = null;
        let _features = {
            separateItems: abp.features.isEnabled('App.SeparateMaterialAndFreightItems')
        };
        const _createOrEditQuoteLineModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Quotes/CreateOrEditQuoteLineModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Quotes/_CreateOrEditQuoteLineModal.js',
            modalClass: 'CreateOrEditQuoteLineModal'
        });

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modalOptions = _modalManager.getArgs();

            _$form = _modalManager.getModal().find('form');

            _loadAtDropdown = _$form.find('#QuoteBasedLoadAtIdFilter');
            _deliverToDropdown = _$form.find('#QuoteBasedDeliverToIdFilter');
            _itemDropdown = _$form.find('#QuoteBasedItemIdFilter');

            _loadAtDropdown.select2Init({
                abpServiceMethod: abp.services.app.location.getLocationsSelectList, //todo, listCacheSelectLists.location() is not supported yet
                abpServiceParamsGetter: (params) => ({
                    loadAtQuoteId: _modalOptions.quoteId,
                    includeInactive: true,
                }),
                showAll: false,
                allowClear: true
            });
            _loadAtDropdown.on("change", function (e) {
                reloadGrid();
            });

            _deliverToDropdown.select2Init({
                abpServiceMethod: abp.services.app.location.getLocationsSelectList, //todo, listCacheSelectLists.location() is not supported yet
                abpServiceParamsGetter: (params) => ({
                    deliverToQuoteId: _modalOptions.quoteId,
                    includeInactive: true,
                }),
                showAll: false,
                allowClear: true
            });
            _deliverToDropdown.on("change", function (e) {
                reloadGrid();
            });

            _itemDropdown.select2Init({
                abpServiceMethod: abp.services.app.item.getItemsSelectList, //listCacheSelectLists.item() //todo we should see if we can switch to using ids
                //the grid doesn't have paging so we might be able to get all lines and get the items from there
                abpServiceParamsGetter: (params) => ({
                    includeInactive: true,
                    quoteId: _modalOptions.quoteId
                }),
                showAll: false,
                allowClear: true
            });
            _itemDropdown.on("change", function (e) {
                reloadGrid();
            });

            let saveButton = _modalManager.getModal().find('.save-button');
            if (_modalOptions.saveButtonText) {
                saveButton.find('span').text(_modalOptions.saveButtonText);
                saveButton.find('i').remove();
            } else {
                saveButton.find('span').text('Add selected items');
                saveButton.find('i').removeClass('fa-save').addClass('fa-plus');
            }

            var table = _modalManager.getModal().find('#QuoteItemsTable');
            _gridOptions = {
                paging: false,
                serverSide: true,
                processing: true,
                info: false,
                selectionColumnOptions: {},
                ajax: async function (data, callback, settings) {
                    if (!_filter) {
                        callback(abp.helper.dataTables.getEmptyResult());
                        return;
                    }
                    _filter.loadAtId = _loadAtDropdown.val();
                    _filter.deliverToId = _deliverToDropdown.val();
                    _filter.itemId = _itemDropdown.val();
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, _filter);
                    _orderAppService.getOrderLines(abpData).done(function (abpResult) {
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
                        }
                    },
                    {
                        data: "loadAtName",
                        title: "Load At"
                    },
                    {
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
                        data: "vehicleCategories",
                        title: "Truck/Trailer category",
                        orderable: false,
                        render: function (data, type, full, meta) {
                            return data.map(function(item) {
                                return item.name;
                            }).join(', ');
                        }
                    },
                    {
                        data: "bedConstruction",
                        title: "Bed Construction",
                        render: function (data, type, full, meta) { return _dtHelper.renderText(full.bedConstructionName); }
                    },
                ],
                drawCallback: function (settings) {
                    var selectElements = _modalManager.getModal().find('select.select2-hidden-accessible');
                    selectElements.each(function (index, element) {
                        $(element).select2('close');
                    });
                }
            };
            _grid = table.DataTableInit(_gridOptions);
            
            const reloadQuoteLinesGrid = () => {
                _grid.ajax.reload();
            };

            _$form.find("#CreateNewQuoteLineButton").click(function(e) {
                e.preventDefault();
                _createOrEditQuoteLineModal.open({ quoteId: _modalOptions.quoteId });
            });

            _modalManager.on('app.createOrEditQuoteLineModalSaved', function() {
                reloadQuoteLinesGrid();
            });

            _modalManager.getModal().on('shown.bs.modal', function () {
                _grid
                    .columns.adjust()
                    .responsive.recalc();
            });
        };

        function reloadGrid() {
            if (_grid) {
                _grid.ajax.reload();
            }
        }

        this.setFilter = function (filter) {
            _filter = filter;
            reloadGrid();
        };

        this.save = async function () {
            var selectedRows = _gridOptions.selectionColumnOptions.getSelectedRows();
            if (_modalOptions.limitSelectionToSingleOrderLine && selectedRows.length !== 1) {
                abp.message.error("Please select 1 desired line item");
                return;
            }
            abp.event.trigger('app.quoteBasedOrderLinesSelectedModal', {
                selectedLines: selectedRows
            });

            _modalManager.close();
        }

    };
})(jQuery);
