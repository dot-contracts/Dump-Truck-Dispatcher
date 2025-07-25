using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Notifications;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dispatching;
using DispatcherWeb.DriverApp.Loads.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Notifications;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.Loads
{
    [AbpAuthorize(AppPermissions.Pages_DriverApplication_ReactNativeDriverApp)]
    public class LoadAppService : DispatcherWebDriverAppAppServiceBase, ILoadAppService
    {
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<Load> _loadRepository;

        public LoadAppService(
            IAppNotifier appNotifier,
            IRepository<Dispatch> dispatchRepository,
            IRepository<Load> loadRepository
            )
        {
            _appNotifier = appNotifier;
            _dispatchRepository = dispatchRepository;
            _loadRepository = loadRepository;
        }

        public async Task<LoadDto> Post(LoadDto model)
        {
            if (model.Id == 0 && model.Guid.HasValue)
            {
                var existingLoad = await (await _loadRepository.GetQueryAsync())
                    .Where(x => x.DispatchId == model.DispatchId
                        && x.Guid == model.Guid.Value)
                    .Select(x => new
                    {
                        x.Id,
                    })
                    .FirstOrDefaultAsync();

                if (existingLoad != null)
                {
                    model.Id = existingLoad.Id;
                }
            }

            var load = model.Id == 0 ? new Load() : await _loadRepository.FirstOrDefaultAsync(model.Id);

            if (load == null)
            {
                var deletedLoad = await _loadRepository.GetDeletedEntity(new EntityDto(model.Id), CurrentUnitOfWork);
                if (deletedLoad == null)
                {
                    throw new UserFriendlyException($"Load with id {model.Id} wasn't found");
                }
                await SendDeletedRnEntityNotificationIfNeededAsync(deletedLoad, model);
                deletedLoad.UnDelete();
                load = deletedLoad;
            }

            if (!await (await _dispatchRepository.GetQueryAsync()).AnyAsync(x => x.Id == model.DispatchId,
                    CancellationTokenProvider.Token))
            {
                throw new UserFriendlyException($"Dispatch with id {model.DispatchId} wasn't found");
            }

            if (!await (await _dispatchRepository.GetQueryAsync()).AnyAsync(x => x.Id == model.DispatchId && x.Driver.UserId == Session.UserId,
                    CancellationTokenProvider.Token))
            {
                throw new UserFriendlyException($"You cannot edit dispatches assigned to other users");
            }

            load.SourceDateTime = model.SourceDateTime;
            load.SourceLatitude = model.SourceLatitude;
            load.SourceLongitude = model.SourceLongitude;
            load.DestinationDateTime = model.DestinationDateTime;
            load.DestinationLatitude = model.DestinationLatitude;
            load.DestinationLongitude = model.DestinationLongitude;
            load.SignatureId = model.SignatureId;
            load.SignatureName = model.SignatureName?.TruncateWithPostfix(EntityStringFieldLengths.Load.SignatureName);

            if (load.Id == 0)
            {
                load.Guid = model.Guid;
                load.DispatchId = model.DispatchId;
                load.TravelTime = model.TravelTime;

                await _loadRepository.InsertAndGetIdAsync(load);
                model.Id = load.Id;
            }

            return model;
        }

        private async Task<string> GetMeaningfulLoadDiffAsync(Load deletedLoad, LoadDto model)
        {
            var result = "";
            var timezone = await GetTimezone();
            if (deletedLoad.SourceDateTime != model.SourceDateTime)
            {
                result += $"Source Date/Time: {deletedLoad.SourceDateTime?.ConvertTimeZoneTo(timezone):g} ➔ {model.SourceDateTime?.ConvertTimeZoneTo(timezone):g}; ";
            }

            if (deletedLoad.DestinationDateTime != model.DestinationDateTime)
            {
                result += $"Destination Date/Time: {deletedLoad.DestinationDateTime?.ConvertTimeZoneTo(timezone):g} ➔ {model.DestinationDateTime?.ConvertTimeZoneTo(timezone):g}; ";
            }

            if (deletedLoad.SignatureId != model.SignatureId)
            {
                result += $"Signature Id: {deletedLoad.SignatureId} ➔ {model.SignatureId}; ";
            }

            return result;
        }

        private async Task SendDeletedRnEntityNotificationIfNeededAsync(Load deletedLoad, LoadDto model)
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.SendRnConflictsToUsers))
            {
                return;
            }

            var loadDiff = await GetMeaningfulLoadDiffAsync(deletedLoad, model);

            await _appNotifier.SendNotificationAsync(
                new SendNotificationInput(
                    AppNotificationNames.SimpleMessage,
                    $"A load that has been deleted in the main app was uploaded from the native driver app. Driver: {await GetCurrentUserFullName()}; {loadDiff}",
                    NotificationSeverity.Warn
                )
                {
                    IncludeLocalUsers = true,
                    PermissionFilter = AppPermissions.ReceiveRnConflicts,
                });
        }
    }
}
