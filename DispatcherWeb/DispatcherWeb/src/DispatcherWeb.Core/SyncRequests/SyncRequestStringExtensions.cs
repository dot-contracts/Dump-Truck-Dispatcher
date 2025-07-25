namespace DispatcherWeb.SyncRequests
{
    public static class SyncRequestStringExtensions
    {
        public static T SetSyncRequestString<T>(this T input, SyncRequest syncRequest) where T : IHaveSyncRequestString
        {
            input.SyncRequestString = Utilities.SerializeWithTypes(syncRequest);
            return input;
        }

        public static SyncRequest GetSyncRequest(this IHaveSyncRequestString input)
        {
            return Utilities.DeserializeWithTypes<SyncRequest>(input.SyncRequestString);
        }
    }
}
