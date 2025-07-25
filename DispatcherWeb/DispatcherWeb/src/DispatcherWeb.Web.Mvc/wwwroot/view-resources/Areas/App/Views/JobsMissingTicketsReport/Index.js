(function() {
    'use strict';
    
    let _dtHelper = abp.helper.dataTables;
    
    abp.helper.reports.setReportService(abp.services.app.jobsMissingTicketsReport);
    
    abp.helper.reports.setFormDataHandler(function (formData) {
        $.extend(formData, _dtHelper.getDateRangeObject(formData.DeliveryDate, 'DeliveryDateBegin', 'DeliveryDateEnd'));
        delete formData.DeliveryDateFilter;        
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
})();