using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HealthX.Server.Database;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HealthX.Server.Controllers {

    [Route("api/[controller]")]
    public class MedicineQueryController : Controller {


        // GET api/values/5
        [HttpGet("{sqlQuery}")]
        public IEnumerable<MedicineRow> Get(string sqlQuery) {

            try {
                string query = QueryCreator.GenerateMedicineQuery(sqlQuery);
                DatabaseManager manager = Startup.database_manager;
                List<MedicineRow> result = manager.QueryMedicine(query);

                return result;
            }
            catch (Exception e) {
                Program.Log(LoggingStatus.WARNING, "Medicine GET", "An exception was raised during query:");
                Console.WriteLine(e.StackTrace);
                Response.StatusCode = 400;
                return null;
            }

        }

    }

}
