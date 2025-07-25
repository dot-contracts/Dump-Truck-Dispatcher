(function () {
    $(function () {

        var _vehicleCategoryService = abp.services.app.vehicleCategory;
        var _dtHelper = abp.helper.dataTables;

        var _createOrEditVehicleCategoryModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/VehicleCategories/CreateOrEditVehicleCategoryModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/VehicleCategories/_CreateOrEditVehicleCategoryModal.js',
            modalClass: 'CreateOrEditVehicleCategoryModal',
            modalSize: 'md'
        });

        $("#AssetTypeFilter").select2Init({
            showAll: true,
            allowClear: true
        });

        $("#IsPoweredFilter").select2Init({
            showAll: true,
            allowClear: true
        });

        var vehicleCategoriesTable = $('#VehicleCategoriesTable');
        var vehicleCategoriesGrid = vehicleCategoriesTable.DataTableInit({
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _vehicleCategoryService.getVehicleCategories(abpData).done(function (abpResult) {
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
                    data: "name",
                    title: "Name"
                },
                {
                    data: "assetType",
                    title: "Asset Type",
                    render: (data, type, full, meta) => _dtHelper.renderText(full.assetTypeName),
                },
                {
                    data: "isPowered",
                    title: "Is Powered",
                    render: (data, type, row) => _dtHelper.renderCheckbox(data)
                },
                {
                    data: "sortOrder",
                    title: "Sort Order"
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    width: "10px",
                    responsivePriority: 2,
                    render: (data, type, full, meta) => '<div class="dropdown">'
                        + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                        + '<ul class="dropdown-menu dropdown-menu-right">'
                        + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                        + ' <li> <a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                        + '</ul>'
                        + '</div>'
                }
            ]
        });

        var reloadMainGrid = function () {
            vehicleCategoriesGrid.ajax.reload();
        };

        abp.event.on('app.CreateOrEditVehicleCategoryModalSaved', function () {
            reloadMainGrid();
        });

        $("#CreateNewVehicleCategoryButton").click(function (e) {
            e.preventDefault();
            _createOrEditVehicleCategoryModal.open();
        });

        vehicleCategoriesTable.on('click', '.btnEditRow', function () {
            var vehicleCategoryId = _dtHelper.getRowData(this).id;
            _createOrEditVehicleCategoryModal.open({ id: vehicleCategoryId });
        });

        vehicleCategoriesTable.on('click', '.btnDeleteRow', async function () {
            var vehicleCategoryId = _dtHelper.getRowData(this).id;
            if (await abp.message.confirm(
                'Are you sure you want to delete this vehicle category?'
            )) {
                _vehicleCategoryService.deleteVehicleCategory({
                    id: vehicleCategoryId
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        });

        $("#SearchButton").click(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $("#VehicleCategoriesFormFilter")[0].reset();
            $(".filter").change();
            reloadMainGrid();
        });

    });

})();
