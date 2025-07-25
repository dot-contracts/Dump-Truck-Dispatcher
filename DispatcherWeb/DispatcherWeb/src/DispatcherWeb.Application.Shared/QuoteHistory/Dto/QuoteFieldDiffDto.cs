namespace DispatcherWeb.QuoteHistory.Dto
{
    public class QuoteFieldDiffDto
    {
        public int Id { get; set; }

        public QuoteFieldEnum Field { get; set; }
        public string FieldName => Field.GetDisplayName();

        public int? OldId { get; set; }

        public int? NewId { get; set; }

        public string OldDisplayValue { get; set; }

        public string NewDisplayValue { get; set; }
    }
}
