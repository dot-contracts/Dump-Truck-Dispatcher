(function ($) {
    app.modals.ViewQuoteDeliveriesModal = function () {

        var _modalManager;
        var _quoteService = abp.services.app.quote;
        var _$form = null;
        var _quoteLineId = null;
        var _quotedMaterialQuantity = 0;
        var _quotedFreightQuantity = 0;
        var _actualMaterialQuantity = 0;
        var _actualFreightQuantity = 0;
        var _dtHelper = abp.helper.dataTables;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');

            _modalManager.getModal().find('.close-button').text('Cancel');

            _quoteLineId = _$form.find('#QuoteLineId').val();
            _quotedMaterialQuantity = Number(_$form.find('#QuotedMaterialQuantity').val());
            _quotedFreightQuantity = Number(_$form.find('#QuotedFreightQuantity').val());

            var quoteDeliveryTable = _modalManager.getModal().find('#QuoteDeliveryTable');
            var quoteDeliveryGrid = quoteDeliveryTable.DataTableInit({
                paging: false,
                ordering: false,
                info: false,
                ajax: function (data, callback, settings) {
                    _quoteService.getQuoteLineDeliveries({ id: _quoteLineId }).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                footerRowCount: 4,
                footerCallback: function (tfoot, data, start, end, display) {
                    _actualMaterialQuantity = data.map(function (x) { return x.actualMaterialQuantity; }).reduce(function (a, b) { return a + b; }, 0);
                    _actualFreightQuantity = data.map(function (x) { return x.actualFreightQuantity; }).reduce(function (a, b) { return a + b; }, 0);
                    var materialPercent = _quotedMaterialQuantity ? abp.utils.round(_actualMaterialQuantity / _quotedMaterialQuantity * 100) + '%' : '';
                    var freightPercent = _quotedFreightQuantity ? abp.utils.round(_actualFreightQuantity / _quotedFreightQuantity * 100) + '%' : '';

                    var api = this.api();

                    var dateIndex = api.column('date:name').index();
                    var materialIndex = this.api().column('actualMaterialQuantity:name').index();
                    var freightIndex = this.api().column('actualFreightQuantity:name').index();

                    var footerRows = $('tr', api.table().footer());
                    footerRows.eq(0).find('th').eq(materialIndex).text('Material');
                    footerRows.eq(0).find('th').eq(freightIndex).text('Freight');
                    footerRows.eq(1).find('th').eq(dateIndex).text('Quoted');
                    footerRows.eq(1).find('th').eq(materialIndex).text(_dtHelper.formatNumber(_quotedMaterialQuantity));
                    footerRows.eq(1).find('th').eq(freightIndex).text(_dtHelper.formatNumber(_quotedFreightQuantity));
                    footerRows.eq(2).find('th').eq(dateIndex).text('Actual');
                    footerRows.eq(2).find('th').eq(materialIndex).text(_dtHelper.formatNumber(_actualMaterialQuantity));
                    footerRows.eq(2).find('th').eq(freightIndex).text(_dtHelper.formatNumber(_actualFreightQuantity));
                    footerRows.eq(3).find('th').eq(dateIndex).text('% Complete');
                    footerRows.eq(3).find('th').eq(materialIndex).text(materialPercent);
                    footerRows.eq(3).find('th').eq(freightIndex).text(freightPercent);
                },
                columns: [
                    {
                        width: '20px',
                        className: 'control responsive',
                        orderable: false,
                        render: function () {
                            return '';
                        }
                    },
                    {
                        data: "date",
                        render: function (data, type, full, meta) { return _dtHelper.renderUtcDate(data); },
                        name: 'date',
                        title: "Date"
                    },
                    {
                        data: "designation",
                        render: function (data, type, full, meta) { return _dtHelper.renderText(full.designationName); },
                        title: "Designation"
                    },
                    {
                        data: "actualMaterialQuantity",
                        render: function (data, type, full, meta) { return _dtHelper.formatNumber(full.actualMaterialQuantity); },
                        name: "actualMaterialQuantity",
                        title: "Actual Material Quantity"
                    },
                    {
                        data: "actualFreightQuantity",
                        render: function (data, type, full, meta) { return _dtHelper.formatNumber(full.actualFreightQuantity); },
                        name: "actualFreightQuantity",
                        title: "Actual Freight Quantity"
                    }
                ]
            });

            _modalManager.getModal().on('shown.bs.modal', function () {
                quoteDeliveryGrid
                    .columns.adjust()
                    .responsive.recalc();
            });

        };

        this.save = function () {
        };

    };
})(jQuery);
