(function () {
    'use strict';

    var _dtHelper = abp.helper.dataTables;

    abp.helper.reports.setReportService(abp.services.app.revenueBreakdownByTruckReport);

    abp.helper.reports.setFormDataHandler(function (formData) {
        $.extend(formData, _dtHelper.getDateRangeObject(formData.DeliveryDate, 'DeliveryDateBegin', 'DeliveryDateEnd'));
        delete formData.DeliveryDateFilter;

        if (formData.Shifts && !$.isArray(formData.Shifts)) {
            formData.Shifts = [formData.Shifts];
        }

        if (formData.TruckIds && !$.isArray(formData.TruckIds)) {
            formData.TruckIds = [formData.TruckIds];
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

    $("#TruckFilter").select2Init({
        abpServiceMethod: abp.services.app.truck.getTrucksSelectList,
        abpServiceParamsGetter: (params) => ({
            officeId: $('#OfficeIdFilter').val(),
            allOffices: true,
            excludeTrailers: true,
            includeLeaseHaulerTrucks: true,
        }),
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


    $('#Shifts').select2Init({
        showAll: true,
        allowClear: false
    });



})();
