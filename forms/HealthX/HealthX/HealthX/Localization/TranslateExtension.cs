using HealthX.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HealthX {

    /// <summary>
    /// Extension enabling localization in XAML files. 
    /// See documentation about localizing a Xamarin.Forms app for details.
    /// </summary>
    [ContentProperty("Text")]
    public class TranslateExtension : IMarkupExtension {

        readonly CultureInfo ci = null;
        const string ResourceId = "HealthX.Localization.AppResources";

        static readonly Lazy<ResourceManager> ResMgr = new Lazy<ResourceManager>(
            () => new ResourceManager(ResourceId, IntrospectionExtensions.GetTypeInfo(typeof(TranslateExtension)).Assembly));

        public string Text { get; set; }

        public TranslateExtension() {

            if(Device.RuntimePlatform == Device.iOS || Device.RuntimePlatform == Device.Android) {
                //ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
                ci = new CultureInfo("fr-FR");
            }

        }

        public object ProvideValue(IServiceProvider serviceProvider) {
            if (Text == null)
                return string.Empty;

            var translation = ResMgr.Value.GetString(Text, ci);
            if (translation == null) {
				translation = Text; // HACK: returns the key, which GETS DISPLAYED TO THE USER
            }
            return translation;
        }

    }
}
