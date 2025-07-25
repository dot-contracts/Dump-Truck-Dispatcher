using System;
using DispatcherWeb.FuelPurchases.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.FuelPurchases
{
    public class CreateOrEditFuelPurchaseModalViewModel
    {
        public int Id { get; set; }
        public int TruckId { get; set; }
        public DateTime FuelDateTime { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Odometer { get; set; }

        public int? OfficeId { get; set; }
        public string TruckCode { get; set; }

        public string TicketNumber { get; set; }

        public static CreateOrEditFuelPurchaseModalViewModel CreateFromFuelPurchaseEditDto(FuelPurchaseEditDto dto)
        {
            return new CreateOrEditFuelPurchaseModalViewModel
            {
                Id = dto.Id,
                TruckId = dto.TruckId,
                TruckCode = dto.TruckCode,
                FuelDateTime = dto.FuelDateTime,
                Amount = dto.Amount,
                Rate = dto.Rate,
                Odometer = dto.Odometer,
                TicketNumber = dto.TicketNumber,
            };
        }
    }
}
