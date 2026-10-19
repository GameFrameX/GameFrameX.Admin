using GameFrameX.Client.Api.Services.PayVerification;
using GameFrameX.Client.Api.Services;
using GameFrameX.Payment.Google.Option;

namespace GameFrameX.Core.Tests.PayConfig;

public class PaymentWebhookPolicyTests
{
    [Fact]
    public void HasGooglePubSubPushAuthenticationConfig_RequiresAudienceAndServiceAccountEmail()
    {
        Assert.False(PaymentWebhookPolicy.HasGooglePubSubPushAuthenticationConfig(new GooglePaymentOptionSetting()));

        Assert.False(PaymentWebhookPolicy.HasGooglePubSubPushAuthenticationConfig(new GooglePaymentOptionSetting
        {
            GooglePaymentPubSubPushAudience = "https://client.example/google",
        }));

        Assert.True(PaymentWebhookPolicy.HasGooglePubSubPushAuthenticationConfig(new GooglePaymentOptionSetting
        {
            GooglePaymentPubSubPushAudience = "https://client.example/google",
            GooglePaymentPubSubPushServiceAccountEmail = "pubsub-push@example.iam.gserviceaccount.com",
        }));
    }

    [Theory]
    [InlineData("payment")]
    [InlineData("cancel_subscription")]
    [InlineData("create_subscription")]
    [InlineData("non_renewal_subscription")]
    [InlineData("update_subscription")]
    public void IsXsollaSubscriptionNotification_RecognizesUnsupportedSubscriptionEvents(string notificationType)
    {
        Assert.True(PaymentWebhookPolicy.IsXsollaSubscriptionNotification(notificationType));
    }

    [Theory]
    [InlineData("order_paid")]
    [InlineData("order_canceled")]
    [InlineData("refund")]
    public void IsXsollaSubscriptionNotification_ExcludesOrdinaryOrderEvents(string notificationType)
    {
        Assert.False(PaymentWebhookPolicy.IsXsollaSubscriptionNotification(notificationType));
    }

    [Fact]
    public async Task GooglePubSubPushAuthenticator_RejectsMissingBearerHeader()
    {
        var authenticator = new GooglePubSubPushAuthenticator();
        var settings = new[]
        {
            new GooglePaymentOptionSetting
            {
                GooglePaymentPubSubPushAudience = "https://client.example/google",
                GooglePaymentPubSubPushServiceAccountEmail = "pubsub-push@example.iam.gserviceaccount.com",
            },
        };

        Assert.False(await authenticator.ValidateAsync(string.Empty, settings));
        Assert.False(await authenticator.ValidateAsync("Basic abc", settings));
    }

    [Fact]
    public async Task GooglePubSubPushAuthenticator_RejectsWhenNoConfiguredCandidateExists()
    {
        var authenticator = new GooglePubSubPushAuthenticator();
        var settings = new[] { new GooglePaymentOptionSetting(), };

        Assert.False(await authenticator.ValidateAsync("Bearer fake-token", settings));
    }
}