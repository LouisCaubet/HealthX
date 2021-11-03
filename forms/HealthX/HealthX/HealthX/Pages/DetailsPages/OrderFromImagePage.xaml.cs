using HealthX.Braintree;
using HealthX.Datatypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HealthX.Pages.DetailsPages {

    /*
     * Logic for the OrderFromImage Page.
     */

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OrderFromImagePage : ContentPage {

        /// <summary>
        /// Constructor to build and display the page.
        /// </summary>
        /// <param name="picture">The prescription picture the user has taken.</param>
        /// <param name="pharmacy">The pharmacy where the reservation is done.</param>
        public OrderFromImagePage(byte[] picture, Pharmacy pharmacy) {

            // Initialize XAML page.
            InitializeComponent();

            // Load arguments onto XAML fields.
            apotheke_name.Text = pharmacy.Name;
            prescription_image.Source = ImageSource.FromStream(() => new MemoryStream(picture));

            // Handle Confirmation button click.
            confirmation_button.Pressed += delegate {
                new ReservationHandler(this, pharmacy, picture);
            };

        }

        /// <summary>
        /// Called when the page is rendered by the engine.
        /// </summary>
        protected override void OnAppearing() {
            base.OnAppearing();

            // Set width of the picture relative to the screen size.
            // The must be done here, as screen size can't be retrieved during construction
            prescription_image.WidthRequest = (int) Application.Current.MainPage.Width / 1.75;

        }

    }
}