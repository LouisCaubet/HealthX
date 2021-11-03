using HealthX.Backend;
using HealthX.Datatypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

using Plugin.Geolocator;
using System.IO;
using HealthX.Utils;
using HealthX.Localization;

namespace HealthX.Pages {

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchPage : ContentPage {


        // Stores current position of SegmentedControl - 0=medicine ; 1=pharmacies
        private int searchType;
        // If there is something written in search field
        private bool researchRunning;
        // Search Thread and cancellation token to cancel thread.
        private Task currentSearch;
        private CancellationTokenSource cancellation = new CancellationTokenSource();

        // Layout to place as results.Content during loading
        private StackLayout LoadingLayout;

        /// <summary>
        /// This is the Search Page to search for a specific medicine or pharmacy. 
        /// You shouldn't need to call the constructor outside the TabbedPage
        /// </summary>
        public SearchPage() {

            InitializeComponent();

            // SEGMENTED CONTROL
            SegmentControl.ValueChanged += SegmentControl_ValueChanged;

            // SEARCH
            searchType = 0; // default to "medicine" - the base position of SegmentedControl
            researchRunning = false; // self-explanatory
            currentSearch = null;

            // add eventlisteners
            search.SearchButtonPressed += Search_SearchButtonPressed; 
            search.TextChanged += Search_TextChanged;

            // CREATE LOADING LAYOUT
            LoadingLayout = new StackLayout {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.StartAndExpand
            };
            ActivityIndicator indicator = new ActivityIndicator {
                Color = Color.Blue,
                IsRunning = true
            };
            LoadingLayout.Children.Add(indicator);

        }

        
        /// <summary>
        /// Runs a search through our database and displays results
        /// </summary>
        private void MakeSearch() {
            
            // MakeSearch shouldn't be called if search field is empty, but check again here for double safety.
            if (search.Text == "") return;

            // Cancel search if already in progress
            if (currentSearch != null && currentSearch.Status == TaskStatus.Running) {
                cancellation.Cancel();
            }

            // Set content to loading
            results.Content = LoadingLayout;

            if (searchType == 1) { // pharmacies

                // Do query and create thread to wait for result
                currentSearch = new Task(async delegate {

                    Debug.WriteLine("[DEBUG][Task:Search] Entering search task (async)");

                    // Starting Server Query
                    var query = App.API.QueryPharmacyAsync(QueryCreator.QueryNameLike(search.Text));
                    Debug.WriteLine("[DEBUG][Task:Search] Started task - server query");

                    // Getting Location
                    bool locationAvailable = CrossGeolocator.IsSupported && CrossGeolocator.Current.IsGeolocationAvailable && CrossGeolocator.Current.IsGeolocationEnabled;

                    double MyLat = 0;
                    double MyLong = 0;

                    // Retrieving position from GPS using CrossGeolocator Plugin. 
                    if (locationAvailable) {
                        Debug.WriteLine("[DEBUG][Task:Search] Attempting to retrieve position from [CrossGeolocator]");
                        try {
                            var position = await CrossGeolocator.Current.GetPositionAsync(TimeSpan.FromSeconds(5));

                            if (position == null) {
                                Debug.WriteLine("[DEBUG][Task:Search] Timed out loading GPS from [CrossGeolocator]");
                                locationAvailable = false;
                            }
                            else {
                                Debug.WriteLine("[DEBUG][Task:Search] Successfully loaded position from [CrossGeolocator]");
                                MyLat = position.Latitude;
                                MyLong = position.Longitude;
                            }
                            
                        }
                        catch (Exception e) {
                            // Code also ends here if GPS is not enabled in Genymotion. Why not caught in locationAvailable check ? 
                            Debug.WriteLine("[ERROR][Task:Search] An error occured while retrieving position. Continuing without position info.");
                            Debug.WriteLine(e.StackTrace);

                            locationAvailable = false;
                        }
                    }

                    Pharmacy[] searchResult = await query;
                    Debug.WriteLine("[DEBUG][Task:Search] Query finished successfully. Starting Phase 2");

                    var resultsLayout = new StackLayout();
                    foreach (Pharmacy result in searchResult) {

                        decimal distance;
                        if (locationAvailable) {
                            // Haversin formula - result in meters
                            distance = (decimal) DistanceUtils.ComputeDistance(MyLat, MyLong, result.Latitude, result.Longitude);
                        }
                        else distance = -1;

                        SearchResult r = new SearchResult {
                            Instance = result,
                            title = result.Name,
                            description = result.Address,
                            distance = distance,
                            icon = new Image { Source = "apotheke_logo.png" }
                        };

                        if (cancellation.IsCancellationRequested) {
                            return;
                        }

                        resultsLayout.Children.Add(RenderSearchResult(r));

                    }

                    Device.BeginInvokeOnMainThread(delegate { results.Content = resultsLayout; });

                }, cancellation);
                currentSearch.Start();
               
            }
            else {

                currentSearch = new Task(async delegate {

                    // Server Query
                    Medicine[] searchResults = await App.API.QueryMedicineAsync(QueryCreator.QueryNameLike(search.Text));

                    var resultsLayout = new StackLayout();
                    foreach (Medicine result in searchResults) {

                        // Create Image item
                        Image img;
                        if (result.Image == null) {
                            img = new Image { Source = "empty.png" };
                        }
                        else {
                            img = new Image { Source = ImageSource.FromStream(() => new MemoryStream(result.Image)) };
                        }

                        SearchResult r = new SearchResult {
                            Instance = result,
                            title = result.Name,
                            description = result.Description,
                            distance = result.Price,
                            icon = img
                        };

                        if (cancellation.IsCancellationRequested) {
                            return;
                        }

                        resultsLayout.Children.Add(RenderSearchResult(r));
                        Device.BeginInvokeOnMainThread(delegate { results.Content = resultsLayout; });

                    }

                }, cancellation);
                currentSearch.Start();

            }

            researchRunning = true;

        }

        /// <summary>
        /// Returns a ready-to-display Layout for the Search Result.
        /// </summary>
        /// <param name="r">The SearchResult you want to display</param>
        /// <returns></returns>
        private Grid RenderSearchResult (SearchResult r) {

            Label title = new Label { Text = r.title, FontAttributes = FontAttributes.Bold };
            Label description = new Label { Text = r.description, FontAttributes = FontAttributes.Italic };
            Label distance;
            if(searchType == 0) {
                // The "distance" is actually the price in this case, so we add a € symbol.
                distance = new Label { Text = ((Medicine) r.Instance).GetPriceText(), TextColor = Color.Blue };
            }
            else {
                // Convert distance to KM if greater than 1000m. 
                string distanceText = DistanceUtils.GetDistanceText((double) r.distance);
                distance = new Label { Text = distanceText, TextColor = Color.Blue };
            }
            

            Grid layout = new Grid {
                ColumnDefinitions = {
                    new ColumnDefinition {Width = new GridLength(100, GridUnitType.Absolute)},
                    new ColumnDefinition {Width = GridLength.Auto}
                }
            };
            layout.Children.Add(r.icon, 0, 1, 1, 4);
            layout.Children.Add(title, 1, 1);
            layout.Children.Add(description, 1, 2);
            layout.Children.Add(distance, 1, 3);

            TapGestureRecognizer gesture = new TapGestureRecognizer();
            gesture.Tapped += delegate {
                // Opens pharmacy detail page in case 1, or medicine detail page in case 0.
                DrawDetailsPage(r);
            };

            layout.GestureRecognizers.Add(gesture);
            
            return layout;
       
        }

        /// <summary>
        /// Private method to open up child page (detail for either medicine or pharmacy)
        /// </summary>
        /// <param name="r"></param>
        private void DrawDetailsPage(SearchResult r) {
            // CALLED WHEN SOME SEARCHRESULT IS TOUCHED
            if (searchType == 0) { // medicine
                Navigation.PushAsync(new DetailsPages.MedicineDetailPage((Medicine) r.Instance));
            }
            else { // pharmacy
                Navigation.PushAsync(new DetailsPages.PharmacyDetailPage((Pharmacy) r.Instance, (double) r.distance));
            }
        }

        /// <summary>
        /// Draws the recommendation view (called when nothing is searched)
        /// Features : for Pharmacies : Search History and shortcut to Home Pharmacy
        ///            for Medicines : Search History and Favorites
        /// Async because of GPS data, but should be fast because no HTTP request is done.
        /// </summary>
        private async void DrawRecommendationView() {

            var layout = new StackLayout {
                Margin = 30
            };

            Label title1 = new Label() {
                Text = AppResources.RecommendationText,
                FontSize = 25,
                FontAttributes = FontAttributes.Bold
            };
            layout.Children.Add(title1);

            ScrollView history = new ScrollView { Orientation = ScrollOrientation.Horizontal };
            StackLayout history_layout = new StackLayout { Orientation = StackOrientation.Horizontal };

            if (searchType == 0) {

                // todo foreach Medicine in MedicineHistory
                for (int i = 0; i < 4; i++) {
                    StackLayout element = new StackLayout();
                    Image icon = new Image { Source = "empty.png", HeightRequest = 100 };
                    Label name = new Label { Text = "MedicineItem " + i, HorizontalTextAlignment = TextAlignment.Center };
                    Label available = new Label { Text = "Available", TextColor = Color.Green, HorizontalTextAlignment = TextAlignment.Center };
                    element.Children.Add(icon);
                    element.Children.Add(name);
                    element.Children.Add(available);
                    history_layout.Children.Add(element);
                }

                history.Content = history_layout;
                layout.Children.Add(history);

                Label title2 = new Label() {
                    Text = AppResources.FavoritesText,
                    FontSize = 25,
                    FontAttributes = FontAttributes.Bold,
                    Margin = new Thickness(0, 30, 0, 0)
                };
                layout.Children.Add(title2);

                ScrollView favorites = new ScrollView { Orientation = ScrollOrientation.Horizontal };
                StackLayout favorites_layout = new StackLayout { Orientation = StackOrientation.Horizontal };

                // todo foreach in favorites
                for (int i = 0; i < 4; i++) {
                    StackLayout element = new StackLayout();
                    Image icon = new Image { Source = "empty.png", HeightRequest = 100 };
                    Label name = new Label { Text = "FavoriteItem " + i, HorizontalTextAlignment = TextAlignment.Center };
                    Label available = new Label { Text = "Price", TextColor = Color.Blue, HorizontalTextAlignment = TextAlignment.Center };
                    element.Children.Add(icon);
                    element.Children.Add(name);
                    element.Children.Add(available);
                    favorites_layout.Children.Add(element);
                }

                favorites.Content = favorites_layout;
                layout.Children.Add(favorites);

            }
            else {

                // todo foreach Pharmacy in PharmacyHistory
                for (int i = 0; i < 4; i++) {
                    StackLayout element = new StackLayout();
                    Image icon = new Image { Source = "empty.png", HeightRequest = 100 };
                    Label name = new Label { Text = "PharmacyItem " + i, HorizontalTextAlignment = TextAlignment.Center };
                    Label available = new Label { Text = "Distance : ", TextColor = Color.Blue, HorizontalTextAlignment = TextAlignment.Center };
                    element.Children.Add(icon);
                    element.Children.Add(name);
                    element.Children.Add(available);
                    history_layout.Children.Add(element);
                }

                history.Content = history_layout;
                layout.Children.Add(history);

                Label title2 = new Label() {
                    Text = AppResources.YourHomePharmacyText,
                    FontSize = 25,
                    FontAttributes = FontAttributes.Bold,
                    Margin = new Thickness(0, 30, 0, 0)
                };
                layout.Children.Add(title2);

                // Get GPS coords
                var position = await DistanceUtils.GetLocation();

                var home_pharmacy = new SearchResult {
                    description = App.HomePharmacy.Address,
                    distance = (decimal) DistanceUtils.ComputeDistance(position.Latitude, position.Longitude, App.HomePharmacy.Latitude, App.HomePharmacy.Longitude),
                    icon = new Image { Source = "apotheke_logo.png"},
                    Instance = App.HomePharmacy,
                    title = App.HomePharmacy.Name
                };

                var grid = RenderSearchResult(home_pharmacy);
                layout.Children.Add(grid);  

            }

            Device.BeginInvokeOnMainThread(delegate {
                results.Content = layout;
            });

        }
        
        // EVENT LISTENERS
        private void Search_SearchButtonPressed(object sender, EventArgs e) {
            MakeSearch();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e) {

            // Clear results if search field is empty.
            if (e.NewTextValue == "") {
                results.Content = new StackLayout();
                DrawRecommendationView();
                researchRunning = false;
            }

        }

        // Keep searchType value up-to-date and redo search if changed while typing something
        private void SegmentControl_ValueChanged(object sender, SegmentedControl.FormsPlugin.Abstractions.ValueChangedEventArgs e) {
            searchType = e.NewValue;
            if (researchRunning) {
                MakeSearch();
            }
            else {
                DrawRecommendationView();
            }
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            DrawRecommendationView();
        }

    }

    /// <summary>
    /// A simple struct to store the required informations of a search result in database for further use.
    /// </summary>
    struct SearchResult {

        // Instance of Pharmacy / Medicine
        public object Instance;

        // Name of the pharmacy / medicine
        public string title;
        // Address of pharmacy / descrption of medicine
        public string description;
        // Distance to pharmacy / price of medicine - already formatted as string
        public decimal distance;
        // Icon of pharmacy / image of medicine
        public Image icon;

    }
}
