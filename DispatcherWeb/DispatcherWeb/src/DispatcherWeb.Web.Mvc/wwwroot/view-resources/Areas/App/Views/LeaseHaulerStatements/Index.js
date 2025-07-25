(function () {
    $(function () {

        var _leaseHaulerStatementService = abp.services.app.leaseHaulerStatement;
        var _dtHelper = abp.helper.dataTables;

        var _addLeaseHaulerStatementModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/LeaseHaulerStatements/AddLeaseHaulerStatementModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/LeaseHaulerStatements/_AddLeaseHaulerStatementModal.js',
            modalClass: 'AddLeaseHaulerStatementModal'
        });

        var _specifyExportOptionsModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/LeaseHaulerStatements/SpecifyExportOptionsModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/LeaseHaulerStatements/_SpecifyExportOptionsModal.js',
            modalClass: 'SpecifyExportOptionsModal',
            modalSize: 'sm'
        });

        $("#DateRangeFilter").daterangepicker({
            locale: {
                cancelLabel: 'Clear'
            },
            showDropDown: true,
            autoUpdateInput: false
        }).on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
        }).on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
        });

        var leaseHaulerStatementsTable = $('#LeaseHaulerStatementsTable');
        var leaseHaulerStatementsGrid = leaseHaulerStatementsTable.DataTableInit({
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                $.extend(abpData, _dtHelper.getDateRangeObject(abpData.dateRangeFilter, 'statementDateBegin', 'statementDateEnd'));
                delete abpData.dateRangeFilter;
                _leaseHaulerStatementService.getLeaseHaulerStatements(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            drawCallback: function (settings) {
                $('table [data-toggle="tooltip"]').tooltip();
            },
            order: [[1, 'desc']],
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
                    data: "id",
                    title: "Statement Id"
                },
                {
                    data: "statementDate",
                    title: "Statement Date",
                    render: function (data, type, full, meta) {
                        return _dtHelper.renderUtcDate(data);
                    },
                },
                {
                    data: "startDate",
                    title: "Start Date",
                    render: function (data, type, full, meta) {
                        return _dtHelper.renderUtcDate(data);
                    },
                },
                {
                    data: "endDate",
                    title: "End Date",
                    render: function (data, type, full, meta) {
                        return _dtHelper.renderUtcDate(data);
                    },
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    width: "10px",
                    responsivePriority: 2,
                    render: function (data, type, full, meta) {
                        return '<div class="dropdown">'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '<ul class="dropdown-menu dropdown-menu-right">'
                            + '<li><a class="btnPrintRow"><i class="fa fa-edit"></i> Export Statement</a></li>'
                            + '<li><a class="btnDeleteRow"><i class="fa fa-trash"></i> Delete</a></li>'
                            + '</ul>'
                            + '</div>';
                    }
                }
            ]
        });

        var reloadMainGrid = function () {
            leaseHaulerStatementsGrid.ajax.reload();
        };


        leaseHaulerStatementsTable.on('click', '.btnPrintRow', async function () {
            var record = _dtHelper.getRowData(this);
            //_printLeaseHaulerStatementModal.open();

            var exportOptions = await _specifyExportOptionsModal.open().then((modal, modalObject) => {
                return modalObject.getResultPromise();
            });

            _leaseHaulerStatementService
                .exportLeaseHaulerStatementById({
                    id: record.id,
                    ...exportOptions
                })
                .done(function (result) {
                    app.downloadTempFile(result);
                });
        });

        leaseHaulerStatementsTable.on('click', '.btnDeleteRow', async function (e) {
            e.preventDefault();
            var record = _dtHelper.getRowData(this);
            if (await abp.message.confirm(
                'Are you sure you want to delete the LH statement?'
            )) {
                _leaseHaulerStatementService.deleteLeaseHaulerStatement({
                    id: record.id
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        });

        abp.event.on('app.addLeaseHaulerStatementModalSaved', function () {
            reloadMainGrid();
        });

        $("#AddLeaseHaulerStatementButton").click(function (e) {
            e.preventDefault();
            _addLeaseHaulerStatementModal.open();
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


