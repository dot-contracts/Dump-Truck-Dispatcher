(function ($) {
    app.modals.AddOrEditLeaseHaulerRequestModal = function () {

        var _modalManager;
        var _leaseHaulerRequestEditAppService = abp.services.app.leaseHaulerRequestEdit;
        var _schedulingService = abp.services.app.scheduling;
        var _$form = null;
        var _validateTrucks = null;
        var _cache = {
            leaseHaulers: [],
            trucks: {},
            drivers: {}
        };
        var _permissions = {
            schedule: abp.auth.hasPermission('Pages.Schedule'),
            leaseHaulerSchedule: abp.auth.hasPermission('LeaseHaulerPortal.Schedule'),
        };

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find('#Date').datepickerInit();
            _$form.find('#Shift').select2Init({ allowClear: false });

            let $requestId = _$form.find('#Id');
            var $leaseHaulerDropdown = _$form.find('#LeaseHaulerId');
            $leaseHaulerDropdown.select2Init({
                abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulersSelectList,
                showAll: false,
                allowClear: true
            });

            var $truckSelectionRowTemplate = _$form.find('#truckSelectionRowTemplate .truck-selection-row');
            var $truckSelectionBlock = _$form.find('#truckSelectionBlock');
            var $truckSelectionBlockTrucks = _$form.find('#truckSelectionBlockTrucks');
            var truckCount = $truckSelectionBlockTrucks.find('.truck-selection-row').length;
            var $numberRequested = _$form.find('#NumberTrucksRequested');

            if ($requestId.val() > 0 && $numberRequested.val() > 0) {
                $truckSelectionBlock.show();
                let newValue = $numberRequested.val();
                if (newValue > truckCount) {
                    var trucksToAdd = newValue - truckCount;
                    addRow(trucksToAdd);
                    truckCount = newValue;
                    return;
                }
            }

            function addTrucksToCache(records) {
                if (records && records.length) {
                    for (var i = 0; i < records.length; i++) {
                        var truck = records[i];
                        if (!_cache.trucks[truck.leaseHaulerId]) {
                            _cache.trucks[truck.leaseHaulerId] = [];
                        }
                        _cache.trucks[truck.leaseHaulerId].push(truck);
                    }
                }
            }

            function addDriversToCache(records) {
                if (records && records.length) {
                    for (var i = 0; i < records.length; i++) {
                        var driver = records[i];
                        if (!_cache.drivers[driver.leaseHaulerId]) {
                            _cache.drivers[driver.leaseHaulerId] = [];
                        }
                        _cache.drivers[driver.leaseHaulerId].push(driver);
                    }
                }
            }

            function populateTruckDropdownWithRecords(dropdowns, records) {
                dropdowns.each(function () {
                    var dropdown = $(this);
                    var oldValue = dropdown.val();
                    dropdown.find('option[value!=""]').remove();
                    if (records && records.length) {
                        for (var i = 0; i < records.length; i++) {
                            var truck = records[i];
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
                    var dropdown = $(this);
                    var oldValue = dropdown.val();
                    dropdown.find('option[value!=""]').remove();
                    if (records && records.length) {
                        for (var i = 0; i < records.length; i++) {
                            var driver = records[i];
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
                        if (_permissions.schedule || _permissions.leaseHaulerSchedule) {
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
                        } else {
                            resolve(null);
                        }
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
                        if (_permissions.schedule || _permissions.leaseHaulerSchedule) {
                            _modalManager.setBusy(true);
                            _schedulingService.getLeaseHaulerDrivers({ ids: [leaseHaulerId] }).done(function (records) {
                                addDriversToCache(records);
                                populateDriverDropdownWithRecords(dropdown, records);
                                resolve(records);
                            }).always(function () {
                                _modalManager.setBusy(false);
                            });
                        } else {
                            resolve(null);
                        }
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
                var result = false;
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
                var leaseHaulerId = parseInt($leaseHaulerDropdown.val());
                var truckDropdowns = $truckSelectionBlockTrucks.find('.lease-hauler-truck-select');
                var driverDropdowns = $truckSelectionBlockTrucks.find('.lease-hauler-driver-select');
                populateTruckDropdownFromCacheOrServer(truckDropdowns, leaseHaulerId).then(function () {
                    populateDriverDropdownFromCacheOrServer(driverDropdowns, leaseHaulerId);
                });
            });

            $truckSelectionBlockTrucks.on('change', '.lease-hauler-truck-select', function () {
                var truckDropdown = $(this);
                var row = truckDropdown.closest('.truck-selection-row');
                var defaultDriverId = truckDropdown.find('option[value="' + truckDropdown.val() + '"]').attr('default-driver-id');
                var driverDropdown = row.find('.lease-hauler-driver-select');
                ensureTruckChangeIsAllowed(truckDropdown, function () {
                    driverDropdown.val(defaultDriverId).trigger('change.select2');
                });
            });

            $truckSelectionBlockTrucks.on('change', '.lease-hauler-driver-select', function () {
                var driverDropdown = $(this);
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
                var $newRows = $();
                for (var i = 0; i < rowsToAdd; i++) {
                    var newRow = $truckSelectionRowTemplate.clone();
                    newRow.appendTo($truckSelectionBlockTrucks).show();
                    $newRows = $newRows.add(newRow);
                }
                initTruckRow($newRows);
            }

            function getLeaseHaulerId() {
                return parseInt($leaseHaulerDropdown.val());
            }

            function initTruckRow(rows) {
                var promise = populateTruckDropdownFromCacheOrServer(rows.find('.lease-hauler-truck-select'), getLeaseHaulerId()).then(function () {
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

            var truckRowsToInit = $truckSelectionBlockTrucks.find('.truck-selection-row');
            var initialValues = storeTruckAndDriverValues(truckRowsToInit);
            initTruckRow(truckRowsToInit).then(function () {
                restoreTruckAndDriverValues(initialValues);
            });

            function storeTruckAndDriverValues(rows) {
                var values = [];
                rows.find('.lease-hauler-truck-select, .lease-hauler-driver-select').each(function () {
                    var control = $(this);
                    var val = control.val();
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

            function getRequestedValue() {
                var val = $numberRequested.val();
                if (!val) {
                    return 0;
                }
                val = parseInt(val);
                return isNaN(val) || val < 0 ? 0 : val;
            }

            function tryGetEmptyTruckRow() {
                var emptyRow = null;
                $truckSelectionBlockTrucks.find('.truck-selection-row').each(function () {
                    if (!$(this).find('.lease-hauler-truck-select').val()) {
                        emptyRow = $(this);
                        return false;
                    }
                });
                return emptyRow;
            }
            window.tryGetEmptyTruckRow = tryGetEmptyTruckRow;

            _validateTrucks = function () {
                if (truckCount > getRequestedValue()) {
                    abp.message.error('Truck count cannot be higher than requested value, please increase requested value or delete unwanted trucks');
                    return false;
                }

                var isValid = true;
                var dropdownToFocusOn = null;
                $truckSelectionBlockTrucks.find('.truck-selection-row').each(function () {
                    var row = $(this);
                    var truckDropdown = row.find('.lease-hauler-truck-select');
                    var driverDropdown = row.find('.lease-hauler-driver-select');
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

            var $truckSelectionRowTemplate = _$form.find('#truckSelectionRowTemplate .truck-selection-row').detach();
            var formData = _$form.serializeFormToObject();
            $("#truckSelectionRowTemplate").append($truckSelectionRowTemplate);

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