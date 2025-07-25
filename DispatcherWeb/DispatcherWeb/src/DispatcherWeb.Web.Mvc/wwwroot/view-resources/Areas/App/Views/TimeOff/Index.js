(function () {
    $(function () {

        var _timeOffService = abp.services.app.timeOff;
        var _dtHelper = abp.helper.dataTables;
        var _lastAbpData = null;

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/TimeOff/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/TimeOff/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditTimeOffModal',
            getDefaultFocusElement: function (modal) {
                let element = modal.find("#EmployeeId");
                return {
                    focus: function () {
                        element.select2('focus');
                    }
                };
            }
        });

        var _addTimeEntryModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/TimeOff/AddTimeEntryModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/TimeOff/_AddTimeEntryModal.js',
            modalClass: 'AddEmployeeTimeEntryModal'
        });

        $("#DateFilter").daterangepicker({
            //autoUpdateInput: false,
            locale: {
                cancelLabel: 'Clear'
            }
        })
            .on('apply.daterangepicker', function (ev, picker) {
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
                reloadMainGrid();
            })
            .on('cancel.daterangepicker', function (ev, picker) {
                $(this).val('');
                reloadMainGrid();
            });

        $("#DriverIdFilter").select2Init({
            abpServiceMethod: _timeOffService.getDriversSelectList,
            showAll: false,
            allowClear: true
        });

        $("#DriverIdFilter").change(function () {
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

        var timeOffTable = $('#TimeOffTable');
        var timeOffGrid = timeOffTable.DataTableInit({
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                _lastAbpData = abpData;
                $.extend(abpData, _dtHelper.getFilterData());
                $.extend(abpData, _dtHelper.getDateRangeObject(abpData.startDateRange, 'startDateStart', 'startDateEnd'));
                delete abpData.startDateRange;
                _timeOffService.getTimeOffRecords(abpData).done(function (abpResult) {
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
                    data: 'driverName',
                    title: 'Driver'
                },
                {
                    data: 'startDate',
                    title: 'Start Date',
                    render: function (data, type, full, meta) { return _dtHelper.renderUtcDate(data); }
                },
                {
                    data: 'endDate',
                    title: 'End Date',
                    render: function (data, type, full, meta) { return _dtHelper.renderUtcDate(data); }
                },
                {
                    data: 'requestedHours',
                    title: 'Requested Time (Hr)',
                    render: function (data, type, full, meta) { return abp.utils.roundTo(data, 2); }
                },
                {
                    data: 'paidHours',
                    title: 'Paid Time (Hr)',
                    render: function (data, type, full, meta) { return abp.utils.roundTo(data, 2); }
                },
                {
                    data: 'reason',
                    title: 'Reason'
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
                            + '<li><a class="btnAddTimeEntry"><i class="fa fa-stopwatch"></i> Add Time Entry</a></li>'
                            + '</ul>'
                            + '</div>';
                    }
                }
            ]
        });

        timeOffTable.on('click', '.btnDeleteRow', function () {
            var record = _dtHelper.getRowData(this);
            deleteTimeOff(record);
        });

        timeOffTable.on('click', '.btnEditRow', function () {
            var timeOffId = _dtHelper.getRowData(this).id;
            _createOrEditModal.open({ id: timeOffId });
        });

        timeOffTable.on('click', '.btnAddTimeEntry', function () {
            var timeOffId = _dtHelper.getRowData(this).id;
            _addTimeEntryModal.open({ id: timeOffId });
        });

        var reloadMainGrid = function () {
            timeOffGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditTimeOffModalSaved', function () {
            reloadMainGrid();
        });

        abp.event.on('app.addEmployeeTimeEntryModalSaved', function () {
            //reloadMainGrid();
        });

        $("#CreateNewTimeOffButton").click(function (e) {
            e.preventDefault();
            _createOrEditModal.open();
        });

        async function deleteTimeOff(record) {
            if (await abp.message.confirm(
                'Are you sure you want to delete the time off request?'
            )) {
                _timeOffService.deleteTimeOff({
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
            _timeOffService
                .getTimeOffRecordsToCsv(abpData)
                .done(function (result) {
                    app.downloadTempFile(result);
                }).always(function () {
                    abp.ui.clearBusy($button);
                });
        });
    });
})();
