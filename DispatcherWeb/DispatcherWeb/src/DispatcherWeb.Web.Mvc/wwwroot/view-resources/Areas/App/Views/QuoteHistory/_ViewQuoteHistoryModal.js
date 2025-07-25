(function ($) {
    app.modals.ViewQuoteHistoryModal = function () {

        var _modalManager;
        var _quoteHistoryService = abp.services.app.quoteHistory;
        var _$form = null;
        var _dtHelper = abp.helper.dataTables;
        var _modalOptions = null;

        this.init = function (modalManager) {
            const _createOrEditQuoteModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Quotes/CreateOrEditQuoteModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Quotes/_CreateOrEditQuoteModal.js',
                modalClass: 'CreateOrEditQuoteModal',
                modalSize: 'xxl',
            });

            _modalManager = modalManager;
            _modalOptions = _modalManager.getArgs();
            _$form = _modalManager.getModal().find('form');

            _modalManager.getModal().find('.save-button').hide();
            _modalManager.getModal().find('.close-button').text('Close');

            if (_modalOptions.hideGoToQuoteButton) {
                _modalManager.getModal().find('#GoToQuoteButton').hide();
            }           

            var fieldDiffsTable = _modalManager.getModal().find('#FieldDiffsTable');
            var fieldDiffsGrid = fieldDiffsTable.DataTableInit({
                paging: false,
                ordering: false,
                info: false,
                ajax: function (data, callback, settings) {
                    _quoteHistoryService.getQuoteFieldDiffDtos({ id: _$form.find('#Id').val() }).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                columns: [
                    {
                        data: "fieldName",
                        title: app.localize('Field')
                    },
                    {
                        data: "oldDisplayValue",
                        title: app.localize('OldValue')
                    },
                    {
                        data: "newDisplayValue",
                        title: app.localize('NewValue')
                    }
                ]
            });

            _modalManager.getModal().on('shown.bs.modal', function () {
                fieldDiffsGrid
                    .columns.adjust()
                    .responsive.recalc();
            });

            _modalManager.getModal().on('hidden.bs.modal', function () {
                abp.event.trigger('app.viewQuoteHistoryModalClosed');
            });

            _modalManager.getModal().find('#GoToQuoteButton').click(function (e) {
                e.preventDefault();
                _createOrEditQuoteModal.open({ id: _$form.find('#QuoteId').val() });
            });
        };

        this.save = function () {

        };
    };
})(jQuery);
