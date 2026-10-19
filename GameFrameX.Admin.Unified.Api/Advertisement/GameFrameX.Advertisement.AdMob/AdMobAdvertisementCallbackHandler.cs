using System.Security.Cryptography;
using GameFrameX.Advertisement.Abstract;
using GameFrameX.Foundation.Json;
using GameFrameX.Foundation.Logger;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using Microsoft.AspNetCore.Http;
using GameFrameX.Core.SignatureAuth;

namespace GameFrameX.Advertisement.AdMob;

/// <summary>
/// Google AdMob 广告回调处理器（HMAC-SHA256 base64 验签，常量时间比较；签名串由 query 去除 signature/key_id 后按 key 字典序拼装）。
/// </summary>
public sealed class AdMobAdvertisementCallbackHandler : AdvertisementCallbackHandlerBase, IAdvertisementCallbackHandler
{
    public string ProviderName
    {
        get { return "GoogleAdMob"; }
    }

    public AdMobAdvertisementCallbackHandler(Repository<TenantGameRoleEntity> roleRepository) : base(roleRepository)
    {
    }

    public OptionVerification Verify(TenantAdvertisementCallbackOptions advertisementOptions, IQueryCollection query)
    {
        var option = DeserializeOption<GoogleAdMobAdvertisementCallbackOption>(advertisementOptions);
        if (option == null || string.IsNullOrWhiteSpace(option.verificationKeys))
        {
            return OptionVerification.Invalid(option);
        }

        var signature = QueryValue(query, option.signatureParam);
        var keyId = QueryValue(query, option.keyIdParam);
        if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(keyId))
        {
            return OptionVerification.Invalid(option);
        }

        // 复用 Provider 已解析的 key_id → base64 key 映射
        if (advertisementOptions.AdMobKeys == null || advertisementOptions.AdMobKeys.Count == 0 ||
            !advertisementOptions.AdMobKeys.TryGetValue(keyId, out var verificationKey) ||
            string.IsNullOrWhiteSpace(verificationKey))
        {
            return OptionVerification.Invalid(option);
        }

        var queryParams = query
                          .Where(p => p.Key != option.signatureParam && p.Key != option.keyIdParam)
                          .OrderBy(p => p.Key, StringComparer.Ordinal)
                          .Select(p => p.Key + "=" + p.Value);
        var stringToSign = string.Join("&", queryParams);

        byte[] keyBytes;
        byte[] actualBytes;
        try
        {
            keyBytes = Convert.FromBase64String(verificationKey);
            actualBytes = Convert.FromBase64String(signature);
        }
        catch
        {
            return OptionVerification.Invalid(option);
        }

        // C267：HMAC 计算收口到 CanonicalRequestBuilder（全仓唯一实现；签名串格式不变，仍为 AdMob 协议）
        var expectedBytes = CanonicalRequestBuilder.HmacSha256(keyBytes, stringToSign);

        if (!CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes))
        {
            return OptionVerification.Invalid(option);
        }

        var transactionId = QueryValue(query, option.transactionIdParam);
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            return OptionVerification.Invalid(option);
        }

        var isSandBox = false;
        long? advertisementPackageId = null;
        var customDataJson = QueryValue(query, option.customDataParam);
        if (!string.IsNullOrWhiteSpace(customDataJson))
        {
            try
            {
                var custom = JsonHelper.Deserialize<AdMobCallbackCustomDataRequest>(customDataJson);
                if (custom != null)
                {
                    if (bool.TryParse(custom.IsSandbox, out var sandbox))
                    {
                        isSandBox = sandbox;
                    }

                    if (!string.IsNullOrEmpty(custom.SlotId) && long.TryParse(custom.SlotId, out var slotId))
                    {
                        advertisementPackageId = slotId;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warning($"AdMob custom_data 解析失败: {ex.Message}");
            }
        }

        return OptionVerification.Valid(option, new VerifiedPayload
        {
            IsValid = true,
            TransactionId = transactionId,
            AdvertisementUnit = QueryValue(query, option.adUnitParam),
            RewardItem = QueryValue(query, option.rewardItemParam),
            RewardAmount = QueryValue(query, option.rewardAmountParam),
            IsSandbox = isSandBox,
            AdvertisementPackageId = advertisementPackageId,
        });
    }

    public async Task<RoleResolution?> ResolveRoleAsync(object option, IQueryCollection query)
    {
        var typed = option as GoogleAdMobAdvertisementCallbackOption;
        if (typed == null)
        {
            return null;
        }

        var customDataJson = QueryValue(query, typed.customDataParam);
        if (string.IsNullOrWhiteSpace(customDataJson))
        {
            return null;
        }

        AdMobCallbackCustomDataRequest custom;
        try
        {
            custom = JsonHelper.Deserialize<AdMobCallbackCustomDataRequest>(customDataJson);
        }
        catch
        {
            return null;
        }

        if (custom == null || !long.TryParse(custom.PlayerId, out var playerId))
        {
            return null;
        }

        var role = await RoleRepository.GetByIdAsync(playerId);
        if (role == null)
        {
            return null;
        }

        long userId = 0;
        long.TryParse(custom.UserId, out userId);
        return new RoleResolution { Role = role, UserId = userId, };
    }
}