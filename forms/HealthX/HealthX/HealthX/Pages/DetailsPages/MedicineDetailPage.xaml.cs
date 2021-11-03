using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using HealthX.Datatypes;
using System.Diagnostics;
using HealthX.Backend;
using HealthX.Braintree;
using HealthX.Utils;
using HealthX.Localization;

namespace HealthX.Pages.DetailsPages {

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MedicineDetailPage : ContentPage {

        // The medicine we draw the details of. 
        Medicine medicine;

        // First two grids are for nearest pharmacies. Only one is used if a specific pharmacy is chosen by user.
        Grid result1;
        Grid result2;

        // Last grid is for Home Pharmacy.
        Grid result3;

        // Price range of the medicine at the displayed pharmacies.
        decimal minPrice;
        decimal maxPrice;

        /// <summary>
        /// Constructor to display nearest pharmacies (and home pharmacy)
        /// </summary>
        /// <param name="medicine">The Medicine to draw details and buy options of.</param>
        public MedicineDetailPage(Medicine medicine) : this(medicine, null, null) { }

        /// <summary>
        /// Constructor to display availability at a specific pharmacy (and home pharmacy).
        /// </summary>
        /// <param name="medicine">The medicine to draw the details of.</param>
        /// <param name="pharmacy">The pharmacy where to buy.</param>
        /// <param name="availability">The availability at this pharmacy.</param>
        public MedicineDetailPage(Medicine medicine, Pharmacy pharmacy, StorageRow availability) {

            // Initialize XAML and pass args to class fields.
            InitializeComponent();
            this.Title = medicine.Name;
            this.medicine = medicine;

            // Fill header.
            text_name.Text = medicine.Name;
            text_desc.Text = medicine.Description;
            text_pflicht.Text = medicine.GetRequiresText();
            text_price.Text = medicine.GetPriceText();

            // "Require Prescription" alert drawn by default by XAML
            // Remove it if no prescription is required.
            if (medicine.Requires != PflichtValue.VERSCHREIBUNGSPFLICHT) {
                root.Children.Remove(alert_layout);
            }

            // If we want to display result at nearest pharmacies.
            if (pharmacy == null) {

                // Create Result View
                Label nearby = new Label { Text = AppResources.NearestAvailableText, Margin = new Thickness(30, 0), VerticalTextAlignment = TextAlignment.Center, FontAttributes = FontAttributes.Bold, FontSize = 20 };
                purchase_layout.Children.Add(nearby);

                // Generate grids.
                result1 = GetDefaultGrid();
                purchase_layout.Children.Add(result1);

                result2 = GetDefaultGrid();
                purchase_layout.Children.Add(result2);

                // At Home Pharmacy
                Label atHomePharmacy = new Label { Text = AppResources.AtHomePharmacyText, Margin = new Thickness(30, 0), VerticalTextAlignment = TextAlignment.Center, FontAttributes = FontAttributes.Bold, FontSize = 20 };
                purchase_layout.Children.Add(atHomePharmacy);

                // generate grid
                result3 = GetDefaultGrid();
                purchase_layout.Children.Add(result3);

                // Load nearest pharmacies and buy options from local DB / server.
                var task1 = LoadingTask(); 
                // Load availability and fill grid for Home Pharmacy.
                var task2 = LoadAtPharmacy(App.HomePharmacy);                

            }
            else { // If we want to buy at a specific pharmacy.

                // Create result view and grids.
                Label searchingAt = new Label { Text = AppResources.YouWereSearchingAtText, Margin = new Thickness(30, 0), VerticalTextAlignment = TextAlignment.Center, FontAttributes = FontAttributes.Bold, FontSize=20 };
                purchase_layout.Children.Add(searchingAt);
                CreateResultGrid(result1, pharmacy, availability.IsAvailable(), availability.Price);

                // Add "Home Pharmacy" title
                Label atHomePharmacy = new Label { Text = AppResources.AtHomePharmacyText, Margin = new Thickness(30, 0), VerticalTextAlignment = TextAlignment.Center, FontAttributes = FontAttributes.Bold, FontSize = 20 };
                purchase_layout.Children.Add(atHomePharmacy);

                // Load availability and fill grid for home pharmacy.
                var task = LoadAtPharmacy(App.HomePharmacy);

            }

        }

        /// <summary>
        /// changes the displayed price range if min/max Prices variables have been changed.
        /// </summary>
        private void UpdatePriceView() {
            if (minPrice == 0) {
                this.text_price.Text = maxPrice + "€";
            }
            else this.text_price.Text = minPrice + "€ - " + maxPrice + "€";
        }

        /// <summary>
        /// Task to load availability and fill the grid for a given pharmacy. 
        /// </summary>
        /// <param name="pharmacy">Self-explanatory.</param>
        /// <returns></returns>
        private async Task LoadAtPharmacy (Pharmacy pharmacy) {

            // Fetch availability
            Debug.WriteLine("[INFO] Load At Home Pharmacy GetAvailability query starting");
            var result = await App.API.QueryStorageAsync(pharmacy.Id, "Medicine_ID=" + medicine.Id);
            Debug.WriteLine("[INFO] Load At Home Pharmacy GetAvailability query completed");

            // cast to availability state.
            AvailabilityState availability = result.Length == 0 ? AvailabilityState.NOT_AVAILABLE : result[0].IsAvailable();

            // create result grid
            CreateResultGrid(result3, pharmacy, availability, result[0].Price);

            // update displayed price range if required.
            if (result[0].Price < minPrice) {
                minPrice = result[0].Price;
            }
            else if (result[0].Price > maxPrice) {
                maxPrice = result[0].Price;
            }
            else return;

            UpdatePriceView();

        }

        /// <summary>
        /// Task to load data for nearest pharmacies where the medicine is available, and fill result grids.
        /// </summary>
        /// <returns></returns>
        private async Task LoadingTask () {

            Debug.WriteLine("[INFO] Starting Loading Task");

            // Find GPS location
            var position = await DistanceUtils.GetLocation();
            Debug.WriteLine("[INFO] Success - Retrieved GPS position");

            // List containing 2 closest pharmacies where product is available
            // if none was found in 2km radius, we also display where it is available on order.
            List<Pharmacy> available_list = new List<Pharmacy>();
            List<AvailabilityState> states_list = new List<AvailabilityState>();

            // List used to store pharmacies where it is available on order. Sorted by distance.
            List<Pharmacy> order_list = new List<Pharmacy>();

            // List used to store price of medicine at each pharmacy
            Dictionary<int, decimal> price_dict = new Dictionary<int, decimal>();


            // Find nearest pharmacies
            Debug.WriteLine("[INFO] Starting first Pharmacies query - radius = 2km");
            Pharmacy[] pharmacies = await App.LocalDB.GetNearestToLocation(position.Longitude, position.Latitude, 2);
            Debug.WriteLine("[INFO] Query completed.");

            foreach (Pharmacy pharmacy in pharmacies) {

                // Check medicine is available at pharmacy
                Debug.WriteLine("[INFO] Querying storage for pharmacy " + pharmacy.Name);
                var result = await App.API.QueryStorageAsync(pharmacy.Id, "Medicine_ID=" + medicine.Id);
                Debug.WriteLine("[INFO] Storage query completed.");

                // medicine unknown at given pharmacy
                if (result.Length == 0) continue; 

                // medicine not available or orderable at this pharmacy.
                if (result[0].IsAvailable() == AvailabilityState.NOT_AVAILABLE) continue;

                // Store price in price dictionary
                price_dict.Add(pharmacy.Id, result[0].Price);

                // If not available but can be ordered, add to order list.
                if (result[0].IsAvailable() == AvailabilityState.AVAILABLE_ON_COMMAND) {
                    order_list.Add(pharmacy);
                    continue;
                }
                
                // If code gets here medicine is available
                available_list.Add(pharmacy);

                // We need to save the specific availability state to fill the grid after search has completed.
                if (result[0].IsAvailable() == AvailabilityState.FEW_LEFT) {
                    states_list.Add(AvailabilityState.FEW_LEFT);
                }
                else states_list.Add(AvailabilityState.AVAILABLE);

                // If we found 2 pharmacies where the medicine is available, we can quit search task.
                if (available_list.Count == 2) {
                    Debug.WriteLine("[INFO] Two Pharmacies found. Displaying and leaving task.");
                    decimal price1 = price_dict[available_list[0].Id];
                    decimal price2 = price_dict[available_list[1].Id];
                    // Draw results on result view.
                    FillNearbyLayout(available_list[0], available_list[1], states_list[0], states_list[1], price1, price2);
                    return;
                }

            }

            // We have 2 results, only one where it is available
            if (available_list.Count == 1 && order_list.Count >= 1) {
                Debug.WriteLine("[INFO] Two Pharmacies found. Displaying and leaving task.");
                decimal price1 = price_dict[available_list[0].Id];
                decimal price2 = price_dict[order_list[0].Id];
                FillNearbyLayout(available_list[0], order_list[0], states_list[0], AvailabilityState.AVAILABLE_ON_COMMAND, price1, price2);
                return;
            }
            
            // We have 0 or one result : continue searching in larger radius
            int search_radius = 5;
            while(available_list.Count < 2 && search_radius <= 10) {
                Debug.WriteLine("[INFO] No Pharmacies found. Continuing search with radius="+search_radius);
                pharmacies = await App.LocalDB.GetNearestToLocation(position.Longitude, position.Latitude, search_radius);
                Debug.WriteLine("[INFO] Query completed.");

                foreach (Pharmacy pharmacy in pharmacies) {

                    // Check medicine is available at pharmacy
                    var result = await App.API.QueryStorageAsync(pharmacy.Id, "Medicine_ID=" + medicine.Id);
                    if (result.Length == 0) continue; // medicine unknown at given pharmacy
                    if (result[0].IsAvailable() == AvailabilityState.NOT_AVAILABLE) continue;

                    // Store price in price dictionary
                    price_dict.Add(pharmacy.Id, result[0].Price);

                    if (result[0].IsAvailable() == AvailabilityState.AVAILABLE_ON_COMMAND) {
                        order_list.Add(pharmacy);
                        continue;
                    }

                    // If code gets here medicine is available
                    available_list.Add(pharmacy);
                    if (result[0].IsAvailable() == AvailabilityState.FEW_LEFT) {
                        states_list.Add(AvailabilityState.FEW_LEFT);
                    }
                    else states_list.Add(AvailabilityState.AVAILABLE);

                    if (available_list.Count == 2) {
                        decimal price1 = price_dict[available_list[0].Id];
                        decimal price2 = price_dict[available_list[1].Id];
                        FillNearbyLayout(available_list[0], available_list[1], states_list[0], states_list[1], price1, price2);
                        return;
                    }

                }

                if (available_list.Count == 1 && order_list.Count >= 1) {
                    decimal price1 = price_dict[available_list[0].Id];
                    decimal price2 = price_dict[order_list[0].Id];
                    FillNearbyLayout(available_list[0], order_list[0], states_list[0], AvailabilityState.AVAILABLE_ON_COMMAND, price1, price2);
                    return;
                }

                search_radius += 2;

            }

            if (available_list.Count == 1) {
                // just draw 1
                decimal price = price_dict[available_list[0].Id];
                FillNearbyLayout(available_list[0], null, states_list[0], null, price, -1);
                return;
            }

            // if code gets here it is available nowhere in 10km radius
            if (order_list.Count > 1) {
                decimal price1 = price_dict[order_list[0].Id];
                decimal price2 = price_dict[order_list[1].Id];
                FillNearbyLayout(order_list[0], order_list[1], AvailabilityState.AVAILABLE_ON_COMMAND, AvailabilityState.AVAILABLE_ON_COMMAND, price1, price2);
            }
            else if (order_list.Count == 1) {
                decimal price = price_dict[order_list[0].Id];
                FillNearbyLayout(order_list[0], null, AvailabilityState.AVAILABLE_ON_COMMAND, null, price, 0);
            }
            else {
                // Not available. Continue searching ?
                Label not_available = new Label {
                    Text = "No pharmacy where this medicine is available was found in a 10km-radius.",
                    TextColor = Color.DodgerBlue,
                    Margin = 50,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };
                purchase_layout.Children.Add(not_available);
                // TODO make clickable and let user continue the search.
            }

        }

        // Fills layout for nearby pharmacies with results in args.
        private void FillNearbyLayout (Pharmacy pharmacy1, Pharmacy pharmacy2, AvailabilityState state1, AvailabilityState state2, decimal price1, decimal price2) {

            CreateResultGrid(result1, pharmacy1, state1, price1);

            if (pharmacy2 != null) {
                CreateResultGrid(result2, pharmacy2, state2, price2);
            }

            // Update price range if required.

            if (price1 > maxPrice) maxPrice = price1;
            if (price2 > maxPrice) maxPrice = price2;

            if (price1 < minPrice) minPrice = price1;
            if (price2 < minPrice) minPrice = price2;

            UpdatePriceView();

        }

        /// <summary>
        /// Generates default result grid. 
        /// </summary>
        /// <returns></returns>
        private Grid GetDefaultGrid () {
            var grid =  new Grid {
                RowDefinitions = {
                        new RowDefinition(),
                        new RowDefinition()
                    },
                ColumnDefinitions = {
                        new ColumnDefinition {Width=GridLength.Auto},
                        new ColumnDefinition {Width=GridLength.Star},
                        new ColumnDefinition {Width=GridLength.Auto}
                    },
                Margin = 20,
                BackgroundColor = Color.LightGray,
                Padding = new Thickness(15, 15, 15, 15),
                HorizontalOptions=LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
            };

            grid.Children.Add(new ActivityIndicator { Color = Color.Blue, IsRunning = true, HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.CenterAndExpand }, 1, 1);
            return grid;
        }

        /// <summary>
        /// Fills grid with data in arguments.
        /// </summary>
        /// <param name="result1">The grid to fill</param>
        /// <param name="pharmacy">The pharmacy of the result</param>
        /// <param name="availability">The availability of the result</param>
        /// <param name="price">The price of the result.</param>
        private async void CreateResultGrid (Grid result1, Pharmacy pharmacy, AvailabilityState availability, decimal price) {
            // WARNING : no check that result1 is a result grid.

            Image pharmacyLogo = new Image { Source = "apotheke_logo.png", WidthRequest = Application.Current.MainPage.Width / 8 };
            Label pharmacyName = new Label { Text = pharmacy.Name, VerticalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.TailTruncation, FontSize = 18 };
            Label availabilityText = new Label { Text = availability.GetText(), VerticalTextAlignment = TextAlignment.Center, TextColor = availability.GetColor() };
            Button button = new Button { Text = availability.GetButtonText(), BackgroundColor = availability.GetButtonColor(), CornerRadius = 5 };

            button.Pressed += delegate {
                Debug.WriteLine("[INFO] Button " + pharmacy.Name + " pressed");
                OnButtonPress(pharmacy, availability, price);
            };

            // Get distance to pharmacy
            var position = await DistanceUtils.GetLocation();
            double distance = DistanceUtils.ComputeDistance(position.Latitude, position.Longitude, pharmacy.Latitude, pharmacy.Longitude);
            if (distance > 1000) distance = Math.Round(distance * 10) / 10;

            string distanceText = DistanceUtils.GetDistanceText(distance);

            Label priceAndDistance = new Label {
                Text = "Price: " + price + "€ - " + distanceText + " away",
                TextColor = Color.Blue,
            };

            StackLayout pharmacy_stack = new StackLayout {
                Orientation = StackOrientation.Vertical,
                Children = {
                    pharmacyName,
                    priceAndDistance
                }
            };

            Device.BeginInvokeOnMainThread(delegate {

                result1.Children.Clear();

                result1.Children.Add(pharmacyLogo, 0, 0);
                result1.Children.Add(pharmacy_stack, 1, 3, 0, 1);
                result1.Children.Add(availabilityText, 0, 2, 1, 2);
                result1.Children.Add(button, 2, 1);

            });


        }

        /// <summary>
        /// Handle "buy" button pressed.
        /// </summary>
        /// <param name="selected">Chosen pharmacy.</param>
        /// <param name="availability">Availability of the medicine there.</param>
        /// <param name="price">Price selected. price will (obviously) be checked server-side.</param>
        protected void OnButtonPress (Pharmacy selected, AvailabilityState availability, decimal price) {
            Debug.WriteLine("[INFO] OnButtonPress for button " + selected.Name + " called");

            ReservationHandler reservation_handler = new ReservationHandler(this, medicine, selected, availability, price);

        }

    }

}