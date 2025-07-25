using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Quotes.Dto
{
    public class CreateQuoteFromOrderInput
    {
        public int OrderId { get; set; }

        [Required]
        public string QuoteName { get; set; }
    }
}
