(function ($) {
    app.modals.CreateOrEditItemModal = function () {
        var _features = {
            pricingTiers: abp.features.isEnabled('App.PricingTiersFeature'),
            separateItems: abp.features.isEnabled('App.SeparateMaterialAndFreightItems'),
        };
        var _modalManager;
        var _modalArgs;
        var _itemService = abp.services.app.item;
        var _haulingCategoryAppService = abp.services.app.haulingCategory;
        var _productLocationAppService = abp.services.app.productLocation;
        var _dtHelper = abp.helper.dataTables;
        var _$form = null;
        var _itemId = null;
        var _pricingKind = null;
        var _itemPricesGrid = null;
        var _typeDropdown = null;
        var _useZoneBasedRatesCheckbox = null;

        var _createOrEditItemPriceModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Items/CreateOrEditItemPriceModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Items/_CreateOrEditItemPriceModal.js',
            modalClass: 'CreateOrEditItemPriceModal'
        });

        var _createOrEditRateModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Items/CreateOrEditRateModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Items/_CreateOrEditRateModal.js',
            modalClass: 'CreateOrEditRateModal'
        });

        var _createOrEditHaulingZoneRateModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Items/CreateOrEditHaulingZoneRateModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Items/_CreateOrEditHaulingZoneRateModal.js',
            modalClass: 'CreateOrEditHaulingZoneRateModal'
        });

        var saveServiceAsync = async function () {
            if (!_$form.valid()) {
                throw new Error('Form is not valid');
            }

            var item = _$form.serializeFormToObject();

            try {
                abp.ui.setBusy(_$form);
                _modalManager.setBusy(true);

                let editResult = await _itemService.editItem(item);

                abp.notify.info('Saved successfully.');
                _itemId = editResult.id;
                _$form.find("#Id").val(_itemId);
                abp.event.trigger('app.createOrEditItemModalSaved', {
                    item: editResult
                });

                return editResult;
            } finally {
                abp.ui.clearBusy(_$form);
                _modalManager.setBusy(false);
            }
        };

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modalArgs = modalManager.getArgs();

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _itemId = _$form.find("#Id").val();
            _typeDropdown = _$form.find("#Type");
            _pricingKind = getItemPricingKind();

            _useZoneBasedRatesCheckbox = _$form.find("#UseZoneBasedRates");
            _useZoneBasedRatesCheckbox.change(function () {
                toggleRateTableVisibility();
                if (!shouldHideRateTable() && !_itemPricesGrid) {
                    initPriceDataTable();
                }
            });

            _typeDropdown.select2Init({
                allowClear: false,
                showAll: true
            });
            _typeDropdown.change(function () {
                _pricingKind = getItemPricingKind();
                itemTypeControlVisibility();
            });

            toggleRateTableVisibility();

            var pricingTiers = _$form.find('.pricing-tier-name')
                .toArray()
                .map(x => ({
                    id: $(x).data('id'),
                    name: $(x).val(),
                }));

            var itemPricesTable = _modalManager.getModal().find('#ItemPricesTable');

            modalManager.getModal().on('shown.bs.modal', function () {
                _itemPricesGrid
                    ?.columns.adjust()
                    .responsive.recalc();
            });

            var reloadItemPriceGrid = function () {
                _itemPricesGrid?.ajax.reload();
            };
            abp.event.on('app.createOrEditItemPriceModalSaved', function () {
                reloadItemPriceGrid();
            });
            abp.event.on('app.createOrEditHaulingZoneItemPriceModalSaved', function () {
                reloadItemPriceGrid();
            });

            _modalManager.getModal().find("#CreateNewItemPriceButton").click(async function (e) {
                e.preventDefault();
                if (_itemId === '') {
                    await saveServiceAsync();
                }
                switch (_pricingKind) {
                    case abp.enums.itemPricingKind.officeBased:
                        _createOrEditItemPriceModal.open({ itemId: _itemId });
                        break;

                    case abp.enums.itemPricingKind.locationBased:
                        _createOrEditRateModal.open({ itemId: _itemId });
                        break;

                    case abp.enums.itemPricingKind.haulZoneBased:
                        _createOrEditHaulingZoneRateModal.open({ itemId: _itemId });
                        break;

                    default:
                        abp.message.warn('This type doesn\'t support pricing');
                        return;
                }
            });

            async function deleteItemPrice(record) {
                if (!await abp.message.confirm('Are you sure you want to delete the price?')) {
                    return;
                }

                switch (_pricingKind) {
                    case abp.enums.itemPricingKind.officeBased:
                        await _itemService.deleteItemPrice({ id: record.id });
                        break;

                    case abp.enums.itemPricingKind.locationBased:
                        await _productLocationAppService.deleteProductLocation({ id: record.id });
                        break;

                    case abp.enums.itemPricingKind.haulZoneBased:
                        await _haulingCategoryAppService.deleteHaulingCategory({ id: record.id });
                        break;

                    default:
                        abp.message.warn('This type doesn\'t support pricing');
                        return;
                }

                abp.notify.info('Successfully deleted.');
                reloadItemPriceGrid();
            }

            function itemTypeControlVisibility() {
                let isUseZoneBasedRatesVisible = _pricingKind === abp.enums.itemPricingKind.haulZoneBased;
                _$form.find("#UseZoneBasedRatesContainer").toggle(isUseZoneBasedRatesVisible);
                if (!isUseZoneBasedRatesVisible) {
                    _useZoneBasedRatesCheckbox.prop('checked', false);
                }
                resetPriceDataTable();
            }
            function resetPriceDataTable() {
                _itemPricesGrid?.destroy();
                _itemPricesGrid = null;

                toggleRateTableVisibility();

                if (!shouldHideRateTable()) {
                    initPriceDataTable();
                }
            }
            function initPriceDataTable() {
                _itemPricesGrid = itemPricesTable.DataTableInit({
                    paging: false,
                    info: false,
                    serverSide: true,
                    processing: true,
                    language: {
                        emptyTable: app.localize("NoDataInTheTableClick{0}ToAdd", app.localize("AddRate"))
                    },
                    ajax: function (data, callback, settings) {
                        if (_itemId === '') {
                            callback(_dtHelper.getEmptyResult());
                            return;
                        }
                        var abpData = _dtHelper.toAbpData(data);
                        $.extend(abpData, { itemId: _itemId });
                        let method = getPricingListMethod();
                        method(abpData).done(function (abpResult) {
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
                        ...(_pricingKind === abp.enums.itemPricingKind.officeBased ? [
                            {
                                data: "materialUomName",
                                title: "Material<br>UOM",
                            },
                            {
                                data: "pricePerUnit",
                                title: "Price<br>Per Unit",
                            },
                            {
                                data: "freightUomName",
                                title: "Freight<br>UOM",
                            },
                            {
                                data: "freightRate",
                                title: "Freight<br>Rate",
                            },
                            {
                                data: "designationName",
                                title: "Designation",
                                orderable: false,
                            },
                        ] : []),
                        ...(_pricingKind === abp.enums.itemPricingKind.locationBased ? [
                            {
                                data: "locationName",
                                title: "Location",
                            },
                            {
                                data: "uom",
                                title: "UOM",
                            },
                            {
                                data: "cost",
                                render: function (data, type, full, meta) { return _dtHelper.renderMoneyUnrounded(data); },
                                title: "Cost",
                            },
                        ] : []),
                        ...(_pricingKind === abp.enums.itemPricingKind.haulZoneBased ? [
                            {
                                data: "truckCategoryName",
                                title: "Truck Category",
                            },
                            {
                                data: "uom",
                                title: "UOM",
                            },
                            {
                                data: "minimumBillableUnits",
                                title: "Min",
                            }
                        ] : []),
                        ...(_pricingKind === abp.enums.itemPricingKind.locationBased
                            || _pricingKind === abp.enums.itemPricingKind.haulZoneBased
                            ? pricingTiers.map(tier => ({
                                data: null,
                                title: _dtHelper.renderText(tier.name),
                                render: function (data) {
                                    let pricingData = _pricingKind === abp.enums.itemPricingKind.locationBased
                                        ? data.productLocationPrices
                                        : data.haulingCategoryPrices;
                                    return _dtHelper.renderMoneyUnrounded(pricingData?.find(x => x.pricingTierId === tier.id)?.pricePerUnit);
                                },
                                orderable: false,
                            }))
                            : []
                        ),
                        {
                            data: "leaseHaulerRate",
                            render: function (data, type, full, meta) { return _dtHelper.renderMoneyUnrounded(data); },
                            title: "LH Rate",
                            visible: _pricingKind === abp.enums.itemPricingKind.haulZoneBased,
                        },
                        {
                            data: null,
                            orderable: false,
                            autoWidth: false,
                            defaultContent: '',
                            responsivePriority: 1,
                            width: '10px',
                            rowAction: {
                                items: [{
                                    text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                                    className: "btn btn-sm btn-default",
                                    action: function (data) {
                                        switch (_pricingKind) {
                                            case abp.enums.itemPricingKind.officeBased:
                                                return _createOrEditItemPriceModal.open({ id: data.record.id });

                                            case abp.enums.itemPricingKind.locationBased:
                                                return _createOrEditRateModal.open({ id: data.record.id });

                                            case abp.enums.itemPricingKind.haulZoneBased:
                                                return _createOrEditHaulingZoneRateModal.open({ id: data.record.id });
                                        }
                                    }
                                }, {
                                    text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                                    className: "btn btn-sm btn-default",
                                    action: function (data) {
                                        deleteItemPrice(data.record);
                                    }
                                }]
                            }
                        }
                    ]
                });
            }

            _typeDropdown.trigger('change');
        };

        function toggleRateTableVisibility() {
            _modalManager.getModal().find('#RateContainer').toggle(!shouldHideRateTable());
        }

        function shouldHideRateTable() {
            if (_modalArgs.hideRate
                || _pricingKind === abp.enums.itemPricingKind.none
                || _pricingKind === abp.enums.itemPricingKind.haulZoneBased && _useZoneBasedRatesCheckbox.is(':checked')
            ) {
                return true;
            }
        }

        function isMaterialType() {
            var type = Number(_typeDropdown.val());
            return abp.enums.itemTypes.material.includes(type);
        }

        function isFreightType() {
            var type = Number(_typeDropdown.val());
            return abp.enums.itemTypes.freight.includes(type);
        }

        window.getItemPricingKind = getItemPricingKind;
        function getItemPricingKind() {
            if (!_features.pricingTiers) {
                return abp.enums.itemPricingKind.officeBased;
            }
            if (!_features.separateItems) {
                return abp.enums.itemPricingKind.locationBased;
            }
            if (isMaterialType()) {
                return abp.enums.itemPricingKind.locationBased;
            }
            if (isFreightType()) {
                return abp.enums.itemPricingKind.haulZoneBased;
            }
            return abp.enums.itemPricingKind.none;
        }

        function getPricingListMethod() {
            switch (_pricingKind) {
                case abp.enums.itemPricingKind.officeBased:
                    return _itemService.getItemPrices;

                case abp.enums.itemPricingKind.locationBased:
                    return _productLocationAppService.getRates;

                case abp.enums.itemPricingKind.haulZoneBased:
                    return _haulingCategoryAppService.getRates;

                default:
                    return Promise.resolve(_dtHelper.getEmptyResult());
            }
        }

        this.save = async function () {
            let editResult = await saveServiceAsync();
            _modalManager.setResult(editResult);
            _modalManager.close();
        };
    };
})(jQuery);
