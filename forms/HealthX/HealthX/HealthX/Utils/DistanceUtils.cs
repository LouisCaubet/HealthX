using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthX.Localization;

namespace HealthX.Utils {

    public static class DistanceUtils {

        public async static Task<Position> GetLocation () {

            // Getting Location
            bool locationAvailable = CrossGeolocator.IsSupported && CrossGeolocator.Current.IsGeolocationAvailable && CrossGeolocator.Current.IsGeolocationEnabled;

            // Default location
            var default_position = new Position(48.13, 11.57);

            // Retrieving position from GPS using CrossGeolocator Plugin. 
            if (locationAvailable) {
                try {
                    var position = await CrossGeolocator.Current.GetPositionAsync(TimeSpan.FromSeconds(5));
                    return position;

                }
                catch (Exception) {
                    return default_position;
                }
            }
            else return default_position;

        }

        public static double ComputeDistance (double lat1, double long1, double lat2, double long2) {

            double distance = (6371000 * 2 * Math.Asin(Math.Sqrt(
                                Math.Pow(Math.Sin((lat1 - lat2) * Math.PI / 180 / 2), 2) +
                                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                                Math.Pow(Math.Sin((long1 - long2) * Math.PI / 180 / 2), 2)
                                )));
            distance = Math.Round(distance);

            return distance;

        }

        public static string GetDistanceText (double distance) {

            string unit;
            string convertedDistance;

            if (distance >= 1000) {
                unit = " km";
                convertedDistance = distance / 1000 + "";
            }
            else if (distance == -1) {
                convertedDistance = AppResources.GpsNotAvailableText;
                unit = "";
            }
            else {
                unit = " m";
                convertedDistance = distance + "";
            }

            return convertedDistance + unit;

        }

    }

}
