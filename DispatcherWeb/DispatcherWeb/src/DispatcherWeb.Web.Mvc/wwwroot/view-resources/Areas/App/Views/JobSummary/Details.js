mobiscroll.setOptions({
    theme: 'material',
    themeVariant: 'light'
});
mobiscroll.momentTimezone.moment = moment;

(function () {
    $(async function () {

        const _orderLineId = $('#OrderLineId').val();
        const _jobSummaryService = abp.services.app.jobSummary;
        const _uomName = $("#UomName").val();
        const _deliveryDate = moment($('#DeliveryDate').val(), 'YYYY-MM-DD');
        var _driversGrouped = [];

        const jobSummaryDetailsTable = $('#job-summary-details');
        const jobSummaryDetailsGrid = jobSummaryDetailsTable.DataTableInit({
            paging: true,
            serverSide: false,
            processing: false,
            data: [],
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
                    responsivePriority: 1,
                    data: 'loadTime',
                    title: 'Load Time'
                },
                {
                    data: 'truck',
                    title: 'Truck'
                },
                {
                    data: 'loadTicket',
                    title: 'Load Ticket'
                },
                {
                    data: 'deliveryTime',
                    title: 'Delivery Time'
                },
                {
                    data: 'deliveryTicket',
                    title: 'Delivery Ticket'
                },
                {
                    data: 'cycleTime',
                    title: 'Cycle Time'
                },
                {
                    data: 'quantity',
                    title: 'Quantity'
                }
            ]
        });

        const resourceStyles = {
            header: {
                rowClass: '',
                columnClass: '',
            },
            row: {
                rowClass: '',
                columnClass: 'trip',
            },
            footer: {
                rowClass: 'md-resource-details-footer md-resource-details-job-summary',
                columnClass: '',
            },
        };

        const composeResourceValues = (resourceStrings, styles) => {
            let result = $('<div class="grid-container">')
                .addClass(styles.rowClass);

            resourceStrings.forEach((resourceValue, ctr) => {
                $('<div>')
                    .text(resourceValue)
                    .addClass(`grid-item grid-column-${ctr + 1}`)
                    .addClass(styles.columnClass)
                    .appendTo(result);
            });

            return $('<div>').append(result.clone()).html();
        };

        const trucksTimeline = $('#trucks-timeline');
        const truckEventsTimeline = trucksTimeline.mobiscroll().eventcalendar({
            timezonePlugin: mobiscroll.momentTimezone,
            dataTimezone: 'utc',
            displayTimezone: abp.timing.timeZoneInfo.iana.timeZoneId,
            clickToCreate: false,
            dragToCreate: false,
            dragToMove: false,
            dragToResize: false,
            dragTimeStep: 30,           // More info about dragTimeStep: https://docs.mobiscroll.com/5-25-1/eventcalendar#opt-dragTimeStep
            view: {                     // More info about view: https://docs.mobiscroll.com/5-25-1/eventcalendar#opt-view
                timeline: {
                    type: 'day',
                    startDay: 0,        // Sunday to
                    endDay: 6,          // Saturday
                    startTime: '00:00',
                    endTime: '24:00',
                    timeCellStep: 5,
                    timeLabelStep: 20,
                    weekNumbers: false,
                    currentTimeIndicator: true,
                }
            },
            renderResourceHeader: function () {
                return '<div class="summary-resource-header-label">Drivers</div>';
            },
            // More info about renderDay: https://docs.mobiscroll.com/5-25-1/eventcalendar#opt-renderDay
            renderDay: function (args) {
                let formatDate = mobiscroll.util.datetime.formatDate;
                return $('<div>').append($('<div>', {
                    class: 'md-work-order-date',
                    text: formatDate('DDDD, MMM DD, YYYY', args.date)
                }).clone()).html();
            },
            // More info about renderScheduleEventContent: https://docs.mobiscroll.com/5-25-1/eventcalendar#opt-renderScheduleEventContent
            renderScheduleEventContent: function (event) {
                return $('<div>').append($('<div>', {
                    text: event.title
                }).clone()).html();
            },
            renderResourceHeader: () => composeResourceValues(
                [
                    'Truck',
                    'Loads',
                    _uomName
                ], resourceStyles.header),
            renderResource: (resource) => composeResourceValues(
                [
                    resource.truckCode,
                    resource.loadCount,
                    resource.quantity
                ], resourceStyles.row),
            renderResourceFooter: () => composeResourceValues(
                [
                    'Truck Loaded',
                    _driversGrouped.map(x => x.loadCount).reduce((a, b) => a + b, 0),
                    _driversGrouped.map(x => x.quantity).reduce((a, b) => a + b, 0)
                ], resourceStyles.footer),
            renderHourFooter: function (day) {
                let date = day.date;
                let formatDate = mobiscroll.util.datetime.formatDate;
                return '<div>' + formatDate('h:mm A', date) + '</div>';
            },

        }).mobiscroll('getInst');

        const renderLoadDetails = (orderTrucksData) => {
            let jobSummaryDetails = [];
            let orderTrucks = orderTrucksData.orderTrucks;

            orderTrucks.forEach(orderTruck => {
                let groupedTripEvents = orderTruck.tripCycles.reduce((result, item) => {
                    result[item.ticketId] ||= [];
                    result[item.ticketId].push(item);
                    return result;
                }, {});

                for (const ticketId in groupedTripEvents) {
                    let tripCycles = groupedTripEvents[ticketId];
                    let startDates = tripCycles.map(x => moment(x.startDateTime));
                    let minDate = moment.min(startDates);
                    let endDates = tripCycles.map(x => moment(x.endDateTime));
                    let maxDate = moment.max(endDates);
                    let deliveryTime = tripCycles.some(x => !x.endDateTime) ? '' : maxDate.format('M/D LT');

                    let jobDetail = {
                        'id': jobSummaryDetails.length + 1,
                        'truck': orderTruck.truckCode,
                        'quantity': tripCycles[0].quantity + ' ' + _uomName,
                        'loadTicket': ticketId,
                        'loadTime': moment.max(startDates).format('M/D LT'),
                        'deliveryTime': deliveryTime,
                        'deliveryTicket': ticketId,
                        'cycleTime': app.formatTimeDifference(minDate, maxDate),
                    };

                    jobSummaryDetails.push(jobDetail);
                }
            });

            jobSummaryDetailsGrid.clear();
            jobSummaryDetailsGrid.rows.add(jobSummaryDetails);
            jobSummaryDetailsGrid.draw();

            abp.ui.clearBusy(jobSummaryDetailsTable.closest('.dataTables_wrapper'));
        };

        const renderTrucksInMap = (orderTrucksData) => {
        };

        let orderTrucksData = await _jobSummaryService.getJobSummaryLoads(_orderLineId);

        let jobStatus = parseInt($('#JobStatus').attr('data-status'));
        $('.job-summary-map-container').toggle(jobStatus === abp.enums.jobStatus.inProgress);

        _driversGrouped = orderTrucksData.orderTrucks.map(ot => ({
            id: ot.truckCode,
            truckCode: ot.truckCode,
            loadCount: ot.loadCount,
            quantity: ot.quantity,
            color: '#e9ec12'
        }));

        if (orderTrucksData.orderTrucks?.length) {
            truckEventsTimeline.setOptions({
                min: moment(orderTrucksData.earliest).startOf('day'),
                max: moment(orderTrucksData.latest).endOf('day'),
                resources: _driversGrouped
            });
        } else {
            truckEventsTimeline.setOptions({
                min: _deliveryDate.startOf('day'),
                max: _deliveryDate.endOf('day')
            });
        }
        await app.sleepAsync(0);

        let tripEvents = orderTrucksData.orderTrucks.flatMap(
            ({
                tripCycles,
                truckCode
            }) => tripCycles.map(t => ({
                resource: [truckCode],
                start: t.startDateTime,
                end: t.endDateTime || moment().utc().format(),
                duration: t.duration,
                title: t.label,
                tooltip: t.segmentHoverText,
                sourceLatitude: t.sourceLatitude,
                sourceLongitude: t.sourceLongitude,
                driver: t.driverName,
                tripType: t.truckTripType,
                location: t.location,
                truckCode: truckCode,
                ticketId: t.ticketId,
                color: t.truckTripType === abp.enums.truckTripTypes.toLoadSite
                    ? app.colors.tripToLoadSite
                    : app.colors.tripToDeliverySite,
            }))
        );

        truckEventsTimeline.setEvents(tripEvents);
        await app.sleepAsync(0);

        if (tripEvents.length) {
            let earliestTripEvent = tripEvents.reduce(function (result, item) {
                return (item.start < result.start) ? item : result;
            });

            truckEventsTimeline.navigate(moment(earliestTripEvent.start).format('L LT'));
            await app.sleepAsync(0);
        }

        renderLoadDetails(orderTrucksData);

        if (jobStatus === abp.enums.jobStatus.inProgress) {
            renderTrucksInMap(orderTrucksData);
        }

    });
})();
