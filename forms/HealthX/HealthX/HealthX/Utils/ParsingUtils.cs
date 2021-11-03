using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using HealthX.Localization;

namespace HealthX.Utils {

    public static class ParsingUtils {

        public static string GetOpeningHoursText(string openingHours) {

            DayOfWeek today = DateTime.UtcNow.DayOfWeek;
            int hour = DateTime.UtcNow.Hour;
            int minute = DateTime.UtcNow.Minute;

            int openingHour;
            int openingMinute;

            int closingHour;
            int closingMinute;

            if(today == DayOfWeek.Monday) {

                openingHour = Int32.Parse((openingHours[4].ToString() + openingHours[5].ToString()));
                openingMinute = Int32.Parse(openingHours[7].ToString() + openingHours[8].ToString());

                closingHour = Int32.Parse(openingHours[10].ToString() + openingHours[11].ToString());
                closingMinute = Int32.Parse(openingHours[13].ToString() + openingHours[14].ToString());

            }
            else if (today == DayOfWeek.Tuesday) {

                openingHour = Int32.Parse((openingHours[20].ToString() + openingHours[21].ToString()));
                openingMinute = Int32.Parse(openingHours[23].ToString() + openingHours[24].ToString());

                closingHour = Int32.Parse(openingHours[26].ToString() + openingHours[27].ToString());
                closingMinute = Int32.Parse(openingHours[29].ToString() + openingHours[30].ToString());

            }
            else if (today == DayOfWeek.Wednesday) {

                openingHour = Int32.Parse((openingHours[36].ToString() + openingHours[37].ToString()));
                openingMinute = Int32.Parse(openingHours[39].ToString() + openingHours[40].ToString());

                closingHour = Int32.Parse(openingHours[42].ToString() + openingHours[43].ToString());
                closingMinute = Int32.Parse(openingHours[45].ToString() + openingHours[46].ToString());

            }
            else if (today == DayOfWeek.Thursday) {

                openingHour = Int32.Parse((openingHours[52].ToString() + openingHours[53].ToString()));
                openingMinute = Int32.Parse(openingHours[55].ToString() + openingHours[56].ToString());

                closingHour = Int32.Parse(openingHours[58].ToString() + openingHours[59].ToString());
                closingMinute = Int32.Parse(openingHours[61].ToString() + openingHours[62].ToString());

            }
            else if (today == DayOfWeek.Friday) {

                openingHour = Int32.Parse((openingHours[68].ToString() + openingHours[69].ToString()));
                openingMinute = Int32.Parse(openingHours[71].ToString() + openingHours[72].ToString());

                closingHour = Int32.Parse(openingHours[74].ToString() + openingHours[75].ToString());
                closingMinute = Int32.Parse(openingHours[77].ToString() + openingHours[78].ToString());

            }
            else if (today == DayOfWeek.Saturday) {

                openingHour = Int32.Parse((openingHours[84].ToString() + openingHours[85].ToString()));
                openingMinute = Int32.Parse(openingHours[87].ToString() + openingHours[88].ToString());

                closingHour = Int32.Parse(openingHours[90].ToString() + openingHours[91].ToString());
                closingMinute = Int32.Parse(openingHours[93].ToString() + openingHours[94].ToString());

            }
            else {

                openingHour = Int32.Parse((openingHours[100].ToString() + openingHours[101].ToString()));
                openingMinute = Int32.Parse(openingHours[103].ToString() + openingHours[104].ToString());

                closingHour = Int32.Parse(openingHours[106].ToString() + openingHours[107].ToString());
                closingMinute = Int32.Parse(openingHours[109].ToString() + openingHours[110].ToString());

            }

            if (hour < openingHour || (hour == openingHour && minute < openingMinute)) {
                // Closed until opening
                return AppResources.ClosedOpeningAt + IntToString(openingHour) + ":" + IntToString(openingMinute);
            }
            else if (hour > closingHour || (hour == closingHour && minute > closingMinute)) {
                return AppResources.ClosedToday;
            }
            else {
                return AppResources.OpenedUntil + IntToString(closingHour) + ":" + IntToString(closingMinute);
            }

        }

        public static Color GetOpeningTextColor(string openingtext) {
            if (openingtext.Contains(AppResources.OpenedUntil)) {
                return Color.Green;
            }
            else {
                return Color.Red;
            }
        }

        private static string IntToString(int integer) {
            string s = integer + "";
            if (s.Length == 1) {
                s = "0" + s;
            }
            return s;
        }

    }

}
