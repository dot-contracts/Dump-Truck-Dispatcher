(function () {
    $(function () {

        var _customerNotificationService = abp.services.app.customerNotification;
        var _dtHelper = abp.helper.dataTables;

        var _createOrEditCustomerNotificationModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/CustomerNotifications/CreateOrEditCustomerNotificationModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/CustomerNotifications/_CreateOrEditCustomerNotificationModal.js',
            modalClass: 'CreateOrEditCustomerNotificationModal'
        });

        initFilterControls();

        var customerNotificationsTable = $('#CustomerNotificationsTable');
        var customerNotificationsGrid = customerNotificationsTable.DataTableInit({
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _customerNotificationService.getCustomerNotifications(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            order: [[1, 'asc']],
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
                    data: "createdByUserFullName",
                    title: "Created by"
                },
                {
                    data: "startDate",
                    title: "Start Date",
                    render: function (data, type, full, meta) {
                        return _dtHelper.renderUtcDate(full.startDate);
                    },
                },
                {
                    data: "endDate",
                    title: "End Date",
                    render: function (data, type, full, meta) {
                        return _dtHelper.renderUtcDate(full.endDate);
                    },
                },
                {
                    data: "title",
                    title: "Title"
                },
                {
                    data: "body",
                    title: "Body"
                },
                {
                    data: "editionNames",
                    title: "Editions",
                    orderable: false,
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.editionNamesFormatted); }
                },
                {
                    data: "type",
                    title: "Type",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.typeFormatted); }
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    width: "10px",
                    responsivePriority: 2,
                    defaultContent: '<div class="dropdown action-button">'
                        + '<ul class="dropdown-menu dropdown-menu-right">'
                        + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                        + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                        + '</ul>'
                        + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                        + '</div>'

                }
            ]
        });
        
        function initFilterControls() {
            $("#EditionIdFilter").select2Init({
                abpServiceMethod: abp.services.app.edition.getEditionsSelectList,
                showAll: true,
                allowClear: true,
            });

            $("#TypeFilter").select2Init({
                showAll: true,
                allowClear: true,
            });

            $("#TenantIdFilter").select2Init({
                abpServiceMethod: abp.services.app.tenant.getTenantsSelectList,
                showAll: false,
                allowClear: true
            });

            $("#CreatedByUserIdFilter").select2Init({
                abpServiceMethod: abp.services.app.user.getUsersSelectList,
                showAll: false,
                allowClear: true
            });
        }
        
        var reloadMainGrid = function () {
            customerNotificationsGrid.ajax.reload();
        };
        
        abp.event.on('app.createCustomerNotificationModalSaved', function () {
            reloadMainGrid();
        });

        customerNotificationsTable.on('click', '.btnEditRow', function () {
            var id = _dtHelper.getRowData(this).id;
            _createOrEditCustomerNotificationModal.open({ id: id });
        });

        customerNotificationsTable.on('click', '.btnDeleteRow', async function () {
            var id = _dtHelper.getRowData(this).id;

            if (await abp.message.confirm(
                'Are you sure you want to delete the customer notification?'
            )) {
                _customerNotificationService.deleteCustomerNotification({
                    id: id
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        });

        $("#CreateNewNotificationButton").click(function (e) {
            e.preventDefault();
            _createOrEditCustomerNotificationModal.open();
        });

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            reloadMainGrid();
        });

    });
})();
