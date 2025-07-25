(function () {
    $(function () {

        var _timeClassificationService = abp.services.app.timeClassification;
        var _dtHelper = abp.helper.dataTables;

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/TimeClassifications/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/TimeClassifications/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditTimeClassificationModal'
        });

        var timeClassificationsTable = $('#TimeClassificationsTable');
        var timeClassificationsGrid = timeClassificationsTable.DataTableInit({
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                _timeClassificationService.getTimeClassifications(abpData).done(function (abpResult) {
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
                    title: 'Time Classification'
                },
                {
                    data: 'isProductionBased',
                    title: app.localize('ProductionBased'),
                    render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(data); },
                    visible: abp.setting.getBoolean('App.TimeAndPay.AllowProductionPay'),
                    className: "checkmark"
                },
                {
                    data: 'defaultRate',
                    title: app.localize('DefaultRate'),
                    render: function (data, type, full, meta) { return _dtHelper.renderRate(data); }
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
                            + (full.isProductionBased ? '' : '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>')
                            + '</ul>'
                            + '</div>';
                    }
                }
            ]
        });

        timeClassificationsTable.on('click', '.btnDeleteRow', function () {
            var record = _dtHelper.getRowData(this);
            deleteTimeClassification(record);
        });

        timeClassificationsTable.on('click', '.btnEditRow', function () {
            var timeClassificationId = _dtHelper.getRowData(this).id;
            _createOrEditModal.open({ id: timeClassificationId });
        });

        var reloadMainGrid = function () {
            timeClassificationsGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditTimeClassificationModalSaved', function () {
            reloadMainGrid();
        });

        $("#CreateNewTimeClassificationButton").click(function (e) {
            e.preventDefault();
            _createOrEditModal.open();
        });

        async function deleteTimeClassification(record) {
            if (await abp.message.confirm(
                'Are you sure you want to delete the time classification?'
            )) {
                _timeClassificationService.deleteTimeClassification({
                    id: record.id
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        }

    });
})();
