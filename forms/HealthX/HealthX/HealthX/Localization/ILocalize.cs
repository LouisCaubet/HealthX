using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthX.Localization {

    /// <summary>
    /// Interface used to load platform-dependent localization informations.
    /// </summary>
    public interface ILocalize {

        CultureInfo GetCurrentCultureInfo(); // retrieve language on platform
        void SetLocale(CultureInfo ci); // set language on platform 

    }

}
