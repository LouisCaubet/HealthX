using System;
using System.Collections.Generic;

using Xamarin.Forms;
using HealthX.Datatypes;
using HealthX.Localization;

using Plugin.Geolocator;
using System.Diagnostics;
using HealthX.Utils;

namespace HealthX.Pages.DetailsPages {

    /*
     * This class renders the Pharmacy Detail Page, containing info about pharmacy, buttons to search medicine, etc
     * We can consider displaying an advertisement on the bottom, the whole screen is not used by buttons for now 
     * STILL TODO :
     * - Asynchronize loading phases (partly done)
     */

    public class PharmacyDetailPage : ContentPage {

        // The header of the page, displaying useful informations about the pharmacy
        Grid header;

        // The instance of the pharmacy being displayed.
        Pharmacy pharmacy;

        // Need to place it here to change settings on appearing
        Image itinerary;

        /// <summary>
        /// Constructor to create the page for the given pharmacy.
        /// </summary>
        /// <param name="pharmacy">The pharmacy to draw the detail page of.</param>
        /// <param name="distance">The distance from the user to the pharmacy.</param>
        public PharmacyDetailPage(Pharmacy pharmacy, double distance = -2) {

            // Copy instance to object field.
            this.pharmacy = pharmacy;

            // If no distance information was passed, compute it here.
            if (distance == -2) {
                // Get current Location
                double MyLat = 0;
                double MyLong = 0;

                if (CrossGeolocator.IsSupported && CrossGeolocator.Current.IsGeolocationAvailable && CrossGeolocator.Current.IsGeolocationEnabled) {
                    Debug.WriteLine("[DEBUG][UITask] Attempting to retrieve position from [CrossGeolocator]");
                    try {
                        // Retrieve current location from the Geolocator Plugin
                        var position = CrossGeolocator.Current.GetPositionAsync(new TimeSpan(1000)).Result;
                        Debug.WriteLine("[DEBUG][UITask] Successfully loaded position from [CrossGeolocator]");

                        MyLat = position.Latitude;
                        MyLong = position.Longitude;

                        // Compute distance with Haversin Formula.
                        distance = DistanceUtils.ComputeDistance(MyLat, MyLong, pharmacy.Latitude, pharmacy.Longitude);
                    }
                    catch (Exception e) {
                        Debug.WriteLine("[ERROR][UITask] An error occured while retrieving position. Continuing without position info.");
                        Debug.WriteLine(e.StackTrace);
                        distance = -1; // means that we couldn't compute distance
                    }

                }
                else {
                    distance = -1;
                }

            }

            //// BUILD PAGE

            // Set Page title
            Title = pharmacy.Name;

            // this is the root layout.
            StackLayout stack_layout = new StackLayout();

            Label title = new Label { Text = pharmacy.Name, FontAttributes = FontAttributes.Bold, FontSize=30, LineBreakMode=LineBreakMode.TailTruncation };
            Label address = new Label { Text = pharmacy.Address, FontAttributes = FontAttributes.Italic };

            // compute distance from target done either in previous view or sooner in this one
            Label distanceLabel = new Label { Text = DistanceUtils.GetDistanceText(distance), TextColor = Color.Blue };
            Image icon = new Image { Source = "apotheke_logo.png", Margin=5 };

            // Get Opening Hours from Pharmacy object and display.
            string openingHoursText = ParsingUtils.GetOpeningHoursText(pharmacy.OpeningHours);
            Color openingHoursColor = ParsingUtils.GetOpeningTextColor(openingHoursText);
            Label opening_hours = new Label { Text = openingHoursText, TextColor=openingHoursColor };

            // Button that opens Google Maps / Apple Maps with itinerary to destination.
            itinerary = new Image {
                Source = "itinerary_blue.png",
                WidthRequest = 35
            };

            TapGestureRecognizer itinerary_tapped = new TapGestureRecognizer();

            // TODO links dont work !!
            // For Android we should create dependency service with Intent to open GMaps
            // For iOS links are supposed to work
            itinerary_tapped.Tapped += delegate {
                // Open google maps
                if (Device.RuntimePlatform == Device.Android) {
                    Device.OpenUri(new Uri("https://www.google.com/maps/search/?api=1&destination=" + pharmacy.Latitude + "," + pharmacy.Longitude));
                }
                else if (Device.RuntimePlatform == Device.iOS) {
                    Device.OpenUri(new Uri("http://maps.apple.com/?daddr=" + pharmacy.Address + "&dirflg=w&t=m"));
                }
            };

            itinerary.GestureRecognizers.Add(itinerary_tapped);


            //// CREATE AND FILL HEADER WITH CREATED UI ELEMENTS

            header = new Grid {
                ColumnDefinitions = {
                    new ColumnDefinition {Width=new GridLength(100, GridUnitType.Absolute)},
                    new ColumnDefinition {Width=GridLength.Star},
                    new ColumnDefinition {Width=new GridLength(50, GridUnitType.Absolute)},
                    new ColumnDefinition {Width=new GridLength(50, GridUnitType.Absolute)}
                },
                RowDefinitions = {
                    new RowDefinition {Height=GridLength.Auto},
                    new RowDefinition {Height=GridLength.Auto},
                    new RowDefinition {Height=GridLength.Auto},
                    new RowDefinition {Height=GridLength.Auto}
                }
            };

            header.Padding = 10;

            header.Children.Add(icon, 0, 1, 0, 4);
            header.Children.Add(title, 1, 4, 0, 1);
            header.Children.Add(address, 1, 3, 1, 2);
            header.Children.Add(distanceLabel, 1, 2);
            header.Children.Add(opening_hours, 1, 3);

            header.Children.Add(itinerary, 2, 4, 2, 4);

            stack_layout.Children.Add(header);

            //// CREATE MENU FOR PHARMACY
            // - Find Medicine
            // - Order from Prescription
            // - More Infos

            // About ListView, see Xamarin documentation.

            // ListItem is the element of ListView, with image and title. 
            // The conversion to ListCell is done in listViewTemplate
            List<ListItem> items = new List<ListItem> {
                new ListItem { Name = AppResources.FindMedicineTitle, ImagePath = "find_medicine.png" },
                new ListItem { Name = AppResources.OrderFromPrescriptionText, ImagePath = "command_picture.png" },
                new ListItem { Name = AppResources.InfosText, ImagePath = "info_icon.png" }
            };

            ListView listView = new ListView {

                ItemsSource = items,
                ItemTemplate = new DataTemplate(listViewTemplate)

            };

            listView.ItemTapped += OnItemTapped;
            

            stack_layout.Children.Add(listView);

            Content = stack_layout;
        }

        /// <summary>
        /// Handle user menu touch. This is called by Xamarin.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemTapped(object sender, ItemTappedEventArgs e) {

            // remove orange highlight on selected item.
            ((ListView)sender).SelectedItem = null;

            // Determine tapped item and do associated tasks.
            ListItem tapped = (ListItem) e.Item;
            if(tapped.Name == AppResources.FindMedicineTitle) {
                // Open the Find Medicine page
                Navigation.PushAsync(new FindMedicinePage(pharmacy));
            }
            else if (tapped.Name == AppResources.InfosText) {
                // Open the More Infos page.
                Navigation.PushAsync(new PharmacyInfoPage(pharmacy));
            }
            else if (tapped.Name == AppResources.OrderFromPrescriptionText) {

                // Start Platform-dependent camera handler
                ICameraHandler camera = DependencyService.Get<ICameraHandler>();
                camera.SetCompletionAction(PrescriptionPictureTaken);
                try {
                    camera.TakePicture();
                }
                catch(Exception) {
                    DisplayAlert("Can't use this on your device", "No camera is available on your device.", "OK");
                }

            }

        }

        /// <summary>
        /// Callback for platform-dependent camera handler.
        /// </summary>
        /// <param name="status">1 = operation cancelled, else success.</param>
        /// <param name="picture">the picture that has just been taken.</param>
        public void PrescriptionPictureTaken(int status, byte[] picture) {
            // start new view to pay & send reservation

            if(status == 1) {
                // DisplayAlert("DEBUG", "Camera cancelled by user", "OK");
            }
            else {
                // Display a new Order from Image confirmation page.
                Navigation.PushAsync(new OrderFromImagePage(picture, pharmacy));
            }

        }

        /// <summary>
        /// Template to create ListView items.
        /// </summary>
        /// <returns></returns>
        private ViewCell listViewTemplate() {

            // The icon of the menu item
            Image image = new Image();
            image.SetBinding(Image.SourceProperty, "ImagePath");
            image.Margin = 7;

            // The arrow at the right of the menu item
            Image arrow = new Image { Source = "arrow_listmenu.png" };
            arrow.Margin = new Thickness(0, 10);

            // The title of the menu item
            Label title = new Label {
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalTextAlignment = TextAlignment.Center,
                FontSize = 18,
                LineBreakMode = LineBreakMode.TailTruncation,

                Margin = new Thickness(20, 0, 0, 0)
            };

            // bind to ListItem field
            title.SetBinding(Label.TextProperty, "Name");

            // Layout for these elements
            var layout = new RelativeLayout {
                Margin = new Thickness(Application.Current.MainPage.Width / 20, 0)
            };

            // Place the icon relative to the layout
            layout.Children.Add(image, Constraint.RelativeToParent((parent) => {
                return 0;
            }));

            // place the title relative to the image.
            layout.Children.Add(title, Constraint.RelativeToView(image, (parent, child) => {
                return child.X + child.Width;
            }),
            Constraint.RelativeToParent((parent) => {
                return 0;
            }),
            Constraint.RelativeToParent((parent) => {
                return parent.Width;
            }),
            Constraint.RelativeToParent((parent) => {
                return parent.Height;
            }));

            // place the arrow relative to the parent.
            layout.Children.Add(arrow, Constraint.RelativeToParent((parent) => {
                return parent.Width * 0.9;
            }));

            // return the ViewCell.
            return new ViewCell { View = layout };

        }

        /// <summary>
        /// Called when page is rendered by engine.
        /// </summary>
        protected override void OnAppearing() {
            base.OnAppearing();

            // Set these sizes relative to the screen size.
            // This must be done here, as screen size is not available earlier.
            header.ColumnDefinitions[0].Width = this.Width / 5;
            itinerary.WidthRequest = this.Width / 10;

        }

    }

    // Struct containing information to create the Menu ViewCell.
    struct ListItem {
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public Page Child { get; set; }
    }

}