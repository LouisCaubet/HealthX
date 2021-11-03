using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using HealthX.Datatypes;
using System.Diagnostics;

namespace HealthX.Backend {

    /// <summary>
    /// Class responsible for all Client-Server interactions.
    /// </summary>
    public class RestApiClient {

        // The URI of the server.
        private const string URI = "http://192.168.1.201:5000";

        // The HTTP client to communicate with the server.
        private HttpClient Client { get; set; }

        /// <summary>
        /// Constructor to initialize the HTTP Client.
        /// </summary>
        public RestApiClient() {

            // Instanciate the Http Client.
            Client = new HttpClient {
                BaseAddress = new Uri(URI)
            };

        }

        /// <summary>
        /// Queries the server to get pharmacies matching given WHERE clause.
        /// </summary>
        /// <param name="s">The WHERE clause of the SQL query on the pharmacy table.</param>
        /// <returns>An array of Pharmacy representing the result of the query</returns>
        public async Task<Pharmacy[]> QueryPharmacyAsync (string s) {

            HttpResponseMessage resp = await Client.GetAsync(new Uri("/api/pharmacyquery/"+s));
            string json = await resp.Content.ReadAsStringAsync();

            Pharmacy[] result = new Pharmacy[] { };
            result = JsonConvert.DeserializeAnonymousType(json, result);

            return result;

        }

        /// <summary>
        /// Queries the server to get medicines matching given WHERE clause.
        /// </summary>
        /// <param name="s">The WHERE clause of the SQL query on the medicine table.</param>
        /// <returns>An array of Medicine representing the result of the query</returns>
        public async Task<Medicine[]> QueryMedicineAsync(string s) {
            
            HttpResponseMessage resp = await Client.GetAsync(new Uri("/api/medicinequery/"+s));
            string json = await resp.Content.ReadAsStringAsync();

            
            MedicineRow[] resulttmp = new MedicineRow[] { };
            resulttmp = JsonConvert.DeserializeAnonymousType(json, resulttmp);

            // convert internal MedicineRow to Medicine objects.
            Medicine[] result = new Medicine[resulttmp.Length];

            for (int i=0; i<resulttmp.Length; i++) {
                result[i] = resulttmp[i].ToMedicine();
            }

            return result;

        }

        /// <summary>
        /// Queries the storage of given pharmacy for given WHERE clause.
        /// </summary>
        /// <param name="pharmacyId">The Id of the pharmacy to query.</param>
        /// <param name="sql">The WHERE clause to run on the storage table.</param>
        /// <returns>An array of StorageRow representing the result of the query.</returns>
        public async Task<StorageRow[]> QueryStorageAsync (int pharmacyId, string sql) {

            HttpResponseMessage resp = await Client.GetAsync(new Uri("/api/storagequery/" + pharmacyId + "/" + sql));
            string json = await resp.Content.ReadAsStringAsync();

            StorageRow[] result = new StorageRow[] { };
            result = JsonConvert.DeserializeAnonymousType(json, result);

            return result;

        }

        // PAYMENT API

        /// <summary>
        /// Gets the user's Braintree Client Token from server. Requires to be logged in!
        /// </summary>
        /// <returns>The user's braintree client token.</returns>
        public async Task<string> GetBraintreeClientToken() {

            // GET PRODUCTION TOKEN
            Debug.WriteLine("Obtaining Braintree client token ...");
            HttpResponseMessage resp = await Client.GetAsync(new Uri("/api/clienttoken/" + App.Username + ";" + App.Password)); 
            Debug.WriteLine("Successfully obtained braintree client token.");

            string json = await resp.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeAnonymousType(json, new UserToken()).Token;

        }

        /// <summary>
        /// Sends a new reservation to the server.
        /// </summary>
        /// <param name="reservation">The Reservation to send.</param>
        /// <returns>The StatusCode of the request.</returns>
        public async Task<int> PostReservation(Reservation reservation) {

            var stringContent = new StringContent(JsonConvert.SerializeObject(reservation), Encoding.UTF8, "application/json");
            Debug.WriteLine("[INFO] Reservation POST - StringContent=" + stringContent);
            var response = await Client.PostAsync("/api/reservation", stringContent);
            Debug.WriteLine("[INFO] Reservation POST - Completed - Code =" + response.StatusCode);


            return (int) response.StatusCode;
        }

        /// <summary>
        /// Gets the current reservations of given user.
        /// </summary>
        /// <param name="username">The user's username.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>An Array of DBReservation representing the result of the request.</returns>
        public async Task<DBReservation[]> GetReservations(string username, string password) {

            HttpResponseMessage response = await Client.GetAsync("/api/reservation/" + username + ";" + password);
            string json = await response.Content.ReadAsStringAsync();

            DBReservation[] reservations = new DBReservation[] { };
            reservations = JsonConvert.DeserializeAnonymousType(json, reservations);

            return reservations;

        }


        // DB INFO

        /// <summary>
        /// Returns the last pharmacy ID known by the server.
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetLastPharmacyId () {

            HttpResponseMessage response = await Client.GetAsync("/api/dbinfo/lastid");
            return Int32.Parse( await response.Content.ReadAsStringAsync() );

        }

    }

    // see server documentation for details
    internal class MedicineRow {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Requirement { get; set; }
        public decimal Price { get; set; }
        public byte[] Image { get; set; }

        public Medicine ToMedicine () {
            Medicine r = new Medicine(Id, Name, Description, (PflichtValue)Requirement, Price, Image);
            return r;
        }

    }

    /// <summary>
    /// Object representing data for a medicine at one pharmacy.
    /// </summary>
    public class StorageRow {

        /// <summary>
        /// The ID of the medicine
        /// </summary>
        public int MedicineId { get; set; }
        /// <summary>
        /// Count of available items of that medicine.
        /// </summary>
        public int Available { get; set; }
        /// <summary>
        /// If the pharmacy can order new items of that medicine.
        /// </summary>
        public bool CanOrder { get; set; }
        /// <summary>
        /// The price of this medicine at that pharmacy.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Converts raw data to an AvailabilityState. See AvailabilityState.
        /// </summary>
        /// <returns>The AvailabilityState for that medicine.</returns>
        public AvailabilityState IsAvailable () {

            if (Available <= 0 && CanOrder) {
                // TODO confirm by checking bool value of Reservable (not implemented yet)
                return AvailabilityState.AVAILABLE_ON_COMMAND;
            }
            else if (Available <= 10) {
                return AvailabilityState.FEW_LEFT;
            }
            else if (Available > 10){
                return AvailabilityState.AVAILABLE;
            }
            else {
                return AvailabilityState.NOT_AVAILABLE;
            }

        }

    }

    /// <summary>
    /// Object representing a reservation as it has to be sent to the server.
    /// </summary>
    public class Reservation {

        /// <summary>
        /// The client's username
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// The client's password
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// The ID of the pharmacy where the reservation is done.
        /// </summary>
        public int PharmacyId { get; set; }
        /// <summary>
        /// The client-side date&time of the reservation.
        /// </summary>
        public string DateTime { get; set; }
        /// <summary>
        /// The type of the reservation. 0 = RESERVATION ; 1 = ORDER
        /// </summary>
        public int ReservationType { get; set; } 
        /// <summary>
        /// The Text Content of the reservation - list of medicine ID's, separated by ';'
        /// </summary>
        public string TextContent { get; set; } 
        /// <summary>
        /// The Image Content of the reservation - picture of a prescription.
        /// </summary>
        public byte[] ImageContent { get; set; }
        /// <summary>
        /// If the reservation is paid through the app.
        /// </summary>
        public bool Paid { get; set; }
        /// <summary>
        /// The price of the reservation.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The Payment Nonce of the Client (See BRAINTREE)
        /// </summary>
        public string PaymentNonce { get; set; }

    }

    /// <summary>
    /// The Reservation model used to get data from database. 
    /// </summary>
    public class DBReservation {

        /// <summary>
        /// The ID of the reservation.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The ID of the Client who did the reservation
        /// </summary>
        public int Client_ID { get; set; }
        /// <summary>
        /// The ID of the Pharmacy where the reservation was done at.
        /// </summary>
        public int Pharmacy_ID { get; set; }
        /// <summary>
        /// The Date&Time of the reservation
        /// </summary>
        public string DateTime { get; set; }
        /// <summary>
        /// The type of the reservation. 0 = RESERVATION, 1 = ORDER
        /// </summary>
        public int ReservationType { get; set; }
        /// <summary>
        /// The Text Content of the reservation
        /// </summary>
        public string TextContent { get; set; }
        /// <summary>
        /// The Image Content of the reservation
        /// </summary>
        public byte[] ImageContent { get; set; }
        /// <summary>
        /// The price of the reservation
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// If the reservation was paid through the app.
        /// </summary>
        public bool Paid { get; set; }
        /// <summary>
        /// Additional payment data.
        /// </summary>
        public string Payment { get; set; }
        /// <summary>
        /// If the reservation has been validated by the pharmacy.
        /// </summary>
        public bool Validated { get; set; }
        /// <summary>
        /// If the reservation has been set ready by the pharmacy.
        /// </summary>
        public bool Ready { get; set; }
        /// <summary>
        /// If the reservation has been set fulfilled by the pharmacy.
        /// </summary>
        public bool Fulfilled { get; set; }

    }

    public class UserToken {
        public string Token { get; set; }
    }

}
