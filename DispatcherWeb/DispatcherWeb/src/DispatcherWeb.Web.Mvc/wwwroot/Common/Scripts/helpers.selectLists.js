window.listCacheSelectLists = {
    uom: () => {
        return listCache.unitOfMeasure.getSelectListMethod({
            fallbackMethod: abp.services.app.unitOfMeasure.getUnitsOfMeasureSelectList,
            modelCallback: (item) => ({
                uomBaseId: item.uomBaseId,
            }),
            idFieldGetter: (input) => input?.getUomBaseId ? 'uomBaseId' : 'id',
            filter: (input, item) => (
                (!input?.uomBaseIds?.length || input.uomBaseIds.includes(item.uomBaseId))
                && (!input?.getUomBaseId || item.uomBaseId)
            ),
        });
    },

    item: () => {
        return listCache.item.getSelectListMethod({
            fallbackMethod: abp.services.app.item.getItemsSelectList,
            fallbackChooser: (input) => {
                if (input?.quoteId) {
                    console.warn('QuoteId is not supported for the cached item select list.');
                    return true;
                }
                return false;
            },
            modelCallback: (item) => ({
                isTaxable: item.isTaxable,
                useZoneBasedRates: item.useZoneBasedRates,
            }),
            filter: (input, item) => (
                (!input?.ids?.length || input.ids.includes(item.id))
                && (input?.includeInactive || item.isActive)
                && (!input?.types?.length || input.types.includes(item.type))
            ),
        });
    },

    vehicleCategory: () => {
        return listCache.vehicleCategory.getSelectListMethod({
            fallbackMethod: abp.services.app.truck.getVehicleCategoriesSelectList,
            fallbackChooser: (input) => {
                if (input?.isInUse && !listCache.truck.isEnabled) {
                    console.warn('isInUse parameter only supports caching if both vehicleCategory and truck caches are enabled');
                    return true;
                }
                return false;
            },
            modelCallback: (item) => ({
                assetType: item.assetType,
                isPowered: item.isPowered,
            }),
            filterAsyncState: async (input) => {
                let result = {};
                if (input.isInUse) {
                    let trucks = await listCache.truck.getList();
                    result.vehicleCategoryIdsInUse = [...new Set(
                        trucks.map(x => x.vehicleCategoryId)
                    )];
                }
                return result;
            },
            filter: (input, item, filterAsyncState) => (
                (!input?.ids?.length || input.ids.includes(item.id))
                && (input?.isPowered === undefined || item.isPowered === input.isPowered)
                && (!input.isInUse || filterAsyncState.vehicleCategoryIdsInUse.includes(item.id))
                && (input?.assetType === undefined || item.assetType === input.assetType)
            ),
        });
    },

    location: () => {
        return listCache.location.getSelectListMethod({
            fallbackMethod: abp.services.app.location.getLocationsSelectList,
            fallbackChooser: (input) => {
                if (input?.loadAtQuoteId || input?.deliverToQuoteId) {
                    console.warn('loadAtQuoteId and deliverToQuoteId is not supported for the cached location select list.');
                    return true;
                }
                return false;
            },
            nameField: 'displayName',
            filter: (input, item, filterAsyncState) => (
                (!input?.ids?.length || input.ids.includes(item.id))
                && (input?.includeInactive || item.isActive)
            ),
        });
    },

    fuelSurchargeCalculation: () => {
        return listCache.fuelSurchargeCalculation.getSelectListMethod({
            fallbackMethod: abp.services.app.fuelSurchargeCalculation.getFuelSurchargeCalculationsSelectList,
            modelCallback: (item) => ({
                canChangeBaseFuelCost: item.canChangeBaseFuelCost,
                baseFuelCost: item.baseFuelCost,
            }),
            filter: (input, item) => (
                (!input?.ids?.length || input.ids.includes(item.id))
                && (input?.canChangeBaseFuelCost === undefined || item.canChangeBaseFuelCost === input.canChangeBaseFuelCost)
            ),
        });
    },

    office: () => {
        return listCache.office.getSelectListMethod({
            fallbackMethod: abp.services.app.office.getOfficesSelectList,
            filter: (input, item) => (
                (!input?.ids?.length || input.ids.includes(item.id))
                && (input?.allOrganizationUnits || abp.session.organizationUnitIds.includes(item.organizationUnitId))
            ),
        });
    },

    taxRate: () => {
        return listCache.taxRate.getSelectListMethod({
            fallbackMethod: abp.services.app.taxRate.getTaxRatesSelectList,
            modelCallback: (item) => ({
                rate: item.rate,
            }),
        });
    },
};
