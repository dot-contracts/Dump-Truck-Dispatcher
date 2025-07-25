(function () {
    $(function () {

        var _scheduledReportService = abp.services.app.scheduledReport;
        var _dtHelper = abp.helper.dataTables;

        var _createOrEditScheduledReportModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/ScheduledReports/CreateOrEditScheduledReportModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/ScheduledReports/_CreateOrEditScheduledReportModal.js',
            modalClass: 'CreateOrEditScheduledReportModal'
        });
        var scheduledReportsTable = $('#ScheduledReportsTable');
        var scheduledReportsGrid = scheduledReportsTable.DataTableInit({
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                _scheduledReportService.getScheduledReportPagedList(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            order: [[0, 'asc']],
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
                    data: "reportName",
                    title: "Report Name"
                },
                {
                    orderable: false,
                    data: "sendTo",
                    title: "Send To"
                },
                {
                    data: "reportFormat",
                    title: "Format"
                },
                {
                    data: "scheduleTime",
                    title: "Schedule Time",
                    render: function (data, type, full, meta) { return _dtHelper.renderTime(full.scheduleTime, ''); }
                },
                {
                    orderable: false,
                    data: "sendOn",
                    title: "Send On"
                },
                {
                    data: null,
                    orderable: false,
                    name: "Actions",
                    width: "10px",
                    responsivePriority: 1,
                    className: "actions",
                    defaultContent: '<div class="dropdown action-button">'
                        + '<ul class="dropdown-menu dropdown-menu-right">'
                        + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                        + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                        + '</ul>'
                        + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                        + '</div>'
                }
            ]
        });

        var reloadMainGrid = function () {
            scheduledReportsGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditScheduledReportModalSaved', function () {
            reloadMainGrid();
        });

        $("#AddScheduledReport").click(function (e) {
            e.preventDefault();
            _createOrEditScheduledReportModal.open();
        });

        scheduledReportsTable.on('click', '.btnEditRow', function () {
            var scheduledReportId = _dtHelper.getRowData(this).id;
            _createOrEditScheduledReportModal.open({ id: scheduledReportId });
        });

        scheduledReportsTable.on('click', '.btnDeleteRow', async function () {
            var scheduledReportId = _dtHelper.getRowData(this).id;
            if (await abp.message.confirm('Are you sure you want to delete the Scheduled Report?')) {
                _scheduledReportService.deleteScheduledReport({
                    id: scheduledReportId
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        });


    });
})();
