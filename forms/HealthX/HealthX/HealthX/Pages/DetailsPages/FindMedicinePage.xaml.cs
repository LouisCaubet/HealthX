using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using HealthX.Datatypes;
using HealthX.Backend;
using System.IO;
using HealthX.Braintree;
using HealthX.Localization;

namespace HealthX.Pages.DetailsPages {

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FindMedicinePage : ContentPage {

        // Pharmacy where search is being done.
        Pharmacy pharmacy;

        /// <summary>
        /// Constructor for FindMedicine Page. Draws pages default content
        /// </summary>
        /// <param name="pharmacy">The pharmacy where user is searching</param>
        public FindMedicinePage(Pharmacy pharmacy) {

            InitializeComponent();
            this.pharmacy = pharmacy;
            pharmacy_name.Text = pharmacy.Name;

            // Listening to search bar
            search_bar.TextChanged += Search_bar_TextChanged;
            search_bar.SearchButtonPressed += Search_bar_SearchButtonPressed;

            // Generate default recommendation scrollview
            DrawRecommendationView();
            

        }

        // LISTENERS
        private void Search_bar_SearchButtonPressed(object sender, EventArgs e) {
            MakeSearch(); // when Search keyboard button is pressed on search_bar. 
        }

        private void Search_bar_TextChanged(object sender, TextChangedEventArgs e) {
            
            // Draw recommendation if user deleted the whole text of the SearchBar.
            if (search_bar.Text == "") {
                DrawRecommendationView();
            }

        }

        /// <summary>
        /// Method drawing the default recommendation view.
        /// TODO - replace information loading and make things asynchronous.
        /// </summary>
        public void DrawRecommendationView () {

            // Root layout for recommendation view
            var layout = new StackLayout {
                Margin = 30
            };

            // "You may be looking for..." title. BTW, I know my variable names are shitty.
            Label title1 = new Label() {
                Text = AppResources.RecommendationText,
                FontSize = 25,
                FontAttributes = FontAttributes.Bold
            };
            layout.Children.Add(title1);

            ScrollView recom1 = new ScrollView { Orientation=ScrollOrientation.Horizontal };
            StackLayout recom1_layout = new StackLayout { Orientation = StackOrientation.Horizontal };

            // Debug code generating empty fields for recommendations. 
            // TODO - replace this with last entries in history, or more advanced recommendation system.
            for (int i=0; i<4; i++) {
                StackLayout element = new StackLayout();
                Image icon = new Image { Source = "empty.png", HeightRequest=100 };
                Label name = new Label { Text = "MedicineItem " + i, HorizontalTextAlignment=TextAlignment.Center };
                Label available = new Label { Text = "Available", TextColor = Color.Green, HorizontalTextAlignment = TextAlignment.Center };
                element.Children.Add(icon);
                element.Children.Add(name);
                element.Children.Add(available);
                recom1_layout.Children.Add(element);
            }

            // adding views to layout.
            recom1.Content = recom1_layout;
            layout.Children.Add(recom1);

            // "Your favorites" title. Still shitty variable name.
            Label title2 = new Label() {
                Text = AppResources.FavoritesText,
                FontSize = 25,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 30, 0, 0)
            };
            layout.Children.Add(title2);

            ScrollView recom2 = new ScrollView { Orientation = ScrollOrientation.Horizontal };
            StackLayout recom2_layout = new StackLayout { Orientation = StackOrientation.Horizontal };

            // Debug code generating empty fields for recommendations. 
            // TODO - replace this with user favorites.
            for (int i = 0; i < 4; i++) {
                StackLayout element = new StackLayout();
                Image icon = new Image { Source = "empty.png", HeightRequest = 100 };
                Label name = new Label { Text = "FavoriteItem " + i, HorizontalTextAlignment = TextAlignment.Center };
                Label available = new Label { Text = "Available", TextColor = Color.Green, HorizontalTextAlignment = TextAlignment.Center };
                element.Children.Add(icon);
                element.Children.Add(name);
                element.Children.Add(available);
                recom2_layout.Children.Add(element);
            }

            recom2.Content = recom2_layout;
            layout.Children.Add(recom2);

            // add layout to the page.
            result_view.Content = layout;


        }

        /// <summary>
        /// Returns a ready-to-display Result Layout
        /// </summary>
        /// <param name="r">The Medicine to render</param>
        /// <returns></returns>
        private Grid RenderSearchResult(Medicine r) {

            // Convert r's fields to xamarin forms views.
            Label title = new Label { Text = r.Name, FontAttributes = FontAttributes.Bold };
            Label description = new Label { Text = r.Availability.GetTextShort(), TextColor=r.Availability.GetColor() };
            Label distance = new Label { Text = r.GetPriceText(), TextColor = Color.Blue };
            Button buy = new Button { Text = r.Availability.GetButtonText(), BackgroundColor = r.Availability.GetButtonColor(), CornerRadius = 5 };

            // If the medicine is not available, disable the "BUY" button.
            if (r.Availability == AvailabilityState.NOT_AVAILABLE) {
                buy.IsEnabled = false;
            }

            // 3-column layout for | IMAGE | TITLE&AVAILABILITY | BUY BUTTON |
            Grid layout = new Grid {
                ColumnDefinitions = {
                    new ColumnDefinition {Width = GridLength.Auto},
                    new ColumnDefinition {Width = GridLength.Star},
                    new ColumnDefinition {Width = GridLength.Auto}
                }
            };

            Image img;
            if (r.Image == null) {
                img = new Image { Source = "empty.png" }; // if no image was loaded for database, use default image.
            }
            else {
                // Convert byte[] to xamarin.forms image.
                img = new Image { Source = ImageSource.FromStream(() => new MemoryStream(r.Image)) };
            }

            // Adding everything to the grid layout.
            layout.Children.Add(img, 0, 1, 0, 3);
            layout.Children.Add(title, 1, 3, 0, 1);
            layout.Children.Add(description, 1, 1);
            layout.Children.Add(distance, 1, 2);
            layout.Children.Add(buy, 2, 3, 1, 3);

            // Call a new reservation handler when button is pressed.
            buy.Pressed += delegate {
                if (r.Availability != AvailabilityState.NOT_AVAILABLE) { // redundant check
                    ReservationHandler reservationHandler = new ReservationHandler(this, r, pharmacy, r.Availability, r.Storage.Price);
                }
            };

            // Draw medicine details page when result is pressed.
            TapGestureRecognizer gesture = new TapGestureRecognizer();
            gesture.Tapped += delegate {
                DrawDetailsPage(r);
            };

            layout.GestureRecognizers.Add(gesture);

            return layout;

        }

        /// <summary>
        /// Makes search in database and displays results. See RenderSearchResult.
        /// </summary>
        private void MakeSearch() {

            // Retrieve search text from SearchBar.
            string text = search_bar.Text;

            // Run search task asynchronously.
            new Task(async delegate {

                // query medicines with a name similar to the search.
                var query = App.API.QueryMedicineAsync(QueryCreator.QueryNameLike(text));
                Medicine[] searchResult = await query;

                // result layout.
                StackLayout layout = new StackLayout {
                    Margin = 30
                };

                foreach (Medicine med in searchResult) {

                    // Query availability data for this medicine.
                    StorageRow[] available = await App.API.QueryStorageAsync(pharmacy.Id, QueryCreator.QueryIdEquals(med.Id));
                    if (available.Length == 0) {
                        med.Availability = AvailabilityState.NOT_AVAILABLE;
                    }
                    else {
                        med.Availability = available[0].IsAvailable();
                        med.Storage = available[0];
                    }

                    Grid rendered = RenderSearchResult(med);
                    layout.Children.Add(rendered);
                }

                // Adding layout to the page must be run synchronously.
                Device.BeginInvokeOnMainThread(delegate {
                    result_view.Content = layout;
                });

            }).Start();

        }

        /// <summary>
        /// Draws a Medicine Detail Page for chosen Medicine.
        /// </summary>
        /// <param name="r">The chosen medicine</param>
        private void DrawDetailsPage (Medicine r) {
            Navigation.PushAsync(new MedicineDetailPage(r, pharmacy, r.Storage));
        }

    }


}