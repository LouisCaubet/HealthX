using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthX.Server.Database;
using HealthX.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace HealthX.Server.Controllers {

    [Route("api/[controller]")]
    public class ReservationByIdController : Controller {

        // GET api/<controller>/5
        [HttpGet("{data}")]
        public DBReservation Get(string data) {

            string username, password;
            int id;

            // Parse username and password from data
            try {
                string[] splitted = data.Split(";");
                username = splitted[0];
                password = splitted[1];
                id = Int32.Parse(splitted[2]);
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

            List<DBReservation> reservations = Startup.private_database_manager.GetReservations("SELECT * FROM Reservations WHERE Id=" + id);
            if (reservations.Count == 0) return null;

            return reservations[0];
            
        }

    }

}
