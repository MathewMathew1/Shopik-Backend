using Shop.Api.Entities;
using MongoDB.Driver;
using Stripe;
using Stripe.Checkout;


namespace Shop.Api.Payments{
    public class StripePayment : IPaymentProcessor{

        private const string URL = "https://api.imgur.com/3/upload";
        private readonly IConfiguration configuration;

        //private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter;

        public StripePayment(IConfiguration configuration){

            this.configuration = configuration;
            var secretKey = this.configuration.GetSection("Stripe:secretKey").Value;
            StripeConfiguration.ApiKey = secretKey;

        }

        public async Task<PaymentInfo> ProcessPayment(List<SessionLineItemOptions> items, string redirectUrlSuccess, string redirectUrlFailure){

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>{"card"} ,
                LineItems = items,
                Mode = "payment",
                SuccessUrl = redirectUrlSuccess,
                CancelUrl = redirectUrlFailure,
            };
            
            var service = new SessionService();
            Session session = service.Create(options);
            
            return new PaymentInfo {Url = session.Url, Id = session.Id};
            
  
        }

        

    } 
}