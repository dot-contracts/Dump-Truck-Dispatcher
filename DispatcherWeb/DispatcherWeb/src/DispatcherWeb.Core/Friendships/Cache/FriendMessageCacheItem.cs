using System;
using DispatcherWeb.Chat;

namespace DispatcherWeb.Friendships.Cache
{
    public class FriendMessageCacheItem
    {
        public long Id { get; set; }
        public string Message { get; set; }
        public DateTime CreationTime { get; set; }
        public ChatSide Side { get; set; }
        public ChatMessageReadState ReadState { get; set; }
        public ChatMessageReadState ReceiverReadState { get; set; }
    }
}
