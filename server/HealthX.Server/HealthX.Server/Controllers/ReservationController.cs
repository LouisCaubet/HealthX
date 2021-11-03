using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using HealthX.Server.Models;
using HealthX.Server.Database;

using System.Security.Cryptography;
using System.Text;
using Braintree;


namespace HealthX.Server.Controllers {

    [Route("api/[controller]")]
    public class ReservationController : Controller {

        // GET api/reservation/
        [HttpGet("{meta}")]
        public List<DBReservation> Get(string meta) { // meta = "username;password"

            string username, password;

            try {
                username = meta.Split(";")[0];
                password = meta.Split(";")[1];
            }
            catch (Exception) {
                Program.Log(LoggingStatus.WARNING, "Reservation GET", "Invalid parameter 'meta' passed: " + meta);
                Response.StatusCode = 400;
                return null;
            }

            Program.Log(LoggingStatus.INFO, "Reservation GET", "Operation for user " + username + " started.");

            User client;

            try {
                client = Startup.private_database_manager.GetUser(username);
                if(client is null) {
                    Response.StatusCode = 401;
                    return null;
                }
            }
            catch(Exception) {
                Program.Log(LoggingStatus.INFO, "Reservation GET", "No user named " + username);
                Response.StatusCode = 400;
                return null;
            }

            // Check Password SHA-1 is correct
            if (!Utils.IsPasswordValid(password, client.Password)) {
                Program.Log(LoggingStatus.WARNING, "Reservation GET", "Incorrect password for user " + username + ". Code 401");
                Response.StatusCode = 401;
                return null;
            }

            Program.Log(LoggingStatus.INFO, "Reservation GET", "Successfully sent DB data back to user " + username);

            return Startup.private_database_manager.GetReservations(QueryCreator.GetReservationsQuery(client.Id));

        }

        // POST api/reservation
        [HttpPost]
        public void Post([FromBody]Reservation value) {

            if (value == null) {
                Program.Log(LoggingStatus.WARNING, "Reservation POST", "Called without a request body containing valid Reservation object.");
                Response.StatusCode = 400;
                return;
            }

            Program.Log(LoggingStatus.INFO, "Reservation POST", "New reservation started for user " + value.Username);

            User client;

            try {
                client = Startup.private_database_manager.GetUser(value.Username);
                if (client is null) { // User does not exist
                    Response.StatusCode = 401;
                    return;
                }
            }
            catch(Exception) {
                Program.Log(LoggingStatus.INFO, "Reservation POST", "No user named " + value.Username);
                Response.StatusCode = 400;
                return;
            }

            // Check Password SHA-1 is correct (DEBUG pswd is admin)
            if (!Utils.IsPasswordValid(value.Password,client.Password)) {
                Program.Log(LoggingStatus.WARNING, "Reservation POST", "The password passed to server for reservation by user " + value.Username + "is incorrect.");
                Response.StatusCode = 401;
                return;
            }

            // Check arguments are correct
            if (value.TextContent == null && value.ImageContent == null) {
                Program.Log(LoggingStatus.WARNING, "Reservation POST", "User =" + value.Username + " Reservation parameters are incorrect : both TextContent and ImageContent are null");
                Response.StatusCode = 400;
                return;
            }

            // Stores the server-side computed price
            decimal price = 0;
            int[] ids_list = null;

            // If Image_Content is null
            if (value.ImageContent == null) {

                string[] medicine_list = value.TextContent.Split(";");
                ids_list = new int[medicine_list.Length];

                for (int i = 0; i < medicine_list.Length; i++) {
                    ids_list[i] = Int32.Parse(medicine_list[i]);
                }

                if (!value.Paid) {
                    price = 1M;
                }

                // Compute price
                foreach (int id in ids_list) {

                    // Get Medicine from database
                    string query = QueryCreator.GenerateStorageQuery(value.PharmacyId, "Medicine_ID=" + id);

                    var med = Startup.database_manager.QueryStorage(query)[0];

                    // Check availability and change type to ORDER if not available
                    if (med.Available <= 0 && med.CanOrder) value.ReservationType = 1; 
                    else if (med.Available <= 0 && !med.CanOrder) {
                        // An error occured
                        Response.StatusCode = 409; // conflict
                        return;
                    }

                    if (value.Paid) {
                        price += med.Price;
                    }
                }


            }
            // If Text_Content is null
            // We have to check price is 1, paid is false and reservation type is 'order' (1).
            else if (value.TextContent == null) {
                price = 1M;
                if (value.Paid) {
                    Program.Log(LoggingStatus.WARNING, "Reservation POST", "User = " + value.Username + ": An image-based order is marked as Paid. Returning 400.");
                    Response.StatusCode = 400;
                    return;
                }
                if (value.ReservationType == 0) {
                    Program.Log(LoggingStatus.WARNING, "Reservation POST", "User = " + value.Username + ": An image-based order is marked as reservation. Returning 400.");
                    Response.StatusCode = 400;
                    return;
                }
            }
            else {
                Program.Log(LoggingStatus.WARNING, "Reservation POST", "User = " + value.Username + ": Incorrect parameters for reservation : both TextContent and ImageContent are used.");
                Response.StatusCode = 400;
                return;
            }

            // Check price is correct
            if (price != value.Price) {
                Program.Log(LoggingStatus.WARNING, "Reservation POST", "User = " + value.Username + ": Incorrect reservation. Client-side computed price is incorrect.");
                Response.StatusCode = 400;
                return;
            }

            // DO checks to validate command
            // - Check price in database
            // - Check price is 1 if not paid
            // - Check price is 1 and not paid if image_content != null

            // execute payment
            string ClientNonce = value.PaymentNonce;
            var request = new TransactionRequest {
                Amount = price,
                PaymentMethodNonce = ClientNonce,
                CustomerId = client.BraintreeID.ToString(),
                Options = new TransactionOptionsRequest {
                    SubmitForSettlement = true,
                    StoreInVaultOnSuccess = true
                },
            };

            Result<Transaction> result = Startup.payment_gateway.Transaction.Sale(request);

            if (!result.IsSuccess()) { // ! TRANSACTION FAILED SUBMITION FOR SETTLEMENT.
                Program.Log(LoggingStatus.WARNING, "Reservation POST", "User= " + value.Username + " Payment failed. Reason=" + result.Message);
                Response.StatusCode = 402;
                return;
            }

            string transaction_id = result.Target.Id;
            string transaction_status = result.Target.Status.ToString();

            string datetime = DateTime.UtcNow.ToString("yyyyMMdd HH:mm");

            string cmd = QueryCreator.GenerateReservationCommand(client.Id, value.PharmacyId, datetime, value.ReservationType, value.TextContent, value.ImageContent, value.Paid, price, transaction_id, transaction_status);
            Startup.private_database_manager.WriteReservation(cmd);

            // Update Storage if reservation
            if(value.ReservationType == 0) {
                foreach(int i in ids_list) {

                    string query = QueryCreator.GenerateStorageQuery(value.PharmacyId, "Medicine_ID=" + i);
                    var storage = Startup.database_manager.QueryStorage(query)[0];

                    PharmacyStorage pharmacyStorage = new PharmacyStorage {
                        MedicineId = i,
                        Available = storage.Available - 1,
                        CanOrder = storage.CanOrder
                    };

                    Startup.private_database_manager.WritePublicData(QueryCreator.UpdateStorage(
                        value.PharmacyId, pharmacyStorage, storage.Reserved + 1));

                }
            }

            Program.Log(LoggingStatus.INFO, "Reservation POST", "Reservation for user " + value.Username + " successfully completed.");
            Response.StatusCode = 200;

        }

    }
}
