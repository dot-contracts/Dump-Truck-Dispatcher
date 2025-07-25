(function ($) {
    app.modals.CreateOrEditHaulingZoneRateModal = function () {
        var _modalManager;
        var _haulingCategoryAppService = abp.services.app.haulingCategory;
        var _$form = null;
        var _materialUomDropdown = null;
        var _truckCategoryDropdown = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _truckCategoryDropdown = _$form.find("#TruckCategoryId");
            _materialUomDropdown = _$form.find("#ItemPrice_MaterialUomId");

            _materialUomDropdown.select2Uom();

            _truckCategoryDropdown.select2Init({
                abpServiceMethod: listCacheSelectLists.vehicleCategory(),
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
            itemPrice.haulingCategoryPrices = getPrices();
            itemPrice.LeaseHaulerRate = itemPrice.LeaseHaulerRate || 0;
            _modalManager.setBusy(true);
            _haulingCategoryAppService.editHaulingCategory(itemPrice).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditHaulingZoneItemPriceModalSaved');
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
