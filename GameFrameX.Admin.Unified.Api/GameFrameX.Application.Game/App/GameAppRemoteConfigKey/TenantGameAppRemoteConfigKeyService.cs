using GameFrameX.Application.Game.App.GameAppRemoteConfigKey.Dto;

namespace GameFrameX.Application.Game.App.GameAppRemoteConfigKey;

/// <summary>
/// 远程配置内容键定义管理服务
/// </summary>
[ApiDescriptionSettings(ClientConst.GroupName, Order = 100)]
public class TenantGameAppRemoteConfigKeyService : GenericCrudService<Entity.Tenant.Game.RemoteConfig.TenantGameAppRemoteConfigKeyEntity, AddGameAppRemoteConfigKeyRequest, UpdateGameAppRemoteConfigKeyRequest, GameAppRemoteConfigKeyResponse, PageGameAppRemoteConfigKeyRequest>
{
    private static readonly string[] AllowedValueTypes = { "string", "bool", "long", };

    public TenantGameAppRemoteConfigKeyService(SqlSugarRepository<Entity.Tenant.Game.RemoteConfig.TenantGameAppRemoteConfigKeyEntity> repository) : base(repository)
    {
    }

    /// <summary>
    /// 构建分页查询条件
    /// </summary>
    protected override ISugarQueryable<GameAppRemoteConfigKeyResponse> BuildPageQuery(PageGameAppRemoteConfigKeyRequest input)
    {
        var keyName = input.KeyName?.Trim() ?? string.Empty;
        var displayName = input.DisplayName?.Trim() ?? string.Empty;
        var valueType = NormalizeValueType(input.ValueType);
        var isEnabled = input.IsEnabled.GetValueOrDefault();

        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrEmpty(keyName), x => x.KeyName.Contains(keyName))
                         .WhereIF(!string.IsNullOrEmpty(displayName), x => x.DisplayName.Contains(displayName))
                         .WhereIF(!string.IsNullOrEmpty(input.ValueType), x => x.ValueType == valueType)
                         .WhereIF(input.IsEnabled.HasValue, x => x.IsEnabled == isEnabled)
                         .Select<GameAppRemoteConfigKeyResponse>();
    }

    /// <summary>
    /// 新增前的名称唯一性校验
    /// </summary>
    protected override async Task OnAddingAsync(AddGameAppRemoteConfigKeyRequest input)
    {
        var keyName = input.KeyName?.Trim() ?? string.Empty;
        input.KeyName = keyName;
        input.ValueType = NormalizeValueType(input.ValueType);

        var exists = await Repository.AsQueryable()
                                     .Where(x => x.KeyName == keyName)
                                     .AnyAsync();
        if (exists)
        {
            throw BusinessError.Oh($"Key 名称 '{keyName}' 已存在");
        }
    }

    /// <summary>
    /// 更新前的名称唯一性校验
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateGameAppRemoteConfigKeyRequest input)
    {
        var keyName = input.KeyName?.Trim() ?? string.Empty;
        input.KeyName = keyName;
        input.ValueType = NormalizeValueType(input.ValueType);

        var current = await Repository.AsQueryable()
                                      .Where(x => x.Id == input.Id)
                                      .FirstAsync();
        if (current == null)
        {
            throw BusinessError.Oh("内容元数据不存在");
        }

        if (!string.Equals(current.KeyName, input.KeyName, StringComparison.Ordinal))
        {
            throw BusinessError.Oh("内容元数据 Key 创建后不可修改");
        }

        if (!string.Equals(current.ValueType, input.ValueType, StringComparison.OrdinalIgnoreCase))
        {
            throw BusinessError.Oh("内容元数据类型创建后不可修改");
        }

        var exists = await Repository.AsQueryable()
                                     .Where(x => x.KeyName == keyName && x.Id != input.Id)
                                     .AnyAsync();
        if (exists)
        {
            throw BusinessError.Oh($"Key 名称 '{keyName}' 已存在");
        }
    }

    private static string NormalizeValueType(string? valueType)
    {
        var normalized = valueType?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(normalized))
        {
            normalized = "string";
        }

        if (!AllowedValueTypes.Contains(normalized))
        {
            throw BusinessError.Oh("值类型只支持 string、bool、long");
        }

        return normalized;
    }
}