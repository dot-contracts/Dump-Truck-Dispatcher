(function () {
    app.modals.EntityTypeHistoryModal = function () {

        var _modalManager;
        var _dataTable;
        var _$table;
        var _args;

        var _auditLogService = abp.services.app.auditLog;

        var _options = {
            serviceMethod: null, //Required
            title: app.localize('SelectAnItem'),
            loadOnStartup: true,
            showFilter: true,
            filterText: '',
            pageSize: app.consts.grid.defaultPageSize,
            canSelect: function (item) {
                /* This method can return boolean or a promise which returns boolean.
                 * A false value is used to prevent selection.
                 */
                return true;
            }
        };

        function refreshTable() {
            _dataTable.ajax.reload();
        };

        function showEntityChangeDetails(entityChange) {
            var entityChangeDetailModal = new app.ModalManager({
                viewUrl: abp.appPath + 'App/AuditLogs/EntityChangeDetailModal',
                modalClass: 'EntityChangeDetailModal'
            });

            entityChangeDetailModal.open({ entityChangeListDto: entityChange });
        }


        this.init = function (modalManager) {
            _modalManager = modalManager;

            _options = $.extend(_options, _modalManager.getOptions().lookupOptions);
            _$table = _modalManager.getModal().find('.entity-type-history-table');
            _args = _modalManager.getArgs();

            _dataTable = _$table.DataTableInit({
                paging: true,
                serverSide: true,
                processing: true,
                lengthChange: false,
                pageLength: _options.pageSize,
                deferLoading: _options.loadOnStartup ? null : 0,
                listAction: {
                    ajaxFunction: _auditLogService.getEntityTypeChanges,
                    inputFilter: function () {
                        return {
                            entityTypeFullName: _args.entityTypeFullName,
                            entityId: _args.entityId
                        };
                    }
                },
                columns: [
                    {
                        data: null,
                        orderable: false,
                        defaultContent: '',
                        rowAction: {
                            element: $("<div/>")
                                .addClass("text-center")
                                .append($("<button/>")
                                    .addClass("btn m-btn m-btn--hover-accent m-btn--icon m-btn--icon-only m-btn--pill")
                                    .attr("title", app.localize("EntityChangeDetail"))
                                    .append($("<i/>").addClass("fa fa-search"))
                                ).click(function () {
                                    showEntityChangeDetails($(this).data());
                                })
                        }
                    },
                    {
                        data: "changeTypeName",
                        orderable: false,
                        render: function (changeTypeName) {
                            return app.localize(changeTypeName);
                        }
                    },
                    {
                        data: "userName"
                    },
                    {
                        data: "changeTime",
                        render: function (changeTime) {
                            return moment(changeTime).format('L LT');
                        }
                    }
                ]
            });

            refreshTable();
        };
    };

    app.modals.EntityTypeHistoryModal.create = function () {
        return new app.ModalManager({
            viewUrl: abp.appPath + 'App/Common/EntityTypeHistoryModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Common/Modals/_EntityTypeHistoryModal.js',
            modalClass: 'EntityTypeHistoryModal'
        });
    };
})();
