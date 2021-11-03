using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Braintree;
using HealthX.Server.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HealthX.Server.Controllers {

    [Route("api/[controller]")]
    public class ClientTokenController : Controller {

        // GET: api/<controller>
        [HttpGet("{data}")]
        public UserToken Get(string data) {

            string username, password;

            try {
                username = data.Split(";")[0];
                password = data.Split(";")[1];
            }
            catch (Exception) {
                Program.Log(LoggingStatus.WARNING, "ClientToken GET", "Invalid argument 'data' passed. Aborting.");
                Response.StatusCode = 400;
                return null;
            }

            // Get User from database
            User user;
            try {
                user = Startup.private_database_manager.GetUser(username);
            }
            catch (Exception) {
                Program.Log(LoggingStatus.INFO, "ClientToken GET", "No user named " + username);
                Response.StatusCode = 400;
                return null;
            }

            if(!Utils.IsPasswordValid(password, user.Password)) {
                Response.StatusCode = 401;
                return null;
            }


            Program.Log(LoggingStatus.INFO, "ClientToken GET", "Started client token generation for " + username);
            string customer_id = user.BraintreeID;

            var token_request = new ClientTokenRequest {
                CustomerId = "" + customer_id
            };

            string client_token = Startup.payment_gateway.ClientToken.Generate(token_request);
            Program.Log(LoggingStatus.INFO, "ClientToken GET", "Returning client token for user " + username);
            return new UserToken { Token = client_token };

        }

    }
}
