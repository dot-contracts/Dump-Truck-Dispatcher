(function () {

    var _dtHelper = abp.helper.dataTables;
    var _ticketService = abp.services.app.ticket;
    var _orderService = abp.services.app.order;
    var _validateTrucksAndDrivers = abp.setting.getBoolean('App.General.ValidateDriverAndTruckOnTickets');
    var _separateItems = abp.features.isEnabled('App.SeparateMaterialAndFreightItems');
    var _allowLoadCount = abp.setting.getBoolean('App.DispatchingAndMessaging.AllowLoadCountOnHourlyJobs');
    var _allowProductionPay = abp.setting.getBoolean('App.TimeAndPay.AllowProductionPay');
    var _orderLines = [];
    var _leaseHaulers = [];
    var _tickets = [];
    var _drivers = [];
    var _trucks = [];
    var _driverAssignments = [];
    var _dailyFuelCost = null;
    var _$currentFuelCostInput = $('#CurrentFuelCost');
    var _hasOpenOrders = false;
    var _orderLineBlocks = [];
    var _leaseHaulerBlocks = [];
    var _$ticketPhotoInput = $('#TicketPhoto');
    var _expandAllPanelsButton = $('#ExpandAllPanelsButton');
    var _ticketForPhotoUpload = null;
    var _blockForPhotoUpload = null;
    var _driverIdFilterInput = null;
    var _driverIdFilter = null;
    var _initializing = 0;
    var _date = null;
    var _hideVerifiedTicketsFilterInput = $("#HideVerifiedTickets");
    var _hideVerifiedTicketsFilter = false;

    var _selectDriverModal = new app.ModalManager({
        viewUrl: abp.appPath + 'app/Tickets/SelectDriverModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Tickets/_SelectDriverModal.js',
        modalClass: 'SelectDriverModal'
    });

    var _selectDateModal = new app.ModalManager({
        viewUrl: abp.appPath + 'app/Tickets/SelectDateModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Tickets/_SelectDateModal.js',
        modalClass: 'SelectDateModal'
    });

    var _selectOfficeModal = new app.ModalManager({
        viewUrl: abp.appPath + 'app/Offices/SelectOfficeModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Offices/_SelectOfficeModal.js',
        modalClass: 'SelectOfficeModal'
    });

    var _selectOrderLineModal = new app.ModalManager({
        viewUrl: abp.appPath + 'app/Orders/SelectOrderLineToMoveTicketsByDriverToModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_SelectOrderLineToMoveTicketsByDriverToModal.js',
        modalClass: 'SelectOrderLineToMoveTicketsByDriverToModal'
    });

    var _editChargesModal = new app.ModalManager({
        viewUrl: abp.appPath + 'app/Charges/EditChargesModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Charges/_EditChargesModal.js',
        modalClass: 'EditChargesModal',
        modalSize: 'xl',
    });

    initFilterControls();
    //reloadAllData();
    $('[data-toggle="tooltip"]').tooltip();


    function saveFilterState() {
        app.localStorage.setItem('tickets_by_driver_filter', _dtHelper.getFilterData());
    }

    function loadFilterState() {
        app.localStorage.getItem('tickets_by_driver_filter', function (result) {
            var filter = result || {};

            var needToReloadData = false;
            if (filter.date) {
                $('#DateFilter').val(filter.date);
                needToReloadData = true;
            }

            if (filter.hideVerifiedTickets) {
                _hideVerifiedTicketsFilter = true;
                _hideVerifiedTicketsFilterInput.prop('checked', true);
            }

            if (needToReloadData) {
                reloadAllData();
            }
        });
    }


    function initFilterControls() {
        //Date filter
        $("#DateFilter")
            //.val(moment().format("MM/DD/YYYY"))
            //.val(moment('10/20/2021').format("MM/DD/YYYY"))
            .blur(function () {
                if (!moment($(this).val(), 'MM/DD/YYYY').isValid()) {
                    $(this).val(moment().format("MM/DD/YYYY"));
                }
            })
            .on('dp.change', function () {
                let newDate = $("#DateFilter").val();
                if (moment(newDate, 'MM/DD/YYYY').isValid() && newDate !== _date) {
                    _date = newDate;
                    saveFilterState();
                    reloadAllData();
                }
            })
            .datepicker({
                format: 'L',
                useCurrent: false,
                //viewMode: 'days',
                //keepInvalid: true,
                //defaultDate: ""
            });

        //Hide verified tickets
        _hideVerifiedTicketsFilterInput.change(function () {
            _hideVerifiedTicketsFilter = _hideVerifiedTicketsFilterInput.is(':checked');
            saveFilterState();

            _orderLineBlocks.forEach(block => {
                if (block.ui) {
                    block.ui.reloadGrid();
                    block.ui.updateVisibility();
                }
            });
            _leaseHaulerBlocks.forEach(lhBlock => {
                if (lhBlock.ui) {
                    lhBlock.ui.updateVisibility();
                    lhBlock.ui.updateTicketCounters();
                }
            });
        });

        loadFilterState();

        //debug
        window.reloadAllData = reloadAllData;
        window.renderView = renderView;
        window.getOrderLineBlocks = function () {
            return _orderLineBlocks;
        };
        window.getLeaseHaulerBlocks = function () {
            return _leaseHaulerBlocks;
        };
    }

    function initDriverFilter() {
        if (!_drivers || !_orderLineBlocks) {
            return;
        }

        if (!_driverIdFilterInput) {
            _driverIdFilterInput = $("#DriverIdFilter");
            _driverIdFilterInput.select2Init({
                showAll: true,
                allowClear: true
            }).change(function () {
                _driverIdFilter = Number(_driverIdFilterInput.val()) || null;
                _orderLineBlocks.forEach(block => {
                    block.ui && block.ui.updateVisibility();
                });
                _leaseHaulerBlocks.forEach(lhBlock => {
                    lhBlock.ui && lhBlock.ui.updateVisibility();
                    lhBlock.ui && lhBlock.ui.updateTicketCounters();
                });
            });
        }

        let driversForFilter = _drivers
            .filter(d => _orderLineBlocks.some(o => o.driverId === d.id)) //d => d.isActive
            .sort(orderDriverByName);

        if (!driversForFilter.some(d => d.id === _driverIdFilter)) {
            _driverIdFilterInput.val(null).change();
        }

        _driverIdFilterInput.find('option').not('[value=""]').remove();
        _driverIdFilterInput.append(
            driversForFilter
                .map(d => $('<option>').attr('value', d.id).text(d.name))
                .reduce((prev, curr) => prev ? prev.add(curr) : curr, $())
        );
    }

    function reloadAllDataAndThrow(e) {
        return reloadAllData({
            suppressWarnings: true
        }).then(() => {
            throw e;
        });
    }

    function reloadAllData(loadDataOptions) {
        $('#TicketList').empty();
        $('#CurrentFuelCostContainer').hide();
        _orderLines = [];
        _leaseHaulers = [];
        _tickets = [];
        _drivers = [];
        _trucks = [];
        _driverAssignments = [];
        _dailyFuelCost = null;
        _hasOpenOrders = false;
        _orderLineBlocks.forEach(function (block) {
            if (block.ui) {
                block.ui.destroy();
                block.ui.card && block.ui.card.remove();
            }
        });
        _orderLineBlocks = []; //one per unique orderLine-driver (later per unique orderLine-driver-truck)
        _leaseHaulerBlocks = [];
        return loadData(loadDataOptions);
    }

    function loadData(loadDataOptions) {
        loadDataOptions = loadDataOptions || {};
        var filter = _dtHelper.getFilterData();
        if (!filter.date) {
            _date = null;
            return;
        }
        _date = filter.date;

        abp.ui.setBusy();
        logTimeIfNeeded('sending request to getTicketsByDriver');
        return _ticketService.getTicketsByDriver(filter).then(function (result) {
            logTimeIfNeeded('received getTicketsByDriver data');
            if (result.orderLines && result.orderLines.length) {
                _orderLines = result.orderLines;
            }
            if (result.tickets && result.tickets.length) {
                _tickets = result.tickets;
            }
            if (result.drivers && result.drivers.length) {
                _drivers = result.drivers;
            }
            if (result.trucks && result.trucks.length) {
                _trucks = result.trucks;
            }
            if (result.driverAssignments && result.driverAssignments.length) {
                _driverAssignments = result.driverAssignments;
            }
            if (result.leaseHaulers && result.leaseHaulers.length) {
                _leaseHaulers = result.leaseHaulers;
            }
            _dailyFuelCost = result.dailyFuelCost;
            if (result.hasOpenOrders !== _hasOpenOrders) {
                _hasOpenOrders = result.hasOpenOrders;
                if (_hasOpenOrders && !loadDataOptions.suppressWarnings) {
                    abp.message.warn(app.localize('SomeOfTheOrdersAreStillOpenWillNotBeDisplayed'));
                }
            }

            populateLeaseHaulerBlocks(_leaseHaulers);
            populateOrderLineBlocks(_orderLines, _tickets);
            renderView();
            abp.ui.clearBusy();
        }, function (err) {
            abp.ui.clearBusy();
            throw err;
        });
    }

    function populateLeaseHaulerBlocks(leaseHaulers) {
        _leaseHaulerBlocks.push({
            leaseHaulerId: null,
            leaseHaulerName: app.localize('UnknownDriver'),
            knownDriver: false,
            ui: null
        });

        _leaseHaulerBlocks.push({
            leaseHaulerId: null,
            leaseHaulerName: app.localize('Internal'),
            knownDriver: true,
            ui: null
        });

        leaseHaulers.forEach(function (leaseHauler) {
            _leaseHaulerBlocks.push({
                leaseHaulerId: leaseHauler.id,
                leaseHaulerName: leaseHauler.name,
                knownDriver: true,
                ui: null
            });
        });
    }

    function populateOrderLineBlocks(orderLines, tickets) {
        if (orderLines) {
            logTimeIfNeeded('started to populate orderLineBlocks from OrderLines');
            orderLines.forEach(function (orderLine) {
                if (orderLine.isCancelled) {
                    return;
                }
                orderLine.orderLineTrucks.filter(olt => olt.driverId).forEach(function (olt) {
                    var existingOrderLineTruck = _orderLineBlocks.find(block => block.orderLineId === orderLine.id && block.driverId === olt.driverId);
                    if (existingOrderLineTruck) {
                        return;
                    }
                    _orderLineBlocks.push({
                        orderLineId: orderLine.id,
                        driverId: olt.driverId,
                        orderLine: orderLine,
                        driver: _drivers.find(d => d.id === olt.driverId),
                        ui: null
                    });
                });
            });
        }
        if (tickets) {
            logTimeIfNeeded('started to populate OrderLineBlocks from Tickets');
            tickets.forEach(function (ticket) {
                var existingOrderLineTruck = _orderLineBlocks.find(block => block.orderLineId === ticket.orderLineId && block.driverId === ticket.driverId);
                if (existingOrderLineTruck) {
                    return;
                }
                let orderLine = _orderLines.find(o => o.id == ticket.orderLineId);
                if (!orderLine) {
                    return; //an orderLine wasn't loaded because it's not closed yet
                }
                _orderLineBlocks.push({
                    orderLineId: ticket.orderLineId,
                    driverId: ticket.driverId,
                    orderLine: orderLine,
                    driver: _drivers.find(d => d.id === ticket.driverId),
                    ui: null
                });
            });
        }
        logTimeIfNeeded('started to sort orderLineBlocks by driver name');
        _orderLineBlocks.sort((a, b) => {
            if (a.driver == b.driver) {
                return 0;
            }
            let driverA = (a.driver && a.driver.name || '').toUpperCase();
            let driverB = (b.driver && b.driver.name || '').toUpperCase();
            if (driverA < driverB) {
                return -1;
            }
            if (driverA > driverB) {
                return 1;
            }
            let customerA = (a.orderLine.customerName || '').toUpperCase();
            let customerB = (b.orderLine.customerName || '').toUpperCase();
            if (customerA < customerB) {
                return -1;
            }
            if (customerA > customerB) {
                return 1;
            }
            return 0;
        });
        logTimeIfNeeded('started to init driver filter');
        initDriverFilter();
        logTimeIfNeeded('finished populating OrderLineBlocks (in-memory)');
    }

    function orderDriverByName(a, b) {
        let driverA = (a.name || '').toUpperCase();
        let driverB = (b.name || '').toUpperCase();
        if (driverA < driverB) {
            return -1;
        }
        if (driverA > driverB) {
            return 1;
        }
        return 0;
    }

    function findBlockByTicket(ticket) {
        var driverId = ticket.driver && ticket.driver.id || null;
        var orderLineId = ticket.orderLineId;
        return _orderLineBlocks.find(b => b.orderLine.id === orderLineId && b.driverId === driverId);
    }

    function getTicketsForLeaseHaulerBlock(lhBlock) {
        var leaseHaulerId = lhBlock.leaseHaulerId;
        if (lhBlock.knownDriver) {
            let matchingDriverIds = _drivers.filter(d => d.leaseHaulerId === leaseHaulerId && (!_driverIdFilter || _driverIdFilter === d.id)).map(d => d.id);
            var tickets = _tickets.filter(t => matchingDriverIds.includes(t.driverId));
            return tickets;
        } else {
            var tickets = _tickets.filter(t => !_driverIdFilter && t.driverId === null && leaseHaulerId === null);
            return tickets;
        }
    }

    function getTicketsForOrderLineBlock(block, applyFilters) {
        var orderLineId = block.orderLine && block.orderLine.id;
        var driverId = block.driver && block.driver.id || null;
        if (!orderLineId) {
            return [];
        }
        var tickets = _tickets.filter(t => t.orderLineId === orderLineId && t.driverId === driverId && (!applyFilters || !_hideVerifiedTicketsFilter || t.isVerified === false));
        return tickets;
    }

    function getTicketsForOrderLine(orderLine) {
        var orderLineId = orderLine && orderLine.id;
        if (!orderLineId) {
            return [];
        }
        var tickets = _tickets.filter(t => t.orderLineId === orderLineId);
        return tickets;
    }

    async function throwIfOrderOfficeCannotBeChanged(block) {
        try {
            abp.ui.setBusy(block.ui.card);
            await _orderService.throwIfOrderOfficeCannotBeChanged(block.orderLine.orderId);
        } finally {
            abp.ui.clearBusy(block.ui.card);
        }
    }

    function saveChanges(model) {
        return _ticketService.editTicketsByDriver(model).then(saveResult => {
            abp.notify.info('Successfully saved');
            return saveResult;
        }, (e) => reloadAllDataAndThrow(e));
    }

    async function saveOrderLine(orderLine) {
        var affectedBlocks = _orderLineBlocks.filter(o => o.orderLine.id === orderLine.id);
        affectedBlocks.forEach(x => x.ui && abp.ui.setBusy(x.ui.card));

        try {
            var saveResult = await saveChanges({
                orderLines: [orderLine]
            });

            var needToUpdateAffectedOrderLineBlocks = false;

            if (saveResult?.orderLines?.length === 1) {
                if (orderLine.fuelSurchargeRate !== saveResult.orderLines[0].fuelSurchargeRate) {
                    orderLine.fuelSurchargeRate = saveResult.orderLines[0].fuelSurchargeRate;
                    needToUpdateAffectedOrderLineBlocks = true;
                }
            } else {
                await reloadAllData();
                return;
            }

            if (needToUpdateAffectedOrderLineBlocks) {
                var affectedBlocks = _orderLineBlocks.filter(o => o.orderLine.id === orderLine.id);
                affectedBlocks.forEach(function (affectedBlock) {
                    updateCardFromModel(affectedBlock);
                    refreshFieldHighlighting(affectedBlock);
                });
            }

            return saveResult;
        } catch (e) {
            await reloadAllDataAndThrow(e);
        } finally {
            affectedBlocks.forEach(x => x.ui && abp.ui.clearBusy(x.ui.card));
        }
    }

    function saveTicket(ticket) {
        return saveChanges({
            tickets: [ticket]
        }).then((saveResult) => {
            if (saveResult?.tickets?.length === 1) {
                ticket.id = saveResult.tickets[0].id;
                //ticket.ticketDateTime = saveResult.tickets[0].ticketDateTime;
            } else {
                return reloadAllData();
            }
            return saveResult;
        }, (e) => reloadAllDataAndThrow(e));
    }

    function updateCardFromModel(block) {
        if (!block.ui) {
            return;
        }
        let leaseHaulerId = block.driver?.leaseHaulerId ?? null;
        _initializing++;
        setInputOrDropdownValue(block.ui.driver, block.driver && block.driver.id, block.driver && block.driver.name);
        setInputOrDropdownValue(block.ui.designation, block.orderLine.designation, abp.enums.designationName[block.orderLine.designation]);
        setInputOrDropdownValue(block.ui.customer, block.orderLine.customerId, block.orderLine.customerName);
        block.ui.orderId.val(block.orderLine.orderId);
        block.ui.jobNumber.val(block.orderLine.jobNumber);
        block.ui.poNumber.val(block.orderLine.poNumber);
        setInputOrDropdownValue(block.ui.salesTaxEntity, block.orderLine.salesTaxEntityId, block.orderLine.salesTaxEntityName);
        block.ui.productionPay.prop('checked', block.orderLine.productionPay);
        block.ui.productionPay.closest('.form-group').toggle(
            _allowProductionPay
            && !abp.enums.designations.materialOnly.includes(block.orderLine.designation)
            && !leaseHaulerId
        );
        setInputOrDropdownValue(block.ui.loadAt, block.orderLine.loadAtId, block.orderLine.loadAtName);
        setInputOrDropdownValue(block.ui.deliverTo, block.orderLine.deliverToId, block.orderLine.deliverToName);

        if (_separateItems) {
            setInputOrDropdownValue(block.ui.freightItem, block.orderLine.freightItemId, block.orderLine.freightItemName);
            setInputOrDropdownValue(block.ui.materialItem, block.orderLine.materialItemId, block.orderLine.materialItemName);
        } else {
            setInputOrDropdownValue(block.ui.item, block.orderLine.freightItemId, block.orderLine.freightItemName);
        }
        setInputOrDropdownValue(block.ui.freightUom, block.orderLine.freightUomId, block.orderLine.freightUomName);
        setInputOrDropdownValue(block.ui.materialUom, block.orderLine.materialUomId, block.orderLine.materialUomName);

        block.ui.freightRate.val(block.orderLine.freightRate);

        if (_separateItems) {
            if (block.orderLine.designation === abp.enums.designation.materialOnly) {
                block.ui.freightItem.closest('.form-group').hide();
            }
        }

        block.ui.freightUom.closest('.form-group').toggle(
            !abp.enums.designations.materialOnly.includes(block.orderLine.designation)
        );

        block.ui.freightRateToPayDrivers.val(block.orderLine.freightRateToPayDrivers);
        block.ui.freightRateToPayDrivers.closest('.form-group').toggle(
            !leaseHaulerId
            && !abp.enums.designations.materialOnly.includes(block.orderLine.designation)
            && abp.setting.getBoolean('App.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate')
            && block.orderLine.productionPay);
        block.ui.materialRate.val(block.orderLine.materialRate);
        block.ui.leaseHaulerRate.val(block.orderLine.leaseHaulerRate);
        block.ui.leaseHaulerRate.closest('.form-group').toggle(
            !!leaseHaulerId
            && abp.setting.getBoolean('App.LeaseHaulers.ShowLeaseHaulerRateOnOrder'));
        block.ui.fuelSurchargeRate.val(block.orderLine.fuelSurchargeRate);
        block.ui.totalCharges.val(_dtHelper.renderMoneyUnrounded(getTotalChargesForBlock(block)));
        setInputOrDropdownValue(block.ui.office, block.orderLine.officeId, block.orderLine.officeName);
        block.ui.office.prop('disabled', abp.setting.getBoolean('App.General.SplitBillingByOffices'));

        if (block.orderLine.isMaterialTotalOverridden || block.orderLine.isFreightTotalOverridden) {
            block.ui.clickableWarningIcon
                .attr('title', getClickableWarningHoverText(block))
                .show()
                .click(() => {
                    askToResetOrderLineOverrideFlags(block);
                });
        } else {
            block.ui.clickableWarningIcon
                .attr('title', '')
                .hide()
                .off('click');
        }

        updateCardReadOnlyState(block);
        if (abp.auth.hasPermission('Pages.TicketsByDriver.EditTicketsOnInvoicesOrPayStatements')
            && block.isReadOnly
            && !block.overrideReadOnlyState
        ) {
            block.ui.overrideReadOnlyStateButton
                .show()
                .click(() => {
                    askToOverrideReadOnlyState(block);
                });
        } else {
            block.ui.overrideReadOnlyStateButton
                .hide()
                .off('click');
        }

        if (block.orderLine.note) {
            block.ui.orderLineNoteIcon
                .prop('title', abp.utils.replaceAll(block.orderLine.note, '\n', '<br>'))
                .tooltip()
                .show();
        } else {
            block.ui.orderLineNoteIcon
                .hide();
        }

        var noteIcons = $();
        var notes = block.orderLine.orderLineTrucks.filter(olt => olt.driverId === block.driverId && olt.driverNote).map(x => x.driverNote);
        notes.forEach(note => {
            let icon = $('<i class="fa-regular fa-files directions-icon color-green" data-toggle="tooltip" data-html="true"></i>');
            icon.prop('title', abp.utils.replaceAll(note, '\n', '<br>'));
            noteIcons = noteIcons.add(icon);
        });
        block.ui.driverNoteIconsContainer.empty().append(noteIcons);
        noteIcons.tooltip();

        _initializing--;
    }

    function getTotalChargesForBlock(block) {
        const orderLine = block.orderLine;
        const charges = orderLine.charges;
        const tickets = getTicketsForOrderLine(orderLine);
        let totalCharges = 0;
        for (let charge of charges) {
            if (charge.useMaterialQuantity) {
                const quantity = tickets.map(ticket => ticket.materialQuantity || 0).reduce((a, b) => a + b, 0);
                totalCharges += (quantity * charge.rate) || 0;
            } else {
                totalCharges += charge.chargeAmount;
            }
        }
        return totalCharges;
    }

    function getClickableWarningHoverText(block) {
        let warningText = '';
        if (block.orderLine.isMaterialTotalOverridden || block.orderLine.isFreightTotalOverridden) {
            warningText += app.localize('OrderItemHasOverriddenTotalValues') + ': ' + getOverriddenOrderLineTotals(block);
        }
        return warningText;
    }

    function getOverriddenOrderLineTotals(block) {
        let totals = [
            block.orderLine.isMaterialTotalOverridden ? `Material: ${block.orderLine.materialTotal}` : null,
            block.orderLine.isFreightTotalOverridden ? `Freight: ${block.orderLine.freightTotal}` : null,
        ];
        return totals.filter(x => x).join(', ');
    }

    async function askToResetOrderLineOverrideFlags(block) {
        if (!await abp.message.confirm('', app.localize('ClearOverriddenValues{0}Prompt', getOverriddenOrderLineTotals(block)))) {
            return;
        }
        var affectedBlocks = _orderLineBlocks.filter(o => o.orderLine.id === block.orderLine.id);
        try {
            affectedBlocks.forEach(x => x.ui && abp.ui.setBusy(x.ui.card));
            let result = await _orderService.resetOverriddenOrderLineValues({
                id: block.orderLine.id,
                overrideReadOnlyState: block.overrideReadOnlyState || false
            });
            block.orderLine.materialTotal = result.materialTotal;
            block.orderLine.freightTotal = result.freightTotal;
            block.orderLine.isFreightTotalOverridden = false;
            block.orderLine.isMaterialTotalOverridden = false;
            affectedBlocks.forEach(function (affectedBlock) {
                updateCardFromModel(affectedBlock);
            });
        } finally {
            affectedBlocks.forEach(x => x.ui && abp.ui.clearBusy(x.ui.card));
        }
    }

    async function askToOverrideReadOnlyState(block) {
        if (!await abp.message.confirm('', app.localize('OverrideReadOnlyStateOfOrderLineBlockPrompt'))) {
            return;
        }
        block.overrideReadOnlyState = true;
        updateCardReadOnlyState(block);
        updateCardFromModel(block);
        block.ui.reloadGrid();
    }

    function setInputOrDropdownValue(inputControl, idValue, textValue) {
        if (inputControl.is('select')) {
            abp.helper.ui.addAndSetDropdownValue(inputControl, idValue, textValue);
        } else if (inputControl.is('input')) {
            inputControl.val(textValue);
        }
    }

    async function handleBlockDropdownChangeAsync(block, dropdown, additionalValidationCallback) {
        var newId = Number(dropdown.val()) || null;
        var newName = newId ? dropdown.getSelectedDropdownOption().text() : null;
        var idField = dropdown.attr('name');
        var nameField = dropdown.data('nameField');

        var result = {
            newId,
            newName,
            idField,
            nameField,
            oldId: block.orderLine[idField],
            oldName: block.orderLine[nameField],
        };

        if (newId === block.orderLine[idField]) {
            return null;
        }

        if (additionalValidationCallback) {
            if (!await additionalValidationCallback(result)) {
                abp.helper.ui.addAndSetDropdownValue(dropdown, block.orderLine[idField], block.orderLine[nameField]);
                return null;
            }
        }

        return result;
    }

    async function handleBlockNumberChangeAsync(block, input, additionalValidationCallback) {
        var newValue = Number(input.val()) || 0;
        var field = input.attr('name');

        if (input.attr('data-rule-min')) {
            let min = Number(input.attr('data-rule-min'));
            newValue = newValue < min ? min : newValue;
        }
        if (input.attr('data-rule-max')) {
            let max = Number(input.attr('data-rule-max'));
            newValue = newValue > max ? max : newValue;
        }

        if (additionalValidationCallback) {
            if (!await additionalValidationCallback(newValue)) {
                input.val(block.orderLine[field] || 0);
                return null;
            }
        }

        return {
            field,
            newValue
        };
    }

    async function handleBlockTextChangeAsync(block, input, additionalValidationCallback) {
        var newValue = input.val() || '';
        var field = input.attr('name');

        if (input.attr('maxlength')) {
            let maxLength = Number(input.attr('maxlength'));
            if (newValue.length > maxLength) {
                newValue = newValue.substring(0, maxLength);
            }
        }

        if (additionalValidationCallback) {
            if (!await additionalValidationCallback(newValue)) {
                input.val(block.orderLine[field] || '');
                return null;
            }
        }

        return {
            field,
            newValue
        };
    }

    function refreshFieldHighlighting(block) {
        if (block.ui) {
            let rates = ['freightRate', 'materialRate'];
            for (let rate of rates) {
                let highlight = !isRateValidForDesignation(rate, block);
                block.ui[rate]
                    .toggleClass('highlight-yellow', highlight)
                    .attr('title', highlight ? app.localize('RateDoesntMatchDesignation') : '');
            }
        }
    }

    function isRateValidForDesignation(field, block) {
        let currentValue = block.orderLine[field] || 0;
        let designation = block.orderLine.designation;
        switch (field) {
            case 'freightRate':
                if (currentValue) {
                    return !abp.enums.designations.materialOnly.includes(designation);
                } else {
                    return abp.enums.designations.materialOnly.includes(designation);
                }
                break;

            case 'materialRate':
                if (currentValue) {
                    return !abp.enums.designations.freightOnly.includes(designation);
                } else {
                    return abp.enums.designations.freightOnly.includes(designation);
                }
                break;
        }
        return true;
    }

    function getFieldsToUpdateFromDropdownEditResult(dropdownEditResult) {
        var { newId, newName, idField, nameField } = dropdownEditResult;
        var result = [];
        if (idField) {
            result.push({ field: idField, newValue: newId });
        }
        if (nameField) {
            result.push({ field: nameField, newValue: newName });
        }
        return result;
    }

    /**
     * @param block - the order line / driver block that was changed
     * @param {{ field: string, newValue: any }[]} newValues - the new values for the fields
     */
    function updateAffectedOrderLineBlocks(block, newValues, includeTickets, ticketFilter) {
        updateAffectedBlocks(block, newValues, includeTickets, ticketFilter, b => b.orderLine, (a, b) => a.id === b.id);
    }

    function updateAffectedOrderBlocks(block, newValues, includeTickets, ticketFilter) {
        updateAffectedBlocks(block, newValues, includeTickets, ticketFilter, b => b.orderLine, (a, b) => a.orderId === b.orderId);
    }

    function updateAffectedBlocks(block, newValues, includeTickets, ticketFilter, entitySelector, filter) {
        var affectedBlocks = _orderLineBlocks.filter(o => filter(entitySelector(o), entitySelector(block)));
        affectedBlocks.forEach(function (affectedBlock) {
            let entity = entitySelector(affectedBlock);
            for (let { field, newValue } of newValues) {
                entity[field] = newValue;
            }

            if (includeTickets) {
                let tickets = getTicketsForOrderLineBlock(affectedBlock);
                if (ticketFilter) {
                    tickets = tickets.filter(ticketFilter);
                }
                tickets.forEach(t => {
                    for (let { field, newValue } of newValues) {
                        let ticketField = mapOrderLineFieldToTicketField(field);
                        t[ticketField] = newValue;
                    }
                });

                if (affectedBlock.ui) {
                    affectedBlock.ui.reloadGrid();
                }
            }

            updateCardFromModel(affectedBlock);
            refreshFieldHighlighting(affectedBlock);
        });
    }

    function mapOrderLineFieldToTicketField(fieldName) {
        //if (!_separateItems) {
        //    switch (fieldName) {
        //        case 'materialUomId':
        //            return 'freightUomId';
        //        case 'materialUomName':
        //            return 'freightUomName';
        //    }
        //}
        return fieldName;
    }

    async function reloadBlocksAfterTicketTransfer(sourceBlock, tickets, newDriverId) {
        //reload this grid (should become empty)
        sourceBlock.ui.reloadTickets();
        sourceBlock.ui.updateVisibility();
        //revert driver dropdown value on the source block
        updateCardFromModel(sourceBlock);
        //create a new panel if needed
        populateOrderLineBlocks(null, tickets);

        //destroy an old orderLineBlock if needed
        let sourceOrderLine = _orderLines.find(ol => ol.id === sourceBlock.orderLineId);
        let sourceTickets = getTicketsForOrderLineBlock(sourceBlock);
        let sourceLhBlock = sourceBlock.leaseHaulerBlock;
        if (!sourceOrderLine.orderLineTrucks.some(olt => olt.driverId === sourceBlock.driverId) && !sourceTickets.length) {
            sourceBlock.ui.destroy();
            sourceBlock.ui.card && sourceBlock.ui.card.remove();
            _orderLineBlocks = _orderLineBlocks.filter(b => b !== sourceBlock);
            if (sourceLhBlock) {
                sourceLhBlock.orderLineBlocks = sourceLhBlock.orderLineBlocks.filter(x => x !== sourceBlock);
            }
        }
        if (sourceLhBlock) {
            sourceLhBlock.ui.updateVisibility();
            sourceLhBlock.ui.updateTicketCounters();
        }
        let driver = _drivers.find(d => d.id === newDriverId);
        let leaseHaulerId = driver && driver.leaseHaulerId;
        _leaseHaulerBlocks.filter(lhBlock => lhBlock.leaseHaulerId === leaseHaulerId && lhBlock.knownDriver === !!driver).forEach(lhBlock => {
            lhBlock.ui && lhBlock.ui.initOrderLineBlocks();
            lhBlock.ui && lhBlock.ui.updateVisibility();
            lhBlock.ui && lhBlock.ui.updateTicketCounters();
        });


        renderView();
        //reload the grid of the existing or new panel
        let targetBlock = _orderLineBlocks.find(o => o.orderLine.id === sourceBlock.orderLine.id && (o.driver && o.driver.id || null) === newDriverId);
        if (targetBlock) {
            //it's now possible to transfer orderline blocks between LH blocks (only from Unknown Driver block to other blocks), so we are updating all affected lhBlocks
            //if (targetBlock.leaseHaulerBlock && targetBlock.leaseHaulerBlock.ui.isExpanded) {
            //    targetBlock.leaseHaulerBlock.ui.initOrderLineBlocks();
            //}
            if (targetBlock.leaseHaulerBlock) {
                if (!targetBlock.leaseHaulerBlock.ui.isExpanded) {
                    await targetBlock.leaseHaulerBlock.ui.toggle(true);
                }
            }
            if (targetBlock.ui) {
                targetBlock.ui.reloadTickets();
                await focusOnBlock(targetBlock);
            }
        }
    }

    function sleepAsync(ms) {
        return new Promise((resolve) => setTimeout(() => resolve(), ms));
    }

    async function focusOnBlock(block) {
        if (block.ui) {
            //focusOnPlaceholderOrDropdown(block.ui.driver);

            _initializing++;
            if (block.ui.driver.is('input')) {
                block.ui.driver.focus();
                await sleepAsync(50);
                block.ui.driver.is('select') && block.ui.driver.select2('focus');
            } else if (block.ui.driver.is('select')) {
                block.ui.driver.select2('focus');
            }
            await sleepAsync(50);
            _initializing--;
        }
    }

    function replaceDropdownPlaceholderWithDropdownOnFocus(block, uiField, callback) {
        let ui = block.ui;
        ui[uiField].on('focus', function () {
            ui[uiField] = replaceDropdownPlaceholderWithDropdown(ui[uiField]);
            updateCardFromModel(block);
            callback(ui[uiField]);
            if (!_initializing) {
                ui[uiField].select2('open');
            }
        });
    }

    function isTicketQuantityFilled(ticket) {
        return ticket.materialQuantity > 0 || ticket.freightQuantity > 0;
    }

    function updateTicketCounters(ui, tickets, orderLineBlocks) {
        let verifiedCount = tickets.filter(t => t.isVerified).length;
        let enteredCount = tickets.filter(t => !t.isVerified && isTicketQuantityFilled(t) && t.ticketNumber).length;
        let missingCount = tickets.filter(t => !t.isVerified && (!isTicketQuantityFilled(t) || !t.ticketNumber)).length; //tickets.length - verifiedCount - enteredCount;
        setTicketCounterValue(ui.verifiedTicketCount, verifiedCount);
        setTicketCounterValue(ui.enteredTicketCount, enteredCount);
        setTicketCounterValue(ui.missingTicketCount, missingCount);
        if (orderLineBlocks) {
            //leahse hauler grouping level
            let emptyBlockCount = orderLineBlocks.filter(b => getTicketsForOrderLineBlock(b).length === 0).length;
            setTicketCounterValue(ui.emptyOrderLineBlockCount, emptyBlockCount);
        } else {
            //orderline-driver grouping level
            ui.emptyOrderLineBlockCount.toggle(tickets.length === 0);
        }
    }

    function setTicketCounterValue(uiControl, count) {
        uiControl.text(count).toggle(count > 0);
    }

    function getOrderLineBlocksForLeaseHaulerBlock(lhBlock) {
        return _orderLineBlocks.filter(block => block.driver && block.driver.leaseHaulerId === lhBlock.leaseHaulerId && lhBlock.knownDriver
            || !block.driver && lhBlock.leaseHaulerId === null && !lhBlock.knownDriver);
    }

    function updateCardReadOnlyState(block) {
        let orderLineTickets = getTicketsForOrderLine(block.orderLine);
        let isReadOnly = !block.overrideReadOnlyState && orderLineTickets.some(t => t.isReadOnly);
        if (block.isReadOnly === isReadOnly) {
            return;
        }
        block.isReadOnly = isReadOnly;

        let allControls = [
            block.ui.driver,
            block.ui.customer,
            //block.ui.orderId, //always readonly
            block.ui.jobNumber,
            block.ui.loadAt,
            block.ui.deliverTo,
            block.ui.freightRate,
            block.ui.freightRateToPayDrivers,
            block.ui.materialRate,
            block.ui.designation,
            block.ui.poNumber,
            block.ui.salesTaxEntity,
            block.ui.productionPay,
            block.ui.leaseHaulerRate,
            block.ui.freightUom,
            block.ui.materialUom,
            //block.ui.fuelSurchargeRate //always readonly
            //block.ui.totalCharges //always readonly
            //block.ui.office //probably should remain editable even for readonly tickets (and disabled even for some editable tickets), there's a separate logic that allows or denies changing the office
        ];

        if (_separateItems) {
            allControls.push(block.ui.freightItem, block.ui.materialItem);
        } else {
            allControls.push(block.ui.item);
        }

        if (isReadOnly
            || block.overrideReadOnlyState
            || !block.orderLine.quoteId
            || abp.auth.hasPermission('Pages.Orders.EditQuotedValues')
        ) {
            allControls.forEach(c => c.prop('disabled', isReadOnly));
        } else {
            let availableControls = [
                block.ui.driver,
                block.ui.loadAt,
                block.ui.deliverTo,
            ];
            allControls.forEach(c => c.prop('disabled', !availableControls.includes(c)));
        }

        let blockTickets = getTicketsForOrderLineBlock(block);
        if (block.overrideReadOnlyState && blockTickets.some(t => t.isReadOnly)) {
            blockTickets.forEach(t => t.isReadOnly = false);
            block.ui.reloadGrid();
        }
    }

    async function checkTruckAssignment(block, truckId, driverId) {
        return;
        let noOrderLineTruck = truckId
            && !block.orderLine.orderLineTrucks
                .some(olt => olt.truckId === truckId);
        if (noOrderLineTruck) {
            if (!await abp.message.confirm(app.localize('DriverWasntAssignedToThisTruckAreYouSure'))) {
                throw new Error('Save was cancelled');
            }
            block.orderLine.orderLineTrucks.push({
                truckId: truckId,
                driverId: driverId,
            });
        }
    }

    function isTicketEmpty(ticket) {
        return !ticket.ticketNumber
            && !ticket.ticketDateTime
            && !isTicketQuantityFilled(ticket)
            && !ticket.truckId;
    }

    function getAssignedTruckForBlock(block) {
        let driverId = block.driver && block.driver.id || null;
        if (!driverId || !block.orderLine) {
            return null;
        }
        let orderLineTrucks = block.orderLine.orderLineTrucks.filter(x => x.driverId === driverId);
        let truckId = null;
        if (orderLineTrucks.length === 1) {
            truckId = orderLineTrucks[0].truckId;
        }
        if (orderLineTrucks.length) {
            truckId = orderLineTrucks
                .reduce((a, b) => a.id > b.id ? a : b, orderLineTrucks[0].id)
                .truckId;
        }
        if (truckId) {
            return _trucks.find(x => x.id === truckId);
        }
        let driverAssignment = _driverAssignments.find(x => x.driverId === driverId && x.shift === block.orderLine.shift);
        if (driverAssignment) {
            return _trucks.find(x => x.id === driverAssignment.truckId);
        }
        return _trucks.find(x => x.defaultDriverId === driverId);
    }

    function getAssignedTrailerForTruckAndBlock(truck, block) {
        let driverId = block.driver && block.driver.id || null;
        if (!truck || !truck.canPullTrailer || !block.orderLine || !driverId) {
            return null;
        }
        let orderLineTrucks = block.orderLine.orderLineTrucks.filter(x => x.driverId === driverId && x.truckId === truck.id);
        let trailerId = null;
        if (orderLineTrucks.length === 1) {
            trailerId = orderLineTrucks[0].trailerId;
        }
        if (orderLineTrucks.length) {
            trailerId = orderLineTrucks
                .reduce((a, b) => a.id > b.id ? a : b, orderLineTrucks[0].id)
                .trailerId;
        }
        if (trailerId) {
            return _trucks.find(x => x.id === trailerId);
        }
        if (truck.currentTrailerId) {
            return _trucks.find(x => x.id === truck.currentTrailerId);
        }
        return null;
    }

    function getEmptyTicket(block) {
        let truck = getAssignedTruckForBlock(block);
        let trailer = getAssignedTrailerForTruckAndBlock(truck, block);
        return {
            id: 0,
            orderLineId: block.orderLine.id,
            driverId: block.driver && block.driver.id || null,
            officeId: block.orderLine.officeId,
            officeName: block.orderLine.officeName,
            ticketNumber: '',
            ticketDateTime: block.orderLine.orderDate,
            isVerified: false,
            nonbillableFreight: abp.enums.designations.materialOnly.includes(block.orderLine.designation),
            nonbillableMaterial: abp.enums.designations.freightOnly.includes(block.orderLine.designation),
            freightQuantity: 0,
            materialQuantity: 0,
            freightUomId: block.orderLine.freightUomId,
            freightUomName: block.orderLine.freightUomName,
            materialUomId: block.orderLine.materialUomId,
            materialUomName: block.orderLine.materialUomName,
            freightItemId: block.orderLine.freightItemId,
            freightItemName: block.orderLine.freightItemName,
            materialItemId: !_separateItems ? null : block.orderLine.materialItemId,
            materialItemName: !_separateItems ? null : block.orderLine.materialItemName,
            truckId: truck && truck.id || null,
            truckCode: truck && truck.truckCode || null,
            trailerId: trailer && trailer.id || null,
            trailerTruckCode: trailer && trailer.truckCode || null,
            ticketPhotoId: null,
            receiptLineId: null,
            isReadOnly: false,
            loadCount: null,
        };
    }

    function logTimeIfNeeded(message) {
        if (localStorage.getItem('logTicketsByDrivers')) {
            console.log(moment().toISOString() + ' ' + message);
        }
    }

    function renderView() {
        var mainContainer = $('#TicketList');

        logTimeIfNeeded('started rendering');

        if (abp.features.isEnabled('App.AllowLeaseHaulersFeature')) {

            let newLhBlocks = [];
            _leaseHaulerBlocks.forEach(lhBlock => {
                if (lhBlock.ui) {
                    return;
                }
                renderLeaseHaulerBlock(lhBlock);
                newLhBlocks.push(lhBlock);
            });

            logTimeIfNeeded('started to append LH blocks');
            if (newLhBlocks.length) {
                mainContainer.append(newLhBlocks.map(lhBlock => lhBlock.ui.card));
            }

            setTimeout(() => {

                logTimeIfNeeded('(async) started to init missing orderLineBlocks for expanded LH blocks');
                _leaseHaulerBlocks.forEach(lhBlock => lhBlock.ui && lhBlock.ui.isExpanded && lhBlock.ui.initOrderLineBlocks());

                logTimeIfNeeded('(async) started to updateTicketCounters');
                newLhBlocks.forEach(lhBlock => lhBlock.ui.updateTicketCounters());

                if (_leaseHaulerBlocks.length === newLhBlocks.length) {
                    let visibleLhBlocks = newLhBlocks.filter(x => x.ui.isVisible);
                    if (visibleLhBlocks.length === 1 && visibleLhBlocks[0].leaseHaulerId === null && visibleLhBlocks[0].knownDriver) {
                        visibleLhBlocks[0].ui.toggle(true);
                    }
                }

                //logTimeIfNeeded('(async) started to update visibility');
                //newLhBlocks.forEach(function (lhBlock) {
                //    lhBlock.ui.updateVisibility();
                //});
                logTimeIfNeeded('(async) finished async rendering tasks');
            }, 0);
        } else {
            renderOrderLineBlocks(_orderLineBlocks, mainContainer);
        }

        updateCurrentFuelCostContainer();

        logTimeIfNeeded('finished rendering');
    }

    function updateCurrentFuelCostContainer() {
        if (!abp.setting.getBoolean('App.Fuel.ShowFuelSurcharge')) {
            $('#CurrentFuelCostContainer').hide();
            return;
        }

        $('#CurrentFuelCostContainer').show();
        $('#CurrentFuelCost').val(getDailyFuelCostFormatted());

        _$currentFuelCostInput.prop("disabled", true);
        $('#EditCurrentFuelCostButton').prop("disabled", false);
    }

    function getDailyFuelCostFormatted() {
        return (_dailyFuelCost && _dailyFuelCost.cost || 0).toFixed(2);
    }

    $('#EditCurrentFuelCostButton').click(function () {
        $(this).prop("disabled", true);
        _$currentFuelCostInput.prop("disabled", false);
        _$currentFuelCostInput.focus();
    });

    _$currentFuelCostInput.blur(async function () {
        let newValue = $(this).val();
        let oldValue = getDailyFuelCostFormatted();

        if (newValue === oldValue) {
            updateCurrentFuelCostContainer();
            return;
        }
        if (!newValue) {
            abp.notify.error('Current Fuel Cost is required!');
            updateCurrentFuelCostContainer();
            return;
        }
        if (isNaN(newValue)) {
            abp.notify.error('Please enter a valid number!');
            updateCurrentFuelCostContainer();
            return;
        }
        if (parseFloat(newValue) < 0) {
            abp.notify.error('Please enter a valid positive number!');
            updateCurrentFuelCostContainer();
            return;
        }

        try {
            abp.ui.setBusy($('#CurrentFuelCostContainer'));
            _dailyFuelCost = await _ticketService.editCurrentFuelCost({
                date: $("#DateFilter").val(),
                cost: newValue
            });
            abp.notify.info('Saved successfully.');
            return reloadAllData();
        } finally {
            abp.ui.clearBusy($('#CurrentFuelCostContainer'));
            updateCurrentFuelCostContainer();
        }

    });

    function renderOrderLineBlocks(orderLineBlocks, parentContainer) {
        logTimeIfNeeded('started rendering orderLineBlocks');

        let newBlocks = [];

        orderLineBlocks.forEach(function (block) {
            if (block.ui) {
                return;
            }
            renderOrderLineBlock(block);
            newBlocks.push(block);
        });

        logTimeIfNeeded('started to append');
        if (newBlocks.length) {
            parentContainer.append(newBlocks.map(block => block.ui.card));
        }

        setTimeout(() => {
            logTimeIfNeeded('(async) started to updateTicketCounters');
            newBlocks.forEach((block, i) => block.ui.updateTicketCounters(i === 0));

            logTimeIfNeeded('(async) finished async rendering tasks');
        }, 0);
    }

    function renderLeaseHaulerBlock(lhBlock) {
        var ui = {};
        lhBlock.ui = ui;

        ui.card = $('<div class="card card-collapsable card-collapse bg-superlight mb-4">').append(
            $('<div class="card-header bg-superlight">').append(
                $('<div class="m-form m-form--label-align-right">').append(
                    $('<div class="row align-items-center">').append(
                        $('<div class="col-lg-3 col-md-4 col-sm-6 pt-2">').append(
                            $('<h5>').text(lhBlock.leaseHaulerName)
                        )
                    ).append(
                        $('<div class="col-lg-7 col-md-4 col-sm-10 col-8 pb-2">').append(
                            renderTicketCounts(ui, 'leaseHauler')
                        )
                    ).append(
                        renderToggleButton(ui)
                    )
                )
            )
        ).append(
            ui.body = $('<div class="card-body pb-0 d-none">')
        );


        lhBlock.ui.updateTicketCounters = function () {
            let tickets = getTicketsForLeaseHaulerBlock(lhBlock);
            updateTicketCounters(lhBlock.ui, tickets, lhBlock.orderLineBlocks.filter(b => !_driverIdFilter || _driverIdFilter === b.driverId));
        };

        lhBlock.orderLineBlocks = getOrderLineBlocksForLeaseHaulerBlock(lhBlock);
        lhBlock.orderLineBlocks.forEach(block => block.leaseHaulerBlock = lhBlock);
        lhBlock.ui.hasBeenExpanded = false;

        lhBlock.ui.initOrderLineBlocks = function () {
            let moreOrderLineBlocks = getOrderLineBlocksForLeaseHaulerBlock(lhBlock);
            moreOrderLineBlocks.forEach(block => {
                if (!lhBlock.orderLineBlocks.includes(block)) {
                    lhBlock.orderLineBlocks.push(block);
                    block.leaseHaulerBlock = lhBlock;
                }
            });
            renderOrderLineBlocks(lhBlock.orderLineBlocks, lhBlock.ui.body);
        };

        lhBlock.ui.isExpanded = false;
        var slideIsInProgress = false;
        let togglePromiseResolves = [];
        lhBlock.ui.toggle = function (isExpanded) {
            return new Promise((resolve) => {
                togglePromiseResolves.push(resolve);
                if (slideIsInProgress) {
                    return;
                }
                if (isExpanded === undefined) {
                    isExpanded = !lhBlock.ui.isExpanded;
                }

                if (lhBlock.ui.isExpanded === isExpanded) {
                    return;
                }

                slideIsInProgress = true;
                let hasBeenExpanded = lhBlock.ui.hasBeenExpanded;
                lhBlock.ui.isExpanded = isExpanded;

                var card = lhBlock.ui.card;
                if (isExpanded) { //card.hasClass('card-collapse')
                    lhBlock.ui.hasBeenExpanded = true;

                    var slideDown = function () {
                        logTimeIfNeeded('(async) started slide down');
                        lhBlock.ui.body.slideDown({
                            complete: () => {
                                logTimeIfNeeded('(async) finished slide down');
                                slideIsInProgress = false;
                                let resolvers = togglePromiseResolves;
                                togglePromiseResolves = [];
                                resolvers.forEach(x => x());
                            }
                        });
                    };
                    var initOrderLineBlocks = function () {
                        logTimeIfNeeded('started to init order line blocks for LH');
                        lhBlock.ui.initOrderLineBlocks();
                    };
                    var setBusyIfNeededAnd = function (callback) {
                        if (!hasBeenExpanded && lhBlock.orderLineBlocks.length > 20) {
                            abp.ui.setBusy(lhBlock.ui.card);
                            callback && setTimeout(callback, 200);
                        } else {
                            callback && callback();
                        }
                    };
                    var clearBusyIfNeededAnd = function (callback) {
                        if (!hasBeenExpanded) {
                            setTimeout(() => {
                                abp.ui.clearBusy(lhBlock.ui.card);
                                callback && callback();
                            }, 0);
                        } else {
                            callback && callback();
                        }
                    };

                    logTimeIfNeeded('started expanding order line block, set busy');
                    setBusyIfNeededAnd(() => {
                        initOrderLineBlocks();
                        setTimeout(slideDown, 0);
                        clearBusyIfNeededAnd(() => {
                            logTimeIfNeeded('finished initializing order line blocks for LH, clearing busy');
                        });
                    });

                } else {
                    lhBlock.ui.isExpanded = false;
                    lhBlock.ui.body.slideUp({
                        complete: () => {
                            slideIsInProgress = false;
                            let resolvers = togglePromiseResolves;
                            togglePromiseResolves = [];
                            resolvers.forEach(x => x());
                        }
                    });
                }
                card.toggleClass('card-collapse', !isExpanded);
            });
        };

        lhBlock.ui.toggleCardDetailsButton.click(function (e) {
            e.preventDefault();
            lhBlock.ui.toggle();
        });

        lhBlock.ui.isVisible = true;
        lhBlock.ui.updateVisibility = function () {
            let hasChildren = lhBlock.orderLineBlocks && lhBlock.orderLineBlocks.filter(shouldOrderLineBlockBeVisible).length > 0 || false;
            lhBlock.ui.isVisible = hasChildren;
            lhBlock.ui.card.toggle(hasChildren);
        };

        lhBlock.ui.updateVisibility();
    }

    function renderOrderLineBlock(block) {
        var ui = {};
        block.ui = ui;

        ui.card = $('<div class="card card-collapsable card-collapse bg-light mb-4">').append(
            $('<div class="card-header bg-light">').append(
                $('<div class="m-form m-form--label-align-right">').append(
                    ui.form = $('<form>')
                )
            )
        ).append(
            ui.body = $('<div class="card-body py-0 d-none">').append(
                ui.table = $('<table class="table table-striped table-bordered table-hover order-line-tickets-table"></table>')
            ).append(
                $('<div class="row">').append(
                    $('<div class="col-sm-12 d-flex justify-content-end mb-3">').append(
                        ui.addTicketRowButton = $('<button type="button" class="btn btn-primary">').text(app.localize('Add'))
                    )
                )
            )
        );

        ui.form.append(
            $('<div class="d-flex justify-content-end"></div>').append(
                renderOrderLineNoteIcon(ui),
                renderOverrideReadOnlyStateButton(ui)
            )
        ).append(
            $('<div class="row align-items-center">').append(
                renderDropdownPlaceholder(ui, 'driver', app.localize('Driver'), 'driverId')
            ).append(
                renderDropdownPlaceholder(ui, 'customer', app.localize('Customer'), 'customerId', 'customerName')
            ).append(
                renderDisabledInput(ui, 'orderId', app.localize('OrderId'), 'orderId', '')
            ).append(
                renderTextInput(ui, 'jobNumber', app.localize('JobNbr'), 'jobNumber', abp.entityStringFieldLengths.orderLine.jobNumber)
            ).append(
                renderDropdownPlaceholder(ui, 'loadAt', app.localize('LoadAt'), 'loadAtId', 'loadAtName')
            ).append(
                renderDropdownPlaceholder(ui, 'deliverTo', app.localize('DeliverTo'), 'deliverToId', 'deliverToName')
            ).append(
                renderDropdownPlaceholder(ui, 'item', app.localize('Item'), 'freightItemId', 'freightItemName')
                    .toggle(!_separateItems)
            ).append(
                renderDropdownPlaceholder(ui, 'freightItem', app.localize('FreightItem'), 'freightItemId', 'freightItemName')
                    .toggle(_separateItems)
            ).append(
                renderDropdownPlaceholder(ui, 'materialItem', app.localize('MaterialItem'), 'materialItemId', 'materialItemName')
                    .toggle(_separateItems)
            ).append(
                renderDropdownPlaceholder(ui, 'freightUom', app.localize('Freight UOM'), 'freightUomId', 'freightUomName')
            ).append(
                renderDropdownPlaceholder(ui, 'materialUom', app.localize('Material UOM'), 'materialUomId', 'materialUomName')
            ).append(
                renderRateInput(ui, 'freightRate', app.localize('FreightRate'), 'freightRate')
            ).append(
                renderRateInput(ui, 'freightRateToPayDrivers', app.localize('FreightRateToPayDriversShort'), 'freightRateToPayDrivers')
                    .toggle(abp.setting.getBoolean('App.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate'))
            ).append(
                renderRateInput(ui, 'materialRate', app.localize('MaterialRate'), 'materialRate')
            ).append(
                renderDropdownPlaceholder(ui, 'designation', app.localize('Designation'), 'designation')
            ).append(
                renderTextInput(ui, 'poNumber', app.localize('PONumber'), 'poNumber', abp.entityStringFieldLengths.order.poNumber)
            ).append(
                renderDropdownPlaceholder(ui, 'salesTaxEntity', app.localize('TaxName'), 'salesTaxEntityId', 'salesTaxEntityName')
            ).append(
                renderCheckbox(ui, 'productionPay', app.localize('ProductionPay'), 'productionPay')
            ).append(
                renderRateInput(ui, 'leaseHaulerRate', app.localize('LHRate'), 'leaseHaulerRate')
                    .toggle(abp.setting.getBoolean('App.LeaseHaulers.ShowLeaseHaulerRateOnOrder'))
            ).append(
                renderDisabledInput(ui, 'fuelSurchargeRate', app.localize('FuelSurchargeRate'), 'fuelSurchargeRate', '')
                    .toggle(abp.setting.getBoolean('App.Fuel.ShowFuelSurcharge'))
            ).append(
                renderDisabledInput(ui, 'totalCharges', app.localize('Charges'), 'totalCharges', '')
                    .toggle(abp.auth.hasPermission('Pages.Charges'))
            ).append(
                renderButton(ui, 'editChargesButton', app.localize('AddOrEditCharges'))
                    .toggle(abp.auth.hasPermission('Pages.Charges'))
            ).append(
                renderOfficeInput(ui)
            ).append(
                $('<div class="form-group col-lg-2 col-md-4 col-sm-1 d-sm-none-">').append(
                    renderClickableWarningIcon(ui)
                ).append(
                    renderDriverNoteIconsContainer(ui)
                )
            )
        ).append(
            $('<div class="row align-items-center">').append(
                $('<div class="col-lg-10 col-md-8 col-sm-10 col-8 pb-2">').append(
                    renderTicketCounts(ui, 'orderLine')
                )
            ).append(
                renderToggleButton(ui)
            )
        );

        updateCardFromModel(block);
        refreshFieldHighlighting(block);

        replaceDropdownPlaceholderWithDropdownOnFocus(block, 'driver', dropdown => {
            let leaseHaulerId = (block.driver && block.driver.leaseHaulerId) || null;
            block.ui.driver.append(
                _drivers
                    .filter(d => d.isActive
                        && (d.leaseHaulerId === leaseHaulerId || !block.driver)
                        && d.id !== (block.driver && block.driver.id))
                    .map(d => $('<option>').attr('value', d.id).text(d.name))
                    .reduce((prev, curr) => prev ? prev.add(curr) : curr, $())
                //.forEach(d => block.ui.driver.append(d));
            ).select2Init({
                showAll: true,
                allowClear: false
            }).change(function () {
                if (_initializing) {
                    return;
                }
                var newDriverId = Number(block.ui.driver.val()) || null;
                //var newDriver = newDriverId ? _drivers.find(d => d.id === newDriverId) : null;
                let tickets = getTicketsForOrderLineBlock(block).filter(t => !isTicketEmpty(t));
                tickets.forEach(t => t.driverId = newDriverId);
                saveChanges({
                    tickets: tickets
                }).then((saveResult) => {
                    reloadBlocksAfterTicketTransfer(block, tickets, newDriverId);
                });
            });
        });

        replaceDropdownPlaceholderWithDropdownOnFocus(block, 'customer', dropdown => {
            block.ui.customer.select2Init({
                abpServiceMethod: abp.services.app.customer.getActiveCustomersSelectList,
                allowClear: false
            }).change(async function () {
                if (_initializing) {
                    return;
                }

                var additionalValidationCallback = async (newValue) => {
                    if (block.orderLine.quoteId) {
                        if (!await abp.message.confirm(app.localize('JobWasCreatedFromQuoteAndWillBeUncoupledPrompt'))) {
                            return false;
                        }
                    }
                    if (!await validateOrderFieldChangeAsync(block)) {
                        return false;
                    }

                    return true;
                };

                var result = await handleBlockDropdownChangeAsync(block, $(this), additionalValidationCallback);
                if (!result) {
                    return;
                }

                if (block.orderLine.quoteId) {
                    block.orderLine.quoteId = null;
                }
                updateAffectedOrderBlocks(block, getFieldsToUpdateFromDropdownEditResult(result));

                await saveOrderLine(block.orderLine);
            });
        });

        replaceDropdownPlaceholderWithDropdownOnFocus(block, 'loadAt', dropdown => {
            block.ui.loadAt.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownLoadSite,
                showAll: false,
                allowClear: true
            }).change(async function () {
                if (_initializing) {
                    return;
                }
                var result = await handleBlockDropdownChangeAsync(block, $(this));
                if (!result) {
                    return;
                }
                updateAffectedOrderLineBlocks(block, getFieldsToUpdateFromDropdownEditResult(result));
                await saveOrderLine(block.orderLine);
            });
        });

        replaceDropdownPlaceholderWithDropdownOnFocus(block, 'deliverTo', dropdown => {
            block.ui.deliverTo.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownDeliverySite,
                showAll: false,
                allowClear: true
            }).change(async function () {
                if (_initializing) {
                    return;
                }
                var result = await handleBlockDropdownChangeAsync(block, $(this));
                if (!result) {
                    return;
                }
                updateAffectedOrderLineBlocks(block, getFieldsToUpdateFromDropdownEditResult(result));
                await saveOrderLine(block.orderLine);
            });
        });

        let itemInputs = _separateItems
            ? [
                { name: 'freightItem', types: abp.enums.itemTypes.freight },
                { name: 'materialItem', types: abp.enums.itemTypes.material },
            ] : [
                { name: 'item', types: null },
            ];

        for (const itemInput of itemInputs) {
            replaceDropdownPlaceholderWithDropdownOnFocus(block, itemInput.name, dropdown => {
                dropdown.select2Init({
                    abpServiceMethod: listCacheSelectLists.item(),
                    abpServiceParamsGetter: (params) => ({
                        types: itemInput.types,
                    }),
                    showAll: true,
                    allowClear: false
                }).change(async function () {
                    if (_initializing) {
                        return;
                    }

                    var additionalValidationCallback = undefined;
                    if (itemInput.name === 'materialItem') {
                        additionalValidationCallback = async (newValue) => {
                            if (!block.orderLine.materialItemId && newValue) {
                                if (!await abp.message.confirm(app.localize('SpecifyingMaterialItemWhenEmptyWillOverwriteTicketsPrompt'))) {
                                    return false;
                                }
                            }

                            return true;
                        };
                    }

                    var result = await handleBlockDropdownChangeAsync(block, $(this), additionalValidationCallback);
                    if (!result) {
                        return;
                    }
                    updateAffectedOrderLineBlocks(block, getFieldsToUpdateFromDropdownEditResult(result), true);
                    await saveOrderLine(block.orderLine);
                });
            });
        }

        let uomInputs = [
            { name: 'freightUom' },
            { name: 'materialUom' },
        ];

        for (const uomInput of uomInputs) {
            replaceDropdownPlaceholderWithDropdownOnFocus(block, uomInput.name, dropdown => {
                dropdown.select2Uom().change(async function () {
                    if (_initializing) {
                        return;
                    }
                    let result = await handleBlockDropdownChangeAsync(block, $(this));
                    if (!result) {
                        return;
                    }
                    showUomWarningIfNeeded(result);

                    let ticketFilter = (t) =>
                        t.freightUomId === result.oldId
                        || t.materialUomId === result.oldId;
                    updateAffectedOrderLineBlocks(block, getFieldsToUpdateFromDropdownEditResult(result), true, ticketFilter);

                    await saveOrderLine(block.orderLine);
                });
            });
        }

        replaceDropdownPlaceholderWithDropdownOnFocus(block, 'designation', dropdown => {
            abp.ui.initDesignationDropdown(block.ui.designation, block.orderLine.designation);
            block.ui.designation.change(async function () {
                if (_initializing) {
                    return;
                }
                var result = await handleBlockDropdownChangeAsync(block, $(this));
                if (!result) {
                    return;
                }
                updateAffectedOrderLineBlocks(block, getFieldsToUpdateFromDropdownEditResult(result), true);
                await saveOrderLine(block.orderLine);
            });
        });

        replaceDropdownPlaceholderWithDropdownOnFocus(block, 'office', dropdown => {
            block.ui.office.select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true,
                allowClear: false
            }).change(async function () {
                if (_initializing) {
                    return;
                }
                await throwIfOrderOfficeCannotBeChanged(block);
                var result = await handleBlockDropdownChangeAsync(block, $(this));
                if (!result) {
                    return;
                }
                updateAffectedOrderBlocks(block, getFieldsToUpdateFromDropdownEditResult(result), true);
                await saveOrderLine(block.orderLine);
            });
        });

        replaceDropdownPlaceholderWithDropdownOnFocus(block, 'salesTaxEntity', dropdown => {
            block.ui.salesTaxEntity.select2Init({
                abpServiceMethod: listCacheSelectLists.taxRate(),
                showAll: true,
                allowClear: true
            }).change(async function () {
                if (_initializing) {
                    return;
                }
                var result = await handleBlockDropdownChangeAsync(block, $(this));
                if (!result) {
                    return;
                }
                updateAffectedOrderBlocks(block, getFieldsToUpdateFromDropdownEditResult(result), true);
                await saveOrderLine(block.orderLine);
            });
        });

        //handle all orderline text fields
        block.ui.jobNumber.focusout(async function () {
            if (_initializing) {
                return;
            }
            var field = $(this).attr('name');
            if ($(this).val() === block.orderLine[field]) {
                return;
            }

            var result = await handleBlockTextChangeAsync(block, $(this));
            if (!result) {
                return;
            }

            updateAffectedOrderLineBlocks(block, [result]);
            await saveOrderLine(block.orderLine);
        });

        //handle all order text fields
        block.ui.poNumber.focusout(async function () {
            if (_initializing) {
                return;
            }
            var field = $(this).attr('name');
            if ($(this).val() === block.orderLine[field]) {
                return;
            }

            var additionalValidationCallback = async (newValue) => {
                if (!await validateOrderFieldChangeAsync(block)) {
                    return false;
                }

                return true;
            };

            var result = await handleBlockTextChangeAsync(block, $(this), additionalValidationCallback);
            if (!result) {
                return;
            }

            updateAffectedOrderBlocks(block, [result]);
            await saveOrderLine(block.orderLine);
        });

        //handle all orderline checkboxes
        block.ui.productionPay.change(async function () {
            if (_initializing) {
                return;
            }
            var field = $(this).attr('name');
            var newValue = $(this).is(':checked');
            if (newValue === block.orderLine[field]) {
                return;
            }

            updateAffectedOrderLineBlocks(block, [{ field, newValue }]);
            await saveOrderLine(block.orderLine);
        });

        block.ui.freightRate.add(
            block.ui.freightRateToPayDrivers
        ).add(
            block.ui.materialRate
        ).focusout(async function () {
            if (_initializing) {
                return;
            }
            var input = $(this);
            var field = input.attr('name');

            var additionalValidationCallback = async (newValue) => {
                if (newValue === block.orderLine[field]) {
                    return false;
                }
                if (!await validateNewRateAsync(newValue, field, input, block)) {
                    return false;
                }
                await syncFreightRateToPayDriversIfNeeded(newValue, field, input, block);
                return true;
            };

            var result = await handleBlockNumberChangeAsync(block, input, additionalValidationCallback);
            if (!result) {
                return;
            }

            updateAffectedOrderLineBlocks(block, [result]);

            await saveOrderLine(block.orderLine);
        });

        block.ui.leaseHaulerRate.focusout(async function () {
            if (_initializing) {
                return;
            }

            var field = $(this).attr('name');

            var additionalValidationCallback = async (newValue) => {
                if (newValue === block.orderLine[field]) {
                    return false;
                }
                return true;
            };

            var result = await handleBlockNumberChangeAsync(block, $(this), additionalValidationCallback);
            if (!result) {
                return;
            }

            updateAffectedOrderLineBlocks(block, [result]);

            await saveOrderLine(block.orderLine);
        });

        block.ui.editChargesButton.click(async function () {
            _editChargesModal.open({
                orderLineId: block.orderLine.id,
            });
        });

        var validateNewRateAsync = async function (newValue, field, input, block) {
            let oldValue = block.orderLine[field] || 0;
            if (field === 'freightRateToPayDrivers') {
                let freightRate = block.orderLine.freightRate || 0;
                if (newValue && !oldValue && !freightRate) {
                    abp.message.error('Freight rate has to be specified first');
                    return false;
                }
                return true;
            }

            return true;
        };

        var validateOrderFieldChangeAsync = async function (block) {
            if (!block.orderLine.hasMultipleOrderLines) {
                return true;
            }
            return await abp.message.confirm(app.localize('OrderHasMultipleLineItemsPrompt'));
        };

        var syncFreightRateToPayDriversIfNeeded = async function (newValue, field, input, block) {
            let oldValue = block.orderLine[field] || 0;
            if (field === 'freightRate') {
                if (!newValue
                    || oldValue === (block.orderLine.freightRateToPayDrivers)
                    || !abp.setting.getBoolean('App.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate')
                ) {
                    block.orderLine.freightRateToPayDrivers = newValue;
                    var affectedBlocks = _orderLineBlocks.filter(o => o.orderLine.id === block.orderLine.id);
                    affectedBlocks.forEach(function (affectedBlock) {
                        affectedBlock.orderLine.freightRateToPayDrivers = block.orderLine.freightRateToPayDrivers;
                    });
                }
            }
        };

        block.ui.destroyGrid = function () {
            if (!block.ui.grid) {
                return;
            }
            block.ui.grid.destroy();
            block.ui.grid = null;
        };

        let saveQueueForIsVerified = new _dtHelper.editors.DelayedSaveQueue({
            delay: 200,
            setBusyOnSave: true,
            saveCallback: async function (updatedRows, cells) {
                await _ticketService.setIsVerifiedForTickets(updatedRows).then(() => {
                    abp.notify.info('Successfully saved');
                }, (e) => reloadAllDataAndThrow(e));
            }
        });

        let refreshTicketDateWarning = function (ticket, cell) {
            let orderDateMoment = moment(block.orderLine.orderDate, ['YYYY-MM-DDTHH:mm:ss']);
            let ticketDateMoment = ticket.ticketDateTime && moment(ticket.ticketDateTime, ['YYYY-MM-DDTHH:mm:ss']);
            if (ticketDateMoment && !orderDateMoment.isSame(ticketDateMoment, 'day')) {
                if (cell.find('.input-validation-icon').length) {
                    return;
                }
                cell.find('input')
                    .addClass('padding-left-25')
                    .parent()
                    .addClass('input-validation-icon-container')
                    .append(
                        $('<i class="fa-regular fa-exclamation-circle text-danger input-validation-icon">').attr('title', app.localize('TicketDateIsDifferentFromOrder'))
                    );
            } else {
                cell.find('input')
                    .removeClass('padding-left-25')
                    .parent()
                    .removeClass('input-validation-icon-container');
                cell.find('.input-validation-icon').remove();
            }
        };

        block.ui.initGrid = function () {
            if (block.ui.grid) {
                return;
            }
            block.ui.grid = block.ui.table.DataTableInit({
                paging: false,
                info: false,
                ordering: false,
                language: {
                    emptyTable: app.localize("NoDataInTheTableClickAddBelow")
                },
                ajax: function (data, callback, settings) {
                    var tickets = getTicketsForOrderLineBlock(block, true);
                    callback(_dtHelper.fromAbpResult({
                        items: tickets,
                        totalCount: tickets.length
                    }));
                },
                editable: {
                    singleThreadSave: true, //to prevent getting duplicate tickets when they edit multiple columns of a new ticket too fast
                    saveCallback: async function (rowData, cell) {
                        block.ui.updateTicketCounters();
                        let oldId = rowData.id;
                        try {
                            block.ui.addTicketRowButton.prop('disabled', true); //otherwise, if they click on "Add" during the long save, the grid will redraw and the waiting indicators will be lost, which will allow them to continue editing the tickets that are still in the progress of saving
                            await saveTicket(rowData);
                            updateCardFromModel(block);
                        } finally {
                            block.ui.addTicketRowButton.prop('disabled', false);
                        }
                        if (rowData.id !== oldId) {
                            $(cell).closest('tr').find('td.actions').html(getTicketGridActionButton(rowData));
                        }
                    },
                    isReadOnly: (rowData) => rowData.isReadOnly
                },
                columns: [
                    {
                        width: '20px',
                        className: 'control responsive',
                        orderable: false,
                        render: function () {
                            return '';
                        }
                    },
                    {
                        data: "isVerified",
                        title: "Verified",
                        className: "checkbox-only-cell",
                        editable: {
                            editor: _dtHelper.editors.checkbox,
                            isReadOnly: (rowData, rowIsReadOnly) => false, //the column is always editable, ignore the row's readonly state
                            saveCallback: function (rowData, cell) { //override grid's saveCallback
                                block.ui.updateTicketCounters();
                                if (rowData.id) {
                                    saveQueueForIsVerified.add(rowData, cell);
                                }
                            },
                            addHeaderCheckbox: true
                        }
                    },
                    {
                        data: "nonbillableFreight",
                        title: abp.helper.ui.nonbillableFreightIcon,
                        className: "checkbox-only-cell",
                        editable: {
                            editor: _dtHelper.editors.checkbox,
                            addHeaderCheckbox: false,
                        },
                    },
                    {
                        data: "nonbillableMaterial",
                        title: abp.helper.ui.nonbillableMaterialIcon,
                        className: "checkbox-only-cell",
                        editable: {
                            editor: _dtHelper.editors.checkbox,
                            addHeaderCheckbox: false,
                        },
                    },
                    {
                        data: "ticketDateTime",
                        title: "Time",
                        render: function (data, type, full, meta) {
                            return _dtHelper.renderActualUtcDateTime(full.ticketDateTime, '');
                        },
                        className: "all ticketDateTimeColumnWithWarningIcon",
                        width: "170px",
                        editable: {
                            editor: _dtHelper.editors.datetime,
                            editCompleteCallback: function (editResult, rowData, cell) {
                                refreshTicketDateWarning(rowData, cell);
                            }
                            //this was needed for "time" editor, not needed for "datetime" editor
                            //convertDisplayValueToData: function (displayValue, rowData) {
                            //    let ticket = rowData;
                            //    let newTime = moment(displayValue, ['YYYY-MM-DDTHH:mm:ss', 'hh:mm A']);
                            //    let dateToUse = ticket.ticketDateTime ? moment(ticket.ticketDateTime, ['YYYY-MM-DDTHH:mm:ss']).startOf('day') : moment(block.orderLine.orderDate, ['YYYY-MM-DDTHH:mm:ss']);
                            //    let newValue = dateToUse.set({
                            //        hour: newTime.hour(),
                            //        minute: newTime.minute()
                            //    });
                            //    return newValue.format('YYYY-MM-DDTHH:mm:ss');
                            //}
                        },
                        createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                            refreshTicketDateWarning(rowData, $(cell));
                        }
                    },
                    {
                        data: "ticketNumber",
                        title: "Ticket Number",
                        className: "cell-text-wrap all",
                        editable: {
                            editor: _dtHelper.editors.text,
                            required: true,
                            maxLength: 20,
                            validate: async (rowData, newValue) => {
                                const isDuplicatedTicketNumber = await _ticketService.isTicketNumberExisted(newValue);
                                if (isDuplicatedTicketNumber === true) {
                                    await abp.message.info("This ticket number may be a duplicate. Please verify that it is correct.");
                                }

                                return true;
                            }
                        }
                    },
                    {
                        data: "freightQuantity",
                        title: abp.helper.getVisibleTicketControls(block.orderLine).materialQuantity ? "Freight Quantity" : "Quantity",
                        className: "all",
                        visible: abp.helper.getVisibleTicketControls(block.orderLine).freightQuantity,
                        editable: {
                            editor: _dtHelper.editors.quantity
                        }
                    },
                    {
                        data: "materialQuantity",
                        title: abp.helper.getVisibleTicketControls(block.orderLine).freightQuantity ? "Material Quantity" : "Quantity",
                        className: "all",
                        visible: abp.helper.getVisibleTicketControls(block.orderLine).materialQuantity,
                        editable: {
                            editor: _dtHelper.editors.quantity
                        }
                    },
                    {
                        data: "materialItemName",
                        title: "Material Item",
                        visible: _separateItems && abp.helper.getVisibleTicketControls(block.orderLine).materialItem,
                        editable: {
                            editor: _dtHelper.editors.dropdown,
                            idField: 'materialItemId',
                            nameField: 'materialItemName',
                            dropdownOptions: {
                                abpServiceMethod: listCacheSelectLists.item(),
                                abpServiceParamsGetter: (params) => ({
                                    types: abp.enums.itemTypes.material,
                                }),
                                showAll: listCache.item.isEnabled,
                                allowClear: true,
                            }
                        }
                    },
                    {
                        data: "truckCode",
                        title: "Truck",
                        width: "90px",
                        className: "all",
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
                                    //orderLineId: _validateTrucksAndDrivers ? _orderLineId : null
                                },
                                showAll: false,
                                allowClear: false
                            },
                            validate: async function (rowData, newId) {
                                try {
                                    newId = Number(newId) || null;
                                    await checkTruckAssignment(block, newId, rowData.driverId);
                                } catch {
                                    return false;
                                }
                                return true;
                            },
                        }
                    },
                    {
                        data: 'trailerTruckCode',
                        title: 'Trailer',
                        width: '90px',
                        className: 'all',
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
                                },
                                showAll: false,
                                allowClear: true
                            },
                        },
                    },
                    {
                        data: 'officeName',
                        title: 'Office',
                        width: '90px',
                        className: 'all',
                        visible: abp.features.isEnabled('App.AllowMultiOfficeFeature')
                            && abp.setting.getBoolean('App.General.ShowOfficeOnTicketsByDriver') //this line wasn't spec'd
                            && abp.setting.getBoolean('App.General.SplitBillingByOffices'),
                        editable: {
                            editor: _dtHelper.editors.dropdown,
                            isReadOnly: (rowData, rowIsReadOnly) => rowData.isInvoiced || rowData.hasLeaseHaulerStatement, //this intentionally doesn't include 'rowIsReadOnly &&' since the office readonly rules are separate
                            idField: 'officeId',
                            nameField: 'officeName',
                            dropdownOptions: {
                                abpServiceMethod: listCacheSelectLists.office(),
                                showAll: true,
                                allowClear: false
                            },
                        },
                    },
                    {
                        data: 'loadCount',
                        title: 'Load Count',
                        visible: _allowLoadCount,
                        className: "all",
                        editable: {
                            editor: _dtHelper.editors.decimal,
                            maxValue: app.consts.maxDecimal,
                            minValue: 0,
                            allowNull: true,
                            validate: function (rowData, newValue) {
                                let isNumber = /^\d+$/.test(newValue);
                                if (newValue !== null && !isNumber) {
                                    abp.message.error('Please enter a valid number!');
                                    return false;
                                }
                                return true;
                            },
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
                            return getTicketGridActionButton(full);
                        }
                    }
                ]
            });

        };

        function getTicketGridActionButton(ticket) {
            let uploadButtonCaption = ticket.ticketPhotoId ? 'Replace image' : 'Add image';
            return '<div class="dropdown action-button">'
                + '<ul class="dropdown-menu dropdown-menu-right">'
                + (ticket.isReadOnly || !ticket.id ? '' : '<li><a class="btnChangeDriver dropdown-item"><i class="fas fa-user-friends"></i> Change driver</a></li>')
                + (ticket.isReadOnly || !ticket.id ? '' : '<li><a class="btnChangeDate dropdown-item"><i class="fas fa-calendar"></i> Change ticket date</a></li>')
                + `<li><a class="btnUploadTicketPhotoForRow dropdown-item"><i class="fa-regular fa-file-image"></i> ${uploadButtonCaption}</a></li>`
                + (ticket.ticketPhotoId ? '<li><a class="showTicketPhotoButton dropdown-item"><i class="fa-regular fa-file-image"></i> View image</a></li>' : '')
                + (ticket.isReadOnly || ticket.id === 0 || ticket.ticketNumber !== '' ? '' : '<li><a class="btnGenerateTicketNumber dropdown-item"><i class="fas fa-hashtag"></i> Generate ticket number</a></li>')
                + (ticket.ticketPhotoId ? '<li><a class="btnDeleteTicketPhotoForRow dropdown-item"><i class="fa-regular fa-file-image"></i> Delete image</a></li>' : '')
                + (ticket.isReadOnly ? '' : '<li><a class="btnDeleteRow dropdown-item" title="Delete"><i class="fa fa-trash"></i> Delete entire ticket</a></li>')
                + '</ul>'
                + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                + '</div>';
        }

        block.ui.reloadGrid = function () {
            return new Promise(resolve => {
                if (block.ui.grid) {
                    block.ui.grid.ajax.reload(() => resolve(), /*resetPaging*/ false);
                } else {
                    resolve();
                }
            });
        };

        block.ui.updateTicketCounters = function (updateParent) {
            if (updateParent === undefined) {
                updateParent = true;
            }
            let tickets = getTicketsForOrderLineBlock(block);
            updateTicketCounters(block.ui, tickets);
            if (block.leaseHaulerBlock && updateParent) {
                block.leaseHaulerBlock.ui.updateTicketCounters();
            }
        };

        block.ui.reloadTickets = function () {
            block.ui.reloadGrid();
            block.ui.updateTicketCounters();
        };

        block.ui.changeOfficeForAllTrucksButton.click(async function (e) {
            e.preventDefault();
            await throwIfOrderOfficeCannotBeChanged(block);
            let selectedOffice = await app.getModalResultAsync(
                _selectOfficeModal.open()
            );
            updateAffectedOrderBlocks(block, [
                { field: 'officeId', newValue: selectedOffice.id },
                { field: 'officeName', newValue: selectedOffice.name },
            ], true);
            await saveOrderLine(block.orderLine);
        });

        block.ui.moveTicketsFromBlockToDifferentJob.click(async function (e) {
            e.preventDefault();
            let tickets = getTicketsForOrderLineBlock(block);
            if (tickets.some(x => x.isInvoiced || x.hasLeaseHaulerStatement)) {
                abp.message.warn(app.localize('TicketsHaveBeenInvoicesdOrOnLHPayStatements_TicketsCantBeMovedToDifferentOffice'));
                return;
            }
            let newOrderLine = await app.getModalResultAsync(
                _selectOrderLineModal.open({
                    deliveryDate: block.orderLine.orderDate,
                    customerId: block.orderLine.customerId,
                    customerName: block.orderLine.customerName,
                })
            );
            try {
                abp.ui.setBusy();
                await _ticketService.moveTicketsToOrderLine({
                    fromDriverId: block.driver?.id || null,
                    fromOrderLineId: block.orderLine.id,
                    toOrderLineId: newOrderLine.orderLineId,
                });
                abp.notify.info('Saved successfully.');
                await reloadAllData();
            } finally {
                abp.ui.clearBusy();
            }
        });

        var slideIsInProgress = false;
        block.ui.toggleCardDetailsButton.click(function (e) {
            e.preventDefault();
            if (slideIsInProgress) {
                return;
            }
            slideIsInProgress = true;
            var card = block.ui.card;
            if (card.hasClass('card-collapse')) {
                block.ui.initGrid();
                block.ui.body.slideDown({
                    complete: () => {
                        slideIsInProgress = false;
                    }
                });
            } else {
                block.ui.body.slideUp({
                    complete: () => {
                        block.ui.destroyGrid();
                        slideIsInProgress = false;
                    }
                });
            }
            card.toggleClass('card-collapse');
        });

        block.ui.addTicketRow = async function (focusOnCell) {
            if (block.orderLine.isMaterialTotalOverridden || block.orderLine.isFreightTotalOverridden) {
                let tickets = getTicketsForOrderLineBlock(block);
                if (tickets.length >= 1) {
                    abp.message.error(app.localize('OrderLineWithOverriddenTotalCanOnlyHaveSingleTicketError'));
                    return;
                }
            }

            _tickets.push(getEmptyTicket(block));
            await block.ui.reloadGrid();
            block.ui.updateTicketCounters();
            if (focusOnCell) {
                block.ui.table.find('tbody tr').last().find('td.cell-editable').first().find('input').focus();
            }
        };

        block.ui.addTicketRowButton.click(function () {
            block.ui.addTicketRow(true);
        });

        block.ui.table.on('keydown', '.dropdown.action-button', function (e) {
            var charCode = e.which || e.keyCode;
            if (charCode === 9) {
                var row = $(this).closest('tr');
                var rows = row.closest('tbody').find('tr');
                if (rows.index(row) === rows.length - 1) {
                    e.preventDefault();
                    block.ui.addTicketRow(true);
                }
            }
        });

        block.ui.table.on('click', '.btnDeleteRow', async function (e) {
            e.preventDefault();
            var ticket = _dtHelper.getRowData(this);
            if (ticket.receiptLineId) {
                abp.message.error('You can\'t delete tickets associated with receipts');
                return;
            }
            if (await abp.message.confirm('Are you sure you want to delete the ticket?')) {
                if (ticket.id) {
                    await _ticketService.deleteTicket({ id: ticket.id });
                }
                _tickets.splice(_tickets.indexOf(ticket), 1);
                block.ui.reloadTickets();
                abp.notify.info('Successfully deleted.');
            }
        });

        block.ui.table.on('click', '.btnUploadTicketPhotoForRow', async function (e) {
            e.preventDefault();
            var ticket = _dtHelper.getRowData(this);
            //we'll save the ticket later, after they select the file
            //if (!ticket.id) {
            //    await saveTicket(ticket);
            //}
            _ticketForPhotoUpload = ticket;
            _blockForPhotoUpload = block;
            _$ticketPhotoInput.click();
        });

        block.ui.table.on('click', '.btnDeleteTicketPhotoForRow', async function (e) {
            e.preventDefault();
            var ticket = _dtHelper.getRowData(this);
            if (!await abp.message.confirm('Are you sure you want to delete the image?')) {
                return;
            }

            abp.ui.setBusy(block.ui.card);

            _ticketService.deleteTicketPhoto({
                ticketId: ticket.id
            }).done(function () {
                ticket.ticketPhotoId = null;
                block.ui.reloadGrid();
            }).always(function () {
                abp.ui.clearBusy(block.ui.card);
            });
        });

        block.ui.table.on('click', '.showTicketPhotoButton', function (e) {
            e.preventDefault();
            var ticket = _dtHelper.getRowData(this);
            let url = abp.appPath + 'app/Tickets/GetTicketPhoto/' + ticket.id;
            app.openPopup(url);
        });

        block.ui.table.on('click', '.printTicketButton', function (e) {
            e.preventDefault();
            var ticket = _dtHelper.getRowData(this);
            let url = abp.appPath + 'app/Tickets/GetTicketPrintOut?ticketId=' + ticket.id;
            app.openPopup(url);
        });

        block.ui.table.on('click', '.btnGenerateTicketNumber', async function (e) {
            e.preventDefault();
            let ticket = _dtHelper.getRowData(this);
            if (!await abp.message.confirm('Are you sure you want to generate a ticket number for this ticket?')) {
                return;
            }
            abp.ui.setBusy(block.ui.card);
            _ticketService.generateTicketNumber(ticket.id).done(function (res) {
                ticket.ticketNumber = res;
                block.ui.reloadGrid();
            }).always(function () {
                abp.ui.clearBusy(block.ui.card);
            });
        });

        block.ui.table.on('click', '.btnChangeDriver', function (e) {
            e.preventDefault();
            var ticket = _dtHelper.getRowData(this);
            _selectDriverModal.open({}).done(function (modal, modalObject) {
                let leaseHaulerId = (block.driver && block.driver.leaseHaulerId) || null;
                modalObject.setDrivers(_drivers.filter(d => d.isActive && (d.leaseHaulerId === leaseHaulerId || !block.driver)));
                modalObject.saveCallback = async function (modalResult) {
                    let newDriverId = modalResult.driverId;
                    let newDriver = modalResult.driver;
                    if (!newDriverId || !newDriver || newDriverId === ticket.driverId) {
                        return;
                    }
                    await checkTruckAssignment(block, ticket.truckId, newDriverId);

                    ticket.driverId = newDriverId;
                    return saveTicket(ticket).then((saveResult) => {
                        reloadBlocksAfterTicketTransfer(block, [ticket], newDriverId);
                    });
                };
            });
        });

        block.ui.table.on('click', '.btnChangeDate', function (e) {
            e.preventDefault();
            var ticket = _dtHelper.getRowData(this);
            _selectDateModal.open({}).done(function (modal, modalObject) {
                modalObject.setDate(moment(ticket.ticketDateTime, ['YYYY-MM-DDTHH:mm:ss']).format('L'));
                modalObject.saveCallback = async function (modalResult) {
                    if (!modalResult.date) {
                        return;
                    }

                    let ticketDateTime = moment(ticket.ticketDateTime || block.orderLine.orderDate, ['YYYY-MM-DDTHH:mm:ss']);
                    let newDate = moment(modalResult.date, ['L', 'YYYY-MM-DDTHH:mm:ss']);

                    if (ticketDateTime.isSame(newDate, 'day')) {
                        return;
                    }

                    let newValue = newDate.set({
                        hour: ticketDateTime.hour(),
                        minute: ticketDateTime.minute()
                    });

                    ticket.ticketDateTime = newValue.format('YYYY-MM-DDTHH:mm:ss');
                    return saveTicket(ticket).then((saveResult) => {
                        block.ui.reloadGrid();
                    });
                };
            });
        });

        block.ui.updateVisibility = function () {
            block.ui.isVisible = shouldOrderLineBlockBeVisible(block);
            block.ui.card.toggle(block.ui.isVisible);
        };

        block.ui.destroy = function () {
            block.ui.destroyGrid();
            let controls = [
                block.ui.driver,
                block.ui.customer,
                block.ui.loadAt,
                block.ui.deliverTo,
                block.ui.office,
                block.ui.freightUom,
                block.ui.materialUom,
            ];

            if (_separateItems) {
                controls.push(block.ui.freightItem, block.ui.materialItem);
            } else {
                controls.push(block.ui.item);
            }

            controls.forEach(control => {
                if (control.is('select')) {
                    control.select2('destroy');
                }
            });
        };

        updateCardReadOnlyState(block);
        block.ui.updateVisibility();
    }

    function shouldOrderLineBlockBeVisible(block) {
        let isVisible = !_driverIdFilter || block.driverId === _driverIdFilter;
        if (isVisible && _hideVerifiedTicketsFilter) {
            let allTickets = getTicketsForOrderLineBlock(block);
            let filteredTickets = getTicketsForOrderLineBlock(block, true);
            if (allTickets.length && !filteredTickets.length) {
                isVisible = false;
            }
        }
        return isVisible;
    }

    function renderDisabledInput(ui, uiField, labelText, idField, nameField) {
        let id = abp.helper.getUniqueElementId();
        var result = $('<div class="form-group col-lg-3 col-md-4 col-sm-6">').append(
            $('<label class="control-label">').attr('for', id).text(labelText)
        ).append(
            ui[uiField] = $('<input type="text" class="form-control select2-placeholder-input-control">').attr('name', idField).attr('id', id).prop('disabled', true)
        );
        if (nameField) {
            ui[uiField].data('nameField', nameField);
        }

        return result;
    }

    function renderButton(ui, uiField, labelText) {
        return $('<div class="form-group col-lg-3 col-md-4 col-sm-6">').append(
            $('<label class="control-label d-none d-sm-inline-block">&nbsp;</label>')
        ).append(
            ui[uiField] = $('<button type="button" class="btn btn-primary form-control">').text(labelText)
        );
    }

    function renderDropdownPlaceholder(ui, uiField, labelText, idField, nameField) {
        let id = abp.helper.getUniqueElementId();
        var result = $('<div class="form-group col-lg-3 col-md-4 col-sm-6">').append(
            $('<label class="control-label">').attr('for', id).text(labelText)
        ).append(
            ui[uiField] = $('<input type="text" class="form-control select2-placeholder-input-control">').attr('name', idField).attr('id', id)
        );
        if (nameField) {
            ui[uiField].data('nameField', nameField);
        }

        return result;
    }

    function replaceDropdownPlaceholderWithDropdown(placeholderControl) {
        let newControl = $('<select class="form-control">').attr('name', placeholderControl.attr('name')).attr('id', placeholderControl.attr('id')).append(
            $('<option>').attr('value', '').html('&nbsp;')
        );
        newControl.prop('disabled', placeholderControl.prop('disabled'));
        if (placeholderControl.data('nameField')) {
            newControl.data('nameField', placeholderControl.data('nameField'));
        }
        placeholderControl.replaceWith(newControl);
        return newControl;
    }

    function renderDropdown(ui, uiField, labelText, idField, nameField) {
        let id = abp.helper.getUniqueElementId();
        var result = $('<div class="form-group col-lg-3 col-md-4 col-sm-6">').append(
            $('<label class="control-label">').attr('for', id).text(labelText)
        ).append(
            ui[uiField] = $('<select class="form-control">').attr('name', idField).attr('id', id).append(
                $('<option>').attr('value', '').html('&nbsp;')
            )
        );
        if (nameField) {
            ui[uiField].data('nameField', nameField);
        }
        return result;
    }

    function renderRateInput(ui, uiField, labelText, nameOnForm) {
        let id = abp.helper.getUniqueElementId();
        return $('<div class="form-group col-lg-3 col-md-4 col-sm-6">').append(
            $('<label class="control-label text-nowrap">').attr('for', id).text(labelText)
        ).append(
            ui[uiField] = $('<input class="form-control" type="text" data-rule-number="true" data-rule-min="0">').attr('data-rule-max', app.consts.maxDecimal).attr('name', nameOnForm).attr('id', id)
        );
    }

    function renderTextInput(ui, uiField, labelText, nameOnForm, maxlength) {
        let id = abp.helper.getUniqueElementId();
        var result = $('<div class="form-group col-lg-3 col-md-4 col-sm-6">').append(
            $('<label class="control-label">').attr('for', id).text(labelText)
        ).append(
            ui[uiField] = $('<input type="text" class="form-control">').attr('name', nameOnForm).attr('id', id).attr('maxlength', maxlength)
        );

        return result;
    }

    function renderCheckbox(ui, uiField, labelText, nameOnForm, isChecked) {
        let id = abp.helper.getUniqueElementId();
        let result = $('<div class="form-group col-lg-3 col-md-3 col-sm-6">').append(
            $('<label class="m-checkbox mt-35px">').attr('for', id).text(labelText).append(
                ui[uiField] = $('<input type="checkbox" class="m-checkbox-input">').attr('name', nameOnForm).attr('id', id).prop('checked', isChecked)
            ).append(
                $('<span>')
            )
        );

        return result;
    }

    function renderOfficeRelatedButtons(ui) {
        return $('<div class="dropdown action-button">').append(
            $('<ul class="dropdown-menu dropdown-menu-right">').append(
                $('<li>').append(
                    ui.changeOfficeForAllTrucksButton = $('<a class="dropdown-item">').text(app.localize('ChangeOfficeForAllTrucksOnJob'))
                )
            ).append(
                $('<li>').append(
                    ui.moveTicketsFromBlockToDifferentJob = $('<a class="dropdown-item">').text(app.localize('MoveTicketsForThisDriverToDifferentJob'))
                )
            )
        ).append(
            $('<button class="btn btn-primary" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>')
        );
    }

    function renderOfficeInput(ui) {
        let result = renderDropdownPlaceholder(ui, 'office', app.localize('Office'), 'officeId', 'officeName')
            .toggle(abp.features.isEnabled('App.AllowMultiOfficeFeature')
                && abp.setting.getBoolean('App.General.ShowOfficeOnTicketsByDriver'));

        var buttons = renderOfficeRelatedButtons(ui);

        if (abp.features.isEnabled('App.AllowMultiOfficeFeature')
            && abp.setting.getBoolean('App.General.ShowOfficeOnTicketsByDriver')
            && abp.setting.getBoolean('App.General.SplitBillingByOffices')
        ) {
            let inputGroup = ui.office.wrap('<div class="input-group">').parent();
            let buttonsWrapper = buttons.wrap('<div class="input-group-append">').parent();
            inputGroup.append(buttonsWrapper);
        }

        return result;
    }

    function renderOverrideReadOnlyStateButton(ui) {
        return ui.overrideReadOnlyStateButton = $('<button class="btn btn-default" type="button"><span class="fa fa-edit"></span></button>').hide();
    }

    function renderOrderLineNoteIcon(ui) {
        return ui.orderLineNoteIcon = $('<i class="fa-regular fa-files directions-icon order-line-note-icon" data-toggle="tooltip" data-html="true"></i>').hide();
    }

    function renderClickableWarningIcon(ui) {
        return $('<div class="clickable-warning-icon-container">').append(
            ui.clickableWarningIcon = $('<i class="fas fa-exclamation-triangle clickable-warning-icon lg"></i>').hide()
        );
    }

    function renderDriverNoteIconsContainer(ui) {
        return ui.driverNoteIconsContainer = $('<div class="driver-note-icons-container">');
    }

    function renderTicketCounts(ui, groupingLevel) {
        let emptyBlockCountTitle = 'Driver jobs with no tickets';
        let emptyBlockCountText = '...';
        if (groupingLevel === 'orderLine') {
            emptyBlockCountTitle = 'No tickets for this job';
            emptyBlockCountText = '?';
        }

        return $('<div>').append(
            ui.verifiedTicketCount = $('<span class="circle-small bg-green">').attr('title', 'Verified tickets').text("...").hide()
        ).append(
            ui.enteredTicketCount = $('<span class="circle-small bg-yellow">').attr('title', 'Entered tickets').text("...").hide()
        ).append(
            ui.missingTicketCount = $('<span class="circle-small bg-red">').attr('title', 'Missing tickets').text("...").hide()
        ).append(
            ui.emptyOrderLineBlockCount = $('<span class="circle-small bd-unavailable">').attr('title', emptyBlockCountTitle).text(emptyBlockCountText).hide()
        );
    }

    function renderToggleButton(ui) {
        return $('<div class="col-lg-2 col-md-4 col-sm-2 col-4 pb-2">').append(
            $() //$('<label class="control-label">&nbsp;</label>')
        ).append(
            $('<div class="d-flex justify-content-end">').append(
                ui.toggleCardDetailsButton = $('<button type="button" class="btn btn-primary" data-card-tool="toggle"><span class="arrow fa fa-angle-up"></span></button>')
            )
        );
    }

    function refreshExpandAllButton() {
        //_expandAllPanelsButton.text(haveCollapsedCards() ? 'Expand All' : 'Collapse All');
        //_expandAllPanelsButton.toggle(haveVisibleCards());
    }

    function showUomWarningIfNeeded(dropdownResult) {
        let { oldName, newName, idField } = dropdownResult;
        if (oldName === newName
            || !abp.setting.getBoolean('App.TimeAndPay.PreventProductionPayOnHourlyJobs')
            || idField === 'materialUomId'
        ) {
            return;
        }
        let hours = ['hour', 'hours'];
        newName = (newName || '').toLowerCase();
        oldName = (oldName || '').toLowerCase();
        if (hours.includes(newName) || hours.includes(oldName)) {
            abp.message.warn(app.localize('TimeEntiresWarningOnHoursUomChange'));
        }
    }

    _$ticketPhotoInput.change(async function () {
        if (!_ticketForPhotoUpload || !_blockForPhotoUpload) {
            return;
        }

        if (!abp.helper.validateTicketPhoto(_$ticketPhotoInput)) {
            return;
        }

        if (!_ticketForPhotoUpload.id) {
            await saveTicket(_ticketForPhotoUpload);
        }

        const file = _$ticketPhotoInput[0].files[0];
        const reader = new FileReader();
        let block = _blockForPhotoUpload;

        reader.addEventListener("load", function () {
            _ticketService.addTicketPhoto({
                ticketId: _ticketForPhotoUpload.id,
                ticketPhoto: reader.result,
                ticketPhotoFilename: file.name
            }).done(function (result) {
                _ticketForPhotoUpload.ticketPhotoId = result.ticketPhotoId;
                _blockForPhotoUpload.ui.reloadGrid();
            }).always(function () {
                _ticketForPhotoUpload = null;
                _blockForPhotoUpload = null;
                _$ticketPhotoInput.val('');
                abp.ui.clearBusy(block.ui.card);
            });
        }, false);

        abp.ui.setBusy(block.ui.card);
        reader.readAsDataURL(file);
    });

    abp.event.on('app.chargesEditedModal', function (data) {
        let affectedOrderLine = _orderLines.find(ol => ol.id === data.orderLineId);
        if (!affectedOrderLine) {
            return;
        }

        affectedOrderLine.charges = data.charges;

        let affectedBlocks = _orderLineBlocks.filter(o => o.orderLine.id === data.orderLineId);
        affectedBlocks.forEach(function (affectedBlock) {
            updateCardFromModel(affectedBlock);
        });
    });

})();
