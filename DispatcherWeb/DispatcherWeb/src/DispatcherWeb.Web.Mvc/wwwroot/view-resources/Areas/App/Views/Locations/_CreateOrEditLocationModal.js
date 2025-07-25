(function ($) {
    app.modals.CreateOrEditLocationModal = function () {

        var _modalManager;
        var _locationService = abp.services.app.location;
        var _dtHelper = abp.helper.dataTables;
        var _permissions = {
            merge: abp.auth.hasPermission('Pages.Locations.Merge')
        };

        var _createOrEditLocationModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Locations/CreateOrEditLocationModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Locations/_CreateOrEditLocationModal.js',
            modalClass: 'CreateOrEditLocationModal',
            modalSize: 'lg'
        });

        var _modal = null;
        var _$form = null;
        var _locationId = null;
        var _mergeWithDuplicateSilently = null;
        var _map = null;
        var _mapMarker = null;
        var _nameAutocomplete = null;
        var _addressAutocomplete = null;
        var _geocoder = null;

        var _latitudeField = null;
        var _longitudeField = null;
        var _placeIdField = null;
        var _nameField = null;
        var _addressField = null;
        var _cityField = null;
        var _stateField = null;
        var _zipField = null;
        var _countryField = null;


        var saveLocationAsync = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                throw new Error("Form is not valid");
            }

            var location = _$form.serializeFormToObject();

            abp.ui.setBusy(_$form);
            _modalManager.setBusy(true);
            try {
                if (!location.Latitude || !location.Longitude) {
                    let place = null;
                    try {
                        if (_addressField.val()) {
                            let address = getFilledAddress();
                            place = await geocodeAddressAsync(address);
                        } else {
                            //continue to the prompt
                        }
                    } catch (e) {
                        console.error(e);
                        //continue to the prompt
                    }

                    if (!place?.geometry?.location) {
                        if (!await abp.message.confirm("Could not find the location on the map. Do you want to continue?")) {
                            throw new Error("Form is not valid");
                        }
                    } else {
                        location.Latitude = place.geometry.location.lat();
                        location.Longitude = place.geometry.location.lng();
                        if (!location.PlaceId) {
                            location.PlaceId = place.place_id;
                            _placeIdField.val(place.place_id);
                        }
                        setMarkerFromPlace(place);
                    }
                }

                if (!_locationId) {
                    let duplicate = await _locationService.findExistingLocationDuplicate(location);

                    if (duplicate != null) {

                        let isDuplicateName = location.Name && duplicate.name == location.Name;
                        if (!await abp.message.confirm(`This ${isDuplicateName ? "Name" : "Address"} already exists. Are you sure you want to add this?`)) {
                            throw new Error("Duplicate exists");
                        }

                    }
                }
                let editResult = await _locationService.editLocation(location);
                abp.notify.info('Saved successfully.');
                _$form.find("#Id").val(editResult.id);
                _locationId = editResult.id;
                location.Id = editResult.id;
                abp.event.trigger('app.createOrEditLocationModalSaved', {
                    item: editResult
                });
                return editResult;
            } finally {
                abp.ui.clearBusy(_$form);
                _modalManager.setBusy(false);
            }
        };

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modal = _modalManager.getModal();

            _$form = _modal.find('form');
            _$form.validate();
            $.validator.addMethod(
                "regex",
                function (value, element, regexp) {
                    var re = new RegExp(regexp, 'i');
                    return this.optional(element) || re.test(value);
                },
                "Please check your input."
            );
            //_$form.find('#Latitude').rules('add', { regex: app.regex.latitudeLongitude });
            //_$form.find('#Longitude').rules('add', { regex: app.regex.latitudeLongitude });
            _mergeWithDuplicateSilently = _$form.find('#MergeWithDuplicateSilently').val() === 'true';

            _latitudeField = _$form.find('#Latitude');
            _longitudeField = _$form.find('#Longitude');
            _placeIdField = _$form.find('#PlaceId');
            _nameField = _$form.find('#Name');
            _addressField = _$form.find('#StreetAddress');
            _cityField = _$form.find('#City'); //locality
            _stateField = _$form.find('#State');
            _zipField = _$form.find('#ZipCode');
            _countryField = _$form.find('#CountryCode');

            _locationId = _$form.find("#Id").val();

            _$form.find("#CategoryId").select2Init({
                abpServiceMethod: abp.services.app.location.getLocationCategorySelectList,
                showAll: true,
                allowClear: false
            });

            var _createOrEditLocationContactModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Locations/CreateOrEditLocationContactModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Locations/_CreateOrEditLocationContactModal.js',
                modalClass: 'CreateOrEditLocationContactModal',
                modalSize: 'md'
            });

            var locationContactsTable = $('#LocationContactsTable');
            var locationContactsGrid = locationContactsTable.DataTableInit({
                paging: false,
                serverSide: true,
                processing: true,
                info: false,
                language: {
                    emptyTable: app.localize("NoDataInTheTableClick{0}ToAdd", app.localize("AddNewContact"))
                },
                ajax: function (data, callback, settings) {
                    if (_locationId === '') {
                        callback(_dtHelper.getEmptyResult());
                        return;
                    }
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, { locationId: _locationId });
                    _locationService.getLocationContacts(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
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
                        data: "name",
                        title: "Name"
                    },
                    {
                        data: "title",
                        title: "Title"
                    },
                    {
                        data: "phone",
                        title: "Phone"
                    },
                    {
                        data: "email",
                        title: "Email"
                    },
                    {
                        data: null,
                        orderable: false,
                        autoWidth: false,
                        width: "10px",
                        responsivePriority: 1,
                        defaultContent: '',
                        rowAction: {
                            items: [{
                                text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                                className: "btn btn-sm btn-default",
                                action: function (data) {
                                    _createOrEditLocationContactModal.open({ id: data.record.id });
                                }
                            }, {
                                text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                                className: "btn btn-sm btn-default",
                                action: function (data) {
                                    deleteLocationContact(data.record);
                                }
                            }]
                        }
                    }
                ]
            });


            _modal.on('shown.bs.modal', function () {
                locationContactsGrid
                    .columns.adjust()
                    .responsive.recalc();
            });

            _modal.find('#ContactsTabButton').click(function () {
                setTimeout(function () {
                    locationContactsGrid
                        .columns.adjust()
                        .responsive.recalc();
                }, 30);
            });

            var reloadLocationContactGrid = function () {
                locationContactsGrid.ajax.reload();
            };

            abp.event.on('app.createOrEditLocationContactModalSaved', function () {
                reloadLocationContactGrid();
            });

            _modal.find("#CreateNewLocationContactButton").click(async function (e) {
                e.preventDefault();
                if (_locationId === '') {
                    await saveLocationAsync();
                }
                _createOrEditLocationContactModal.open({ locationId: _locationId });
            });

            locationContactsTable.on('click', '.btnEditRow', function () {
                var locationContactId = _dtHelper.getRowData(this).id;
                _createOrEditLocationContactModal.open({ id: locationContactId });
            });
            _latitudeField.add(_longitudeField).on('input', function () {
                placeMarkerAndPanTo(getLocationLatLng());
            });

            async function deleteLocationContact(record) {
                if (await abp.message.confirm(
                    'Are you sure you want to delete the contact?'
                )) {
                    _locationService.deleteLocationContact({
                        id: record.id
                    }).done(function () {
                        abp.notify.info('Successfully deleted.');
                        reloadLocationContactGrid();
                    });
                }
            }

            initMapAndAddressControlsAsync();

        };

        this.focusOnDefaultElement = function () {
            if (_locationId) {
                return;
            }
            _nameField.focus();
        }

        function getLocationLatLng() {
            let lat = parseFloat(_latitudeField.val());
            let lng = parseFloat(_longitudeField.val());
            if (isNaN(lat) || isNaN(lng)) {
                return null;
            }
            return { lat, lng };
        }

        function setLocationLatLng(latLng) {
            //lat can be function or number
            let lat = !latLng ? '' : (typeof latLng.lat === 'function' ? latLng.lat() : latLng.lat);
            _latitudeField.val(lat);
            //lng can be function or number
            let lng = !latLng ? '' : (typeof latLng.lng === 'function' ? latLng.lng() : latLng.lng);
            _longitudeField.val(lng);
        }

        async function getMapFallbackLocationAsync() {
            //let billingAddress = abp.setting.get('App.UserManagement.BillingAddress');
            //if (billingAddress) {
            //    let place = await geocodeAddressAsync(billingAddress);
            //    if (place && place.geometry && place.geometry.location) {
            //        return place.geometry.location;
            //    }
            //}
            let defaultMapLocation = abp.setting.get('App.General.DefaultMapLocation');
            if (defaultMapLocation) {
                let [lat, lng] = defaultMapLocation.split(',', 2);
                return { lat: parseFloat(lat), lng: parseFloat(lng) };
            }
            let userLatLng = await getCurrentLocationAsync();
            if (userLatLng) {
                return userLatLng;
            }

            const fallbackPosition = { lat: 34.81683, lng: -82.37566 };
            return fallbackPosition;
        }

        function getCurrentLocationAsync() {
            return new Promise((resolve, reject) => {
                if (navigator.geolocation) {
                    navigator.geolocation.getCurrentPosition((pos) => resolve(pos ? { lat: pos.coords.latitude, lng: pos.coords.longitude } : null), onError);
                } else {
                    resolve(null);
                }

                function onError(error) {
                    console.error(error);
                    resolve(null);
                }
            });
        }

        async function initMapAndAddressControlsAsync() {
            await abp.maps.waitForGoogleMaps();

            _geocoder = new google.maps.Geocoder();

            let markerPosition = getLocationLatLng();
            let mapPosition = markerPosition || await getMapFallbackLocationAsync();

            _map = new google.maps.Map(_modal.find("#map")[0], {
                center: mapPosition,
                zoom: 15,
                styles: [{
                    featureType: 'poi',
                    stylers: [{ visibility: 'off' }]  // Turn off points of interest.
                }, {
                    featureType: 'transit.station',
                    stylers: [{ visibility: 'off' }]  // Turn off bus stations, train stations, etc.
                }],
                disableDoubleClickZoom: true,
                streetViewControl: false,
                fullscreenControl: false
            });

            _map.controls[google.maps.ControlPosition.RIGHT_TOP].push(_modal.find('#mapHelp')[0]);

            _map.addListener("dblclick", (e) => {
                setLocationLatLng(e.latLng);
                placeMarkerAndPanTo(e.latLng);
                //findAndSetAddressAsync(e.latLng);
            });

            if (markerPosition) {
                placeMarkerAndPanTo(markerPosition);
            } else if (_locationId && _addressField.val()) {
                let place = await geocodeAddressAsync(getFilledAddress());
                if (place) {
                    setMarkerFromPlace(place);
                    _placeIdField.val(place.place_id);
                }
            }

            _addressAutocomplete = new google.maps.places.Autocomplete(_addressField[0], {
                //componentRestrictions: { country: ["us", "ca"] },
                fields: ["address_components", "geometry", "place_id", "name"],
                types: ["address"],
            });

            _addressAutocomplete.addListener("place_changed", () => {
                let place = _addressAutocomplete.getPlace();
                fillAddressFromPlace(place);
                setMarkerFromPlace(place);
                _placeIdField.val(place.place_id);
            });

            _nameAutocomplete = new google.maps.places.Autocomplete(_nameField[0], {
                fields: ["address_components", "geometry", "place_id", "name"],
                types: ["establishment"],
            });

            _nameAutocomplete.addListener("place_changed", () => {
                let place = _nameAutocomplete.getPlace();
                fillAddressFromPlace(place);
                setMarkerFromPlace(place);
                _placeIdField.val(place.place_id);
                _nameField.val(place.name);
            });

            _addressField.add(
                _cityField
            ).add(
                _stateField
            ).on('blur', async function () {
                if (!_addressField.val() || !_cityField.val() || !_stateField.val()) {
                    return;
                }

                if (getLocationLatLng()) {
                    return;
                }

                let address = getFilledAddress();
                let place = await geocodeAddressAsync(address);

                setMarkerFromPlace(place);

                if (!_placeIdField.val()) {
                    _placeIdField.val(place.place_id);
                }
            });
        }

        function setMarkerFromPlace(place) {
            if (!place.geometry || !place.geometry.location) {
                console.warn("Returned place contains no geometry");
                return;
            }
            setLocationLatLng(place.geometry.location);
            placeMarkerAndPanTo(place.geometry.location);
        }

        function fillAddressFromPlace(place) {
            var address = abp.helper.googlePlacesHelper.parseAddressComponents(place.address_components);

            _addressField.val(address.streetAddress);
            _cityField.val(address.city);
            _stateField.val(address.state);
            _zipField.val(address.zipCode);
            _countryField.val(address.countryCode);
        }

        function placeMarkerAndPanTo(latLng) {
            if (_mapMarker) {
                _mapMarker.setMap(null);
                _mapMarker = null;
            }
            if (latLng) {
                _mapMarker = new google.maps.Marker({
                    position: latLng,
                    map: _map,
                    draggable: true,
                });
                _map.panTo(latLng);
                _mapMarker.addListener("dblclick", () => {
                    placeMarkerAndPanTo(null);
                });
                _mapMarker.addListener("dragend", (e) => {
                    setLocationLatLng(e.latLng);
                    //findAndSetAddressAsync(e.latLng);
                });
            }
        }

        async function findAndSetAddressAsync(latLng) {
            abp.ui.setBusy(_$form);
            _modalManager.setBusy(true);
            try {
                let geocodeResponse = await _geocoder.geocode({ location: latLng });
                if (!geocodeResponse.results[0]) {
                    return;
                }
                let place = geocodeResponse.results[0];
                fillAddressFromPlace(place);
                _placeIdField.val(place.place_id);
            } finally {
                abp.ui.clearBusy(_$form);
                _modalManager.setBusy(false);
            }
        }

        async function geocodeAddressAsync(address) {
            let geocodeResponse = await _geocoder.geocode({ address });
            if (!geocodeResponse.results[0]) {
                return null;
            }
            let place = geocodeResponse.results[0];
            return place;
        }

        function getFilledAddress() {
            return `${_addressField.val()} ${_cityField.val()}, ${_stateField.val()} ${_zipField.val()}, ${_countryField.val()}`;
        }

        this.save = async function () {
            var editResult = await saveLocationAsync();
            if (editResult) {
                _modalManager.setResult(editResult);
            }
            _modalManager.close();
        };
    };
})(jQuery);
