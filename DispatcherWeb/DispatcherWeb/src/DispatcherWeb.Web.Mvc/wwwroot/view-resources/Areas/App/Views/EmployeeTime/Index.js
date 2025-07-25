(function () {
    $(function () {

        var _employeeTimeService = abp.services.app.employeeTime;
        var _dtHelper = abp.helper.dataTables;
        var _lastAbpData = null;

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/EmployeeTime/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/EmployeeTime/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditEmployeeTimeModal',
            getDefaultFocusElement: function (modal) {
                let element = modal.find("#StartDateTime");
                return {
                    focus: function () {
                        element.focus(); //.select2('focus');
                    }
                };
            }
        });

        var _addBulkTimeModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/EmployeeTime/AddBulkTimeModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/EmployeeTime/_AddBulkTimeModal.js',
            modalClass: 'AddBulkTimeModal',
            getDefaultFocusElement: function (modal) {
                let element = modal.find("#EmployeeId");
                return {
                    focus: function () {
                        if (element)
                            element.select2('focus');
                    }
                };
            }
        });

        $("#DateFilter").daterangepicker({
            //autoUpdateInput: false,
            locale: {
                cancelLabel: 'Clear'
            }
        }).on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            reloadMainGrid();
        }).on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
            reloadMainGrid();
        });

        $("#TimeClassificationIdFilter").select2Init({
            abpServiceMethod: abp.services.app.timeClassification.getTimeClassificationsSelectList,
            showAll: true,
            allowClear: true
        });

        $("#EmployeeIdFilter").select2Init({
            //abpServiceMethod: abp.services.app.user.getUsersSelectList,
            abpServiceMethod: _employeeTimeService.getUsersSelectList,
            showAll: false,
            allowClear: true
        });

        $("#TimeClassificationIdFilter, #EmployeeIdFilter").change(function () {
            reloadMainGrid();
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

        var employeeTimeTable = $('#EmployeeTimeTable');
        var employeeTimeGrid = employeeTimeTable.DataTableInit({
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                _lastAbpData = abpData;
                $.extend(abpData, _dtHelper.getFilterData());
                $.extend(abpData, _dtHelper.getDateRangeObject(abpData.startDateRange, 'startDateStart', 'startDateEnd'));
                delete abpData.startDateRange;
                _employeeTimeService.getEmployeeTimeRecords(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            order: [[1, 'asc'], [2, 'asc']],
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
                    responsivePriority: 1,
                    data: 'employeeName',
                    title: 'Employee'
                },
                {
                    data: 'startDateTime',
                    title: 'Start Date Time',
                    render: function (data, type, full, meta) { return _dtHelper.renderDateShortTime(data); }
                },
                {
                    data: 'endDateTime',
                    title: 'End Date Time',
                    render: function (data, type, full, meta) { return _dtHelper.renderDateShortTime(data); }
                },
                {
                    data: 'timeClassificationName',
                    title: 'Time Classification'
                },
                {
                    data: 'elapsedHoursSort',
                    title: 'Elapsed Time (Hr)',
                    render: function (data, type, full, meta) { return abp.utils.roundTo(full.elapsedHours, 2); }
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    width: '10px',
                    visible: abp.auth.hasAnyOfPermissions('Pages.TimeEntry.EditAll', 'Pages.TimeEntry.EditPersonal'),
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

        employeeTimeTable.on('click', '.btnDeleteRow', function () {
            var record = _dtHelper.getRowData(this);
            deleteEmployeeTime(record);
        });

        employeeTimeTable.on('click', '.btnEditRow', function () {
            var employeeTimeId = _dtHelper.getRowData(this).id;
            _createOrEditModal.open({ id: employeeTimeId });
        });

        var reloadMainGrid = function () {
            employeeTimeGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditEmployeeTimeModalSaved', function () {
            reloadMainGrid();
        });

        abp.event.on('app.addBulkTimeModalSaved', function () {
            reloadMainGrid();
        });

        $("#CreateNewEmployeeTimeButton").click(function (e) {
            e.preventDefault();
            _createOrEditModal.open();
        });

        $("#AddBulkTimeButton").click(function (e) {
            e.preventDefault();
            _addBulkTimeModal.open();
        });

        async function deleteEmployeeTime(record) {
            if (await abp.message.confirm(
                'Are you sure you want to delete the time record?'
            )) {
                _employeeTimeService.deleteEmployeeTime({
                    id: record.id
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        }

        $('#ExportToCsvButton').click(function () {
            var $button = $(this);
            var abpData = $.extend({}, _lastAbpData);
            if (!abpData) {
                return;
            }
            $.extend(abpData, _dtHelper.getFilterData());
            $.extend(abpData, _dtHelper.getDateRangeObject(abpData.startDateRange, 'startDateStart', 'startDateEnd'));
            delete abpData.startDateRange;
            abp.ui.setBusy($button);
            _employeeTimeService
                .getEmployeeTimeRecordsToCsv(abpData)
                .done(function (result) {
                    app.downloadTempFile(result);
                }).always(function () {
                    abp.ui.clearBusy($button);
                });
        });
    });
})();
