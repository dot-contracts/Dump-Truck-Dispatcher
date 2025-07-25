using System.Collections.Generic;
using System.Linq;
using DispatcherWeb.Dispatching.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.Scheduling
{
    public class SendDispatchMessageModalViewModel
    {
        public SendDispatchMessageModalViewModel(SendDispatchMessageDto sendDispatchMessageDto, bool canAddDispatchBasedOnTime, int? selectedOrderLineTruckId)
        {
            OrderLineId = sendDispatchMessageDto.OrderLineId;
            OrderLineTrucks = sendDispatchMessageDto.OrderLineTrucks
                .Select(x => new OrderLineTruckSelectListItem
                {
                    Text = $"{x.TruckCode} - {x.DriverName}",
                    Value = x.OrderLineTruckId.ToString(),
                    Selected = selectedOrderLineTruckId.HasValue
                        ? selectedOrderLineTruckId == x.OrderLineTruckId
                        : true,
                    HasContactInfo = x.HasContactInfo,
                    OrderLineTruckId = x.OrderLineTruckId,
                }).ToList();
            Message = sendDispatchMessageDto.Message;
            IsMultipleLoads = sendDispatchMessageDto.IsMultipleLoads;
            CanAddDispatchBasedOnTime = canAddDispatchBasedOnTime;
            FreightUom = sendDispatchMessageDto.FreightUom;
            FreightUomBaseId = sendDispatchMessageDto.FreightUomBaseId;
            UseTruckCapacityForLoadQuantity = sendDispatchMessageDto.UseTruckCapacityForLoadQuantity;
            LoadMaterialQuantity = sendDispatchMessageDto.LoadMaterialQuantity;
            MaterialItemId = sendDispatchMessageDto.MaterialItemId;
        }

        public int OrderLineId { get; set; }
        public IList<OrderLineTruckSelectListItem> OrderLineTrucks { get; set; }
        public string Message { get; }
        public bool IsMultipleLoads { get; set; }
        public bool CanAddDispatchBasedOnTime { get; set; }
        public string FreightUom { get; set; }
        public UnitOfMeasureBaseEnum? FreightUomBaseId { get; set; }
        public bool UseTruckCapacityForLoadQuantity { get; set; }
        public decimal? LoadMaterialQuantity { get; set; }
        public int? MaterialItemId { get; set; }


        public class OrderLineTruckSelectListItem
        {
            public string Text { get; set; }
            public string Value { get; set; }
            public bool Selected { get; set; }
            public bool HasContactInfo { get; set; }
            public int OrderLineTruckId { get; set; }
        }
    }
}
