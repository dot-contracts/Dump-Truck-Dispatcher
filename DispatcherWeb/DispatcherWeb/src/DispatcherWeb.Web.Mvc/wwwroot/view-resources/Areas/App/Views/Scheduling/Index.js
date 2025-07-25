(function () {
    $(function () {

        var _orderService = abp.services.app.order;
        var _schedulingService = abp.services.app.scheduling;
        var _truckService = abp.services.app.truck;
        var _dispatchingService = abp.services.app.dispatching;
        var _driverAssignmentService = abp.services.app.driverAssignment;
        var _trailerAssignmentService = abp.services.app.trailerAssignment;
        var _dtHelper = abp.helper.dataTables;
        var _permissions = {
            edit: abp.auth.hasPermission('Pages.Orders.Edit'),
            print: abp.auth.hasPermission('Pages.PrintOrders'),
            editTickets: abp.auth.hasPermission('Pages.Tickets.Edit'),
            editQuotes: abp.auth.hasPermission('Pages.Quotes.Edit'),
            editCharges: abp.auth.hasPermission('Pages.Charges'),
            driverMessages: abp.auth.hasPermission('Pages.DriverMessages'),
            trucks: abp.auth.hasPermission('Pages.Trucks'),
            viewJobSummary: abp.auth.hasPermission('Pages.Orders.ViewJobSummary'),
            schedule: abp.auth.hasPermission('Pages.Schedule'),
            leaseHaulerSchedule: abp.auth.hasPermission('LeaseHaulerPortal.Schedule'),
            acceptLeaseHaulerJob: abp.auth.hasPermission('LeaseHaulerPortal.Jobs.Accept'),
            rejectLeaseHaulerJob: abp.auth.hasPermission('LeaseHaulerPortal.Jobs.Reject'),
            editLeaseHaulerJob: abp.auth.hasPermission('LeaseHaulerPortal.Jobs.Edit'),
            leaseHaulerTickets: abp.auth.hasPermission('LeaseHaulerPortal.Tickets'),
            leaseHaulerPortalTrucks: abp.auth.hasPermission('LeaseHaulerPortal.MyCompany.Trucks'),
            leaseHaulerTruckRequest: abp.auth.hasPermission('LeaseHaulerPortal.Truck.Request'),
        };
        var _features = {
            allowMultiOffice: abp.features.isEnabled('App.AllowMultiOfficeFeature'),
            allowSendingOrdersToDifferentTenant: abp.features.isEnabled('App.AllowSendingOrdersToDifferentTenant'),
            leaseHaulers: abp.features.isEnabled('App.AllowLeaseHaulersFeature'),
            leaseHaulerPortal: abp.features.isEnabled('App.LeaseHaulerPortalFeature'),
            jobBasedLeaseHaulerRequest: abp.features.isEnabled('App.LeaseHaulerPortalJobBasedLeaseHaulerRequest'),
            separateItems: abp.features.isEnabled('App.SeparateMaterialAndFreightItems'),
        };
        var _settings = {
            validateUtilization: abp.setting.getBoolean('App.DispatchingAndMessaging.ValidateUtilization'),
            allowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders: abp.setting.getBoolean('App.General.AllowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders'),
            showTrailersOnSchedule: abp.setting.getBoolean('App.DispatchingAndMessaging.ShowTrailersOnSchedule'),
            allowLeaseHaulerRequestProcess: abp.setting.getBoolean('App.LeaseHaulers.AllowLeaseHaulerRequestProcess'),
            showStaggerTimes: abp.setting.getBoolean('App.DispatchingAndMessaging.ShowStaggerTimes'),
            notAllowSchedulingLeaseHaulersWithExpiredInsurance: abp.setting.getBoolean('App.LeaseHaulers.NotAllowSchedulingLeaseHaulersWithExpiredInsurance'),
        };
        var _vehicleCategories = null;
        var _loadingState = false;
        var _trucksWereLoadedOnce = false;
        var _scheduleTrucks = [];
        var _driverAssignments = [];
        var _orderLines = null;
        var _orderLineGridCache = null;

        var isDispatchViaGeotabEnabled = false;
        var dispatchVia = abp.setting.getInt('App.DispatchingAndMessaging.DispatchVia');
        var allowSmsMessages = abp.setting.getBoolean('App.DispatchingAndMessaging.AllowSmsMessages');
        var hasDispatchPermissions = abp.auth.hasPermission('Pages.Dispatches.Edit');
        var showDispatchItems = dispatchVia !== abp.enums.dispatchVia.none && hasDispatchPermissions;
        var showDispatchViaSmsItems = dispatchVia !== abp.enums.dispatchVia.none && hasDispatchPermissions;
        var showDispatchViaGeotabItems = isDispatchViaGeotabEnabled && hasDispatchPermissions;
        var showProgressColumn = dispatchVia === abp.enums.dispatchVia.driverApplication;
        var _scheduleTruckSortKind = 0;

        var _setTruckUtilizationModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/SetTruckUtilizationModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_SetTruckUtilizationModal.js',
            modalClass: 'SetTruckUtilizationModal',
            modalSize: 'sm'
        });

        var _changeOrderLineUtilizationModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/ChangeOrderLineUtilizationModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_ChangeOrderLineUtilizationModal.js',
            modalClass: 'ChangeOrderLineUtilizationModal',
            modalSize: 'sm'
        });

        var _setOrderOfficeIdModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/SetOrderOfficeIdModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_SetOrderOfficeIdModal.js',
            modalClass: 'SetOrderOfficeIdModal',
            modalSize: 'sm'
        });

        var _setOrderDateModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/SetOrderDateModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_SetOrderDateModal.js',
            modalClass: 'SetOrderDateModal',
            modalSize: 'sm'
        });

        var _copyOrderModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CopyOrderModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CopyOrderModal.js',
            modalClass: 'CopyOrderModal'
        });

        var _carryUnfinishedPortionForwardModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CarryUnfinishedPortionForwardModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CarryUnfinishedPortionForwardModal.js',
            modalClass: 'CarryUnfinishedPortionForwardModal'
        });

        var _sendOrderLineToHaulingCompanyModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/SendOrderLineToHaulingCompanyModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_SendOrderLineToHaulingCompanyModal.js',
            modalClass: 'SendOrderLineToHaulingCompanyModal',
            modalSize: 'sm'
        });

        var _setNoDriverForTruckModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/SetNoDriverForTruckModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_SetNoDriverForTruckModal.js',
            modalClass: 'SetNoDriverForTruckModal'
        });

        var _assignDriverForTruckModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/AssignDriverForTruckModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_AssignDriverForTruckModal.js',
            modalClass: 'AssignDriverForTruckModal'
        });

        var _setDefaultDriverForTruckModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/SetDefaultDriverForTruckModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_SetDefaultDriverForTruckModal.js',
            modalClass: 'SetDefaultDriverForTruckModal'
        });

        var _showTruckOrdersModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/ShowTruckOrdersModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_ShowTruckOrdersModal.js',
            modalClass: 'ShowTruckOrdersModal',
            modalSize: 'lg'
        });

        var _addOutOfServiceReasonModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Trucks/AddOutOfServiceReasonModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Trucks/_AddOutOfServiceReasonModal.js',
            modalClass: 'AddOutOfServiceReasonModal'
        });

        var _tripsReportModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/TripsReportModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_TripsReportModal.js',
            modalClass: 'TripsReportModal'
        });

        var _cycleTimesModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/CycleTimesModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_CycleTimesModal.js',
            modalClass: 'CycleTimesModal',
            modalSize: 'lg'
        });

        var _sendDispatchMessageModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/SendDispatchMessageModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_SendDispatchMessageModal.js',
            modalClass: 'SendDispatchMessageModal'
        });

        var _rejectLeaseHaulerRequestModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/LeaseHaulerRequests/RejectLeaseHaulerRequestModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/LeaseHaulerRequests/_RejectLeaseHaulerRequestModal.js',
            modalClass: 'RejectLeaseHaulerRequestModal'
        });

        var _activateClosedTrucksModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/ActivateClosedTrucksModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_ActivateClosedTrucksModal.js',
            modalClass: 'ActivateClosedTrucksModal'
        });

        var _sendDriverMessageModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/DriverMessages/SendMessageModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/DriverMessages/_SendMessageModal.js',
            modalClass: 'SendMessageModal'
        });

        var _createOrEditTicketModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CreateOrEditTicketModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CreateOrEditTicketModal.js',
            modalClass: 'CreateOrEditTicketModal',
            modalSize: 'xl'
        });

        var _printOrderWithDeliveryInfoModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/PrintOrderWithDeliveryInfoModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_PrintOrderWithDeliveryInfoModal.js',
            modalClass: 'PrintOrderWithDeliveryInfoModal'
        });

        var _assignTrucksModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/AssignTrucksModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_AssignTrucksModal.js',
            modalClass: 'AssignTrucksModal',
            modalSize: 'xl'
        });

        var _changeDriverForOrderLineTruckModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/ChangeDriverForOrderLineTruckModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_ChangeDriverForOrderLineTruckModal.js',
            modalClass: 'ChangeDriverForOrderLineTruckModal'
        });

        var _sendOrdersToDriversModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/SendOrdersToDriversModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_SendOrdersToDriversModal.js',
            modalClass: 'SendOrdersToDriversModal',
            //modalSize: 'sm'
        });

        var _createOrEditOrderModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CreateOrEditOrderModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CreateOrEditOrderModal.js',
            modalClass: 'CreateOrEditOrderModal',
            modalSize: 'xl'
        });

        var _createOrEditJobModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CreateOrEditJobModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CreateOrEditJobModal.js',
            modalClass: 'CreateOrEditJobModal',
            modalSize: 'lg'
        });

        var _specifyPrintOptionsModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/SpecifyPrintOptionsModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_SpecifyPrintOptionsModal.js',
            modalClass: 'SpecifyPrintOptionsModal',
            modalSize: 'sm'
        });

        var _createOrEditTruckModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Trucks/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Trucks/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditTruckModal',
            modalSize: 'lg',
        });

        const _createOrEditLeaseHaulerTruckModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/LeaseHaulers/CreateOrEditLeaseHaulerTruckModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/LeaseHaulers/_CreateOrEditLeaseHaulerTruckModal.js',
            modalClass: 'CreateOrEditLeaseHaulerTruckModal'
        });

        var _selectTrailerModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/SelectTrailerModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_SelectTrailerModal.js',
            modalClass: 'SelectTrailerModal'
        });

        var _setTractorForTrailer = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/SetTractorForTrailerModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_SetTractorForTrailerModal.js',
            modalClass: 'SetTractorForTrailerModal'
        });

        var _reassignTrucksModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/ReassignTrucksModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_ReassignTrucksModal.js',
            modalClass: 'ReassignTrucksModal'
        });

        var _createOrEditLeaseHaulerRequestModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/LeaseHaulerRequests/CreateOrEditLeaseHaulerRequestModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/LeaseHaulerRequests/_CreateOrEditLeaseHaulerRequestModal.js',
            modalClass: 'CreateOrEditLeaseHaulerRequestModal'
        });

        var _addOrEditLeaseHaulerRequestModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/LeaseHaulerRequests/AddOrEditLeaseHaulerRequestModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/LeaseHaulerRequests/_AddOrEditLeaseHaulerRequestModal.js',
            modalClass: 'AddOrEditLeaseHaulerRequestModal'
        });

        var _editChargesModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Charges/EditChargesModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Charges/_EditChargesModal.js',
            modalClass: 'EditChargesModal',
            modalSize: 'xl',
        });


        $("#DateFilter").val(moment().format("MM/DD/YYYY"));
        //$("#DateFilter").val(moment("02/16/2017", "MM/DD/YYYY").format("MM/DD/YYYY")); //debug
        $("#DateFilter").datepicker();
        $("#OfficeIdFilter").select2Init({
            abpServiceMethod: listCacheSelectLists.office(),
            showAll: true,
            allowClear: true
        });
        abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), abp.session.officeId, abp.session.officeName);
        $('#ShiftFilter').select2Init({ allowClear: false });

        $("#TruckCategoryIdFilter").select2Init({
            abpServiceMethod: listCacheSelectLists.vehicleCategory(),
            showAll: true,
            allowClear: true
        });

        refreshHideProgressBarCheckboxVisibility();
        refreshDateRelatedButtonsVisibility();

        $("#TruckTileChooseGroupingButton > .btn").click(function () {
            updateTruckTileContainerVisibility($(this));
        });

        $("#TruckTileSortByButton > .btn").click(function () {
            sortScheduleTrucksForButton($(this));
        });
        function updateTruckTileContainerVisibility($button) {
            $button.addClass("active").hide().siblings().removeClass("active").show();
            var truckTileGroupingCategory = $button.data('category');
            app.localStorage.setItem('truckTileGroupingCategory', truckTileGroupingCategory);
            if (truckTileGroupingCategory) {
                $('#TruckTiles').hide();
                if (_trucksWereLoadedOnce) {
                    $('#TruckTilesByCategory').show();
                } else {
                    $('#TruckTilesByCategory').hide();
                }
            } else {
                $('#TruckTiles').show();
                $('#TruckTilesByCategory').hide();
            }
        }

        function sortScheduleTrucksForButton($button) {
            $button.hide().siblings().show();
            _scheduleTruckSortKind = Number($button.data('sort'));
            app.localStorage.setItem('scheduleTruckSortKind', _scheduleTruckSortKind);
            if (_scheduleTrucks.length) {
                sortScheduleTrucks();
                drawTruckTiles();
            }
        }

        function updateTruckTileGroupingContainerVisibilityFromCache() {
            app.localStorage.getItem('truckTileGroupingCategory', function (result) {
                var truckTileGroupingCategory = result || false;
                var button = $('#TruckTileChooseGroupingButton > .btn[data-category="' + truckTileGroupingCategory + '"]');
                updateTruckTileContainerVisibility(button);
            });
        }
        updateTruckTileGroupingContainerVisibilityFromCache();

        async function updateTruckTileSortButtonVisibility() {
            var button = $('#TruckTileSortByButton > .btn[data-sort="' + _scheduleTruckSortKind + '"]');
            sortScheduleTrucksForButton(button);
        }

        var truckTiles = $("#TruckTiles");
        async function reloadTruckTiles() {
            if (_vehicleCategories === null) {
                await loadVehicleCategoriesAsync();
            }
            if (!_scheduleTruckSortKind) {
                _scheduleTruckSortKind = await app.localStorage.getItemAsync('scheduleTruckSortKind') || abp.enums.scheduleTruckSortKind.byTruckCode;
                updateTruckTileSortButtonVisibility();
            }

            var options = {};
            $.extend(options, _dtHelper.getFilterData());
            var result = await _schedulingService.getScheduleTrucks(options);
            _scheduleTrucks = result.items;
            sortScheduleTrucks();
            drawTruckTiles();
        }

        function sortScheduleTrucks() {
            switch (_scheduleTruckSortKind) {
                default:
                case abp.enums.scheduleTruckSortKind.byTruckCode:
                    _scheduleTrucks.sort((a, b) => (a.truckCode || '').localeCompare(b.truckCode || ''));
                    _scheduleTrucks.sort((a, b) => b.vehicleCategory.isPowered - a.vehicleCategory.isPowered);
                    _scheduleTrucks.sort((a, b) => b.isExternal - a.isExternal);
                    break;

                case abp.enums.scheduleTruckSortKind.byDriverSeniority:
                    _scheduleTrucks.sort((a, b) => {
                        var dateA = a.driverDateOfHire;
                        var dateB = b.driverDateOfHire;
                        if (dateA === dateB) {
                            return 0;
                        }
                        if (!dateA) {
                            return 1;
                        }
                        if (!dateB) {
                            return -1;
                        }
                        return new Date(dateA) - new Date(dateB);
                    });
                    break;
            }
        }

        function drawTruckTiles() {
            var trucks = _scheduleTrucks;

            var truckTilesByCategory = $('.schedule-truck-tiles[data-truck-category-id]');
            truckTiles.empty();
            truckTilesByCategory.empty();
            $.each(trucks, function (ind, truck) {
                var tileWrapper = $('<div class="truck-tile-wrap">')
                    .addClass(getTruckTileOfficeClass(truck) ? '' : getTruckTileClass(truck)); //don't add the color class (getTruckTileClass) to the wrapper if the tile is office-specific (when getTruckTileOfficeClass(truck) != '')
                $('<div class="truck-tile"></div>')
                    .data('truck', truck)
                    .addClass(getTruckTileClass(truck))
                    .addClass(getTruckTileOfficeClass(truck))
                    .addClass(getTruckTilePointerClass(truck))
                    .text(truck.truckCode)
                    .attr('title', getTruckTileTitle(truck))
                    .appendTo(tileWrapper);
                tileWrapper
                    .appendTo(truckTiles);
                var truckTilesContainer = truckTilesByCategory.closest(`[data-truck-category-id="${truck.vehicleCategory.id}"`);
                tileWrapper.clone(true, true).appendTo(truckTilesContainer);
            });

            if (trucks.length) {
                truckTiles.append(getAddTruckTileButton());
                $("#TruckTilesNoTrucksMessage").hide();
            } else {
                $("#TruckTilesNoTrucksMessage").show();
            }

            var groupedTruckTilesContainers = $('#TruckTilesByCategory div.schedule-truck-tiles');
            $.each(groupedTruckTilesContainers, function (ind, truckTilesContainer) {
                let $truckTilesContainer = $(truckTilesContainer);
                let $categoryContainer = $truckTilesContainer.parents('div.m-accordion__item');
                var $collapsableHeader = $categoryContainer.children('div.m-accordion__item-head');
                if ($truckTilesContainer.has('div.truck-tile-wrap').length === 0) {
                    //$collapsableHeader.parent().remove();
                    $categoryContainer.hide();
                    //$collapsableHeader.removeAttr('data-toggle');
                    //$collapsableHeader.find('span:nth-child(3)').remove();
                } else {
                    $categoryContainer.show();
                    $truckTilesContainer.append(getAddTruckTileButton($truckTilesContainer.data('truck-category-id')));
                    var $dataToggleAttr = $collapsableHeader.attr('data-toggle');
                    if (typeof $dataToggleAttr !== typeof undefined && $dataToggleAttr !== false) {
                        return;
                    }
                    $collapsableHeader.attr('data-toggle', 'collapse');
                    $collapsableHeader.append('<span class="m-accordion__item-mode"></span>');
                }
            });
            _trucksWereLoadedOnce = true;
            updateTruckTileGroupingContainerVisibilityFromCache();
        }

        function getTruckTagText(item) {
            if (item.isLeaseHaulerRequest) {
                return `${item.leaseHaulerName} - ${item.numberTrucksRequested}`;
            } else if (item.isRequestedLeaseHaulerTruck) {
                return item.truckCode;
            }
            if (_settings.showStaggerTimes) {
                return (item.timeOnJob ? moment(item.timeOnJob).format('HH:mm') + ' ' : '') + item[getCombinedTruckCodeFieldName()];
            }
            return item[getCombinedTruckCodeFieldName()];
        }

        function getTruckTagElement(item) {
            if (item.isLeaseHaulerRequest || item.isRequestedLeaseHaulerTruck) {
                return null;
            }
            var result = $();
            if (_settings.showStaggerTimes && item.timeOnJob) {
                result = result.add($('<span>').text(moment(item.timeOnJob).format('HH:mm') + " "));
            }
            result = result.add($('<span class="truck-text">').text(item[getCombinedTruckCodeFieldName()]));

            return result;
        }

        function getAddTruckTileButton(category) {
            return $('<button type="button" class="btn btn-default add-truck-tile-button" title="Add Truck"><i class="fa fa-plus"></i></button>').click(function () {
                openAddTruckModal({
                    vehicleCategoryId: category || null
                });
            });
        }

        function openAddTruckModal(additionalOptions) {
            if (_permissions.trucks) {
                _createOrEditTruckModal.open(additionalOptions || {});
            } else if (_permissions.leaseHaulerPortalTrucks && abp.session.leaseHaulerId) {
                _createOrEditLeaseHaulerTruckModal.open({
                    leaseHaulerId: abp.session.leaseHaulerId,
                    ...additionalOptions
                });
            } else {
                abp.message.warn('You do not have permission to add trucks.');
            }
        }

        async function reloadDriverAssignments() {
            var result = await _driverAssignmentService.getAllDriverAssignmentsLite(_dtHelper.getFilterData());
            result.items.sort((a, b) => {
                if (a.driverLastName < b.driverLastName) return -1;
                if (a.driverLastName > b.driverLastName) return 1;
                if (a.driverFirstName < b.driverFirstName) return -1;
                if (a.driverFirstName > b.driverFirstName) return 1;
                if (a.truckCode < b.truckCode) return -1;
                if (a.truckCode > b.truckCode) return 1;
                return 0;
            });
            _driverAssignments = result.items;
        }

        async function loadVehicleCategoriesAsync() {
            _vehicleCategories = await _truckService.getVehicleCategories();
            renderVehicleCategories();
        }
        function renderVehicleCategories() {
            $('#TruckTilesByCategory').empty();
            _vehicleCategories.forEach((vehicleCategory, i) => {
                $('#TruckTilesByCategory').append(
                    $('<div class="m-accordion__item">').append(
                        $('<div class="m-accordion__item-head collapsed" role="tab" data-toggle="collapse" aria-expanded="false">')
                            .attr('id', `collapse${i}_head`)
                            .attr('href', `#collapse${i}`)
                            .append(
                                $('<span class="m-accordion__item-icon"></span>')
                            )
                            .append(
                                $('<span class="m-accordion__item-title"></span>').text(vehicleCategory.name + 's')
                            ).append(
                                $('<span class="m-accordion__item-mode"></span>')
                            )
                    ).append(
                        $('<div class="m-accordion__item-body collapse" role="tabpanel">')
                            .attr('id', `collapse${i}`)
                            .attr('aria-labelledby', `collapse${i}_head`)
                            .append(
                                $('<div class="m-accordion__item-content">')
                                    .append(
                                        $('<div class="schedule-truck-tiles truck-tiles-by-category"></div>')
                                            .attr('data-truck-category-id', vehicleCategory.id)
                                            .attr('id', `TruckTiles${vehicleCategory.id}`)
                                    )
                            )
                    )
                );
            });
        }

        var _truckOrdersModalOpening = false;
        var $trucksBlock = $('.schedule-truck-tiles');
        $trucksBlock.on('click', 'div.truck-tile.hand', async function () {
            if (_truckOrdersModalOpening) {
                return;
            }
            try {
                _truckOrdersModalOpening = true;
                var $truckDiv = $(this);
                abp.ui.setBusy($trucksBlock);
                var truckCode = $truckDiv.text();
                var truckId = $truckDiv.data('truck').id;
                var filterData = _dtHelper.getFilterData();
                var date = filterData.date;
                await _showTruckOrdersModal.open({
                    truckId: truckId,
                    truckCode: truckCode,
                    scheduleDate: date,
                    shift: filterData.shift
                });
            } finally {
                abp.ui.clearBusy($trucksBlock);
                _truckOrdersModalOpening = false;
            }
        });

        function removeScheduleTruckFromArray(trucks, truck) {
            var index = trucks.indexOf(truck);
            if (index !== -1) {
                trucks.splice(index, 1);
            }
        }

        function orderLineHasTruck(orderLine, truck) {
            return orderLine.trucks.some(olt => olt.truckId === truck.id);
        }

        function getTractorForTrailer(trailer) {
            if (trailer.vehicleCategory.assetType !== abp.enums.assetType.trailer || !trailer.tractor) {
                return null;
            }
            return _scheduleTrucks.find(x => x.id === trailer.tractor.id);
        }

        function sortOrderLines(orderGridCache, abpData) {
            let direction = abpData.sorting?.split(',')[0].split(' ')[1]?.toUpperCase() === 'DESC' ? -1 : 1;
            let field = abp.utils.toCamelCase(abpData.sorting?.split(',')[0].split(' ')[0] || '');

            if (!field) {
                return orderGridCache;
            }

            orderGridCache.items.sort((a, b) => {

                let valueA = (getOrderLineSortingFieldValue(a, field) || '');
                if (typeof valueA === 'string') {
                    valueA = valueA.toUpperCase();
                }

                let valueB = (getOrderLineSortingFieldValue(b, field) || '');
                if (typeof valueB === 'string') {
                    valueB = valueB.toUpperCase();
                }

                if (valueA < valueB) {
                    return -1 * direction;
                }
                if (valueA > valueB) {
                    return 1 * direction;
                }
                return 0;
            });
            return orderGridCache;
        }

        function getOrderLineSortingFieldValue(orderLine, field) {
            switch (field) {
                default:
                    return orderLine[field];
            }
        }

        function canAddTruckWithDriverToOrderLine(truck, driverId, orderLine) {
            if (isTodayOrFutureDate(orderLine)
                && (truck.utilization >= 1 && _settings.validateUtilization
                    || truckHasNoDriver(truck) && truckCategoryNeedsDriver(truck) && _settings.validateUtilization
                    || truck.isOutOfService
                    || !allowScheduling(truck)
                    || truck.vehicleCategory.assetType === abp.enums.assetType.trailer && !getTractorForTrailer(truck))
            ) {
                return false;
            }
            if (orderLine.trucks.some(olt => !olt.isDone && (olt.truckId === truck.id && olt.driverId === driverId))) {
                return false;
            }
            if (_settings.allowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders
                && orderLine.vehicleCategoryIds.length && !orderLine.vehicleCategoryIds.includes(truck.vehicleCategory.id)
                && (!truck.trailer || !orderLine.vehicleCategoryIds.includes(truck.trailer?.vehicleCategory.id))
            ) {
                return false;
            }

            return true;
        }

        function getScheduleTrucksForOrderLine(orderLine, query, syncCallback, asyncCallback) {
            var result = [];
            query = (query || '').toLowerCase();
            _scheduleTrucks.forEach(truck => {
                if (!canAddTruckWithDriverToOrderLine(truck, truck.driverId, orderLine)) {
                    return;
                }

                var truckCode = (truck.truckCode || '').toLowerCase();
                var trailerCode = (truck.trailer && truck.trailer.truckCode || '').toLowerCase();
                if (!(truckCode.startsWith(query)
                    || !_settings.showTrailersOnSchedule && trailerCode.startsWith(query))
                ) {
                    return;
                }

                var trailer = truck.trailer;
                if (truck.vehicleCategory.assetType === abp.enums.assetType.trailer) {
                    var tractor = getTractorForTrailer(truck);
                    if (!tractor) {
                        return;
                    }
                    trailer = {
                        id: truck.id,
                        truckCode: truck.truckCode,
                    };
                    truck = tractor;
                    if (!canAddTruckWithDriverToOrderLine(truck, truck.driverId, orderLine)) {
                        return;
                    }
                    if (result.some(r => r.truckId === truck.id && r.trailer && r.trailer.id === trailer.id)) {
                        return;
                    }
                }

                result.push({
                    id: 0,
                    parentId: null,
                    truckId: truck.id,
                    truckCode: truck.truckCode,
                    truckCodeCombined: getCombinedTruckCode(truck),
                    textForLookup: getCombinedTruckCode(truck),
                    trailer: trailer,
                    driverId: truck.driverId,
                    orderId: orderLine.orderId,
                    orderLineId: orderLine.id,
                    isExternal: truck.isExternal,
                    officeId: truck.officeId,
                    //utilization, //will be populated after it is added to the order line
                    vehicleCategory: {
                        id: truck.vehicleCategory.id,
                        name: truck.vehicleCategory.name,
                        assetType: truck.vehicleCategory.assetType,
                        isPowered: truck.vehicleCategory.isPowered,
                        sortOrder: truck.vehicleCategory.sortOrder
                    },
                    alwaysShowOnSchedule: truck.alwaysShowOnSchedule,
                    canPullTrailer: truck.canPullTrailer,
                    isDone: false,
                    //timeOnJob, //will be populated after it is added to the order line
                    leaseHaulerId: truck.leaseHaulerId,
                    dispatches: [],
                });
            });

            if (!result.length) {
                _driverAssignments
                    .filter(da => da.driverId && da.driverIsActive && !da.driverIsExternal)
                    .forEach(driverAssignment => {
                        var truck = _scheduleTrucks.find(t => t.id === driverAssignment.truckId);
                        if (!truck) {
                            return;
                        }

                        if (!canAddTruckWithDriverToOrderLine(truck, driverAssignment.driverId, orderLine)) {
                            return;
                        }

                        var driverName = ((driverAssignment.driverLastName || '') + ", " + (driverAssignment.driverFirstName || ''));
                        var driverNameWithTruck = driverName + " - " + (getCombinedTruckCode(truck) || '');
                        //if (!driverName.toLowerCase().startsWith(query)) { //only matching by driverName causes issues when the text is autocompleted with Tab first and only then they hit Enter, as opposed to just hitting Enter. We need to match against a complete string to avoid issues
                        if (!driverNameWithTruck.toLowerCase().startsWith(query)) {
                            return;
                        }

                        result.push({
                            id: 0,
                            parentId: null,
                            orderId: orderLine.orderId,
                            truckId: truck.id,
                            truckCode: truck.truckCode,
                            truckCodeCombined: getCombinedTruckCode(truck),
                            trailer: truck.trailer,
                            driverId: driverAssignment.driverId,
                            textForLookup: driverNameWithTruck,
                            officeId: truck.officeId,
                            isExternal: truck.isExternal,
                            leaseHaulerId: truck.leaseHaulerId,
                            vehicleCategory: {
                                id: truck.vehicleCategory.id,
                                name: truck.vehicleCategory.name,
                                assetType: truck.vehicleCategory.assetType,
                                isPowered: truck.vehicleCategory.isPowered,
                                sortOrder: truck.vehicleCategory.sortOrder
                            },
                            canPullTrailer: truck.canPullTrailer,
                            alwaysShowOnSchedule: truck.alwaysShowOnSchedule
                        });

                    });
            }

            syncCallback(result);
        }

        function getOrderPriorityClass(orderLine) {
            switch (orderLine.priority) {
                case abp.enums.orderPriority.high: return 'order-priority-icon-high fas fa-arrow-circle-up';
                case abp.enums.orderPriority.medium: return 'order-priority-icon-medium far fa-circle';
                case abp.enums.orderPriority.low: return 'order-priority-icon-low fas fa-arrow-circle-down';
            }
            return '';
        }

        function getOrderPriorityTitle(orderLine) {
            switch (orderLine.priority) {
                case abp.enums.orderPriority.high: return 'High Priority';
                case abp.enums.orderPriority.medium: return 'Medium Priority';
                case abp.enums.orderPriority.low: return 'Low Priority';
            }
            return '';
        }

        function allowScheduling(truck) {
            var dateFilter = moment($('#DateFilter').val(), 'MM/DD/YYYY');
            if (_settings.notAllowSchedulingLeaseHaulersWithExpiredInsurance) {
                if (truck.insurances != null) {
                    if (!truck.insurances.some(c => c.isActive)
                        || truck.insurances.some(i => moment(i.expirationDate, 'YYYY-MM-DD') < dateFilter && i.isActive)) {
                        return false;
                    } else {
                        return true;
                    }
                } else {
                    return true;
                }
            } else {
                return true;
            }
        }

        function getTruckTileClass(truck) {
            if (truck.vehicleCategory.assetType === abp.enums.assetType.trailer && truck.tractor) {
                let tractor = _scheduleTrucks.find(x => x.id === truck.tractor.id);
                if (tractor) {
                    return getTruckTileClass(tractor);
                }
            }
            if (truck.isOutOfService) {
                return "gray";
            }
            if (!allowScheduling(truck)) {
                return "gray";
            }
            if (truckHasNoDriver(truck) && truckCategoryNeedsDriver(truck)) {
                return "blue";
            }
            if (_settings.validateUtilization) {
                if (truck.utilization >= 1) {
                    return "red";
                }
                if (truck.utilization > 0) {
                    return "yellow";
                }
                return "green";
            } else {
                if (truck.utilization > 1) {
                    return "red";
                }
                if (truck.utilization === 1) {
                    return "yellow";
                }
                if (truck.utilization > 0) {
                    return "green";
                }
                return "white";
            }
        }

        function getTruckTileWidthClass(truck) {
            if (_settings.showTrailersOnSchedule) {
                if (truck.trailer || truck.tractor) {
                    return 'double-width';
                }
            }
            return '';
        }

        function truckCategoryNeedsDriver(truck) {
            return truck.vehicleCategory.isPowered
                && (_features.leaseHaulers || (!truck.alwaysShowOnSchedule && !truck.isExternal)); //&& truck.officeId !== null
        }

        function getTruckTileOfficeClass(truck) {
            var officeId = parseInt($('#OfficeIdFilter').val());
            if (truck.officeId !== officeId) {
                return 'truck-office-' + truck.officeId;
            }
            return '';
        }

        function getTruckTilePointerClass(truck) {
            if (truck.utilization > 0) {
                return 'hand';
            }
            return '';
        }

        function getCombinedTruckCodeFieldName() {
            return _settings.showTrailersOnSchedule ? 'truckCodeCombined' : 'truckCode';
        }

        function getCombinedTruckCode(truck) {
            if (_settings.showTrailersOnSchedule) {
                if (truck.canPullTrailer && truck.trailer) {
                    return truck.truckCode + ' :: ' + truck.trailer.truckCode;
                }
                if (truck.vehicleCategory.assetType === abp.enums.assetType.trailer && truck.tractor) {
                    return truck.tractor.truckCode + ' :: ' + truck.truckCode;
                }
            }
            return truck.truckCode;
        }

        function getTruckSpec(truck) {
            const type = truck.vehicleCategory.assetType === abp.enums.assetType.trailer ? 'Trailer' : 'Truck';
            let spec = '';
            spec += '\n' + `${type}: ${truck.truckCode}`;
            spec += '\n' + truck.vehicleCategory.name;
            spec += truck.bedConstructionFormatted ? ` ${truck.bedConstructionFormatted}` : '';
            spec += truck.year ? ` ${truck.year}` : '';
            spec += truck.make ? ` ${truck.make}` : '';
            spec += truck.model ? ` ${truck.model}` : '';
            return spec;
        }

        function getTruckTileTitle(truck) {
            let title = '';
            if (truck.vehicleCategory.assetType === abp.enums.assetType.dumpTruck
                || truck.vehicleCategory.assetType === abp.enums.assetType.tractor
            ) {
                title += `Driver: ${truck.driverName}`;
                if (truck.canPullTrailer && truck.trailer) {
                    title += getTruckSpec(truck);
                    title += getTruckSpec(truck.trailer);
                } else {
                    title += getTruckSpec(truck);
                }
            } else if (truck.vehicleCategory.assetType === abp.enums.assetType.trailer) {
                title += getTruckSpec(truck);
                title += truck.tractor ? '\n' + `Currently coupled to truck ${truck.tractor.truckCode}` : '';
            } else {
                title += `Driver: ${truck.driverName}`;
                title += getTruckSpec(truck);
            }
            return title;
        }

        function getScheduleTruckDispatchStatusClass(truck) {
            if (!truck.dispatches?.length) {
                return 'schedule-dispatch-status-inactive';
            }
            if (truck.dispatches.some(x => x.status === abp.enums.dispatchStatus.acknowledged || x.status === abp.enums.dispatchStatus.loaded)) {
                return 'schedule-dispatch-status-dgreen';
            }
            if (truck.dispatches.some(x => x.status === abp.enums.dispatchStatus.created || x.status === abp.enums.dispatchStatus.sent)) {
                return 'schedule-dispatch-status-lgreen';
            }
            if (truck.dispatches.some(x => x.status === abp.enums.dispatchStatus.completed)) {
                return 'schedule-dispatch-status-active';
            }
            return 'schedule-dispatch-status-inactive';
        }

        function getLeaseHaulerRequestClass(req) {
            switch (req.status) {
                case abp.enums.leaseHaulerRequestStatus.requested: return 'requested';
                case abp.enums.leaseHaulerRequestStatus.accepted: return 'accepted';
                case abp.enums.leaseHaulerRequestStatus.rejected: return 'rejected';
            }
            return '';
        }

        async function setOrderIsClosedValue(orderLineId, val, isCancelled) {
            try {
                abp.ui.setBusy();
                await _schedulingService.setOrderLineIsComplete({
                    orderLineId: orderLineId,
                    isComplete: val,
                    isCancelled: isCancelled
                }).done(function () {
                    reloadMainGrid(null, false);
                    reloadTruckTiles();
                });
            }
            finally {
                abp.ui.clearBusy();
            }
        }

        function setTruckIsOutOfServiceValue(truckId, val) {
            if (!val) {
                _truckService.setTruckIsOutOfService({
                    truckId: truckId,
                    isOutOfService: val
                }).done(function () {
                    reloadMainGrid(null, false);
                    reloadTruckTiles();
                });
            } else {
                _addOutOfServiceReasonModal.open({ truckId: truckId, date: _dtHelper.getFilterData().date });
            }
        }
        abp.event.on('app.addOutOfServiceReasonModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        async function acceptLeaseHaulerRequest(leaseHaulerRequestId) {
            try {
                abp.ui.setBusy();
                await _schedulingService.setLeaseHaulerRequestAsAccepted({
                    leaseHaulerRequestId: leaseHaulerRequestId
                }).done(function () {
                    reloadMainGrid(null, false);
                });
            }
            finally {
                abp.ui.clearBusy();
            }
        }

        function isAllowedToEditOrder(orderLine) {
            return true; //orderLine.officeId === abp.session.officeId;
        }

        function isAllowedToEditOrderClosedState(orderLine) {
            return true; //orderLine.officeId === abp.session.officeId;
        }

        function isAllowedToEditOrderTrucks(orderLine) {
            return orderLine.isClosed === false;
        }

        function isTodayOrFutureDate(orderLine) {
            var today = new Date(moment().format("YYYY-MM-DD") + 'T00:00:00Z');
            return (new Date(orderLine.date)).getTime() >= today.getTime();
        }

        function hasOrderEditPermissions() {
            return _permissions.edit;
        }
        function hasTicketEditPermissions() {
            return _permissions.edit && _permissions.editTickets;
        }
        function hasOrderPrintPermissions() {
            return _permissions.print;
        }
        function hasTrucksPermissions() {
            return _permissions.trucks;
        }
        function hasViewLeaseHaulerJobPermission() {
            return _permissions.leaseHaulerSchedule;
        }
        function hasAcceptLeaseHaulerJobPermission() {
            return _permissions.acceptLeaseHaulerJob;
        }
        function hasRejectLeaseHaulerJobPermission() {
            return _permissions.rejectLeaseHaulerJob;
        }

        var staggeredIcon = ' <span class="far fa-clock staggered-icon pull-right" title="Staggered"></span>';
        function handleTimepickerCellCreation(fieldName, saveMethod, isTimeEditableField, isTimeStaggeredField) {
            return function (cell, cellData, rowData, rowIndex, colIndex) {
                $(cell).empty();
                var isTimeStaggered = function (rowData) {
                    return isTimeStaggeredField ? rowData[isTimeStaggeredField] : false;
                }
                var isTimeEditable = function (rowData) {
                    return isTimeEditableField ? rowData[isTimeEditableField] : true;
                }
                var getFormattedCellValue = function () {
                    return _dtHelper.renderTime(rowData[fieldName], '');
                };
                var oldValue = getFormattedCellValue();
                var setRowValue = function (value) {
                    rowData[fieldName] = value;
                    oldValue = value;
                };
                if (!isAllowedToEditOrder(rowData) || !isTimeEditable(rowData)) {
                    var text = getFormattedCellValue();
                    if (isTimeStaggered(rowData)) {
                        $(cell).html(text + staggeredIcon);
                    } else {
                        $(cell).text(text);
                    }
                    $(cell).removeClass('cell-editable');
                    return;
                }
                var editor = $('<input type="text">').appendTo($(cell));
                if (isTimeStaggered(rowData)) {
                    editor.addClass('with-staggered-icon');
                    $(staggeredIcon).removeClass('pull-right').appendTo($(cell));
                }
                editor.val(oldValue);
                editor.focusout(function (e) {
                    var newValue = $(this).val();
                    if (newValue === (oldValue || "")) {
                        return;
                    }
                    if (newValue && !abp.helper.isTimeString(newValue)) {
                        abp.message.error('Enter a valid time in a format like 11:24 PM');
                        $(this).val(oldValue);
                        return;
                    }
                    abp.ui.setBusy(cell);
                    var request = {
                        orderLineId: rowData.id
                    };
                    request[fieldName] = newValue;
                    saveMethod(
                        request
                    ).done(function () {
                        setRowValue(newValue);
                        abp.notify.info('Saved successfully.');
                    }).always(function () {
                        abp.ui.clearBusy(cell);
                    });
                });
            };
        }

        function handleLocationCellClickForEdit(idField) {
            return function (cell, cellData, rowData, rowIndex, colIndex) {
                $(cell).click(function () {
                    if (!hasOrderEditPermissions()) return;
                    _schedulingService.isOrderLineFieldReadonly({ orderLineId: rowData.id, fieldName: abp.utils.toPascalCase(idField) })
                        .done(function (result) {
                            if (result) {
                                return;
                            }
                            _createOrEditJobModal.open({
                                orderLineId: rowData.id,
                                focusFieldId: idField
                            });
                        });
                });
            };
        }

        function refreshDateRelatedButtonsVisibility() {
            refreshDriverAssignmentButtonVisibility();
            refreshSendOrdersToDriversButtonVisibility();
        }

        function refreshDriverAssignmentButtonVisibility() {
            $('#AddDefaultDriverAssignmentsButton').closest('li').toggle(!isPastDate());
        }

        function refreshSendOrdersToDriversButtonVisibility() {
            $('#SendOrdersToDriversButton').closest('li').toggle(!isPastDate());
        }

        function refreshProgressBarColumnVisibility() {
            scheduleGrid.column('progress:name').visible(!$('#HideProgressBar').is(':checked') && showDispatchItems && showProgressColumn && isToday());
        }

        function refreshHideProgressBarCheckboxVisibility() {
            if (!showDispatchItems || !showProgressColumn || !isToday()) {
                $('#HideProgressBar').closest('div').hide();
            } else {
                $('#HideProgressBar').closest('div').show();
            }
        }

        $('#DateFilter').on('dp.change', function () {
            if (moment($(this).val(), 'MM/DD/YYYY').isValid()) {
                reloadTruckTiles();
                reloadDriverAssignments();
                reloadMainGrid();
                refreshHideProgressBarCheckboxVisibility();
                refreshDateRelatedButtonsVisibility();
            }
        });
        $('#DateFilter').blur(function () {
            if (!moment($(this).val(), 'MM/DD/YYYY').isValid()) {
                $(this).val(moment().format("MM/DD/YYYY"));
            }
        });

        $('#OfficeIdFilter, #HideCompletedOrders, #ShiftFilter, #HideProgressBar, #TruckCategoryIdFilter').change(function () {
            if (!_loadingState && moment($('#DateFilter').val(), 'MM/DD/YYYY').isValid()) {
                reloadTruckTiles();
                reloadDriverAssignments();
                reloadMainGrid();
            }
        });
        //reloadTruckTiles();

        // Menu functions definitions
        var menuFunctions = {
            isVisible: {},
            fn: {}
        };
        menuFunctions.isVisible.viewEdit = function (rowData) {
            return _permissions.schedule && hasOrderEditPermissions();
        };
        menuFunctions.fn.viewEdit = function (element) {
            var orderId = _dtHelper.getRowData(element).orderId;
            _createOrEditOrderModal.open({ id: orderId });
        };
        menuFunctions.isVisible.viewOrEditJob = function (rowData) {
            return _permissions.edit
                || _features.leaseHaulerPortal
                && _permissions.leaseHaulerSchedule
                && (!_features.jobBasedLeaseHaulerRequest || rowData.leaseHaulerRequests.length);
        };
        menuFunctions.fn.viewOrEditJob = function (element) {
            _createOrEditJobModal.open({
                orderLineId: _dtHelper.getRowData(element).id
            });
        };
        menuFunctions.isVisible.acceptJob = function (rowData) {
            return hasAcceptLeaseHaulerJobPermission() && rowData.leaseHaulerRequests[0]?.status === abp.enums.leaseHaulerRequestStatus.requested;
        };
        menuFunctions.fn.acceptJob = async function (element) {
            await acceptLeaseHaulerRequest(_dtHelper.getRowData(element).leaseHaulerRequests[0].id);
        };
        menuFunctions.isVisible.rejectJob = function (rowData) {
            return hasRejectLeaseHaulerJobPermission() && rowData.leaseHaulerRequests[0]?.status === abp.enums.leaseHaulerRequestStatus.requested;
        };
        menuFunctions.fn.rejectJob = function (element) {
            _rejectLeaseHaulerRequestModal.open({
                id: _dtHelper.getRowData(element).leaseHaulerRequests[0].id
            });
        };
        menuFunctions.isVisible.assignLeaseHaulerTrucks = function (rowData) {
            return hasViewLeaseHaulerJobPermission()
                && !rowData.trucks.length
                && rowData.leaseHaulerRequests[0]?.status !== abp.enums.leaseHaulerRequestStatus.rejected;
        };
        menuFunctions.fn.assignLeaseHaulerTrucks = function (element) {
            const orderLine = _dtHelper.getRowData(element);
            _addOrEditLeaseHaulerRequestModal.open({
                orderLineId: orderLine.id,
                leaseHaulerRequestId: orderLine.leaseHaulerRequests[0].id
            });
        };
        menuFunctions.isVisible.markComplete = function (rowData) {
            return _permissions.schedule && hasOrderEditPermissions() && rowData.isClosed === false && isAllowedToEditOrderClosedState(rowData);
        };
        menuFunctions.fn.markComplete = async function (element, options) {
            options = options || {};
            var orderLineId = _dtHelper.getRowData(element).id;
            try {
                abp.ui.setBusy();
                if (await _dispatchingService.dispatchesExist({
                    orderLineId: orderLineId,
                    isMultipleLoads: false,
                    dispatchStatuses: [abp.enums.dispatchStatus.loaded]
                })) {
                    let actionDescription = options.isCancelled ? 'cancelled' : 'complete';
                    let prompt = `There are loaded dispatches associated with this order. Marking this order ${actionDescription} will remove these dispatches. Are you sure you want to do this?`;
                    abp.ui.clearBusy();
                    if (!await abp.message.confirm(prompt)) {
                        return;
                    }
                    abp.ui.setBusy();
                }
                await setOrderIsClosedValue(orderLineId, true, options.isCancelled);
            }
            finally {
                abp.ui.clearBusy();
            }
        };
        menuFunctions.isVisible.reOpenJob = function (rowData) {
            return _permissions.schedule && hasOrderEditPermissions() && rowData.isClosed === true && isAllowedToEditOrderClosedState(rowData);
        };
        menuFunctions.fn.reOpenJob = async function (element) {
            await setOrderIsClosedValue(_dtHelper.getRowData(element).id, false);
            abp.notify.info('Saved successfully.');
        };
        menuFunctions.isVisible.copy = function (rowData) {
            return _permissions.schedule && hasOrderEditPermissions() && isAllowedToEditOrderClosedState(rowData);
        };
        menuFunctions.fn.copy = async function (element) {
            var orderLine = _dtHelper.getRowData(element);
            _copyOrderModal.open({
                orderId: orderLine.orderId,
                orderLineId: orderLine.id,
            });
        };
        menuFunctions.isVisible.carryUnfinishedPortionForward = function (rowData) {
            return _permissions.schedule && hasOrderEditPermissions() && isAllowedToEditOrderClosedState(rowData);
        };
        menuFunctions.fn.carryUnfinishedPortionForward = async function (element) {
            var orderLine = _dtHelper.getRowData(element);
            _carryUnfinishedPortionForwardModal.open({
                orderId: orderLine.orderId,
                orderLineId: orderLine.id,
            });
        };
        menuFunctions.isVisible.transfer = function (rowData) {
            return _permissions.schedule && _features.allowMultiOffice && hasOrderEditPermissions() && !rowData.isClosed && isAllowedToEditOrder(rowData);
        };
        menuFunctions.fn.transfer = function (element) {
            var orderLineId = _dtHelper.getRowData(element).id;
            _setOrderOfficeIdModal.open({ id: orderLineId }).fail(handlePopupException);
        };
        menuFunctions.isVisible.sendOrderToLeaseHauler = function (rowData) {
            return _permissions.schedule && _features.allowSendingOrdersToDifferentTenant && hasOrderEditPermissions() && !rowData.isClosed && !isPastDate();
        };
        menuFunctions.fn.sendOrderToLeaseHauler = function (element) {
            var orderLineId = _dtHelper.getRowData(element).id;
            _sendOrderLineToHaulingCompanyModal.open({ orderLineId: orderLineId }).fail(handlePopupException);
        };
        menuFunctions.isVisible.changeDate = function (rowData) {
            return _permissions.schedule && hasOrderEditPermissions() && !rowData.isClosed && isAllowedToEditOrder(rowData);
        };
        menuFunctions.fn.changeDate = function (element) {
            var orderLine = _dtHelper.getRowData(element);
            _setOrderDateModal.open({ orderLineId: orderLine.id }).fail(handlePopupException);
        };
        menuFunctions.isVisible.reassignTrucks = function (rowData) {
            return hasOrderEditPermissions() && isAllowedToEditOrder(rowData) && isTodayOrFutureDate(rowData);
        };
        menuFunctions.fn.reassignTrucks = function (element) {
            var orderLine = _dtHelper.getRowData(element);
            _reassignTrucksModal.open({ orderLineId: orderLine.id }).fail(handlePopupException);
        };
        menuFunctions.isVisible.markAllLeaseHaulerTrucksComplete = function (rowData) {
            return _permissions.leaseHaulerSchedule && _features.jobBasedLeaseHaulerRequest && _settings.allowLeaseHaulerRequestProcess && rowData.isClosed === false;
        };
        menuFunctions.fn.markAllLeaseHaulerTrucksComplete = async function (element) {
            await menuFunctions.fn.markComplete(element, { isCancelled: false });
        };
        menuFunctions.isVisible.delete = function (rowData) {
            return _permissions.schedule && hasOrderEditPermissions() && isAllowedToEditOrder(rowData);
        };
        menuFunctions.fn.delete = async function (element) {
            var orderLine = _dtHelper.getRowData(element);
            if (!await abp.message.confirm('Are you sure you want to delete the order line for the "' + orderLine.customerName + '"?')) {
                return;
            }
            try {
                abp.ui.setBusy();
                let hasMultipleLines = await _orderService.doesOrderHaveOtherOrderLines(orderLine.orderId, orderLine.id);
                if (!hasMultipleLines) {
                    await deleteOrder(orderLine.orderId);
                    return;
                }
                abp.ui.clearBusy();
                let multipleOrderLinesResponse = await swal(
                    "There are multiple line items associated with this order. Select the button below corresponding with what you want to delete.",
                    {
                        buttons: {
                            cancel: "Cancel",
                            single: "Single line item",
                            all: "Entire order"
                        }
                    }
                );
                abp.ui.setBusy();
                switch (multipleOrderLinesResponse) {
                    case "single":
                        await deleteOrderLine(orderLine.id, orderLine.orderId);
                        return;
                    case "all":
                        await deleteOrder(orderLine.orderId);
                        return;
                    default:
                        return;
                }
            }
            finally {
                abp.ui.clearBusy();
            }

            async function deleteOrderLine(orderLineId, orderId) {
                await _orderService.deleteOrderLine({
                    id: orderLineId,
                    orderId: orderId
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
            async function deleteOrder(orderId) {
                await _orderService.deleteOrder({
                    id: orderId
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        };
        menuFunctions.fn.printNoPrices = function (element) {
            var orderId = _dtHelper.getRowData(element).orderId;
            app.openPopup(abp.appPath + 'app/orders/GetWorkOrderReport?' + 'id=' + orderId + '&hidePrices=true');
        };
        menuFunctions.fn.printCombinedPrices = function (element) {
            var orderId = _dtHelper.getRowData(element).orderId;
            app.openPopup(abp.appPath + 'app/orders/GetWorkOrderReport?' + 'id=' + orderId);
        };
        menuFunctions.fn.printSeparatePrices = function (element) {
            var orderId = _dtHelper.getRowData(element).orderId;
            var options = app.order.getOrderWithSeparatePricesReportOptions({ id: orderId });
            app.openPopup(abp.appPath + 'app/orders/GetWorkOrderReport?' + $.param(options));
        };
        menuFunctions.fn.printBackOfficeDetail = function (element) {
            var orderId = _dtHelper.getRowData(element).orderId;
            var options = app.order.getBackOfficeReportOptions({ id: orderId });
            app.openPopup(abp.appPath + 'app/orders/GetWorkOrderReport?' + $.param(options));
        };
        menuFunctions.fn.printWithDeliveryInfo = function (element) {
            var orderId = _dtHelper.getRowData(element).orderId;
            _printOrderWithDeliveryInfoModal.open({ id: orderId });
        };
        menuFunctions.isVisible.tickets = function (rowData) {
            return hasTicketEditPermissions()
                && (
                    abp.session.officeIds.includes(rowData.officeId)
                    || !abp.setting.getBoolean('App.General.SplitBillingByOffices')
                )
                || _permissions.leaseHaulerTickets && _permissions.editLeaseHaulerJob
                || _permissions.leaseHaulerTruckRequest;
        };
        menuFunctions.fn.tickets = function (element) {
            var orderLineId = _dtHelper.getRowData(element).id;
            _createOrEditTicketModal.open({ orderLineId: orderLineId });
        };
        menuFunctions.isVisible.showMap = function (rowData) {
            return showDispatchViaGeotabItems;
        };
        menuFunctions.fn.showMap = function (element) {
            var orderLineId = _dtHelper.getRowData(element).id;
            app.openPopup('scheduling/ShowMap?orderLineId=' + orderLineId, '_blank');
        };
        menuFunctions.isVisible.showTripsReport = function (rowData) {
            return showDispatchViaGeotabItems;
        };
        menuFunctions.fn.showTripsReport = function (element) {
            _tripsReportModal.open();
        };
        menuFunctions.isVisible.showCycleTimes = function (rowData) {
            return showDispatchViaGeotabItems;
        };
        menuFunctions.fn.showCycleTimes = function (element) {
            _cycleTimesModal.open();
        };
        function thereAreNotDoneTrucks(rowData) {
            return rowData.trucks.some(function (truck) {
                return !truck.isDone && truck.vehicleCategory.isPowered;
            });
        }
        function thereAreNotDoneAndNotLeasedTrucks(rowData) {
            return rowData.trucks.some(function (truck) {
                return !truck.isDone && truck.vehicleCategory.isPowered && !truck.alwaysShowOnSchedule; //truck.officeId == null (external lease haulers) are included
            });
        }
        menuFunctions.isVisible.dispatchToDriver = function (rowData) {
            var today = new Date(moment().format("YYYY-MM-DD") + 'T00:00:00Z');
            return showDispatchItems
                && !rowData.isClosed
                && rowData.trucks.length > 0
                && thereAreNotDoneTrucks(rowData)
                && (new Date(rowData.date)).getTime() >= today.getTime();
        };
        menuFunctions.fn.dispatchToDriver = function (element) {
            var orderLineId = _dtHelper.getRowData(element).id;
            sendDispatchMessage({ orderLineId: orderLineId });
        };
        menuFunctions.isVisible.messageToDriver = function (rowData) {
            return allowSmsMessages && _permissions.driverMessages && thereAreNotDoneTrucks(rowData);
        };
        menuFunctions.fn.messageToDriver = function (element) {
            var orderLineId = _dtHelper.getRowData(element).id;
            _sendDriverMessageModal.open({ orderLineId: orderLineId });
        };
        menuFunctions.isVisible.viewDispatches = function (rowData) {
            return showDispatchViaSmsItems;
        };
        menuFunctions.fn.viewDispatches = function (element) {
            var orderLineId = _dtHelper.getRowData(element).id;
            window.location.href = abp.appPath + 'app/Dispatches/?orderLineId=' + orderLineId;
        };
        menuFunctions.isVisible.activateClosedTrucks = function (rowData) {
            return hasOrderEditPermissions()
                && !rowData.isClosed
                && rowData.trucks.some(t => t.isDone);
        };
        menuFunctions.fn.activateClosedTrucks = function (element) {
            var rowData = _dtHelper.getRowData(element);
            if (_settings.validateUtilization && (rowData.maxUtilization === 0 || rowData.maxUtilization - rowData.utilization <= 0)) {
                abp.notify.error("Increase # of Trucks");
                return;
            }

            var orderLineId = _dtHelper.getRowData(element).id;
            _activateClosedTrucksModal.open({ orderLineId: orderLineId });
        };
        menuFunctions.isVisible.viewJobSummary = function (rowData) {
            return _permissions.viewJobSummary;
        };
        menuFunctions.fn.viewJobSummary = async function (element) {
            var rowData = _dtHelper.getRowData(element);
            abp.ui.setBusy();
            window.location = abp.appPath + 'app/jobsummary/details/' + rowData.id;
        };

        function getResponsivePriorityByName(name) {
            //the highest will be hidden first
            let columnsToHide = [
                'isClosed',
                'priority',
                'jobNumber',
                'truckType',
                'quantityFormatted',
                'time',
                'item',
                'freightItem',
                'materialItem',
                'progress',
                'customerName'
            ];
            var index = columnsToHide.indexOf(name);
            if (index === -1) {
                return 0;
            }
            return columnsToHide.length - index;
        }

        function getNoteIcon(rowData) {
            const icon = $('<i class="fa-regular fa-files directions-icon" data-toggle="tooltip" data-html="true"></i>');
            const hasNote = rowData.note;
            const canEdit = _permissions.edit || _permissions.editLeaseHaulerJob;

            if (hasNote) {
                let title = abp.utils.replaceAll(rowData.note, '\n', '<br>');
                if (canEdit) {
                    title += '<br><br><b>Click icon to edit comments</b>';
                }
                icon.prop('title', title);
            } else if (canEdit) {
                icon.prop('title', '<b>Click icon to add comments</b>');
            }

            if (canEdit) {
                icon.click(function () {
                    _createOrEditJobModal.open({
                        orderLineId: rowData.id,
                        focusFieldId: 'Note'
                    });
                });
            }

            if (!hasNote) {
                icon.addClass('gray');
            }

            return icon;
        }

        function getCommentsIcon(rowData) {
            if (!rowData.directions) {
                return $();
            }

            const commentsIcon = $('<i class="fa-regular fa-files directions-icon" data-toggle="tooltip" data-html="true"></i>');
            commentsIcon.addClass('text-info');
            commentsIcon.prop('title', abp.utils.replaceAll(rowData.directions, '\n', '<br>'));

            return commentsIcon;
        }

        var scheduleTable = $('#ScheduleTable');
        var scheduleGrid = scheduleTable.DataTableInit({
            stateSave: true,
            stateDuration: 0,
            stateLoadCallback: function (settings, callback) {
                app.localStorage.getItem('schedule_filter', function (result) {
                    var filter = result || {};

                    _loadingState = true;
                    if (filter.date) {
                        $('#DateFilter').val(filter.date);
                        refreshHideProgressBarCheckboxVisibility();
                        refreshDateRelatedButtonsVisibility();
                    }
                    if (filter.shift) {
                        $('#ShiftFilter').val(filter.shift).trigger("change");
                    }
                    if (filter.officeId !== undefined) { //allow null
                        abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), filter.officeId, filter.officeName);
                        $('#OfficeIdFilter').val(filter.officeId).trigger("change");
                    }
                    if (filter.truckCategoryId !== undefined) {
                        abp.helper.ui.addAndSetDropdownValue($("#TruckCategoryIdFilter"), filter.truckCategoryId, filter.truckCategoryName);
                        $('#TruckCategoryIdFilter').val(filter.truckCategoryId).trigger("change");
                    }
                    if (filter.hideCompletedOrders) {
                        $('#HideCompletedOrders').prop('checked', true);
                    }
                    if (filter.hideProgressBar) {
                        $('#HideProgressBar').prop('checked', true);
                    }
                    _loadingState = false;
                    reloadTruckTiles();
                    reloadDriverAssignments();

                    app.localStorage.getItem('schedule_grid', function (result) {
                        callback(JSON.parse(result));
                    });
                });
            },
            stateSaveCallback: function (settings, data) {
                delete data.columns;
                delete data.search;
                app.localStorage.setItem('schedule_grid', JSON.stringify(data));
                app.localStorage.setItem('schedule_filter', _dtHelper.getFilterData());
            },
            searching: false,
            paging: false,
            serverSide: true,
            processing: true,
            pageLength: 100,
            ajax: async function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                if (!_orderLines || !_orderLineGridCache) {
                    let abpResult = await _schedulingService.getScheduleOrders(abpData);
                    _orderLines = abpResult.items;
                    _orderLineGridCache = abpResult;
                }
                refreshProgressBarColumnVisibility();
                callback(_dtHelper.fromAbpResult(sortOrderLines(_orderLineGridCache, abpData)));
                $('.tt-input').attr('size', '1'); //fixes the size of Trucks enter-new-tag-input (by default it's too big and sometimes jumps to a new line)
            },
            order: [[3, 'asc']],
            headerCallback: function (thead, data, start, end, display) {
            },
            footerCallback: function (row, data, start, end, display) {
                recalculateFooterTotals();
            },
            responsive: {
                details: {
                    renderer: function (api, rowIdx, columns) {
                        var data = $.map(columns, function (col, i) {
                            return col.hidden ?
                                '<tr data-dt-row="' + col.rowIndex + '" data-dt-column="' + col.columnIndex + '">' +
                                '<td>' + col.title + (col.title ? ':' : '') + '</td> ' +
                                '<td>' + col.data + '</td>' +
                                '</tr>' :
                                '';
                        }).join('');

                        return data ? $('<table/>').append(data) : false;
                    }
                }
            },

            columns: [
                {
                    defaultContent: "",
                    className: 'control responsive',
                    orderable: false,
                    title: "&nbsp;",
                    width: "10px",
                    render: function () {
                        return '';
                    }
                },
                {
                    defaultContent: "",
                    data: "priority",
                    title: "",
                    width: "10px",
                    responsivePriority: getResponsivePriorityByName('priority'),
                    className: 'small-padding',
                    render: function (data, type, full, meta) {
                        return '<i class="' + getOrderPriorityClass(full) + '" title="' + getOrderPriorityTitle(full) + '"></i>';
                    }
                },
                {
                    defaultContent: "",
                    data: "customerIsCod",
                    title: "COD",
                    sorting: false,
                    //orderable: false,
                    render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(data); },
                    className: "checkmark all p-1",
                    width: "20px"
                },
                {
                    defaultContent: "",
                    data: 'note',
                    title: "",
                    orderable: false,
                    className: "checkmark all p-2",
                    width: "40px",
                    render: function (data, type, full, meta) {
                        return '';
                    },
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        let noteIcon = getNoteIcon(rowData);
                        $(cell).append(noteIcon);

                        let commentsIcon = getCommentsIcon(rowData);
                        $(cell).append(commentsIcon);
                    }
                },
                {
                    defaultContent: "",
                    className: "all",
                    data: "customerName",
                    responsivePriority: getResponsivePriorityByName('customerName'),
                    title: "Customer",
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        $(cell).wrapInner('<div class="customer-column"></div>');
                    }
                },
                {
                    defaultContent: "",
                    className: "job-number-column",
                    data: "jobNumber",
                    title: app.localize('JobNbr'),
                    responsivePriority: getResponsivePriorityByName('jobNumber')
                },
                {
                    defaultContent: "",
                    data: "time",
                    render: function (data, type, full, meta) { return _dtHelper.renderTime(full.time, '') + (full.isTimeStaggered ? staggeredIcon : ''); },
                    title: 'Time on Job',
                    titleHoverText: 'Time on Job',
                    responsivePriority: getResponsivePriorityByName('time'),
                    width: "94px",
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        $(cell).wrapInner('<div class="time-on-job-column"></div>');
                        $(cell).click(function () {
                            if (rowData["isTimeEditable"]) {
                                _createOrEditJobModal.open({
                                    orderLineId: rowData.id,
                                    focusFieldId: 'TimeOnJob'
                                });
                            }
                        });
                    },
                    orderable: true
                },
                {
                    defaultContent: "",
                    data: "loadAtName",
                    title: "Load at",
                    className: "load-at all",
                    createdCell: handleLocationCellClickForEdit('LoadAtId')
                },
                {
                    defaultContent: "",
                    data: "deliverToName",
                    title: "Deliver to",
                    className: 'deliver-to-column all',
                    createdCell: handleLocationCellClickForEdit('DeliverToId')
                },
                {
                    defaultContent: "",
                    data: "item",
                    title: "Item",
                    visible: !_features.separateItems,
                    responsivePriority: getResponsivePriorityByName('item')
                },
                {
                    defaultContent: "",
                    visible: _features.separateItems,
                    data: "freightItem",
                    title: "Freight Item",
                    responsivePriority: getResponsivePriorityByName('freightItem')
                },
                {
                    defaultContent: "",
                    visible: _features.separateItems,
                    data: "materialItem",
                    title: "Material Item",
                    responsivePriority: getResponsivePriorityByName('materialItem')
                },
                {
                    defaultContent: "",
                    data: "quantityFormatted",
                    orderable: false,
                    title: "Quantity",
                    responsivePriority: getResponsivePriorityByName('quantityFormatted'),
                    width: "100px",
                    render: function (data, type, full, meta) {
                        let span = $('<span>').text(data);
                        return abp.utils.replaceAll(span.html(), '\n', '<br>');
                    }
                },
                {
                    defaultContent: "",
                    data: "truckType",
                    orderable: false,
                    title: "Truck Category",
                    responsivePriority: getResponsivePriorityByName('truckType'),
                    width: "100px",
                    visible: _settings.allowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders,
                    render: function (data, type, full, meta) {
                        return full.vehicleCategoryNames.join(', ');
                    }
                },
                {
                    defaultContent: "",
                    data: "numberOfTrucks",
                    name: "numberOfTrucks",
                    orderable: false,
                    title: '<span title="# of Trucks">Req. Truck</span>',
                    width: "65px",
                    //responsivePriority: 1,
                    className: "cell-number-of-trucks all",
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        $(cell).click(function () {
                            if (!hasOrderEditPermissions()) return;
                            _createOrEditJobModal.open({
                                orderLineId: rowData.id,
                                focusFieldId: 'NumberOfTrucks'
                            });
                        });
                    }
                },
                {
                    defaultContent: "",
                    data: "scheduledTrucks",
                    name: "scheduledTrucks",
                    orderable: false,
                    title: 'Sched. Trucks',
                    width: "65px",
                    visible: _settings.validateUtilization,
                    //responsivePriority: 1,
                    className: "cell-editable cell-scheduled-trucks all",
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        $(cell).empty();
                        if (!hasOrderEditPermissions() || !isAllowedToEditOrder(rowData)) {
                            $(cell).text(rowData.scheduledTrucks);
                            $(cell).removeClass('cell-editable');
                            return;
                        }
                        var editor = $('<input type="text">').appendTo($(cell));
                        editor.val(rowData.scheduledTrucks);
                        editor.focusout(function () {
                            var newValue = $(this).val();
                            if (newValue === (rowData.scheduledTrucks || "").toString()) {
                                return;
                            }
                            if (isNaN(newValue) || parseFloat(newValue) < 0) {
                                abp.message.error('Please enter a valid number!');
                                $(this).val(rowData.scheduledTrucks);
                                return;
                            }
                            else if (parseFloat(newValue) > 999) {
                                abp.message.error('Please enter a valid number less than 1,000!');
                                $(this).val(rowData.scheduledTrucks);
                                return;
                            }

                            newValue = newValue === "" ? null : abp.utils.round(parseFloat(newValue));
                            $(this).val(newValue);
                            var saveCallback = function () {
                                abp.ui.setBusy(cell);
                                _schedulingService.setOrderLineScheduledTrucks({
                                    orderLineId: rowData.id,
                                    scheduledTrucks: newValue
                                }).done(function (result) {
                                    rowData.scheduledTrucks = result.scheduledTrucks;
                                    rowData.utilization = result.orderUtilization;
                                    rowData.maxUtilization = abp.utils.round(result.scheduledTrucks || 0);

                                    updateRowAppearance(editor, rowData);
                                    //scheduleGrid.draw(false);
                                    recalculateFooterTotals();
                                    abp.notify.info('Saved successfully.');
                                }).always(function () {
                                    abp.ui.clearBusy(cell);
                                });
                            };
                            if (rowData.utilization > 0) {
                                if (abp.utils.round(newValue) < rowData.utilization) {
                                    abp.message.warn(app.localize('RemoveSomeTrucks'))
                                        .then(function () {
                                            editor.val(rowData.scheduledTrucks);
                                        });
                                    return;
                                }
                            }
                            saveCallback();
                        });
                    }
                },
                {
                    defaultContent: "",
                    data: null,
                    orderable: false,
                    render: function (data, type, full, meta) {
                        return data.trucks.map(function (t) { return t[getCombinedTruckCodeFieldName()]; }).join(', ');
                    },
                    title: "Trucks",
                    //responsivePriority: 0,
                    className: "trucks all",
                    createdCell: function (cell, cellData, rowData, rowIndex, colIndex) {
                        $(cell).empty();
                        var cancellingTagRemoval = false;
                        if (!hasOrderEditPermissions() || !isAllowedToEditOrderTrucks(rowData)) {
                            $(cell).addClass("readonly");
                        }
                        var editor = $('<input type="text" class="truck-cell-editor">').appendTo($(cell));
                        editor.tagsinput({
                            itemValue: (i) => i.isRequestedLeaseHaulerTruck ? `requested-lh-truck-${i.id}` : i.isLeaseHaulerRequest ? `lh-request-${i.id}` : `truck-${i.truckId}`,
                            itemText: (i) => getTruckTagText(i),
                            itemElement: (i) => getTruckTagElement(i),
                            allowDuplicates: true,
                            isRepeatIconVisible: function (truck) {
                                return hasOpenMultipleLoadsDispatch(truck.dispatches);
                            },
                            tagClass: function (truck) {
                                if (truck.isLeaseHaulerRequest) {
                                    return 'lh-request-tag ' + getLeaseHaulerRequestClass(truck);
                                } else if (truck.isRequestedLeaseHaulerTruck) {
                                    return 'requested-lh-truck-tag '
                                        + getScheduleTruckDispatchStatusClass(truck)
                                        + ' truck-office-' + truck.officeId
                                        + ' ' + getLeaseHaulerRequestClass(truck);
                                }

                                return 'truck-tag '
                                    + (truck.leaseHaulerId ? 'lease-hauler-border ' : '')
                                    + getScheduleTruckDispatchStatusClass(truck)
                                    + ' truck-office-' + truck.officeId
                                    + (truck.isDone ? ' truck-isdone' : '');
                            },
                            typeaheadjs: {
                                displayKey: 'textForLookup',
                                source: function (query, syncCallback, asyncCallback) {
                                    getScheduleTrucksForOrderLine(rowData, query, syncCallback, asyncCallback);
                                },
                                limit: 500
                            }
                        });
                        $.each(rowData.trucks, function (int, truck) {
                            editor.tagsinput('add', truck);
                        });

                        if (rowData.leaseHaulerRequests.length) {
                            $.each(rowData.leaseHaulerRequests, function (int, leaseHaulerRequest) {
                                leaseHaulerRequest.isLeaseHaulerRequest = true;
                                if (leaseHaulerRequest.status !== abp.enums.leaseHaulerRequestStatus.accepted
                                    || !leaseHaulerRequest.requestedTrucks.length
                                ) {
                                    editor.tagsinput('add', leaseHaulerRequest);
                                }

                                if (leaseHaulerRequest.status === abp.enums.leaseHaulerRequestStatus.requested
                                    && leaseHaulerRequest.requestedTrucks.length
                                ) {
                                    $.each(leaseHaulerRequest.requestedTrucks, function (int, requestedTruck) {
                                        requestedTruck.isRequestedLeaseHaulerTruck = true;
                                        editor.tagsinput('add', requestedTruck);
                                    });
                                }
                            });
                        }

                        var orderLineIsFullyUtilized = function () {
                            if (!_settings.validateUtilization) {
                                return false;
                            }
                            return rowData.maxUtilization === 0 || rowData.maxUtilization - rowData.utilization <= 0;
                        };

                        if (_permissions.schedule) {
                            $(cell).find('.tag').not('.truck-isdone').not('.lh-request-tag').not('.requested-lh-truck-tag').draggable({
                                containment: $('table tbody'),
                                revert: 'invalid'
                            });
                            $(cell).droppable({
                                accept: function (element) {
                                    var tag = $(element).data('item');
                                    if (orderLineIsFullyUtilized() || rowData.isClosed || rowData.id === tag.orderLineId) {
                                        return false;
                                    } else {
                                        return true;
                                    }
                                },
                                drop: function (event, ui) {
                                    var tag = ui.helper.data('item');
                                    var moveTruck = function () {
                                        _schedulingService.moveTruck({
                                            truckId: tag.truckId,
                                            sourceOrderLineTruckId: tag.id,
                                            destinationOrderLineId: rowData.id
                                        }).done(function (result) {
                                            reloadMainGrid(null, false);
                                            reloadTruckTiles();
                                            if (result.success) {
                                                abp.notify.info('Truck has been successfully moved.');
                                            } else {
                                                if (result.orderLineTruckExists) {
                                                    if (tag.vehicleCategory.assetType === abp.enums.assetType.trailer) {
                                                        abp.notify.warn("This trailer is already associated with this order line and can't be added again.");
                                                    } else {
                                                        abp.notify.warn("This truck is already assigned to this order and can't be added again. Instead, adjust the utilization if that is what you intend.");
                                                    }
                                                }
                                            }
                                        }).fail(function () {
                                            reloadMainGrid(null, false);
                                        });
                                    };

                                    abp.services.app.scheduling.hasDispatches({
                                        orderLineTruckId: tag.id
                                    }).done(function (result) {
                                        if (result.acknowledgedOrLoaded || result.unacknowledged) {
                                            abp.notify.error('There are active dispatches!');
                                            reloadMainGrid(null, false);
                                        } else {
                                            moveTruck();
                                        }
                                    });
                                }
                            });
                        }

                        let askToAssignDriverAndAddTagAgain = async function (tag, eventOptions) {
                            eventOptions = eventOptions || {};
                            var filterData = _dtHelper.getFilterData();
                            var assignDriverResult = await app.getModalResultAsync(
                                _assignDriverForTruckModal.open({
                                    message: 'There was no driver assigned to this truck. Please select a driver.',
                                    truckId: tag.truckId,
                                    truckCode: tag.truckCode,
                                    leaseHaulerId: tag.leaseHaulerId,
                                    alwaysShowOnSchedule: tag.alwaysShowOnSchedule,
                                    date: filterData.date,
                                    shift: filterData.shift,
                                    officeId: filterData.officeId,
                                    driverId: null,
                                    driverName: null
                                })
                            );

                            tag.driverId = assignDriverResult.driverId;
                            editor.tagsinput('add', tag, {
                                ...eventOptions,
                                preventPost: false,
                                allowNoDriver: true
                            });
                        };

                        let askToAssignTrailerAndAddTagAgain = async function (tag, eventOptions) {
                            eventOptions = eventOptions || {};
                            let trailer = await app.getModalResultAsync(
                                _selectTrailerModal.open({
                                    leaseHaulerId: tag.leaseHaulerId,
                                    optional: true
                                })
                            );

                            tag.trailer = trailer;
                            if (trailer && _settings.showTrailersOnSchedule) {
                                tag.truckCodeCombined = tag.truckCode + ' :: ' + trailer.truckCode;
                            }
                            editor.tagsinput('add', tag, {
                                ...eventOptions,
                                preventPost: false,
                                allowNoTrailer: true
                            });
                            if (trailer) {
                                setTrailerForTractorAsync({
                                    tractorId: tag.truckId,
                                    trailerId: trailer.id
                                });
                            }
                        };

                        editor.on('beforeItemAdd', function (event) {
                            if (cancellingTagRemoval) {
                                cancellingTagRemoval = false;
                                return;
                            }
                            var tag = event.item;
                            if (!hasOrderEditPermissions() || !isAllowedToEditOrderTrucks(rowData)) {
                                event.cancel = true;
                                return;
                            }
                            if (orderLineIsFullyUtilized()) {
                                event.cancel = true;
                                abp.notify.error("Increase # of Trucks");
                                return;
                            }
                            if (!event.options || !event.options.preventPost) {
                                if (!tag.driverId && tag.vehicleCategory.isPowered && (!event.options || !event.options.allowNoDriver)) {
                                    event.cancel = true;
                                    askToAssignDriverAndAddTagAgain(tag, event.options);
                                    return;
                                }

                                if (_settings.showTrailersOnSchedule && !tag.trailer && tag.canPullTrailer && (!event.options || !event.options.allowNoTrailer)) {
                                    event.cancel = true;
                                    askToAssignTrailerAndAddTagAgain(tag, event.options);
                                    return;
                                }

                                var onFail = function (errorMessage) {
                                    editor.tagsinput('remove', tag, { preventPost: true });
                                    abp.notify.error(errorMessage || 'Unknown error occurred on saving the truck assignment');
                                    reloadMainGrid(null, false);
                                    reloadTruckTiles();
                                    reloadDriverAssignments();
                                };
                                _schedulingService.addOrderLineTruck({
                                    orderLineId: rowData.id,
                                    truckId: tag.truckId,
                                    driverId: tag.driverId,
                                    trailerId: tag.trailer?.id,
                                }).done(function (result) {
                                    if (result.isFailed) {
                                        onFail(result.errorMessage);
                                    } else {
                                        abp.notify.info('Saved successfully.');
                                        rowData.utilization = result.orderUtilization;
                                        $.extend(true, tag, result.item);
                                        rowData.trucks.push(tag);
                                        updateRowAppearance(editor, rowData);
                                        $(cell).find('.tag').draggable({
                                            containment: $('table tbody'),
                                            revert: 'invalid'
                                        });

                                        editor.tagsinput('refresh');
                                        reloadTruckTiles();
                                        reloadDriverAssignments();
                                    }
                                }).fail(function () { onFail(); });
                            }
                        });
                        editor.on('beforeItemRemove', function (event) {
                            var tag = event.item;
                            if (!hasOrderEditPermissions() || !isAllowedToEditOrderTrucks(rowData) || tag.isLeaseHaulerRequest) {
                                event.cancel = true;
                                return;
                            }

                            if (event.options?.preventPost) {
                                return;
                            }

                            if (tag.isRequestedLeaseHaulerTruck) {
                                swal('Do you want to remove this lease hauler truck?', {
                                    buttons: {
                                        cancel: "Cancel",
                                        remove: "Remove"
                                    }
                                }).then(function (btnClicked) {
                                    if (btnClicked === "remove") {
                                        _schedulingService.deleteRequestedLeaseHaulerTruck(tag.id).done(function (result) {
                                            abp.notify.info('Removed successfully.');
                                        }).fail(function () {
                                            editor.tagsinput('add', tag, { preventPost: true });
                                            abp.notify.error('Unknown error occured on removing the LeaseHaulerTruck request');
                                        });
                                    } else {
                                        cancellingTagRemoval = true;
                                        editor.tagsinput('add', tag, { preventPost: true });
                                    }
                                });
                            } else {
                                var deleteOrderLineTruck = async function (markAsDone) {
                                    try {
                                        abp.ui.setBusy($(cell));
                                        let result = await _schedulingService.deleteOrderLineTruck({
                                            orderLineTruckId: tag.id,
                                            orderLineId: rowData.id,
                                            markAsDone: markAsDone,
                                        });

                                        rowData.utilization = result.orderLineUtilization;
                                        let trailers = rowData.trucks.filter(x => x.parentId === tag.id);
                                        if (markAsDone) {
                                            trailers.forEach(function (trailer) {
                                                trailer.utilization = 0;
                                                trailer.isDone = true;
                                            });
                                            abp.notify.info('Marked as done successfully.');
                                        } else {
                                            removeScheduleTruckFromArray(rowData.trucks, tag);
                                            trailers.forEach(function (trailer) {
                                                removeScheduleTruckFromArray(rowData.trucks, trailer);
                                                editor.tagsinput('remove', trailer, { preventPost: true });
                                            });
                                            abp.notify.info('Removed successfully.');
                                        }
                                        updateRowAppearance(editor, rowData);
                                        reloadTruckTiles();
                                        editor.tagsinput('refresh');
                                    } catch {
                                        editor.tagsinput('add', tag, { preventPost: true });
                                        abp.notify.error('Unknown error occurred on removing the truck assignment');
                                    } finally {
                                        abp.ui.clearBusy($(cell));
                                    }
                                };

                                abp.scheduling.checkExistingDispatchesBeforeRemovingTruck(
                                    tag.id,
                                    tag.truckCode,
                                    function () {
                                        deleteOrderLineTruck(false);
                                    },
                                    function () {
                                        cancellingTagRemoval = true;
                                        editor.tagsinput('add', tag, { preventPost: true });
                                    },
                                    function () {
                                        cancellingTagRemoval = true;
                                        tag.isDone = true;
                                        editor.tagsinput('add', tag, { preventPost: true });
                                        deleteOrderLineTruck(true);
                                    }
                                );
                            }
                        });
                    }

                },
                {
                    defaultContent: "",
                    data: "amount",
                    orderable: false,
                    name: "progress",
                    visible: showDispatchItems && showProgressColumn && !$('#HideProgressBar').is(':checked'),
                    responsivePriority: getResponsivePriorityByName('progress'),
                    title: "Progress",
                    render: function (data, type, full, meta) {
                        if (full.isCancelled) {
                            return app.localize('Cancel');
                        }
                        if (!showProgressColumn || !isToday()) {
                            return '';
                        }

                        let shouldRenderProgressBar = true;
                        let shouldShowAmountsTooltip = true;
                        let shouldShowNumberOfLoads = false;

                        let designationIsFreightOnly = abp.enums.designations.freightOnly.includes(full.designation);
                        let designationHasMaterial = abp.enums.designations.hasMaterial.includes(full.designation);
                        let amountPercent = 0;
                        let amountOrdered = full.amountOrdered || 0;
                        let amountLoaded = full.amountLoaded || 0;
                        let amountDelivered = full.amountDelivered || 0;

                        if (!designationHasMaterial && !designationIsFreightOnly) {
                            //If the designation is anything else, do not show the progress bar.
                            //Only show the number of loads in the cell. Display the quantities based on the UOM on hover.
                            shouldRenderProgressBar = false;
                            shouldShowNumberOfLoads = true;
                        }

                        if (!amountOrdered) {
                            //order quantity is not specified, then the % complete can’t be calculated.
                            //Show the number of loads in the column, but don’t show the progress bar.
                            shouldRenderProgressBar = false;
                            shouldShowNumberOfLoads = true;
                        }

                        if (designationIsFreightOnly) {
                            switch ((full.freightUom || '').toLowerCase()) {
                                case 'hour':
                                case 'hours':
                                    shouldRenderProgressBar = false;
                                    shouldShowNumberOfLoads = true;
                                    //amountLoaded = abp.utils.round(full.hoursOnDispatchesLoaded);
                                    //amountDelivered = abp.utils.round(full.hoursOnDispatches);
                                    break;
                            }
                        }

                        if (shouldRenderProgressBar) {
                            amountPercent = abp.utils.round(amountDelivered / amountOrdered * 100);
                        }

                        if (isNaN(amountPercent) || amountPercent === null) {
                            amountPercent = 0;
                        }

                        if (isNaN(amountLoaded) || amountLoaded === null) {
                            amountLoaded = 0;
                        }

                        if (isNaN(amountDelivered) || amountDelivered === null) {
                            amountDelivered = 0;
                        }

                        if (full.cargoCapacityRequiredError) {
                            shouldRenderProgressBar = false;
                            shouldShowNumberOfLoads = true;
                            //return getCargoCapacityErrorIcon(full.cargoCapacityRequiredError);
                        }

                        let tooltipTags = 'data-toggle="tooltip" data-html="true" title="<div class=\'text-left\'>Amount loaded: ' + amountLoaded +
                            '</div><div class=\'text-left\'>Amount delivered: ' + amountDelivered +
                            '</div><div class=\'text-left\'>Number of loads: ' + (full.loadCount || '0') +
                            '</div><div class=\'text-left\'>Number of dispatches: ' + (full.dispatchCount || '0') + '</div>"';

                        if (!shouldShowAmountsTooltip) {
                            tooltipTags = '';
                        }

                        if (shouldRenderProgressBar) {
                            return '<div class="progress" ' + tooltipTags + '>' +
                                '<div class="progress-bar ' + (amountPercent > 100 ? 'progress-bar-overflown' : '') + '" role="progressbar" aria-valuenow="' + ((Math.round(amountPercent) > 100) ? Math.round(100) : Math.round(amountPercent)) + '" aria-valuemin="0" aria-valuemax="100">' +
                                amountPercent + '%' +
                                '</div>' +
                                '</div>';
                        } else if (shouldShowNumberOfLoads) {
                            return (full.cargoCapacityRequiredError ? getCargoCapacityErrorIcon(full.cargoCapacityRequiredError) : '')
                                + '<span ' + tooltipTags + '>' + (full.loadCount || '0') + '</span>';
                        } else {
                            return '';
                        }
                    }
                },
                {
                    defaultContent: "",
                    data: "isClosed",
                    render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(full.isClosed); },
                    className: "checkmark is-closed-column",
                    width: "40px",
                    responsivePriority: getResponsivePriorityByName('isClosed'),
                    title: "Closed",
                    titleHoverText: "Closed"
                },
                {
                    defaultContent: "",
                    data: null,
                    orderable: false,
                    name: "Actions",
                    title: "",
                    width: "10px",
                    //responsivePriority: 0, used class 'all' instead of this
                    className: "actions all",
                    //defaultContent: "<button type='button' class='btn btn-sm btn-default btnEditRow' title='Edit'><i class='fa fa-edit'></i>Edit</button>\n",
                    render: function (data, type, full, meta) {
                        var content = actionMenuHasItems() ?
                            '<div class="dropdown">' +
                            '<button class="btn btn-primary btn-sm btnActions"><i class="fa fa-ellipsis-h"></i></button>'
                            + '</div>' : '';
                        return content;

                    }
                }
            ],
            createdRow: function (row, rowData, index) {
                updateRowAppearance(row, rowData);
            },
            preDrawCallback: function (settings) {
                // check if filter includes current day or futures dates
                if (!isPastDate()) {
                    //scheduleGrid.settings().context[0].oLanguage.sEmptyTable = "<span>There are no jobs for this date.</span><br /><button type='button' id='howToAddaJob' class='btn btn-primary btn-sm mt-2'>Click here to see how to add a job</button>";
                } else {
                    scheduleGrid.settings().context[0].oLanguage.sEmptyTable = "No data available in table";
                }
            },
            drawCallback: function (settings) {
                $('table [data-toggle="tooltip"]').tooltip();
            }
        });

        function isOrderLineShared(orderLine) {
            return !!(orderLine.haulingCompanyOrderLineId || orderLine.materialCompanyOrderLineId);
        }

        function recalculateFooterTotals() {
            var api = scheduleGrid;
            var numberOfTrucksColumnName = 'numberOfTrucks:name';
            var scheduledTrucksColumnName = 'scheduledTrucks:name';

            var pageTotalReqTrucks = api.column(numberOfTrucksColumnName, { page: 'current' }).data().reduce((a, b) => parseFloat(a) + parseFloat(b || 0), 0);
            var pageTotalSchedTrucks = api.column(scheduledTrucksColumnName, { page: 'current' }).data().reduce((a, b) => parseFloat(a) + parseFloat(b || 0), 0);

            $(api.column(numberOfTrucksColumnName).footer()).html(pageTotalReqTrucks.toFixed(2));
            $(api.column(scheduledTrucksColumnName).footer()).html(pageTotalSchedTrucks.toFixed(2));
        }

        function getCargoCapacityErrorIcon(cargoCapacityRequiredError) {
            let errorIcon = $('<i class="fa fa-exclamation-circle color-red mr-2"></i>').attr('title', cargoCapacityRequiredError);
            return $('<div>').append(errorIcon).html();
        }

        function updateRowAppearance(element, rowData) {
            var requestedTrucksCount = rowData.numberOfTrucks;
            var assignedTrucksCount = rowData.trucks.filter(t => t.vehicleCategory.isPowered).length;

            var row = $(element).closest('tr');
            var requestedTrucksCountCell = row.find('.cell-number-of-trucks');

            row.toggleClass('order-closed',
                rowData.isClosed
            );
            row.toggleClass('order-shared',
                isOrderLineShared(rowData)
            );
            row.toggleClass('order-not-fully-utilized',
                !rowData.isClosed && rowData.utilization < rowData.maxUtilization && _settings.validateUtilization
            );
            row.toggleClass('reqtruck-red',
                !rowData.isClosed
                && (_settings.validateUtilization
                        ? rowData.scheduledTrucks < rowData.numberOfTrucks
                        : (requestedTrucksCount > assignedTrucksCount || requestedTrucksCount < assignedTrucksCount)
                )
            );
            requestedTrucksCountCell.tooltip('dispose');
            requestedTrucksCountCell.attr('title', `${assignedTrucksCount} trucks assigned`).attr('data-toggle', 'tooltip').tooltip();
        }

        function truckHasNoDriver(truck) {
            return !truck.isExternal && (truck.hasNoDriver || !truck.hasDefaultDriver && !truck.hasDriverAssignment);
        }

        function truckHasOrderLineTrucks(truck) {
            var orders = scheduleGrid.data().toArray();
            return orders.some(o => o.trucks.some(olt => olt.truckId === truck.id));
        }

        function hasOpenMultipleLoadsDispatch(dispatches) {
            return dispatches?.some(d => d.isMultipleLoads && abp.enums.dispatchStatuses.open.includes(d.status)) || false;
        }

        function isPastDate() {
            var isPastDate = moment($("#DateFilter").val(), 'MM/DD/YYYY') < moment().startOf('day');
            return isPastDate;
        }

        function isFutureDate() {
            var isFutureDate = moment($("#DateFilter").val(), 'MM/DD/YYYY') > moment().startOf('day');
            return isFutureDate;
        }

        function isToday() {
            var isToday = moment($("#DateFilter").val(), 'MM/DD/YYYY').isSame(moment().startOf('day'));
            return isToday;
        }

        //automatically select first dropdown value in trucks tagsinput on enter keypress
        $('body').on('keydown', '.tt-input', function (e) {
            if (e.keyCode === 13) {
                e = $.Event('keydown'); e.which = 40; $(this).trigger(e); //down arrow
                e = $.Event('keydown'); e.which = 13; $(this).trigger(e); //enter
            }
        });

        scheduleTable.on('dblclick', '.truck-tag', function () {
            var orderLine = _dtHelper.getRowData(this);
            if (!hasOrderEditPermissions() || !isAllowedToEditOrderTrucks(orderLine)) {
                return;
            }
            var truck = $(this).data('item');
            if (truck.isDone || !truck.id) {
                return;
            }
            if (truck.vehicleCategory.assetType === abp.enums.assetType.trailer) {
                abp.message.info("Trailer utilization can't be changed.");
                return;
            }
            if (!truck.vehicleCategory.isPowered) {
                abp.message.info("Utilization can't be changed for this type of truck");
                return;
            }
            _setTruckUtilizationModal.open({ id: truck.id }).fail(handlePopupException);
        });

        function handlePopupException(failResult) {
            if (failResult && failResult.loadResponseObject && failResult.loadResponseObject.userFriendlyException) {
                var param = failResult.loadResponseObject.userFriendlyException.parameters;
                if (param && param.Kind === "EntityDeletedException") {
                    reloadMainGrid(null, false);
                    reloadTruckTiles();
                }
            }
        }

        async function sendDispatchMessage(options) {
            if (dispatchVia === abp.enums.dispatchVia.simplifiedSms //sendSmsOnDispatching is always set to dontSend when using simplifiedSms, so we're not checking that value in this case
                || (dispatchVia === abp.enums.dispatchVia.driverApplication)
            ) {
                _sendDispatchMessageModal.open(options);
            } else if (dispatchVia === abp.enums.dispatchVia.driverApplication) {
                try {
                    abp.ui.setBusy();
                    await _dispatchingService.sendDispatchMessageNonInteractive(options);
                    abp.notify.info(app.localize('DispatchesBeingCreated'));
                }
                finally {
                    abp.ui.clearBusy();
                }
            } else {
                abp.message.warn("Dispatch via is not set to Driver Application");
            }
        }

        async function setTrailerForTractorAsync(options) {
            var filterData = _dtHelper.getFilterData();
            await _trailerAssignmentService.setTrailerForTractor({
                date: filterData.date,
                shift: filterData.shift,
                officeId: filterData.officeId,
                ...options
            });

            abp.notify.info('Saved successfully.');

            if (options.updateExistingOrderLineTrucks) {
                setTimeout(() => {
                    reloadMainGrid(null, false);
                }, 500);
            }

            reloadTruckTiles();
            //reloadDriverAssignments();
        }

        async function setTrailerForOrderLineTruckAsync(options) {
            await _trailerAssignmentService.setTrailerForOrderLineTruck({
                ...options
            });

            abp.notify.info('Saved successfully.');

            reloadMainGrid(null, false);
            reloadTruckTiles();
            //reloadDriverAssignments();
        }

        async function promptWhetherToReplaceTrailerOnExistingOrderLineTrucks(options) {
            try {
                abp.ui.setBusy();
                let result = {};
                var filterData = _dtHelper.getFilterData();
                var isPastDate = moment(filterData.date, 'MM/DD/YYYY') < moment().startOf('day');

                if (options.truckId) {
                    var validationResult = await _driverAssignmentService.hasOrderLineTrucks({
                        trailerId: options.trailerId,
                        forceTrailerIdFilter: true,
                        truckId: options.truckId,
                        officeId: filterData.officeId,
                        date: filterData.date,
                        shift: filterData.shift
                    });

                    if (isPastDate) {
                        //same as a "no" answer
                    } else if (validationResult.hasOrderLineTrucks) {
                        abp.ui.clearBusy();
                        var userResponse = await swal(
                            app.localize("TrailerAlreadyScheduledForTruck{0}Prompt_YesToReplace", options.truckCode),
                            {
                                buttons: {
                                    no: "No",
                                    yes: "Yes"
                                }
                            }
                        );
                        abp.ui.setBusy();
                        if (userResponse === 'yes') {
                            if (validationResult.hasOpenDispatches) {
                                abp.message.error(app.localize("CannotChangeTrailerBecauseOfDispatchesError"));
                                throw new Error(app.localize("CannotChangeTrailerBecauseOfDispatchesError"));
                            }
                            result.updateExistingOrderLineTrucks = true;
                        }
                    }
                }

                return result;
            }
            finally {
                abp.ui.clearBusy();
            }
        }

        function reloadMainGrid(callback, resetPaging, resetCache) {
            resetPaging ??= true;
            resetCache ??= true;
            if (resetCache) {
                _orderLines = null;
                _orderLineGridCache = null;
            }
            scheduleGrid.ajax.reload(callback, resetPaging);
        }

        abp.event.on('app.assignDriverForTruckModalSaved', function () {
            //reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.truckUtilizationModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        abp.event.on('app.orderOfficeIdModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        abp.event.on('app.orderDirectionsModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        abp.event.on('app.orderLineNoteModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        abp.event.on('app.orderDateModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        abp.event.on('app.orderModalCopied', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        abp.event.on('app.quoteCreatedFromOrderModal', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        abp.event.on('app.orderLineSentToHaulingCompany', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
        });

        abp.event.on('app.noDriverForTruckModalSet', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.defaultDriverForTruckModalSet', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.activateClosedTrucksModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.reassignModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.trucksAssignedModal', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.createOrEditLeaseHaulerRequestModalSaved', function () {
            //reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.changeDriverForOrderLineTruckModalSaved', function () {
            reloadMainGrid(null, false);
            reloadTruckTiles();
            reloadDriverAssignments();
        });

        abp.event.on('app.createOrEditJobModalSaved', function () {
            reloadMainGrid(null, false);
            //reloadTruckTiles();
            //reloadDriverAssignments();
        });

        abp.event.on('app.createOrEditTruckModalSaved', function () {
            reloadTruckTiles();
        });

        abp.event.on('app.createOrEditLeaseHaulerTruckModalSaved', function () {
            reloadTruckTiles();
        });

        abp.event.on('app.createOrEditOrderModalSaved', function () {
            reloadMainGrid(null, false);
        });

        abp.event.on('app.createOrEditLeaseHaulerRequestModalSaved', function () {
            reloadMainGrid(null, false);
        });

        abp.event.on('app.leaseHaulerRequestRejectedModal', function () {
            reloadMainGrid(null, false);
        });
        abp.event.on('app.ticketEditedModal', function () {
            reloadMainGrid(null, false);
        });

        abp.event.on('app.ticketDeletedModal', function () {
            reloadMainGrid(null, false);
        });

        abp.event.on('abp.signalR.receivedSyncRequest', (syncRequest) => {
            reloadUpdatedDispatches(syncRequest);
        });
        abp.signalr.subscribeToSyncRequests();

        function reloadUpdatedDispatches(syncRequest) {
            let modifiedDispatches = syncRequest.changes
                .filter(c => c.entityType === abp.enums.entityEnum.dispatch)
                .map(c => c.entity)
                .filter(c => c.orderLineTruckId);
            if (!modifiedDispatches.length) {
                return;
            }
            if (!_orderLines) {
                return;
            }
            for (let updatedDispatch of modifiedDispatches) {
                let orderLine = _orderLines.find(ol => ol.id === updatedDispatch.orderLineId);
                let truck = orderLine?.trucks?.find(t => t.id === updatedDispatch.orderLineTruckId);
                if (!truck) {
                    continue;
                }
                truck.dispatches ??= [];
                let existingDispatch = truck.dispatches.find(d => d.id === updatedDispatch.id);
                if (existingDispatch) {
                    existingDispatch.status = updatedDispatch.status;
                    existingDispatch.isMultipleLoads = updatedDispatch.isMultipleLoads;
                } else {
                    truck.dispatches.push({
                        id: updatedDispatch.id,
                        status: updatedDispatch.status,
                        isMultipleLoads: updatedDispatch.isMultipleLoads,
                    });
                }
            }
            scheduleGrid.rows().every(function () {
                let rowData = this.data();
                if (modifiedDispatches.some(x => x.orderLineId == rowData.id)) {
                    let rowNode = this.node();
                    if (rowNode) {
                        $(rowNode).find('.truck-cell-editor').tagsinput('refresh');
                    }
                }
            });
        }

        function actionMenuHasItems() {
            return _permissions.leaseHaulerSchedule
                || _permissions.edit
                || _permissions.editTickets
                || _permissions.print
                || _permissions.driverMessages;
        }

        scheduleTable.on('click', '.btnActions', function (e) {
            e.preventDefault();
            var button = $(this);
            var position = button[0].getBoundingClientRect();
            position.x += $(window).scrollLeft();
            position.y += $(window).scrollTop();
            button.contextMenu({ x: position.x, y: position.y });
        });

        scheduleTable.on('click', '#howToAddaJob', function (e) {
            e.preventDefault();
            //this was previously showing a userguiding guide
            //the button has been commented out for a while, the userguiding call is also removed now
        });

        $("#TruckTilesNoTrucksMessage").click(function (e) {
            e.preventDefault();
            openAddTruckModal();
        })

        $('#AddLeaseHaulerRequestButton').click(function (e) {
            e.preventDefault();
            _createOrEditLeaseHaulerRequestModal.open({
                date: $('#DateFilter').val(),
                suppressLeaseHaulerDispatcherNotification: true
            });
        });

        $('#MarkAllJobsCompletedButton').click(async function (e) {
            e.preventDefault();
            try {
                let prompt = `Are you sure you want to mark all jobs complete?`;
                if (!await abp.message.confirm(prompt)) {
                    return;
                }
                abp.ui.setBusy();
                await _schedulingService.setAllOrderLinesIsComplete(_dtHelper.getFilterData()).done(function () {
                    abp.notify.info('Marked all jobs complete');
                    reloadMainGrid(null, false);
                    reloadTruckTiles();
                });
            }
            finally {
                abp.ui.clearBusy();
            }
        });

        $('#AddDefaultDriverAssignmentsButton').click(async function (e) {
            e.preventDefault();
            try {
                abp.ui.setBusy();
                await _driverAssignmentService.addDefaultDriverAssignments(_dtHelper.getFilterData()).done(function () {
                    abp.notify.info('Added default driver assignments');
                    //reloadMainGrid(null, false);
                    reloadTruckTiles();
                    reloadDriverAssignments();
                });
            }
            finally {
                abp.ui.clearBusy();
            }
        });

        $('#SendOrdersToDriversButton').click(function (e) {
            e.preventDefault();
            var filterData = _dtHelper.getFilterData();
            let selectedOffices = [];
            if (filterData.officeId) {
                selectedOffices.push({
                    id: filterData.officeId,
                    name: filterData.officeName
                });
            }
            _sendOrdersToDriversModal.open({
                deliveryDate: filterData.date,
                shift: filterData.shift,
                selectedOffices
            });
        });

        $('#AddJobButton').click(function (e) {
            e.preventDefault();
            var filterData = _dtHelper.getFilterData();
            var date = filterData.date;
            if (abp.setting.getBoolean('App.DispatchingAndMessaging.DefaultDesignationToMaterialOnly')) {
                date = moment().format('L');
            }
            _createOrEditJobModal.open({
                deliveryDate: date,
                shift: filterData.shift,
                officeId: filterData.officeId,
                officeName: filterData.officeName
            });
        });

        $("#PrintScheduleButton").click(async function (e) {
            e.preventDefault();
            var printOptions = await _specifyPrintOptionsModal.open().then((modal, modalObject) => {
                return modalObject.getResultPromise();
            });

            let filterData = _dtHelper.getFilterData();
            let date = filterData.date;
            let reportParams = {
                date: date,
                officeId: filterData.officeId,
                ...printOptions
            };
            _orderService.doesOrderSummaryReportHaveData(reportParams).done(function (result) {
                if (!result) {
                    abp.message.warn('There are no orders to print for ' + date + '.');
                    return;
                }
                app.openPopup(abp.appPath + 'app/orders/GetOrderSummaryReport?' + $.param(reportParams));
            });
        });

        $("#PrintAllOrdersButton").click(async function (e) {
            e.preventDefault();
            var printOptions = await _specifyPrintOptionsModal.open().then((modal, modalObject) => {
                return modalObject.getResultPromise();
            });

            let filterData = _dtHelper.getFilterData();
            let date = filterData.date;
            let reportParams = {
                date: date,
                officeId: filterData.officeId,
                splitRateColumn: true,
                ...printOptions
            };
            _orderService.doesWorkOrderReportHaveData(reportParams).done(function (result) {
                if (!result) {
                    abp.message.warn('There are no orders to print for ' + date + '.');
                    return;
                }
                app.openPopup(abp.appPath + 'app/orders/GetWorkOrderReport?' + $.param(reportParams));
            });
        });

        $('#ExportScheduleButton').click(async function (e) {
            e.preventDefault();
            let $button = $(this);
            let abpData = {};
            $.extend(abpData, _dtHelper.getFilterData());
            abp.ui.setBusy($button);
            _schedulingService
                .getScheduleOrdersToCsv(abpData)
                .done(function (result) {
                    app.downloadTempFile(result);
                }).always(function () {
                    abp.ui.clearBusy($button);
                });
        });

        $('#ScheduleTable tbody tr').contextmenu({ 'target': '#context-menu' });

        scheduleTable.contextMenu({
            selector: 'tbody tr',
            zIndex: 103,
            events: {
                show: function (options) {
                    var rowData = _dtHelper.getRowData(this);
                    if ($.isEmptyObject(rowData) || !actionMenuHasItems()) {
                        return false;
                    }
                    return true;
                }
            },
            items: {
                editJob: {
                    name: _permissions.edit ? app.localize('EditJob') : app.localize('ViewJob'),
                    visible: function () {
                        return menuFunctions.isVisible.viewOrEditJob(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        return menuFunctions.fn.viewOrEditJob(this);
                    }
                },
                acceptJob: {
                    name: app.localize('AcceptJob'),
                    visible: function () {
                        return menuFunctions.isVisible.acceptJob(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        return menuFunctions.fn.acceptJob(this);
                    }
                },
                rejectJob: {
                    name: app.localize('RejectJob'),
                    visible: function () {
                        return menuFunctions.isVisible.rejectJob(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        return menuFunctions.fn.rejectJob(this);
                    }
                },
                carryUnfinishedPortionForward: {
                    name: app.localize('CarryUnfinishedPortionForward'),
                    visible: function () {
                        return menuFunctions.isVisible.carryUnfinishedPortionForward(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.carryUnfinishedPortionForward(this);
                    }
                },
                assignLeaseHaulerTrucks: {
                    name: app.localize('AssignTrucks'),
                    visible: function () {
                        return menuFunctions.isVisible.assignLeaseHaulerTrucks(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        return menuFunctions.fn.assignLeaseHaulerTrucks(this);
                    }
                },
                orderGroup: {
                    name: app.localize('Schedule_DataTable_MenuItems_Order'),
                    visible: function () {
                        var rowData = _dtHelper.getRowData(this);
                        return menuFunctions.isVisible.viewEdit(rowData)
                            || menuFunctions.isVisible.markComplete(rowData)
                            || menuFunctions.isVisible.reOpenJob(rowData)
                            || menuFunctions.isVisible.copy(rowData)
                            || menuFunctions.isVisible.transfer(rowData)
                            || menuFunctions.isVisible.sendOrderToLeaseHauler(rowData)
                            || menuFunctions.isVisible.changeDate(rowData)
                            || menuFunctions.isVisible.delete(rowData);
                    },
                    items: {
                        viewEdit: {
                            name: app.localize('Schedule_DataTable_MenuItems_ViewEdit'),
                            visible: function () {
                                return menuFunctions.isVisible.viewEdit(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.viewEdit(this);
                            }
                        },
                        markComplete: {
                            name: app.localize('Schedule_DataTable_MenuItems_MarkComplete'),
                            visible: function () {
                                return menuFunctions.isVisible.markComplete(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.markComplete(this, { isCancelled: false });
                            }
                        },
                        cancel: {
                            name: app.localize('Cancel'),
                            visible: function () {
                                return menuFunctions.isVisible.markComplete(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.markComplete(this, { isCancelled: true });
                            }
                        },
                        reOpenJob: {
                            name: app.localize('Schedule_DataTable_MenuItems_ReOpenJob'),
                            visible: function () {
                                return menuFunctions.isVisible.reOpenJob(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.reOpenJob(this);
                            }
                        },
                        copy: {
                            name: app.localize('Schedule_DataTable_MenuItems_Copy'),
                            visible: function () {
                                return menuFunctions.isVisible.copy(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.copy(this);
                            }
                        },
                        transfer: {
                            name: app.localize('Schedule_DataTable_MenuItems_Transfer'),
                            visible: function () {
                                return menuFunctions.isVisible.transfer(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.transfer(this);
                            }
                        },
                        sendOrderToLeaseHauler: {
                            name: app.localize('SendOrderToLeaseHauler'),
                            visible: function () {
                                return menuFunctions.isVisible.sendOrderToLeaseHauler(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.sendOrderToLeaseHauler(this);
                            }
                        },
                        changeDate: {
                            name: app.localize('Schedule_DataTable_MenuItems_ChangeDate'),
                            visible: function () {
                                return menuFunctions.isVisible.changeDate(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.changeDate(this);
                            }
                        },
                        reassignTrucks: {
                            name: app.localize('Schedule_DataTable_MenuItems_ReassignTrucks'),
                            visible: function () {
                                return menuFunctions.isVisible.reassignTrucks(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.reassignTrucks(this);
                            }
                        },
                        delete: {
                            name: app.localize('Schedule_DataTable_MenuItems_Delete'),
                            visible: function () {
                                return menuFunctions.isVisible.delete(_dtHelper.getRowData(this));
                            },
                            callback: function () {
                                menuFunctions.fn.delete(this);
                            }
                        }
                    }
                },
                printGroup: {
                    name: app.localize('Schedule_DataTable_MenuItems_PrintOrder'),
                    visible: function () {
                        return hasOrderPrintPermissions();
                    },
                    items: {
                        printWithoutPrices: {
                            name: app.localize('Schedule_DataTable_MenuItems_NoPrices'),
                            callback: function () {
                                menuFunctions.fn.printNoPrices(this);
                            }
                        },
                        printCombinedPrices: {
                            name: app.localize('Schedule_DataTable_MenuItems_CombinedPrices'),
                            callback: function () {
                                menuFunctions.fn.printCombinedPrices(this);
                            }
                        },
                        printSeparatePrices: {
                            name: app.localize('Schedule_DataTable_MenuItems_SeparatePrices'),
                            callback: function () {
                                menuFunctions.fn.printSeparatePrices(this);
                            }
                        },
                        printForBackOffice: {
                            name: app.localize('Schedule_DataTable_MenuItems_BackOfficeDetail'),
                            visible: function () {
                                return true;
                            },
                            callback: function () {
                                menuFunctions.fn.printBackOfficeDetail(this);
                            }
                        }
                    }
                },
                assignTrucks: {
                    name: app.localize('AssignTrucks'),
                    visible: function () {
                        var orderLine = _dtHelper.getRowData(this);
                        return _permissions.schedule && hasOrderEditPermissions() && isAllowedToEditOrder(orderLine) && isTodayOrFutureDate(orderLine);
                    },
                    callback: function () {
                        var orderLine = _dtHelper.getRowData(this);
                        var popupOptions = {
                            orderLineId: orderLine.id
                        };
                        $.extend(popupOptions, _dtHelper.getFilterData());
                        _assignTrucksModal.open(popupOptions);
                    }
                },
                editCharges: {
                    name: app.localize('AddOrEditCharges'),
                    visible: function () {
                        return _permissions.editCharges;
                    },
                    callback: function () {
                        const orderLine = _dtHelper.getRowData(this);
                        _editChargesModal.open({
                            orderLineId: orderLine.id,
                        });
                    }
                },
                markComplete: {
                    name: app.localize('MarkComplete'),
                    visible: function () {
                        return menuFunctions.isVisible.markAllLeaseHaulerTrucksComplete(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.markAllLeaseHaulerTrucksComplete(this);
                    }
                },
                changeOrderLineUtilization: {
                    name: app.localize('ChangeUtilization'),
                    visible: function () {
                        var orderLine = _dtHelper.getRowData(this);
                        return hasOrderEditPermissions()
                            && isAllowedToEditOrder(orderLine)
                            && isTodayOrFutureDate(orderLine)
                            && !orderLine.isClosed && orderLine.trucks.length > 0
                            && thereAreNotDoneTrucks(orderLine);
                    },
                    callback: function () {
                        var orderLine = _dtHelper.getRowData(this);
                        _changeOrderLineUtilizationModal.open({ id: orderLine.id });
                    }
                },
                tickets: {
                    name: app.localize('Tickets'),
                    visible: function () {
                        return menuFunctions.isVisible.tickets(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.tickets(this);
                    }
                },
                showMap: {
                    name: app.localize('Schedule_DataTable_MenuItems_ShowMap'),
                    visible: function () {
                        return menuFunctions.isVisible.showMap(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.showMap(this);
                    }
                },
                showTripsReport: {
                    name: app.localize('Schedule_DataTable_MenuItems_ShowTripsReport'),
                    visible: function () {
                        return menuFunctions.isVisible.showTripsReport(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.showTripsReport(this);
                    }
                },
                showCycleTimes: {
                    name: app.localize('Schedule_DataTable_MenuItems_ShowCycleTimes'),
                    visible: function () {
                        return menuFunctions.isVisible.showCycleTimes(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.showCycleTimes(this);
                    }
                },
                dispatchToDriver: {
                    name: app.localize('Schedule_DataTable_MenuItems_DispatchToDriver'),
                    visible: function () {
                        return menuFunctions.isVisible.dispatchToDriver(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.dispatchToDriver(this);
                    }
                },
                requestLeaseHauler: {
                    name: app.localize('Schedule_DataTable_MenuItems_RequestLeaseHauler'),
                    visible: function() {
                        var orderLine = _dtHelper.getRowData(this);
                        return _features.jobBasedLeaseHaulerRequest
                            && _settings.allowLeaseHaulerRequestProcess
                            && _permissions.schedule
                            && isTodayOrFutureDate(orderLine);
                    },
                    callback: function() {
                        var orderLine = _dtHelper.getRowData(this);
                        _addOrEditLeaseHaulerRequestModal.open({
                            orderLineId: orderLine.id,
                            date: $('#DateFilter').val()
                        });
                    }
                },
                messageToDriver: {
                    name: app.localize('Schedule_DataTable_MenuItems_MessageToDriver'),
                    visible: function () {
                        return menuFunctions.isVisible.messageToDriver(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.messageToDriver(this);
                    }
                },
                viewDispatches: {
                    name: app.localize('Schedule_DataTable_MenuItems_ViewDispatches'),
                    visible: function () {
                        return menuFunctions.isVisible.viewDispatches(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.viewDispatches(this);
                    }
                },
                printWithDeliveryInfo: {
                    name: app.localize('Schedule_DataTable_MenuItems_WithDeliveryInfo'),
                    visible: function () {
                        return _permissions.schedule;
                    },
                    callback: function () {
                        menuFunctions.fn.printWithDeliveryInfo(this);
                    }
                },
                activateClosedTrucks: {
                    name: 'Activate closed truck',
                    visible: function () {
                        return menuFunctions.isVisible.activateClosedTrucks(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.activateClosedTrucks(this);
                    }
                },
                viewJobSummary: {
                    name: app.localize('Schedule_DataTable_MenuItems_JobSummary'),
                    visible: function () {
                        return menuFunctions.isVisible.viewJobSummary(_dtHelper.getRowData(this));
                    },
                    callback: function () {
                        menuFunctions.fn.viewJobSummary(this);
                    }
                },
            }
        });

        scheduleTable.contextMenu({
            selector: '.truck-tag',
            zIndex: 103,
            events: {
                show: function () {
                    var orderLine = _dtHelper.getRowData(this);
                    if (!hasOrderEditPermissions() || !isAllowedToEditOrderTrucks(orderLine)) {
                        return false;
                    }
                    return true;
                }
            },
            items: {
                dispatchToDriver: {
                    name: app.localize('Schedule_DataTable_MenuItems_DispatchToDriver'),
                    visible: function () {
                        var truck = $(this).data('item');
                        var rowData = _dtHelper.getRowData(this);
                        var today = new Date(moment().format("YYYY-MM-DD") + 'T00:00:00Z');
                        return showDispatchItems
                            && !rowData.isClosed
                            && rowData.trucks.length > 0
                            && !truck.isDone && truck.vehicleCategory.isPowered
                            && (new Date(rowData.date)).getTime() >= today.getTime();
                    },
                    callback: function () {
                        var orderLineId = _dtHelper.getRowData(this).id;
                        var orderLineTruckId = $(this).data('item').id;
                        sendDispatchMessage({
                            orderLineId: orderLineId,
                            selectedOrderLineTruckId: orderLineTruckId
                        });
                    }
                },
                messageToDriver: {
                    name: app.localize('Schedule_DataTable_MenuItems_MessageToDriver'),
                    visible: function () {
                        var truck = $(this).data('item');
                        return allowSmsMessages && _permissions.driverMessages && !truck.isDone && truck.vehicleCategory.isPowered;
                    },
                    callback: function () {
                        var orderLineId = _dtHelper.getRowData(this).id;
                        var driverId = $(this).data('item').driverId;
                        _sendDriverMessageModal.open({
                            orderLineId: orderLineId,
                            selectedDriverId: driverId
                        });
                    }
                },
                changeUtilization: {
                    name: app.localize('ChangeUtilization'),
                    visible: function () {
                        var truck = $(this).data('item');
                        return !truck.isDone
                            && truck.vehicleCategory.assetType !== abp.enums.assetType.trailer
                            && truck.vehicleCategory.isPowered
                            && truck.id;
                    },
                    callback: function () {
                        var truck = $(this).data('item');
                        _setTruckUtilizationModal.open({ id: truck.id }).fail(handlePopupException);
                    }
                },
                viewDispatches: {
                    name: app.localize('Schedule_DataTable_MenuItems_ViewDispatches'),
                    visible: function () {
                        var truck = $(this).data('item');
                        return showDispatchViaSmsItems && truck.vehicleCategory.assetType !== abp.enums.assetType.trailer;
                    },
                    callback: function () {
                        var orderLineId = _dtHelper.getRowData(this).id;
                        var truckId = $(this).data('item').truckId;
                        window.location.href = abp.appPath + 'app/Dispatches/?orderLineId=' + orderLineId + '&truckId=' + truckId;
                    }
                },
                deleteOrderLineTruck: {
                    name: app.localize('RemoveTruckFromJob'),
                    visible: function () {
                        var truck = $(this).data('item');
                        return !truck.isDone;
                    },
                    callback: function () {
                        var truck = $(this).data('item');
                        var editor = $(this).closest('td').find('.truck-cell-editor');
                        editor.tagsinput('remove', truck);
                    }
                },
                changeDriverForOrderLineTruck: {
                    name: app.localize('ChangeDriver'),
                    visible: function () {
                        var truck = $(this).data('item');
                        return hasTrucksPermissions()
                            && truck.vehicleCategory.isPowered
                            && truck.id;
                    },
                    callback: function () {
                        var orderLineTruckId = $(this).data('item').id;
                        _changeDriverForOrderLineTruckModal.open({ orderLineTruckId: orderLineTruckId });
                    }
                },
                changeTrailer: {
                    name: 'Change trailer',
                    visible: function () {
                        var truck = $(this).data('item');
                        return truck.canPullTrailer;
                    },
                    callback: async function () {
                        var item = $(this).data('item');
                        var orderLine = _dtHelper.getRowData(this);
                        let trailer = await app.getModalResultAsync(
                            _selectTrailerModal.open({
                                message: 'Select trailer for truck ' + item.truckCode + ' for single job',
                                trailerId: item.trailer && item.trailer.id || null,
                                trailerTruckCode: item.trailer && item.trailer.truckCode || null,
                                trailerVehicleCategoryId: item.trailer && item.trailer.vehicleCategory.id || null,
                                leaseHaulerId: item.leaseHaulerId,
                            })
                        );

                        if (orderLine.vehicleCategoryIds.length && !orderLine.vehicleCategoryIds.includes(trailer.vehicleCategory.id)) {
                            abp.message.error(app.localize("CannotChangeTrailerBecauseOfOrderLineVehicleCategoryError"));
                            return;
                        }

                        await setTrailerForOrderLineTruckAsync({
                            orderLineTruckId: item.id,
                            trailerId: trailer.id
                        });
                    }
                },
            }
        });

        $.contextMenu({
            selector: '.truck-tile',
            zIndex: 103,
            events: {
                show: function (options) {
                    if (!hasTrucksPermissions()) {
                        return false;
                    }
                    var anyItemIsVisible = false;
                    for (var itemName in options.items) {
                        if (!options.items.hasOwnProperty(itemName)) {
                            continue;
                        }
                        var item = options.items[itemName];

                        if (item.visible.apply(this)) {
                            anyItemIsVisible = true;
                            break;
                        }
                    }
                    if (!anyItemIsVisible) {
                        return false;
                    }
                    return true;
                }
            },
            items: {
                notOutOfService: {
                    name: 'Place back in service',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return !truck.isExternal
                            && !truck.alwaysShowOnSchedule
                            && truck.isOutOfService; // && truck.officeId === abp.session.officeId;
                    },
                    callback: function () {
                        var truck = $(this).data('truck'); //category, hasNoDriver, id, isOutOfService, officeId, truckCode, utilization, utilizationList
                        setTruckIsOutOfServiceValue(truck.id, false);
                    }
                },
                outOfService: {
                    name: 'Place out of service',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return !truck.isExternal
                            && !truck.alwaysShowOnSchedule
                            && !truck.isOutOfService; //&& truck.officeId === abp.session.officeId;
                    },
                    callback: function () {
                        var truck = $(this).data('truck');
                        setTruckIsOutOfServiceValue(truck.id, true);
                    }
                },
                noDriverForTruck: {
                    name: 'No driver for truck',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return !truck.isExternal
                            && !truckHasNoDriver(truck)
                            && truck.vehicleCategory.isPowered
                            && !isPastDate();
                    },
                    callback: function () {
                        var truck = $(this).data('truck');
                        var date = _dtHelper.getFilterData().date;
                        _setNoDriverForTruckModal.open({
                            truckId: truck.id,
                            startDate: date,
                            endDate: date
                        });
                    }
                },
                assignDriverForTruck: {
                    name: 'Assign driver',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return !truck.isExternal
                            && truckHasNoDriver(truck)
                            && truck.vehicleCategory.isPowered;
                    },
                    callback: function () {
                        var truck = $(this).data('truck');
                        var filterData = _dtHelper.getFilterData();
                        _assignDriverForTruckModal.open({
                            truckId: truck.id,
                            truckCode: truck.truckCode,
                            leaseHaulerId: truck.leaseHaulerId,
                            alwaysShowOnSchedule: truck.alwaysShowOnSchedule,
                            date: filterData.date,
                            shift: filterData.shift,
                            officeId: filterData.officeId
                        });
                    }
                },
                changeDriverForTruck: {
                    name: 'Change driver',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return (truck.isExternal || !truckHasNoDriver(truck))
                            && truck.vehicleCategory.isPowered;
                    },
                    callback: function () {
                        var truck = $(this).data('truck');
                        var filterData = _dtHelper.getFilterData();
                        _assignDriverForTruckModal.open({
                            truckId: truck.id,
                            truckCode: truck.truckCode,
                            leaseHaulerId: truck.leaseHaulerId,
                            alwaysShowOnSchedule: truck.alwaysShowOnSchedule,
                            date: filterData.date,
                            shift: filterData.shift,
                            officeId: filterData.officeId,
                            driverId: truck.driverId,
                            driverName: truck.driverName
                        });
                    }
                },
                defaultDriverForTruck: {
                    name: 'Assign default driver back to truck',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return !truck.isExternal && truck.hasNoDriver && truck.hasDefaultDriver && !isPastDate();
                    },
                    callback: function () {
                        var truck = $(this).data('truck');
                        var date = _dtHelper.getFilterData().date;
                        _setDefaultDriverForTruckModal.open({
                            truckId: truck.id,
                            startDate: date,
                            endDate: date
                        });
                    }
                },
                removeLhTruckFromSchedule: {
                    name: 'Remove from schedule',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return truck.isExternal && !truckHasOrderLineTrucks(truck);
                    },
                    callback: async function () {
                        var truck = $(this).data('truck');
                        var filterData = _dtHelper.getFilterData();
                        await abp.services.app.leaseHaulerRequestEdit.removeAvailableLeaseHaulerTruckFromSchedule({
                            truckId: truck.id,
                            date: filterData.date,
                            shift: filterData.shift,
                            officeId: filterData.officeId,
                        });
                        abp.notify.info('Successfully removed.');
                        reloadTruckTiles();
                        reloadDriverAssignments();
                    }
                },
                addTrailer: {
                    name: 'Add trailer',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return truck.canPullTrailer && !truck.trailer;
                    },
                    callback: async function () {
                        var truck = $(this).data('truck');

                        let trailer = await app.getModalResultAsync(
                            _selectTrailerModal.open({
                                leaseHaulerId: truck.leaseHaulerId
                            })
                        );

                        var result = await promptWhetherToReplaceTrailerOnExistingOrderLineTrucks({
                            truckId: truck.id,
                            truckCode: truck.truckCode
                        });

                        await setTrailerForTractorAsync({
                            tractorId: truck.id,
                            trailerId: trailer.id,
                            ...result
                        });
                    }
                },
                changeTrailer: {
                    name: 'Change trailer',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return truck.canPullTrailer && truck.trailer;
                    },
                    callback: async function () {
                        var truck = $(this).data('truck');

                        let trailer = await app.getModalResultAsync(
                            _selectTrailerModal.open({
                                trailerId: truck.trailer.id,
                                trailerTruckCode: truck.trailer.truckCode,
                                trailerVehicleCategoryId: truck.trailer.vehicleCategory.id,
                                leaseHaulerId: truck.leaseHaulerId,
                                modalSubtitle: truck.truckCode + ' is currently coupled to ' + truck.trailer.truckCode
                                    + ' - ' + truck.trailer.vehicleCategory.name + ' ' + truck.trailer.make + ' ' + truck.trailer.model + ' '
                                    + truck.trailer.bedConstructionFormatted + ' bed'
                            })
                        );

                        var result = await promptWhetherToReplaceTrailerOnExistingOrderLineTrucks({
                            trailerId: truck.trailer.id,
                            truckId: truck.id,
                            truckCode: truck.truckCode
                        });

                        await setTrailerForTractorAsync({
                            tractorId: truck.id,
                            trailerId: trailer.id,
                            ...result
                        });
                    }
                },
                removeTrailer: {
                    name: 'Remove trailer',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return truck.canPullTrailer && truck.trailer;
                    },
                    callback: async function () {
                        var truck = $(this).data('truck');

                        var result = await promptWhetherToReplaceTrailerOnExistingOrderLineTrucks({
                            trailerId: truck.trailer.id,
                            truckId: truck.id,
                            truckCode: truck.truckCode
                        });

                        await setTrailerForTractorAsync({
                            tractorId: truck.id,
                            trailerId: null,
                            ...result
                        });
                    }
                },
                addTractor: {
                    name: 'Add tractor',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return truck.vehicleCategory.assetType === abp.enums.assetType.trailer && !truck.tractor;
                    },
                    callback: async function () {
                        var truck = $(this).data('truck');
                        var filterData = _dtHelper.getFilterData();
                        _setTractorForTrailer.open({
                            date: filterData.date,
                            shift: filterData.shift,
                            officeId: filterData.officeId,
                            trailerId: truck.id
                        });
                    }
                },
                changeTractor: {
                    name: 'Change tractor',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return truck.vehicleCategory.assetType === abp.enums.assetType.trailer && truck.tractor;
                    },
                    callback: async function () {
                        var truck = $(this).data('truck');
                        var filterData = _dtHelper.getFilterData();
                        _setTractorForTrailer.open({
                            date: filterData.date,
                            shift: filterData.shift,
                            officeId: filterData.officeId,
                            trailerId: truck.id,
                            tractorId: truck.tractor.id,
                            tractorTruckCode: truck.tractor.truckCode
                        });
                    }
                },
                removeTractor: {
                    name: 'Remove tractor',
                    visible: function () {
                        var truck = $(this).data('truck');
                        return truck.vehicleCategory.assetType === abp.enums.assetType.trailer && truck.tractor;
                    },
                    callback: async function () {
                        var truck = $(this).data('truck');
                        var filterData = _dtHelper.getFilterData();
                        await abp.services.app.trailerAssignment.setTractorForTrailer({
                            date: filterData.date,
                            shift: filterData.shift,
                            officeId: filterData.officeId,
                            trailerId: truck.id,
                            tractorId: null
                        });
                        abp.notify.info('Successfully removed.');
                        reloadTruckTiles();
                    }
                },
            }
        });
    });
})();
