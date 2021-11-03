using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthX.Datatypes {

    /// <summary>
    /// Object representing a pharmacy as it is in database.
    /// </summary>
    public class Pharmacy {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string StorageName { get; set; }
        public string Email { get; set; }
        public string Infos { get; set; }
        public string OpeningHours { get; set; }

    }

}
