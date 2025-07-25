(function ($) {
    app.modals.ViewHostEmailModal = function () {

        var _modalManager;
        var _hostEmailAppService = abp.services.app.hostEmail;
        var _dtHelper = abp.helper.dataTables;
        var _$form = null;
        var _hostEmailId = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _hostEmailId = _$form.find("#Id").val();

            _$form.find("#Editions").select2Init({
                abpServiceMethod: abp.services.app.edition.getEditionsSelectList,
                showAll: true,
                allowClear: true
            });

            _$form.find("#ActiveFilter").select2Init({
                showAll: true,
                allowClear: true
            });

            _$form.find("#Tenants").select2Init({
                showAll: false,
                allowClear: true
            });

            _$form.find("#Type").select2Init({
                showAll: true,
                allowClear: false
            });

            _$form.find("#Roles").select2Init({
                showAll: true,
                allowClear: true
            });

            var hostEmailReceiversTable = _modalManager.getModal().find('#HostEmailReceiversTable');
            var hostEmailReceiversGrid = hostEmailReceiversTable.DataTableInit({
                serverSide: true,
                info: false,
                ajax: function (data, callback, settings) {
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, { hostEmailId: _hostEmailId });
                    _hostEmailAppService.getHostEmailReceivers(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
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
                        data: "tenantName",
                        title: "Tenant"
                    },
                    {
                        data: "userFullName",
                        title: "User Name"
                    },
                    {
                        data: "emailAddress",
                        title: "Email Address"
                    },
                    {
                        data: "emailDeliveryStatus",
                        render: function (data, type, full, meta) { return _dtHelper.renderText(full.emailDeliveryStatusFormatted); },
                        title: "Status"
                    }
                ]
            });

            _modalManager.getModal().on('shown.bs.modal', function () {
                hostEmailReceiversGrid
                    .columns.adjust()
                    .responsive.recalc();
            });

            var reloadHostEmailReceiversGrid = function () {
                hostEmailReceiversGrid.ajax.reload(null, false);
            };

            _modalManager.getModal().find("#ReloadHostEmailReceiversButton").click(function (e) {
                e.preventDefault();
                reloadHostEmailReceiversGrid();
            });
        };

    };
})(jQuery);
