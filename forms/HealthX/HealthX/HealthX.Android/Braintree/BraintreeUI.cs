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
using HealthX.Droid.Braintree;
using HealthX.Braintree;
using Com.Braintreepayments.Api.Dropin;

[assembly: Xamarin.Forms.Dependency (typeof(BraintreeUI))]
namespace HealthX.Droid.Braintree {

    public class BraintreeUI : IBraintreeUI {

        static Action<int, string, string> returnCall;

        public void StartDropInUiForResult(string token, decimal amount) {

            DropInRequest dropInRequest = new DropInRequest().ClientToken(token).Amount(amount.ToString());
            Activity activity = Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity;
            activity.StartActivityForResult(dropInRequest.GetIntent(activity), 42);

        }

        public static void OnDropInResult(Activity activity, Result resultCode, Intent data) {
            if (resultCode == Result.Ok) {
                DropInResult result = (DropInResult) data.GetParcelableExtra(DropInResult.ExtraDropInResult);
                returnCall(0, result.PaymentMethodNonce.Nonce, result.PaymentMethodType.ToString());
                // use the result to update your UI and send the payment method nonce to your server
            }
            else if (resultCode == Result.Canceled) {
                returnCall(1, null, null);
                // the user canceled
            }
            else {
                // handle errors here, an exception may be available in
                Exception error = (Exception)data.GetSerializableExtra(DropInActivity.ExtraError);
                returnCall(-1, null, null);
            }
        }

        public void SetReturnCall(Action<int, string, string> method) {
            returnCall = method;
        }

    }

}