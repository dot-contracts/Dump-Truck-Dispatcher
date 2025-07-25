(function () {
    jQuery.fn.select2Location = function (userOptions) {
        userOptions = userOptions || {};

        if (!userOptions.predefinedLocationCategoryKind) {
            console.error('userOptions.predefinedLocationCategoryKind is not set in a call to select2Location');
            return;
        }

        var $element = $(this);
        const select2PageSize = 20;

        var googlePlacesContext = {
            sessionToken: null //new google.maps.places.AutocompleteSessionToken()
        };
        $element.data('google-places-context', googlePlacesContext);
        let mapsDiv = $('<div>').appendTo($element.parent());
        var initGooglePlaces = async function () {
            await abp.maps.waitForGoogleMaps();
            if (googlePlacesContext.autocompleteService) {
                return;
            }
            googlePlacesContext.autocompleteService = new google.maps.places.AutocompleteService();
            googlePlacesContext.placesService = new google.maps.places.PlacesService(mapsDiv[0]);
        };

        var getLocationsWithRates = async function (select2RequestParams) {
            if (select2RequestParams.data.page !== 1
                || !userOptions.rateParamsGetter
                || !abp.features.isEnabled('App.SeparateMaterialAndFreightItems')
                || !abp.features.isEnabled('App.HaulZone')
            ) {
                return [];
            }
            let rateParams = userOptions.rateParamsGetter();
            let uomBaseId = abp.setting.getInt('HaulZone.HaulRateCalculation.BaseUomId' + (rateParams.customerIsCod ? 'ForCod' : ''));
            if (!uomBaseId
                || !rateParams.useZoneBasedRates
                || !rateParams.pricingTierId
                || !rateParams.freightItemId
                || !rateParams.materialItemId
                || !rateParams.freightUomId
                || !rateParams.deliverToId
            ) {
                return [];
            }
            switch (Number(rateParams.designation)) {
                case abp.enums.designation.freightAndMaterial:
                    if (!rateParams.materialUomId) {
                        return [];
                    }
                    break;

                case abp.enums.designation.freightOnly:
                    //no additional validation rules
                    break;

                default:
                    //other designations are not valid
                    return [];
            }

            if (!await abp.maps.areGoogleMapsAvailable()) {
                //fallback for local debug with no google maps API key
                return [];
            }

            let rates = await abp.services.app.item.getLocationsWithRates({
                ...rateParams,
                loadAtId: null,
                uomBaseId: uomBaseId,
                term: select2RequestParams.data.term,
            });

            return rates;
        };

        var defaultOptions = {
            abpServiceMethod: listCacheSelectLists.location(),
            showAll: false,
            allowClear: true,
            ajax: {
                delay: 500,
                transport: async function (params, success, failure) {
                    try {
                        var additionalParams = {};
                        if (userOptions.abpServiceParamsGetter) {
                            additionalParams = userOptions.abpServiceParamsGetter(params);
                        }

                        let locationsWithRates = await getLocationsWithRates(params);
                        if (locationsWithRates.length) {
                            success({
                                results: locationsWithRates,
                                pagination: {
                                    more: false,
                                },
                            });
                            return;
                        }

                        let dbResponse = await options.abpServiceMethod($.extend({}, params.data, userOptions.abpServiceParams, additionalParams));
                        if (dbResponse.totalCount === 0 && params.data.page === 1 && params.data.term) {
                            //console.log('using google places');
                            await initGooglePlaces();
                            if (!googlePlacesContext.sessionToken) {
                                googlePlacesContext.sessionToken = new google.maps.places.AutocompleteSessionToken();
                            }
                            let { predictions } = await googlePlacesContext.autocompleteService.getPlacePredictions({
                                input: params.data.term,
                                sessionToken: googlePlacesContext.sessionToken
                            });
                            success({
                                results: predictions.map(p => ({
                                    id: _googlePlacesHelper.googlePlaceIdPrefix + p.place_id,
                                    name: p.description
                                })),
                                pagination: {
                                    more: false,
                                },
                            });
                        } else {
                            success({
                                results: dbResponse.items,
                                pagination: {
                                    more: params.data.page * select2PageSize < dbResponse.totalCount,
                                },
                            });
                        }
                    } catch (e) {
                        failure(e);
                        throw e;
                    }
                },
                cache: false
            }
        };

        var options = $.extend(true, {}, defaultOptions, userOptions);

        $element.on("select2:close", function () {
            //'close' is called after the 'change', so we can clear the session token
            googlePlacesContext.sessionToken = null;
        });
        $element.change(function (e) {
            var val = $element.val();
            var select2 = $element.data('select2');
            if (val && val.startsWith(_googlePlacesHelper.googlePlaceIdPrefix)) {
                e.stopImmediatePropagation();
                var onFail = function () {
                    $element.val(null).change();
                    abp.ui.clearBusy(select2?.$container);
                };
                var placeId = val.substring(_googlePlacesHelper.googlePlaceIdPrefix.length);
                if (!placeId) {
                    console.error('select2 had value ' + val + ' that can\'t be replaced');
                    onFail();
                    return;
                }
                var sessionToken = googlePlacesContext.sessionToken;
                if (!sessionToken) {
                    console.error('select2 had value ' + val + ', but session token is missing');
                    onFail();
                    return;
                }

                abp.ui.setBusy(select2?.$container);
                googlePlacesContext.placesService.getDetails({
                    placeId,
                    sessionToken,
                    fields: ["address_components", "geometry", "place_id", "name"]
                }, function (place, status) {
                    if (status != google.maps.places.PlacesServiceStatus.OK || !place) {
                        console.error('unexpected response code: ' + status, place);
                        onFail();
                        return;
                    }
                    googlePlacesContext.sessionToken = null;
                    let locationName = place.name;
                    let locationAddress = _googlePlacesHelper.parseAddressComponents(place.address_components);
                    if (place.name === locationAddress.streetAddress) {
                        //console.log('location name matched street address, setting name to ""');
                        locationName = '';
                    }

                    let newLocation = $.extend({
                        PredefinedLocationCategoryKind: userOptions.predefinedLocationCategoryKind,
                        Name: locationName,
                        //...locationAddress, our JS bunlder might not support destructuring assignment
                        PlaceId: place.place_id,
                        Latitude: place.geometry?.location?.lat() || null,
                        Longitude: place.geometry?.location?.lng() || null,
                        IsActive: true,
                    }, locationAddress);

                    abp.services.app.location.createOrGetExistingLocation(
                        newLocation
                    ).then(createdLocation => {
                        abp.helper.ui.addAndSetDropdownValue($element, createdLocation.id, createdLocation.displayName);
                        abp.ui.clearBusy(select2?.$container);
                    }).fail(function () {
                        onFail();
                    });
                });
            }
        });

        return $element.select2Init(options);
    };

    var _googlePlacesHelper = {
        googlePlaceIdPrefix: 'google.maps.places.id.',
        parseAddressComponents: function (address_components) {

            let result = {
                streetAddress: "",
                city: "",
                state: "",
                zipCode: "",
                countryCode: ""
            };

            if (!address_components) {
                return result;
            }

            // address_components are google.maps.GeocoderAddressComponent objects
            // which are documented at http://goo.gle/3l5i5Mr
            for (var component of address_components) {
                const componentType = component.types[0];

                switch (componentType) {
                    case "street_number": {
                        result.streetAddress = component.long_name + ' ' + result.streetAddress;
                        break;
                    }

                    case "route": {
                        result.streetAddress += component.short_name;
                        break;
                    }

                    case "postal_code": {
                        result.zipCode = component.long_name + result.zipCode;
                        break;
                    }

                    case "postal_code_suffix": {
                        result.zipCode = result.zipCode + '-' + component.long_name;
                        break;
                    }
                    case "locality":
                        result.city = component.long_name;
                        break;

                    case "administrative_area_level_1": {
                        result.state = component.short_name;
                        break;
                    }
                    case "country":
                        result.countryCode = component.short_name;
                        break;
                }
            }

            return result;
        }
    };
    abp.helper.googlePlacesHelper = _googlePlacesHelper;
})();
