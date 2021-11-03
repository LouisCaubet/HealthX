using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HealthX.Server.Database;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HealthX.Server.Controllers {

    [Route("api/[controller]")]
    public class StorageQueryController : Controller {


        // GET api/values/5/Id=...
        [HttpGet("{pharmacyID}/{sqlQuery}")]
        public IEnumerable<StorageRow> Get(int pharmacyID, string sqlQuery) {

            // SQL Query should only contain the WHERE clause.
            // Final Query will look like
            // SELECT * WHERE + sqlQuery + FROM Storage+pharmacyID

            try {
                string query = QueryCreator.GenerateStorageQuery(pharmacyID, sqlQuery);
                DatabaseManager manager = Startup.database_manager;
                List<StorageRow> result = manager.QueryStorage(query);

                // Do query on "storage+pharmacyID" table, then send back result
                return result;
            }
            catch (Exception e) {
                Program.Log(LoggingStatus.WARNING, "Storage GET", "An exception was raised during query:");
                Console.WriteLine(e.StackTrace);
                Response.StatusCode = 400;
                return null;
            }

        }

    }

}
