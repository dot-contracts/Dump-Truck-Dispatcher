using System;

namespace DispatcherWeb.Dashboard.Dto
{
    public class RevenueChartsDataDto
    {
        public TicketType RequestedTicketType { get; set; }
        public bool IsGPSConfigured { get; set; }
        public decimal FuelCostValue { get; set; }
        public decimal AvgFuelCostPerMileValue { get; set; }
        public decimal AvgRevenuePerHourValue { get; set; }
        public decimal AvgRevenuePerMileValue { get; set; }
        public decimal AvgAdjustedRevnuePerTruckValue { get; set; }

        #region Avg Revenue

        public decimal FuelSurchargeValue { get; set; }
        public decimal InternalTrucksFuelSurchargeValue { get; set; }
        public decimal LeaseHaulersFuelSurchargeValue { get; set; }
        public decimal FreightRevenueValue { get; set; }
        public decimal MaterialRevenueValue { get; set; }
        public decimal TotalRevenue { get { return FuelSurchargeValue + FreightRevenueValue + MaterialRevenueValue; } }
        public decimal FuelSurchargePercent
        {
            get
            {
                if (TotalRevenue == 0)
                {
                    return 0;
                }

                if (FreightRevenueValue == 0 && MaterialRevenueValue == 0)
                {
                    return 100;
                }

                return Math.Round(FuelSurchargeValue / TotalRevenue, 2) * 100;
            }
        }
        public decimal FreightRevenuePercent
        {
            get
            {
                if (TotalRevenue == 0)
                {
                    return 0;
                }

                if (FuelSurchargeValue == 0 && MaterialRevenueValue == 0)
                {
                    return 100;
                }

                return Math.Round(FreightRevenueValue / TotalRevenue, 2) * 100;
            }
        }
        public decimal MaterialRevenuePercent
        {
            get
            {
                if (TotalRevenue == 0)
                {
                    return 0;
                }

                if (FuelSurchargeValue == 0 && FreightRevenueValue == 0)
                {
                    return 100;
                }

                return Math.Round(MaterialRevenueValue / TotalRevenue, 2) * 100;
            }
        }

        #endregion

        #region Avg Revenue/Truck

        public decimal FuelSurchargePerTruckValue { get; set; }
        public decimal FreightRevenuePerTruckValue { get; set; }
        public decimal MaterialRevenuePerTruckValue { get; set; }
        public decimal TotalRevenuePerTruck { get { return FuelSurchargePerTruckValue + FreightRevenuePerTruckValue + MaterialRevenuePerTruckValue; } }
        public decimal FuelSurchargePerTruckPercent
        {
            get
            {
                if (TotalRevenuePerTruck == 0)
                {
                    return 0;
                }

                if (FreightRevenuePerTruckValue == 0 && MaterialRevenuePerTruckValue == 0)
                {
                    return 100;
                }

                return Math.Round(FuelSurchargePerTruckValue / TotalRevenuePerTruck, 2) * 100;
            }
        }
        public decimal FreightRevenuePerTruckPercent
        {
            get
            {
                if (TotalRevenuePerTruck == 0)
                {
                    return 0;
                }

                if (FuelSurchargePerTruckValue == 0 && MaterialRevenuePerTruckValue == 0)
                {
                    return 100;
                }

                return Math.Round(FreightRevenuePerTruckValue / TotalRevenuePerTruck, 2) * 100;
            }
        }
        public decimal MaterialRevenuePerTruckPercent
        {
            get
            {
                if (TotalRevenuePerTruck == 0)
                {
                    return 0;
                }

                if (FuelSurchargePerTruckValue == 0 && FreightRevenuePerTruckValue == 0)
                {
                    return 100;
                }

                return Math.Round(MaterialRevenuePerTruckValue / TotalRevenuePerTruck, 2) * 100;
            }
        }

        #endregion

        #region Adjusted Revenue

        public decimal ProductionPayValue { get; set; }
        public decimal HourlyPayValue { get; set; }
        public decimal LeaseHaulerPaymentValue { get; set; }
        public decimal AdjustedRevenueValue
        {
            get
            {
                var result = FreightRevenueValue + MaterialRevenueValue;

                if (RequestedTicketType != TicketType.LeaseHaulers) //Internal Trucks or Both
                {
                    result += InternalTrucksFuelSurchargeValue - FuelCostValue - ProductionPayValue - HourlyPayValue;
                }
                if (RequestedTicketType != TicketType.InternalTrucks) //Lease Haulers or Both
                {
                    result -= LeaseHaulerPaymentValue;
                }

                return result;
            }
        }

        #endregion
    }
}
