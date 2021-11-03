using HealthX.Backend;
using HealthX.Datatypes;
using HealthX.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;

using Xamarin.Forms;
using System.Globalization;

namespace HealthX {
    public partial class App : Application {

        public static App INSTANCE;

        public static bool DEBUG = true;
        public static bool NO_SERVER = false;

        public static RestApiClient API;
        public static LocalDatabase LocalDB;

        public static string BraintreeClientToken;
        public static string Username; // TODO replace this with account login. 
        public static string Password;
        public static Pharmacy HomePharmacy;

        public App() {
            InitializeComponent();

            INSTANCE = this;

            // Launch API Client
            Debug.WriteLine("Starting API initialization");
            API = new RestApiClient();
            Debug.WriteLine("API initialized!");

            BraintreeClientToken = null;

            Username = "Louis Caubet";
            Password = "admin";


            // Load localization resources
            var ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
            // AppResources.Culture = ci; // set the RESX for resource localization
            AppResources.Culture = new CultureInfo("fr-FR"); 
            DependencyService.Get<ILocalize>().SetLocale(new CultureInfo("fr-FR")); // set the Thread for locale-aware methods

            MainPage = new MainPage();


            // Load local database
            Device.StartTimer(TimeSpan.FromMilliseconds(300), () => {
                LocalDB = new LocalDatabase();
                SetHomePharmacy(6);
                return false;
            });
            
        }

        private async void SetHomePharmacy (int id) {
            Debug.WriteLine("[INFO] Starting PharmacyQuery to get Home Pharmacy");
            var pharmacies = await API.QueryPharmacyAsync("Id=" + id);
            Debug.WriteLine("[INFO] Query completed. Home pharmacy set");
            if (pharmacies.Length == 0) throw new ArgumentException("No pharmacy with Id of home pharmacy");
            HomePharmacy = pharmacies[0];
        }

        protected override void OnStart() {
            // Handle when your app starts
        }

        protected override void OnSleep() {
            // Handle when your app sleeps
        }

        protected override void OnResume() {
            // Handle when your app resumes
        }
    }
}
