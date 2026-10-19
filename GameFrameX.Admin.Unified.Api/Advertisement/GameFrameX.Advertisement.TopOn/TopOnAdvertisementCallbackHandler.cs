using System.Security.Cryptography;
using System.Text;
using GameFrameX.Advertisement.Abstract;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Advertisement.TopOn;

/// <summary>
/// TopOn 广告回调处理器（MD5 验签，签名串字面量按 TopOn 协议固定，参数名 schema-driven）。
/// </summary>
public sealed class TopOnAdvertisementCallbackHandler : AdvertisementCallbackHandlerBase, IAdvertisementCallbackHandler
{
    public string ProviderName
    {
        get { return "TopOn"; }
    }

    public TopOnAdvertisementCallbackHandler(Repository<TenantGameRoleEntity> roleRepository) : base(roleRepository)
    {
    }

    public OptionVerification Verify(TenantAdvertisementCallbackOptions advertisementOptions, IQueryCollection query)
    {
        var option = DeserializeOption<TopOnAdvertisementCallbackOption>(advertisementOptions);
        if (option == null || string.IsNullOrWhiteSpace(option.signingSecret))
        {
            return OptionVerification.Invalid(null);
        }

        var sign = QueryValue(query, option.signParam);
        if (string.IsNullOrWhiteSpace(sign))
        {
            return OptionVerification.Invalid(option);
        }

        var transactionId = QueryValue(query, option.transactionIdParam);
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            return OptionVerification.Invalid(option);
        }

        var placementId = QueryValue(query, option.placementIdParam);
        var adsourceId = QueryValue(query, option.adsourceIdParam);
        var rewardAmount = QueryValue(query, option.rewardAmountParam);
        var rewardName = QueryValue(query, option.rewardNameParam);

        var raw = "trans_id=" + transactionId +
                  "&placement_id=" + placementId +
                  "&adsource_id=" + adsourceId +
                  "&reward_amount=" + rewardAmount +
                  "&reward_name=" + rewardName +
                  "&sec_key=" + option.signingSecret;
        var ilrd = QueryValue(query, "ilrd");
        if (!string.IsNullOrEmpty(ilrd))
        {
            raw += "&ilrd=" + ilrd;
        }

        var hash = MD5.HashData(Encoding.UTF8.GetBytes(raw));
        var expected = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        if (!string.Equals(expected, sign, StringComparison.OrdinalIgnoreCase))
        {
            return OptionVerification.Invalid(option);
        }

        var isSandBox = long.TryParse(QueryValue(query, "is_test"), out var isTest) && isTest > 0;
        var advertisementPackageId = long.TryParse(QueryValue(query, option.extraDataParam), out var pkgId) ? pkgId : (long?)null;

        return OptionVerification.Valid(option, new VerifiedPayload
        {
            IsValid = true,
            TransactionId = transactionId,
            AdvertisementUnit = placementId,
            RewardItem = rewardName,
            RewardAmount = rewardAmount,
            IsSandbox = isSandBox,
            AdvertisementPackageId = advertisementPackageId,
        });
    }

    public Task<RoleResolution?> ResolveRoleAsync(object option, IQueryCollection query)
    {
        var typed = option as TopOnAdvertisementCallbackOption;
        if (typed == null)
        {
            return Task.FromResult<RoleResolution?>(null);
        }

        return ResolveRoleByIdAsync(query, typed.userIdParam);
    }
}