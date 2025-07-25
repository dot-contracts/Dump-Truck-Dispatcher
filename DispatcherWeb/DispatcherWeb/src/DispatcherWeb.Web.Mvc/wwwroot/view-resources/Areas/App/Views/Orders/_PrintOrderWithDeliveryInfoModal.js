(function ($) {
    app.modals.PrintOrderWithDeliveryInfoModal = function () {

        var _modalManager;
        var _$form = null;
        var _idInput = null;
        var _hidePricesInput = null;
        var _includeTicketsInput = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            var saveButton = _modalManager.getModal().find('.save-button');
            saveButton.find('span').text('Print');
            saveButton.find('i.fa-save').removeClass('fa-save').addClass('fa-print');

            _idInput = _$form.find('#Id');

            _modalManager.getModal().on('shown.bs.modal', function () {
                saveButton.focus();
            });

            _hidePricesInput = _$form.find("#HidePrices");
            _includeTicketsInput = _$form.find("#IncludeTickets");
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var id = _idInput.val();
            var hidePrices = _hidePricesInput.is(":checked");
            var includeTickets = _includeTicketsInput.is(":checked");

            var reportParams = app.order.getOrdersWithDeliveryInfoReportOptions({
                id: id,
                hidePrices: hidePrices,
                includeTickets: includeTickets
            });
            _modalManager.close();
            app.openPopup(abp.appPath + 'app/orders/GetWorkOrderReport?' + $.param(reportParams));
        };
    };
})(jQuery);
