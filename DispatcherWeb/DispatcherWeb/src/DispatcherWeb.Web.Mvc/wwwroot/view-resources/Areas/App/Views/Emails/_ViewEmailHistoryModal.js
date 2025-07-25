(function ($) {
    app.modals.ViewEmailHistoryModal = function () {

        var _modalManager;
        var _modal;
        var _emailService = abp.services.app.email;
        var _dtHelper = abp.helper.dataTables;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _modal = _modalManager.getModal();

            _modal.find('.save-button').hide();
            _modal.find('.cancel-button').text('Close');

            _modalManager.getOptions().getDefaultFocusElement = function (modal) { return modal.find('.cancel-button'); };

            var $customerIdFilter = _modal.find("#CustomerIdFilterForEmailHistory");
            var $quoteIdFilter = _modal.find("#QuoteIdFilter");

            $customerIdFilter.select2Init({
                abpServiceMethod: abp.services.app.customer.getCustomersSelectList,
                showAll: true
            });

            var quoteChildDropdown = abp.helper.ui.initChildDropdown({
                parentDropdown: $customerIdFilter,
                childDropdown: $quoteIdFilter,
                abpServiceMethod: abp.services.app.quote.getQuotesForCustomer
            });

            $quoteIdFilter.select2Init({
                showAll: true
            });

            var emailHistoryTable = _modal.find('#EmailHistoryTable');
            var emailHistoryGrid = emailHistoryTable.DataTableInit({
                ajax: function (data, callback, settings) {
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, _dtHelper.getFilterData(_modal.find(".filter")));
                    _emailService.getEmailHistory(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                order: [[1, 'desc']],
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
                        data: 'receiverId',
                        orderable: false,
                        render: function (data, type, full, meta) {
                            var icon = abp.helper.ui.getEmailDeliveryStatusIcon(full.receiverDeliveryStatus);
                            if (!icon) {
                                return '';
                            }
                            //var title = $("<span>").text(full.receiverDeliveryStatusFormatted);
                            return $("<div>").append(icon).html();
                        },
                        title: ' ' //"Delivery Status"
                    },
                    {
                        data: "emailCreationTime",
                        render: function (data, type, full, meta) { return _dtHelper.renderUtcDateTime(data); },
                        title: "Sent Time"
                    },
                    {
                        data: "emailSubject",
                        title: "Subject"
                    },
                    {
                        data: "receiverEmail",
                        title: "Receiver Email"
                    },
                    //{
                    //    data: "receiverKindFormatted",
                    //    title: "Receiver Kind"
                    //},
                    {
                        data: "emailDeliveryTime",
                        orderable: false,
                        render: function (data, type, full, meta) { return _dtHelper.renderUtcDateTime(data); },
                        title: "Delivery Time"
                    },
                    {
                        data: "emailOpenedTime",
                        orderable: false,
                        render: function (data, type, full, meta) { return _dtHelper.renderUtcDateTime(data); },
                        title: "Opened Time"
                    },
                    {
                        data: "emailFailedTime",
                        orderable: false,
                        render: function (data, type, full, meta) { return _dtHelper.renderUtcDateTime(data); },
                        title: "Delivery Failed Time"
                    }
                ]
            });

            _modal.on('shown.bs.modal', function () {
                emailHistoryGrid
                    .columns.adjust()
                    .responsive.recalc();
                _modal.find('.cancel-button').focus();
            });

            function reloadMainGrid() {
                emailHistoryGrid.ajax.reload();
            }

            _modal.find("#SearchButton").closest('form').submit(function (e) {
                e.preventDefault();
                reloadMainGrid();
            });

            _modal.find("#ClearSearchButton").click(function () {
                $quoteIdFilter.val(null).change();
                $customerIdFilter.val(null).change();
                _modal.find("#OrderIdFilter").val(null);
                $(".filter").change();
                reloadMainGrid();
            });
        };

        this.save = function () {

        };
    };
})(jQuery);
