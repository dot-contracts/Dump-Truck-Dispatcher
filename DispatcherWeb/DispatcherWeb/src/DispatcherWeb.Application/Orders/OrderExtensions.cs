namespace DispatcherWeb.Orders
{
    public static class OrderExtensions
    {
        public static Order CreateCopy(this Order order)
        {
            return new Order
            {
                CODTotal = order.CODTotal,
                ContactId = order.ContactId,
                ChargeTo = order.ChargeTo,
                CustomerId = order.CustomerId,
                DeliveryDate = order.DeliveryDate,
                Shift = order.Shift,
                IsPending = order.IsPending,
                Directions = order.Directions,
                FreightTotal = order.FreightTotal,
                IsClosed = order.IsClosed,
                OfficeId = order.OfficeId,
                MaterialTotal = order.MaterialTotal,
                PONumber = order.PONumber,
                SpectrumNumber = order.SpectrumNumber,
                QuoteId = order.QuoteId,
                IsTaxExempt = order.IsTaxExempt,
                SalesTax = order.SalesTax,
                SalesTaxRate = order.SalesTaxRate,
                SalesTaxEntityId = order.SalesTaxEntityId,
                Priority = order.Priority,
                EncryptedInternalNotes = order.EncryptedInternalNotes,
                FuelSurchargeCalculationId = order.FuelSurchargeCalculationId,
                BaseFuelCost = order.BaseFuelCost,
            };

        }
    }
}
