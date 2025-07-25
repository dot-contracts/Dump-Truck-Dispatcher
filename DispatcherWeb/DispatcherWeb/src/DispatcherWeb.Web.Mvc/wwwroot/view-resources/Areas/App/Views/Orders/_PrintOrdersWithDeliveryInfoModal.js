(function ($) {
    app.modals.PrintOrdersWithDeliveryInfoModal = function () {

        var _modalManager;
        var _orderService = abp.services.app.order;
        var _$form = null;
        var _dateInput = null;
        var _hidePricesInput = null;
        var _includeTicketsInput = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            var saveButton = _modalManager.getModal().find('.save-button');
            _modalManager.getModal().on('shown.bs.modal', function () {
                saveButton.focus();
                //_$form.find("#DateFilter").datepicker('hide');
            });

            _dateInput = _$form.find("#DateFilter");
            //_dateInput.val(moment().format("MM/DD/YYYY"));
            _dateInput.datepickerInit();

            _hidePricesInput = _$form.find("#HidePrices");
            _includeTicketsInput = _$form.find("#IncludeTickets");
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var date = _dateInput.val();

            var dateValue = moment(date, "MM/DD/YYYY", true);
            if (!dateValue.isValid()) {
                abp.message.warn("Date is incorrect!");
                return;
            }

            var hidePrices = _hidePricesInput.is(":checked");
            var includeTickets = _includeTicketsInput.is(":checked");

            var reportParams = app.order.getOrdersWithDeliveryInfoReportOptions({
                date: date,
                hidePrices: hidePrices,
                includeTickets: includeTickets
            });
            _orderService.doesWorkOrderReportHaveData(reportParams).done(function (result) {
                if (!result) {
                    abp.message.warn('There are no orders to print for ' + date + '.');
                    return;
                }
                _modalManager.close();
                app.openPopup(abp.appPath + 'app/orders/GetWorkOrderReport?' + $.param(reportParams));
            });
        };
    };
})(jQuery);
