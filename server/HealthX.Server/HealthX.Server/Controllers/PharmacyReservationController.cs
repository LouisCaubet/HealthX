using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthX.Server.Database;
using HealthX.Server.Models;
using Microsoft.AspNetCore.Mvc;


namespace HealthX.Server.Controllers {

    [Route("api/[controller]")]
    public class PharmacyReservationController : Controller {


        // GET api/<controller>/5
        [HttpGet("{data}")]
        public DBReservation[] Get(string data) {

            string username, password;

            // Parse username and password from data
            try {
                username = data.Split(";")[0];
                password = data.Split(";")[1];
            }
            catch (Exception) {
                Program.Log(LoggingStatus.INFO, "Pharmacy Reservation GET", "Could not parse data");
                Response.StatusCode = 400;
                return null;
            }

            PharmacyAccount pharmacy;

            // Get pharmacy acccount from database if exists
            try {
                var result = Startup.private_database_manager.GetPharmacyAccounts(QueryCreator.GetPharmacyAccount(username));
                if (result.Count == 0) {
                    Program.Log(LoggingStatus.INFO, "Pharmacy Reservation GET", "Invalid Username : " + username);
                    Response.StatusCode = 401;
                    return null;
                }
                else pharmacy = result[0];
            }
            catch (Exception e) {
                Program.Log(LoggingStatus.WARNING, "Pharmacy Reservation GET", "An error occured during SQL Query");
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                Response.StatusCode = 400;
                return null;
            }

            // Check password
            if (!Utils.IsPasswordValid(password, pharmacy.PasswordHash)) {
                Response.StatusCode = 401;
                return null;
            }

            // Get reservations by id
            try {
                var reservations = Startup.private_database_manager.GetReservations(QueryCreator.GetReservationsAtPharmacy(pharmacy.PublicId));
                return reservations.ToArray();
            }
            catch(Exception e) {
                Program.Log(LoggingStatus.WARNING, "Pharmacy Reservations GET", "An error occured while querying reservations.");
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                Response.StatusCode = 400;
                return null;
            }

        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody] PharmacyReservationEdit edit) {
            
            // Following values may not be null
            if(edit == null || edit.PharmacyUsername == null || edit.PharmacyPassword == null) {
                Response.StatusCode = 400;
                return;
            }

            // Check status is possible.
            if (edit.Fulfilled && (!edit.Ready || !edit.Validated)) {
                Response.StatusCode = 406;
                return;
            }
            if(edit.Ready && !edit.Validated) {
                Response.StatusCode = 406;
                return;
            }
            

            // Get pharmacy account from DB with username (if not exists, 400)
            PharmacyAccount pharmacy;
            try {
                var result = Startup.private_database_manager.GetPharmacyAccounts(QueryCreator.GetPharmacyAccount(edit.PharmacyUsername));
                if (result.Count == 0) {
                    Program.Log(LoggingStatus.INFO, "PharmacyReservation POST", "No pharmacy with username " + edit.PharmacyUsername);
                    Response.StatusCode = 400;
                    return;
                }
                else pharmacy = result[0];
            }
            catch (Exception) {
                Program.Log(LoggingStatus.WARNING, "PharmacyReservation POST", "An error occured while querying pharmacy accounts");
                Response.StatusCode = 400;
                return;
            }

            // Check password
            if (!Utils.IsPasswordValid(edit.PharmacyPassword, pharmacy.PasswordHash)) {
                Program.Log(LoggingStatus.INFO, "PharmacyReservation POST", "Incorrect password for pharmacy " + edit.PharmacyUsername);
                Response.StatusCode = 401;
                return;
            }
            
            // Get reservation
            DBReservation reservation;
            try {
                var result = Startup.private_database_manager.GetReservations("SELECT * FROM Reservations WHERE Id=" + edit.ReservationId);
                if (result.Count == 0) {
                    Program.Log(LoggingStatus.INFO, "PharmacyReservation POST", "No reservation with ID " + edit.ReservationId);
                    Response.StatusCode = 406;
                    return;
                }
                else reservation = result[0];
            }
            catch (Exception) {
                Program.Log(LoggingStatus.WARNING, "PharmacyReservation POST", "An error occured while querying reservations with Id = " + edit.ReservationId);
                Response.StatusCode = 406;
                return;
            }

            // Check reservation is done at this pharmacy
            if (reservation.Pharmacy_ID != pharmacy.PublicId) {
                Program.Log(LoggingStatus.INFO, "PharmacyReservation POST", "The pharmacy " + pharmacy.PharmacyLogin + " asked for a reservation that was not done there.");
                Response.StatusCode = 403;
                return;
            }

            // Do Change to order (if reservation is reservation)
            if (edit.ChangeToOrder && reservation.ReservationType == 0) {
                Program.Log(LoggingStatus.INFO, "PharmacyReservation POST", "Reservation with Id " + reservation.Id + " changed to ORDER.");
                string changetoorderquery = QueryCreator.ChangeReservationToOrderCommand(reservation.Id);
                Startup.private_database_manager.WriteReservation(changetoorderquery);
                // TODO send push notification
            }

            // Update reservation
            string query = QueryCreator.GenerateReservationUpdateCommand(edit.ReservationId, edit.Validated, edit.Ready, edit.Fulfilled);

            try {
                Startup.private_database_manager.WriteReservation(query);
            }
            catch (Exception e) {
                // SQL exception
                Program.Log(LoggingStatus.ERROR, "PharmacyReservation POST", "An error occured while updating the reservation");
                Response.StatusCode = 500;
            }
            
        }

    }

}
