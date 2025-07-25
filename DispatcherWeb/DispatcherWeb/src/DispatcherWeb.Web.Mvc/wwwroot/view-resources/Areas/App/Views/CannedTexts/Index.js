(function () {
    $(function () {

        var _cannedTextService = abp.services.app.cannedText;
        var _dtHelper = abp.helper.dataTables;
        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/CannedTexts/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/CannedTexts/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditCannedTextModal'
        });

        $("#OfficeIdFilter").select2Init({
            abpServiceMethod: listCacheSelectLists.office(),
            showAll: true,
            allowClear: true
        });

        var cannedTextsTable = $('#CannedTextsTable');
        var cannedTextsGrid = cannedTextsTable.DataTableInit({
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _cannedTextService.getCannedTexts(abpData).done(function (abpResult) {
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
                    data: "name",
                    title: "Name"
                },
                {
                    data: "officeName",
                    title: "Office",
                    visible: abp.features.getValue('App.AllowMultiOfficeFeature') === "true"
                },
                {
                    data: "text",
                    render: function (data, type, full, meta) {
                        var value = full.text;
                        if (value.length > 50) {
                            value = value.substring(0, 50) + "...";
                        }
                        return _dtHelper.renderText(value);
                    },
                    title: "Text"
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    width: "10px",
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

        var reloadMainGrid = function () {
            cannedTextsGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditCannedTextModalSaved', function () {
            reloadMainGrid();
        });

        $("#CreateNewCannedTextButton").click(function (e) {
            e.preventDefault();
            _createOrEditModal.open();
        });

        cannedTextsTable.on('click', '.btnEditRow', function (e) {
            e.preventDefault();
            var record = _dtHelper.getRowData(this);
            _createOrEditModal.open({ id: record.id });
        });

        cannedTextsTable.on('click', '.btnDeleteRow', async function (e) {
            e.preventDefault();
            var recordId = _dtHelper.getRowData(this).id;
            if (await abp.message.confirm(
                'Are you sure you want to delete the canned text?'
            )) {
                _cannedTextService.deleteCannedText({
                    id: recordId
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        });

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            reloadMainGrid();
        });

    });
})();
