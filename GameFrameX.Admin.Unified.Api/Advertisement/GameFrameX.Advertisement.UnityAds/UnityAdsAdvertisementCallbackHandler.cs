using System.Text;
using GameFrameX.Advertisement.Abstract;
using GameFrameX.Core.SignatureAuth;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Advertisement.UnityAds;

/// <summary>
/// Unity Ads 广告回调处理器（HMAC-SHA256 hex 验签，签名串 <c>sid,oid</c>）。
/// </summary>
public sealed class UnityAdsAdvertisementCallbackHandler : AdvertisementCallbackHandlerBase, IAdvertisementCallbackHandler
{
    public string ProviderName
    {
        get { return "UnityAds"; }
    }

    public UnityAdsAdvertisementCallbackHandler(Repository<TenantGameRoleEntity> roleRepository) : base(roleRepository)
    {
    }

    public OptionVerification Verify(TenantAdvertisementCallbackOptions advertisementOptions, IQueryCollection query)
    {
        var option = DeserializeOption<UnityAdsAdvertisementCallbackOption>(advertisementOptions);
        if (option == null || string.IsNullOrWhiteSpace(option.signingSecret))
        {
            return OptionVerification.Invalid(option);
        }

        var sid = QueryValue(query, option.sidParam);
        var oid = QueryValue(query, option.orderIdParam);
        var hmac = QueryValue(query, option.hmacParam);
        if (string.IsNullOrWhiteSpace(sid) || string.IsNullOrWhiteSpace(oid) || string.IsNullOrWhiteSpace(hmac))
        {
            return OptionVerification.Invalid(option);
        }

        var raw = sid + "," + oid;
        // C267：HMAC 计算收口到 CanonicalRequestBuilder（全仓唯一实现；签名串格式不变，仍为 Unity Ads 协议）
        var expectedBytes = CanonicalRequestBuilder.HmacSha256(Encoding.UTF8.GetBytes(option.signingSecret), raw);

        var expected = BitConverter.ToString(expectedBytes).Replace("-", string.Empty).ToLowerInvariant();
        if (!string.Equals(expected, hmac, StringComparison.OrdinalIgnoreCase))
        {
            return OptionVerification.Invalid(option);
        }

        return OptionVerification.Valid(option, new VerifiedPayload
        {
            IsValid = true,
            TransactionId = oid,
            IsSandbox = false,
        });
    }

    public Task<RoleResolution?> ResolveRoleAsync(object option, IQueryCollection query)
    {
        var typed = option as UnityAdsAdvertisementCallbackOption;
        if (typed == null)
        {
            return Task.FromResult<RoleResolution?>(null);
        }

        return ResolveRoleByIdAsync(query, typed.sidParam);
    }
}