(function () {
    $(function () {
        var _itemService = abp.services.app.item;
        var _dtHelper = abp.helper.dataTables;
        var _fulcrumAppService = abp.services.app.fulcrum;

        var _permissions = {
            merge: abp.auth.hasPermission('Pages.Items.Merge')
        };
        var _createOrEditItemModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Items/CreateOrEditItemModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Items/_CreateOrEditItemModal.js',
            modalClass: 'CreateOrEditItemModal',
            modalSize: 'xl'
        });

        var statusFilter = $('#StatusFilter');
        statusFilter.select2Init({
            showAll: true,
            allowClear: false
        });

        var typeFilter = $('#TypeFilter');
        typeFilter.select2Init({
            showAll: true,
        });

        var itemsTable = $('#ItemsTable');
        var itemsGrid = itemsTable.DataTableInit({
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _itemService.getItems(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            dataMergeOptions: {
                enabled: _permissions.merge,
                description: "The selected products or services are about to be merged into one entry. Select the entry that you would like them to be merged into. The other entries will be deleted. There is no undoing this process. If you don't want this to happen, press cancel.",
                entitiesName: 'entries',
                dropdownServiceMethod: _itemService.getItemsByIdsSelectList,
                mergeServiceMethod: _itemService.mergeItems,
                newSelectionValidation: function (newItem, selectedRows) {
                    if (selectedRows.length > 0) {
                        let initialSelected = selectedRows[0];
                        var initialSelectedCategory = abp.helper.getItemTypeCategory(initialSelected.type);

                        var newItemCategory = abp.helper.getItemTypeCategory(newItem.type);

                        if (initialSelectedCategory !== newItemCategory) {
                            let initialCategoryDisplayName = abp.helper.getItemTypeCategoryDisplayName(initialSelectedCategory);
                            let newItemCategoryDisplayName = abp.helper.getItemTypeCategoryDisplayName(newItemCategory);

                            abp.notify.warn(`You cannot merge items of ${initialCategoryDisplayName} and ${newItemCategoryDisplayName} types. Please check your selection.`);
                            return false;
                        }
                    }
                    return true;
                }
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
                    data: "name",
                    title: "Name",
                    responsivePriority: 1
                },
                {
                    data: "description",
                    title: "Description"
                },
                {
                    data: "type",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.typeName); },
                    title: "Type"
                },
                {
                    data: "isActive",
                    render: function (data, type, full, meta) {
                        return _dtHelper.renderCheckbox(full.isActive);
                    },
                    className: "checkmark",
                    width: "100px",
                    title: "Active"
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
                            + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                            + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                            + '</ul>'
                            + '</div>';
                    }
                }
            ]
        });

        var reloadMainGrid = function () {
            itemsGrid.ajax.reload();
        };

        itemsTable.on('click', '.btnEditRow', function () {
            var record = _dtHelper.getRowData(this);
            _createOrEditItemModal.open({ id: record.id });
        });

        itemsTable.on('click', '.btnDeleteRow', function (e) {
            e.preventDefault();
            var record = _dtHelper.getRowData(this);
            deleteItem(record);
        });

        abp.event.on('app.createOrEditItemModalSaved', function () {
            reloadMainGrid();
        });

        $("#CreateNewItemButton").click(function (e) {
            e.preventDefault();
            _createOrEditItemModal.open();
        });

        async function deleteItem(record) {
            if (await abp.message.confirm('Are you sure you want to delete the item?')) {
                _itemService.deleteItem({
                    id: record.id
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        }

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            reloadMainGrid();
        });

        $('#ExportItemsToCsvButton').click(function () {
            var $button = $(this);
            var abpData = {};
            $.extend(abpData, _dtHelper.getFilterData());
            abp.ui.setBusy($button);
            _itemService
                .getItemsToCsv(abpData)
                .done(function (result) {
                    app.downloadTempFile(result);
                }).always(function () {
                    abp.ui.clearBusy($button);
                });
        });

        $("#SyncWithFulcrumButton").click(function (e) {
            var $button = $(this);
            abp.ui.setBusy($button);

            e.preventDefault();
            _fulcrumAppService
                .scheduleSyncFulcrumProducts()
                .always(function (result) {
                    abp.ui.clearBusy($button);
                });
        });

    });
})();
