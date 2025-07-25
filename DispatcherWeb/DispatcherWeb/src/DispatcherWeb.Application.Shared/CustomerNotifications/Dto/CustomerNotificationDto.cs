using System;
using System.Collections.Generic;
using System.Linq;

namespace DispatcherWeb.CustomerNotifications.Dto
{
    public class CustomerNotificationDto
    {
        public const int MaxLengthOfBodyAndTitle = 50;

        public int Id { get; set; }

        public string CreatedByUserFullName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public List<string> EditionNames { get; set; }
        public string EditionNamesFormatted => string.Join(", ", EditionNames.OrderBy(x => x));

        public HostEmailType Type { get; set; }
        public string TypeFormatted => Type.GetDisplayName();
    }
}
