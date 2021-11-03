using System;

namespace HealthX.Braintree {

    /// <summary>
    /// Interface to interact with the Drop-IN UI through DependencyService.
    /// </summary>
    public interface IBraintreeUI {

        /// <summary>
        /// Sets the return call of the Drop IN UI
        /// </summary>
        /// <param name="method">the method to call when Drop-IN completed its task</param>
        void SetReturnCall(Action<int,string, string> method);

        /// <summary>
        /// Starts the Drop-IN UI with given user token and amount.
        /// </summary>
        /// <param name="token">The braintreeclient token of the user.</param>
        /// <param name="amount">The price to be paid.</param>
        void StartDropInUiForResult(string token, decimal amount);

    }

}
