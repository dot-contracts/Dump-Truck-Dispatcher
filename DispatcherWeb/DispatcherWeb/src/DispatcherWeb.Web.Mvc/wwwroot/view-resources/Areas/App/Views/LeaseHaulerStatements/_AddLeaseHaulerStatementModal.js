
(function ($) {
    app.modals.AddLeaseHaulerStatementModal = function () {

        var _modalManager;
        var _leaseHaulerStatementService = abp.services.app.leaseHaulerStatement;
        var _dtHelper = abp.helper.dataTables;
        var _$form = null;
        var _leaseHaulersDropdown = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            var modal = _modalManager.getModal();
            _$form = modal.find('form');
            _$form.validate();

            _leaseHaulersDropdown = _$form.find("#LeaseHaulerIds");
            _leaseHaulersDropdown.select2Init({
                abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulersSelectList,
                showAll: true,
                allowClear: false
            });
            _modalManager.onOpenOnce(function () {
                _leaseHaulersDropdown.val(null).change();
            });

            _$form.find("#DateRange").daterangepicker({
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

            _$form.find("#ReportOnly").change(function () {
                var reportOnly = $(this).is(':checked');
                _$form.find("#ReportOptions").toggle(reportOnly);
                modal.find('.save-button').text(reportOnly ? app.localize('Export') : app.localize('Save'));
            });

        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            let reportOnly = _$form.find("#ReportOnly").is(':checked');

            var model = _$form.serializeFormToObject();
            $.extend(model, _dtHelper.getDateRangeObject(model.DateRange, 'startDate', 'endDate'));
            model.LeaseHaulerIds = _leaseHaulersDropdown.val();
            delete model.dateRange;
            delete model.ExportLeaseHaulerStatementsOption;
            delete model.ReportOnly;

            try {
                _modalManager.setBusy(true);
                if (reportOnly) {
                    model.SplitByLeaseHauler = _$form.find("#SplitByLeaseHauler").is(':checked');
                    let tempFile = await _leaseHaulerStatementService.exportLeaseHaulerStatementIntermediatelyByDates(model);
                    app.downloadTempFile(tempFile);
                    abp.notify.info('Exported successfully.');
                } else {
                    await _leaseHaulerStatementService.addLeaseHaulerStatement(model);
                    abp.notify.info('Saved successfully.');
                    abp.event.trigger('app.addLeaseHaulerStatementModalSaved');
                }
                _modalManager.close();
            } finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);
