(function () {
    $(function () {
        let _$leaseHaulerPerformanceTable = $('#LeaseHaulerPerformanceTable');
        let _leaseHaulerPerformanceService = abp.services.app.leaseHaulerPerformance;
        let _dtHelper = abp.helper.dataTables;

        let yesterday = moment().startOf('day').add(-1, 'days').format("MM/DD/YYYY");
        let _filter = {
            startDate: yesterday,
            endDate: yesterday,
        };

        $("#DateRangeFilter").val(_filter.startDate + ' - ' + _filter.endDate);
        $("#DateRangeFilter").daterangepicker({
            locale: {
                cancelLabel: 'Clear'
            },
            showDropDown: true,
            autoUpdateInput: false
        }).on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            _filter.startDate = picker.startDate.format('MM/DD/YYYY');
            _filter.endDate = picker.endDate.format('MM/DD/YYYY');
            reloadMainGrid();
        }).on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
            _filter.startDate = '';
            _filter.endDate = '';
        });

        let leaseHaulerPerformanceGrid = _$leaseHaulerPerformanceTable.DataTableInit({
            paging: false,
            serverSide: true,
            processing: true,
            listAction: {
                ajaxFunction: _leaseHaulerPerformanceService.getLeaseHaulerPerformances,
                inputFilter: () => ({
                    ..._filter,
                })
            },
            footerCallback: function (row, data, start, end, display) {
                let api = this.api();

                $(api.column(1).footer()).text('Total');
                let totalColumns = [
                    'completed',
                    'canceled',
                    'total',
                    'percentComplete',
                ];
                for (let columnName of totalColumns) {
                    let totalValue = data.map(x => x[columnName] || 0).reduce((a, b) => a + b, 0);
                    let footerCell = api.column(columnName + ':name').footer();
                    if (columnName === 'percentComplete') {
                        totalValue = totalValue.toFixed(2) + '%';
                    }
                    $(footerCell).text(totalValue);
                }
            },
            columns: [
                {
                    width: '20px',
                    className: 'control responsive',
                    orderable: false,
                    render: function () {
                        return '';
                    },
                },
                {
                    data: "leaseHaulerName",
                    name: "leaseHaulerName",
                    title: app.localize("LeaseHauler"),
                },
                {
                    data: "completed",
                    name: "completed",
                    title: app.localize("Completed"),
                },
                {
                    data: "canceled",
                    name: "canceled",
                    title: app.localize("Canceled"),
                },
                {
                    data: "total",
                    name: "total",
                    title: app.localize("Total"),
                },
                {
                    data: "percentComplete",
                    name: "percentComplete",
                    title: app.localize("PercentComplete"),
                    render: function(data) {
                        return data.toFixed(2) + '%';
                    },
                },
            ],
        });

        function reloadMainGrid() {
            leaseHaulerPerformanceGrid.ajax.reload();
        }

        $('#LeaseHaulerPerformanceFilterForm').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $('#ClearSearchButton').click(function () {
            $('#LeaseHaulerPerformanceFilterForm')[0].reset();
            _filter.startDate = '';
            _filter.endDate = '';
            reloadMainGrid();
        });
    });
})();
