using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthX.Server.Database;
using HealthX.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace HealthX.Server.Controllers {

    [Route("api/[controller]")]
    public class UpdateStorageController : Controller {


        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id) {
            return "";
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody] PharmacyStorageUpdate data) {

            // These values may not be null
            if (data == null || data.Updates == null || data.PharmacyPassword == null || data.PharmacyUsername == null) {
                Response.StatusCode = 400;
                return;
            }

            // Get Pharmacy Account from username
            PharmacyAccount pharmacy;
            try {
                var result = Startup.private_database_manager.GetPharmacyAccounts(QueryCreator.GetPharmacyAccount(data.PharmacyUsername));
                if (result.Count == 0) {
                    Response.StatusCode = 401;
                    return;
                }
                else pharmacy = result[0];
            }
            catch (Exception) {
                Response.StatusCode = 500;
                return;
            }

            // Check Password
            if (!Utils.IsPasswordValid(data.PharmacyPassword, pharmacy.PasswordHash)) {
                Response.StatusCode = 401;
                return;
            }

            // Load data in database
            foreach (PharmacyStorage line in data.Updates) {

                // Get current storage
                var result = Startup.database_manager.QueryStorage(QueryCreator.GenerateStorageQuery(pharmacy.PublicId, "Id=" + line.MedicineId));
                if (result.Count == 0) {
                    // add the line
                    string query = QueryCreator.InsertStorage(pharmacy.PublicId, line);
                    Startup.private_database_manager.WritePublicData(query);
                }
                else {

                    var storage = result[0];
                    var copy = line;
                    copy.Available = line.Available - storage.Reserved;

                    string query = QueryCreator.UpdateStorage(pharmacy.PublicId, copy);
                    Startup.private_database_manager.WritePublicData(query);

                }

            }

        }

    }
}
