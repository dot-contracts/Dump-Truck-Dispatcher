(function () {
    $(function () {

        var _locationService = abp.services.app.location;
        var _dtHelper = abp.helper.dataTables;
        var _permissions = {
            merge: abp.auth.hasPermission('Pages.Locations.Merge')
        };

        var _createOrEditLocationModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Locations/CreateOrEditLocationModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Locations/_CreateOrEditLocationModal.js',
            modalClass: 'CreateOrEditLocationModal',
            modalSize: 'lg'
        });

        $("#CategoryIdFilter").select2Init({
            abpServiceMethod: abp.services.app.location.getLocationCategorySelectList,
            showAll: true,
            allowClear: true
        });

        $("#StatusFilter").select2Init({
            allowClear: false,
            showAll: true
        });

        var locationsTable = $('#LocationsTable');
        var locationsGrid = locationsTable.DataTableInit({
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _locationService.getLocations(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            dataMergeOptions: {
                enabled: _permissions.merge,
                description: "The selected locations are about to be merged into one entry. Select the location that you would like them to be merged into. The other locations will be deleted. There is no undoing this process. If you don't want this to happen, press cancel.",
                entitiesName: 'locations',
                dropdownServiceMethod: _locationService.getLocationsByIdsSelectList,
                mergeServiceMethod: _locationService.mergeLocations
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
                    data: "categoryName",
                    title: "Category"
                },
                {
                    data: "streetAddress",
                    title: "Street Address"
                },
                {
                    data: "city",
                    title: "City"
                },
                {
                    data: "state",
                    title: "State"
                },
                {
                    data: "zipCode",
                    title: "Zip Code"
                },
                {
                    data: "countryCode",
                    title: "Country Code"
                },
                {
                    data: "isActive",
                    render: function (isActive) { return _dtHelper.renderCheckbox(isActive); },
                    className: "checkmark",
                    title: "Active"
                },
                {
                    data: "abbreviation",
                    title: "Abbreviation"
                },
                {
                    data: "notes",
                    title: "Notes"
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    width: "10px",
                    responsivePriority: 2,
                    render: function (data, type, full, meta) {
                        if (full.predefinedLocationKind === null) {
                            return '<div class="dropdown">'
                                + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                                + '<ul class="dropdown-menu dropdown-menu-right">'
                                + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                                + ' <li> <a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                                + '</ul>'
                                + '</div>'
                                ;
                        }
                    }
                }
            ]
        });



        var reloadMainGrid = function () {
            locationsGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditLocationModalSaved', function () {
            reloadMainGrid();
        });

        $("#CreateNewLocationButton").click(function (e) {
            e.preventDefault();
            _createOrEditLocationModal.open();
        });

        locationsTable.on('click', '.btnEditRow', function () {
            var locationId = _dtHelper.getRowData(this).id;
            _createOrEditLocationModal.open({ id: locationId });
        });


        locationsTable.on('click', '.btnDeleteRow', async function () {
            var locationId = _dtHelper.getRowData(this).id;
            if (await abp.message.confirm(
                'Are you sure you want to delete the location?'
            )) {
                _locationService.deleteLocation({
                    id: locationId
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        });


        $('#ShowAdvancedFiltersSpan').click(function () {
            $('#ShowAdvancedFiltersSpan').hide();
            $('#HideAdvancedFiltersSpan').show();
            $('#AdvacedAuditFiltersArea').slideDown();
        });

        $('#HideAdvancedFiltersSpan').click(function () {
            $('#HideAdvancedFiltersSpan').hide();
            $('#ShowAdvancedFiltersSpan').show();
            $('#AdvacedAuditFiltersArea').slideUp();
        });

        $("#SearchButton").click(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $("#LocationsFormFilter")[0].reset();
            $(".filter").change();
            reloadMainGrid();
        });

        $('#ExportLocationsToCsvButton').click(function () {
            var $button = $(this);
            var abpData = {};
            $.extend(abpData, _dtHelper.getFilterData());
            abp.ui.setBusy($button);
            _locationService
                .getLocationsToCsv(abpData)
                .done(function (result) {
                    app.downloadTempFile(result);
                }).always(function () {
                    abp.ui.clearBusy($button);
                });
        });


    });



})();
