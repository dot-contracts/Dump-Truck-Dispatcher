
(function ($) {
    app.modals.PrintDriverPayStatementModal = function () {

        var _modalManager;
        var _payStatementService = abp.services.app.payStatement;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            var modal = _modalManager.getModal();
            _$form = modal.find('form');
            _$form.validate();

            var saveButton = _modalManager.getModal().find('.save-button');
            saveButton.find('span').text('Print');
            saveButton.find('i.fa-save').removeClass('fa-save').addClass('fa-print');

            _modalManager.getModal().on('shown.bs.modal', function () {
                saveButton.focus();
            });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            //_$form.serializeFormToObject();
            var reportParams = {
                id: _$form.find("#Id").val(),
                splitByDriver: _$form.find('#PrintSeparatePdfs').is(':checked')
            };
            _modalManager.close();
            app.openPopup(abp.appPath + 'app/driverPayStatements/GetDriverPayStatementReport?' + $.param(reportParams));

            //_orderService.doesOrderSummaryReportHaveData(reportParams).done(function (result) {
            //    if (!result) {
            //        abp.message.warn(noDataMessage);
            //        return;
            //    }
            //    _modalManager.close();
            //    app.openPopup(abp.appPath + 'app/driverPayStatements/GetDriverPayStatementReport?' + $.param(reportParams));
            //});
        };
    };
})(jQuery);
