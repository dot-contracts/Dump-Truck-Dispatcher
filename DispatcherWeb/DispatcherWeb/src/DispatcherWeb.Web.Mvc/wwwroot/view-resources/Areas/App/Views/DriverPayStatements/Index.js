(function () {
    $(function () {

        var _payStatementService = abp.services.app.payStatement;
        var _dtHelper = abp.helper.dataTables;

        var _addPayStatementModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/DriverPayStatements/AddPayStatementModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/DriverPayStatements/_AddPayStatementModal.js',
            modalClass: 'AddPayStatementModal'
        });

        var _printDriverPayStatementModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/DriverPayStatements/PrintDriverPayStatementModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/DriverPayStatements/_PrintDriverPayStatementModal.js',
            modalClass: 'PrintDriverPayStatementModal'
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

        var driverPayStatementsTable = $('#DriverPayStatementsTable');

        var driverPayStatementsGrid = driverPayStatementsTable.DataTableInit({
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                $.extend(abpData, _dtHelper.getDateRangeObject(abpData.dateRangeFilter, 'statementDateBegin', 'statementDateEnd'));
                delete abpData.dateRangeFilter;
                _payStatementService.getPayStatements(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            drawCallback: function (settings) {
                $('table [data-toggle="tooltip"]').tooltip();
            },
            order: [[2, 'desc']],
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
                    data: null,
                    width: '30px',
                    orderable: false,
                    render: function (data, type, full, meta) {
                        if (!full.driverDateConflicts || !full.driverDateConflicts.length) {
                            return '';
                        }

                        let tooltipHtml = "";

                        let productionPayTimeButNoTickets = full.driverDateConflicts.filter(d => d.conflictKind === abp.enums.driverDateConflictKind.productionPayTimeButNoTickets);
                        let bothProductionAndHourlyPay = full.driverDateConflicts.filter(d => d.conflictKind === abp.enums.driverDateConflictKind.bothProductionAndHourlyPay);

                        if (productionPayTimeButNoTickets.length) {
                            tooltipHtml += productionPayTimeButNoTickets.map(d => `<div class='text-left'>${_dtHelper.renderText(app.localize('{0}HasProductionPayTimeOn{1}ButNoTickets', d.driverName, _dtHelper.renderUtcDate(d.date)))}</div>`).join('');
                        }

                        if (bothProductionAndHourlyPay.length) {
                            tooltipHtml += '<div class=\'text-left\'>' + _dtHelper.renderText(app.localize('ListOfDriverTimeConflicts')) + '</div>';
                            tooltipHtml += bothProductionAndHourlyPay.map(d => `<div class='text-left'>${_dtHelper.renderText(d.driverName)} - ${_dtHelper.renderUtcDate(d.date)}</div>`).join('');
                        }

                        let tooltipTags = `class="btnPrintWarningsForRow" data-toggle="tooltip" data-html="true" data-placement="right" title="${tooltipHtml}"`;
                        return '<span ' + tooltipTags + '><i class="fas fa-exclamation-triangle text-warning"></i></span>';
                    }
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
                    data: "includeProductionPay",
                    render: function (data, type, full, meta) {
                        return _dtHelper.renderCheckbox(data);
                    },
                    className: "checkmark",
                    width: "100px",
                    title: "Include Production Pay"
                },
                {
                    data: "includeHourly",
                    render: function (data, type, full, meta) {
                        return _dtHelper.renderCheckbox(data);
                    },
                    className: "checkmark",
                    width: "100px",
                    title: "Include Hourly"
                },
                //{
                //    data: "includeSalary",
                //    render: function (data, type, full, meta) {
                //        return _dtHelper.renderCheckbox(data);
                //    },
                //    className: "checkmark",
                //    width: "100px",
                //    title: "Include Salary"
                //},
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    width: "10px",
                    responsivePriority: 2,
                    render: function (data, type, full, meta) {
                        let viewUrl = abp.appPath + 'app/DriverPayStatements/Details/' + full.id;
                        return '<div class="dropdown">'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '<ul class="dropdown-menu dropdown-menu-right">'
                            + '<li><a href="' + viewUrl + '" class="btnViewRow"><i class="fa fa-edit"></i> View</a></li>'
                            + '<li><a class="btnPrintRow"><i class="fas fa-print"></i> Print</a></li>'
                            + '<li><a class="btnExportRow"><i class="fas fa-file-excel"></i> Export</a></li>'
                            + (full.driverDateConflicts.length ? '<li><a class="btnPrintWarningsForRow"><i class="fas fas fa-exclamation-triangle"></i> Print Warnings</a></li>' : '')
                            + '<li><a class="btnDeleteRow"><i class="fa fa-trash"></i> Delete</a></li>'
                            + '</ul>'
                            + '</div>';
                    }
                }
            ]
        });

        var reloadMainGrid = function () {
            driverPayStatementsGrid.ajax.reload();
        };


        driverPayStatementsTable.on('click', '.btnPrintRow', function () {
            var record = _dtHelper.getRowData(this);
            _printDriverPayStatementModal.open({ id: record.id });
        });


        driverPayStatementsTable.on('click', '.btnExportRow', async function () {
            var record = _dtHelper.getRowData(this);
            var tempFile = await _payStatementService.exportPayStatementToCsv({ id: record.id });
            app.downloadTempFile(tempFile);
        });

        driverPayStatementsTable.on('click', '.btnPrintWarningsForRow', function () {
            var record = _dtHelper.getRowData(this);
            var reportParams = {
                id: record.id,
            };
            app.openPopup(abp.appPath + 'app/driverPayStatements/GetDriverPayStatementWarningReport?' + $.param(reportParams));
        });

        driverPayStatementsTable.on('click', '.btnDeleteRow', async function (e) {
            e.preventDefault();
            var record = _dtHelper.getRowData(this);
            if (await abp.message.confirm(
                'Are you sure you want to delete the pay statement?'
            )) {
                _payStatementService.deletePayStatement({
                    id: record.id
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        });

        abp.event.on('app.addDriverPayStatementModalSaved', function () {
            reloadMainGrid();
        });

        $("#AddPayStatementButton").click(function (e) {
            e.preventDefault();
            _addPayStatementModal.open();
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


