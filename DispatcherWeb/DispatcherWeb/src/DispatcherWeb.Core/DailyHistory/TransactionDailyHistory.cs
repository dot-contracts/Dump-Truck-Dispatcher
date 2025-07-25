using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;

namespace DispatcherWeb.DailyHistory
{
    public class TransactionDailyHistory : Entity
    {
        public DateTime Date { get; set; }

        [StringLength(256)]
        [Required]
        public string ServiceName { get; set; }

        [StringLength(256)]
        [Required]
        public string MethodName { get; set; }

        public int NumberOfTransactions { get; set; }
        public int AverageExecutionDuration { get; set; }

    }
}
