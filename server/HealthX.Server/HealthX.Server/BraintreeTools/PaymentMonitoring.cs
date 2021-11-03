using Braintree;
using HealthX.Server.Database;
using HealthX.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HealthX.Server.BraintreeTools {

    public static class PaymentMonitoring {

        /// <summary>
        /// This list must contain all transactions that are not submitted for settlement yet.
        /// Status of submittion will be refreshed every 5 minutes until transaction is submitted.
        /// </summary>
        public static List<Transaction> AwaitingSubmittion { get; private set; }

        /// <summary>
        /// This list must contain all reservation ids that are not settled yet.
        /// This will be refreshed on the next day, 4:00 AM UTC, as those transactions are very unlikely to fail.
        /// We will still check the status if pickup occurs before settlement.
        /// </summary>
        public static List<int> AwaitingSettlement { get; private set; }

        private static Timer submittion_timer;
        private static Timer settlement_timer;

        /// <summary>
        /// This method must be called once on server start / reboot.
        /// </summary>
        public static void Init () {

            Program.Log(LoggingStatus.INFO, "BraintreeInit", "Initialization of payment monitoring started.");

            AwaitingSubmittion = new List<Transaction>();
            AwaitingSettlement = new List<int>();

            submittion_timer = new Timer(x => {
                RunSubmittionCheck();
            }, null, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5));


            TimeSpan timeToGo = TimeSpan.FromHours(24) - DateTime.UtcNow.TimeOfDay;

            settlement_timer = new Timer(x => {
                RunSettlementCheck();
            }, null, timeToGo, TimeSpan.FromHours(24));

            Program.Log(LoggingStatus.INFO, "BraintreeInit", "Initialization of payment monitoring completed. Now monitoring transactions.");

        }

        private static void RunSubmittionCheck () {

            new Task(delegate {
                Program.Log(LoggingStatus.INFO, "PaymentMonitoring", "Starting check batch for transactions AWAITING SUBMITTION");
                foreach (Transaction transaction in AwaitingSubmittion) {
                    string newStatus = transaction.Status.ToString();
                    // TODO do something if status is declined / disputed
                    Startup.private_database_manager.WriteReservation(QueryCreator.GenerateReservationUpdateCommand(transaction.Id, newStatus));
                }
                Program.Log(LoggingStatus.INFO, "PaymentMonitoring", "Operation completed.");
            }).Start();

        }

        private static void RunSettlementCheck () {

            new Task(delegate {
                Program.Log(LoggingStatus.INFO, "PaymentMonitoring", "Starting check batch for transactions AWAITING SETTLEMENT");
                foreach (int id in AwaitingSettlement) {
                    DBReservation res = Startup.private_database_manager.GetReservations("SELECT * FROM Reservations WHERE Id=" + id)[0];
                    Transaction transaction = Startup.payment_gateway.Transaction.Find(res.Payment_ID);
                    // TODO do something if transaction is disputed / declined.
                    Startup.private_database_manager.WriteReservation(QueryCreator.GenerateReservationUpdateCommand(id, transaction.Status.ToString()));
                }
                Program.Log(LoggingStatus.INFO, "PaymentMonitoring", "Operation completed.");
            }).Start();

        }

    }

}
