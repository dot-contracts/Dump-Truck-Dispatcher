using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Trucks;
using DispatcherWeb.VehicleMaintenance;
using DispatcherWeb.WorkOrders.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.WorkOrders
{
    [AbpAuthorize]
    public class WorkOrderAppService : DispatcherWebAppServiceBase, IWorkOrderAppService
    {
        private readonly IAttachmentHelper _attachmentHelper;
        private readonly IRepository<WorkOrder> _workOrderRepository;
        private readonly IRepository<WorkOrderLine> _workOrderLineRepository;
        private readonly IRepository<WorkOrderPicture> _workOrderPictureRepository;
        private readonly IRepository<OutOfServiceHistory> _outOfServiceHistoryRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<VehicleService> _vehicleServiceRepository;
        private readonly IRepository<VehicleServiceType> _vehicleServiceTypeRepository;

        public WorkOrderAppService(
            IAttachmentHelper attachmentHelper,
            IRepository<WorkOrder> workOrderRepository,
            IRepository<WorkOrderLine> workOrderLineRepository,
            IRepository<WorkOrderPicture> workOrderPictureRepository,
            IRepository<OutOfServiceHistory> outOfServiceHistoryRepository,
            IRepository<Truck> truckRepository,
            IRepository<VehicleService> vehicleServiceRepository,
            IRepository<VehicleServiceType> vehicleServiceTypeRepository
        )
        {
            _attachmentHelper = attachmentHelper;
            _workOrderRepository = workOrderRepository;
            _workOrderLineRepository = workOrderLineRepository;
            _workOrderPictureRepository = workOrderPictureRepository;
            _outOfServiceHistoryRepository = outOfServiceHistoryRepository;
            _truckRepository = truckRepository;
            _vehicleServiceRepository = vehicleServiceRepository;
            _vehicleServiceTypeRepository = vehicleServiceTypeRepository;
        }

        [AbpAuthorize(AppPermissions.Pages_WorkOrders_View)]
        public async Task<PagedResultDto<WorkOrderDto>> GetWorkOrderPagedList(GetWorkOrderPagedListInput input)
        {
            var query = (await _workOrderRepository.GetQueryAsync())
                .WhereIf(input.IssueDateBegin.HasValue, x => x.IssueDate >= input.IssueDateBegin.Value)
                .WhereIf(input.IssueDateEnd.HasValue, x => x.IssueDate <= input.IssueDateEnd.Value)
                .WhereIf(input.StartDateBegin.HasValue, x => x.StartDate >= input.StartDateBegin.Value)
                .WhereIf(input.StartDateEnd.HasValue, x => x.StartDate <= input.StartDateEnd.Value)
                .WhereIf(input.CompletionDateBegin.HasValue, x => x.CompletionDate >= input.CompletionDateBegin.Value)
                .WhereIf(input.CompletionDateEnd.HasValue, x => x.CompletionDate <= input.CompletionDateEnd.Value)
                .WhereIf(input.TruckId.HasValue, x => x.TruckId == input.TruckId)
                .WhereIf(input.AssignedToId.HasValue, x => x.AssignedToId == input.AssignedToId)
                .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
                .WhereIf(input.WorkOrderId.HasValue, x => x.Id == input.WorkOrderId);

            var totalCount = await query.CountAsync();

            bool editPermission = await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_WorkOrders_Edit);
            bool editLimitedPermission = await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_WorkOrders_EditLimited);
            Debug.Assert(AbpSession.UserId != null, "AbpSession.UserId != null");
            long userId = AbpSession.UserId.Value;
            var rawItems = await query
                .Select(x => new
                {
                    Id = x.Id,
                    IssueDate = x.IssueDate,
                    StartDate = x.StartDate,
                    CompletionDate = x.CompletionDate,
                    Status = x.Status,
                    Vehicle = x.Truck.TruckCode,
                    Note = x.Note,
                    Odometer = x.Odometer,
                    AssignedTo = x.AssignedTo.Name + " " + x.AssignedTo.Surname,
                    CreatorUserId = x.CreatorUserId,
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();
            var items = rawItems.Select(x => new WorkOrderDto
            {
                Id = x.Id,
                IssueDate = x.IssueDate,
                StartDate = x.StartDate,
                CompletionDate = x.CompletionDate,
                Status = x.Status.GetDisplayName(),
                Vehicle = x.Vehicle,
                Note = x.Note,
                Odometer = x.Odometer,
                AssignedTo = x.AssignedTo,
                CanEdit = editPermission || editLimitedPermission && x.CreatorUserId == userId,
            }).ToList();

            return new PagedResultDto<WorkOrderDto>(totalCount, items);
        }

        [AbpAuthorize(AppPermissions.Pages_WorkOrders_View)]
        public async Task<WorkOrderEditDto> GetWorkOrderForEdit(NullableIdDto input)
        {
            WorkOrderEditDto dto;
            if (input.Id.HasValue)
            {
                dto = await (await _workOrderRepository.GetQueryAsync())
                    .Select(x => new WorkOrderEditDto
                    {
                        Id = x.Id,
                        VehicleServiceTypeId = x.VehicleServiceTypeId,
                        VehicleServiceTypeName = x.VehicleServiceType.Name,
                        IssueDate = x.IssueDate,
                        StartDate = x.StartDate,
                        CompletionDate = x.CompletionDate,
                        Status = x.Status,
                        TruckId = x.TruckId,
                        TruckCode = x.Truck.TruckCode,
                        Note = x.Note,
                        Odometer = x.Odometer,
                        Hours = x.Hours,
                        AssignedToId = x.AssignedToId,
                        AssignedToName = x.AssignedTo.Name + " " + x.AssignedTo.Surname,
                        TotalLaborCost = x.TotalLaborCost,
                        IsTotalLaborCostOverridden = x.IsTotalLaborCostOverridden,
                        TotalPartsCost = x.TotalPartsCost,
                        IsTotalPartsCostOverridden = x.IsTotalPartsCostOverridden,
                        Tax = x.Tax,
                        Discount = x.Discount,
                        TotalCost = x.TotalCost,

                        Pictures = x.WorkOrderPictures.Select(p => new WorkOrderPictureEditDto
                        {
                            Id = p.Id,
                            WorkOrderId = p.WorkOrderId,
                            FileId = p.FileId,
                            FileName = p.FileName,
                        }).ToList(),
                    })
                    .FirstOrDefaultAsync(x => x.Id == input.Id.Value);
            }
            else
            {
                var today = await GetToday();
                dto = new WorkOrderEditDto
                {
                    IssueDate = today,
                    StartDate = today,

                    Pictures = new List<WorkOrderPictureEditDto>(),
                };

            }
            return dto;
        }

        [AbpAuthorize(AppPermissions.Pages_WorkOrders_Edit, AppPermissions.Pages_WorkOrders_EditLimited)]
        public async Task<WorkOrderEditDto> SaveWorkOrder(WorkOrderEditDto model)
        {
            var entity = model.Id != 0 ? await _workOrderRepository.GetAsync(model.Id) : new WorkOrder();

            await EnsurePermissionsForEntity(entity);

            if (await IsUserInactive(model.AssignedToId))
            {
                throw new UserFriendlyException($"You are trying to assign this work order to an inactive user!");
            }

            if (model.CompletionDate > await GetToday())
            {
                throw new UserFriendlyException("Some of the data is invalid", "The \"Completion Date\" cannot be later than today");
            }

            //if (model.Id == 0)
            //{
            //    await CreateOutOfServiceHistory(model.TruckId, model.IssueDate, model.Note);
            //}

            if (model.Status != WorkOrderStatus.Complete && entity.Status == WorkOrderStatus.Complete)
            {
                model.CompletionDate = null;
            }

            var latestCompletedWorkOrder = await (await _workOrderRepository.GetQueryAsync())
                .Where(wo => wo.TruckId == model.TruckId
                       && wo.VehicleServiceTypeId == model.VehicleServiceTypeId
                       && wo.Status == WorkOrderStatus.Complete)
                .OrderByDescending(wo => wo.CompletionDate)
                .Select(x => new
                {
                    x.Id,
                })
                .FirstOrDefaultAsync();

            bool isFirstCompletion = latestCompletedWorkOrder == null;
            bool isLatestWorkOrder = latestCompletedWorkOrder?.Id == model.Id;
            bool isNewlyCompleted = entity.Status != WorkOrderStatus.Complete && model.Status == WorkOrderStatus.Complete;

            if (model.Status == WorkOrderStatus.Complete
                && (isFirstCompletion
                    || isNewlyCompleted
                    || isLatestWorkOrder
                    && (model.Odometer != entity.Odometer
                        || model.Hours != entity.Hours
                        || model.CompletionDate != entity.CompletionDate))
                  )
            {
                await OnWorkOrderCompleted(model);
            }

            entity.VehicleServiceTypeId = model.VehicleServiceTypeId;
            entity.IssueDate = model.IssueDate;
            entity.StartDate = model.StartDate;
            entity.CompletionDate = model.CompletionDate;
            entity.Status = model.Status;
            entity.TruckId = model.TruckId;
            entity.Note = model.Note.Truncate(EntityStringFieldLengths.WorkOrder.Note);
            entity.Odometer = model.Odometer;
            entity.Hours = model.Hours;
            entity.AssignedToId = model.AssignedToId;
            entity.TotalLaborCost = model.TotalLaborCost;
            entity.IsTotalLaborCostOverridden = model.IsTotalLaborCostOverridden;
            entity.TotalPartsCost = model.TotalPartsCost;
            entity.IsTotalPartsCostOverridden = model.IsTotalPartsCostOverridden;
            entity.Tax = model.Tax;
            entity.Discount = model.Discount;
            entity.TotalCost = model.TotalCost;

            model.Id = await _workOrderRepository.InsertOrUpdateAndGetIdAsync(entity);

            var truckEntity = await _truckRepository.GetAsync(model.TruckId);
            if (truckEntity.CurrentMileage < entity.Odometer)
            {
                truckEntity.CurrentMileage = entity.Odometer;
            }
            if (truckEntity.CurrentHours < entity.Hours)
            {
                truckEntity.CurrentHours = entity.Hours;
            }

            return model;
        }

        private async Task<bool> IsUserInactive(long? userId)
        {
            if (!userId.HasValue)
            {
                return false;
            }

            return await (await UserManager.GetQueryAsync()).AnyAsync(u => u.Id == userId && !u.IsActive);
        }


        private async Task OnWorkOrderCompleted(WorkOrderEditDto model)
        {
            if (!model.CompletionDate.HasValue)
            {
                model.CompletionDate = await GetToday();
            }

            if (model.Id != 0)  // A new (Id == 0) WorkOrder cannot have WorkOrderLines
            {
                await UpdatePreventiveMaintenanceFromWorkOrder(model);
            }
        }

        private async Task CreateOutOfServiceHistory(int truckId, DateTime issueDate, string note)
        {
            var truck = await _truckRepository.GetAsync(truckId);
            if (!truck.IsOutOfService)
            {
                truck.IsOutOfService = true;

                var outOfServiceHistory = new OutOfServiceHistory
                {
                    TruckId = truckId,
                    OutOfServiceDate = issueDate,
                    Reason = $"Work Order for {note}".Truncate(500),
                };

                await _outOfServiceHistoryRepository.InsertAsync(outOfServiceHistory);
            }
        }
        private async Task SetInServiceDateForTruckAndOutOfServiceHistory(int truckId, DateTime inServiceDate, int completedWorkOrderId)
        {
            if (await (await _workOrderRepository.GetQueryAsync()).AnyAsync(wo => wo.Id != completedWorkOrderId))
            {
                return;
            }
            var truck = await _truckRepository.GetAsync(truckId);
            truck.IsOutOfService = false;
            truck.InServiceDate = inServiceDate;

            await _outOfServiceHistoryRepository.SetInServiceDate(truckId, inServiceDate);
        }
        private async Task UpdatePreventiveMaintenanceFromWorkOrder(WorkOrderEditDto model)
        {
            var preventiveMaintenanceToComplete = await (await _workOrderLineRepository.GetQueryAsync())
                .Where(wol => wol.WorkOrderId == model.Id)
                .SelectMany(wol => wol.VehicleService.PreventiveMaintenance)
                .Distinct()
                .Where(pm => pm.TruckId == model.TruckId)
                .Select(pm => pm)
                .ToListAsync();

            var vehicleServiceIds = preventiveMaintenanceToComplete.Select(x => x.VehicleServiceId).ToList();
            var vehicleServices = await (await _vehicleServiceRepository.GetQueryAsync())
                .Where(x => vehicleServiceIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.WarningDays,
                    x.WarningHours,
                    x.WarningMiles,
                    x.RecommendedHourInterval,
                    x.RecommendedMileageInterval,
                    x.RecommendedTimeInterval,
                })
                .ToListAsync();

            foreach (var preventiveMaintenance in preventiveMaintenanceToComplete)
            {
                preventiveMaintenance.LastDate = model.CompletionDate ?? await GetToday();
                preventiveMaintenance.LastMileage = model.Odometer;
                preventiveMaintenance.LastHour = model.Hours;

                var vehicleService = vehicleServices.FirstOrDefault(x => x.Id == preventiveMaintenance.VehicleServiceId);
                if (vehicleService == null)
                {
                    continue;
                }

                if (vehicleService.RecommendedHourInterval.HasValue)
                {
                    preventiveMaintenance.DueHour = preventiveMaintenance.LastHour + vehicleService.RecommendedHourInterval.Value;
                    if (vehicleService.WarningHours.HasValue)
                    {
                        preventiveMaintenance.WarningHour = Math.Max(preventiveMaintenance.DueHour.Value - vehicleService.WarningHours.Value, 0M);
                    }
                }

                if (vehicleService.RecommendedTimeInterval.HasValue)
                {
                    preventiveMaintenance.DueDate = preventiveMaintenance.LastDate.AddDays(vehicleService.RecommendedTimeInterval.Value);
                    if (vehicleService.WarningDays.HasValue)
                    {
                        preventiveMaintenance.WarningDate = preventiveMaintenance.DueDate.Value.AddDays(-vehicleService.WarningDays.Value);
                    }
                }

                if (vehicleService.RecommendedMileageInterval.HasValue)
                {
                    preventiveMaintenance.DueMileage = preventiveMaintenance.LastMileage + vehicleService.RecommendedMileageInterval.Value;
                    if (vehicleService.WarningMiles.HasValue)
                    {
                        preventiveMaintenance.WarningMileage = Math.Max(preventiveMaintenance.DueMileage.Value - vehicleService.WarningMiles.Value, 0);
                    }
                }
            }
        }

        private async Task EnsurePermissionsForEntity(FullAuditedEntity entity)
        {
            if (entity.Id != 0 && !await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_PreventiveMaintenanceSchedule_Edit))
            {
                Debug.Assert(await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_WorkOrders_EditLimited));
                Debug.Assert(AbpSession.UserId != null, "AbpSession.UserId != null");
                if (entity.CreatorUserId != AbpSession.UserId.Value)
                {
                    throw new AbpAuthorizationException();
                }
            }
        }



        [AbpAuthorize(AppPermissions.Pages_WorkOrders_Edit, AppPermissions.Pages_WorkOrders_EditLimited)]
        public async Task DeleteWorkOrder(EntityDto input)
        {
            var workOrder = await (await _workOrderRepository.GetQueryAsync())
                .Include(x => x.WorkOrderLines)
                .SingleAsync(x => x.Id == input.Id);
            await EnsurePermissionsForEntity(workOrder);
            foreach (var workOrderLine in workOrder.WorkOrderLines.ToList())
            {
                await _workOrderLineRepository.DeleteAsync(workOrderLine);
            }
            await _workOrderRepository.DeleteAsync(workOrder);
        }

        [AbpAuthorize(AppPermissions.Pages_WorkOrders_View)]
        public async Task<PagedResultDto<WorkOrderLineDto>> GetWorkOrderLines(GetWorkOrderLinesInput input)
        {
            var query = (await _workOrderLineRepository.GetQueryAsync())
                .Where(x => x.WorkOrderId == input.Id)
                ;
            int totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new WorkOrderLineDto
                {
                    Id = x.Id,
                    VehicleServiceName = x.VehicleService.Name,
                    Note = x.Note,
                    LaborTime = x.LaborTime,
                    LaborCost = x.LaborCost,
                    LaborRate = x.LaborRate,
                    PartsCost = x.PartsCost,
                })
                .OrderBy(input.Sorting)
                .ToListAsync();

            return new PagedResultDto<WorkOrderLineDto>(totalCount, items);
        }

        [AbpAuthorize(AppPermissions.Pages_WorkOrders_View)]
        public async Task<WorkOrderLineEditDto> GetWorkOrderLineForEdit(GetWorkOrderLineForEditInput input)
        {
            WorkOrderLineEditDto dto;
            if (input.Id.HasValue)
            {
                dto = await (await _workOrderLineRepository.GetQueryAsync())
                    .Select(x => new WorkOrderLineEditDto
                    {
                        Id = x.Id,
                        WorkOrderId = x.WorkOrderId,
                        VehicleServiceId = x.VehicleServiceId,
                        VehicleServiceName = x.VehicleService.Name,
                        Note = x.Note,
                        LaborTime = x.LaborTime,
                        LaborCost = x.LaborCost,
                        LaborRate = x.LaborRate,
                        PartsCost = x.PartsCost,
                    })
                    .FirstOrDefaultAsync(x => x.Id == input.Id.Value);
            }
            else
            {
                dto = new WorkOrderLineEditDto
                {
                    WorkOrderId = input.WorkOrderId,
                };
            }
            return dto;
        }

        [AbpAuthorize(AppPermissions.Pages_WorkOrders_Edit, AppPermissions.Pages_WorkOrders_EditLimited)]
        public async Task<SaveWorkOrderLineResult> SaveWorkOrderLine(WorkOrderLineEditDto model)
        {
            WorkOrderLine entity;
            if (model.Id != 0)
            {
                entity = await _workOrderLineRepository.GetAsync(model.Id);
            }
            else
            {
                entity = new WorkOrderLine
                {
                    WorkOrderId = model.WorkOrderId,
                };
            }
            await EnsurePermissionsForEntity(entity);

            entity.VehicleServiceId = model.VehicleServiceId;
            entity.Note = model.Note;
            entity.LaborTime = model.LaborTime;
            entity.LaborCost = model.LaborCost;
            entity.LaborRate = model.LaborRate;
            entity.PartsCost = model.PartsCost;

            model.Id = await _workOrderLineRepository.InsertOrUpdateAndGetIdAsync(entity);

            return await UpdateWorkOrderTotals(model.WorkOrderId);
        }

        private async Task<SaveWorkOrderLineResult> UpdateWorkOrderTotals(int workOrderId)
        {
            await CurrentUnitOfWork.SaveChangesAsync();

            var workOrder = await _workOrderRepository.GetAsync(workOrderId);
            var workOrderDetails = await (await _workOrderRepository.GetQueryAsync())
                .Where(x => x.Id == workOrderId)
                .Select(x => new
                {
                    TotalLaborCost = x.WorkOrderLines.Select(l => l.LaborCost == null ? 0 : l.LaborCost.Value).Sum(),
                    TotalPartsCost = x.WorkOrderLines.Select(l => l.PartsCost == null ? 0 : l.PartsCost.Value).Sum(),
                }).FirstAsync();

            var result = new SaveWorkOrderLineResult
            {
                TotalLaborCost = workOrder.IsTotalLaborCostOverridden ? workOrder.TotalLaborCost : workOrderDetails.TotalLaborCost,
                TotalPartsCost = workOrder.IsTotalPartsCostOverridden ? workOrder.TotalPartsCost : workOrderDetails.TotalPartsCost,
            };

            var subTotal = result.TotalLaborCost + result.TotalPartsCost;
            var taxAmount = Math.Round(subTotal * workOrder.Tax / 100, 2);
            var discountAmount = Math.Round((subTotal + taxAmount) * workOrder.Discount / 100, 2);
            result.TotalCost = subTotal + taxAmount - discountAmount;

            workOrder.TotalLaborCost = result.TotalLaborCost;
            workOrder.TotalPartsCost = result.TotalPartsCost;
            workOrder.TotalCost = result.TotalCost;

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_WorkOrders_Edit, AppPermissions.Pages_WorkOrders_EditLimited)]
        public async Task<SaveWorkOrderLineResult> DeleteWorkOrderLine(EntityDto input)
        {
            var entity = await _workOrderLineRepository.GetAsync(input.Id);
            await EnsurePermissionsForEntity(entity);
            var workOrderId = entity.WorkOrderId;
            await _workOrderLineRepository.DeleteAsync(entity);

            return await UpdateWorkOrderTotals(workOrderId);
        }

        [AbpAuthorize(AppPermissions.Pages_WorkOrders_Edit, AppPermissions.Pages_WorkOrders_EditLimited)]
        public async Task<WorkOrderPictureEditDto> SavePicture(WorkOrderPictureEditDto model)
        {
            var entity = model.Id != 0 ? await _workOrderPictureRepository.GetAsync(model.Id) : new WorkOrderPicture();
            entity.WorkOrderId = model.WorkOrderId;
            entity.FileId = model.FileId;
            entity.FileName = model.FileName;
            model.Id = await _workOrderPictureRepository.InsertOrUpdateAndGetIdAsync(entity);
            return model;
        }

        [AbpAuthorize(AppPermissions.Pages_WorkOrders_Edit, AppPermissions.Pages_WorkOrders_EditLimited)]
        public async Task DeletePicture(EntityDto input)
        {
            var entity = await _workOrderPictureRepository.GetAsync(input.Id);
            await _attachmentHelper.DeleteFromAzureBlobAsync($"{entity.WorkOrderId}/{entity.FileId}", BlobContainerNames.WorkOrderPictures);
            await _workOrderPictureRepository.DeleteAsync(entity);
        }

        [AbpAuthorize(AppPermissions.Pages_WorkOrders_Edit, AppPermissions.Pages_WorkOrders_EditLimited)]
        public async Task<WorkOrderPictureEditDto> GetPictureEditDto(int id)
        {
            return await (await _workOrderPictureRepository.GetQueryAsync())
                .Select(x => new WorkOrderPictureEditDto
                {
                    Id = x.Id,
                    WorkOrderId = x.WorkOrderId,
                    FileId = x.FileId,
                    FileName = x.FileName,
                })
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        [AbpAuthorize(AppPermissions.Pages_WorkOrders_Edit, AppPermissions.Pages_WorkOrders_EditLimited)]
        public async Task CreateWorkOrdersFromPreventiveMaintenance(CreateWorkOrdersFromPreventiveMaintenanceInput input)
        {
            var workOrderTrucks = await (await _truckRepository.GetQueryAsync())
                .Where(t => t.PreventiveMaintenances.Any(pm => input.PreventiveMaintenanceIds.Contains(pm.Id)))
                .Select(t => new
                {
                    t.Id,
                    PreventiveMaintenances = t.PreventiveMaintenances
                        .Where(pm => input.PreventiveMaintenanceIds.Contains(pm.Id))
                        .Select(pm => new { pm.VehicleServiceId })
                        .ToList()
                    ,
                })
                .ToListAsync();

            var vehicleServiceType = await (await _vehicleServiceTypeRepository.GetQueryAsync())
                .Where(vst => vst.Name == AppConsts.PreventiveMaintenanceServiceTypeName)
                .Select(vst => vst.Id)
                .FirstOrDefaultAsync();

            foreach (var workOrderTruck in workOrderTrucks)
            {
                var workOrderEditDto = new WorkOrderEditDto
                {
                    IssueDate = await GetToday(),
                    TruckId = workOrderTruck.Id,
                    Status = WorkOrderStatus.Pending,
                    VehicleServiceTypeId = vehicleServiceType != 0 ? (int?)vehicleServiceType : null,
                };
                workOrderEditDto = await SaveWorkOrder(workOrderEditDto);

                foreach (var preventiveMaintenance in workOrderTruck.PreventiveMaintenances)
                {
                    var workOrderLineEditDto = new WorkOrderLineEditDto
                    {
                        WorkOrderId = workOrderEditDto.Id,
                        VehicleServiceId = preventiveMaintenance.VehicleServiceId,
                    };
                    await SaveWorkOrderLine(workOrderLineEditDto);
                }
            }

        }

    }
}
