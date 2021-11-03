using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Maps;
using HealthX.Utils;
using HealthX.Datatypes;
using System.Diagnostics;

namespace HealthX.Pages {

    /*
     * This page contains a Map displaying nearby pharmacies.
     * Refresh on move is not fully operational. TODO
     */

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage {

        // The Map object, defined below.
        CustomMap Map;

        // The currently displayed region. 
        MapSpan lastVisibleRegion;

        public MapPage() {

            // Doesn't do much, as there is nothing built in XAML. 
            InitializeComponent();

            Debug.WriteLine("[INFO][Map] Map Page loading started...");

            // MAP INIT - centering on user done in LoadMap.
            Map = new CustomMap(MapSpan.FromCenterAndRadius(new Position(48.1373932, 11.5732598), new Distance(5000))) {
                IsShowingUser = true,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            Debug.WriteLine("[DEBUG][Map] Map object initialized.");

            // Root layout, only contains the Map UI Element.
            StackLayout stack = new StackLayout { Spacing = 0 };
            stack.Children.Add(Map);
            Content = stack;

            // Refresh map every second when focused.
            // See On Map move.
            Device.StartTimer(TimeSpan.FromSeconds(1), () => {
                new Task(delegate {
                    if(this.IsFocused) OnMapMove();
                }).Start();
                return true;
            });

        }

        /// <summary>
        /// Method handling icon refresh when the user moves the map.
        /// </summary>
        public void OnMapMove () {
            try {
                // If the user actually moved the map. 
                if (Map.VisibleRegion != lastVisibleRegion && Map.VisibleRegion.Radius.Kilometers <= 20) {
                    // reload displayed pharmacies on the map
                    var v = LoadMap(Map.VisibleRegion);
                    // save new visible region
                    lastVisibleRegion = Map.VisibleRegion;
                }
            }
            catch (Exception) {
                // This happens when this part of code is called too early in the app loading cycle
                // NullReferenceException
            }
        }

        /// <summary>
        /// Method loading pharmacies around user and displaying pins.
        /// </summary>
        public async void LoadMap() {

            // Get current user position
            var position1 = await DistanceUtils.GetLocation();
            var position = new Position(position1.Latitude, position1.Longitude);
            Debug.WriteLine("[INFO][Map] Got user position.");

            // Center map on position
            try {
                Map.MoveToRegion(MapSpan.FromCenterAndRadius(position, new Distance(300)));
                Debug.WriteLine("[DEBUG][Map] Map Span adjusted.");
                // Task loading nearby pharmacies and displaying pins.
                await LoadMap(Map.VisibleRegion);
                Debug.WriteLine("[INFO][Map] Map loading complete. Referencing event handlers...");
            }
            catch(Exception e) {
                Debug.WriteLine(e.Message + "\n" + e.StackTrace);
            }
            
          

        }

        /// <summary>
        /// Task getting pharmacies in visible area, and adding pins to the Map. 
        /// </summary>
        /// <param name="VisibleArea">The region currently displayed by the Map.</param>
        /// <returns></returns>
        public async Task LoadMap (MapSpan VisibleArea) {

            // Get pharmacies in radius
            Debug.WriteLine("[INFO][Map] Getting pharmacies to display from LocalDB");
            Pharmacy[] pharmacies = await App.LocalDB.GetInRadius(VisibleArea.Center.Latitude, VisibleArea.Center.Longitude, VisibleArea.Radius.Kilometers);
            Debug.WriteLine("[INFO][Map] Got pharmacies to display. " + pharmacies.Length + " pins to render");

            // Create pins for each pharmacy
            Device.BeginInvokeOnMainThread(delegate {
                foreach (Pharmacy pharm in pharmacies) {
                    CustomPin pin = new CustomPin {
                        Position = new Position(pharm.Latitude, pharm.Longitude),
                        Type = PinType.Generic,
                        Pharmacy = pharm,
                        Label = pharm.Name,
                        Address = pharm.Address
                    };
                    Map.CustomPins.Add(pin);
                    Map.Pins.Add(pin);
                }
                Debug.WriteLine("[INFO][Map] Pin rendering (PCL) completed");
            });
            
        }

    }

    /// <summary>
    /// Represents our custom implementation of map, containing pins as our custom pin implementation.
    /// </summary>
    public class CustomMap : Map {

        public CustomMap (MapSpan span) : base(span) {
            CustomPins = new List<CustomPin>();
        }

        public List<CustomPin> CustomPins { get; set; }
    }

    /// <summary>
    /// Our custom implementation of Pin, containing an additional field Pharmacy, to be able to redirect to it
    /// when clicked.
    /// </summary>
    public class CustomPin : Pin {
        public Pharmacy Pharmacy { get; set; }
    }

}