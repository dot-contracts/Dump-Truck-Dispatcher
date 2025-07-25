using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Events.Bus.Handlers;
using Abp.Runtime.Session;
using DispatcherWeb.Identity;
using DispatcherWeb.Imports.Services;
using DispatcherWeb.Infrastructure.EventBus.Events;

namespace DispatcherWeb.Infrastructure.EventBus.EventHandlers
{
    public class ImportCompletedTruckUpdater : IAsyncEventHandler<ImportCompletedEventData>, ITransientDependency
    {
        private readonly IUpdateTruckFromImportAppService _updateTruckFromImportAppService;
        private readonly IAbpSession _session;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public ImportCompletedTruckUpdater(
            IUpdateTruckFromImportAppService updateTruckFromImportAppService,
            IAbpSession session,
            IUnitOfWorkManager unitOfWorkManager
            )
        {
            _updateTruckFromImportAppService = updateTruckFromImportAppService;
            _session = session;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task HandleEventAsync(ImportCompletedEventData eventData)
        {
            if (eventData.Args.ImportType == ImportType.VehicleUsage)
            {
                await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (_session.Use(eventData.Args.RequestorUser.TenantId, eventData.Args.RequestorUser.UserId))
                    {
                        await _updateTruckFromImportAppService.UpdateMileageAndHoursAsync(eventData.Args.RequestorUser.GetTenantId(), eventData.Args.RequestorUser.UserId);
                    }
                });
            }
        }
    }
}
