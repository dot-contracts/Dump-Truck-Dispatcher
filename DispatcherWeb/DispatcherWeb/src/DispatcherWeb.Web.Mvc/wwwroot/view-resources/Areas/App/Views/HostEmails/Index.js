(function () {
    $(function () {

        var _hostEmailService = abp.services.app.hostEmail;
        var _dtHelper = abp.helper.dataTables;

        var _sendHostEmailModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/HostEmails/SendHostEmailModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/HostEmails/_SendHostEmailModal.js',
            modalClass: 'SendHostEmailModal'
        });

        var _viewHostEmailModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/HostEmails/ViewHostEmailModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/HostEmails/_ViewHostEmailModal.js',
            modalClass: 'ViewHostEmailModal',
            modalSize: 'lg'
        });

        initFilterControls();

        var hostEmailsTable = $('#HostEmailsTable');
        var hostEmailsGrid = hostEmailsTable.DataTableInit({
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                $.extend(abpData, _dtHelper.getDateRangeObject(abpData.date, 'dateBegin', 'dateEnd'));
                _hostEmailService.getHostEmails(abpData).done(function (abpResult) {
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
                    data: "sentAtDateTime",
                    title: "Sent at",
                    render: function (data, type, full, meta) { return _dtHelper.renderDateTime(data); }
                },
                {
                    data: "sentEmailCount",
                    orderable: false,
                    title: "Sent",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(data + (full.processedAtDateTime ? '' : '*')); }
                },
                {
                    data: "deliveredEmailCount",
                    orderable: false,
                    title: "Delivered"
                },
                {
                    data: "openedEmailCount",
                    orderable: false,
                    title: "Opened"
                },
                {
                    data: "failedEmailCount",
                    orderable: false,
                    title: "Failed"
                },
                {
                    data: "type",
                    title: "Type",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.typeFormatted); }
                },
                {
                    data: "subject",
                    title: "Subject"
                },
                {
                    data: "body",
                    title: "Body"
                },
                {
                    data: "processedAtDateTime",
                    title: "Finished processing at",
                    render: function (data, type, full, meta) { return _dtHelper.renderDateTime(data); }
                },
                {
                    data: "sentByUserFullName",
                    title: "Sent by"
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    width: "10px",
                    responsivePriority: 2,
                    defaultContent: '<div class="dropdown action-button">'
                        + '<ul class="dropdown-menu dropdown-menu-right">'
                        + '<li><a class="btnViewRow" title="View"><i class="fa fa-eye"></i> View</a></li>'
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

            $("#DateFilter").daterangepicker({
                locale: {
                    cancelLabel: 'Clear'
                }
            }).on('apply.daterangepicker', function (ev, picker) {
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            }).on('cancel.daterangepicker', function (ev, picker) {
                $(this).val('');
            });

            $("#TenantIdFilter").select2Init({
                abpServiceMethod: abp.services.app.tenant.getTenantsSelectList,
                showAll: false,
                allowClear: true
            });

            $("#SentByUserIdFilter").select2Init({
                abpServiceMethod: abp.services.app.user.getUsersSelectList,
                showAll: false,
                allowClear: true
            });
        }

        var reloadMainGrid = function () {
            hostEmailsGrid.ajax.reload();
        };

        abp.event.on('app.sendHostEmailModalSaved', function () {
            reloadMainGrid();
        });

        hostEmailsTable.on('click', '.btnViewRow', function () {
            var id = _dtHelper.getRowData(this).id;
            _viewHostEmailModal.open({ id: id });
        });


        $("#SendNewHostEmailButton").click(function (e) {
            e.preventDefault();
            _sendHostEmailModal.open();
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
