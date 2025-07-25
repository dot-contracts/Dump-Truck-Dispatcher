namespace DispatcherWeb.QuickbooksOnline.Dto
{
    public class InvoiceLineItemToUploadDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsInQuickBooks { get; set; }
        public ItemType? Type { get; set; }
        public string Description { get; set; }
        public string IncomeAccount { get; set; }

        public InvoiceLineItemToUploadDto Clone()
        {
            return new InvoiceLineItemToUploadDto
            {
                Id = Id,
                Name = Name,
                IsInQuickBooks = IsInQuickBooks,
                Type = Type,
                Description = Description,
                IncomeAccount = IncomeAccount,
            };
        }
    }
}
