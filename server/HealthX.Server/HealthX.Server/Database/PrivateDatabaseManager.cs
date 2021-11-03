using HealthX.Server.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace HealthX.Server.Database {

    public class PrivateDatabaseManager {

        private const string Address = "ASUSCAUBET";
        private const string Database = "HealthX_protected";

        private const string UserId = "PHARMACY";
        private const string Password = "FdrdUGWPHyJeVpzW";

        private const string UserId_private = "SERVER";
        private const string Password_private = "pFxE7KL9xTe7yYRP";

        SqlConnection connection;
        SqlConnection connection_private;

        public void Connect() {

            connection = new SqlConnection(
                "Server=" + Address + ";" +
                "Database=" + Database + ";" +
                "User Id=" + UserId + ";" +
                "Password=" + Password + ";" +
                "MultipleActiveResultSets=true");

            connection_private = new SqlConnection(
                "Server=" + Address + ";" +
                "Database=" + "HealthX_private" + ";" +
                "User Id=" + UserId_private + ";" +
                "Password=" + Password_private + ";" +
                "MultipleActiveResultSets=true");

            try {
                connection.Open();
                connection_private.Open();
                Console.WriteLine("[INFO] Connection to SQL Server as private successful");
            }
            catch (Exception e) {
                Console.WriteLine("[FATAL] Could not connect to database (as private)!");
                Console.WriteLine(e.StackTrace);
            }

        }

        public void WriteReservation (string command) {

            SqlCommand cmd = new SqlCommand(command, connection);
            cmd.ExecuteNonQueryAsync();

        }

        public List<DBReservation> GetReservations (string query) {

            SqlCommand command = new SqlCommand(query, connection);
            SqlDataReader reader = command.ExecuteReader();
            List<DBReservation> result = new List<DBReservation>();

            if (!reader.HasRows) {
                reader.Close();
                return result;
            }

            while (reader.Read()) {

                DBReservation row = new DBReservation {
                    Id = (int) reader[0],
                    Client_ID = (int) reader[1],
                    Pharmacy_ID = (int) reader[2],
                    DateTime = ((DateTime) reader[3]).ToString("yyyyMMdd HH:mm"),
                    ReservationType = (int) reader[4],
                    TextContent = reader[5].GetType() == typeof(DBNull) ? null : (string) reader[5],
                    ImageContent = reader[6].GetType() == typeof(DBNull) ? null : (byte[]) reader[6],
                    Paid = (bool) reader[7], 
                    Price = (decimal) reader[8],
                    Payment_ID = (string) reader[9],
                    Payment_Status = (string) reader[10],
                    Validated = (bool) reader[11], 
                    Ready = (bool) reader[12],
                    Fulfilled = (bool) reader[13], 
                };

                result.Add(row);
            }

            reader.Close();
            return result;

        }

        public User GetUser (string username) {
            return QueryUsers("SELECT * FROM Users WHERE Username='" + username+"'");
        }

        public User GetUser (int id) {
            return QueryUsers("SELECT * FROM Users WHERE Id=" + id);
        }

        private User QueryUsers(string query) {

            SqlCommand command = new SqlCommand(query, connection_private);
            SqlDataReader reader = command.ExecuteReader();
            List<User> result = new List<User>();

            while (reader.Read()) {

                User user = new User {
                    Id = (int) reader[0],
                    Username = (string) reader[1],
                    Password = (string) reader[2],
                    Favorites = reader[3] is DBNull ? null : (string) reader[3],
                    HomePharmacyId = reader[4] is DBNull ? -1 : (int) reader[4],
                    Email = (string) reader[5],
                    Phone = reader[6] is DBNull ? null : (string) reader[6],
                    BraintreeID = (string) reader[7],
                    LastLogin = ((DateTime) reader[8]).ToString("yyyyMMdd HH:mm"),
                    ReservationCount = (int) reader[9],
                    Country = (string) reader[10],
                    AccountCreation = ((DateTime) reader[11]).ToString("yyyyMMdd HH:mm")
                };

                result.Add(user);

            }

            if (result.Count == 0) return null;
            else if (result.Count == 1) return result[0];
            else throw new ArgumentException("More than one user found with query " + query);

        }

        public void UpdateUser(int id, string lastlogin) {
            SqlCommand cmd = new SqlCommand("UPDATE Users SET LastLogin=" + lastlogin + " WHERE Id="+id, connection_private);
            cmd.ExecuteNonQueryAsync();
        }

        public void CreateUser (string query) {
            SqlCommand cmd = new SqlCommand(query, connection_private);
            cmd.ExecuteNonQuery();
        }

        public List<PharmacyAccount> GetPharmacyAccounts(string query) {

            SqlCommand cmd = new SqlCommand(query, connection_private);
            SqlDataReader reader = cmd.ExecuteReader();
            List<PharmacyAccount> result = new List<PharmacyAccount>();

            while (reader.Read()) {

                PharmacyAccount account = new PharmacyAccount {
                    Id = (int) reader[0],
                    PharmacyLogin = (string) reader[1],
                    PasswordHash = (string) reader[2],
                    PublicId = (int) reader[3],
                    PharmacyCode = (string) reader[4],
                    PrivateMail = (string) reader[5],
                    LastDataUpdate = reader[6] is DBNull ? new DateTime() : (DateTime) reader[6]
                };

                result.Add(account);

            }

            return result;

        } 

        public void WritePublicData(string query) {
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.ExecuteNonQuery();
        }




    }

}
