(function () {
    $(function () {

        var _officeService = abp.services.app.office;
        var _dtHelper = abp.helper.dataTables;

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Offices/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Offices/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditOfficeModal'
        });

        var officeData = [];

        var officesTable = $('#OfficesTable');
        var officesGrid = officesTable.DataTableInit({
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                _officeService.getOffices(abpData).done(function (abpResult) {
                    officeData = _dtHelper.fromAbpResult(abpResult);
                    callback(officeData);
                    initTruckTilesSample();
                });
            },
            drawCallback: function () {
                var truckTags = $('#OfficesTable .truck-tag[data-background-color]');
                truckTags.each(function () {
                    var color = $(this).attr('data-background-color');
                    $(this).css('background-color', color).css('border-color', color);
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
                    data: 'truckColor',
                    title: 'Truck Color',
                    width: '100px',
                    render: function (data, type, full, meta) {
                        return '<span class="tag truck-tag dump-truck" data-background-color="' + _dtHelper.renderText(data) + '">' + data + '</span>';
                    }
                },
                {
                    data: 'truckColor',
                    title: 'Samples',
                    //width: '310px',
                    render: function (data, type, full, meta) {
                        return '<span class="tag truck-tag dump-truck truck-office-' + full.id + '">Dump Truck</span>'
                            + '<span class="tag truck-tag tractor truck-office-' + full.id + '">Tractor</span>'
                            + '<span class="tag truck-tag trailer truck-office-' + full.id + '">Trailer</span>'
                            + '<span class="tag truck-tag leased-dump-truck truck-office-' + full.id + '">Leased Dump Truck</span>';
                    }
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

        officesTable.on('click', '.btnDeleteRow', function () {
            var record = _dtHelper.getRowData(this);
            deleteOffice(record);
        });

        officesTable.on('click', '.btnEditRow', function () {
            var officeId = _dtHelper.getRowData(this).id;
            _createOrEditModal.open({ id: officeId });
        });

        var reloadMainGrid = function () {
            officesGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditOfficeModalSaved', function () {
            reloadMainGrid();
        });

        $("#CreateNewOfficeButton").click(function (e) {
            e.preventDefault();
            _createOrEditModal.open();
        });

        async function deleteOffice(record) {
            if (await abp.message.confirm(
                'Are you sure you want to delete the office?'
            )) {
                _officeService.deleteOffice({
                    id: record.id
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        }

        function initTruckTilesSample() {
            var container = $("#truckTilesSample");
            container.empty();
            var addSampleTile = function (colorClass, officeId) {
                var tile = $('<div class="truck-tile" title="604">604</div>').addClass(colorClass);
                if (officeId) {
                    tile.addClass('truck-office-' + officeId);
                }
                var wrapperDiv = $('<div class="truck-tile-wrap">').addClass(colorClass);
                wrapperDiv.append(tile);
                container.append(wrapperDiv);
            };
            var colors = ['green', 'red', 'yellow', 'gray', 'blue'];
            $.each(colors, function (ind, color) {
                addSampleTile(color);
                addSampleTile(color);
            });
            $.each(officeData.data, function (ind, office) {
                var officeId = office.id;
                $.each(colors, function (ind, color) {
                    addSampleTile(color);
                    addSampleTile(color, officeId);
                });
            });
            $.each(colors, function (ind, color) {
                addSampleTile(color);
                addSampleTile(color);
            });
        }

    });
})();
