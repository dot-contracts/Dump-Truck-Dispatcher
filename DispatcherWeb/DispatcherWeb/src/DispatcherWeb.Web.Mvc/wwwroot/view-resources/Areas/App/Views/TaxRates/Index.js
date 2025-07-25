(function () {
    $(function () {
        var _taxRateService = abp.services.app.taxRate;
        var _dtHelper = abp.helper.dataTables;
        var _fulcrumAppService = abp.services.app.fulcrum;

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/TaxRates/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/TaxRates/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditTaxRateModal'
        });

        var taxRatesTable = $('#TaxRatesTable');
        var taxRatesGrid = taxRatesTable.DataTableInit({
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                _taxRateService.getTaxRates(abpData).done(function (abpResult) {
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
                    responsivePriority: 1,
                    data: 'name',
                    title: 'Name'
                },
                {
                    data: 'rate',
                    title: 'Rate',
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    width: '10px',
                    responsivePriority: 2,
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

        taxRatesTable.on('click', '.btnDeleteRow', function () {
            var record = _dtHelper.getRowData(this);
            deleteTaxRate(record);
        });

        taxRatesTable.on('click', '.btnEditRow', function () {
            var taxRateId = _dtHelper.getRowData(this).id;
            _createOrEditModal.open({ id: taxRateId });
        });
        var reloadMainGrid = function () {
            taxRatesGrid.ajax.reload();
        };
        abp.event.on('app.createOrEditTaxRateModalSaved', function () {
            reloadMainGrid();
        });

        $("#CreateNewTaxRateButton").click(function (e) {
            e.preventDefault();
            _createOrEditModal.open();
        });

        async function deleteTaxRate(record) {
            if (await abp.message.confirm(
                'Are you sure you want to delete the tax rate?'
            )) {
                _taxRateService.deleteTaxRate({
                    id: record.id
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        }

        $("#SyncWithFulcrumButton").click(function (e) {
            var $button = $(this);
            abp.ui.setBusy($button);

            e.preventDefault();
            _fulcrumAppService
                .scheduleSyncFulcrumTaxRate()
                .always(function () {
                    abp.ui.clearBusy($button);
                });
        });

    })
})();
