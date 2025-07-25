using System;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Trucks.Dto
{
    public class TruckFileEditDto
    {
        public int Id { get; set; }
        public int TruckId { get; set; }

        [StringLength(50)]
        public string Title { get; set; }
        [StringLength(1000)]
        public string Description { get; set; }

        public Guid FileId { get; set; }
        public Guid? ThumbnailId { get; set; }

        [StringLength(500)]
        public string FileName { get; set; }

        public FileType FileType { get; set; }

    }
}
