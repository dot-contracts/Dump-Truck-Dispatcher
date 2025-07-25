using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.BackgroundJobs
{
    [Table("BackgroundJobHistory")]
    public class BackgroundJobHistory : Entity
    {
        public BackgroundJobEnum Job { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Completed { get; set; }
        [StringLength(EntityStringFieldLengths.BackgroundJobHistory.Details)]
        public string Details { get; set; }
    }
}
