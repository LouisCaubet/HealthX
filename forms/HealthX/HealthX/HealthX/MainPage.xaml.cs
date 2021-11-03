using HealthX.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HealthX {
    public partial class MainPage : TabbedPage {

        public MapPage mapPage;

        public MainPage() {
            InitializeComponent();
            mapPage = (MapPage) map_page.CurrentPage;
        }

        protected override void OnAppearing() {
            base.OnAppearing();

        }

    }
}
