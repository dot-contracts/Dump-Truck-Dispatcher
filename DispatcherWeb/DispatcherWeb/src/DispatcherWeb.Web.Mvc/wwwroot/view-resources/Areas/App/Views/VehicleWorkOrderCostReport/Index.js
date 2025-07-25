(function () {
    'use strict';

    var _dtHelper = abp.helper.dataTables;

    abp.helper.reports.setReportService(abp.services.app.vehicleWorkOrderCostReport);

    abp.helper.reports.setFormDataHandler(function (formData) {
        $.extend(formData, _dtHelper.getDateRangeObject(formData.IssueDateFilter, 'IssueDateBegin', 'IssueDateEnd'));
        delete formData.IssueDateFilter;

        $.extend(formData, _dtHelper.getDateRangeObject(formData.StartDateFilter, 'StartDateBegin', 'StartDateEnd'));
        delete formData.StartDateFilter;

        $.extend(formData, _dtHelper.getDateRangeObject(formData.CompletionDateFilter, 'CompletionDateBegin', 'CompletionDateEnd'));
        delete formData.CompletionDateFilter;

        if (formData.OfficeIds && !$.isArray(formData.OfficeIds)) {
            formData.OfficeIds = [formData.OfficeIds];
        }
    });
        
    $("#StartDateFilter, #CompletionDateFilter, #IssueDateFilter").daterangepicker({
        locale: {
            cancelLabel: 'Clear'
        }
    }).on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
    }).on('cancel.daterangepicker', function (ev, picker) {
        $(this).val('');
    });

    $("#TruckFilter").select2Init({
        abpServiceMethod: abp.services.app.truck.getTrucksSelectList,
        abpServiceParams: {
            allOffices: true,
            inServiceOnly: true
        },
        showAll: false,
        allowClear: true
    });

    $('#AssignedToFilter').select2Init({
        abpServiceMethod: abp.services.app.user.getMaintenanceUsersSelectList,
        showAll: false,
        allowClear: true
    });

    $("#StatusFilter").select2Init({
        showAll: true,
        allowClear: true
    });

    $("#OfficeIdsFilter").select2Init({
        abpServiceMethod: listCacheSelectLists.office(),
        showAll: true,
        allowClear: true
    });
    if (abp.session.officeId) {
        abp.helper.ui.addAndSetDropdownValue($("#OfficeIdsFilter"), abp.session.officeId, abp.session.officeName);
    }

})();
