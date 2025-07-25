(function ($) {
    app.modals.CallLeaseHaulerContactsModal = function () {

        var _modalManager;
        var _leaseHaulerService = abp.services.app.leaseHauler;
        var _dtHelper = abp.helper.dataTables;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _$form = _modalManager.getModal().find('form');

            var _leaseHaulerId = _$form.find("#Id").val();

            var callLeaseHaulerContactsTable = _modalManager.getModal().find('#CallLeaseHaulerContactsTable');
            var callLeaseHaulerContactsGrid = callLeaseHaulerContactsTable.DataTableInit({

                serverSide: true,
                processing: true,
                ajax: function (data, callback, settings) {
                    if (_leaseHaulerId === '') {
                        callback(_dtHelper.getEmptyResult());
                        return;
                    }
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, { leaseHaulerId: _leaseHaulerId });
                    _leaseHaulerService.getLeaseHaulerContacts(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                paging: false,
                info: false,
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
                        data: "name",
                        title: "Name"
                    },
                    {
                        data: "title",
                        title: "Title"
                    },
                    {
                        data: "phone",
                        render: function (data, type, full, meta) { return _dtHelper.renderPhone(data); },
                        title: "Phone"
                    }
                ]
            });

            _modalManager.getModal().on('shown.bs.modal', function () {
                callLeaseHaulerContactsGrid
                    .columns.adjust()
                    .responsive.recalc();
            });

        };

    };
})(jQuery);