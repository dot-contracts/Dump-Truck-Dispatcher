namespace DispatcherWeb.Tickets
{
    public static class TicketExtensions
    {
        public static decimal? GetFreightQuantity(this ITicketQuantity ticket)
        {
            var (materialQuantity, freightQuantity) = GetMaterialAndFreightQuantity(ticket);
            return freightQuantity;
        }

        public static decimal? GetMaterialQuantity(this ITicketQuantity ticket)
        {
            var (materialQuantity, freightQuantity) = GetMaterialAndFreightQuantity(ticket);
            return materialQuantity;
        }

        public static (decimal? materialQuantity, decimal? freightQuantity) GetMaterialAndFreightQuantity(this ITicketQuantity ticket)
        {
            //var fallback = ticket.GetMaterialAndFreightQuantityForSingleItem();
            //return (ticket.MaterialQuantity ?? fallback.materialQuantity, ticket.FreightQuantity ?? fallback.freightQuantity);
            return (ticket.MaterialQuantity ?? 0, ticket.FreightQuantity ?? 0);
        }

        //private static (decimal? materialQuantity, decimal? freightQuantity) GetMaterialAndFreightQuantityForSingleItem(this ITicketQuantity ticket)
        //{
        //    if (ticket.Designation.MaterialOnly())
        //    {
        //        return (ticket.Quantity, null); //quantity is material
        //    }
        //    if (ticket.Designation.FreightAndMaterial())
        //    {
        //        if (ticket.OrderLineMaterialUomId == ticket.OrderLineFreightUomId)
        //        {
        //            return (ticket.Quantity, ticket.Quantity); //quantity is material and freight
        //        }

        //        if (ticket.TicketUomId == ticket.OrderLineMaterialUomId)
        //        {
        //            return (ticket.Quantity, null); //quantity is material
        //        }

        //        if (ticket.TicketUomId == ticket.OrderLineFreightUomId)
        //        {
        //            return (null, ticket.Quantity); //quantity is freight
        //        }

        //        //fallback to the previous version of logic:
        //        return (ticket.Quantity, null); //quantity is material
        //    }
        //    return (null, ticket.Quantity); //quantity is freight
        //}

        public static (bool useMaterial, bool useFreight) GetAmountTypeToUse(this ITicketQuantity ticket)
        {
            if (ticket.Designation.MaterialOnly())
            {
                return (true, false); //quantity is material
            }
            if (ticket.Designation.FreightAndMaterial())
            {
                if (ticket.OrderLineMaterialUomId == ticket.OrderLineFreightUomId)
                {
                    return (true, true); //quantity is material and freight
                }

                if (ticket.TicketUomId == ticket.OrderLineMaterialUomId)
                {
                    return (true, false); //quantity is material
                }

                if (ticket.TicketUomId == ticket.OrderLineFreightUomId)
                {
                    return (false, true); //quantity is freight
                }

                return (true, false); //quantity is material
            }
            return (false, true); //quantity is freight
        }
    }
}
