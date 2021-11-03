using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HealthX.Server.Utils { 

    public class FCMUtils {

        static string Scope = "https://www.googleapis.com/auth/firebase.messaging";
        static HttpClient Client;

        public static void Init() {
            Client = new HttpClient {
                BaseAddress = new Uri("https://fcm.googleapis.com/fcm/send")
            };
        }

        public static async Task<string> GetAccessToken () {

            ServiceAccountCredential service = ServiceAccountCredential.FromServiceAccountData(new FileStream("service-account.json", FileMode.Open, FileAccess.Read));
            await service.RequestAccessTokenAsync(new System.Threading.CancellationToken());
            return service.Token.AccessToken;

        }

        public static void SendNotificationTo(string device_token) {

        }

    }
}
