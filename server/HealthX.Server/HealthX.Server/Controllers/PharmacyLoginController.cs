using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthX.Server.Database;
using HealthX.Server.Models;
using Microsoft.AspNetCore.Mvc;


namespace HealthX.Server.Controllers {

    [Route("api/[controller]")]
    public class PharmacyLoginController : Controller {

        // GET api/<controller>/5
        [HttpGet("{data}")]
        public PharmacyAccount Get(string data) {

            string username, password;

            // Parse username and password from data
            try {
                username = data.Split(";")[0];
                password = data.Split(";")[1];
            }
            catch (Exception) {
                Program.Log(LoggingStatus.INFO, "Pharmacy Login", "Could not parse data");
                Response.StatusCode = 400;
                return null;
            }

            PharmacyAccount pharmacy;

            // Get pharmacy acccount from database if exists
            try {
                var result = Startup.private_database_manager.GetPharmacyAccounts(QueryCreator.GetPharmacyAccount(username));
                if (result.Count == 0) {
                    Program.Log(LoggingStatus.INFO, "Pharmacy Login", "Invalid Username : " + username);
                    Response.StatusCode = 401;
                    return null;
                }
                else pharmacy = result[0];
            }
            catch (Exception e) {
                Program.Log(LoggingStatus.WARNING, "Pharmacy Login", "An error occured during SQL Query");
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                Response.StatusCode = 400;
                return null;
            }

            // Check password
            if (!Utils.IsPasswordValid(password, pharmacy.PasswordHash)) {
                Response.StatusCode = 401;
                return null;
            }

            // Login / password valid. returning
            return pharmacy;

        }

    }
}
