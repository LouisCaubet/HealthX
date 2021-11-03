using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthX.Server.Database;
using HealthX.Server.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HealthX.Server.Controllers {

    [Route("api/[controller]")]
    public class GetClientController : Controller {


        // GET api/<controller>/5
        [HttpGet("{data}")]
        public User Get(string data) {

            string username, password;
            int id;

            // PARSE DATA
            try {
                var raw = data.Split(";");
                username = raw[0];
                password = raw[1];
                id = Int32.Parse(raw[2]);
            }
            catch (Exception) {
                // Invalid argument data
                Program.Log(LoggingStatus.INFO, "Client GET", "Invalid argument 'data'");
                Response.StatusCode = 400;
                return null;
            }

            // CHECK PHARMACY ID
            PharmacyAccount account;
            try {
                var result = Startup.private_database_manager.GetPharmacyAccounts(QueryCreator.GetPharmacyAccount(username));
                if (result.Count == 0) {
                    Program.Log(LoggingStatus.INFO, "Pharmacy Login", "Invalid Username : " + username);
                    Response.StatusCode = 401;
                    return null;
                }
                else account = result[0];
            }
            catch (Exception e) {
                Program.Log(LoggingStatus.WARNING, "Pharmacy Login", "An error occured during SQL Query");
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                Response.StatusCode = 400;
                return null;
            }

            // Check password
            if (!Utils.IsPasswordValid(password, account.PasswordHash)) {
                Response.StatusCode = 401;
                return null;
            }

            // Get client by Id
            try {
                User user = Startup.private_database_manager.GetUser(id);
                user.AccountCreation = null;
                user.BraintreeID = null;
                user.Favorites = null;
                user.HomePharmacyId = 0;
                user.LastLogin = null;
                user.ReservationCount = -1;
                return user;
            }
            catch (Exception) {
                Program.Log(LoggingStatus.INFO, "Client GET", "No user with Id " + id);
                Response.StatusCode = 400;
                return null;
            }
            

        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

    }

}
