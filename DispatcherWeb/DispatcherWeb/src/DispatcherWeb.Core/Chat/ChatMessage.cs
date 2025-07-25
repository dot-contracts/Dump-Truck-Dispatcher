using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.Chat
{
    [Table("AppChatMessages")]
    public class ChatMessage : Entity<long>, IHasCreationTime, IMayHaveTenant
    {
        public long UserId { get; set; }

        public int? TenantId { get; set; }

        public long TargetUserId { get; set; }

        public int? TargetTenantId { get; set; }

        public int? TargetDriverId { get; set; }

        public virtual Driver TargetDriver { get; set; }

        public int? TargetTruckId { get; set; }

        public virtual Truck TargetTruck { get; set; }

        public int? TargetTrailerId { get; set; }

        public virtual Truck TargetTrailer { get; set; }

        [Required]
        [StringLength(EntityStringFieldLengths.ChatMessage.Message)]
        public string Message { get; set; }

        public DateTime CreationTime { get; set; }

        public ChatSide Side { get; set; }

        public ChatMessageReadState ReadState { get; private set; }

        public ChatMessageReadState ReceiverReadState { get; private set; }

        public Guid? SharedMessageId { get; set; }

        public ChatMessage(
            UserIdentifier user,
            UserIdentifier targetUser,
            ChatSide side,
            string message,
            ChatMessageReadState readState,
            Guid sharedMessageId,
            ChatMessageReadState receiverReadState)
        {
            UserId = user.UserId;
            TenantId = user.TenantId;
            TargetUserId = targetUser.UserId;
            TargetTenantId = targetUser.TenantId;
            Message = message;
            Side = side;
            ReadState = readState;
            SharedMessageId = sharedMessageId;
            ReceiverReadState = receiverReadState;

            CreationTime = Clock.Now;
        }

        public void ChangeReadState(ChatMessageReadState newState)
        {
            ReadState = newState;
        }

        protected ChatMessage()
        {

        }

        public void ChangeReceiverReadState(ChatMessageReadState newState)
        {
            ReceiverReadState = newState;
        }
    }
}
