(function () {

    abp.enums ??= {};
    abp.helper ??= {};

    abp.enums.quoteStatus = {
        pending: 0,
        active: 1,
        inactive: 2
    };
    abp.enums.workOrderStatus = {
        pending: 0,
        inProgress: 1,
        complete: 2
    };
    abp.enums.assetType = {
        dumpTruck: 1,
        tractor: 2,
        trailer: 3,
        other: 4
    };
    abp.enums.designation = {
        freightOnly: 1,
        materialOnly: 2,
        freightAndMaterial: 3,
        //rental: 4,
        backhaulFreightOnly: 5,
        backhaulFreightAndMaterial: 9,
        disposal: 6,
        backHaulFreightAndDisposal: 7,
        straightHaulFreightAndDisposal: 8,
    };
    abp.enums.designationName = {
        [abp.enums.designation.freightOnly]: 'Freight Only',
        [abp.enums.designation.materialOnly]: 'Material Only',
        [abp.enums.designation.freightAndMaterial]: 'Freight and Material',
        [abp.enums.designation.backhaulFreightOnly]: 'Backhaul Freight Only',
        [abp.enums.designation.backhaulFreightAndMaterial]: 'Backhaul Freight and Material',
        [abp.enums.designation.disposal]: 'Disposal',
        [abp.enums.designation.backHaulFreightAndDisposal]: 'Back haul freight & disposal',
        [abp.enums.designation.straightHaulFreightAndDisposal]: 'Straight haul freight & disposal',
    };
    abp.enums.designations = {
        hasMaterial: [
            abp.enums.designation.materialOnly,
            abp.enums.designation.freightAndMaterial,
            abp.enums.designation.backhaulFreightAndMaterial,
            abp.enums.designation.disposal,
            abp.enums.designation.backHaulFreightAndDisposal,
            abp.enums.designation.straightHaulFreightAndDisposal
        ],
        materialOnly: [
            abp.enums.designation.materialOnly
        ],
        freightOnly: [
            abp.enums.designation.freightOnly,
            abp.enums.designation.backhaulFreightOnly
        ],
        freightAndMaterial: [
            abp.enums.designation.freightAndMaterial,
            abp.enums.designation.backhaulFreightAndMaterial,
            abp.enums.designation.disposal,
            abp.enums.designation.backHaulFreightAndDisposal,
            abp.enums.designation.straightHaulFreightAndDisposal
        ]
    };

    abp.enums.emailDeliveryStatus = {
        notProcessed: 0,
        processed: 1,
        dropped: 2,
        deferred: 3,
        bounced: 4,
        delivered: 5,
        opened: 6
    };

    abp.enums.smsStatus = {
        unknown: 0,
        accepted: 1,
        delivered: 2,
        failed: 3,
        queued: 4,
        received: 5,
        receiving: 6,
        sending: 7,
        sent: 8,
        undelivered: 9
    };

    abp.enums.driverMessageType = {
        email: 1,
        sms: 2
    };

    abp.enums.orderPriority = {
        high: 1,
        medium: 2,
        low: 3
    };

    abp.enums.paymentProcessor = {
        none: 0,
        heartlandConnect: 1
    };

    abp.enums.taxCalculationType = {
        freightAndMaterialTotal: 1,
        materialLineItemsTotal: 2,
        materialTotal: 3,
        noCalculation: 4
    };

    abp.enums.dispatchStatus = {
        created: 0,
        sent: 1,
        acknowledged: 3,
        loaded: 4,
        completed: 5,
        error: 6,
        canceled: 7
    };

    abp.enums.dispatchStatuses = {
        open: [
            abp.enums.dispatchStatus.created,
            abp.enums.dispatchStatus.sent,
            abp.enums.dispatchStatus.acknowledged,
            abp.enums.dispatchStatus.loaded
        ],
        closed: [
            abp.enums.dispatchStatus.completed,
            abp.enums.dispatchStatus.error,
            abp.enums.dispatchStatus.canceled
        ]
    };

    abp.enums.shifts = {
        shift1: 0,
        shift2: 1,
        shift3: 2,
        noShift: 255
    };

    abp.enums.importType = {
        fuelUsage: 1,
        vehicleUsage: 2,
        customers: 3,
        vendors: 4,
        items: 5,
        employees: 6,
        trucks: 7,
        trux: 8,
        luckStone: 9,
        ironSheepdog: 10,
    };

    abp.enums.dispatchVia = {
        none: 0,
        //sms: 1,
        simplifiedSms: 2,
        driverApplication: 3
    };

    abp.enums.sendSmsOnDispatching = {
        dontSend: 1,
        sendWhenUserNotClockedIn: 2,
        sendForAllDispatches: 3
    };

    abp.enums.invoiceStatus = {
        draft: 0,
        sent: 1,
        viewed: 2,
        readyForExport: 3,
        printed: 4,
        approved: 5
    };

    abp.enums.quickbooksIntegrationKind = {
        none: 0,
        desktop: 1,
        //online: 2,
        qboExport: 3,
        transactionProExport: 4,
        sbtCsvExport: 5,
        hollisExport: 6,
        sageExport: 7,
        jandjExport: 8,
    };
    abp.enums.staggeredTimeKind = {
        none: 0,
        setInterval: 1
        //specificStartTimes: 2
    };
    abp.enums.driverDateConflictKind = {
        bothProductionAndHourlyPay: 1,
        productionPayTimeButNoTickets: 2
    };
    abp.enums.invoicingMethod = {
        aggregateAllTickets: 0,
        separateTicketsByJobNumber: 1,
        separateInvoicePerTicket: 2,
        separateTicketsByJob: 3,
    };
    abp.enums.gpsPlatform = {
        dtdTracker: 1,
        geotab: 2,
        samsara: 3,
        intelliShift: 4
    };
    abp.enums.predefinedLocationCategoryKind = {
        asphaltPlant: 1,
        concretePlant: 2,
        landfillOrRecycling: 3,
        miscellaneous: 4,
        yard: 5,
        quarry: 6,
        sandPit: 7,
        temporary: 8,
        projectSite: 10,
        unknownLoadSite: 11,
        unknownDeliverySite: 12
    };
    abp.enums.entityEnum = {
        dispatch: 1,
        employeeTime: 2,
        driverAssignment: 3,
        employeeTimeClassification: 4,
        timeClassification: 5,
        chatMessage: 6,
        settings: 7,
        order: 8,
        orderLine: 9,
        orderLineTruck: 10,
        driver: 11,
        leaseHaulerDriver: 12,
        truck: 13,
        leaseHaulerTruck: 14,
        leaseHauler: 15,
        user: 16,
        customer: 17,
        location: 18,
        item: 19,
        vehicleCategory: 20,
        unitOfMeasure: 21,
        availableLeaseHaulerTruck: 22,
        insurance: 23,
        customerContact: 24,
        leaseHaulerUser: 25,
    };
    abp.enums.changeType = {
        removed: 0,
        modified: 1
    };
    abp.enums.payStatementItemKind = {
        time: 1,
        ticket: 2
    };
    abp.enums.showFuelSurchargeOnInvoiceEnum = {
        none: 0, //default for historical invoices
        lineItemPerTicket: 2,
        singleLineItemAtTheBottom: 3,
    };
    abp.enums.childInvoiceLineKind = {
        none: 0,
        fuelSurchargeLinePerTicket: 1,
        bottomFuelSurchargeLine: 2
    };
    abp.enums.analyzeRevenueBy = {
        driver: 0,
        truck: 1,
        customer: 2,
        date: 3
    };
    abp.enums.revenueGraphDatePeriod = {
        daily: 1,
        weekly: 2,
        monthly: 3,
        total: 4
    };
    abp.enums.orderNotifyPreferredFormat = {
        neither: 0x0,
        email: 0x1,
        sms: 0x2,
        both: 0x1 | 0x2,
    };
    abp.enums.orderNotifyPreferredFormats = {
        notNeither: [
            abp.enums.orderNotifyPreferredFormat.email,
            abp.enums.orderNotifyPreferredFormat.sms,
            abp.enums.orderNotifyPreferredFormat.both,
        ]
    };
    abp.enums.filterActiveStatus = {
        all: 0,
        active: 1,
        inactive: 2
    };
    abp.enums.multiTenancySides = {
        tenant: 0x1,
        host: 0x2
    };
    abp.enums.truckTripTypes = {
        toLoadSite: 0,
        toDumpSite: 1
    };
    abp.enums.jobStatus = {
        scheduled: 0,
        inProgress: 1,
        completed: 2
    };
    abp.enums.leaseHaulerRequestStatus = {
        requested: 0,
        accepted: 1,
        rejected: 2
    };

    abp.enums.itemType = {
        system: 0,
        service: 1,
        inventoryPart: 2,
        nonInventoryPart: 4,
        otherCharge: 5,
        discount: 6,
        payment: 7,
        salesTaxItem: 8
    };
    abp.enums.itemTypes = {
        material: [
            abp.enums.itemType.inventoryPart,
            abp.enums.itemType.nonInventoryPart,
        ],
        freight: [
            abp.enums.itemType.service,
        ],
        other: [
            abp.enums.itemType.system,
            abp.enums.itemType.otherCharge,
            abp.enums.itemType.discount,
            abp.enums.itemType.payment,
            abp.enums.itemType.salesTaxItem,
        ],
    };
    abp.helper.getItemTypeCategory = function (itemType) {
        for (const [categoryName, category] of Object.entries(abp.enums.itemTypes)) {
            if (category.includes(itemType)) {
                return category;
            }
        }
        return null;
    };
    abp.helper.getItemTypeCategoryDisplayName = function (itemTypeCategory) {
        switch (itemTypeCategory) {
            case abp.enums.itemTypes.material: return 'product';
            case abp.enums.itemTypes.freight: return 'service';
            case abp.enums.itemTypes.other: return 'other';
            default: return 'unknown or empty';
        }
    };

    abp.enums.itemPricingKind = {
        none: 0,
        officeBased: 1,
        locationBased: 2,
        haulZoneBased: 3,
    };

    abp.enums.scheduleTruckSortKind = {
        byTruckCode: 1,
        byDriverSeniority: 2,
    };

    abp.enums.uomBase = {
        hours: 1,
        tons: 2,
        loads: 3,
        cubicYards: 4,
        each: 5,
        cubicMeters: 6,
        miles: 7,
        driveMiles: 8,
        airMiles: 9,
        driveKMs: 10,
        airKMs: 11,
    };
    abp.enums.uomBases = {
        haulRateCalculation: [
            abp.enums.uomBase.driveMiles,
            abp.enums.uomBase.driveKMs,
            abp.enums.uomBase.airMiles,
            abp.enums.uomBase.airKMs,
        ],
    };

    abp.enums.fulcrumEntity = {
        truck: 1,
        driver: 2,
        customer: 3,
        product: 4,
        leaseHauler: 5,
        taxRate: 6,
        dtdTicket: 7,
    };

    abp.enums.ticketType = {
        both: 0,
        internalTrucks: 1,
        leaseHaulers: 2,
    };

    abp.enums.documentType = {
        insurance: 1,
        certification: 2,
    };

    abp.enums.fuelSurchargeCalculationType = {
        basedOnActualFuelCost: 1,
        simplePercentage: 2,
    };

    abp.enums.requiredTicketEntry = {
        none: 0,
        always: 1,
        byJobDefaultingToRequired: 2,
        byJobDefaultingToNotRequired: 3,
    };
})();
