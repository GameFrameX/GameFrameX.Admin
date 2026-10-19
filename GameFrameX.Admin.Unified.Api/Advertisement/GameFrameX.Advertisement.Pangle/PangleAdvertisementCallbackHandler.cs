using GameFrameX.Advertisement.Abstract;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Hash;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Advertisement.Pangle;

/// <summary>
/// Pangle 广告回调处理器（SHA256(SecurityKey:TransId) 验签，密钥取租户配置 <see cref="TenantAdvertisementCallbackOptions.PangleSecurityKey"/>）。
/// <para>与 TopOn / AdMob / UnityAds 不同：不进发奖链路，仅由宿主 Controller 调用 <see cref="Verify"/> 验签，保持既有 HTTP 响应行为。</para>
/// </summary>
public sealed class PangleAdvertisementCallbackHandler : AdvertisementCallbackHandlerBase, IAdvertisementCallbackHandler
{
    public string ProviderName
    {
        get { return "Pangle"; }
    }

    public PangleAdvertisementCallbackHandler(Repository<TenantGameRoleEntity> roleRepository) : base(roleRepository)
    {
    }

    public OptionVerification Verify(TenantAdvertisementCallbackOptions advertisementOptions, IQueryCollection query)
    {
        if (advertisementOptions == null || string.IsNullOrWhiteSpace(advertisementOptions.PangleSecurityKey))
        {
            return OptionVerification.Invalid(null);
        }

        // 验签材料取 ConfigJson 的 signingSecret（Provider 层保证必填）；ConfigJson 未携带参数名时回落 Option 默认参数名（TransId/Sign）
        var option = DeserializeOption<PangleAdvertisementCallbackOption>(advertisementOptions) ?? new PangleAdvertisementCallbackOption();
        var transId = QueryValue(query, option.transactionIdParam);
        var sign = QueryValue(query, option.signParam);
        if (string.IsNullOrWhiteSpace(transId) || string.IsNullOrWhiteSpace(sign))
        {
            return OptionVerification.Invalid(option);
        }

        var rawData = advertisementOptions.PangleSecurityKey + ":" + transId;
        if (!Sha256Helper.VerifyHash(rawData, sign))
        {
            return OptionVerification.Invalid(option);
        }

        return OptionVerification.Valid(option, new VerifiedPayload
        {
            IsValid = true,
            TransactionId = transId,
        });
    }

    /// <summary>
    /// Pangle 既有回调不解析角色（仅验签），恒返回 null。
    /// </summary>
    public Task<RoleResolution?> ResolveRoleAsync(object option, IQueryCollection query)
    {
        return Task.FromResult<RoleResolution?>(null);
    }
}