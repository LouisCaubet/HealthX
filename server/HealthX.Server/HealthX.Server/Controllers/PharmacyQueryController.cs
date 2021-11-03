using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HealthX.Server.Database;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HealthX.Server.Controllers {

    [Route("api/[controller]")]
    public class PharmacyQueryController : Controller {

        // GET api/values/Name LIKE ...
        [HttpGet("{sqlQuery}")]
        public IEnumerable<PharmacyRow> Get(string sqlQuery) {

            try {
                string query = QueryCreator.GeneratePharmacyQuery(sqlQuery.Replace("_", " "));
                DatabaseManager manager = Startup.database_manager;
                List<PharmacyRow> result = manager.QueryPharmacy(query);
                return result;
            }
            catch (Exception e) {
                Program.Log(LoggingStatus.WARNING, "Pharmacy GET", "An exception was raised during query:");
                Console.WriteLine(e.StackTrace);
                Response.StatusCode = 400;
                return null;
            }

        }

    }

}
