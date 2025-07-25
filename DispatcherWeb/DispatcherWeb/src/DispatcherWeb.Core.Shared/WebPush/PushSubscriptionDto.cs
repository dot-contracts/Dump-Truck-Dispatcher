namespace DispatcherWeb.WebPush
{
    public class PushSubscriptionDto
    {
        public PushSubscriptionDto()
        {
        }

        public PushSubscriptionDto(string pushEndpoint, string p256dh, string auth)
        {
            Endpoint = pushEndpoint;
            Keys = new PushSubscriptionKeys
            {
                P256dh = p256dh,
                Auth = auth,
            };
        }

        public string Endpoint { get; set; }

        public PushSubscriptionKeys Keys { get; set; }

        public class PushSubscriptionKeys
        {
            public string P256dh { get; set; }
            public string Auth { get; set; }
        }
    }
}
