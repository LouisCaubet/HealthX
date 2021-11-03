using HealthX.Datatypes;
using HealthX.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HealthX.Pages.DetailsPages {

    // LOGIC FOR THE PHARMACY MORE INFO PAGE

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PharmacyInfoPage : ContentPage {

        public PharmacyInfoPage(Pharmacy pharmacy) {

            // Create page from XAML
            InitializeComponent();

            // Set title text to "More Infos : PharmacyName"
            title_label.Text = AppResources.InfosText + ": " + pharmacy.Name;
            // Set Margin of title label (IDK how to do unsymmetrical Thickness in XAML)
            title_label.Margin = new Thickness(0, 0, 0, 20);
            // Set info field to the text loaded from server.
            info_label.Text = pharmacy.Infos;

        }

    }

}