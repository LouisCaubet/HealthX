using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HealthX.Pages {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MePage : ContentPage {

        /*
         * This page will display following :
         * - Infos about user account, buttons to change password, etc
         * - Calendar, button to add medicine to take
         * - Buy credits ?
         * - 
         */

        public MePage() {
            InitializeComponent();
        }
    }
}