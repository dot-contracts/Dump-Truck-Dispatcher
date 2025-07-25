(function () {
    $(function () {

        var _haulZoneService = abp.services.app.haulZone;
        var _dtHelper = abp.helper.dataTables;

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/HaulZones/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/HaulZones/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditHaulZoneModal',
        });

        $('#StatusFilter').select2Init({
            showAll: true,
            allowClear: false,
        });

        var haulZonesTable = $('#HaulZonesTable');
        var haulZonesGrid = haulZonesTable.DataTableInit({
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _haulZoneService.getHaulZones(abpData).done(function (abpResult) {
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
                    data: 'name',
                    title: app.localize('Name'),
                },
                {
                    data: 'quantity',
                    title: app.localize('Quantity'),
                },
                {
                    data: 'unitOfMeasureName',
                    title: app.localize('UOM'),
                },
                {
                    data: 'billRatePerTon',
                    title: app.localize('BillRatePerTon'),
                },
                {
                    data: 'minPerLoad',
                    title: app.localize('MinPerLoad'),
                    visible: false,
                },
                {
                    data: 'payRatePerTon',
                    title: app.localize('PayRatePerTon'),
                    visible: false,
                },
                {
                    data: 'isActive',
                    render: (data) => _dtHelper.renderCheckbox(data),
                    className: 'checkmark',
                    title: 'Active',
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
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

        function reloadMainGrid(callback, resetPaging) {
            resetPaging = resetPaging === undefined ? true : resetPaging;
            haulZonesGrid.ajax.reload(callback, resetPaging);
        }

        haulZonesTable.on('click', '.btnEditRow', function () {
            var record = _dtHelper.getRowData(this);
            _createOrEditModal.open({ id: record.id });
        });

        haulZonesTable.on('click', '.btnDeleteRow', function (e) {
            e.preventDefault();
            var record = _dtHelper.getRowData(this);
            deleteHaulZone(record);
        });

        abp.event.on('app.createOrEditHaulZoneModalSaved', function () {
            reloadMainGrid(null, false);
        });

        $('#CreateNewHaulZoneButton').click(function (e) {
            e.preventDefault();
            _createOrEditModal.open();
        });

        async function deleteHaulZone(record) {
            if (!await abp.message.confirm(
                'Are you sure you want to delete the haul zone?'
            )) {
                return;
            }

            await _haulZoneService.deleteHaulZone({
                id: record.id
            });

            abp.notify.info('Successfully deleted.');
            reloadMainGrid();
        }

        $('#SearchButton').closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $('#ClearSearchButton').click(function () {
            $(this).closest('form')[0].reset();
            $('.filter').change();
            reloadMainGrid();
        });
    });
})();
