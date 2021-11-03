using HealthX.Backend;
using HealthX.Datatypes;
using HealthX.Localization;
using HealthX.Pages.DetailsPages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HealthX.Braintree {

    /// <summary>
    /// Class that handles payment and send of reservation.
    /// </summary>
    public class ReservationHandler {

        Page caller_page; // page that called the ReservationHandler
        Medicine medicine; // the medicine being reserved. null if image-based.
        Pharmacy pharmacy; // the pharmacy where the reservation is done at.
        AvailabilityState availability; // the Availability of the medicine
        decimal price; // the price of the reservation

        // the reservation object that will eventually be sent to the server.
        Reservation reservation;

        public ReservationHandler(Page caller_page, Medicine medicine, Pharmacy pharmacy, AvailabilityState availability, decimal price) {

            this.caller_page = caller_page;
            this.medicine = medicine;
            this.pharmacy = pharmacy;
            this.availability = availability;
            this.price = price;

            StartReservation();

        }

        public ReservationHandler(Page caller_page, Pharmacy pharmacy, byte[] image) {
            // Order from Image
            this.caller_page = caller_page;
            this.pharmacy = pharmacy;
            StartReservationFromImage(image);
        }

        /// <summary>
        /// First part of the reservation : instanciate objects and open Drop-IN for payment. 
        /// This method is called for text-based reservations.
        /// </summary>
        private async void StartReservation() {

            // make sure the app knows the user's braintree token.
            if (App.BraintreeClientToken == null) {
                App.BraintreeClientToken = await App.API.GetBraintreeClientToken();
            }

            // Create the Action Sheet for payment choice.
            string action = await caller_page.DisplayActionSheet(AppResources.PaymentQuestion, AppResources.CancelText, null, AppResources.PayPickupText, AppResources.PayNowText);

            // Instanciate the reservation object from given data.
            reservation = new Reservation {
                Username = App.Username,
                Password = "admin",
                DateTime = DateTime.UtcNow.ToString("yyyyMMdd HH:mm"), // we should take server time...
                ImageContent = null,
                PharmacyId = pharmacy.Id,
                Price = 1M,
                ReservationType = (availability == AvailabilityState.AVAILABLE || availability == AvailabilityState.FEW_LEFT) ? 0 : 1,
                TextContent = "" + medicine.Id
            };

            // open payment UI based on user's choice in action sheet.
            if (action == AppResources.CancelText) {
                return; // cancel reservation.
            }
            else if (action == AppResources.PayPickupText) {
                // User will pay on pickup. Only App Use Fee must be paid.
                reservation.Paid = false;
                DependencyService.Get<IBraintreeUI>().SetReturnCall(DoCommand);
                DependencyService.Get<IBraintreeUI>().StartDropInUiForResult(App.BraintreeClientToken, 1M);
            }
            else if (action == AppResources.PayNowText) {
                // User will pay now through Braintree Drop-IN.
                reservation.Paid = true;
                reservation.Price = this.price;
                DependencyService.Get<IBraintreeUI>().SetReturnCall(DoCommand);
                DependencyService.Get<IBraintreeUI>().StartDropInUiForResult(App.BraintreeClientToken, reservation.Price);
            }
            else {
                // Should never occur.
                throw new ArgumentException("No ActionSheet result named " + action);
            }

        }

        /// <summary>
        /// First part of the reservation : instanciate objects and open Drop-IN for payment. 
        /// This method is called for image-based reservations.
        /// </summary>
        /// <param name="image">The picture of the prescription.</param>
        public async void StartReservationFromImage(byte[] image) {

            // Make sure we have the user's BRAINTREE client token.
            if (App.BraintreeClientToken == null) {
                App.BraintreeClientToken = await App.API.GetBraintreeClientToken();
            }

            // Instanciate the reservation object.
            reservation = new Reservation {
                ImageContent = image,
                Paid = false,
                Password = App.Password,
                PharmacyId = pharmacy.Id,
                Price = 1M,
                ReservationType = 1,
                TextContent = null,
                Username = App.Username
            };

            // Instanciate and launch the braintree Drop-IN UI.
            IBraintreeUI braintree = DependencyService.Get<IBraintreeUI>();
            braintree.SetReturnCall(DoCommand);
            braintree.StartDropInUiForResult(App.BraintreeClientToken, 1M);

        }

        /// <summary>
        /// Return call of the Drop-In UI.
        /// Does second part of reservation : send the reservation to the server.
        /// </summary>
        /// <param name="result">The result code.</param>
        /// <param name="nonce">The Payment Nonce.</param>
        /// <param name="type">The reservation type-</param>
        public void DoCommand(int result, string nonce, string type) {
            if (result == 0) {
                // Transaction Confirmed 
                reservation.PaymentNonce = nonce;

                // Do reservation in other thread and Display Alert of success / failure after await. 
                new Task(async delegate {

                    // Do copy of reservation in lock block to prevent concurrent use.
                    Reservation reservationLocal;
                    lock (reservation) {
                        reservationLocal = reservation;
                        reservation = null;
                    }

                    int code = await App.API.PostReservation(reservationLocal);
                    switch (code) {
                        case 200:
                            Device.BeginInvokeOnMainThread(delegate {
                                caller_page.DisplayAlert(AppResources.SuccessText, AppResources.ReservationSuccessText, AppResources.GenericOK);
                            });
                            break;
                        case 400:
                            Device.BeginInvokeOnMainThread(delegate {
                                caller_page.DisplayAlert(AppResources.ErrorText, AppResources.ReservationFailureText, AppResources.GenericOK);
                            });
                            break;
                        case 401:
                            // TODO - password error.
                            break;
                    }
                }).Start();

            }
            else {
                // An error occured
                caller_page.DisplayAlert(AppResources.ErrorText, AppResources.PaymentFailureText, AppResources.GenericOK);
                reservation = null;
            }

            // Remove caller page from navigation stack if image-based reservation.
            if(caller_page.GetType() == typeof(OrderFromImagePage)) {
                caller_page.Navigation.PopAsync(true);
            }

        }

    }

}
