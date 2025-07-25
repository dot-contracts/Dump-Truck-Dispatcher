(function ($) {
    app.modals.EditChargesModal = function () {

        var _modalManager;
        var _modal = null;
        var _orderLineId = null;
        var _dtHelper = abp.helper.dataTables;
        //var _$form = null;
        var _chargesDataList = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _modal = _modalManager.getModal();

            const args = _modalManager.getArgs();
            _orderLineId = args.orderLineId;

            //_$form = _modal.find('form');
            //_$form.validate();

            _chargesDataList = _modalManager.getModal().find('#ChargesDataList').dataListInit({
                listMethod: async () => {
                    return await abp.services.app.charge.getChargesForOrderLine({ orderLineId: _orderLineId });
                },
                editMethod: abp.services.app.charge.editCharge,
                deleteMethod: abp.services.app.charge.deleteCharge,
                postModelChange: postModelChange,
                preModelChange: preModelChange,
                isRowEditable: (rowData) => !rowData.isBilled && !rowData.hasInvoiceLines,
                localization: {
                    addNewButtonText: _modalManager.getModal().find('#AddNewCharge').text(),
                },
                columns: [
                    {
                        title: 'Item',
                        data: 'itemId',
                        width: '200px',
                        editor: dataList.editors.dropdown,
                        editorOptions: {
                            required: true,
                            idField: 'itemId',
                            nameField: 'itemName',
                            dropdownOptions: {
                                abpServiceMethod: listCacheSelectLists.item(),
                                showAll: true,
                                allowClear: false,
                            },
                        },
                    },
                    {
                        title: 'UOM',
                        data: 'unitOfMeasureId',
                        width: '200px',
                        editor: dataList.editors.dropdown,
                        editorOptions: {
                            required: true,
                            idField: 'unitOfMeasureId',
                            nameField: 'unitOfMeasureName',
                            dropdownOptions: {
                                abpServiceMethod: listCacheSelectLists.uom(),
                                showAll: true,
                                allowClear: false,
                            },
                        },
                    },
                    {
                        title: 'Rate',
                        data: 'rate',
                        width: '100px',
                        editor: dataList.editors.decimal,
                        editorOptions: {
                            required: true,
                            number: true,
                            min: 0,
                        },
                    },
                    {
                        title: 'Qty',
                        data: 'quantity',
                        width: '100px',
                        editor: dataList.editors.decimal,
                        editorOptions: {
                            number: true,
                            min: 0,
                        },
                        isVisible: (rowData) => !rowData.useMaterialQuantity,
                    },
                    {
                        title: 'Use material quantity',
                        data: 'useMaterialQuantity',
                        width: '200px',
                        editor: dataList.editors.checkbox,
                        isVisible: (rowData) => abp.features.isEnabled('App.Charges.UseMaterialQuantity'),
                    },
                    {
                        title: 'Charge',
                        data: 'chargeAmount',
                        width: '100px',
                        editor: dataList.editors.text,
                        isEditable: (rowData, isRowEditable) => false,
                        readData: (rowData) => _dtHelper.renderMoneyUnrounded(rowData.rate * rowData.quantity),
                        writeData: (value, rowData) => { },
                        isVisible: (rowData) => !rowData.useMaterialQuantity,
                    },
                    {
                        title: 'Description',
                        data: 'description',
                        width: '780px',
                        editor: dataList.editors.text,
                        editorOptions: {
                            maxLength: abp.entityStringFieldLengths.charge.description,
                        },
                    },
                ],
            });

            _modal.find('#AddNewCharge').click(async function (e) {
                e.preventDefault();
                _chargesDataList.addRow({
                    orderLineId: _orderLineId,
                    isBilled: false,
                });
            });
        };

        async function preModelChange(rowData) {
            rowData.chargeAmount = (rowData.rate * rowData.quantity) || 0;
        }

        async function postModelChange() {
            await calculateNewTotalAndNotifyAsync();
        }

        async function calculateNewTotalAndNotifyAsync() {
            let charges = await _chargesDataList.getModelAsync();
            let newTotal = charges.reduce((acc, charge) => acc + charge.chargeAmount, 0);
            try {
                abp.event.trigger('app.chargesEditedModal', {
                    orderLineId: _orderLineId,
                    charges: charges,
                    totalCharges: newTotal,
                });
            } catch (e) {
                console.error(e);
                //continue on event related error
            }
        }

        this.save = async function () {
            //if (!_$form.valid()) {
            //    _$form.showValidateMessage();
            //    return;
            //}

            if (_chargesDataList.hasUnsavedRows()) {
                abp.message.warn('Please save or cancel the unsaved charges.');
                return;
            }

            _modalManager.setBusy(true);
            try {
                // Nothing else to save yet
            } finally {
                _modalManager.setBusy(false);
            }

            //they can cancel the modal, but the charges could be already saved separately, so we'll use events instead
            //let charges = await _chargesDataList.getModelAsync();
            //let total = charges.reduce((acc, charge) => acc + charge.chargeAmount, 0);
            //_modalManager.setResult({
            //    total,
            //});
            _modalManager.close();
        };
    };
})(jQuery);
