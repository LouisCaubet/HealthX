using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthX.Backend {

    /// <summary>
    /// Helper class to create SQL queries.
    /// </summary>
    public static class QueryCreator {

        /// <summary>
        /// Creates a LIKE query on Name field
        /// </summary>
        /// <param name="name">the name to compare to.</param>
        /// <returns>the SQL WHERE clause.</returns>
        public static string QueryNameLike(string name) {
            return "Name LIKE '" + name + "%'";
        }

        /// <summary>
        /// Creates an SQL WHERE clause where the id equals the argument.
        /// </summary>
        /// <param name="id">the id to find.</param>
        /// <returns>The SQL Where clause.</returns>
        public static string QueryIdEquals(int id) {
            return "Id=" + id;
        }

    }

}
