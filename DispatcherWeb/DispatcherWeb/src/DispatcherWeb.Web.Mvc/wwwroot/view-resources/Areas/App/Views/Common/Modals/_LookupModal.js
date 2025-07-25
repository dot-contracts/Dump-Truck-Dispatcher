(function () {
    app.modals.LookupModal = function () {

        var _modalManager;
        var _dataTable;
        var _$table;
        var _$filterInput;

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
        }

        function selectItem(item) {
            var boolOrPromise = _options.canSelect(item);
            if (!boolOrPromise) {
                return;
            }

            if (boolOrPromise === true) {
                _modalManager.setResult(item);
                _modalManager.close();
                return;
            }

            //assume as promise
            boolOrPromise.then(function (result) {
                if (result) {
                    _modalManager.setResult(item);
                    _modalManager.close();
                }
            });
        }

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _options = $.extend(_options, _modalManager.getOptions().lookupOptions);

            _$filterInput = _modalManager.getModal().find('.lookup-filter-text');
            _$filterInput.val(_options.filterText);

            _$table = _modalManager.getModal().find('.lookup-modal-table');

            _dataTable = _$table.DataTableInit({
                paging: true,
                responsive: true,
                serverSide: true,
                processing: true,
                pageLength: _options.pageSize,
                deferLoading: _options.loadOnStartup ? null : 0,
                listAction: {
                    ajaxFunction: _options.serviceMethod,
                    inputFilter: function () {
                        return $.extend({
                            filter: _$filterInput.val()
                        }, _modalManager.getArgs().extraFilters);
                    }
                },
                columns: [
                    {
                        data: null,
                        orderable: false,
                        defaultContent: '',
                        className: 'text-center',
                        rowAction: {
                            element: $("<button/>")
                                .addClass("btn btn-metal m-btn m-btn--icon m-btn--icon-only")
                                .attr("title", app.localize('Select'))
                                .append($("<i/>").addClass("fa-regular fa-chevron-circle-right")).click(function () {
                                    var record = $(this).data();
                                    selectItem(record);
                                })
                        }
                    },
                    {
                        data: "name",
                        className: "all"
                    }
                ]
            });

            _modalManager.getModal()
                .find('.lookup-filter-button')
                .click(function (e) {
                    e.preventDefault();
                    refreshTable();
                });

            _modalManager.getModal()
                .find('.modal-body')
                .keydown(function (e) {
                    if (e.which === 13) {
                        e.preventDefault();
                        refreshTable();
                    }
                });
        };
    };

    app.modals.LookupModal.create = function (lookupOptions) {
        return new app.ModalManager({
            viewUrl: abp.appPath + 'App/Common/LookupModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Common/Modals/_LookupModal.js',
            modalClass: 'LookupModal',
            lookupOptions: lookupOptions
        });
    };
})();
