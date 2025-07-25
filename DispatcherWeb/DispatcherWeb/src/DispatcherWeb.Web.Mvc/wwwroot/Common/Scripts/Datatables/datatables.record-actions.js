/************************************************************************
* RECORD-ACTIONS extension for datatables                               *
*************************************************************************/
(function ($) {

    if (!$.fn.dataTableExt) {
        return;
    }

    var _createDropdownItem = function (record, fieldItem) {
        var $li = $('<li/>');
        var $a = $('<a/>');

        if (fieldItem.text) {
            $a.html(fieldItem.text);
        }

        if (fieldItem.action) {
            $a.click(function (e) {
                e.preventDefault();

                if (!$(this).closest('li').hasClass('disabled')) {
                    fieldItem.action({
                        record: record
                    });
                }
            });
        }

        $a.appendTo($li);
        return $li;
    }

    var _hasPermission = function (field) {
        if (field.permission === undefined || typeof (field.permission) !== 'string' || field.permission === '') {
            return true;
        }
        return abp.auth.isGranted(field.permission);
    }

    var _createButtonDropdown = function (record, field) {
        var $container = $('<div/>')
            .addClass('dropdown')
            .addClass('action-button');

        var $dropdownButton = $('<button/>')
			.html(field.text || '<i class="fa fa-ellipsis-h"></i>')
            .addClass('btn btn-primary btn-sm')
            .attr('data-toggle', 'dropdown')
            .attr('aria-haspopup', 'true')
            .attr('aria-expanded', 'false');

        if (field.cssClass) {
            $dropdownButton.addClass(field.cssClass);
        }

        var $dropdownItemsContainer = $('<ul/>').addClass('dropdown-menu').addClass('dropdown-menu-right');

        for (var i = 0; i < field.items.length; i++) {
            var fieldItem = field.items[i];

            if (!_hasPermission(fieldItem)) {
                continue;
            }
            if (fieldItem.visible && !fieldItem.visible({ record: record })) {
                continue;
            }

            var $dropdownItem = _createDropdownItem(record, fieldItem);

            if (fieldItem.enabled && !fieldItem.enabled({ record: record })) {
                $dropdownItem.addClass('disabled');
            }

            $dropdownItem.appendTo($dropdownItemsContainer);
        }

        if ($dropdownItemsContainer.find('li').length > 0) {
            $dropdownItemsContainer.appendTo($container);
            $dropdownButton.appendTo($container);
        }

        if ($dropdownItemsContainer.children().length === 0) {
            return "";
        }

        return $container;
    };

    var _createSingleButton = function (record, field) {
        $(field.element).data(record);

        if (!_hasPermission(field)) {
            return "";
        }

        if (field.visible === undefined) {
            return field.element;
        }

        var isVisibilityFunction = typeof field.visible === "function";
        if (isVisibilityFunction) {
            if (field.visible()) {
                return field.element;
            }
        } else {
            if (field.visible) {
                return field.element;
            }
        }

        return "";
    };

    var _createRowAction = function (record, field, tableInstance) {
        if (field.items && field.items.length >= 1) {
            return _createButtonDropdown(record, field, tableInstance);
        } else if (field.element) {
            var $singleActionButton = _createSingleButton(record, field);
            if ($singleActionButton != "") {
                return $singleActionButton.clone(true);
            }
        }

        return "";
    }

    var hideColumnWithoutRedraw = function (tableInstance, colIndex) {
        tableInstance.fnSetColumnVis(colIndex, false, false);
    }

    var hideEmptyColumn = function(cellContent, tableInstance, colIndex) {
        if (cellContent == "") {
            hideColumnWithoutRedraw(tableInstance, colIndex);
        }
    };

    var renderRowActions = function (tableInstance, nRow, aData, iDisplayIndex, iDisplayIndexFull) {
        var columns = null;
        if (tableInstance.aoColumns) {
            columns = tableInstance.aoColumns;
		} else {
			if (tableInstance.fnSettings()) {
				columns = tableInstance.fnSettings().aoColumns;
			}
        }

        if (!columns) {
            return;
        }

        for (var colIndex = 0; colIndex < columns.length; colIndex++) {
            var column = columns[colIndex];
            if (column.rowAction) {
                var $actionContainer = _createRowAction(aData, column.rowAction, tableInstance);
                hideEmptyColumn($actionContainer, tableInstance, colIndex);

                var cell = tableInstance.api().cell(nRow, colIndex).node();
                var $actionButton = $(cell).find(".action-button");
                if ($actionButton.length === 0) {
                    $(cell).append($actionContainer);
                }
            }
        }
    };

    var _existingApiRenderRowActionsFunction = $.fn.dataTableExt.oApi.renderRowActions;
    $.fn.dataTableExt.oApi.renderRowActions = function (tableInstance, nRow, aData, iDisplayIndex, iDisplayIndexFull) {
        if (_existingApiRenderRowActionsFunction) {
            _existingApiRenderRowActionsFunction(tableInstance, nRow, aData, iDisplayIndex, iDisplayIndexFull);
        }

        renderRowActions(tableInstance, nRow, aData, iDisplayIndex, iDisplayIndexFull);
    };

    if (!$.fn.dataTable) {
        return;
    }

    var _existingDefaultFnRowCallback = $.fn.dataTable.defaults.fnRowCallback;
    $.extend(true, $.fn.dataTable.defaults, {
        fnRowCallback: function (nRow, aData, iDisplayIndex, iDisplayIndexFull) {
            if (_existingDefaultFnRowCallback) {
                _existingDefaultFnRowCallback(this, nRow, aData, iDisplayIndex, iDisplayIndexFull);
            }

            renderRowActions(this, nRow, aData, iDisplayIndex, iDisplayIndexFull);
        }
    });

})(jQuery);
