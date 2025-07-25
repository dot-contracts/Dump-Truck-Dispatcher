(function ($) {
    app.modals.CreateOrEditRateModal = function () {

        var _modalManager;
        var _productLocationAppService = abp.services.app.productLocation;
        var _$form = null;
        var _materialUomDropdown = null;
        var _locationDropdown = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _locationDropdown = _$form.find("#LocationId");
            _materialUomDropdown = _$form.find("#ItemPrice_MaterialUomId");

            _materialUomDropdown.select2Uom();

            _locationDropdown.select2Init({
                abpServiceMethod: listCacheSelectLists.location(),
                showAll: true,
                allowClear: true
            });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var itemPrice = _$form.serializeFormToObject();
            itemPrice.productLocationPrices = getPrices();

            _modalManager.setBusy(true);
            _productLocationAppService.editProductLocation(itemPrice).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditItemPriceModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };

        function getPrices() {
            var $prices = _$form.find('.prices');
            var prices = [];
            $prices.each(function (i) {
                var price = {};
                var $inputs = $(this).find('input');
                $inputs.each(function (e) {
                    var $input = $(this);
                    var inputName = $input.attr('name').replace('Price-', '');
                    price[inputName] = $input.val();
                });
                prices.push(price);
            });
            return prices;
        }
    };
})(jQuery);
