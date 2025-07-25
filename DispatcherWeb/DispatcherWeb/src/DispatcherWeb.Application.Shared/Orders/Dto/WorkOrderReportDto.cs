using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DispatcherWeb.Orders.TaxDetails;

namespace DispatcherWeb.Orders.Dto
{
    public class WorkOrderReportDto : IOrderTaxDetails
    {
        public byte[] LogoBytes { get; set; }
        public bool HidePrices { get; set; }
        public bool SplitRateColumn { get; set; }
        public bool ShowPaymentStatus { get; set; }
        public bool ShowSpectrumNumber { get; set; }
        public bool ShowOfficeName { get; set; }
        public bool UseActualAmount { get; set; }
        public bool UseReceipts { get; set; }
        public bool ShowDeliveryInfo { get; set; }
        public bool IncludeTickets { get; set; }
        public string CustomerAccountNumber { get; set; }
        public DateTime? OrderDeliveryDate { get; set; }
        public bool OrderIsPending { get; set; }
        public int? OfficeId { get; set; }
        public string OfficeName { get; set; }
        public string CustomerName { get; set; }
        public string ContactFullDetails => string.Join("     ", (new[] { ContactName, ContactPhoneNumber, ContactEmail }).Where(x => !string.IsNullOrWhiteSpace(x)));
        public string ContactName { get; set; }
        public string ContactPhoneNumber { get; set; }
        public string ContactEmail { get; set; }
        public string PoNumber { get; set; }
        public string SpectrumNumber { get; set; }
        public string SpectrumNumberLabel { get; set; }
        public decimal MaterialTotal { get; set; }
        public decimal FreightTotal { get; set; }
        public decimal SalesTaxRate { get; set; }
        public decimal SalesTax { get; set; }
        public decimal CodTotal { get; set; }
        decimal IOrderTaxDetails.CODTotal { get => CodTotal; set => CodTotal = value; }
        public string ChargeTo { get; set; }
        public List<TruckDriverDto> AllTrucksNonDistinct { get; set; }
        public List<TruckDriverDto> GetAllTrucks()
        {
            var result = new List<TruckDriverDto>();

            foreach (var truck in AllTrucksNonDistinct)
            {
                if (result.Any(r => r.TruckId == truck.TruckId && r.DriverName == truck.DriverName))
                {
                    continue;
                }
                result.Add(truck);
            }

            return result;
        }

        public List<TruckDriverDto> GetLeasedTrucks()
        {
            return GetAllTrucks().Where(x => x.IsPowered && x.IsLeased).ToList();
        }

        public List<TruckDriverDto> GetNonLeasedTrucks()
        {
            return GetAllTrucks().Where(x => x.IsPowered && !x.IsLeased).ToList();
        }

        public string Directions { get; set; }
        public DateTime? AuthorizationDateTime { get; set; }
        public DateTime? AuthorizationCaptureDateTime { get; set; }
        public decimal? AuthorizationCaptureSettlementAmount { get; set; }
        public string AuthorizationCaptureTransactionId { get; set; }
        public string TimeZone { get; set; }
        public List<WorkOrderReportItemDto> Items { get; set; }
        public List<WorkOrderReportDeliveryInfoDto> DeliveryInfoItems { get; set; }
        public int Id { get; set; }
        public Shift? OrderShift { get; set; }
        public string OrderShiftName { get; set; }
        public bool ShowDriverNamesOnPrintedOrder { get; set; }
        public bool ShowLoadAtOnPrintedOrder { get; set; }
        public bool ShowSignatureColumn { get; set; }
        public bool ShowTruckCategories { get; set; }
        public bool IncludeTravelTime { get; set; }
        public CultureInfo CurrencyCulture { get; set; }
        public bool DebugLayout { get; set; }

        public class TruckDriverDto
        {
            public int TruckId { get; set; }
            public string TruckCode { get; set; }
            public string DriverName { get; set; }
            public AssetType AssetType { get; set; }
            public bool IsLeased { get; set; }
            public bool IsPowered { get; set; }

            public override string ToString()
            {
                return TruckCode + (DriverName != null ? " - " + DriverName : "");
            }
        }
    }
}
