(function ($) {
    app.modals.CreateOrEditTicketModal = function () {

        var _modalManager;
        var _ticketService = abp.services.app.ticket;
        var _$form = null;
        var _$ticketPhotoInput = null;
        var _ticketIdForPhotoUpload = null;
        var _dtHelper = abp.helper.dataTables;
        var _orderLineId = null;
        var _orderLineIsProductionPay = null;
        var _validateTrucksAndDrivers = abp.setting.getBoolean('App.General.ValidateDriverAndTruckOnTickets');
        var _features = {
            separateItems: abp.features.isEnabled('App.SeparateMaterialAndFreightItems'),
        };
        var _visibleTicketControls = null;

        this.init = async function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form[name=TicketForm]');
            _$ticketPhotoInput = _$form.find('#TicketPhoto');

            _orderLineId = Number(_$form.find('#OrderLineId').val());
            _orderLineIsProductionPay = _$form.find('#OrderLineIsProductionPay').val() === 'True';

            _$form.find("#RefreshGridButton").click(function () {
                reloadGrid();
            });

            _$form.find("#AddTicketButton").click(function () {
                _modalManager.setBusy(true);
                _ticketService.editOrderTicket({
                    orderLineId: _orderLineId,
                    ticketNumber: ''
                }).done(function (e) {
                    abp.notify.info('Ticket added');
                    abp.event.trigger('app.ticketEditedModal', e);
                    reloadGrid();
                }).always(function () {
                    _modalManager.setBusy(false);
                });
            });

            _$ticketPhotoInput.change(function () {
                if (!_ticketIdForPhotoUpload) {
                    return;
                }

                if (!abp.helper.validateTicketPhoto(_$ticketPhotoInput)) {
                    return;
                }
                const file = _$ticketPhotoInput[0].files[0];
                const reader = new FileReader();

                reader.addEventListener("load", function () {
                    _ticketService.addTicketPhoto({
                        ticketId: _ticketIdForPhotoUpload,
                        ticketPhoto: reader.result,
                        ticketPhotoFilename: file.name
                    }).done(function (result) {
                        //result.ticketPhotoId
                        reloadGrid();
                    }).always(function () {
                        _ticketIdForPhotoUpload = null;
                        _$ticketPhotoInput.val('');
                        abp.ui.clearBusy(_modalManager.getModal());
                        _modalManager.setBusy(false);
                    });
                }, false);

                abp.ui.setBusy(_modalManager.getModal());
                _modalManager.setBusy(true);
                reader.readAsDataURL(file);
            });

            function editTicket(ticket, cell) {
                //abp.ui.setBusy(cell);
                return _ticketService.editOrderTicket(ticket).done(function (result) {
                    abp.event.trigger('app.ticketEditedModal', result);
                    abp.notify.info('Saved successfully.');
                    if (result.warningText) {
                        abp.message.warn(result.warningText);
                    }
                    return result;
                }).fail(function () {
                    reloadGrid();
                }).always(function () {
                    //abp.ui.clearBusy(cell);
                });
            }

            _visibleTicketControls = await _ticketService.getVisibleTicketControls(_orderLineId);

            var ticketsTable = $('#TicketsTable');
            var ticketsGrid = ticketsTable.DataTableInit({
                paging: false,
                info: false,
                ordering: false,
                ajax: function (data, callback, settings) {
                    _ticketService.loadTicketsByOrderLineId(_orderLineId).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                editable: {
                    saveCallback: function (rowData, cell) {
                        return editTicket(rowData, cell);
                    }
                },
                columns: [
                    {
                        width: '20px',
                        className: 'control responsive',
                        orderable: false,
                        render: function () { return ''; }
                    },
                    {
                        data: "ticketDateTime",
                        title: "Time",
                        render: function (data, type, full, meta) { return _dtHelper.renderTime(full.ticketDateTime, ''); },
                        className: "all",
                        width: "85px",
                        editable: {
                            editor: _dtHelper.editors.time
                        }
                    },
                    {
                        data: "ticketNumber",
                        title: "Ticket Number",
                        className: "cell-text-wrap all",
                        editable: {
                            editor: _dtHelper.editors.text,
                            required: true,
                            maxLength: 20
                        }
                    },
                    {
                        data: "freightQuantity",
                        title: _visibleTicketControls.materialQuantity ? "Freight<br>Quantity" : "Quantity",
                        className: "all",
                        visible: _visibleTicketControls.freightQuantity,
                        editable: {
                            editor: _dtHelper.editors.quantity
                        }
                    },
                    {
                        data: "materialQuantity",
                        title: _visibleTicketControls.freightQuantity ? "Material<br>Quantity" : "Quantity",
                        className: "all",
                        visible: _visibleTicketControls.materialQuantity,
                        editable: {
                            editor: _dtHelper.editors.quantity
                        }
                    },
                    {
                        data: "freightUomName",
                        title: "Freight<br>UOM",
                        visible: _visibleTicketControls.freightUom,
                    },
                    {
                        data: "materialUomName",
                        title: "Material<br>UOM",
                        visible: _visibleTicketControls.materialUom,
                    },
                    {
                        data: "truckCode",
                        title: "Truck",
                        width: "90px",
                        className: "truck-code-column all",
                        editable: {
                            editor: _dtHelper.editors.dropdown,
                            idField: 'truckId',
                            nameField: 'truckCode',
                            dropdownOptions: {
                                abpServiceMethod: abp.services.app.truck.getTrucksSelectList,
                                abpServiceParams: {
                                    allOffices: true,
                                    includeLeaseHaulerTrucks: true,
                                    activeOnly: true,
                                    excludeTrailers: true,
                                    orderLineId: _validateTrucksAndDrivers ? _orderLineId : null,
                                    leaseHaulerId: abp.session.leaseHaulerId,
                                },
                                showAll: false,
                                allowClear: true
                            },
                            editCompleteCallback: function editCompleteCallback(editResult, rowData, cell) {
                                updateRelatedDropdownCellValue(cell, rowData, editResult.ticket, 'driver-name-column', 'driverId', 'driverName');
                                updateRelatedDropdownCellValue(cell, rowData, editResult.ticket, 'trailer-truck-code-column', 'trailerId', 'trailerTruckCode');
                            }
                        }
                    },
                    {
                        data: "trailerTruckCode",
                        title: "Trailer",
                        width: "90px",
                        className: "trailer-truck-code-column all",
                        editable: {
                            editor: _dtHelper.editors.dropdown,
                            idField: 'trailerId',
                            nameField: 'trailerTruckCode',
                            dropdownOptions: {
                                abpServiceMethod: abp.services.app.truck.getTrucksSelectList,
                                abpServiceParams: {
                                    allOffices: true,
                                    includeLeaseHaulerTrucks: true,
                                    activeOnly: true,
                                    assetType: abp.enums.assetType.trailer,
                                    orderLineId: _validateTrucksAndDrivers ? _orderLineId : null,
                                    leaseHaulerId: abp.session.leaseHaulerId,
                                },
                                showAll: false,
                                allowClear: true
                            },
                            editCompleteCallback: function editCompleteCallback(editResult, rowData, cell) {
                                updateRelatedDropdownCellValue(cell, rowData, editResult.ticket, 'driver-name-column', 'driverId', 'driverName');
                                updateRelatedDropdownCellValue(cell, rowData, editResult.ticket, 'truck-code-column', 'truckId', 'truckCode');
                            }
                        }
                    },
                    {
                        data: "driverName",
                        title: "Driver",
                        width: "90px",
                        className: "driver-name-column all",
                        editable: {
                            editor: _dtHelper.editors.dropdown,
                            idField: 'driverId',
                            nameField: 'driverName',
                            dropdownOptions: {
                                abpServiceMethod: abp.services.app.driver.getDriversSelectList,
                                abpServiceParams: {
                                    includeLeaseHaulerDrivers: true,
                                    orderLineId: _validateTrucksAndDrivers ? _orderLineId : null,
                                    leaseHaulerId: abp.session.leaseHaulerId,
                                },
                                abpServiceParamsGetter: (params, rowData) => ({
                                    truckId: rowData.truckId
                                }),
                                showAll: false,
                                allowClear: true
                            },
                            editCompleteCallback: function editCompleteCallback(editResult, rowData, cell) {
                                updateRelatedDropdownCellValue(cell, rowData, editResult.ticket, 'truck-code-column', 'truckId', 'truckCode');
                                updateRelatedDropdownCellValue(cell, rowData, editResult.ticket, 'trailer-truck-code-column', 'trailerId', 'trailerTruckCode');
                            }
                        }
                    },
                    {
                        data: 'ticketPhotoId',
                        width: "10px",
                        className: "all",
                        render: function (data, type, full, meta) {
                            return full.ticketPhotoId ? '<i class="fa-regular fa-file-image showTicketPhotoButton"></i>'
                                : full.isInternal ? '<i class="fa-regular fa-file printTicketButton"></i>' : '';
                        }
                    },
                    {
                        data: null,
                        orderable: false,
                        responsivePriority: 1,
                        name: "Actions",
                        width: "10px",
                        className: "actions all",
                        render: function (data, type, full, meta) {
                            let uploadButtonCaption = full.ticketPhotoId ? 'Replace ticket' : 'Add ticket image';
                            return '<div class="dropdown action-button">'
                                + '<ul class="dropdown-menu dropdown-menu-right">'
                                + `<li><a class="btnUploadTicketPhotoForRow dropdown-item"><i class="fa-regular fa-file-image"></i> ${uploadButtonCaption}</a></li>`
                                + (full.ticketPhotoId ? '<li><a class="btnDeleteTicketPhotoForRow dropdown-item"><i class="fa-regular fa-file-image"></i> Delete image</a></li>' : '')
                                + '<li><a class="btnDeleteRow dropdown-item" title="Delete"><i class="fa fa-trash"></i> Delete entire ticket</a></li>'
                                + '</ul>'
                                + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                                + '</div>';
                        }
                    }
                ]
            });

            function updateRelatedDropdownCellValue(editedCell, rowData, editResult, relatedCellClass, relatedIdField, relatedNameField) {
                let relatedCell = $(editedCell).closest('tr').find('td.' + relatedCellClass);
                let relatedEditor = relatedCell.find('select');
                if (!relatedEditor.length) {
                    relatedCell.text(editResult[relatedNameField]);
                } else {
                    if (parseInt(rowData[relatedIdField]) !== editResult[relatedIdField]) {
                        abp.helper.ui.addAndSetSelect2ValueSilently(relatedEditor, editResult[relatedIdField], editResult[relatedNameField]);
                    }
                }
                rowData[relatedIdField] = editResult[relatedIdField];
                rowData[relatedNameField] = editResult[relatedNameField];
            }

            _modalManager.getModal().on('shown.bs.modal', function () {
                ticketsGrid.columns.adjust().responsive.recalc();
            });

            function reloadGrid() {
                ticketsGrid.ajax.reload(null, false);
            }

            ticketsTable.on('click', '.btnDeleteRow', async function () {
                var ticket = _dtHelper.getRowData(this);
                if (ticket.receiptLineId) {
                    abp.message.error('You can\'t delete tickets associated with receipts');
                    return;
                }

                if (!await abp.message.confirm('Are you sure you want to delete the item?')) {
                    return;
                }

                _ticketService.deleteTicket({ id: ticket.id }).done(function (e) {
                    abp.notify.info('Successfully deleted.');
                    abp.event.trigger('app.ticketDeletedModal', e);
                    reloadGrid();
                });

            });

            ticketsTable.on('click', '.btnUploadTicketPhotoForRow', function () {
                var ticket = _dtHelper.getRowData(this);
                _ticketIdForPhotoUpload = ticket.id;
                _$ticketPhotoInput.click();
            });

            ticketsTable.on('click', '.printTicketButton', function (e) {
                e.preventDefault();
                var ticket = _dtHelper.getRowData(this);
                let url = abp.appPath + 'app/Tickets/GetTicketPrintOut?ticketId=' + ticket.id;
                app.openPopup(url);
            });

            ticketsTable.on('click', '.showTicketPhotoButton', function (e) {
                e.preventDefault();
                var ticket = _dtHelper.getRowData(this);
                let url = abp.appPath + 'app/Tickets/GetTicketPhoto/' + ticket.id;
                app.openPopup(url);
            });

            ticketsTable.on('click', '.btnDeleteTicketPhotoForRow', async function (e) {
                e.preventDefault();
                var ticket = _dtHelper.getRowData(this);
                if (!await abp.message.confirm('Are you sure you want to delete the image?')) {
                    return;
                }

                _modalManager.setBusy(true);
                _ticketService.deleteTicketPhoto({
                    ticketId: ticket.id
                }).done(function () {
                    ticket.ticketPhotoId = null;
                    reloadGrid();
                }).always(function () {
                    _modalManager.setBusy(false);
                });

            });
        };

    };
})(jQuery);
