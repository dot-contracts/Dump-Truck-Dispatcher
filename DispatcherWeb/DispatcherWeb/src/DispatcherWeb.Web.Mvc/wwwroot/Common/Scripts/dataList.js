/// <reference path="../../../node_modules/@dumptruckdispatcher/datatables-helper/typings/moment.d.ts" />
(function () {
    class RowModelState {
        originalData;
        workingData;
        constructor(initialData) {
            // Deep clone the initial data to prevent reference issues
            this.originalData = $.extend(true, {}, initialData);
            this.workingData = initialData;
        }
        isDirty() {
            return !Object.keys(this.originalData).every(key => JSON.stringify(this.originalData[key]) === JSON.stringify(this.workingData[key]));
        }
        revertChanges() {
            // Restore original values while maintaining reference to working data
            Object.keys(this.workingData).forEach(key => {
                delete this.workingData[key];
            });
            Object.assign(this.workingData, $.extend(true, {}, this.originalData));
        }
        commit() {
            // Update original data to match current state
            this.originalData = $.extend(true, {}, this.workingData);
        }
    }
    jQuery.fn.dataListInit = function (dataListOptions) {
        var $element = $(this);
        var rows = [];
        var api = {
            addRow,
            deleteRow,
            readValuesFromModel,
            hasUnsavedRows,
            getModelAsync,
        };
        async function reloadData() {
            try {
                //empty();
                dataListHelpers.setBusy($element);
                var rowsData = await dataListOptions.listMethod();
                rows = rowsData.map(rowData => new DataListRow(rowData, api, dataListOptions));
                redraw();
            }
            finally {
                dataListHelpers.clearBusy($element);
            }
        }
        async function getModelAsync() {
            //todo we might have to await a save first if it's in progress
            //or if we didn't receive the initial data yet
            //they also might expect that the model is a static array to which new rows can be pushed or sliced from, but that is not the case here
            return [...rows.map(r => r.rowData)];
        }
        function empty() {
            rows.forEach(r => r.destroy());
            $element.empty();
        }
        function redraw() {
            empty();
            if (!rows.length) {
                $element.append(dataListHelpers.getNoRowsPanel(dataListOptions.localization?.addNewButtonText));
            }
            else {
                rows.forEach(row => {
                    $element.append(row.draw());
                });
            }
        }
        function readValuesFromModel() {
            rows.forEach(row => row.readValuesFromModel());
        }
        async function deleteRow(row) {
            if (row.rowData['id']) {
                if (!await window.abp.message.confirm('Are you sure you want to delete this row?')) {
                    return;
                }
                try {
                    dataListHelpers.setBusy(row.panel);
                    await dataListOptions.deleteMethod(row.rowData);
                }
                finally {
                    dataListHelpers.clearBusy(row.panel);
                }
            }
            rows.splice(rows.indexOf(row), 1);
            row.panel.remove();
            if (!rows.length) {
                $element.append(dataListHelpers.getNoRowsPanel(dataListOptions.localization?.addNewButtonText));
            }
        }
        function addRow(rowData = {}) {
            let row = new DataListRow(rowData, api, dataListOptions);
            if (!rows.length) {
                empty(); //remove the 'no data to display' element
            }
            $element.append(row.draw());
            row.setEditable(true);
            rows.push(row);
        }
        function hasUnsavedRows() {
            return rows.some(r => r.isRowEditable);
        }
        reloadData();
        return api;
    };
    class DataListRow {
        rowData;
        modelState = null;
        dataListApi;
        dataListOptions;
        cells = [];
        panel;
        isRowEditable = false;
        #rowContainer;
        #rowActionsButton;
        #editButton;
        #deleteButton;
        #saveButton;
        #cancelButton;
        constructor(rowData, dataListApi, dataListOptions) {
            this.rowData = rowData;
            this.dataListApi = dataListApi;
            this.dataListOptions = dataListOptions;
        }
        draw() {
            this.panel = $('<div class="card card-collapsable card-collapse bg-light mb-4">').append($('<div class="card-header bg-light">').append($('<div class="d-flex- justify-content-between-">').append(this.#rowContainer = $('<div class="d-flex align-items-end flex-wrap">'))));
            this.cells = this.dataListOptions.columns.map(columnOptions => {
                return new DataListCell(this, columnOptions);
            });
            this.cells.forEach(cell => {
                this.#rowContainer.append(cell.draw());
            });
            this.cells.forEach(cell => {
                cell.updateVisibility();
            });
            this.cells.forEach(cell => {
                if (cell.columnOptions.postDraw) {
                    cell.columnOptions.postDraw(cell);
                }
            });
            this.#rowActionsButton = new RowActionsButton(this, this.dataListOptions.rowActions || []);
            this.#rowContainer.append($('<div class="datalist-button-container-placeholder">')).append($('<div class="datalist-button-container">').append(this.#rowActionsButton.draw()).append(this.#editButton = $('<button type="button" class="btn btn-primary">').attr("title", "Edit").append($('<i class="fa fa-edit">'))).append(this.#deleteButton = $('<button type="button" class="btn btn-primary">').attr("title", "Delete").append($('<i class="fa fa-trash">'))).append(this.#cancelButton = $('<button type="button" class="btn btn-secondary close-button">').attr("title", "Cancel").append($('<i class="fa fa-undo">'))).append(this.#saveButton = $('<button type="button" class="btn btn-primary">').attr("title", "Save").append($('<i class="fa fa-save"></i>'))));
            this.#editButton.click(() => {
                this.setEditable(true);
            });
            this.#cancelButton.click(async () => {
                if (this.modelState) {
                    this.modelState.revertChanges();
                    this.readValuesFromModel();
                }
                this.setEditable(false);
                if (!this.rowData['id']) {
                    await this.dataListApi.deleteRow(this);
                }
            });
            this.#saveButton.click(async () => {
                try {
                    this.validate();
                    dataListHelpers.setBusy(this.panel);
                    if (this.dataListOptions.preModelChange) {
                        await this.dataListOptions.preModelChange(this.rowData);
                    }
                    var result = await this.dataListOptions.editMethod(this.rowData);
                    this.rowData['id'] = result['id'];
                    this.modelState?.commit();
                    this.setEditable(false);
                    if (this.dataListOptions.postModelChange) {
                        await this.dataListOptions.postModelChange(this.rowData);
                    }
                    this.readValuesFromModel();
                }
                finally {
                    dataListHelpers.clearBusy(this.panel);
                }
            });
            this.#deleteButton.click(async () => {
                if (this.dataListOptions.preModelChange) {
                    await this.dataListOptions.preModelChange(this.rowData);
                }
                await this.dataListApi.deleteRow(this);
                if (this.dataListOptions.postModelChange) {
                    await this.dataListOptions.postModelChange(this.rowData);
                }
            });
            this.setEditable(this.isRowEditable);
            return this.panel;
        }
        async setEditable(isEditable) {
            let isAllowedToEdit = this.dataListOptions.isRowEditable
                ? await Promise.resolve(this.dataListOptions.isRowEditable(this.rowData))
                : true;
            if (isEditable && !this.isRowEditable) {
                this.modelState = new RowModelState(this.rowData);
            }
            else if (!isEditable && this.isRowEditable) {
                this.modelState?.commit();
                this.modelState = null;
            }
            this.isRowEditable = isEditable;
            this.cells.forEach(cell => cell.setRowEditable(isEditable));
            this.#editButton.toggle(!isEditable && isAllowedToEdit);
            this.#deleteButton.toggle(!isEditable && isAllowedToEdit);
            this.#saveButton.toggle(isEditable);
            this.#cancelButton.toggle(isEditable);
            this.#rowActionsButton.updateVisibility();
        }
        updateVisibility() {
            this.cells.forEach(cell => cell.updateVisibility());
            this.#rowActionsButton.updateVisibility();
        }
        readValuesFromModel() {
            this.cells.forEach(cell => cell.readValueFromModel());
            this.#rowActionsButton.updateVisibility();
        }
        validate() {
            for (const cell of this.cells.filter(c => c.isVisible())) {
                cell.validate();
            }
        }
        getCellByDataField(data) {
            return this.cells.find(x => x.columnOptions.data === data);
        }
        destroy() {
        }
    }
    class RowActionsButton {
        row;
        rowActions;
        rowActionMap;
        #actionsButton;
        #actionsList;
        constructor(row, rowActions) {
            this.row = row;
            this.rowActions = rowActions;
            this.rowActionMap = new Map();
        }
        draw() {
            const result = $('<div class="dropdown d-inline-block pr-2">').append(this.#actionsList = $('<ul class="dropdown-menu dropdown-menu-right">')).append(this.#actionsButton = $('<button type="button" class="btn btn-primary d-none" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">')
                .attr("title", "Actions")
                .append($('<i class="fa fa-ellipsis-h">')));
            this.rowActions.forEach(item => {
                let li = $('<li>').append($('<a>', {
                    href: '#',
                    text: item.text,
                    class: 'dropdown-item',
                    click: async (e) => {
                        e.preventDefault();
                        try {
                            dataListHelpers.setBusy(this.row.panel);
                            await Promise.resolve(item.action(this.row.rowData, this.row));
                            this.row.readValuesFromModel();
                        }
                        finally {
                            dataListHelpers.clearBusy(this.row.panel);
                        }
                    }
                }).prepend(item.icon ? $('<i class="pr-1">').addClass(item.icon) : $()));
                this.rowActionMap.set(item, li);
            });
            this.updateVisibility();
            this.#actionsList.append([...this.rowActionMap.values()]);
            return result;
        }
        updateVisibility() {
            if (this.row.isRowEditable) {
                this.#actionsButton.hide();
                return;
            }
            var hasVisibleOptions = false;
            for (const [rowAction, element] of this.rowActionMap) {
                const isVisible = rowAction.isVisible(this.row.rowData);
                element.toggle(isVisible);
                hasVisibleOptions ||= isVisible;
            }
            this.#actionsButton.toggle(hasVisibleOptions);
        }
    }
    class DataListCell {
        row;
        columnOptions;
        editor;
        isRowEditable = false;
        constructor(row, columnOptions) {
            this.row = row;
            this.columnOptions = columnOptions;
            this.editor = new this.columnOptions.editor(this, this.columnOptions);
        }
        draw() {
            return this.editor.draw();
        }
        async readValueFromModel() {
            await this.editor.readValueFromModel();
            this.updateVisibility();
        }
        validate() {
            this.editor.getValidatedInputValue();
        }
        setRowEditable(isRowEditable) {
            this.isRowEditable = isRowEditable;
            this.editor.setRowEditable(isRowEditable);
            this.updateVisibility();
        }
        updateVisibility() {
            this.editor.domElement.toggle(this.isVisible());
        }
        isVisible() {
            return this.columnOptions.isVisible?.(this.row.rowData) ?? true;
        }
    }
    class DataListAbstractCellEditor {
        cell;
        row;
        columnOptions;
        domElement;
        isRowEditable = false;
        isCellEditable = false;
        constructor(cell, columnOptions) {
            this.cell = cell;
            this.row = cell.row;
            this.columnOptions = columnOptions;
        }
        async setRowEditable(isRowEditable) {
            this.isRowEditable = isRowEditable;
            if (this.columnOptions.isEditable === undefined) {
                this.isCellEditable = isRowEditable;
            }
            else if (typeof this.columnOptions.isEditable === 'boolean') {
                this.isCellEditable = this.columnOptions.isEditable;
            }
            else if (typeof this.columnOptions.isEditable === 'function') {
                this.isCellEditable = await Promise.resolve(this.columnOptions.isEditable(this.cell.row.rowData, this.isRowEditable));
            }
            this.refreshEditableState();
        }
        async readValueFromModelInternal() {
            if (this.columnOptions.readData) {
                return await Promise.resolve(this.columnOptions.readData(this.row.rowData));
            }
            if (this.columnOptions.data) {
                return this.row.rowData[this.columnOptions.data];
            }
            return null;
        }
        async writeValueToModel() {
            let value = await this.writeValueToModelInternal();
            await this.postChange(value);
        }
        async writeValueToModelInternal() {
            let value = this.getValidatedInputValue();
            if (this.columnOptions.writeData) {
                await Promise.resolve(this.columnOptions.writeData(value, this.row.rowData));
            }
            else if (this.columnOptions.data) {
                this.row.rowData[this.columnOptions.data] = value;
            }
            return value;
        }
        async postChange(value) {
            if (this.columnOptions.postChange) {
                await Promise.resolve(this.columnOptions.postChange(this.cell, value));
            }
            this.row.updateVisibility();
            this.row.readValuesFromModel();
        }
        throwValidationError(message, input) {
            input.focus();
            window.abp.notify.error(message);
            throw new Error(message);
        }
    }
    class TextCellEditor extends DataListAbstractCellEditor {
        input;
        label;
        editorOptions;
        draw() {
            this.editorOptions = this.columnOptions.editorOptions || {};
            this.domElement = $('<div class="form-group mr-4">').append(this.label = $('<label class="control-label">').text(this.columnOptions.title)).append(this.input = $('<input class="form-control" type="text">'));
            if (this.columnOptions.width) {
                this.domElement.css('width', this.columnOptions.width);
            }
            if (this.editorOptions.required) {
                this.input.attr('required', 'required');
                this.label.addClass('required-label');
            }
            if (this.editorOptions.maxLength) {
                this.input.attr('maxlength', this.editorOptions.maxLength);
            }
            if (this.editorOptions.regex) {
                this.input.attr('regex', this.editorOptions.regex);
            }
            if (this.editorOptions.placeholder) {
                this.input.attr('placeholder', this.editorOptions.placeholder);
            }
            this.readValueFromModel();
            this.input.blur(() => {
                this.writeValueToModel();
            });
            return this.domElement;
        }
        refreshEditableState() {
            this.input.prop('disabled', !this.isCellEditable);
        }
        async readValueFromModel() {
            var value = await this.readValueFromModelInternal() || '';
            this.input.val(value);
        }
        getValidatedInputValue() {
            let value = this.input.val();
            if (this.editorOptions.required && !value) {
                let message = this.columnOptions.title + ' is required';
                this.throwValidationError(message, this.input);
            }
            if (value && this.editorOptions.regex) {
                const regex = new RegExp(this.editorOptions.regex);
                if (!regex.test(value)) {
                    const message = this.editorOptions.regexValidationMessage || ('Please check the input for ' + this.columnOptions.title);
                    this.throwValidationError(message, this.input);
                }
            }
            return value;
        }
    }
    class TextAreaCellEditor extends DataListAbstractCellEditor {
        input;
        label;
        editorOptions;
        draw() {
            this.editorOptions = this.columnOptions.editorOptions || {};
            this.domElement = $('<div class="form-group mr-4">').append(this.label = $('<label class="control-label">').text(this.columnOptions.title)).append(this.input = $('<textarea class="form-control">'));
            if (this.columnOptions.width) {
                this.domElement.css('width', this.columnOptions.width);
            }
            if (this.editorOptions.required) {
                this.input.attr('required', 'required');
                this.label.addClass('required-label');
            }
            if (this.editorOptions.maxLength) {
                this.input.attr('maxlength', this.editorOptions.maxLength);
            }
            if (this.editorOptions.placeholder) {
                this.input.attr('placeholder', this.editorOptions.placeholder);
            }
            this.readValueFromModel();
            this.input.blur(() => {
                this.writeValueToModel();
            });
            return this.domElement;
        }
        refreshEditableState() {
            this.input.prop('disabled', !this.isCellEditable);
        }
        async readValueFromModel() {
            var value = await this.readValueFromModelInternal() || '';
            this.input.val(value);
        }
        getValidatedInputValue() {
            let value = this.input.val();
            if (this.editorOptions.required && !value) {
                let message = this.columnOptions.title + ' is required';
                this.throwValidationError(message, this.input);
            }
            return value;
        }
    }
    class DecimalCellEditor extends DataListAbstractCellEditor {
        input;
        label;
        editorOptions;
        draw() {
            this.editorOptions = this.columnOptions.editorOptions || {};
            this.domElement = $('<div class="form-group mr-4">').append(this.label = $('<label class="control-label">').text(this.columnOptions.title)).append(this.input = $('<input class="form-control no-numeric-spinner" type="number">'));
            if (this.columnOptions.width) {
                this.domElement.css('width', this.columnOptions.width);
            }
            if (this.editorOptions.required) {
                this.input.attr('required', 'required');
                this.label.addClass('required-label');
            }
            if (this.editorOptions.max !== undefined) {
                this.input.attr('max', this.editorOptions.max);
            }
            if (this.editorOptions.min !== undefined) {
                this.input.attr('min', this.editorOptions.min);
            }
            this.readValueFromModel();
            this.input.blur(() => {
                this.writeValueToModel();
            });
            return this.domElement;
        }
        refreshEditableState() {
            this.input.prop('disabled', !this.isCellEditable);
        }
        async readValueFromModel() {
            var value = await this.readValueFromModelInternal();
            this.input.val(value);
        }
        getValidatedInputValue() {
            let value = this.input.val();
            if (this.editorOptions.allowNull === false && value === '') {
                value = 0;
            }
            if (this.editorOptions.required && value === '') {
                let message = this.columnOptions.title + ' is required';
                this.throwValidationError(message, this.input);
            }
            if (value !== '') {
                if (this.editorOptions.max !== undefined && value > this.editorOptions.max || this.editorOptions.min !== undefined && value < this.editorOptions.min) {
                    let sizeValidationMessage = this.editorOptions.max !== undefined && this.editorOptions.min !== undefined
                        ? `${this.columnOptions.title} has to be a number between ${this.editorOptions.min} and ${this.editorOptions.max}`
                        : this.editorOptions.min !== undefined
                            ? `${this.columnOptions.title} has to be a number larger than ${this.editorOptions.min}`
                            : `${this.columnOptions.title} has to be a number smaller than ${this.editorOptions.max}`;
                    this.throwValidationError(sizeValidationMessage, this.input);
                }
            }
            let convertedValue = dataListHelpers.parseStringToNullableNumber(value);
            return convertedValue;
        }
    }
    class CheckboxCellEditor extends DataListAbstractCellEditor {
        input;
        draw() {
            let id = dataListHelpers.getUniqueElementId();
            this.domElement = $('<div class="form-group mr-4">').append($('<label class="m-checkbox">').attr('for', id).text(this.columnOptions.title).prepend(this.input = $('<input type="checkbox" value="true">').attr('id', id)).append($('<span>')));
            if (this.columnOptions.width) {
                this.domElement.css('width', this.columnOptions.width);
            }
            this.readValueFromModel();
            this.input.change(() => {
                this.writeValueToModel();
            });
            return this.domElement;
        }
        refreshEditableState() {
            this.input.prop('disabled', !this.isCellEditable);
        }
        async readValueFromModel() {
            var value = await this.readValueFromModelInternal() || false;
            this.input.prop('checked', value);
        }
        getValidatedInputValue() {
            return this.input.is(':checked');
        }
    }
    class IconCellEditor extends DataListAbstractCellEditor {
        iconElement;
        editorOptions;
        draw() {
            this.editorOptions = this.columnOptions.editorOptions || {};
            this.domElement = $('<div class="form-group mr-4">').append(this.iconElement = $('<i>')
                .addClass('grid-clickable-document-icon pb-3')
                .addClass(this.editorOptions.icon)
                .click(async () => {
                await this.editorOptions.click(this.row.rowData);
            }));
            this.columnOptions.width ??= '20px';
            if (this.columnOptions.width) {
                this.domElement.css('width', this.columnOptions.width);
            }
            this.readValueFromModel();
            return this.domElement;
        }
        refreshEditableState() {
            this.iconElement.prop('disabled', !this.isCellEditable);
        }
        async readValueFromModel() {
            let value = !!(await this.readValueFromModelInternal() || '');
            this.iconElement.toggle(value);
        }
        async writeValueToModel() {
        }
        async writeValueToModelInternal() {
        }
        getValidatedInputValue() {
            return undefined;
        }
    }
    class DropdownCellEditor extends DataListAbstractCellEditor {
        dropdown;
        readonlyInput;
        label;
        editorOptions;
        dropdownOptions;
        lastProcessedIsEditableState;
        draw() {
            this.editorOptions = this.columnOptions.editorOptions || {};
            this.dropdownOptions = this.editorOptions.dropdownOptions;
            this.columnOptions.data = this.editorOptions.idField;
            let id = dataListHelpers.getUniqueElementId();
            this.domElement = $('<div class="form-group mr-4">').append(this.label = $('<label>').attr('for', id).text(this.columnOptions.title)).append(this.dropdown = $('<select class="form-control d-none"></select>').attr('id', id).append($('<option value="">&nbsp;</option>'))).append(this.readonlyInput = $('<input class="form-control" type="text" disabled>'));
            if (this.columnOptions.width) {
                this.domElement.css('width', this.columnOptions.width);
            }
            if (this.editorOptions.required) {
                this.dropdown.attr('required', 'required');
                this.label.addClass('required-label');
            }
            this.lastProcessedIsEditableState = false;
            this.readValueFromModel();
            this.dropdown.change(() => {
                this.writeValueToModel();
            });
            return this.domElement;
        }
        refreshEditableState() {
            this.readonlyInput.toggle(!this.isCellEditable);
            this.dropdown.toggle(this.isCellEditable);
            if (this.lastProcessedIsEditableState !== this.isCellEditable) {
                this.lastProcessedIsEditableState = this.isCellEditable;
                if (this.isCellEditable) {
                    this.initSelect2();
                }
                else {
                    dataListHelpers.destroySelect2(this.dropdown);
                }
            }
        }
        initSelect2() {
            let rowData = this.row.rowData;
            let originalAbpServiceParamsGetter = this.dropdownOptions.abpServiceParamsGetter;
            let dropdownExtendedOptions = $.extend({}, this.dropdownOptions, {
                abpServiceParamsGetter: function (params) {
                    let result = {};
                    if (originalAbpServiceParamsGetter) {
                        $.extend(result, originalAbpServiceParamsGetter(params, rowData));
                    }
                    return result;
                }
            });
            dataListHelpers.initSelect2(this.dropdown, dropdownExtendedOptions);
        }
        async readValueFromModel() {
            var idValue = await this.readValueFromModelInternal() || '';
            var textValue = this.editorOptions.nameField
                ? this.row.rowData[this.editorOptions.nameField]
                : '';
            this.readonlyInput.val(textValue);
            dataListHelpers.addAndSetDropdownValue(this.dropdown, idValue, textValue);
        }
        async writeValueToModel() {
            let idValue = await this.writeValueToModelInternal();
            let textValue = '';
            if (idValue && this.editorOptions.nameField) {
                textValue = this.dropdown.find('option:selected').text();
                this.row.rowData[this.editorOptions.nameField] = textValue;
            }
            this.readonlyInput.val(textValue);
            if (this.dropdownOptions.onDropdownChangeWithModel
                && this.dropdown.data('select2') //or any other way to check if the select2 dropdown has already been initialized and still is initialized
            ) {
                let dropdownData = dataListHelpers.getSelect2Data(this.dropdown);
                if (dropdownData?.length && dropdownData[0].item) {
                    await Promise.resolve(this.dropdownOptions.onDropdownChangeWithModel(this.row.rowData, dropdownData[0].item, this.cell));
                }
            }
            await this.postChange(idValue);
        }
        getValidatedInputValue() {
            let value = dataListHelpers.parseStringToNullableNumber(this.dropdown.val());
            if (this.editorOptions.required && !value) {
                let message = this.columnOptions.title + ' is required';
                this.throwValidationError(message, this.dropdown);
            }
            return value;
        }
    }
    class DatePickerCellEditor extends DataListAbstractCellEditor {
        input;
        label;
        editorOptions;
        dateFormat;
        lastProcessedIsEditableState;
        draw() {
            this.editorOptions = this.columnOptions.editorOptions || {};
            this.dateFormat = this.editorOptions.dateFormat || 'L';
            this.domElement = $('<div class="form-group mr-4">').append(this.label = $('<label>').text(this.columnOptions.title)).append(this.input = $('<input type="text" class="form-control datepicker">'));
            if (this.columnOptions.width) {
                this.domElement.css('width', this.columnOptions.width);
                this.input.css('width', '100%').css('min-width', '100%');
            }
            if (this.editorOptions.required) {
                this.input.attr('required', 'required');
                this.label.addClass('required-label');
            }
            this.lastProcessedIsEditableState = false;
            this.readValueFromModel();
            this.input.blur(() => {
                this.writeValueToModel();
            });
            return this.domElement;
        }
        refreshEditableState() {
            this.input.prop('disabled', !this.isCellEditable);
            if (this.lastProcessedIsEditableState !== this.isCellEditable) {
                this.lastProcessedIsEditableState = this.isCellEditable;
                if (this.isCellEditable) {
                    dataListHelpers.initDatepicker(this.input, {
                        format: this.dateFormat,
                    });
                }
                else {
                    dataListHelpers.destroyDatepicker(this.input);
                }
            }
        }
        async readValueFromModel() {
            const value = await this.readValueFromModelInternal() || '';
            this.input.val(value ? moment(value, 'YYYY-MM-DD').format(this.dateFormat) : '');
        }
        getInputValue() {
            const value = this.input.val();
            const momentValue = value ? moment(value, this.dateFormat, true) : null;
            let convertedValue = momentValue?.isValid() ? momentValue.format('YYYY-MM-DD') : null;
            return convertedValue;
        }
        getValidatedInputValue() {
            let value = this.getInputValue();
            if (this.editorOptions.required && !value) {
                let message = this.columnOptions.title + ' is required';
                this.throwValidationError(message, this.input);
            }
            return value;
        }
    }
    window.dataList = {
        editors: {
            text: TextCellEditor,
            textarea: TextAreaCellEditor,
            checkbox: CheckboxCellEditor,
            decimal: DecimalCellEditor,
            dropdown: DropdownCellEditor,
            datepicker: DatePickerCellEditor,
            icon: IconCellEditor,
        }
    };
    var dataListHelpers = {
        getNoRowsPanel: function (addNewButtonText) {
            addNewButtonText ??= dataListHelpers.localize('AddNew');
            return $('<div class="card card-collapsable card-collapse bg-light mb-4">').append($('<div class="card-header bg-light">').append($('<div class="text-center mt-4 mb-4">').text(dataListHelpers.localize('NoDataInTheListClick{0}ToAdd', addNewButtonText))));
        },
        localize: function (key, ...args) {
            return window.app.localize.apply(this, [key, ...args]);
        },
        getUniqueElementId: function () {
            return window.abp.helper.getUniqueElementId();
        },
        addAndSetDropdownValue: function (element, value, text) {
            window.abp.helper.ui.addAndSetDropdownValue(element, value, text);
        },
        parseStringToNullableNumber: function (value) {
            return window.abp.utils.parseStringToNullableNumber(value);
        },
        setBusy: function (element) {
            window.abp.ui.setBusy(element);
        },
        clearBusy: function (element) {
            window.abp.ui.clearBusy(element);
        },
        initSelect2: function (element, options) {
            element.select2Init(options);
        },
        destroySelect2: function (element) {
            element.select2('destroy');
        },
        getSelect2Data: function (element) {
            return element.data('select2') ? element.select2('data') : undefined;
        },
        initDatepicker: function (element, options) {
            element.datepickerInit({
                format: this.dateFormat,
            });
        },
        destroyDatepicker: function (element) {
            element.datepicker('destroy');
        },
    };
})();
