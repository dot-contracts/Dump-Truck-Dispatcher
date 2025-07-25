(function () {
    'use strict';

    var _dtHelper = abp.helper.dataTables;

    abp.helper.reports.setReportService(abp.services.app.revenueBreakdownReport);

    abp.helper.reports.setFormDataHandler(function (formData) {
        $.extend(formData, _dtHelper.getDateRangeObject(formData.DeliveryDate, 'DeliveryDateBegin', 'DeliveryDateEnd'));
        delete formData.DeliveryDateFilter;

        if (formData.Shifts && !$.isArray(formData.Shifts)) {
            formData.Shifts = [formData.Shifts];
        }

    });

    $('#DeliveryDateFilter').val(moment().format('MM/DD/YYYY - MM/DD/YYYY'));
    $("#DeliveryDateFilter").daterangepicker({
        //autoUpdateInput: false,
        locale: {
            cancelLabel: 'Clear'
        },
        showDropDown: true
    }).on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
    }).on('cancel.daterangepicker', function (ev, picker) {
        $(this).val('');
    });

    $("#CustomerIdFilter").select2Init({
        abpServiceMethod: abp.services.app.customer.getCustomersSelectList,
        showAll: false,
        allowClear: true
    });

    $("#OfficeIdFilter").select2Init({
        abpServiceMethod: listCacheSelectLists.office(),
        showAll: true,
        allowClear: true
    });
    if (abp.session.officeId) {
        abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), abp.session.officeId, abp.session.officeName);
    }

    $("#LoadAtIdFilter").select2Init({
        abpServiceMethod: listCacheSelectLists.location(),
        abpServiceParams: {
            includeInactive: true
        },
        showAll: false,
        allowClear: true
    });
    $("#DeliverToIdFilter").select2Init({
        abpServiceMethod: listCacheSelectLists.location(),
        abpServiceParams: {
            includeInactive: true
        },
        showAll: false,
        allowClear: true
    });
    $("#ItemIdFilter").select2Init({
        abpServiceMethod: listCacheSelectLists.item(),
        abpServiceParams: {
            includeInactive: true
        },
        showAll: listCache.item.isEnabled,
        allowClear: true
    });

    $('#Shifts').select2Init({ allowClear: false });

})();
