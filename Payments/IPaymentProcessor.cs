using Stripe.Checkout;
using Stripe;
using Shop.Api.Entities;

namespace Shop.Api.Payments {
    public interface IPaymentProcessor{
        Task<PaymentInfo> ProcessPayment(List<SessionLineItemOptions> items, string redirectUrlSuccess, string redirectUrlFailure);
    }
}