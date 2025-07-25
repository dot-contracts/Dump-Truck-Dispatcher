using System;
using Abp.Dependency;
using DispatcherWeb.Imports.DataResolvers.OfficeResolvers;

namespace DispatcherWeb.Imports.Services
{
    public static class ImportServiceFactory
    {
        public enum OfficeResolverType
        {
            ByName,
            ByFuelId,
        }

        public static IImportDataBaseAppService GetImportAppService(
            IIocResolver iocResolver,
            ImportType importType,
            OfficeResolverType officeResolverType
        )
        {
            IImportDataBaseAppService importAppService;
            switch (importType)
            {
                case ImportType.FuelUsage:
                    importAppService = iocResolver.Resolve<IImportFuelUsageAppService>();
                    break;

                case ImportType.VehicleUsage:
                    importAppService = iocResolver.Resolve<IImportVehicleUsageAppService>();
                    break;

                case ImportType.Customers:
                    importAppService = iocResolver.Resolve<IImportCustomersAppService>();
                    break;

                case ImportType.Vendors:
                    importAppService = iocResolver.Resolve<IImportVendorsAppService>();
                    break;

                case ImportType.Items:
                    importAppService = iocResolver.Resolve<IImportItemsAppService>();
                    break;

                case ImportType.Trucks:
                    importAppService = iocResolver.Resolve<IImportTrucksAppService>();
                    break;

                case ImportType.Employees:
                    importAppService = iocResolver.Resolve<IImportEmployeesAppService>();
                    break;

                case ImportType.Trux:
                    importAppService = iocResolver.Resolve<IImportTruxEarningsAppService>();
                    break;

                case ImportType.LuckStone:
                    importAppService = iocResolver.Resolve<IImportLuckStoneEarningsAppService>();
                    break;

                case ImportType.IronSheepdog:
                    importAppService = iocResolver.Resolve<IImportIronSheepdogEarningsAppService>();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(importType), $"Not supported {nameof(importType)}.");

            }

            switch (importType)
            {
                case ImportType.FuelUsage:
                case ImportType.VehicleUsage:
                    importAppService.OfficeResolver = GetOfficeResolver(iocResolver, officeResolverType);
                    break;

                case ImportType.Items:
                case ImportType.Trucks:
                case ImportType.Employees:
                    importAppService.OfficeResolver = iocResolver.Resolve<IOfficeResolver>(typeof(OfficeByUserIdResolver));
                    break;
            }

            return importAppService;
        }

        private static IOfficeResolver GetOfficeResolver(
            IIocResolver iocResolver,
            OfficeResolverType officeResolverType
        )
        {
            switch (officeResolverType)
            {
                case OfficeResolverType.ByName:
                    return iocResolver.Resolve<IOfficeResolver>(typeof(OfficeByNameResolver));

                case OfficeResolverType.ByFuelId:
                    return iocResolver.Resolve<IOfficeResolver>(typeof(OfficeByFuelIdResolver));

                default:
                    throw new ArgumentOutOfRangeException(nameof(officeResolverType), $"Not supported {nameof(officeResolverType)}.");
            }
        }
    }
}
