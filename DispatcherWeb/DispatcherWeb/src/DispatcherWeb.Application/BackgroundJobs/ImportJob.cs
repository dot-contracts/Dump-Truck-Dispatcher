using System;
using System.IO;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Events.Bus;
using Abp.Timing;
using DispatcherWeb.Imports.Services;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.EventBus.Events;
using DispatcherWeb.Infrastructure.Utilities;

namespace DispatcherWeb.BackgroundJobs
{
    public class ImportJob : AsyncBackgroundJob<ImportJobArgs>, ITransientDependency
    {
        private readonly IIocResolver _iocResolver;
        private readonly ISecureFileBlobService _secureFileBlobService;

        public ImportJob(
            IIocResolver iocResolver,
            ISecureFileBlobService secureFileBlobService
        )
        {
            _iocResolver = iocResolver;
            _secureFileBlobService = secureFileBlobService;

            EventBus = NullEventBus.Instance;
        }
        public IEventBus EventBus { get; set; }

        public override async Task ExecuteAsync(ImportJobArgs args)
        {
            try
            {
                await using (var fileStream = await _secureFileBlobService.GetStreamFromAzureBlobAsync(args.File))
                using (TextReader textReader = new StreamReader(fileStream))
                {
                    var officeResolverType = args.JacobusEnergy ? ImportServiceFactory.OfficeResolverType.ByFuelId : ImportServiceFactory.OfficeResolverType.ByName;
                    var importAppService = ImportServiceFactory.GetImportAppService(_iocResolver, args.ImportType, officeResolverType);
                    try
                    {
                        var startDateTime = Clock.Now;
                        var result = await importAppService.Import(
                            textReader,
                            args);
                        var endDateTime = Clock.Now;

                        string resultJsonString = Utility.Serialize(result);
                        await _secureFileBlobService.AddChildBlob(args.File, SecureFileChildFileNames.ImportResult, resultJsonString);

                        try
                        {
                            await EventBus.TriggerAsync(new ImportCompletedEventData(args));
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"Error when triggering the ImportCompletedEventData event: {e}");
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Error in the main try block: {e}");
                        await EventBus.TriggerAsync(new ImportFailedEventData(args));
                    }
                    finally
                    {
                        if (importAppService.OfficeResolver != null)
                        {
                            _iocResolver.Release(importAppService.OfficeResolver);
                        }
                        _iocResolver.Release(importAppService);
                    }
                }

            }
            catch (Exception e)
            {

                Logger.Error($"Error in the ImportJob.Execute method: {e}");
            }
        }
    }
}
