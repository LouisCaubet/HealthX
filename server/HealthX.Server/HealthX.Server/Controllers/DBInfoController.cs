using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthX.Server.Database;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HealthX.Server.Controllers {

    [Route("api/[controller]")]
    public class DBInfoController : Controller {

        // GET api/<controller>/5
        [HttpGet("{data}")]
        public object Get(string data) {

            Program.Log(LoggingStatus.INFO, "DBInfo GET", "Request for DB info " + data + " responding.");

            if (data == "lastid") {
                // Get last id of Pharmacy Table
                List<PharmacyRow> result = Startup.database_manager.QueryPharmacy("SELECT TOP 1 * FROM Pharmacies ORDER BY Id DESC");
                int lastId = result[0].Id;

                Program.Log(LoggingStatus.INFO, "DBInfo GET", "Successfully responded to DBInfo " + data);
                return lastId;

            }

            Program.Log(LoggingStatus.WARNING, "DBInfo GET", "Unknown data parameter: " + data + ". 404");

            Response.StatusCode = 404;
            return null;

        }

    }

}
