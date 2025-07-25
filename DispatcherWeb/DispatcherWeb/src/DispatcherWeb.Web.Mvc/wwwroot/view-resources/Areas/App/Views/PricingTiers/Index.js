(function () {
    $(function () {
        const _pricingTierService = abp.services.app.pricingTier;
        const _dtHelper = abp.helper.dataTables;
        const _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/PricingTiers/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/PricingTiers/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditPricingTierModal'
        });

        const pricingTiersTable = $('#PricingTiersTable');
        const addNewButton = $('#CreateNewPricingTierButton');
        const pricingTiersGrid = pricingTiersTable.DataTableInit({
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                const abpData = _dtHelper.toAbpData(data);
                _pricingTierService.getPricingTiers(abpData).done(function (abpResult) {
                    var result = _dtHelper.fromAbpResult(abpResult);
                    callback(result);
                    updateAddButtonState(result.recordsTotal);
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
                }, {
                    responsivePriority: 1,
                    data: 'name',
                    title: 'Name'
                }, {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    width: '10px',
                    responsivePriority: 1,
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

        pricingTiersTable.on('click', '.btnDeleteRow', function () {
            const record = _dtHelper.getRowData(this);
            deletePricingTier(record);
        });

        pricingTiersTable.on('click', '.btnEditRow', function () {
            const pricingTierId = _dtHelper.getRowData(this).id;
            _createOrEditModal.open({ id: pricingTierId });
        });

        addNewButton.click(function (e) {
            e.preventDefault();
            _createOrEditModal.open();
        });

        const reloadMainGrid = function () {
            pricingTiersGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditPricingTierModalSaved', function () {
            reloadMainGrid();
        });

        function updateAddButtonState(count) {
            if (count >= 5) {
                addNewButton.attr('disabled', true);
                addNewButton.attr('title', app.localize("MaximumNumberOfPricingTiersError"));
            } else {
                addNewButton.removeAttr('disabled');
                addNewButton.removeAttr('title');
            }
        }

        async function deletePricingTier(record) {
            if (await abp.message.confirm(
                'Are you sure you want to delete the pricing tier?'
            )) {
                _pricingTierService.deletePricingTier({
                    id: record.id
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        }
    });
})();
