(function ($) {
    app.modals.EditTruckRequestDetailModal = function () {
        const _leaseHaulerRequestEditAppService = abp.services.app.leaseHaulerRequestEdit;
        const _schedulingService = abp.services.app.scheduling;
        const _permissions = {
            viewLeaseHaulerRequests: abp.auth.hasPermission('Pages.LeaseHaulerRequests'),
            editLeaseHaulerRequests: abp.auth.hasPermission('Pages.LeaseHaulerRequests.Edit'),
            leaseHaulerPortalTruckRequests: abp.auth.hasPermission('LeaseHaulerPortal.Truck.Request'),
        };
        let _modalManager;
        let _$form = null;
        let _validateTrucks = null;
        let _cache = {
            leaseHaulers: [],
            trucks: {},
            drivers: {}
        };

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find('#Date').datepickerInit();
            _$form.find('#Shift').select2Init({ allowClear: false });

            _$form.find('#OfficeId').select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: false
            });

            const $leaseHaulerDropdown = _$form.find('#LeaseHaulerId');
            if (_permissions.editLeaseHaulerRequests) {
                $leaseHaulerDropdown.select2Init({
                    abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulersSelectList,
                    showAll: false,
                    allowClear: true
                });
            }

            const $truckSelectionRowTemplate = _$form.find('#truckSelectionRowTemplate .truck-selection-row');
            const $truckSelectionBlock = _$form.find('#truckSelectionBlock');
            const $truckSelectionBlockTrucks = _$form.find('#truckSelectionBlockTrucks');
            let truckCount = $truckSelectionBlockTrucks.find('.truck-selection-row').length;
            const $approved = _$form.find('#Approved');
            const $available = _$form.find('#Available');

            if ($available.val() > 0) {
                $truckSelectionBlock.show();

                if (_permissions.leaseHaulerPortalTruckRequests) {
                    let approvedValue = parseInt($approved.val());
                    let availableValue = parseInt($available.val());

                    // when editing request but nothing has been approved yet
                    if (isNaN(approvedValue)
                        && !isNaN(availableValue)
                        && availableValue > truckCount
                    ) {
                        let trucksToAdd = availableValue - truckCount;
                        addRow(trucksToAdd);
                        truckCount = availableValue;
                        return;
                    }
                }
            }

            function addTrucksToCache(records) {
                if (records && records.length) {
                    for (let i = 0; i < records.length; i++) {
                        const truck = records[i];
                        if (!_cache.trucks[truck.leaseHaulerId]) {
                            _cache.trucks[truck.leaseHaulerId] = [];
                        }
                        _cache.trucks[truck.leaseHaulerId].push(truck);
                    }
                }
            }

            function addDriversToCache(records) {
                if (records && records.length) {
                    for (let i = 0; i < records.length; i++) {
                        const driver = records[i];
                        if (!_cache.drivers[driver.leaseHaulerId]) {
                            _cache.drivers[driver.leaseHaulerId] = [];
                        }
                        _cache.drivers[driver.leaseHaulerId].push(driver);
                    }
                }
            }

            function populateTruckDropdownWithRecords(dropdowns, records) {
                dropdowns.each(function () {
                    const dropdown = $(this);
                    const oldValue = dropdown.val();
                    dropdown.find('option[value!=""]').remove();
                    if (records && records.length) {
                        for (let i = 0; i < records.length; i++) {
                            const truck = records[i];
                            $('<option>')
                            .attr('value', truck.truckId)
                            .attr('default-driver-id', truck.defaultDriverId)
                            .data('truck', truck)
                            .text(truck.truckCode)
                            .prop('selected', truck.truckId.toString() === oldValue ? true : false)
                            .appendTo(dropdown);
                        }
                    }
                });
            }

            function populateDriverDropdownWithRecords(dropdowns, records) {
                dropdowns.each(function () {
                    const dropdown = $(this);
                    const oldValue = dropdown.val();
                    dropdown.find('option[value!=""]').remove();
                    if (records && records.length) {
                        for (let i = 0; i < records.length; i++) {
                            const driver = records[i];
                            $('<option>')
                            .attr('value', driver.driverId)
                            .text(driver.driverName)
                            .prop('selected', driver.driverId.toString() === oldValue ? true : false)
                            .appendTo(dropdown);
                        }
                    }
                });
            }

            function populateTruckDropdownFromCacheOrServer(dropdown, leaseHaulerId) {
                return new Promise(function (resolve, reject) {
                    if (!leaseHaulerId) {
                        populateTruckDropdownWithRecords(dropdown, null);
                        resolve(null);
                        return;
                    }
                    if (_cache.trucks[leaseHaulerId]) {
                        populateTruckDropdownWithRecords(dropdown, _cache.trucks[leaseHaulerId]);
                        resolve(_cache.trucks[leaseHaulerId]);
                    } else {
                        _modalManager.setBusy(true);
                        _schedulingService.getLeaseHaulerTrucks({ ids: [leaseHaulerId] }).done(function (records) {
                            addTrucksToCache(records);
                            populateTruckDropdownWithRecords(dropdown, records);
                            resolve(records);
                        }).always(function () {
                            _modalManager.setBusy(false);
                        }).catch(function (e) {
                            reject(e);
                        });
                    }
                });
            }

            function populateDriverDropdownFromCacheOrServer(dropdown, leaseHaulerId) {
                return new Promise(function (resolve, reject) {
                    if (!leaseHaulerId) {
                        populateTruckDropdownWithRecords(dropdown, null);
                        resolve(null);
                        return;
                    }
                    if (_cache.drivers[leaseHaulerId]) {
                        populateDriverDropdownWithRecords(dropdown, _cache.drivers[leaseHaulerId]);
                        resolve(_cache.drivers[leaseHaulerId]);
                    } else {
                        _modalManager.setBusy(true);
                        _schedulingService.getLeaseHaulerDrivers({ ids: [leaseHaulerId] }).done(function (records) {
                            addDriversToCache(records);
                            populateDriverDropdownWithRecords(dropdown, records);
                            resolve(records);
                        }).always(function () {
                            _modalManager.setBusy(false);
                        });
                    }
                });
            }

            $leaseHaulerDropdown.on('select2:clearing', function (e) {
                handleLeaseHaulerChanging(e);
            });

            $leaseHaulerDropdown.on('select2:selecting', function (e) {
                handleLeaseHaulerChanging(e);
            });

            $truckSelectionBlockTrucks.on('select2:clearing', '.lease-hauler-truck-select', function (e) {
                handleTruckChanging(e, $(this));
            });

            $truckSelectionBlockTrucks.on('select2:selecting', '.lease-hauler-truck-select', function (e) {
                handleTruckChanging(e, $(this));
            });

            $truckSelectionBlockTrucks.on('select2:clearing', '.lease-hauler-driver-select', function (e) {
                handleDriverChanging(e, $(this));
            });

            $truckSelectionBlockTrucks.on('select2:selecting', '.lease-hauler-driver-select', function (e) {
                handleDriverChanging(e, $(this));
            });

            function isAnyRowInUse() {
                let result = false;
                $truckSelectionBlockTrucks.find('.lease-hauler-truck-select').each(function () {
                    if ($(this).attr('data-truck-isinuse') === 'True') {
                        result = true;
                        return false;
                    }
                });
                if (!result) {
                    $truckSelectionBlockTrucks.find('.lease-hauler-driver-select').each(function () {
                        if ($(this).attr('data-driver-isinuse') === 'True') {
                            result = true;
                            return false;
                        }
                    });
                }
                return result;
            }

            function handleLeaseHaulerChanging(e) {
                if (!isAnyRowInUse()) {
                    return;
                }
                e.preventDefault();
                abp.message.warn('If you want to remove or change the lease hauler, you need to remove any associated orders, dispatches, and tickets for this date.',
                    'Trucks are associated with orders, dispatches, or tickets.');
            }

            function handleTruckChanging(e, truckDropdown) {
                if (truckDropdown.attr('data-truck-isinuse') !== 'True') {
                    return;
                }
                e.preventDefault();
                abp.message.warn('If you want to remove or change the truck, you need to remove any associated orders, dispatches, and tickets for this date.',
                    'This truck is associated with orders, dispatches, or tickets.');
            }

            function handleDriverChanging(e, driverDropdown) {
                if (driverDropdown.attr('data-driver-isinuse') !== 'True') {
                    return;
                }
                e.preventDefault();
                abp.message.warn('If you want to remove or change the driver, you need to remove any associated dispatches, and tickets for this date.',
                    'This driver is associated with dispatches, or tickets.');
            }

            $leaseHaulerDropdown.change(function () {
                if (_permissions.editLeaseHaulerRequests) {
                    const leaseHaulerId = parseInt($leaseHaulerDropdown.val());
                    const truckDropdowns = $truckSelectionBlockTrucks.find('.lease-hauler-truck-select');
                    const driverDropdowns = $truckSelectionBlockTrucks.find('.lease-hauler-driver-select');
                    populateTruckDropdownFromCacheOrServer(truckDropdowns, leaseHaulerId).then(function () {
                        populateDriverDropdownFromCacheOrServer(driverDropdowns, leaseHaulerId);
                    });
                }
            });

            $truckSelectionBlockTrucks.on('change', '.lease-hauler-truck-select', function () {
                const truckDropdown = $(this);
                const row = truckDropdown.closest('.truck-selection-row');
                const defaultDriverId = truckDropdown.find('option[value="' + truckDropdown.val() + '"]').attr('default-driver-id');
                const driverDropdown = row.find('.lease-hauler-driver-select');
                ensureTruckChangeIsAllowed(truckDropdown, function () {
                    driverDropdown.val(defaultDriverId).trigger('change.select2');
                });
            });

            $truckSelectionBlockTrucks.on('change', '.lease-hauler-driver-select', function () {
                const driverDropdown = $(this);
                ensureDriverChangeIsAllowed(driverDropdown);
            });

            function ensureTruckChangeIsAllowed($select, successCallback) {
                if ($select.attr('data-truck-isinuse') === 'True' && $select.attr('data-truck-originalid') !== $select.val()) {
                    $select.val($select.attr('data-truck-originalid')).trigger('change.select2');
                } else {
                    successCallback && successCallback();
                }
            }

            function ensureDriverChangeIsAllowed($select, successCallback) {
                if ($select.attr('data-driver-isinuse') === 'True' && $select.attr('data-driver-originalid') !== $select.val()) {
                    $select.val($select.attr('data-driver-originalid')).trigger('change.select2');
                } else {
                    successCallback && successCallback();
                }
            }

            function addRow(rowsToAdd) {
                let $newRows = $();
                for (let i = 0; i < rowsToAdd; i++) {
                    const newRow = $truckSelectionRowTemplate.clone();
                    newRow.appendTo($truckSelectionBlockTrucks).show();
                    $newRows = $newRows.add(newRow);
                }
                initTruckRow($newRows);
            }

            function getLeaseHaulerId() {
                if (_permissions.editLeaseHaulerRequests) {
                    return parseInt($leaseHaulerDropdown.val());
                } else if (_permissions.leaseHaulerPortalTruckRequests) {
                    return parseInt(abp.session.leaseHaulerId);
                }
            }

            function initTruckRow(rows) {
                const promise = populateTruckDropdownFromCacheOrServer(rows.find('.lease-hauler-truck-select'), getLeaseHaulerId()).then(function () {
                    return populateDriverDropdownFromCacheOrServer(rows.find('.lease-hauler-driver-select'), getLeaseHaulerId());
                });
                rows.find('.lease-hauler-truck-select').each(function () {
                    $(this).select2Init({
                        showAll: true,
                        allowClear: true
                    });
                });

                rows.find('.lease-hauler-driver-select').each(function () {
                    $(this).select2Init({
                        showAll: true,
                        allowClear: true
                    });
                });
                return promise;
            }

            $truckSelectionBlockTrucks.on('click', '.delete-truck-selection-row-button', function () {
                const row = $(this).closest('.truck-selection-row');
                const truckDropdown = row.find('.lease-hauler-truck-select');
                if (truckDropdown.attr('data-truck-isinuse') === 'True') {
                    abp.message.warn('If you want to delete this record, you need to remove any associated orders, dispatches, and tickets for this date.',
                        'This truck is associated with orders, dispatches, or tickets.');
                    return;
                }
                row.remove();
                const availableAndTruckCountMatch = truckCount === getAvailableValue();
                truckCount = Math.max(0, truckCount - 1); //Math.max(0, getApprovedValue() - 1);
                if (availableAndTruckCountMatch) {
                    $available.val(truckCount);
                }
            });

            const truckRowsToInit = $truckSelectionBlockTrucks.find('.truck-selection-row');
            const initialValues = storeTruckAndDriverValues(truckRowsToInit);
            initTruckRow(truckRowsToInit).then(function () {
                restoreTruckAndDriverValues(initialValues);
            });

            function storeTruckAndDriverValues(rows) {
                let values = [];
                rows.find('.lease-hauler-truck-select, .lease-hauler-driver-select').each(function () {
                    const control = $(this);
                    const val = control.val();
                    values.push({
                        control: control,
                        val: val,
                        name: control.find('option[value="' + val + '"]').text()
                    });
                });
                return values;
            }

            function restoreTruckAndDriverValues(values) {
                values.forEach(function (x) {
                    if (!x.control.find('option[value="' + x.val + '"]').length) {
                        $('<option></option>').text(x.name).attr('value', x.val).appendTo(x.control);
                        x.control.val(x.val).trigger('change.select2');
                    }
                });
            }

            function getAvailableValue() {
                let val = $available.val();
                if (!val) {
                    return 0;
                }
                val = parseInt(val);
                return isNaN(val) || val < 0 ? 0 : val;
            }

            function tryGetEmptyTruckRow() {
                let emptyRow = null;
                $truckSelectionBlockTrucks.find('.truck-selection-row').each(function () {
                    if (!$(this).find('.lease-hauler-truck-select').val()) {
                        emptyRow = $(this);
                        return false;
                    }
                });
                return emptyRow;
            }
            window.tryGetEmptyTruckRow = tryGetEmptyTruckRow;

            $available.change(function () {
                const newValue = getAvailableValue();
                if (newValue === truckCount) {
                    return;
                }
                if (newValue === 0) {
                    $truckSelectionBlockTrucks.empty();
                    truckCount = 0;
                    return;
                }

                if (newValue > truckCount) {
                    const trucksToAdd = newValue - truckCount;
                    addRow(trucksToAdd);
                    truckCount = newValue;
                    return;
                }

                while (newValue < truckCount) {
                    var emptyRow = tryGetEmptyTruckRow();
                    if (!emptyRow) {
                        $available.val(truckCount);
                        abp.message.warn('Please remove the unwanted truck rows to decrease available trucks count');
                        return;
                    }
                    emptyRow.remove();
                    truckCount = truckCount - 1;
                }
                truckCount = newValue;
            });

            $available.on('input', function () {
                const newValue = $(this).val();
                if (newValue && !$truckSelectionBlock.is(":visible")) {
                    $truckSelectionBlock.show();
                    if (truckCount === 0) {
                        truckCount = 1;
                        addRow(1);
                    }
                }
            });

            _validateTrucks = function () {
                if (truckCount > getAvailableValue()) {
                    abp.message.error('Truck count cannot be higher than approved value, please increase approved value or delete unwanted trucks');
                    return false;
                }

                let isValid = true;
                let dropdownToFocusOn = null;
                $truckSelectionBlockTrucks.find('.truck-selection-row').each(function () {
                    const row = $(this);
                    const truckDropdown = row.find('.lease-hauler-truck-select');
                    const driverDropdown = row.find('.lease-hauler-driver-select');
                    if (truckDropdown.val() && !driverDropdown.val()) {
                        dropdownToFocusOn = driverDropdown;
                        isValid = false;
                        return false;
                    }
                    if (!truckDropdown.val() && driverDropdown.val()) {
                        dropdownToFocusOn = truckDropdown;
                        isValid = false;
                        return false;
                    }
                });
                if (!isValid) {
                    abp.message.error('Driver is required for rows where truck is specified').done(function () {
                        dropdownToFocusOn && dropdownToFocusOn.data('select2').focus();
                    });
                    return false;
                }
                return true;
            };
        };

        this.focusOnDefaultElement = function () {
            if (_$form.find('#Date').is(':disabled')) {
                _$form.find('#Shift').select2('focus');
            } else {
                _$form.find('#Date').focus();
            }
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            const $truckSelectionRowTemplate = _$form.find('#truckSelectionRowTemplate .truck-selection-row').detach();
            const formData = _$form.serializeFormToObject();
            formData.NumberTrucksRequested = formData.Approved;
            $("#truckSelectionRowTemplate").append($truckSelectionRowTemplate);

            if (formData.Id === '0') {
                formData.Available = formData.Approved;
            }

            if (formData.Approved !== '' && formData.Available === '') {
                abp.message.error('There is an Approved value but there is not an Available value!');
                return;
            }
            if (formData.Approved !== '' && formData.Available !== '' && parseInt(formData.Approved) > parseInt(formData.Available)) {
                abp.message.error('Approved must be less than or equal to available!');
                return;
            }

            if (_validateTrucks && !_validateTrucks()) {
                return;
            }

            _modalManager.setBusy(true);
            _leaseHaulerRequestEditAppService.editLeaseHaulerRequest(formData).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditLeaseHaulerRequestModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
