using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Maps.Android;
using System.ComponentModel;
using Android.Gms.Maps;
using Xamarin.Forms;
using HealthX.Pages;
using HealthX.Droid.Renderers;
using Android.Gms.Maps.Model;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Platform.Android;
using HealthX.Utils;
using HealthX.Localization;
using Android.Graphics.Drawables;
using Android.Graphics;
using HealthX.Pages.DetailsPages;

[assembly: ExportRenderer(typeof(CustomMap), typeof(CustomMapRenderer))]
namespace HealthX.Droid.Renderers {
    public class CustomMapRenderer : MapRenderer {

        /*
         * This renderer handles platform-dependent Map features for ANDROID.
         * Handling:
         *  - Google Maps display
         *  - Pin rendering
         *  - Pin click events.
         */

        // Pins on the Map.
        List<CustomPin> customPins;

        // Required constructor, nothing to do here.
        public CustomMapRenderer(Context context) : base(context) { }

        // Loading GMap here, See Xamarin.Android Maps documentation
        protected override void OnElementChanged(ElementChangedEventArgs<Map> e) {
            base.OnElementChanged(e);

            if (e.OldElement != null) {
                NativeMap.InfoWindowClick -= OnInfoWindowClick;
            }

            if (e.NewElement != null) {
                var formsMap = (CustomMap) e.NewElement;
                customPins = formsMap.CustomPins;

                Control.GetMapAsync(this);
            }

        }

        // Google Maps callback when everything is ready.
        protected override void OnMapReady(GoogleMap map) {

            base.OnMapReady(map);

            // Configure map UI. 
            NativeMap.UiSettings.ZoomGesturesEnabled = true;
            NativeMap.UiSettings.ZoomControlsEnabled = false;

            NativeMap.SetMapStyle(MapStyleOptions.LoadRawResourceStyle(Context, Resource.Raw.MapStyle));
            NativeMap.InfoWindowClick += OnInfoWindowClick;

        }

        // Handle pin redirect to the pharmacy detail page.
        private void OnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e) {

            // Get clicked pin
            CustomPin pin = customPins.Where((CustomPin pin1) => {
                return pin1.Label == e.Marker.Title;
            }).ToList()[0];

            // open pharmacy detail page for Pharmacy attached to the pin.
            new PharmacyDetailPage(pin.Pharmacy);

        }

        /// <summary>
        /// Creates custom marker by replacing the icon of the base one.
        /// </summary>
        /// <param name="pin">The pin to create a marker for.</param>
        /// <returns></returns>
        protected override MarkerOptions CreateMarker(Pin pin) {
            var marker = new MarkerOptions();
            marker.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
            marker.SetSnippet(pin.Address);
            marker.SetTitle(pin.Label);

            BitmapDrawable icon = (BitmapDrawable) Context.Resources.GetDrawable(Resource.Drawable.map_pin, Context.Theme);
            Bitmap bitmap = icon.Bitmap;
            Bitmap small_icon = Bitmap.CreateScaledBitmap(bitmap, 100, 130, true);

            marker.SetIcon(BitmapDescriptorFactory.FromBitmap(small_icon));
            return marker;
        }

    }
}