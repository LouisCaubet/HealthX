using HealthX.Backend;
using HealthX.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HealthX.Datatypes {

    /// <summary>
    /// Enum representing possible requirements to buy a medicine
    /// </summary>
    public enum PflichtValue {
        NO_PFLICHT, // no requirement
        APOTHEKENPFLICHT, // can only be bought in pharmacies
        VERSCHREIBUNGSPFLICHT // only with a prescription
    }

    /// <summary>
    /// Represents the availability of a medicine at a given pharmacy
    /// </summary>
    public class AvailabilityState {

        // Static Instances representing possible availability states.
        public static AvailabilityState NOT_AVAILABLE = new AvailabilityState();
        public static AvailabilityState AVAILABLE_ON_COMMAND = new AvailabilityState();
        public static AvailabilityState FEW_LEFT = new AvailabilityState();
        public static AvailabilityState AVAILABLE = new AvailabilityState();

        /// <summary>
        /// Returns the description text for this availability state
        /// </summary>
        /// <returns>String representing the availability state for the user.</returns>
        public string GetText() {
            if (this == NOT_AVAILABLE) return AppResources.NotAvailableText;
            if (this == AVAILABLE_ON_COMMAND) return AppResources.AvailableOrderText;
            if (this == FEW_LEFT) return AppResources.FewLeftText;
            if (this == AVAILABLE) return AppResources.AvailableText;

            throw new ArgumentException();
        }

        /// <summary>
        /// Returns the short description for this availability state
        /// Short descriptions should be used when there is only space for 1-2 words.
        /// </summary>
        /// <returns>String : short description</returns>
        public string GetTextShort() {
            if (this == NOT_AVAILABLE) return AppResources.NotAvailableTextShort;
            if (this == AVAILABLE_ON_COMMAND) return AppResources.AvailableOrderTextShort;
            if (this == FEW_LEFT) return AppResources.FewLeftTextShort;
            if (this == AVAILABLE) return AppResources.AvailableTextShort;

            throw new ArgumentException();
        }


        /// <summary>
        /// Returns the Color associated with this availability state
        /// GetText text should be colored with this color.
        /// </summary>
        /// <returns>The Color recommended for this availability state</returns>
        public Color GetColor() {
            if (this == NOT_AVAILABLE) return Color.Red;
            if (this == AVAILABLE_ON_COMMAND) return Color.OrangeRed;
            if (this == FEW_LEFT) return Color.DarkOrange;
            if (this == AVAILABLE) return Color.Green;

            throw new ArgumentException();
        }

        /// <summary>
        /// Returns the text for the purchase button.
        /// This is the text that must be placed in the "BUY" button.
        /// </summary>
        /// <returns></returns>
        public string GetButtonText() {
            if (this == NOT_AVAILABLE) return AppResources.PurchaseText;
            if (this == AVAILABLE_ON_COMMAND) return AppResources.OrderNowText;
            if (this == FEW_LEFT || this == AVAILABLE) return AppResources.ReserveText;

            throw new ArgumentException();
        }

        /// <summary>
        /// Returns the recommended color for the "BUY" button.
        /// </summary>
        /// <returns></returns>
        public Color GetButtonColor() {
            if (this == NOT_AVAILABLE) return Color.Gray;
            if (this == AVAILABLE_ON_COMMAND) return Color.Orange;
            if (this == FEW_LEFT) return Color.Orange;
            if (this == AVAILABLE) return Color.Green;

            throw new ArgumentException();
        }

    }

    /// <summary>
    /// Represents a medicine, containing standard fields + fields for a specific pharmacy.
    /// </summary>
    public class Medicine {

        /// <summary>
        /// The ID of the medicine in the Database.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// The Name of the medicine
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The description of the medicine (in database)
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// The Requirement to purchase this medicine. See PflichtValue.
        /// </summary>
        public PflichtValue Requires { get; private set; }

        /// <summary>
        /// The price of this medicine. Possibly pharmacy-dependent.
        /// </summary>
        public decimal Price { get; private set; }

        /// <summary>
        /// The Image representing this medicine (loaded from server)
        /// </summary>
        public byte[] Image { get; private set; }

        /// <summary>
        /// The Availability of this medicine. Obviously Pharmacy-Dependent.
        /// </summary>
        public AvailabilityState Availability { get; set; }

        /// <summary>
        /// The raw storage info of this medicine. Obviously Pharmacy-Dependent.
        /// </summary>
        public StorageRow Storage { get; set; }

        /// <summary>
        /// Constructor for a medicine, from DB info. This should only be used when fetching the object from DB.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="requires"></param>
        /// <param name="price"></param>
        /// <param name="image"></param>
        public Medicine (int id, string name, string description, PflichtValue requires, decimal price, byte[] image) {
            Id = id;
            this.Name = name;
            this.Description = description;
            this.Requires = requires;
            this.Price = price;
            Image = image;
        }


        /// <summary>
        /// Returns the text associated with the PflichtValue (requirement) of the medicine.
        /// </summary>
        /// <returns></returns>
        public string GetRequiresText () {
            switch (Requires) {
                case PflichtValue.NO_PFLICHT: return AppResources.NoSellRestrictionsText;
                case PflichtValue.APOTHEKENPFLICHT: return AppResources.OnlyPharmaciesText;
                case PflichtValue.VERSCHREIBUNGSPFLICHT: return AppResources.OnlyPrescriptionText;
            }
            throw new ArgumentException();
        }

        /// <summary>
        /// Returns price text, with approx. if pharmacy-dependent.
        /// </summary>
        /// <returns></returns>
        public string GetPriceText () {

            if(Price <= 0) {
                return AppResources.ApproxText + Math.Ceiling(Math.Abs(Price)) + "€";
            }
            else return Price + "€";
        }

    }

}
