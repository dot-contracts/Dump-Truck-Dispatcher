
(function ($) {
    app.modals.CreateOrEditDriverModal = function () {

        var _modalManager;
        var _driverService = abp.services.app.driver;
        var _dtHelper = abp.helper.dataTables;
        var _$generalForm = null;
        var _$statusForm = null;
        var _$payForm = null;
        var _wasInactive = null;
        var _employeeTimeClassifications = null;
        var _allTimeClassifications = null;
        var _dispatchViaDriverApp = abp.setting.getInt('App.DispatchingAndMessaging.DispatchVia') === abp.enums.dispatchVia.driverApplication;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            var modal = _modalManager.getModal();
            _$generalForm = modal.find('form[name="DriverGeneralForm"]');
            _$generalForm.validate();
            _$statusForm = modal.find('form[name="DriverStatusForm"]');
            _$statusForm.validate();
            _$payForm = modal.find('form[name="DriverPayForm"]');
            _$payForm.validate();

            $.validator.addMethod(
                "regex",
                function (value, element, regexp) {
                    var re = new RegExp(regexp, 'i');
                    return this.optional(element) || re.test(value);
                },
                "Please check your input."
            );
            _$generalForm.find('#CellPhoneNumber').rules('add', { regex: app.regex.cellPhoneNumber });
            _$generalForm.find('#EmailAddress').rules('add', { regex: app.regex.email });

            _wasInactive = getIsInactiveValue();

            setRequiredAttributesAccordingToPreferredFormat();

            modal.find('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
                var target = $(e.target).attr("href");
                if (target === "#DriverPayTab") {
                    modal.find("div.modal-content").css({ 'min-width': 'fit-content' });
                } else {
                    modal.find("div.modal-content").css({ 'min-width': 'unset' });
                }
            });

            _$generalForm.find("#OrderNotifyPreferredFormat").select2Init({
                showAll: true,
                allowClear: false
            });

            _$generalForm.find("#OfficeId").select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: false
            });

            _$statusForm.find("#LicenseExpirationDate, #LastPhysicalDate, #NextPhysicalDueDate, #LastMvrDate, #NextMvrDueDate, #DateOfHire, #TerminationDate").datepickerInit();

            _$generalForm.find('#OrderNotifyPreferredFormat').change(function () {
                setRequiredAttributesAccordingToPreferredFormat();
            });

            function setRequiredAttributesAccordingToPreferredFormat() {
                var preferredFormat = Number(_$generalForm.find('#OrderNotifyPreferredFormat').val());
                var preferredFormatEnum = abp.enums.orderNotifyPreferredFormat;

                var $emailAddress = _$generalForm.find('#EmailAddress');
                setRequired($emailAddress, [
                    preferredFormatEnum.email,
                    preferredFormatEnum.both,
                ].includes(preferredFormat));

                var $cellPhoneNumber = _$generalForm.find('#CellPhoneNumber');
                setRequired($cellPhoneNumber, [
                    preferredFormatEnum.sms,
                    preferredFormatEnum.both,
                ].includes(preferredFormat));
            }

            function setRequired($ctrl, isRequired) {
                $ctrl.attr('required', isRequired ? 'required' : null);
                $ctrl.closest('.form-group').find('label').toggleClass('required-label', isRequired);
                if (!isRequired) {
                    $ctrl.removeAttr('aria-required');
                    $ctrl.closest('.form-group').removeClass('has-error');
                }
            }

            function getTimeClassification(id) {
                if (_allTimeClassifications === null) {
                    return null;
                }
                var matches = _allTimeClassifications.filter(a => a.id === id);
                if (matches.length) {
                    return matches[0];
                }
                return null;
            }

            function getTimeClassificationName(id) {
                var timeClassification = getTimeClassification(id);
                return timeClassification && timeClassification.name || null;
            }

            function getUnselectedTimeClassifications() {
                if (_employeeTimeClassifications === null || _allTimeClassifications === null) {
                    return [];
                }
                let remaining = _allTimeClassifications.filter(a => !_employeeTimeClassifications.map(e => e.timeClassificationId).includes(a.id));
                return remaining;
            }

            function getGridData() {
                if (_employeeTimeClassifications === null) {
                    return _dtHelper.getEmptyResult();
                }
                //if all classification rows are filled (no empty rows exist), but there are unused classifications, add one empty row to the bottom
                if (!_employeeTimeClassifications.filter(x => x.timeClassificationId === 0).length) {
                    let remaining = getUnselectedTimeClassifications();
                    if (remaining.length) {
                        _employeeTimeClassifications.push({
                            id: 0,
                            timeClassificationId: 0,
                            isDefault: false,
                            allowForManualTime: false,
                            payRate: 0.00
                        });
                    }
                }

                return _dtHelper.fromAbpResult({
                    items: _employeeTimeClassifications,
                    totalCount: _employeeTimeClassifications.length
                });
            }

            var employeeTimeClassificationsTable = modal.find('#EmployeeTimeClassificationsTable');
            var employeeTimeClassificationsGrid = employeeTimeClassificationsTable.DataTableInit({
                paging: false,
                serverSide: true,
                processing: true,
                ordering: false,
                info: false,
                ajax: function (data, callback, settings) {
                    let driverId = _$generalForm.find("#Id").val();
                    if (_employeeTimeClassifications !== null) {
                        callback(getGridData());
                        return;
                    }
                    _driverService.getDriverEmployeeTimeClassifications({ id: driverId }).done(function (result) {
                        if (_employeeTimeClassifications === null) {
                            _employeeTimeClassifications = result.employeeTimeClassifications;
                            _allTimeClassifications = result.allTimeClassifications;
                        }
                        callback(getGridData());
                    });
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
                        data: "timeClassificationId",
                        title: "Time Classification",
                        className: "all",
                        createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                            cell = $(cell);
                            if (rowData.timeClassificationId && getTimeClassification(rowData.timeClassificationId).isProductionBased) {
                                cell.text(getTimeClassification(rowData.timeClassificationId).name);
                                return;
                            }
                            cell.empty().addClass('cell-editable');
                            var editor = $('<select></select>').appendTo(cell);
                            if (rowData.timeClassificationId > 0) {
                                editor.append($('<option selected></option>').text(getTimeClassificationName(rowData.timeClassificationId)).attr("value", rowData.timeClassificationId));
                            } else {
                                editor.append($('<option selected></option>').html('&nbsp;').attr("value", 0));
                            }
                            var remainingItems = getUnselectedTimeClassifications();
                            remainingItems.forEach(a => {
                                editor.append($('<option></option>').text(a.name).attr("value", a.id));
                            });
                            editor.change(function () {
                                var newValue = Number(editor.val());
                                if (newValue === rowData.timeClassificationId) {
                                    return;
                                }
                                rowData.timeClassificationId = newValue;
                                var timeClassification = getTimeClassification(newValue);
                                rowData.payRate = timeClassification && timeClassification.defaultRate || 0;
                                //rowData.timeClassification = getTimeClassification(newValue);
                                setTimeout(() => reloadGrid(), 0); //we need to let the select2 events to finish firing first before we proceed to redraw the grid, otherwise scroll.select2 event does not get cleared
                            });
                            editor.select2Init({
                                showAll: true,
                                allowClear: false
                            });
                        }
                    },
                    {
                        data: "payRate",
                        title: "Pay Rate",
                        className: "all payRateColumn",
                        createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                            cell = $(cell);
                            cell.empty();
                            if (rowData.timeClassificationId === 0) {
                                return;
                            }
                            cell.addClass('cell-editable');
                            var editor = $('<input type="number" step="0.01" class="no-numeric-spinner form-control">');
                            var timeClassification = getTimeClassification(rowData.timeClassificationId);
                            var isProductionBased = timeClassification && timeClassification.isProductionBased;
                            var inputGroup = $('<div class="input-group">').appendTo(cell);
                            if (isProductionBased) {
                                editor.appendTo(inputGroup);
                                $('<div class="input-group-append"><span class="input-group-text">%</span></div>').appendTo(inputGroup);
                            } else {
                                $('<div class="input-group-prepend"><span class="input-group-text"></span></div>').appendTo(inputGroup).find('span').text(abp.setting.get('App.General.CurrencySymbol'));
                                editor.appendTo(inputGroup);
                            }
                            editor.val(_dtHelper.renderRate(rowData.payRate));
                            editor.focusout(function () {
                                var newValue = _dtHelper.renderRate(editor.val());
                                if (newValue === _dtHelper.renderRate(rowData.payRate)) {
                                    return;
                                }
                                var timeClassification = getTimeClassification(rowData.timeClassificationId);
                                if (Number(newValue) > 100 && timeClassification && timeClassification.isProductionBased) {
                                    abp.message.error('Please enter a valid number less than 100!');
                                    editor.val(_dtHelper.renderRate(rowData.payRate));
                                    return;
                                }
                                if (Number(newValue) < 0) {
                                    abp.message.error('Negative numbers aren\'t allowed');
                                    editor.val(_dtHelper.renderRate(rowData.payRate));
                                    return;
                                }
                                rowData.payRate = Number(newValue);
                                editor.val(newValue);
                            });
                        }
                    },
                    {
                        data: "isDefault",
                        title: "Default",
                        render: function (data) { return _dtHelper.renderCheckbox(data); },
                        className: "checkmark text-center all",
                        createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                            cell = $(cell);
                            if (rowData.timeClassificationId === 0) {
                                cell.empty();
                                return;
                            }
                            cell.click(function () {
                                if (rowData.isDefault) {
                                    return;
                                }
                                _employeeTimeClassifications.forEach(e => e.isDefault = false);
                                rowData.isDefault = true;
                                reloadGrid();
                            });
                        }
                    },
                    {
                        data: "allowForManualTime",
                        title: "Allow for<br/>Manual Time",
                        render: function (data) { return _dtHelper.renderCheckbox(data); },
                        className: "checkmark text-center all",
                        createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                            cell = $(cell);
                            if (rowData.timeClassificationId === 0) {
                                cell.empty();
                                return;
                            }
                            cell.click(function () {
                                rowData.allowForManualTime = !rowData.allowForManualTime;
                                reloadGrid();
                            });
                        }
                    },
                    {
                        data: null,
                        orderable: false,
                        autoWidth: false,
                        defaultContent: '',
                        width: "70px",
                        className: "all actions-sm",
                        responsivePriority: 1,
                        render: function (data, type, full, meta) {
                            return full.isDefault
                                || full.timeClassificationId && getTimeClassification(full.timeClassificationId).isProductionBased
                                || full.timeClassificationId === 0
                                    ? ''
                                    : '<button type="button" class="btn btn-primary btn-sm btnDeleteRow"><i class="fa fa-trash"></i> Delete</button>';
                        }
                    }
                ]
            });

            _modalManager.getModal().on('shown.bs.modal', function () {
                employeeTimeClassificationsGrid
                    .columns.adjust()
                    .responsive.recalc();
            });

            var reloadGrid = function () {
                employeeTimeClassificationsGrid.ajax.reload();
            };

            employeeTimeClassificationsTable.on('click', '.btnDeleteRow', async function (e) {
                e.preventDefault();
                var record = _dtHelper.getRowData(this);
                if (await abp.message.confirm(
                    'Are you sure you want to delete the time classification?'
                )) {
                    _employeeTimeClassifications.splice(_employeeTimeClassifications.indexOf(record), 1);
                    reloadGrid();
                }
            });

        };

        this.save = async function () {
            if (!_$generalForm.valid()) {
                _$generalForm.showValidateMessage();
                return;
            }

            if (!_$statusForm.valid()) {
                _$statusForm.showValidateMessage();
                return;
            }

            if (!_$payForm.valid()) {
                _$payForm.showValidateMessage();
                return;
            }

            var driver = _$generalForm.serializeFormToObject();
            $.extend(driver, _$statusForm.serializeFormToObject());
            //$.extend(driver, _$payForm.serializeFormToObject());
            var isInactive = getIsInactiveValue();
            driver.IsInactive = isInactive;
            driver.ConnectToUser = _$generalForm.find("#ConnectToUser").is(":checked");
            driver.employeeTimeClassifications = _employeeTimeClassifications;

            if (driver.employeeTimeClassifications === null || driver.employeeTimeClassifications.filter(e => e.timeClassificationId > 0).length === 0) {
                abp.message.error("At least one time classification is required");
                return;
            }

            if (driver.employeeTimeClassifications.filter(e => e.timeClassificationId === 0 && e.isDefault).length) {
                abp.message.error("The default time classification has to be selected");
                return;
            }

            if (driver.employeeTimeClassifications.filter(e => e.isDefault).length === 0) {
                abp.message.error("The default time classification has to be selected");
                return;
            }

            if (driver.employeeTimeClassifications.filter(e => e.payRate < 0).length) {
                abp.message.error("Negative pay rates aren't allowed");
                return;
            }

            try {
                _modalManager.setBusy(true);
                if (driver.Id && !_wasInactive && isInactive) {
                    let driverInactivationResult = await _driverService.getDriverInactivationInfo({
                        id: driver.Id
                    });

                    if (driverInactivationResult.hasOpenDispatches) {
                        abp.message.error("This driver has open dispatches and can't be inactivated until the dispatches are either completed or cancelled.");
                        return;
                    }
                    if (driverInactivationResult.hasDriverAssignments) {
                        _modalManager.setBusy(false);
                        if (!await abp.message.confirm(
                            'This driver is still associated with a truck and will be removed from the truck. If this truck is assigned to an order line, the truck will be removed from the order line. Are you sure you want to do this?'
                        )) {
                            return;
                        }
                        _modalManager.setBusy(true);
                    }

                    if (driver.UserId && driver.IsUserActive) {
                        if (await abp.message.confirm(
                            `Do you want us to inactivate the user account associated with this driver? If you do not want the user account to be inactivated, select 'Cancel'.`
                        )) {
                            driver.InactivateUser = true;
                        }
                    }
                }
                if (!isInactive && _dispatchViaDriverApp) {
                    if (driver.EmailAddress) {
                        var thereAreActiveDriversWithSameEmail = await _driverService.thereAreActiveDriversWithSameEmail({
                            email: driver.EmailAddress,
                            exceptDriverId: driver.Id
                        });
                        if (thereAreActiveDriversWithSameEmail) {
                            _modalManager.setBusy(false);
                            if (!await abp.message.confirm(
                                app.localize('ThereAreActiveDriversWithSameEmailPrompt')
                            )) {
                                return;
                            }
                            _modalManager.setBusy(true);
                        }
                    } else if (!driver.UserId) {
                        _modalManager.setBusy(false);
                        if (!await abp.message.confirm(
                            app.localize('EmailIsRequiredForDriverAppPrompt')
                        )) {
                            return;
                        }
                        _modalManager.setBusy(true);
                    }
                }

                _modalManager.setBusy(true);
                var editResult = await _driverService.editDriver(driver);

                abp.notify.info('Saved successfully.');
                _modalManager.setResult(editResult);
                _modalManager.close();
                abp.event.trigger('app.createOrEditDriverModalSaved', editResult);
            }
            finally {
                _modalManager.setBusy(false);
            }

        };

        function getIsInactiveValue() {
            return _$generalForm.find("#IsInactive").is(":checked");
        }
    };
})(jQuery);
