(function () {
    var dataMerge = function () {
        var mergeButtonClassName = "data-merge-button";
        var rowSelectionClassName = "data-merge-row-selection";
        var mergeButtonClass = '.' + mergeButtonClassName;
        var rowSelectionClass = '.' + rowSelectionClassName;
        var mergeRecordCountLimit = 5;

        function addColumn(options) {
            options.columns.push({
                data: null,
                orderable: false,
                render: function (data, type, full, meta) {
                    if (full.disallowDataMerge === true) {
                        return "";
                    }
                    return '<label class="m-checkbox"><input type="checkbox" class="minimal ' + rowSelectionClassName + '"><span></span></label>';
                },
                className: "checkmark text-center",
                width: "50px",
                title: " ",
                responsivePriority: 3,
                responsiveDispalyInHeaderOnly: true
            });
            var oldHeaderCallback = options.headerCallback;
            options.headerCallback = function (thead, data, start, end, display) {
                var headerCell = $(thead).find('th').eq(options.columns.length - 1).html('');
                headerCell.append($('<button type="button" class="btn btn-default btn-sm" disabled>Merge</button>').addClass(mergeButtonClassName));
                if (oldHeaderCallback)
                    oldHeaderCallback(thead, data, start, end, display);
            };
        }

        function handleColumn(table, grid, options) {
            table.on('change', rowSelectionClass, function () {
                if ($(this).is(":checked")) {
                    if (table.find(rowSelectionClass + ':checked').length > mergeRecordCountLimit) {
                        $(this).prop('checked', false).change();
                        abp.notify.warn(`You can only merge ${mergeRecordCountLimit} ${(options.dataMergeOptions.entitiesName || 'records')} at a time. Please change your selection.`);
                    }
                    let validate = options.dataMergeOptions.newSelectionValidation || function () { return true; };
                    let newRow = abp.helper.dataTables.getRowData($(this));
                    let selectedRows = table.find(rowSelectionClass + ':checked').not($(this))
                        .toArray()
                        .map(x => abp.helper.dataTables.getRowData(x));

                    let isValidRow = validate(newRow, selectedRows);
                    $(this).prop('checked', isValidRow);
                }
                table.find(mergeButtonClass).prop('disabled', table.find(rowSelectionClass + ':checked').length < 2);
            });

            var mergeModal = app.modals.MergeModal.create({
                dropdownServiceMethod: options.dataMergeOptions.dropdownServiceMethod,
                mergeServiceMethod: options.dataMergeOptions.mergeServiceMethod
            });

            table.on('click', mergeButtonClass, function (e) {
                e.preventDefault();
                var ids = [];
                table.find(rowSelectionClass + ':checked').each(function () {
                    ids.push(abp.helper.dataTables.getRowData(this).id);
                });
                mergeModal.open({
                    idsToMerge: ids.join(),
                    description: options.dataMergeOptions.description
                });
            });

            abp.event.on('app.mergeModalFinished', function () {
                grid.ajax.reload();
            });
        }

        return {
            addColumn: addColumn,
            handleColumn: handleColumn
        };
    }();
    abp.helperConfiguration.dataTables.beforeInit.push((options) => {
        if (options.dataMergeOptions && options.dataMergeOptions.enabled && app.modals.MergeModal) {
            dataMerge.addColumn(options);
        }
    });
    abp.helperConfiguration.dataTables.afterInit.push((table, grid, options) => {
        if (options.dataMergeOptions && options.dataMergeOptions.enabled && app.modals.MergeModal) {
            dataMerge.handleColumn(table, grid, options);
        }
    });

    var massDelete = function () {
        function handleColumn(table, grid, options) {
            options.selectionColumnOptions.selectionChangedCallbacks.push(function (selectedRowsCount) {
                $(options.massDeleteOptions.deleteButton).prop('disabled', selectedRowsCount < 1);
            });

            $(options.massDeleteOptions.deleteButton).click(async function (e) {
                e.preventDefault();
                var ids = options.selectionColumnOptions.getSelectedRowsIds();
                if (ids.length === 0) {
                    return;
                }
                if (await abp.message.confirm(
                    'Are you sure you want to delete all selected items?'
                )) {
                    options.massDeleteOptions.deleteServiceMethod({
                        ids: ids
                    }).done(function () {
                        abp.notify.info('Successfully deleted.');
                        grid.ajax.reload();
                    });
                }
            });
        }

        return {
            handleColumn: handleColumn
        };
    }();
    abp.helperConfiguration.dataTables.beforeInit.push((options) => {
        if (options.massDeleteOptions && options.massDeleteOptions.enabled) {
            options.selectionColumnOptions = options.selectionColumnOptions || {};
        }
    });
    abp.helperConfiguration.dataTables.afterInit.push((table, grid, options) => {
        if (options.massDeleteOptions && options.massDeleteOptions.enabled) {
            massDelete.handleColumn(table, grid, options);
        }
    });

    abp.helperConfiguration.dataTables.beforeInit.push((options, table) => {
        var lastRequestedPageSize;
        table.on("draw.dt", () => {
            var pageSizeSelector = findPageSizeDropdownForTable(table);

            if (lastRequestedPageSize && parseInt(pageSizeSelector.val()) !== lastRequestedPageSize) {
                pageSizeSelector.val(lastRequestedPageSize).trigger('change.select2');
            }

            if (!pageSizeSelector.data('select2')) {
                pageSizeSelector.select2Init({
                    showAll: true,
                    allowClear: false
                });
            } else {
                pageSizeSelector.trigger('change.select2');
            }
        });

        table.on('preXhr.dt', (e, settings, data) => {
            if (data && data.length > 0) {
                lastRequestedPageSize = data.length;
            }
        });
    });

    abp.helperConfiguration.dataTables.beforeInit.push((options, table) => {
        table.on('preXhr.dt', () => abp.ui.setBusy(table.closest('.dataTables_wrapper')));
        table.on('xhr.dt', () => abp.ui.clearBusy(table.closest('.dataTables_wrapper')));
    });

    abp.helper ??= {};
    abp.helper.dataTables ??= {};
    abp.helper.dataTables.renderMileage = function(value) {
        if (value !== null && value !== undefined && !isNaN(value)) {
            return Number(value).toLocaleString('en-US');
        }
        return '';
    }

    function findPageSizeDropdownForTable(table) {
        return $(table).closest('.dataTables_wrapper').find('div.dataTables_length select');
    }
})();
