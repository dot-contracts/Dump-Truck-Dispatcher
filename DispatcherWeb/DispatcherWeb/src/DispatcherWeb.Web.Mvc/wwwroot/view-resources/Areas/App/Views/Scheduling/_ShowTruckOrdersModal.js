(function ($) {
    app.modals.ShowTruckOrdersModal = function () {

        var _modalManager;
        var _schedulingService = abp.services.app.scheduling;
        var _$form = null;
        var _dtHelper = abp.helper.dataTables;
        var _separateItems = abp.features.isEnabled('App.SeparateMaterialAndFreightItems');
        var _permissions = {
            editOrder: abp.auth.isGranted('Pages.Orders.Edit')
        };

        var _createOrEditOrderModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CreateOrEditOrderModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CreateOrEditOrderModal.js',
            modalClass: 'CreateOrEditOrderModal',
            modalSize: 'xl'
        });

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _$form = _modalManager.getModal().find('form');

            var $truckOrdersTable = _modalManager.getModal().find('#TruckOrdersTable');
            var $truckOrdersGrid = $truckOrdersTable.DataTableInit({
                paging: false,
                ordering: false,
                ajax: function (data, callback, settings) {
                    _schedulingService.getTruckOrderLinesPaged({
                        scheduleDate: _$form.find('#ScheduleDate').val(),
                        shift: _$form.find('#Shift').val(),
                        truckId: _$form.find('#TruckId').val()
                    }).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                columns: [
                    {
                        className: 'control responsive',
                        orderable: false,
                        title: "&nbsp;",
                        width: "10px",
                        render: function () {
                            return '';
                        }
                    },
                    {
                        data: "driverName",
                        title: "Driver"
                    },
                    {
                        data: "startTime",
                        title: "Time on Job",
                        render: function (data, type, full, meta) {
                            return _dtHelper.renderTime(data, '');
                        }
                    },
                    {
                        data: "customer",
                        title: "Customer"
                    },
                    {
                        data: "loadAtName",
                        title: "Load At"
                    },
                    {
                        data: "deliverToName",
                        title: "Deliver to"
                    },
                    {
                        data: "utilization",
                        title: "Portion of day"
                    },
                    {
                        data: "freightItem",
                        title: _separateItems ? 'Freight Item' : 'Item'
                    },
                    {
                        data: "materialItem",
                        visible: _separateItems,
                        title: "Material Item"
                    },
                    {
                        data: "quantityFormatted",
                        title: "Quantity"
                    },
                    {
                        title: "",
                        data: null,
                        orderable: false,
                        autoWidth: false,
                        width: "100px",
                        responsivePriority: 2,
                        render: function (data, type, full, meta) {
                            return _permissions.editOrder ? '<a class="btn btn-default btnViewOrder">View</a>' : '';
                        }
                    }
                ]
            });

            $truckOrdersTable.hide();
            modalManager.getModal().on('shown.bs.modal', function () {
                $truckOrdersTable.show(0);
                $truckOrdersGrid.columns.adjust().responsive.recalc();
            });

            $truckOrdersTable.on('click', '.btnViewOrder', function (e) {
                e.preventDefault();
                var orderId = _dtHelper.getRowData(this).orderId;
                _createOrEditOrderModal.open({ id: orderId });
            });

        };

    };
})(jQuery);
