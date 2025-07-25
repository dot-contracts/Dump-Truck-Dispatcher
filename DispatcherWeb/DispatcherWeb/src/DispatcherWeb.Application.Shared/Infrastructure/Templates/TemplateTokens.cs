namespace DispatcherWeb.Infrastructure.Templates
{
    public static class TemplateTokens
    {
        public const string TruckCode = "{TruckCode}";
        public const string DeliveryDate = "{DeliveryDate}";
        public const string Shift = "{Shift}";
        public const string OrderNumber = "{OrderNumber}";
        public const string Customer = "{Customer}";
        public const string Supplier = "{Supplier}";
        public const string LoadAt = "{LoadAt}";
        public const string Directions = "{Directions}"; //backward compatibility
        public const string Comments = "{Comments}";
        public const string Note = "{Note}";
        public const string TimeOnJob = "{TimeOnJob}";
        public const string StartTime = "{StartTime}";
        public const string Item = "{Item}";
        public const string Quantity = "{Quantity}";
        public const string MaterialQuantity = "{MaterialQuantity}";
        public const string FreightQuantity = "{FreightQuantity}";
        public const string Uom = "{Uom}";
        public const string MaterialUom = "{MaterialUom}";
        public const string FreightUom = "{FreightUom}";
        public const string UserFirstName = "{User.FirstName}";
        public const string UserLastName = "{User.LastName}";
        public const string UserPhoneNumber = "{User.PhoneNumber}";
        public const string CompanyName = "{CompanyName}";
        public const string DeliverTo = "{DeliverTo}";
        public const string ChargeTo = "{ChargeTo}";
    }
}
