using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Configuration;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.WebPush
{
    public class FirebasePushSender : IFirebasePushSender, ISingletonDependency
    {
        private readonly IConfigurationRoot _appConfiguration;
        private readonly ConcurrentDictionary<string, FirebaseApp> _firebaseApps = new();

        public FirebasePushSender(IAppConfigurationAccessor configurationAccessor)
        {
            _appConfiguration = configurationAccessor.Configuration;
        }

        private FirebaseMessaging GetFirebaseMessaging(int? version)
        {
            var firebaseApp = _firebaseApps.GetOrAdd(version.ToString(), v =>
            {
                var jsonCredential = _appConfiguration[$"WebPush:FirebaseCredential{v}"];
                if (string.IsNullOrEmpty(jsonCredential))
                {
                    return null;
                }
                var credential = GoogleCredential.FromJson(jsonCredential);

                return FirebaseApp.Create(new AppOptions
                {
                    Credential = credential,
                });
            });

            if (firebaseApp == null)
            {
                return null;
            }

            return FirebaseMessaging.GetMessaging(firebaseApp);
        }

        public async Task SendAsync(FcmRegistrationTokenDto registrationToken, string jsonPayload) //jsonPayload should be a serialized instance of DriverApplication.FcmPushMessage
        {
            var messaging = GetFirebaseMessaging(registrationToken.Version);

            if (messaging == null)
            {
                //do not throw the exception, just silently skip these messages
                //this will happen in cases when we have to discontinue the use of one of the keys
                return;
            }

            await messaging.SendAsync(new Message
            {
                Token = registrationToken.Token,
                Data = new Dictionary<string, string>
                {
                    { "jsonPayload", jsonPayload },
                },
            });
        }
    }
}
