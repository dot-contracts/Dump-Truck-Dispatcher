using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Locations.Dto
{
    public class LocationContactEditDto
    {
        public int? Id { get; set; }

        [Required]
        public int LocationId { get; set; }

        [Required(ErrorMessage = "Name is a required field")]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(120)]
        public string Email { get; set; }

        [StringLength(40)]
        public string Title { get; set; }
    }
}
