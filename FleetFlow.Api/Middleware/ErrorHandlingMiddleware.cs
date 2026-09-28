using Stripe;

namespace FleetFlow.Api.Services;

public interface IBillingService
{
    Task<object> CreateCheckoutSessionAsync(string planName, string customerEmail, string successUrl, string cancelUrl);
}

public class BillingService : IBillingService
{
    private readonly IConfiguration _configuration;

    public BillingService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<object> CreateCheckoutSessionAsync(string planName, string customerEmail, string successUrl, string cancelUrl)
    {
        StripeConfiguration.ApiKey = _configuration["Stripe:ApiKey"];

        var priceMap = new Dictionary<string, long>
        {
            ["Starter"] = 4900,
            ["Growth"] = 9900,
            ["Pro"] = 19900
        };

        var options = new SessionCreateOptions
        {
            CustomerEmail = customerEmail,
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = priceMap.GetValueOrDefault(planName, 4900),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = planName
                        }
                    },
                    Quantity = 1
                }
            },
            Mode = "subscription",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        return new { sessionId = session.Id, url = session.Url };
    }
}

