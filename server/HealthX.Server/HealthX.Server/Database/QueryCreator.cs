using HealthX.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthX.Server.Database {

    public static class QueryCreator {

        /// <summary>
        /// Generates the SQL Query to get infos from Storage Table
        /// </summary>
        /// <param name="pharmacyID">The pharmacy to query the storage of</param>
        /// <param name="query">The SQL WHERE params</param>
        /// <returns>The SQL Query as string</returns>
        public static string GenerateStorageQuery (int pharmacyID, string query) {
            return "SELECT * FROM Storage" + pharmacyID + " WHERE " + query ;
        }

        /// <summary>
        /// Generates SQL Query to get medicines from the Medicines table
        /// </summary>
        /// <param name="where">The SQL WHERE clause</param>
        /// <returns>The SQL Query as string</returns>
        public static string GenerateMedicineQuery (string where) {
            return "SELECT * FROM Medicines WHERE " + where;
        }

        /// <summary>
        /// Generates SQL Query to get pharmacies from the Pharmacies table
        /// </summary>
        /// <param name="where">The SQL WHERE clause</param>
        /// <returns>The SQL Query as string</returns>
        public static string GeneratePharmacyQuery (string where) {
            return "SELECT * FROM Pharmacies WHERE " + where;
        }

        /// <summary>
        /// Generates the SQL Command to INSERT a new reservation
        /// </summary>
        /// <param name="client_id">The ID of the client doing the reservation</param>
        /// <param name="pharmacy_id">The ID of the pharmacy where the reservation is done.</param>
        /// <param name="datetime">The Date&Time of the reservation.</param>
        /// <param name="type">Order OR Reservation</param>
        /// <param name="text_content">The TextContent of the reservation</param>
        /// <param name="image_content">The ImageContent of the reservation</param>
        /// <param name="paid">If the reservation is already paid.</param>
        /// <param name="paymentId">The braintree Transaction Id</param>
        /// <param name="paymentStatus">The braintree Transaction Status</param>
        /// <returns>The SQL Command to insert the reservation</returns>
        public static string GenerateReservationCommand (int client_id, int pharmacy_id, string datetime, int type, string text_content, byte[] image_content, bool paid, decimal price, string paymentId, string paymentStatus) {
            string paidString = paid ? "1": "0";
            string textContentString = text_content == null ? "NULL" : "'" + text_content + "'";
            string imageContentString = image_content == null ? "NULL" : "0x"+BitConverter.ToString(image_content).Replace("-","");
            return "INSERT INTO Reservations (Client_ID, Pharmacy_ID, DateTime, Type, Text_Content, Image_Content, Paid, Price, Payment_ID, Payment_Status, Validated, Ready, Fulfilled) VALUES (" + client_id+","+pharmacy_id+",'"+datetime+"',"+type+","+textContentString+","+imageContentString+","+paidString+"," + price + ",'" + paymentId + "','" + paymentStatus +"',0,0,0)";
        }

        /// <summary>
        /// Generates the SQL Query to get the reservation of a specific user
        /// </summary>
        /// <param name="userId">The user to get the reservations of.</param>
        /// <returns>The SQL Query.</returns>
        public static string GetReservationsQuery (int userId) {
            return "SELECT * FROM Reservations WHERE Client_ID=" + userId;
        }

        /// <summary>
        /// Generates the command to update the Payment Status of a reservation
        /// </summary>
        /// <param name="id">The Id of the reservation</param>
        /// <param name="newPaymentStatus">The new Payment Status</param>
        /// <returns>The SQL Query</returns>
        public static string GenerateReservationUpdateCommand (int id, string newPaymentStatus) {
            return "UPDATE Reservations SET Payment_Status='" + newPaymentStatus + "' WHERE Id=" + id;
        }

        /// <summary>
        /// Generates the command to update the Payment Status of a reservation
        /// </summary>
        /// <param name="transaction_id">The transaction Id of the reservation</param>
        /// <param name="newPaymentStatus">The new Payment Status</param>
        /// <returns>The SQL Query</returns>
        public static string GenerateReservationUpdateCommand(string transaction_id, string newPaymentStatus) {
            return "UPDATE Reservations SET Payment_Status='" + newPaymentStatus + "' WHERE Payment_ID='" + transaction_id+"'";
        }

        /// <summary>
        /// Generates the SQL command to update the Status of a reservation
        /// </summary>
        /// <param name="id">The Id of the reservation</param>
        /// <param name="validated">If the reservation is validated</param>
        /// <param name="ready">Id the reservation is ready</param>
        /// <param name="fulfilled">If the reservation is fulfilled</param>
        /// <returns>The SQL Command</returns>
        public static string GenerateReservationUpdateCommand(int id, bool validated, bool ready, bool fulfilled) {
            string validatedString = validated ? "1" : "0";
            string readyString = ready ? "1" : "0";
            string fulfilledString = fulfilled ? "1" : "0";
            return "UPDATE Reservations SET Validated=" + validatedString + ", Ready=" + readyString + ", Fulfilled=" + fulfilledString + " WHERE Id=" + id;
        }

        public static string ChangeReservationToOrderCommand (int resId) {
            return "UPDATE Reservations SET Type=1 WHERE Id=" + resId;
        }

        // USERS
        public static string CreateUserCommand(string username, string passwordHash, string email, string phone, string braintreeId, string country, int pharmacyId) {

            string datetime = DateTime.UtcNow.ToString("yyyyMMdd HH:mm");

            if (pharmacyId == -1) return 
                    "INSERT INTO Users(Username,PasswordHash,Email,Phone,Customer_ID,LastLogin,ReservationCount,Country,AccountCreation) " +
                    "VALUES ('" + username + "','"+passwordHash+"','"+email+"','"+phone+"','"+braintreeId+"','"+datetime+"',0,'"+country+"','"+datetime+"')";
            else return "INSERT INTO Users(Username,PasswordHash,HomePharmacy_ID,Email,Phone,Customer_ID,LastLogin,ReservationCount,Country,AccountCreation) " +
                    "VALUES ('" + username + "','" + passwordHash + "'," + pharmacyId +",'" + email + "','" + phone + "','" + braintreeId + "','" + datetime + "',0,'" + country + "','" + datetime + "')";

        }

        public static string GetUserByUsername (string username) {
            return "SELECT * FROM Users WHERE Username='" + username+"'";
        }

        // PHARMACY ACCOUNTS
        public static string GetPharmacyAccount(string pharmacy_username) {
            return "SELECT * FROM PharmacyAccounts WHERE PharmacyLogin='" + pharmacy_username+"'";
        }

        public static string GetPharmacyAccountFromCode (string pharmacy_code) {
            return "SELECT * FROM PharmacyAccounts WHERE PharmacyCode='" + pharmacy_code+"'";
        }

        public static string GetReservationsAtPharmacy (int pharmacyId) {
            return "SELECT * FROM Reservations WHERE Pharmacy_ID=" + pharmacyId;
        }

        public static string UpdateStorage (int pharmacyId, PharmacyStorage storage, int reserved = -1) {

            string canOrderString = storage.CanOrder ? "1" : "0";

            if (storage.Price != 0) { // Called from Pharmacy Update
                return "UPDATE HealthX_data.dbo.Storage" + pharmacyId + " SET Available=" + storage.Available + ", CanOrder=" + canOrderString + ", Price="+ storage.Price +" WHERE Medicine_ID=" + storage.MedicineId;
            }

            if (reserved == -1) { // Maybe called somewhere
                return "UPDATE HealthX_data.dbo.Storage" + pharmacyId + " SET Available=" + storage.Available + ", CanOrder=" + canOrderString + " WHERE Medicine_ID=" + storage.MedicineId; 
            }
            else { // Called from reservation
                return "UPDATE HealthX_data.dbo.Storage" + pharmacyId + " SET Available=" + storage.Available + ", CanOrder=" + canOrderString + ", Reserved=" + reserved + " WHERE Medicine_ID=" + storage.MedicineId;
            }

        }

        public static string InsertStorage (int pharmacyId, PharmacyStorage storage) {
            return "INSERT INTO HealthX_data.dbo.Storage" + pharmacyId + " VALUES (" + storage.MedicineId + "," + storage.Available + ",0," + storage.Price + ")";
        }

    }

}
