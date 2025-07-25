(function () {
    $(function () {

        var _driverService = abp.services.app.driver;
        var _dtHelper = abp.helper.dataTables;
        var _dtHelperCommon = abp.helper.dataTables;
        var _fulcrumAppService = abp.services.app.fulcrum;

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Drivers/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Drivers/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditDriverModal',
            //modalSize: 'lg'
        });

        $("#OfficeIdFilter").select2Init({
            abpServiceMethod: listCacheSelectLists.office(),
            showAll: true,
            allowClear: true
        });

        $("#StatusFilter").select2Init({
            showAll: true,
            allowClear: false
        });

        var driversTable = $('#DriversTable');
        var driversGrid = driversTable.DataTableInit({
            serverSide: true,
            processing: true,
            //listAction: {
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _driverService.getDrivers(abpData).done(function (abpResult) {
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
                    responsivePriority: 1,
                    data: "firstName",
                    title: "First Name"
                },
                {
                    responsivePriority: 3,
                    data: "lastName",
                    title: "Last Name"
                },
                {
                    data: "officeName",
                    title: "Office",
                    visible: abp.features.getValue('App.AllowMultiOfficeFeature') === "true"
                },
                {
                    data: "dateOfHire",
                    title: "Date of Hire",
                    render: function (data, type, full, meta) {
                        return data ? _dtHelper.renderUtcDate(data) : '';
                    },
                    width: "100px",
                },
                {
                    data: "isInactive",
                    render: function (data, type, full, meta) {
                        //return full.isInactive;
                        var checked = full.isInactive === true ? "checked" : "";
                        return '<label class="m-checkbox"><input type="checkbox" disabled name="IsDefault" ' + checked + ' value="true"><span></span></label>';

                    },
                    className: "checkmark",
                    width: "100px",
                    title: "Inactive"
                },
                {
                    data: "licenseExpirationDate",
                    render: function (data, type, full, meta) {
                        return getDateStatusIcon(data);
                    },
                    className: "date-status",
                    width: "25px",
                    title: '<span title="License Due">L</span>'
                },
                {
                    data: "nextPhysicalDueDate",
                    render: function (data, type, full, meta) {
                        return getDateStatusIcon(data);
                    },
                    className: "date-status",
                    width: "25px",
                    title: '<span title="Physical Due">P</span>'
                },
                {
                    data: "nextMvrDueDate",
                    render: function (data, type, full, meta) {
                        return getDateStatusIcon(data);
                    },
                    className: "date-status",
                    width: "25px",
                    title: '<span title="MVR Due">M</span>'
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    width: "10px",
                    responsivePriority: 2,
                    render: function (data, type, full, meta) {
                        return '<div class="dropdown">'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '<ul class="dropdown-menu dropdown-menu-right">'
                            + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                            + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                            + '</ul>'
                            + '</div>';
                    }
                }
            ]
        });

        function reloadMainGrid(callback, resetPaging) {
            resetPaging = resetPaging === undefined ? true : resetPaging;
            driversGrid.ajax.reload(callback, resetPaging);
        }

        function getDateStatusIcon(data) {
            if (!data) {
                return '';
            }
            var date = moment(data, 'YYYY-MM-DDTHH:mm:ss');
            var today = moment().startOf('day');
            var color;
            if (today >= date) {
                color = 'red';
            } else if (today.add(30, 'd') >= date) {
                color = 'yellow';
            } else {
                color = 'green';
            }
            return '<span><i class="fa fa-circle ' + color + '"></i></span>';
        }

        driversTable.on('click', '.btnEditRow', function () {
            var record = _dtHelper.getRowData(this);
            _createOrEditModal.open({ id: record.id });
        });

        driversTable.on('click', '.btnDeleteRow', function (e) {
            e.preventDefault();
            var record = _dtHelper.getRowData(this);
            deleteDriver(record);
        });

        abp.event.on('app.createOrEditDriverModalSaved', function () {
            reloadMainGrid(null, false);
        });

        $("#CreateNewDriverButton").click(function (e) {
            e.preventDefault();
            _createOrEditModal.open();
        });

        async function deleteDriver(record) {
            if (await abp.message.confirm(
                'Are you sure you want to delete the driver?'
            )) {
                _driverService.deleteDriver({
                    id: record.id
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        }

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            reloadMainGrid();
        });

        $('#ExportDriversToCsvButton').click(function () {
            var $button = $(this);
            var abpData = {};
            $.extend(abpData, _dtHelper.getFilterData());
            abp.ui.setBusy($button);
            _driverService
                .getDriversToCsv(abpData)
                .done(function (result) {
                    app.downloadTempFile(result);
                }).always(function () {
                    abp.ui.clearBusy($button);
                });
        });

        $("#SyncWithFulcrumButton").click(function (e) {
            var $button = $(this);
            abp.ui.setBusy($button);

            e.preventDefault();
            _fulcrumAppService
                .scheduleSyncFulcrumDrivers()
                .always(function () {
                    abp.ui.clearBusy($button);
                });
        });

    });
})();


