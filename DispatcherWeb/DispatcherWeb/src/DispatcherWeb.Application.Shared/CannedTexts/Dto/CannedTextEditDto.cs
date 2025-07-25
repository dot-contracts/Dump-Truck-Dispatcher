using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Offices.Dto;

namespace DispatcherWeb.CannedTexts.Dto
{
    public class CannedTextEditDto : IOfficeIdNameDto
    {
        public int? Id { get; set; }

        public int OfficeId { get; set; }

        public string OfficeName { get; set; }
        public bool IsSingleOffice { get; set; }

        [Required(ErrorMessage = "Name is a required field")]
        [StringLength(100)]
        public string Name { get; set; }

        public string Text { get; set; }

    }
}
