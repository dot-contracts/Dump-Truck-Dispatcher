(function () {
    $(function () {
        var _userLoginService = abp.services.app.userLogin;
        var _dtHelper = abp.helper.dataTables;

        $("#LoginResultFilter").select2Init({
            showAll: true,
            allowClear: false
        });

        $("#DateRangeFilter").daterangepicker({
            autoUpdateInput: false,
            locale: {
                cancelLabel: 'Clear'
            }
        }, function (start, end, label) {
            $("#DateRangeStartFilter").val(start.format('MM/DD/YYYY'));
            $("#DateRangeEndFilter").val(end.format('MM/DD/YYYY'));
        });

        $("#DateRangeFilter").on('blur', function () {
            var startDate = $("#DateRangeStartFilter").val();
            var endDate = $("#DateRangeEndFilter").val();
            $(this).val(startDate && endDate ? startDate + ' - ' + endDate : '');
        });

        $("#DateRangeFilter").on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            $("#DateRangeStartFilter").val(picker.startDate.format('MM/DD/YYYY'));
            $("#DateRangeEndFilter").val(picker.endDate.format('MM/DD/YYYY'));
            reloadMainGrid();
        });

        $("#DateRangeFilter").on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
            $("#DateRangeStartFilter").val('');
            $("#DateRangeEndFilter").val('');
            reloadMainGrid();
        });

        var loginAttemptsTable = $('#LoginAttemptsTable');
        var loginAttemptsGrid = loginAttemptsTable.DataTableInit({
            drawCallback: function (settings) {
                $('[data-toggle=tooltip]').tooltip();
            },
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _userLoginService.getUserLoginAttempts(abpData).done(function (abpResult) {
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
                    data: 'clientIpAddress',
                    title: app.localize('IpAddress')
                },
                {
                    data: 'clientName',
                    title: app.localize('Client')
                },
                {
                    data: 'browserInfo',
                    title: app.localize('Browser')
                },
                {
                    data: 'creationTime',
                    render: function (creationTime) {
                        return moment(creationTime).format('YYYY-MM-DD HH:mm:ss');
                    },
                    title: app.localize('Time')
                },
                {
                    data: 'result',
                    render: function (result) {
                        if (result === 'Success') {
                            return '<span class="text-success">' + app.localize('AbpLoginResultType_' + result) + '</span>';
                        }

                        return '<span class="text-warning">' + app.localize('AbpLoginResultType_' + result) + '</span>';
                    },
                    title: app.localize('Result')
                },
            ],
        });

        function reloadMainGrid() {
            loginAttemptsGrid.ajax.reload();
        }

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            $("#DateRangeStartFilter").val('');
            $("#DateRangeEndFilter").val('');
            reloadMainGrid();
        });
    });
})();
