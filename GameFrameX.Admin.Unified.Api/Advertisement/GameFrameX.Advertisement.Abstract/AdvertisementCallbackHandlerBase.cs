using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Json;
using GameFrameX.Foundation.Logger;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 广告回调处理器基类：提供 Option 反序列化、query 取值、按 roleId 解析角色等共用工具。
/// </summary>
public abstract class AdvertisementCallbackHandlerBase
{
    protected readonly Repository<TenantGameRoleEntity> RoleRepository;

    protected AdvertisementCallbackHandlerBase(Repository<TenantGameRoleEntity> roleRepository)
    {
        RoleRepository = roleRepository;
    }

    /// <summary>
    /// 反序列化 ConfigJson 为强类型 Option；校验 Provider 绑定与类型匹配。
    /// </summary>
    protected static TOption? DeserializeOption<TOption>(TenantAdvertisementCallbackOptions advertisementOptions) where TOption : class, IAdvertisementCallbackOptionSetting
    {
        if (string.IsNullOrWhiteSpace(advertisementOptions?.ConfigJson))
        {
            return null;
        }

        if (advertisementOptions.Provider == null || !AdvertisementCallbackProviderRegistry.TryGet(advertisementOptions.Provider, out var binding) || binding == null)
        {
            return null;
        }

        if (binding.OptionType != typeof(TOption))
        {
            return null;
        }

        try
        {
            return JsonHelper.Deserialize(advertisementOptions.ConfigJson, typeof(TOption)) as TOption;
        }
        catch (Exception ex)
        {
            LogHelper.Error($"广告回调配置解析失败 provider={advertisementOptions.Provider}: {ex.Message}", ex);
            return null;
        }
    }

    protected static string QueryValue(IQueryCollection query, string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }

        return query.TryGetValue(key, out var values) ? values.ToString() : string.Empty;
    }

    /// <summary>
    /// 按 roleId 查角色（TopOn user_id / UnityAds sid 共用）。
    /// </summary>
    protected async Task<RoleResolution?> ResolveRoleByIdAsync(IQueryCollection query, string roleIdParam)
    {
        var roleIdRaw = QueryValue(query, roleIdParam);
        if (!long.TryParse(roleIdRaw, out var roleId))
        {
            return null;
        }

        var role = await RoleRepository.GetByIdAsync(roleId);
        if (role == null)
        {
            return null;
        }

        return new RoleResolution { Role = role, UserId = role.UserId, };
    }
}