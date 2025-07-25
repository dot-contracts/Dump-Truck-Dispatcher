(function ($) {
    app.modals.SelectOrderQuoteModal = function () {

        var _modalManager;
        var _$form = null;
        var _quoteInput = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _quoteInput = _$form.find('#PopupQuoteId');

            abp.event.trigger('app.selectOrderQuoteModal.requestInput', function (input) {
                $(input)
                    .find('option')
                    .clone()
                    .removeAttr('data-select2-id')
                    .appendTo(_quoteInput);

                _quoteInput.select2Init({
                    showAll: true,
                    allowClear: true,
                });
            });
        };

        this.save = function () {
            _modalManager.setResult(_quoteInput.val());
            _modalManager.close();
        };
    };
})(jQuery);
