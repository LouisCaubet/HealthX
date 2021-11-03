using System;
using System.Collections.Generic;

namespace HealthX.Server.Models {

    public class User {

        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string BraintreeID { get; set; }
        public string LastLogin { get; set; }
        public int ReservationCount { get; set; }
        public string Country { get; set; }
        public string AccountCreation { get; set; }

        public string Favorites { get; set; }
        public int HomePharmacyId { get; set; }

    }

    public class MedicineQuery {

        public string Query { get; set; }

    }

    public class PharmacyQuery {
        public string Query { get; set; }
    }

    public class StorageQuery {

        public int PharmacyId { get; set; }
        public string SqlQuery { get; set; }

    }

    // The Reservation model used to create a new reservation
    public class Reservation {

        public string Username { get; set; }
        public string Password { get; set; }
        public int PharmacyId { get; set; }
        public string DateTime { get; set; }
        public int ReservationType { get; set; } // 0=reserve, 1=order
        public string TextContent { get; set; } // List of medicine Ids, separated by ';'
        public byte[] ImageContent { get; set; }
        public bool Paid { get; set; }
        public decimal Price { get; set; }

        public string PaymentNonce { get; set; }

    }

    // The Reservation model used to get data from database. 
    public class DBReservation {

        public int Id { get; set; }
        public int Client_ID { get; set; }
        public int Pharmacy_ID { get; set; }
        public string DateTime { get; set; }
        public int ReservationType { get; set; }
        public string TextContent { get; set; }
        public byte[] ImageContent { get; set; }
        public decimal Price { get; set; }
        public bool Paid { get; set; }
        public string Payment_ID { get; set; }
        public string Payment_Status { get; set; }
        public bool Validated { get; set; }
        public bool Ready { get; set; }
        public bool Fulfilled { get; set; }

    }

    public class UserSignup {

        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PharmacyCode { get; set; }
        public string Country { get; set; }

    }

    public class UserToken {
        public string Token { get; set; }
    }

    public class PharmacyAccount {
        public int Id { get; set; }
        public string PharmacyLogin { get; set; }
        public string PasswordHash { get; set; }
        public int PublicId { get; set; } // the id of this pharmacy in the public pharmacies table
        public string PharmacyCode { get; set; }
        public string PrivateMail { get; set; }
        public DateTime LastDataUpdate { get; set; }
    }

    public class PharmacyReservationEdit {
        public string PharmacyUsername { get; set; }
        public string PharmacyPassword { get; set; }
        public int ReservationId { get; set; }
        public bool Validated { get; set; }
        public bool Ready { get; set; }
        public bool Fulfilled { get; set; }
        public bool ChangeToOrder { get; set; }
    }

    public class PharmacyStorageUpdate {
        public string PharmacyUsername { get; set; }
        public string PharmacyPassword { get; set; }
        public List<PharmacyStorage> Updates { get; set; }
    }

    public struct PharmacyStorage {
        public int MedicineId { get; set; }
        public int Available { get; set; }
        public bool CanOrder { get; set; }
        public decimal Price { get; set; }
    }

}
