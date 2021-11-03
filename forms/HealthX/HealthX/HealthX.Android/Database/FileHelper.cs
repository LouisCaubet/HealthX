using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using HealthX.Backend;
using HealthX.Droid.Database;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileHelper))]
namespace HealthX.Droid.Database {

    public class FileHelper : IFileHelper {

        public bool DatabaseExists { get; set; }

        public string GetLocalFilePath(string filename) {

            string docFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            // DEBUG delete file
            // File.Delete(Path.Combine(docFolder, filename));

            if (!File.Exists(Path.Combine(docFolder, filename))) {
                DatabaseExists = false;
            }
            else {
                DatabaseExists = true;
            }

            return Path.Combine(docFolder, filename);

        }

    }

}