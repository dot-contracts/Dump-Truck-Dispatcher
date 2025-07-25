(function () {
    $(function () {

        var _quoteHistoryService = abp.services.app.quoteHistory;
        var _dtHelper = abp.helper.dataTables;

        var _viewQuoteHistoryModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/QuoteHistory/ViewQuoteHistoryModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/QuoteHistory/_ViewQuoteHistoryModal.js',
            modalClass: 'ViewQuoteHistoryModal',
            modalSize: 'lg'
        });

        var today = moment().startOf('day');
        var yesterday = moment().add(-1, 'days').startOf('day');
        $("#StartDateFilter").val(yesterday.format("MM/DD/YYYY"));
        $("#EndDateFilter").val(today.format("MM/DD/YYYY"));
        $("#DateFilter").val(yesterday.format("MM/DD/YYYY") + ' - ' + today.format("MM/DD/YYYY"));

        $("#DateFilter").daterangepicker({
            locale: {
                cancelLabel: 'Clear'
            }
        }, function (start, end, label) {
            $("#StartDateFilter").val(start.clone().tz('UTC').format());
            $("#EndDateFilter").val(end.clone().tz('UTC').format());
        });

        $("#DateFilter").on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            $("#StartDateFilter").val(picker.startDate.format("MM/DD/YYYY"));
            $("#EndDateFilter").val(picker.endDate.format("MM/DD/YYYY"));
            reloadMainGrid();
        });

        $("#DateFilter").on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
            $("#StartDateFilter").val('');
            $("#EndDateFilter").val('');
            reloadMainGrid();
        });

        $("#CustomerIdFilter").select2Init({
            abpServiceMethod: abp.services.app.customer.getCustomersSelectList,
            showAll: true
        });

        var quoteChildDropdown = abp.helper.ui.initChildDropdown({
            parentDropdown: $("#CustomerIdFilter"),
            childDropdown: $("#QuoteIdFilter"),
            abpServiceMethod: abp.services.app.quote.getQuotesForCustomer
        });

        $("#QuoteIdFilter").select2Init({
            allowClear: false,
            showAll: true
        });

        var quoteHistoryTable = $('#QuoteHistoryTable');
        var quoteHistoryGrid = quoteHistoryTable.DataTableInit({

            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _quoteHistoryService.getQuoteHistory(abpData).done(function (abpResult) {
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
                    data: "quoteName",
                    title: "Quote"
                },
                {
                    data: "dateTime",
                    render: function (data, type, full, meta) { return _dtHelper.renderUtcDateTime(full.dateTime); },
                    title: "When changed"
                },
                {
                    data: "changedByUserName",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.changedByUserName); },
                    title: "Changed by"
                },
                {
                    data: "changeType",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.changeTypeName); },
                    title: "Type of change"
                },
                {
                    data: null,
                    orderable: false,
                    name: "Actions",
                    width: "10px",
                    responsivePriority: 1,
                    className: "actions",
                    defaultContent: '<div class="dropdown action-button">'
                        + '<ul class="dropdown-menu dropdown-menu-right">'
                        + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Details</a></li>'
                        + '</ul>'
                        + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                        + '</div>'
                }
            ],
            order: [[1, "desc"]]
        });

        function reloadMainGrid() {
            quoteHistoryGrid.ajax.reload();
        }

        quoteHistoryTable.on('click', '.btnEditRow', function () {
            var quoteHistoryId = _dtHelper.getRowData(this).id;
            _viewQuoteHistoryModal.open({ id: quoteHistoryId });
        });

        if ($("#QuoteHistoryRecordId").val() !== '') {
            _viewQuoteHistoryModal.open({ id: $("#QuoteHistoryRecordId").val() });
        }

        abp.event.on('app.viewQuoteHistoryModalClosed', function () {
            if ($("#QuoteHistoryRecordId").val() !== '') {
                $("#QuoteHistoryRecordId").val('');
                window.history.replaceState({}, '', abp.appPath + 'app/QuoteHistory/');
            }
        });

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $('#StartDateFilter').val('');
            $('#EndDateFilter').val('');
            $(".filter").change();
            reloadMainGrid();
        });

    });
})();