using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HealthX.Server.Database;
using HealthX.Server.Models;
using Microsoft.AspNetCore.Mvc;


namespace HealthX.Server.Controllers {

    [Route("api/[controller]")]
    public class LoginController : Controller {

        // GET api/<controller>/5
        [HttpGet("{data}")]
        public User Get(string data) {

            // Parse data
            string username, password;
            try {
                username = data.Split(";")[0];
                password = data.Split(";")[1];
            }
            catch (Exception) {
                Response.StatusCode = 404;
                return null;
            }

            // Attempt to find user in database
            User user;
            try {
                user = Startup.private_database_manager.GetUser(username);
            }
            catch (Exception e) {
                Response.StatusCode = 400;
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                return null;
            }

            if (user == null) {
                // User does not exist
                Response.StatusCode = 403;
                return null;
            }

            // Check password by comparing SHA1 signatures
            if (!Utils.IsPasswordValid(password, user.Password)) {
                // Incorrect password
                Response.StatusCode = 401;
                return null;
            }

            // User is valid. Updating login date 
            Startup.private_database_manager.UpdateUser(user.Id, DateTime.UtcNow.ToString("yyyyMMdd HH:mm"));

            // Deleting everything irrelevant from user object
            user.LastLogin = null;
            user.BraintreeID = null;
            user.Password = null;

            return user;

        }

    }

}
