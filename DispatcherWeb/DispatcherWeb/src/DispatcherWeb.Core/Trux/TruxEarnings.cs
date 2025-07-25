using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Trux
{
    [Table("TruxEarnings")]
    public class TruxEarnings : FullAuditedEntity, IMustHaveTenant
    {
        //Shift/Assignment – primary key, unique constraint

        public int TenantId { get; set; }


        public int JobId { get; set; }

        [StringLength(EntityStringFieldLengths.TruxEarnings.JobName)]
        public string JobName { get; set; }

        public DateTime StartDateTime { get; set; }

        [StringLength(EntityStringFieldLengths.TruxEarnings.TruckType)]
        public string TruckType { get; set; }

        [StringLength(EntityStringFieldLengths.TruxEarnings.Status)]
        public string Status { get; set; }

        [StringLength(EntityStringFieldLengths.TruxEarnings.TruxTruckId)]
        public string TruxTruckId { get; set; }

        [StringLength(EntityStringFieldLengths.TruxEarnings.DriverName)]
        public string DriverName { get; set; }

        [StringLength(EntityStringFieldLengths.TruxEarnings.HaulerName)]
        public string HaulerName { get; set; }

        public DateTime PunchInDatetime { get; set; }

        public DateTime PunchOutDatetime { get; set; }

        public decimal Hours { get; set; }

        public decimal Tons { get; set; }

        public int Loads { get; set; }

        [StringLength(EntityStringFieldLengths.TruxEarnings.Unit)]
        public string Unit { get; set; }

        public decimal Rate { get; set; }

        public decimal Total { get; set; }


        public int BatchId { get; set; }

        public virtual TruxEarningsBatch Batch { get; set; }
    }
}
