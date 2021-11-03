using System;
using System.Security.Cryptography;
using System.Text;
using Braintree;
using HealthX.Server.Database;
using HealthX.Server.Models;
using Microsoft.AspNetCore.Mvc;


namespace HealthX.Server.Controllers {

    [Route("api/[controller]")]
    public class SignupController : Controller {

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody] UserSignup user) {

            // Check values are not null
            if (user == null || user.Username == null || user.Password == null || user.Email == null || user.Country == null) {
                Response.StatusCode = 400;
                return;
            }

            if (user.Username.Replace(" ", "") == "") {
                Response.StatusCode = 400;
                return;
            }

            User existingUser = Startup.private_database_manager.GetUser(user.Username);
            if(existingUser != null) {
                Response.StatusCode = 406;
                return;
            }

            if(!(user.Email.Contains("@") && user.Email.Contains("."))) {
                Response.StatusCode = 417;
                return;
            }

            // No spaces in phone number
            user.Phone = user.Phone.Replace(" ", "");

            int pharmacyId = -1;

            if (user.PharmacyCode != null) {

                // PharmacyAccount Query
                try {
                    var accounts = Startup.private_database_manager.GetPharmacyAccounts(QueryCreator.GetPharmacyAccountFromCode(user.PharmacyCode));
                    if (accounts.Count == 0) {
                        // Invalid Pharmacy Code
                    }
                    else {
                        pharmacyId = accounts[0].PublicId;
                    }
                }
                catch (Exception e) {
                    Program.Log(LoggingStatus.WARNING, "Signup Controller", "Exception while querying pharmacy accounts!");
                    Console.WriteLine(e.Message + "\n" + e.StackTrace);
                }

            }

            string passwordSha1;
            try {
                byte[] bytes = Encoding.UTF8.GetBytes(user.Password);
                passwordSha1 = HexStringFromBytes(SHA1.Create().ComputeHash(bytes));
            }
            catch (Exception) {
                Program.Log(LoggingStatus.WARNING, "User SIGNUP", "Could not compute SHA1 of password!");
                Response.StatusCode = 400;
                return;
            }

            // Create a new braintree customer

            CustomerRequest customerRequest = new CustomerRequest {
                Email = user.Email,
                LastName = user.Username
            };

            Result<Customer> customer = Startup.payment_gateway.Customer.Create(customerRequest);
            string braintree_ID = customer.Target.Id;

            // If everything is valid, then we can create the new user
            // TODO send confirmation email, etc (probably not in 1.0...)

            string query = QueryCreator.CreateUserCommand(
                user.Username, passwordSha1, user.Email, user.Phone, braintree_ID, user.Country, pharmacyId);

            Console.WriteLine("[DEBUG][SQL] query = " + query);

            Startup.private_database_manager.CreateUser(query);


        }

        public static string HexStringFromBytes(byte[] bytes) {
            var sb = new StringBuilder();
            foreach (byte b in bytes) {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

    }

}
