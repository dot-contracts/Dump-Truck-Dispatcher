(function ($) {
    app.modals.PrintPaymentReconciliationReportModal = function () {

        var _modalManager;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            var saveButton = _modalManager.getModal().find('.save-button');
            saveButton.find('span').text('Print');
            saveButton.find('i.fa-save').removeClass('fa-save').addClass('fa-print');

            var dateRangeInput = _$form.find("#DateRange");
            var startDateInput = _$form.find("#StartDate");
            var endDateInput = _$form.find("#EndDate");

            dateRangeInput.val(moment(startDateInput.val(), 'MM/DD/YYYY').format('MM/DD/YYYY') + ' - ' + moment(endDateInput.val(), 'MM/DD/YYYY').format('MM/DD/YYYY'));

            dateRangeInput.daterangepicker({
            }, function (start, end, label) {
                startDateInput.val(start.format('MM/DD/YYYY'));
                endDateInput.val(end.format('MM/DD/YYYY'));
            });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            _modalManager.close();

            app.openPopup(abp.appPath + 'app/orders/GetPaymentReconciliationReport?' + $.param({
                startDate: formData.StartDate,
                endDate: formData.EndDate,
                allOffices: formData.AllOffices || false
            }));
        };
    };
})(jQuery);
