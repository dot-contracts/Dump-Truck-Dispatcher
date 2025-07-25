(function () {
    $(function () {

        var _orderService = abp.services.app.order;
        var _quoteService = abp.services.app.quote;
        var _dtHelper = abp.helper.dataTables;
        var _permissions = {
            edit: abp.auth.isGranted('Pages.Orders.Edit'),
            print: abp.auth.isGranted('Pages.PrintOrders'),
            viewQuotes: abp.auth.isGranted('Pages.Quotes.View'),
            editQuotes: abp.auth.isGranted('Pages.Quotes.Edit'),
            manageAllOffices: abp.auth.isGranted('Pages.OfficeAccess.All')
        };
        var _isFilterReady = false;
        var _isGridInitialized = false;

        var _copyOrderModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CopyOrderModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CopyOrderModal.js',
            modalClass: 'CopyOrderModal'
        });

        var _createQuoteFromOrderModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CreateQuoteFromOrderModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CreateQuoteFromOrderModal.js',
            modalClass: 'CreateQuoteFromOrderModal'
        });

        var _viewEmailHistoryModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Emails/ViewEmailHistoryModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Emails/_ViewEmailHistoryModal.js',
            modalClass: 'ViewEmailHistoryModal',
            modalSize: 'lg'
        });

        var _printOrderWithDeliveryInfoModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/PrintOrderWithDeliveryInfoModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_PrintOrderWithDeliveryInfoModal.js',
            modalClass: 'PrintOrderWithDeliveryInfoModal'
        });

        var _createOrEditOrderModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CreateOrEditOrderModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CreateOrEditOrderModal.js',
            modalClass: 'CreateOrEditOrderModal',
            modalSize: 'xxl'
        });

        //init the filter controls
        app.localStorage.getItem('OrdersFilter', function (cachedFilter) {
            if (!cachedFilter) {
                cachedFilter = {
                    startDate: moment().format("MM/DD/YYYY"),
                    endDate: moment().add(1, 'days').format("MM/DD/YYYY"),
                    officeId: abp.session.officeId,
                    officeName: abp.session.officeName
                };
            }

            var dateFilterIsEmpty = false;

            if (!cachedFilter.startDate || cachedFilter.startDate === 'Invalid date') {
                dateFilterIsEmpty = true;
                //still need to init the daterangepicker with real dates first and clear the inputs only after the init.
                cachedFilter.startDate = moment().format("MM/DD/YYYY");
            }

            if (!cachedFilter.endDate || cachedFilter.endDate === 'Invalid date') {
                dateFilterIsEmpty = true;
                cachedFilter.endDate = moment().add(1, 'days').format("MM/DD/YYYY");
            }

            $("#DateStartFilter").val(cachedFilter.startDate);
            $("#DateEndFilter").val(cachedFilter.endDate);
            $("#DateFilter").val($("#DateStartFilter").val() + ' - ' + $("#DateEndFilter").val());

            $("#DateFilter").daterangepicker({
                locale: {
                    cancelLabel: 'Clear'
                }
            }, function (start, end, label) {
                $("#DateStartFilter").val(start.format('MM/DD/YYYY'));
                $("#DateEndFilter").val(end.format('MM/DD/YYYY'));
            });

            $("#DateFilter").on('apply.daterangepicker', function (ev, picker) {
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
                $("#DateStartFilter").val(picker.startDate.format('MM/DD/YYYY'));
                $("#DateEndFilter").val(picker.endDate.format('MM/DD/YYYY'));
                reloadMainGrid();
            });

            $("#DateFilter").on('cancel.daterangepicker', function (ev, picker) {
                $(this).val('');
                $("#DateStartFilter").val('');
                $("#DateEndFilter").val('');
                reloadMainGrid();
            });

            if (dateFilterIsEmpty) {
                $("#DateFilter").val('');
                $("#DateStartFilter").val('');
                $("#DateEndFilter").val('');
            }

            $("#OfficeIdFilter").select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: true
            });
            if (cachedFilter.officeId) {
                abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), cachedFilter.officeId, cachedFilter.officeName);
            }

            $("#CustomerIdFilter").select2Init({
                abpServiceMethod: abp.services.app.customer.getCustomersSelectList,
                showAll: false,
                allowClear: true
            });
            if (cachedFilter.customerId) {
                abp.helper.ui.addAndSetDropdownValue($("#CustomerIdFilter"), cachedFilter.customerId, cachedFilter.customerName);
            }

            $("#ItemIdFilter").select2Init({
                abpServiceMethod: listCacheSelectLists.item(),
                abpServiceParams: {
                    includeInactive: true
                },
                showAll: listCache.item.isEnabled,
                allowClear: true
            });
            if (cachedFilter.itemId) {
                abp.helper.ui.addAndSetDropdownValue($("#ItemIdFilter"), cachedFilter.itemId, cachedFilter.itemName);
            }

            $("#LoadAtIdFilter").select2Init({
                abpServiceMethod: listCacheSelectLists.location(),
                abpServiceParams: {
                    includeInactive: true
                },
                showAll: false,
                allowClear: true
            });
            if (cachedFilter.loadAtId) {
                abp.helper.ui.addAndSetDropdownValue($("#LoadAtIdFilter"), cachedFilter.loadAtId, cachedFilter.loadAt);
            }

            $("#DeliverToIdFilter").select2Init({
                abpServiceMethod: listCacheSelectLists.location(),
                abpServiceParams: {
                    includeInactive: true
                },
                showAll: false,
                allowClear: true
            });
            if (cachedFilter.deliverToId) {
                abp.helper.ui.addAndSetDropdownValue($("#DeliverToIdFilter"), cachedFilter.deliverToId, cachedFilter.deliverTo);
            }

            $("#JobNumberFilter").val(cachedFilter.jobNumber);
            $("#MiscFilter").val(cachedFilter.misc);
            $("#ShowPendingOrdersFilter").prop('checked', !!cachedFilter.showPendingOrders);

            _isFilterReady = true;
            if (_isGridInitialized) {
                reloadMainGrid(null, false);
            }
        });
        var ordersTable = $('#OrdersTable');
        var ordersGrid = ordersTable.DataTableInit({
            stateSave: true,
            stateDuration: 0,
            ajax: function (data, callback, settings) {
                if (!_isGridInitialized) {
                    _isGridInitialized = true;
                }
                if (!_isFilterReady) {
                    callback(_dtHelper.getEmptyResult());
                    return;
                }
                var abpData = _dtHelper.toAbpData(data);
                var filterData = _dtHelper.getFilterData();
                app.localStorage.setItem('OrdersFilter', filterData);
                $.extend(abpData, filterData);
                _orderService.getOrders(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            //order: [[1, 'asc']],
            columns: [
                {
                    data: null,
                    width: '20px',
                    className: 'control responsive',
                    orderable: false,
                    render: function () {
                        return '';
                    }
                },
                {
                    data: 'id',
                    orderable: false,
                    render: function (data, type, full, meta) {
                        var icon = abp.helper.ui.getEmailDeliveryStatusIcon(full.calculatedEmailDeliveryStatus);
                        if (!icon) {
                            return '';
                        }
                        if (_permissions.viewQuotes) {
                            icon.addClass('clickable').addClass('email-delivery-status');
                        }
                        return $("<div>").append(icon).html();
                    },
                    title: ""
                },
                {

                    data: "deliveryDate",
                    render: function (data, type, full, meta) {
                        return _dtHelper.renderUtcDate(full.deliveryDate);
                    },
                    title: "Delivery Date"
                },
                {
                    responsivePriority: 1,
                    data: "officeName",
                    title: "Office",
                    visible: abp.features.getValue('App.AllowMultiOfficeFeature') === "true"
                },
                {
                    data: "customerName",
                    title: "Customer"
                },
                {
                    data: "quoteName",
                    title: "Quote"
                },
                {
                    data: "poNumber",
                    title: "PO #"
                },
                {
                    data: "contactName",
                    title: "Contact"
                },
                {
                    data: "chargeTo",
                    title: "Charge to"
                },
                {
                    data: "codTotal",
                    render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.codTotal); },
                    title: "Total"
                },
                {
                    data: "numberOfTrucks",
                    title: "# of Trucks"
                },
                {
                    data: null,
                    orderable: false,
                    responsivePriority: 2,
                    width: "10px",
                    className: "actions",
                    render: function (data, type, full, meta) {
                        var content = actionMenuHasItems() ?
                            '<div class="dropdown">' +
                            '<button class="btn btn-primary btn-sm btnActions"><i class="fa fa-ellipsis-h"></i></button>'
                            + '</div>' : '<div>&nbsp;&nbsp;&nbsp;</div>';
                        return content;

                    }
                }

            ]
        });

        function reloadMainGrid(callback, resetPaging) {
            resetPaging = resetPaging === undefined ? true : resetPaging;
            ordersGrid.ajax.reload(callback, resetPaging);
        }





        ordersTable.on('click', '.btnEditRow', function () {
            var orderId = _dtHelper.getRowData(this).id;
            _createOrEditOrderModal.open({ id: orderId });
        });

        ordersTable.on('click', '.btnCopyRow', function () {
            var orderId = _dtHelper.getRowData(this).id;
            _copyOrderModal.open({ orderId: orderId });
        });
        ordersTable.on('click', '.btnCreateQuotefromOrderRow', function () {
            var orderId = _dtHelper.getRowData(this).id;
            _createQuoteFromOrderModal.open({ orderId: orderId });
        });
        ordersTable.on('click', '.btnPrintOrderRow', function () {
            var orderId = _dtHelper.getRowData(this).id;
            app.openPopup(abp.appPath + 'app/orders/GetWorkOrderReport?' + 'id=' + orderId);
        });
        ordersTable.on('click', '.btnPrintOrderWithoutPricesRow', function () {
            var orderId = _dtHelper.getRowData(this).id;
            app.openPopup(abp.appPath + 'app/orders/GetWorkOrderReport?' + 'id=' + orderId + '&hidePrices=true');
        });
        ordersTable.on('click', '.btnPrintOrderforBackOfficeRow', function () {
            var orderId = _dtHelper.getRowData(this).id;
            var options = app.order.getBackOfficeReportOptions({ id: orderId });
            app.openPopup(abp.appPath + 'app/orders/GetWorkOrderReport?' + $.param(options));
        });
        ordersTable.on('click', '.btnDeleteRow', async function () {
            var orderId = _dtHelper.getRowData(this).id;
            var data = _dtHelper.getRowData(this);
            if (await abp.message.confirm(
                'Are you sure you want to delete order number ' + orderId + ' for ' + data.customerName + '?'
            )) {
                _orderService.deleteOrder({
                    id: orderId
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        });



        abp.event.on('app.orderModalCopied', function () {
            reloadMainGrid(null, false);
        });

        abp.event.on('app.quoteCreatedFromOrderModal', function () {
            reloadMainGrid(null, false);
        });

        abp.event.on('app.createOrEditOrderModalSaved', function () {
            reloadMainGrid(null, false);
        });

        abp.event.on('app.createOrEditJobModalSaved', function () {
            reloadMainGrid(null, false);
        });

        //$(".filter").change(function () {
        //    reloadMainGrid();
        //});

        $("#CreateNewOrderButton").click(function (e) {
            e.preventDefault();
            _createOrEditOrderModal.open();
        });

        ordersTable.on('click', '.email-delivery-status', function () {
            var orderId = _dtHelper.getRowData(this).id;
            _viewEmailHistoryModal.open({ orderId: orderId });
        });

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            $("#DateStartFilter").val('');
            $("#DateEndFilter").val('');
            reloadMainGrid();
        });

        async function deleteOrder(order) {
            if (await abp.message.confirm(
                'Are you sure you want to delete ' + order.id + ' for ' + order.customerName + '?'
            )) {
                _orderService.deleteOrder({
                    id: order.id
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        }

        ordersTable.on('click', '.btnActions', function (e) {
            e.preventDefault();
            var button = $(this);
            var position = button[0].getBoundingClientRect();
            position.x += $(window).scrollLeft();
            position.y += $(window).scrollTop();
            button.contextMenu({ x: position.x, y: position.y });
        });

        function actionMenuHasItems() {
            return _permissions.edit
                || _permissions.editTickets
                || _permissions.print
                || _permissions.editQuotes;
        }


        $('#OrdersTable tbody tr').contextmenu({ 'target': '#context-menu' });

        ordersTable.contextMenu({
            selector: 'tbody tr',
            zIndex: 103,
            events: {
                show: function (options) {
                    if (!actionMenuHasItems()) {
                        return false;
                    }
                    return true;
                }
            },
            items: {
                edit: {
                    name: "Edit",
                    icon: "fas fa-edit",
                    visible: function () {
                        var order = _dtHelper.getRowData(this);
                        return _permissions.edit;
                    },
                    callback: function () {
                        var orderId = _dtHelper.getRowData(this).id;
                        _createOrEditOrderModal.open({ id: orderId });
                    }
                },
                separator1: _permissions.edit ? "-----" : { visible: false },
                copy: {
                    name: "Copy Order",
                    icon: "fas fa-clone",
                    visible: function () {
                        var order = _dtHelper.getRowData(this);
                        return _permissions.edit; //&& order.officeId === abp.session.officeId;
                    },
                    callback: function () {
                        var order = _dtHelper.getRowData(this);
                        _copyOrderModal.open({
                            orderId: order.id
                        });
                    }
                },
                createQuoteFromOrder: {
                    name: "Create Quote from Order",
                    icon: "fas fa-shopping-cart",
                    visible: function () {
                        return _permissions.editQuotes;
                    },
                    callback: function () {
                        var order = _dtHelper.getRowData(this);
                        _createQuoteFromOrderModal.open({
                            orderId: order.id
                        });
                    }
                },
                separator2: _permissions.edit || _permissions.editQuotes ? "-----" : { visible: false },
                print: {
                    name: "Print Order",
                    icon: "fas fa-print",
                    visible: function () {
                        return _permissions.print;
                    },
                    callback: function () {
                        var orderId = _dtHelper.getRowData(this).id;
                        app.openPopup(abp.appPath + 'app/orders/GetWorkOrderReport?' + 'id=' + orderId);
                    }
                },
                printWithoutPrices: {
                    name: "Print Order without Prices",
                    icon: "fas fa-print",
                    visible: function () {
                        return _permissions.print;
                    },
                    callback: function () {
                        var orderId = _dtHelper.getRowData(this).id;
                        app.openPopup(abp.appPath + 'app/orders/GetWorkOrderReport?' + 'id=' + orderId + '&hidePrices=true');
                    }
                },
                printWithSeparatePrices: {
                    name: "Print Order with Separate Prices",
                    icon: "fas fa-print",
                    visible: function () {
                        return _permissions.print;
                    },
                    callback: function () {
                        var orderId = _dtHelper.getRowData(this).id;
                        var options = app.order.getOrderWithSeparatePricesReportOptions({ id: orderId });
                        app.openPopup(abp.appPath + 'app/orders/GetWorkOrderReport?' + $.param(options));
                    }
                },
                printForBackOffice: {
                    name: "Print Order for Back Office",
                    icon: "fas fa-print",
                    visible: function () {
                        return _permissions.print;
                    },
                    callback: function () {
                        var orderId = _dtHelper.getRowData(this).id;
                        var options = app.order.getBackOfficeReportOptions({ id: orderId });
                        app.openPopup(abp.appPath + 'app/orders/GetWorkOrderReport?' + $.param(options));
                    }
                },
                printWithDeliveryInfo: {
                    name: app.localize('PrintOrderWithDeliveryInfo'),
                    icon: "fas fa-print",
                    visible: function () {
                        return _permissions.print;
                    },
                    callback: function () {
                        var orderId = _dtHelper.getRowData(this).id;
                        _printOrderWithDeliveryInfoModal.open({ id: orderId });
                    }
                },
                separator3: _permissions.print && _permissions.edit ? "-----" : { visible: false },
                "delete": {
                    name: "Delete",
                    icon: "fas fa-trash",
                    visible: function () {
                        return _permissions.edit;
                    },
                    callback: function () {
                        var order = _dtHelper.getRowData(this);
                        deleteOrder(order);
                    }
                }
            }
        });

    });
})();
