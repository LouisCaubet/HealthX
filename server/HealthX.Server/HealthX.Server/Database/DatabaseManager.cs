using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace HealthX.Server.Database {

    public class DatabaseManager {

        public const string Address = "ASUSCAUBET";
        public const string Database = "HealthX_data";
        public const string UserId = "APPLICATION";
        public const string Password = "Muy9EqqRrzWT8JQp";

        SqlConnection connection;

        public void Connect () {

            connection = new SqlConnection(
                "Server=" + Address + ";" +
                "Database=" + Database + ";" +
                "User Id=" + UserId + ";" +
                "Password=" + Password + ";" + 
                "MultipleActiveResultSets=true"); // without MARS, server crashes when simultaneous request are done.

            try {
                connection.Open();
                Console.WriteLine("[INFO] Connection to SQL Server successful");
            }
            catch (Exception e) {
                Console.WriteLine("[FATAL] Could not connect to database!");
                Console.WriteLine(e.StackTrace);
            }
            
        }

        public List<PharmacyRow> QueryPharmacy (string query) {

            SqlCommand command = new SqlCommand(query, connection);
            List<PharmacyRow> result = new List<PharmacyRow>();

            using (SqlDataReader reader = command.ExecuteReader()) {

                if (!reader.HasRows) {
                    reader.Close();
                    return result;
                }

                while (reader.Read()) {
                    PharmacyRow row = new PharmacyRow(reader[0], reader[1], reader[2], reader[3], reader[4], reader[5], reader[6], reader[7], reader[8]);
                    result.Add(row);
                }

            }

            return result;

        }

        public List<MedicineRow> QueryMedicine (string query) {

            SqlCommand command = new SqlCommand(query, connection);
            List<MedicineRow> result = new List<MedicineRow>();

            using (SqlDataReader reader = command.ExecuteReader()) {

                if (!reader.HasRows) {
                    reader.Close();
                    return result;
                }

                while (reader.Read()) {
                    MedicineRow row = new MedicineRow(reader[0], reader[1], reader[2], reader[3], reader[4], reader[5]);
                    result.Add(row);
                }

            }

            return result;

        }

        public List<StorageRow> QueryStorage(string query) {

            SqlCommand command = new SqlCommand(query, connection);
            List<StorageRow> result = new List<StorageRow>();

            using (SqlDataReader reader = command.ExecuteReader()) {

                if (!reader.HasRows) {
                    reader.Close();
                    return result;
                }

                while (reader.Read()) {
                    StorageRow row = new StorageRow(reader[0], reader[1], reader[2], reader[3], reader[4]);
                    result.Add(row);
                }

            } 

            return result;

        }

    }

    public class PharmacyRow {

        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public double Longitude { get; private set; }
        public double Latitude { get; private set; }
        public string StorageName { get; private set; }
        public string Email { get; private set; }
        public string Info { get; private set; }
        public string OpeningHours { get; private set; }

        // Opening Hours String Type = char(112)
        // Mon 09:00 17:00
        // Tue 09:00 17:00
        // Wed 08:30 17:30
        // Thu 09:00 17:30
        // Fri 09:00 17:00
        // Sat 10:00 16:00
        // Sun -1:00 -1:00 // = closed

        public PharmacyRow(object id, object name, object address, object longitude, object latitude, object storageName, object email, object info, object openingHours) {
            Id = (int) id;
            Name = (string) name;
            Address = (string) address;
            Longitude = (double) longitude;
            Latitude = (double) latitude;
            StorageName = (string) storageName;
            Email = (string) email;

            if (info.GetType() == typeof(DBNull)) {
                Info = "";
            }
            else Info = (string) info;

            OpeningHours = (string) openingHours;
        }

    }

    public class MedicineRow {

        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int Requirement { get; private set; }
        public decimal Price { get; private set; }
        public byte[] Image { get; private set; }

        public MedicineRow (object id, object name, object desc, object require, object price, object image) {

            Id = (int) id;
            Name = (string) name;
            Description = (string) desc;
            Requirement = (int) require;
            Price = (decimal) price;

            if (image is DBNull) Image = null;
            else Image = (byte[]) image; 

        }

    }

    public class StorageRow {

        public int MedicineId { get; private set; }
        public int Available { get; private set; }
        public int Reserved { get; private set; }
        public bool CanOrder { get; private set; }
        public decimal Price { get; private set; }

        public StorageRow (object id, object available, object reserved, object canorder, object price) {

            MedicineId = (int) id;
            Available = (int) available;
            Reserved = (int) reserved;
            CanOrder = (bool) canorder;
            Price = (decimal) price;

        }

    }

}
