using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Insurances.Dto;
using DispatcherWeb.Storage;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Insurances
{
    [AbpAuthorize(AppPermissions.Pages_LeaseHauler, AppPermissions.LeaseHaulerPortal)]
    public class InsuranceAppService : DispatcherWebAppServiceBase, IInsuranceAppService
    {
        private readonly IRepository<Insurance> _insuranceRepository;
        private readonly IRepository<InsuranceType> _insuranceTypeRepository;
        private readonly IBinaryObjectManager _binaryObjectManager;

        public InsuranceAppService(
            IRepository<Insurance> insuranceRepository,
            IRepository<InsuranceType> insuranceTypeRepository,
            IBinaryObjectManager binaryObjectManager
        )
        {
            _insuranceRepository = insuranceRepository;
            _insuranceTypeRepository = insuranceTypeRepository;
            _binaryObjectManager = binaryObjectManager;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHauler, AppPermissions.LeaseHaulerPortal_MyCompany_Insurance)]
        public async Task<List<InsuranceTypeDto>> GetInsuranceTypes()
        {
            return await (await _insuranceTypeRepository.GetQueryAsync())
                .Select(s => new InsuranceTypeDto
                {
                    Id = s.Id,
                    Name = s.Name,
                })
                .ToListAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHauler, AppPermissions.LeaseHaulerPortal_MyCompany_Insurance)]
        public async Task<PagedResultDto<SelectListDto>> GetInsuranceTypesSelectList(GetSelectListInput input)
        {
            return await (await _insuranceTypeRepository.GetQueryAsync())
                .Select(s => new SelectListDto<InsuranceTypeSelectListInfoDto>
                {
                    Id = s.Id.ToString(),
                    Name = s.Name,
                    Item = new InsuranceTypeSelectListInfoDto
                    {
                        DocumentType = s.DocumentType,
                    },
                })
                .GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Insurance)]
        public async Task<List<InsuranceEditDto>> GetInsurances(int leaseHaulerId)
        {
            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.LeaseHaulerPortal_MyCompany_Profile,
                Session.LeaseHaulerId,
                leaseHaulerId);

            var insurances = await (await _insuranceRepository.GetQueryAsync())
                .Where(q => q.LeaseHaulerId == leaseHaulerId)
                .Select(s => new InsuranceEditDto
                {
                    Id = s.Id,
                    LeaseHaulerId = s.LeaseHaulerId,
                    IsActive = s.IsActive,
                    InsuranceTypeId = s.InsuranceTypeId,
                    InsuranceTypeName = s.InsuranceType.Name,
                    IssueDate = s.IssueDate,
                    ExpirationDate = s.ExpirationDate,
                    IssuedBy = s.IssuedBy,
                    IssuerPhone = s.IssuerPhone,
                    BrokerName = s.BrokerName,
                    BrokerPhone = s.BrokerPhone,
                    CoverageLimit = s.CoverageLimit,
                    Comments = s.Comments,
                    FileId = s.FileId,
                    DocumentType = s.InsuranceType.DocumentType,
                })
                .ToListAsync();
            return insurances;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Insurance)]
        public async Task<List<ActiveInsuranceDto>> GetActiveInsurances(int leaseHaulerId)
        {
            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.LeaseHaulerPortal_MyCompany_Profile,
                Session.LeaseHaulerId,
                leaseHaulerId);

            var insurances = await (await _insuranceRepository.GetQueryAsync())
                .Where(q => q.LeaseHaulerId == leaseHaulerId && q.IsActive)
                .Select(s => new ActiveInsuranceDto
                {
                    InsuranceTypeName = s.InsuranceType.Name,
                    ExpirationDate = s.ExpirationDate,
                })
                .ToListAsync();
            return insurances;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Insurance)]
        public async Task<InsuranceEditDto> EditInsurance(InsuranceEditDto model)
        {
            var permissions = new
            {
                EditAnyLeaseHauler = await IsGrantedAsync(AppPermissions.Pages_LeaseHaulers_Edit),
                EditLeaseHaulerProfile = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_MyCompany_Insurance),
            };

            var entity = model.Id == 0
                ? new Insurance { LeaseHaulerId = model.LeaseHaulerId }
                : await _insuranceRepository.GetAsync(model.Id);

            var oldLeaseHaulerId = entity.LeaseHaulerId;
            if (permissions.EditAnyLeaseHauler)
            {
                entity.LeaseHaulerId = model.LeaseHaulerId;
            }
            entity.InsuranceTypeId = model.InsuranceTypeId;
            entity.IssueDate = model.IssueDate;
            entity.ExpirationDate = model.ExpirationDate;
            entity.IssuedBy = model.IssuedBy;
            entity.IssuerPhone = model.IssuerPhone;
            entity.BrokerName = model.BrokerName;
            entity.BrokerPhone = model.BrokerPhone;
            entity.CoverageLimit = model.CoverageLimit;
            entity.Comments = model.Comments;
            entity.IsActive = model.IsActive;

            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.LeaseHaulerPortal_MyCompany_Insurance,
                Session.LeaseHaulerId,
                entity.LeaseHaulerId,
                oldLeaseHaulerId);

            model.Id = await _insuranceRepository.InsertOrUpdateAndGetIdAsync(entity);

            return model;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Insurance)]
        public async Task DeleteInsurance(EntityDto input)
        {
            var insurance = await _insuranceRepository.FirstOrDefaultAsync(input.Id);
            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.LeaseHaulerPortal_MyCompany_Insurance,
                Session.LeaseHaulerId,
                insurance.LeaseHaulerId);
            await _insuranceRepository.DeleteAsync(insurance);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Insurance)]
        public async Task<AddInsurancePhotoResult> AddInsurancePhoto(AddInsurancePhotoInput input)
        {
            var insurance = await _insuranceRepository.GetAsync(input.InsuranceId);
            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.LeaseHaulerPortal_MyCompany_Insurance,
                Session.LeaseHaulerId,
                insurance.LeaseHaulerId);

            var dataId = await _binaryObjectManager.UploadDataUriStringAsync(input.FileBytesString, await AbpSession.GetTenantIdOrNullAsync() ?? 0);
            if (dataId == null)
            {
                throw new UserFriendlyException("Policy document is required");
            }

            if (insurance.FileId.HasValue)
            {
                await _binaryObjectManager.DeleteAsync(insurance.FileId.Value);
            }
            insurance.FileId = dataId;
            insurance.FileName = input.Filename;
            return new AddInsurancePhotoResult
            {
                FileId = dataId.Value,
            };
        }


        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Insurance)]
        public async Task DeleteInsurancePhoto(DeleteInsurancePhotoInput input)
        {
            var insurance = await _insuranceRepository.GetAsync(input.InsuranceId);
            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.LeaseHaulerPortal_MyCompany_Insurance,
                Session.LeaseHaulerId,
                insurance.LeaseHaulerId);
            if (insurance.FileId.HasValue)
            {
                await _binaryObjectManager.DeleteAsync(insurance.FileId.Value);
            }
            insurance.FileId = null;
            insurance.FileName = null;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHauler, AppPermissions.LeaseHaulerPortal_MyCompany_Insurance)]
        public async Task<InsurancePhotoDto> GetInsurancePhoto(int insuranceId)
        {
            var insurance = await (await _insuranceRepository.GetQueryAsync())
                .Where(x => x.Id == insuranceId)
                .Select(x => new
                {
                    x.FileId,
                    x.FileName,
                    LeaseHaulerId = (int?)x.LeaseHaulerId,
                }).FirstOrDefaultAsync();

            if (insurance?.FileId == null)
            {
                return new InsurancePhotoDto();
            }

            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHauler,
                AppPermissions.LeaseHaulerPortal_MyCompany_Insurance,
                Session.LeaseHaulerId,
                insurance.LeaseHaulerId
            );

            var image = await _binaryObjectManager.GetOrNullAsync(insurance.FileId.Value);
            if (image?.Bytes?.Length > 0)
            {
                return new InsurancePhotoDto
                {
                    FileBytes = image.Bytes,
                    FileName = insurance.FileName ?? (insurance.FileId + ".jpg"),
                };
            }
            return new InsurancePhotoDto();
        }
    }
}
