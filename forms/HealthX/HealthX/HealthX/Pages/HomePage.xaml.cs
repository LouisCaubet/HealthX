using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HealthX.Pages {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage {

        /* 
         * HomePage will display following :
         * - Info about the app's evolution, news, etc
         * - User's next medicines to take
         * - Shortcut for favorites & home pharmacy
         * - Maybe other.
         */

        public HomePage() {
            InitializeComponent();
        }
    }
}