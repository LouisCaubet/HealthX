using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

using SQLite;
using HealthX.Datatypes;
using System.Diagnostics;
using HealthX.Utils;

namespace HealthX.Backend {

    public class LocalDatabase {

        // If the database is up-to-date
        public bool UpToDate { get; private set; }


        // SQLite Connection Object. Used for Queries, ...
        SQLiteAsyncConnection connection;

        // Opens connection to database and checks tables are created as required.
        public LocalDatabase () {

            Debug.WriteLine("[INFO] Starting loading of local database");
            IFileHelper helper = DependencyService.Get<IFileHelper>();

            Debug.WriteLine("[INFO] Getting DB File from FileHelper");
            connection = new SQLiteAsyncConnection(helper.GetLocalFilePath("pharmacies.db3"));
            Debug.WriteLine("[INFO] Got DB File from FileHelper");

            //Debug.WriteLine("[INFO] Local database does not exist. Creating...");
            connection.CreateTableAsync<PharmacyItem>().Wait();
            //Debug.WriteLine("[INFO] Local database created.");
            

            UpToDate = false;
            CheckUpdate();
            
        }

        // Called once LocalDB is loaded and updated
        private void LoadingComplete () {
            // This has to be done here to make sure Local BB is loaded
            ((MainPage) App.INSTANCE.MainPage).mapPage.LoadMap();
        }

        /// <summary>
        /// Checks if an update is available for the local database.
        /// </summary>
        public async void CheckUpdate() {

            Debug.WriteLine("[INFO] Checking Local Database is up to date...");

            if (App.NO_SERVER) {
                Update(0);
                return;
            }

            // Ask server for DB versions, then update if not up-to-date
            // Get Last Known ID
            Debug.WriteLine("[DEBUG] Getting Last Known ID from local database");
            List<PharmacyItem> raw = await connection.QueryAsync<PharmacyItem>("SELECT * FROM PharmacyItem ORDER BY Id DESC LIMIT 1");

            // Compute last known id.
            int lastKnownId;
            if (raw.Count == 0) {
                lastKnownId = -1;
            }
            else lastKnownId = raw[0].Id;
                
            // Get last server Id
            Debug.WriteLine("[DEBUG] Getting Last Known ID from Server.");
            int lastServerId = await App.API.GetLastPharmacyId();

            if (lastServerId > lastKnownId) {
                Debug.WriteLine("[INFO] Database is outdated. Updating...");
                Update(lastKnownId);
            }
            else {
                Debug.WriteLine("[INFO] Database is up-to-date!");
                UpToDate = true;
                LoadingComplete();
            }

        }

        /// <summary>
        /// Updates the database by downloading entries higher than last known id. 
        /// </summary>
        /// <param name="lastKnownId">The last id in the local database.</param>
        public async void Update(int lastKnownId) {

            Pharmacy[] newEntries;

            // async ask server for entries newer than last known id, then add to this database
            Debug.WriteLine("[INFO] Getting newest pharmacies from server.");
            newEntries = await App.API.QueryPharmacyAsync("Id > " + lastKnownId);
            Debug.WriteLine("[INFO] Got newest pharmacies from server. Adding them to local database...");
            
            foreach (Pharmacy pharm in newEntries) {
                await connection.InsertAsync(PharmacyItem.FromPharmacy(pharm));
            }

            Debug.WriteLine("[INFO] Newest pharmacies added to local database. DB is now up-to-date!");

            // Database up-to-date
            UpToDate = true;
            LoadingComplete();

        }

        [Obsolete] // only here for compatibility reasons. TODO delete.
        public async Task<Pharmacy[]> GetNearestToLocation(double longitude, double latitude, int numberOfResults, double radius) {
            return await GetNearestToLocation(longitude, latitude, radius);
        }

        /// <summary>
        /// Gets the pharmacies in a radius around specified location, ordered by distance to the location.
        /// </summary>
        /// <param name="longitude">Longitude of location</param>
        /// <param name="latitude">Latitude of location</param>
        /// <param name="radius">Radius of search area.</param>
        /// <returns>An array of pharmacies found in the area, ordered by distance to given location.</returns>
        public async Task<Pharmacy[]> GetNearestToLocation (double longitude, double latitude, double radius) {

            double rad = Math.PI / 180; // to convert degrees to radians

            // left and right borders of search area.
            double long1 = longitude - radius / Math.Abs(Math.Cos(latitude * rad) * 69);
            double long2 = longitude + radius / Math.Abs(Math.Cos(latitude * rad) * 69);

            // top and bottom borders of search area.
            double lat1 = latitude - (radius / 69);
            double lat2 = latitude + (radius / 69);

            Debug.WriteLine("[INFO][LocalDB] Starting query for GetNearestToLocation");

            // get all entries in the area from database.
            List<PharmacyItem> resultRaw = await connection.Table<PharmacyItem>().Where((PharmacyItem item) =>
                item.Latitude >= lat1 && item.Latitude <= lat2 && item.Longitude >= long1 && item.Longitude <= long2
            ).ToListAsync();

            Debug.WriteLine("[INFO][LocalDB] Query for GetNearestToLocation completed");

            resultRaw = resultRaw.OrderBy(delegate (PharmacyItem item) {
                return DistanceUtils.ComputeDistance(lat1, long1, item.Latitude, item.Longitude);
            }).ToList();


            Pharmacy[] result = new Pharmacy[resultRaw.Count];

            for (int i=0; i<resultRaw.Count; i++) {
                result[i] = resultRaw[i].ToPharmacy();
            }

            return result;
            
        }

        /// <summary>
        /// Gets the pharmacies in a radius around given location.
        /// </summary>
        /// <param name="latitude">The Latitude of the location</param>
        /// <param name="longitude">The Longitude of the location</param>
        /// <param name="radius">The radius of the area</param>
        /// <returns>An array containing all pharmacies in the area.</returns>
        public async Task<Pharmacy[]> GetInRadius (double longitude, double latitude, double radius) {

            double rad = Math.PI / 180;

            double long1 = longitude - radius / Math.Abs(Math.Cos(latitude * rad) * 69);
            double long2 = longitude + radius / Math.Abs(Math.Cos(latitude * rad) * 69);

            double lat1 = latitude - (radius / 69);
            double lat2 = latitude + (radius / 69);

            List<PharmacyItem> resultRaw = await connection.Table<PharmacyItem>().Where((PharmacyItem item) =>
                item.Latitude >= lat1 && item.Latitude <= lat2 && item.Longitude >= long1 && item.Longitude <= long2
            ).ToListAsync();

            Pharmacy[] result = new Pharmacy[resultRaw.Count];

            for (int i = 0; i < resultRaw.Count; i++) {
                result[i] = resultRaw[i].ToPharmacy();
            }

            return result;

        }

        /// <summary>
        /// Low-level WHERE query on the Pharmacies table of the Local Database.
        /// </summary>
        /// <param name="where">the SQL WHERE statement.</param>
        /// <returns>An array of Pharmacy representing the result of the query</returns>
        public async Task<Pharmacy[]> GetWhere (string where) {

            List<PharmacyItem> raw = await connection.QueryAsync<PharmacyItem>("SELECT * FROM PharmacyItem WHERE " + where);
            Pharmacy[] result = new Pharmacy[raw.Count];

            for (int i=0; i<raw.Count; i++) {
                result[i] = raw[i].ToPharmacy();
            }

            return result;

        }

    }


    /// <summary>
    /// Represents the Pharmacy Object stored in local database. 
    /// </summary>
    class PharmacyItem {

        /// <summary>
        /// The Id of the Pharmacy. Must be the same as server-side.
        /// </summary>
        [PrimaryKey]
        public int Id { get; set; }

        /// <summary>
        /// The Name of the Pharmacy. Should be the same as server-side.
        /// </summary>
        [NotNull]
        public string Name { get; set; }

        /// <summary>
        /// The Address of the Pharmacy.
        /// </summary>
        [NotNull]
        public string Address { get; set; }

        /// <summary>
        /// Position - Longitude.
        /// </summary>
        [NotNull]
        public double Longitude { get; set; }

        /// <summary>
        /// Position - Latitude.
        /// </summary>
        [NotNull]
        public double Latitude { get; set; }

        public string OpeningHours { get; set; }

        public Pharmacy ToPharmacy() {
            return new Pharmacy {
                Id = Id,
                Name = Name,
                Address = Address,
                Longitude = Longitude,
                Latitude = Latitude,
                StorageName = null,
                Email = null,
                OpeningHours = OpeningHours
            };
        }

        public static PharmacyItem FromPharmacy(Pharmacy pharmacy) {
            return new PharmacyItem {
                Id = pharmacy.Id,
                Name = pharmacy.Name,
                Address = pharmacy.Address,
                Longitude = pharmacy.Longitude,
                Latitude = pharmacy.Latitude,
                OpeningHours = pharmacy.OpeningHours
            };
        }

    }

}
