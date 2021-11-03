using HealthX.Backend;
using HealthX.Datatypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using HealthX.Localization;

namespace HealthX.Pages {

    /* LOGIC for Shop Page. This script does :
     *  - Retrieve current reservations/orders from server
     *  - Handle conversions between Server-IDs and real names (for pharmacies and medicines)
     *  - Displays information about each reservation of user
     *  - Handle click on info_field to display details
     * 
     */

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShopPage : ContentPage {

        // Size of the "reservation status" image.
        static int ImageSize;

        // If the detail overlay is currently displayed.
        bool overlayVisible;

        /// <summary>
        /// Constructor for ShopPage. Should only be called by the TabbedPage. 
        /// Initializes display, bindings and gesture recognizers. 
        /// </summary>
        public ShopPage() {

            try {
                // Initialize XAML and PullToRefresh bindings.
                InitializeComponent();
                BindingContext = new UpdateManager(this);
            }
            catch (Exception e) {
                // Why try/catch - shouldn't go wrong !
                Debug.WriteLine(e.StackTrace);
            }

            // Register gesture recognizer. 
            TapGestureRecognizer overlay_tapped = new TapGestureRecognizer();
            overlay_tapped.Tapped += delegate {
                // Overlay tapped - do something ?
            };

            Overlay.GestureRecognizers.Add(overlay_tapped);
            
        }

        /// <summary>
        /// Displays the Detail Overlay for the Reservation passed in argument. 
        /// </summary>
        /// <param name="data">The reservation to draw the details of.</param>
        public void DisplayOverlayFor(ReservationData data) {

            // Set XAML objects visible
            Overlay.IsVisible = true;
            BlackBackground.IsVisible = true;
            
            // Fill ID Label with the reservation ID, and set it visible. 
            IdLabel.Text = "ID : " + data.Id;
            IdLabel.IsVisible = true;

            // If the reservation is image-based, display the image.
            if(data.Reservation.TextContent == null) {
                // Create an Image from byte[] and set the Xamarin Image visible.
                ReservationImage.Source = ImageSource.FromStream(() => new MemoryStream(data.Reservation.ImageContent));
                ReservationImage.IsVisible = true;
            }
            // If the reservation is text-based. 
            else {
                // Set label text to the content of the reservation, set font and visible. 
                // TODO set font for iOS. 
                ContentLabel.Text = AppResources.ContainsText + data.Content;
                ContentLabel.FontFamily = Device.RuntimePlatform == Device.Android ? "BalooBhaijaan-Regular.ttf#Baloo Bhaijaan" : null;
                ContentLabel.IsVisible = true;
            }

            // Draw status of reservation
            if (data.Ready) {
                // READY - green, bold
                StatusLabel.Text = AppResources.StatusText + AppResources.ReadyText;
                StatusLabel.TextColor = Color.Green;
                StatusLabel.FontAttributes = FontAttributes.Bold;
            }
            else if (data.Validated) {
                // Validated - orange.
                StatusLabel.Text = AppResources.StatusText + AppResources.InPreparationText;
                StatusLabel.TextColor = Color.Orange;
            }
            else {
                // Awaiting Validation - orangered. 
                StatusLabel.Text = AppResources.StatusText + AppResources.AwaitingValidationText;
                StatusLabel.TextColor = Color.OrangeRed;
            }

            // See overlayVisible definition. 
            overlayVisible = true;

        }

        /// <summary>
        /// Hides the overlay. 
        /// </summary>
        public void HideOverlay () {
            Overlay.IsVisible = false;
            ReservationImage.IsVisible = false;
            ContentLabel.IsVisible = false;
            BlackBackground.IsVisible = false;
            overlayVisible = false;
        }

        /// <summary>
        /// Creates the Frame for a reservation passed in argument. 
        /// </summary>
        /// <param name="data">The reservation to create the frame of. </param>
        /// <returns></returns>
        public Frame CreateReservationTemplate(ReservationData data) {

            // Whether the reservation is an ORDER or a RESERVATION
            Label ReservationType = new Label {
                Text = data.ReservationType == 0 ? AppResources.ReservationText.ToUpper() : AppResources.OrderText.ToUpper(),
                FontFamily = Device.RuntimePlatform == Device.Android ? "OpenSans.ttf#Open Sans SemiBold" : null,
                FontSize = 14,
                TextColor = Color.Gray
            };

            // The Pharmacy at which the reservation is made. 
            Label Pharmacy = new Label {
                Text = " @ " + data.Pharmacy,
                FontFamily = Device.RuntimePlatform == Device.Android ? "BalooBhaijaan-Regular.ttf#Baloo Bhaijaan" : null,
                FontSize = 11,
                TextColor = Color.DarkRed
            };

            // The content of the reservation, either list of medicines or "Image of Prescription" if image-based.
            Label Content = new Label {
                Text = AppResources.ContainsText + data.Content,
                FontFamily = Device.RuntimePlatform == Device.Android ? "BalooBhaijaan-Regular.ttf#Baloo Bhaijaan" : null,
                TextColor = Color.Black
            };

            // The Date and time when the reservation was made. 
            Label Datetime = new Label {
                Text = data.Datetime + ", ",
                TextColor = Color.Gray
            };

            // If the reservation has already been paid or not. 
            Label PaidLabel = new Label();
            if (data.Paid) {
                PaidLabel.Text = AppResources.PaidText;
                PaidLabel.TextColor = Color.Green;
            }
            else {
                PaidLabel.Text = AppResources.NotPaidText;
                PaidLabel.TextColor = Color.Orange;
            }

            // The status of the reservation. 
            Label StatusLabel = new Label {
                FontSize = 10,
                HorizontalTextAlignment = TextAlignment.Center
            };
            Image StatusImage = new Image {
                WidthRequest = ImageSize
            };

            if (data.Validated) {
                StatusLabel.Text = AppResources.InPreparationText;
                StatusLabel.TextColor = Color.Orange;

                StatusImage.Source = "InPreparation.png";
            }
            else if (data.Ready) {
                StatusLabel.Text = "    "+ AppResources.ReadyText + "    ";
                StatusLabel.TextColor = Color.Green;
                StatusLabel.FontAttributes = FontAttributes.Bold;

                StatusImage.Source = "Ready.png";
            }
            else {
                StatusLabel.Text = AppResources.AwaitingValidationText;
                StatusLabel.TextColor = Color.OrangeRed;

                StatusImage.Source = "AwaitingValidation.png";
            }

            // Organize Elements in layouts. 

            // First line. 
            StackLayout title = new StackLayout {
                Children = { ReservationType, Pharmacy },
                Orientation = StackOrientation.Horizontal
            };

            // Second line. 
            StackLayout bottom = new StackLayout {
                Children = { Datetime, PaidLabel },
                Orientation = StackOrientation.Horizontal
            };

            // Right column
            StackLayout status = new StackLayout {
                Children = { StatusImage, StatusLabel },
                Orientation = StackOrientation.Vertical,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.EndAndExpand
            };

            // Organize elements like this:
            // Reservation @ pharmacy_name  | Status
            // Datetime | Made?             |

            Grid layout = new Grid {
                BackgroundColor = Color.White,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,

                ColumnDefinitions = {
                    new ColumnDefinition {Width = GridLength.Auto},
                    new ColumnDefinition {Width = GridLength.Star}
                }

            };
            layout.Children.Add(title, 0, 0);
            layout.Children.Add(Content, 0, 1);
            layout.Children.Add(bottom, 0, 2);
            layout.Children.Add(status, 1, 2, 0, 3);

            // make the list item rounded rectangle with a Frame. 
            Frame frame = new Frame {
                CornerRadius = 10,
                Margin = new Thickness(20,0,20,20),
                OutlineColor = Color.Black,
                HasShadow = true,
                BackgroundColor = Color.White
            };

            // Register tap gesture recognizer and open the overlay when tapped. 
            TapGestureRecognizer tapped = new TapGestureRecognizer();
            tapped.Tapped += delegate {
                if (!overlayVisible) {
                    DisplayOverlayFor(data);
                }
            };

            frame.GestureRecognizers.Add(tapped);
            frame.Content = layout;

            return frame;

        }

        /// <summary>
        /// Returns the Layout. Required in the UpdateManager class. 
        /// </summary>
        /// <returns></returns>
        public StackLayout GetReservationView () {
            return ReservationView;
        }

        /// <summary>
        /// Called by Xamarin when page in rendered. Tasks done here can't be done in constructor, as some
        /// things are still unknown there. 
        /// </summary>
        protected override void OnAppearing() {
            base.OnAppearing();

            // Determine the size of the status image. 
            ImageSize = (int) Application.Current.MainPage.Width / 8;

            // Initialize Update Manager and run it once. 
            try { 
                UpdateManager manager = (UpdateManager)BindingContext;
                Device.StartTimer(TimeSpan.FromMilliseconds(500), () => {
                    // a timer is required here, as the command fails else. WHY?
                    var task = manager.ExecuteRefreshCommand();
                    return false;
                });
            }
            catch (Exception e) {
                // Shouldn't fail !
                Debug.WriteLine(e.StackTrace);
            }

            // This loading indicator should be completely removed, as it is never visible. TODO
            LoadingIndicator.IsVisible = false;

            // Set Window title font and Thickness (title is created in XAML but those things can't be done there)
            // TODO set font for IOS.
            TitleLabel.FontFamily = Device.RuntimePlatform == Device.Android ? "BalooBhaijaan-Regular.ttf#Baloo Bhaijaan" : null;
            TitleLabel.Margin = new Thickness(10, 20);

            // Gesture Recognizer to close the overlay. 
            TapGestureRecognizer exit_overlay = new TapGestureRecognizer();
            exit_overlay.Tapped += delegate {
                if (overlayVisible) {
                    HideOverlay();
                }
            };

            absolute_layout.GestureRecognizers.Add(exit_overlay);

        }

    }

    public class UpdateManager : INotifyPropertyChanged {

        // Instance of the page. 
        ShopPage page;

        // Simple constructor. 
        public UpdateManager (ShopPage page) {
            this.page = page;
        }

        bool isBusy;
        public bool IsBusy {
            get { return isBusy; }
            set {
                if (isBusy == value)
                    return;

                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        // Synchronous execution of ExecuteRefreshCommand, used by the PullToRefresh plugin. 
        public ICommand RefreshCommand {
            get { return new Command(async () => await ExecuteRefreshCommand()); }
        }

        // Refreshs the displayed reservations. Loads from server and calls drawing methods. 
        public async Task ExecuteRefreshCommand() {

            // Make sure refresh is not already running. 
            if (IsBusy) return;
            IsBusy = true;

            // Get reservations from server
            Debug.WriteLine("\n[INFO] Getting reservations from server.");
            DBReservation[] reservations = await App.API.GetReservations(App.Username, App.Password);
            Debug.WriteLine("\n[INFO] Got reservations from server.");

            // Create ReservationData list
            List<ReservationData> temp = new List<ReservationData>();

            // If user has no reservations. 
            if (reservations.Length == 0) {

                // "You have no ongoing purchases"
                Label empty = new Label {
                    Text = AppResources.NoCurrentPurchasesText,
                    HorizontalTextAlignment = TextAlignment.Center,
                    
                };

                // Clear the page and draw the label. 
                Device.BeginInvokeOnMainThread(delegate {
                    page.GetReservationView().Children.Clear();
                    page.GetReservationView().Children.Add(empty);
                });

                // Timer is required here to prevent interferences. 
                Device.StartTimer(TimeSpan.FromMilliseconds(50), () => {
                    IsBusy = false;
                    return false;
                });

                return;

            }

            // Iterate through all reservation of the user. 
            foreach (DBReservation res in reservations) {

                if (res.Fulfilled) {
                    // Load it as archive ?
                    continue;
                }

                // Get the name of the pharmacy (db only stores ID). - Task starts here
                Debug.WriteLine("[INFO] Starting pharmacy query with ID=" + res.Pharmacy_ID);
                var pharmacyRaw = App.API.QueryPharmacyAsync(QueryCreator.QueryIdEquals(res.Pharmacy_ID));

                // Parse content of the reservation.
                string contains = "";

                if (res.TextContent == null) {
                    // Image-based case is very simple. 
                    contains = AppResources.ImageOfPrescriptionText;
                }
                else {
                    // If Text-based, we need to get the name of each medicine from the ID.
                    // Currently there is only one medicine by reservation. 
                    // TODO this is not compatible with reservation with more medicines. 
                    Debug.WriteLine("[INFO] Starting medicine query with ID=" + res.TextContent);
                    var medicineRaw = App.API.QueryMedicineAsync(QueryCreator.QueryIdEquals(Int32.Parse(res.TextContent)));
                    Medicine medicine = (await medicineRaw)[0];
                    Debug.WriteLine("[INFO] Completed medicine query with ID=" + res.TextContent);
                    contains = medicine.Name;
                }

                // Get pharmacy name - end task. 
                Pharmacy pharmacy = (await pharmacyRaw)[0];
                Debug.WriteLine("[INFO] Completed pharmacy query with ID=" + res.Pharmacy_ID);

                // Get the datetime and convert it to local time (server stores as UTC). 
                DateTime.TryParseExact(res.DateTime, "yyyyMMdd HH:mm", null, DateTimeStyles.AssumeUniversal, out DateTime dateTime);
                string datetime = dateTime.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

                // Create the reservation data object. 
                var data = new ReservationData {
                    Id = res.Id,
                    ReservationType = res.ReservationType,
                    Pharmacy = pharmacy.Name,
                    Content = contains,
                    Datetime = datetime,
                    Paid = res.Paid,
                    Validated = res.Validated,
                    Ready = res.Ready,
                    Reservation = res
                };

                temp.Add(data);

            }

            // Order the reservation list. 
            temp = temp.OrderBy((ReservationData d) => {
                int value;
                if (d.Ready) value = 0;
                else if (d.Validated) value = 1;
                else value = 2;
                return value;
            }).ToList();

            // Call draw methods. Must be done on UI thread. 
            Device.BeginInvokeOnMainThread(delegate {

                page.GetReservationView().Children.Clear();

                foreach (ReservationData data in temp) {
                    View line = page.CreateReservationTemplate(data);
                    page.GetReservationView().Children.Add(line);
                }

            });

            // Here it works without a timer 
            IsBusy = false;
            
        }

        // Handle property changed. 
        public void OnPropertyChanged(string propertyName) {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    /// <summary>
    /// Data needed to draw the frame for a reservation.
    /// This is very different from the Reservation object!
    /// </summary>
    public struct ReservationData {

        public int Id { get; set; }
        public int ReservationType { get; set; }
        public string Pharmacy { get; set; }
        public string Content { get; set; }
        public string Datetime { get; set; }
        public bool Paid { get; set; }
        public bool Validated { get; set; }
        public bool Ready { get; set; }

        public DBReservation Reservation { get; set; }

    }

}