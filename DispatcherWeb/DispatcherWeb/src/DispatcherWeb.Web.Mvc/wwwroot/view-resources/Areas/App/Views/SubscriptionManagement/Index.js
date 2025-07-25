(function () {
    $(function () {

        var _$paymentHistoryTable = $('#PaymentHistoryTable');
        var _paymentService = abp.services.app.payment;
        var _invoiceService = abp.services.app.invoice;
        var _dataTable;

        function createDatatable() {
            var dataTable = _$paymentHistoryTable.DataTableInit({
                paging: true,
                serverSide: true,
                processing: true,
                listAction: {
                    ajaxFunction: _paymentService.getPaymentHistory
                },
                columns: [
                    {
                        className: 'control responsive',
                        orderable: false,
                        render: function () {
                            return '';
                        },
                    },
                    {
                        data: null,
                        orderable: false,
                        defaultContent: '',
                        rowAction: {
                            element: $("<button/>")
                                .addClass("btn btn-xs btn-primary")
                                .text(app.localize('ShowInvoice'))
                                .click(function () {
                                    createOrShowInvoice($(this).data());
                                })
                        }
                    },
                    {
                        data: "creationTime",
                        render: function (creationTime) {
                            return moment(creationTime).format('L');
                        }
                    },
                    {
                        data: "editionDisplayName"
                    },
                    {
                        data: "gateway",
                        render: function (gateway) {
                            return app.localize("SubscriptionPaymentGatewayType_" + gateway);
                        }
                    },
                    {
                        data: "amount",
                        render: $.fn.dataTable.render.number(',', '.', 2)
                    },
                    {
                        data: "status",
                        render: function (status) {
                            return app.localize("SubscriptionPaymentStatus_" + status);
                        }
                    },
                    {
                        data: "paymentPeriodType",
                        render: function (paymentPeriodType) {
                            return app.localize("PaymentPeriodType_" + paymentPeriodType);
                        }
                    },
                    {
                        data: "dayCount"
                    },
                    {
                        data: "paymentId"
                    },
                    {
                        data: "invoiceNo"
                    },
                    {
                        visible: false,
                        data: "id"
                    }
                ]
            });

            return dataTable;
        }

        $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
            var target = $(e.target).attr("href");
            if (target === '#SubscriptionManagementPaymentHistoryTab') {

                if (_dataTable) {
                    return;
                }

                _dataTable = createDatatable();
            }
        });

        function createOrShowInvoice(data) {
            var invoiceNo = data["invoiceNo"];
            var paymentId = data["id"];

            if (invoiceNo) {
                app.openPopup('/App/Invoice?paymentId=' + paymentId, '_blank');
            } else {
                _invoiceService.createInvoice({
                    subscriptionPaymentId: paymentId
                }).done(function () {
                    _dataTable.ajax.reload();
                    app.openPopup('/App/Invoice?paymentId=' + paymentId, '_blank');
                });
            }
        }
    });
})();
