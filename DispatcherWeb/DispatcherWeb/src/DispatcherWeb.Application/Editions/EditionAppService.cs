using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Editions;
using Abp.Application.Features;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dto;
using DispatcherWeb.Editions.Dto;
using DispatcherWeb.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Editions
{
    [AbpAuthorize(AppPermissions.Pages_Editions)]
    public class EditionAppService : DispatcherWebAppServiceBase, IEditionAppService
    {
        private readonly EditionManager _editionManager;
        private readonly IRepository<SubscribableEdition> _editionRepository;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IBackgroundJobManager _backgroundJobManager;

        public EditionAppService(
            EditionManager editionManager,
            IRepository<SubscribableEdition> editionRepository,
            IRepository<Tenant> tenantRepository,
            IBackgroundJobManager backgroundJobManager)
        {
            _editionManager = editionManager;
            _editionRepository = editionRepository;
            _tenantRepository = tenantRepository;
            _backgroundJobManager = backgroundJobManager;
        }

        [AbpAuthorize(AppPermissions.Pages_Editions)]
        public async Task<ListResultDto<EditionListDto>> GetEditions()
        {
            var editions = await (from edition in await _editionRepository.GetQueryAsync()
                                  join expiringEdition in await _editionRepository.GetQueryAsync() on edition.ExpiringEditionId equals expiringEdition.Id into expiringEditionJoined
                                  from expiringEdition in expiringEditionJoined.DefaultIfEmpty()
                                  select new
                                  {
                                      Edition = edition,
                                      expiringEditionDisplayName = expiringEdition.DisplayName,
                                  }).ToListAsync();

            var result = new List<EditionListDto>();

            foreach (var edition in editions)
            {
                var resultEdition = new EditionListDto
                {
                    Id = edition.Edition.Id,
                    Name = edition.Edition.Name,
                    DisplayName = edition.Edition.DisplayName,
                    DailyPrice = edition.Edition.DailyPrice,
                    WeeklyPrice = edition.Edition.WeeklyPrice,
                    MonthlyPrice = edition.Edition.MonthlyPrice,
                    AnnualPrice = edition.Edition.AnnualPrice,
                    WaitingDayAfterExpire = edition.Edition.WaitingDayAfterExpire,
                    TrialDayCount = edition.Edition.TrialDayCount,
                    ExpiringEditionDisplayName = edition.expiringEditionDisplayName,
                    CreationTime = edition.Edition.CreationTime,
                };

                result.Add(resultEdition);
            }

            return new ListResultDto<EditionListDto>(result);
        }

        [AbpAuthorize(AppPermissions.Pages_Editions_Create, AppPermissions.Pages_Editions_Edit)]
        public async Task<GetEditionEditOutput> GetEditionForEdit(NullableIdDto input)
        {
            var features = FeatureManager.GetAll()
                .Where(f => f.Scope.HasFlag(FeatureScopes.Edition))
                .Select(x => new FlatFeatureDto
                {
                    ParentName = x.Parent?.Name,
                    Name = x.Name,
                    DisplayName = L(x.DisplayName),
                    Description = L(x.Description),
                    DefaultValue = x.DefaultValue,
                    InputType = new FeatureInputTypeDto
                    {
                        Name = x.InputType.Name,
                        Attributes = x.InputType.Attributes,
                        Validator = x.InputType.Validator,
                    },
                });

            EditionEditDto editionEditDto;
            List<NameValue> featureValues;

            if (input.Id.HasValue) //Editing existing edition?
            {
                var edition = await _editionManager.GetByIdAsync(input.Id.Value);
                featureValues = (await _editionManager.GetFeatureValuesAsync(input.Id.Value)).ToList();
                editionEditDto = new EditionEditDto
                {
                    Id = edition.Id,
                    DisplayName = edition.DisplayName,
                };
            }
            else
            {
                editionEditDto = new EditionEditDto();
                featureValues = features.Select(f => new NameValue(f.Name, f.DefaultValue)).ToList();
            }

            var featureDtos = features
                .OrderBy(x => x.DisplayName)
                .ToList();

            return new GetEditionEditOutput
            {
                Edition = editionEditDto,
                Features = featureDtos,
                FeatureValues = featureValues.Select(fv => new NameValueDto(fv)).ToList(),
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Editions_Create)]
        public async Task CreateEdition(CreateEditionDto input)
        {
            await CreateEditionAsync(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Editions_Edit)]
        public async Task UpdateEdition(CreateEditionDto input)
        {
            await UpdateEditionAsync(input);
        }

        public async Task CreateOrUpdateEdition(CreateEditionDto input)
        {
            if (input.Edition.Id.HasValue)
            {
                await UpdateEditionAsync(input);
            }
            else
            {
                await CreateEditionAsync(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Editions_Delete)]
        public async Task DeleteEdition(EntityDto input)
        {
            var hasTenants = await _tenantRepository.AnyAsync(t => t.EditionId == input.Id);
            if (hasTenants)
            {
                throw new UserFriendlyException(L("ThereAreTenantsSubscribedToThisEdition"));
            }

            var edition = await _editionManager.GetByIdAsync(input.Id);
            await _editionManager.DeleteAsync(edition);
        }

        [AbpAuthorize(AppPermissions.Pages_Editions_MoveTenantsToAnotherEdition)]
        public async Task MoveTenantsToAnotherEdition(MoveTenantsToAnotherEditionDto input)
        {
            await _backgroundJobManager.EnqueueAsync<MoveTenantsToAnotherEditionJob, MoveTenantsToAnotherEditionJobArgs>(new MoveTenantsToAnotherEditionJobArgs
            {
                SourceEditionId = input.SourceEditionId,
                TargetEditionId = input.TargetEditionId,
                User = await AbpSession.ToUserIdentifierAsync(),
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Editions, AppPermissions.Pages_Tenants)]
        public async Task<List<SubscribableEditionComboboxItemDto>> GetEditionComboboxItems(int? selectedEditionId = null, bool addAllItem = false, bool onlyFreeItems = false)
        {
            var subscribableEditions = (await (await _editionManager.GetQueryAsync()).AsNoTracking().ToListAsync())
                .Cast<SubscribableEdition>()
                .WhereIf(onlyFreeItems, e => e.IsFree)
                .OrderBy(e => e.MonthlyPrice);

            var editionItems =
                new ListResultDto<SubscribableEditionComboboxItemDto>(subscribableEditions
                    .Select(e => new SubscribableEditionComboboxItemDto(e.Id.ToString(), e.DisplayName, e.IsFree)).ToList()).Items.ToList();

            var defaultItem = new SubscribableEditionComboboxItemDto("", L("NotAssigned"), null);
            editionItems.Insert(0, defaultItem);

            if (addAllItem)
            {
                editionItems.Insert(0, new SubscribableEditionComboboxItemDto("-1", "- " + L("All") + " -", null));
            }

            if (selectedEditionId.HasValue)
            {
                var selectedEdition = editionItems.FirstOrDefault(e => e.Value == selectedEditionId.Value.ToString());
                if (selectedEdition != null)
                {
                    selectedEdition.IsSelected = true;
                }
            }
            else
            {
                editionItems[0].IsSelected = true;
            }

            return editionItems;
        }

        public async Task<int> GetTenantCount(int editionId)
        {
            return await _tenantRepository.CountAsync(t => t.EditionId == editionId);
        }

        public async Task<PagedResultDto<SelectListDto>> GetEditionsSelectList(GetSelectListInput input)
        {
            var query = (await _editionManager.GetQueryAsync())
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.DisplayName,
                });

            return await query.GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Editions_Create)]
        protected virtual async Task CreateEditionAsync(CreateEditionDto input)
        {
            var edition = new SubscribableEdition
            {
                ExpiringEditionId = input.Edition.ExpiringEditionId,
                MonthlyPrice = input.Edition.MonthlyPrice,
                DisplayName = input.Edition.DisplayName,
                WeeklyPrice = input.Edition.WeeklyPrice,
                DailyPrice = input.Edition.DailyPrice,
                AnnualPrice = input.Edition.AnnualPrice,
                TrialDayCount = input.Edition.TrialDayCount,
            };

            if (edition.ExpiringEditionId.HasValue)
            {
                var expiringEdition = (SubscribableEdition)await _editionManager.GetByIdAsync(edition.ExpiringEditionId.Value);
                if (!expiringEdition.IsFree)
                {
                    throw new UserFriendlyException(L("ExpiringEditionMustBeAFreeEdition"));
                }
            }

            await _editionManager.CreateAsync(edition);
            await CurrentUnitOfWork.SaveChangesAsync(); //It's done to get Id of the edition.

            await SetFeatureValues(edition, input.FeatureValues);
        }

        [AbpAuthorize(AppPermissions.Pages_Editions_Edit)]
        protected virtual async Task UpdateEditionAsync(CreateEditionDto input)
        {
            if (input.Edition.Id != null)
            {
                var edition = await _editionManager.GetByIdAsync(input.Edition.Id.Value);

                edition.DisplayName = input.Edition.DisplayName;

                await SetFeatureValues(edition, input.FeatureValues);
            }
        }

        private Task SetFeatureValues(Edition edition, List<NameValueDto> featureValues)
        {
            return _editionManager.SetFeatureValuesAsync(edition.Id,
                featureValues.Select(fv => new NameValue(fv.Name, fv.Value)).ToArray());
        }
    }
}
