using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace DispatcherWeb.Emailing
{
    public static class EmailHelper
    {
        public static string[] SplitEmailAddresses(string source)
        {
            if (source == null)
            {
                return Array.Empty<string>();
            }

            return source.Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();
        }

        public static List<TrackableEmailReceiver> GetTrackableEmailReceivers(this MailMessage mail, Guid trackableEmailId)
        {
            var receivers = mail.To.Select(x => x.Address).Distinct()
                .Select((x, i) => new TrackableEmailReceiver
                {
                    TrackableEmailId = trackableEmailId,
                    Email = x,
                    Order = i,
                    ReceiverKind = EmailReceiverKind.To,
                    IsSender = false, /* IsSender==true is filtered out in the grid but we want it to be shown in this case */ /*x == mail.From.Address*/
                }).ToList();

            var cc = mail.CC.Select(x => x.Address).Distinct()
                .Where(x => receivers.All(r => r.Email != x))
                .Select((x, i) => new TrackableEmailReceiver
                {
                    TrackableEmailId = trackableEmailId,
                    Email = x,
                    Order = receivers.Count + i,
                    ReceiverKind = EmailReceiverKind.Cc,
                    IsSender = x == mail.From?.Address,
                }).ToList();
            receivers.AddRange(cc);

            var bcc = mail.Bcc.Select(x => x.Address).Distinct()
                .Where(x => receivers.All(r => r.Email != x))
                .Select((x, i) => new TrackableEmailReceiver
                {
                    TrackableEmailId = trackableEmailId,
                    Email = x,
                    Order = receivers.Count + i,
                    ReceiverKind = EmailReceiverKind.Bcc,
                    IsSender = x == mail.From?.Address,
                }).ToList();
            receivers.AddRange(bcc);

            receivers.ForEach(x =>
            {
                x.TruncateFieldsIfNeeded();
            });

            return receivers;
        }

        public static List<TrackableEmailReceiver> SetTenantId(this List<TrackableEmailReceiver> list, int? tenantId)
        {
            list.ForEach(x => x.TenantId = tenantId);
            return list;
        }
    }
}
