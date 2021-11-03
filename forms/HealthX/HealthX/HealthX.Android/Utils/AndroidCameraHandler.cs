using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using HealthX.Droid.Utils;
using HealthX.Utils;

using Xamarin.Forms;
using Plugin.CurrentActivity;
using Android.Graphics;
using Java.Nio;
using System.Runtime.InteropServices;

[assembly: Dependency (typeof(AndroidCameraHandler))]
namespace HealthX.Droid.Utils {

    public class AndroidCameraHandler : ICameraHandler {

        // Stores instance of the current activity
        Activity current = CrossCurrentActivity.Current.Activity;
        // The file where we'll save the picture
        public static Java.IO.File _file;

        // The Status
        public static int Status { get; set; }

        // Completion action
        static Action<int, byte[]> CompletionAction;


        public void TakePicture() {

            // Reset static variables
            _file = null;

            // Catch this in PCL.
            if (!IsCameraAvailable()) throw new ArgumentException("No Camera Available");

            // Take picture...
            Intent intent = new Intent(MediaStore.ActionImageCapture);

            _file = new Java.IO.File(MainActivity._dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_file));

            current.StartActivityForResult(intent, 12);

        }

        public static void OnPictureTaken (Result resultcode, Intent data) {

            if(resultcode == Result.Canceled) {
                Status = 1;
                CompletionAction(Status, null);
                return;
            }

            CompletionAction(Status, File.ReadAllBytes(_file.Path));

        }

        // Checks if the camera is available
        public bool IsCameraAvailable () {

            Intent intent = new Intent(MediaStore.ActionImageCapture);
            
            IList<ResolveInfo> availableActivities =
                 current.PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;

        }

        public void SetCompletionAction(Action<int, byte[]> action) {
            CompletionAction = action;
        }
    }

}