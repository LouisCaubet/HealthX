using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Android.Content;

using Refractored.XamForms.PullToRefresh.Droid;

using Firebase.Messaging;
using Firebase.Iid;
using Android.Util;
using Android.Gms.Common;

namespace HealthX.Droid {

    /// <summary>
    /// Main Activity of Android App. Implementing default android methods like onCreate, etc.
    /// </summary>
    [Activity(Label = "Health Plus", Icon = "@drawable/HealthPlusAppLogo", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {

        /// <summary>
        /// The App's directory.
        /// </summary>
        public static Java.IO.File _dir;

        /// <summary>
        /// If the Google Play Services are available
        /// </summary>
        public static bool IsPlayAvailable;

        /// <summary>
        /// Called on application start.
        /// </summary>
        /// <param name="bundle"></param>
        protected override void OnCreate(Bundle bundle) {

            // Get path of application directory.
            _dir = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(
            Android.OS.Environment.DirectoryPictures), "HealthPlus");

            // make sure application's directory exists.
            if (!_dir.Exists()) _dir.Mkdirs();

            // default xamarin call
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            // Required for the Pull to Refresh plugin.
            PullToRefreshLayoutRenderer.Init();

            // default xamarin call
            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());

            // init xamarin forms maps.
            Xamarin.FormsMaps.Init(this, bundle);
            // Required for the GPS plugin.
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity = this;

            // Check Availability of Google Play Services
            IsPlayServicesAvailable();

        }

        // Required for the GPS plugin
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults) {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        // Called when external activity completed. Calls the specific handler according to requestCode.
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
            if (requestCode == 42) { // Code for the Braintree Drop-In UI
                Braintree.BraintreeUI.OnDropInResult(this, resultCode, data);
            }
            else if (requestCode == 12) { // Code for the camera.
                base.OnActivityResult(requestCode, resultCode, data);
                Utils.AndroidCameraHandler.OnPictureTaken(resultCode, data);
            }
            else base.OnActivityResult(requestCode, resultCode, data); // default to base.
        }

        /// <summary>
        /// Checks if the Google Play Services are available on this device.
        /// </summary>
        /// <returns>if the service is available.</returns>
        public bool IsPlayServicesAvailable() {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success) {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                    IsPlayAvailable = false;
                else {
                    IsPlayAvailable = false;
                }
                return false;
            }
            else {
                IsPlayAvailable = true;
                return true;
            }
        }

    }
}

