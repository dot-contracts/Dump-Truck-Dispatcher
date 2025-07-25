(function () {
    'use strict';

    var _dtHelper = abp.helper.dataTables;

    $(function () {

        //abp.helper.reports.setReportService(abp.services.app.revenueBreakdownReport);
        //abp.helper.reports.setFormDataHandler(function (formData) {
        //});

        //$("#DateFilter").val(moment().startOf('day').add(-1, 'd').format('MM/DD/YYYY')).datepickerInit();

        var _$form = $('#CreateReportForm');
        $('#DateFilter').val(moment().add(-1, 'd').format('MM/DD/YYYY - MM/DD/YYYY'));
        $("#DateFilter").daterangepicker({
            //autoUpdateInput: false,
            locale: {
                cancelLabel: 'Clear'
            },
            showDropDown: true
        }).on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));

            var formData = _$form.serializeFormToObject();
            var dateRange = extractDateRange(formData);
            if (dateRange.totalDays > 7) {
                // reset range to default
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.startDate.format('MM/DD/YYYY'));
                abp.message.error("This report is limited to a week. Please select a date range that doesn't exceed 7 days.");
            } else if (dateRange.totalDays <= 7 && dateRange.totalDays > 1) {
                setDriverIdAsRequired();
            } else {
                setDriverIdAsNotRequired();
            }
        }).on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
            setDriverIdAsNotRequired();
        });
        $('#DateFilter').on('change', function () {
            resetFilterValidation();
        });

        $("#DriverIdFilter").select2Init({
            abpServiceMethod: abp.services.app.driver.getDriversSelectList,
            showAll: false,
            allowClear: true
        });

        $('#CreateReportPdf, #CreateReportCsv').off('click');
        $('#CreateReportPdf').click(function (e) {
            e.preventDefault();

            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();
            var dateRange = extractDateRange(formData);
            formData = dateRange.data;
            var driverId = formData.DriverId;

            if (!formData.DateBegin || !formData.DateEnd) {
                abp.message.error('Date range is required.');
                return;
            }

            if (driverId && dateRange.totalDays > 7) {
                abp.message.error('Please select a date range of 7 days or less when a single driver is selected.');
                return;
            }

            if (!driverId && dateRange.totalDays > 1) {
                abp.message.error('Please select a date range of 1 day when no driver is selected.');
                return;
            }

            app.openPopup(abp.appPath + 'app/DriverActivityDetailReport/GetReport?' + $.param(formData));
        });

        function setDriverIdAsRequired() {
            _$form.find("#DriverIdFilter").attr('required', 'required');
            _$form.find("#DriverIdFilter").closest('.form-group').find('label').addClass('required-label');
        }

        function setDriverIdAsNotRequired() {
            _$form.find("#DriverIdFilter").removeAttr('required').removeAttr('aria-required');
            _$form.find("#DriverIdFilter").closest('.form-group').removeClass('has-error');
            _$form.find("#DriverIdFilter").closest('.form-group').find('label').removeClass('required-label');
        }

        function resetFilterValidation() {
            _$form.find('.has-danger').removeClass('has-danger');
            _$form.find('.form-control-feedback').remove();
            _$form.find('.form-control').removeAttr('aria-invalid');
        }

        function extractDateRange(formData) {
            $.extend(formData, _dtHelper.getDateRangeObject(formData.Date, 'DateBegin', 'DateEnd'));
            delete formData.Date;

            var startDate = moment(formData.DateBegin);
            var endDate = moment(formData.DateEnd);
            var daysInRange = endDate.diff(startDate, 'days') + 1;

            return {
                data: formData,
                totalDays: daysInRange
            }
        }
    });
})();
