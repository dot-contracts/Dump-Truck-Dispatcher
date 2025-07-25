using System;
using DispatcherWeb.SyncRequests.Entities;
using DispatcherWeb.SyncRequests.FcmPushMessages.ChangeDetails;

namespace DispatcherWeb.SyncRequests.DriverApp
{
    public class ChatMessageChangeDetailsConverter : GenericChangeDetailsConverter
    {
        public override FcmEntityChangeDetailsDto GetChangeDetails(IChangedDriverAppEntity changedEntity, ChangeType changeType)
        {
            if (changedEntity is ChangedChatMessage changedChatMessage)
            {
                return new FcmChatMessageDetailsDto
                {
                    Id = changedChatMessage.Id,
                    LastUpdateDateTime = changedChatMessage.LastUpdateDateTime,
                    TargetUserId = changedChatMessage.TargetUserId,
                    ChangeType = changeType,
                };
            }
            throw new NotImplementedException("Only ChangedChatMessage entities are expected");
        }
    }
}
