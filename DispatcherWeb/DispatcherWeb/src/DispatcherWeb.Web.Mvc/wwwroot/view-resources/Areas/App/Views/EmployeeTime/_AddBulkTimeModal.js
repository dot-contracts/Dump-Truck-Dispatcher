(function ($) {

    app.modals.AddBulkTimeModal = function () {

        var _dtHelper = abp.helper.dataTables;
        var _driverService = abp.services.app.driver;
        var _employeeTimeService = abp.services.app.employeeTime;
        var _timeClassificationService = abp.services.app.timeClassification;
        var _modalManager;
        var _$form = null;
        var _modal;
        let _employeesSelectionColumn = {};

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modal = _modalManager.getModal();
            _$form = _modal.find('form');
            _$form.validate();

            _modal.find("#StartDateTime").datetimepickerInit();
            _modal.find("#EndDateTime").datetimepickerInit();

            var timeClassificationIdInput = _modal.find("#TimeClassificationId");
            timeClassificationIdInput.select2Init({
                abpServiceMethod: _timeClassificationService.getTimeClassificationsSelectList,
                showAll: true,
                allowClear: false
            });

            $('#BulkUsersTimeTable').DataTableInit({
                paging: false,
                info: false,
                ordering: false,
                selectionColumnOptions: _employeesSelectionColumn,
                ajax: function (data, callback, settings) {
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, _dtHelper.getFilterData());
                    abpData.maxResultCount = 1000;
                    abpData.status = abp.enums.filterActiveStatus.active;
                    abpData.hasUserId = true;
                    _driverService.getDrivers(abpData).done(function (abpResult) {
                        abpResult.items.sort((a, b) => (a.fullName > b.fullName) ? 1 : ((b.fullName > a.fullName) ? -1 : 0))
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                order: [[2, 'asc']],
                rowId: "id",
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
                        data: 'fullName',
                        width: 'auto',
                        orderable: false,
                        responsivePriority: 1,
                        title: 'Employee'
                    }
                ]
            });
        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var employeeIds = _employeesSelectionColumn.getSelectedRowsIds();

            if (employeeIds.length === 0) {
                $("#BulkUsersTimeTable_wrapper").closest(".form-group").addClass("has-danger");
                abp.message.error("Please select one or more employees to bulk add time.", 'Data is incomplete');
                return;
            }

            var employeeTime = _$form.serializeFormToObject();

            if (employeeTime.EndDateTime && !abp.helper.validateStartEndDates(
                { value: employeeTime.StartDateTime, title: _$form.find('label[for="StartDateTime"]').text() },
                { value: employeeTime.EndDateTime, title: _$form.find('label[for="EndDateTime"]').text() }
            )) {
                return;
            }

            employeeTime.driverIds = employeeIds;

            try {
                _modalManager.setBusy(true);

                await _employeeTimeService.addBulkTime(employeeTime);
                abp.notify.info('Bulk add completed successfully!');
                _modalManager.close();
                abp.event.trigger('app.addBulkTimeModalSaved');
            } finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);
