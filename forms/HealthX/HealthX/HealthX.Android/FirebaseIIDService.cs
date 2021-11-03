using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Firebase.Iid;

namespace HealthX.Droid {

    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class FirebaseIIDService : FirebaseInstanceIdService {

        const string TAG = "HealthXFirebaseIIDService";
        public override void OnTokenRefresh() {
            try {
                var refreshedToken = FirebaseInstanceId.Instance.Token;
                Log.Debug(TAG, "Refreshed token: " + refreshedToken);
                SendRegistrationToServer(refreshedToken);
            }
            catch (Exception e) {
                Log.Debug(TAG, e.Message + "\n" + e.StackTrace);
            }
        }

        void SendRegistrationToServer(string token) {
            // TODO link this to user account (PCL)
        }

    }

}