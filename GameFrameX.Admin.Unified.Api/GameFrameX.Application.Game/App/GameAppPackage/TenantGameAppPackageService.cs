using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Application.Game.App.GameAppPackage.Dto;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Core.Service.TenantPackage;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;

#pragma warning disable CS8602, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.App.GameAppPackage;

/// <summary>
/// 游戏应用分包管理服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 101)]
public class TenantGameAppPackageService
    : GenericCrudSelectService<TenantGameAppPackageEntity,
        AddGameAppPackageRequest, UpdateGameAppPackageRequest,
        GameAppPackageResponse, PageGameAppPackageRequest>
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;
    private readonly SqlSugarRepository<GlobalPlatformEntity> _globalPlatformRepository;
    private readonly SqlSugarRepository<GlobalChannelDefinitionEntity> _globalChannelDefinitionRepository;
    private readonly SqlSugarRepository<TenantChannelDefinitionEntity> _tenantChannelDefinitionRepository;
    private readonly TenantQuotaService _tenantQuotaService;
    private readonly CurrentUserContext _currentUserContext;

    public TenantGameAppPackageService(
        SqlSugarRepository<TenantGameAppPackageEntity> repository,
        SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository,
        SqlSugarRepository<GlobalPlatformEntity> globalPlatformRepository,
        SqlSugarRepository<GlobalChannelDefinitionEntity> globalChannelDefinitionRepository,
        SqlSugarRepository<TenantChannelDefinitionEntity> tenantChannelDefinitionRepository,
        TenantQuotaService tenantQuotaService,
        CurrentUserContext currentUserContext)
        : base(repository)
    {
        _gameAppRepository = gameAppRepository;
        _globalPlatformRepository = globalPlatformRepository;
        _globalChannelDefinitionRepository = globalChannelDefinitionRepository;
        _tenantChannelDefinitionRepository = tenantChannelDefinitionRepository;
        _tenantQuotaService = tenantQuotaService;
        _currentUserContext = currentUserContext;
    }

    protected override ISugarQueryable<GameAppPackageResponse> BuildPageQuery(PageGameAppPackageRequest input)
    {
        var gameAppId = AppScopeRules.EnsureFactAppId(input.AppId, "游戏应用分包");
        var tenantId = TenantOperationContext.GetEffectiveTenantId();
        return Repository.AsQueryable()
                         .Filter(null, true)
                         .Where(u => u.IsDeleted == false)
                         .WhereIF(tenantId.HasValue, u => u.TenantId == null || u.TenantId == tenantId.Value)
                         .Where(u => u.GameAppId == gameAppId)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.PackageName), u => u.PackageName.Contains(input.PackageName.Trim()))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.PackageVersion), u => u.PackageVersion.Contains(input.PackageVersion.Trim()))
                         .WhereIF(input.IsEnabled.HasValue, u => u.IsEnabled == input.IsEnabled)
                         .Select(u => new GameAppPackageResponse
                         {
                             Id = u.Id,
                             AppId = u.GameAppId,
                             Name = u.Name,
                             PackageName = u.PackageName,
                             PlatformCode = u.PlatformCode,
                             PlatformName = u.PlatformName,
                             ChannelId = u.ChannelId,
                             ChannelCode = u.ChannelCode,
                             ChannelName = u.ChannelName,
                             ParentPackageId = u.ParentPackageId,
                             PackageKind = u.PackageKind,
                             PlatformInheritanceMode = u.PlatformInheritanceMode,
                             PackageVersion = u.PackageVersion,
                             IsDefault = u.IsDefault,
                             IsEnabled = u.IsEnabled,
                             CreatedTime = u.CreatedTime,
#pragma warning disable CS8601 // u.CreatedName 来自实体（string?），DTO CreatedName 字段在 DB 实际允许 null；下游序列化容忍 null
                             CreatedName = u.CreatedName,
#pragma warning restore CS8601
                             UpdateTime = u.UpdateTime ?? 0,
#pragma warning disable CS8601 // u.UpdatedName 来自实体（string?），DTO UpdatedName 字段在 DB 实际允许 null；下游序列化容忍 null
                             UpdatedName = u.UpdatedName,
#pragma warning restore CS8601
                             RowVersion = u.RowVersion ?? 0,
                             Remark = u.Remark,
                         });
    }

    /// <summary>
    /// 获取分包选择器列表。
    /// </summary>
    [HttpPost(CrudActionRoutes.Select)]
    [ApiDescriptionSettings(Name = "Select")]
    public override async Task<List<BaseSelectPageResponse>> SelectAsync(BaseSelectPageRequest input)
    {
        var gameAppId = AppScopeRules.EnsureFactAppId(input.AppId, "游戏应用分包");
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var pageSize = Math.Min(input.PageSize > 0 ? input.PageSize : BaseSelectPageRequest.DefaultPageSize, BaseSelectPageRequest.MaxPageSize);
        var packages = await Repository.AsQueryable()
                                       .Where(u => u.IsEnabled != false)
                                       .Where(u => u.GameAppId == gameAppId)
                                       .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                                    u.Name.Contains(searchKey) ||
                                                    u.PackageName.Contains(searchKey) ||
                                                    u.PackageVersion.Contains(searchKey))
                                       .OrderByDescending(u => u.IsDefault)
                                       .OrderBy(u => u.CreatedTime)
                                       .Take(pageSize)
                                       .Select(u => new
                                       {
                                           u.Id,
                                           u.Name,
                                           u.PackageName,
                                           u.PackageVersion,
                                       })
                                       .ToListAsync();

        return packages.Select(u => new BaseSelectPageResponse
                       {
                           Id = u.Id,
                           Value = u.Name,
                           Label = FormatPackageLabel(u.PackageName, u.PackageVersion),
                       })
                       .ToList();
    }

    /// <summary>
    /// 新增分包
    /// </summary>
    public override async Task<GameAppPackageResponse> AddAsync(AddGameAppPackageRequest input)
    {
        await ValidateParentPackageAsync(input.AppId, input.ParentPackageId, input.PackageKind);
        var channel = await ValidatePackageAsync(input.AppId, input.PackageName, input.PlatformCode, input.ChannelId);
        await CheckPackageQuotaAsync(input.AppId);

        var entity = input.Adapt<TenantGameAppPackageEntity>();
        NormalizePackageKind(entity);
        FillChannel(entity, channel);
        await FillSnapshotsAsync(entity);
        entity.IsEnabled = true;
        await Repository.AsInsertable(entity).ExecuteCommandAsync();

        if (entity.IsDefault)
        {
            await ClearOtherDefaultsAsync(entity.GameAppId, entity.Id);
        }

        // 实体 GameAppId 与 Output AppId 名不一致，Mapster 直映会丢失，显式补齐
        var output = entity.Adapt<GameAppPackageResponse>();
        output.AppId = entity.GameAppId;
        return output;
    }

    /// <summary>
    /// 更新分包
    /// </summary>
    public override async Task UpdateAsync(UpdateGameAppPackageRequest input)
    {
        await ApplyImmutableFieldsAsync(input);
        await ValidateParentPackageAsync(input.AppId, input.ParentPackageId, input.PackageKind, input.Id);
        var channel = await ValidatePackageAsync(input.AppId, input.PackageName, input.PlatformCode, input.ChannelId, input.Id);

        var entity = input.Adapt<TenantGameAppPackageEntity>();
        NormalizePackageKind(entity);
        FillChannel(entity, channel);
        await FillSnapshotsAsync(entity);
        await Repository.AsUpdateable(entity)
                        .IgnoreColumns(u => new { u.CreatedTime, u.CreatedId, u.CreatedName, })
                        .IgnoreColumns(true)
                        .ExecuteCommandWithOptLockAsync(true);

        if (input.IsDefault)
        {
            await ClearOtherDefaultsAsync(input.AppId, input.Id);
        }
    }

    protected override async Task OnAddingAsync(AddGameAppPackageRequest input)
    {
        await ValidateParentPackageAsync(input.AppId, input.ParentPackageId, input.PackageKind);
        await ValidatePackageAsync(input.AppId, input.PackageName, input.PlatformCode, input.ChannelId);
        await CheckPackageQuotaAsync(input.AppId);
    }

    protected override async Task OnAddedAsync(AddGameAppPackageRequest input, TenantGameAppPackageEntity entity)
    {
        await FillSnapshotsAsync(entity);
        entity.IsEnabled = true;
        await Repository.AsUpdateable(entity)
                        .UpdateColumns(u => new
                        {
                            u.PlatformCode,
                            u.PlatformName,
                            u.ChannelId,
                            u.ChannelCode,
                            u.ChannelName,
                            u.IsEnabled,
                        })
                        .ExecuteCommandAsync();

        if (entity.IsDefault)
        {
            await ClearOtherDefaultsAsync(entity.GameAppId, entity.Id);
        }
    }

    protected override async Task OnUpdatingAsync(UpdateGameAppPackageRequest input)
    {
        await ApplyImmutableFieldsAsync(input);
        await ValidateParentPackageAsync(input.AppId, input.ParentPackageId, input.PackageKind, input.Id);
        await ValidatePackageAsync(input.AppId, input.PackageName, input.PlatformCode, input.ChannelId, input.Id);
    }

    protected override async Task OnUpdatedAsync(UpdateGameAppPackageRequest input, TenantGameAppPackageEntity before, TenantGameAppPackageEntity after)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (entity == null)
        {
            return;
        }

        await FillSnapshotsAsync(entity);
        await Repository.AsUpdateable(entity)
                        .UpdateColumns(u => new
                        {
                            u.PlatformCode,
                            u.PlatformName,
                            u.ChannelId,
                            u.ChannelCode,
                            u.ChannelName,
                        })
                        .ExecuteCommandAsync();

        if (input.IsDefault)
        {
            await ClearOtherDefaultsAsync(input.AppId, input.Id);
        }
    }

    /// <summary>
    /// 获取应用下启用分包列表
    /// </summary>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<GameAppPackageListResponse>> ListAsync([FromQuery] long appId)
    {
        AppScopeRules.EnsureFactAppId(appId, "游戏应用分包");
        return await Repository.AsQueryable()
                               .Where(u => u.GameAppId == appId && u.IsEnabled != false)
                               .OrderByDescending(u => u.IsDefault)
                               .OrderBy(u => u.CreatedTime)
                               .Select(u => new GameAppPackageListResponse
                               {
                                   Id = u.Id,
                                   AppId = u.GameAppId,
                                   Name = u.Name,
                                   PackageName = u.PackageName,
                                   PackageVersion = u.PackageVersion,
                                   IsDefault = u.IsDefault,
                               })
                               .ToListAsync();
    }

    /// <summary>
    /// 启用或停用分包
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "SetStatus")]
    public async Task SetStatusAsync(SetGameAppPackageStatusRequest input)
    {
        var tenantId = TenantOperationContext.GetEffectiveTenantId();
        var entity = await Repository.AsQueryable()
                                     .Filter(null, true)
                                     .Where(u => u.IsDeleted == false)
                                     .WhereIF(tenantId.HasValue, u => u.TenantId == null || u.TenantId == tenantId.Value)
                                     .FirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh("分包不存在");
        entity.IsEnabled = input.IsEnabled;
        if (!input.IsEnabled)
        {
            entity.IsDefault = false;
        }

        await Repository.AsUpdateable(entity).UpdateColumns(u => new { u.IsEnabled, u.IsDefault, }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 设置默认分包
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "SetDefault")]
    public async Task SetDefaultAsync(BaseIdRequest input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh("分包不存在");
        if (entity.IsEnabled == false)
        {
            throw BusinessError.Oh("停用分包不能设为默认");
        }

        await ClearOtherDefaultsAsync(entity.GameAppId, entity.Id);
        entity.IsDefault = true;
        await Repository.AsUpdateable(entity).UpdateColumns(u => u.IsDefault).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除分包
    /// </summary>
    public override async Task DeleteAsync(BaseDeleteRequest input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh("分包不存在");
        entity.IsDefault = false;
        await Repository.AsUpdateable(entity).UpdateColumns(u => u.IsDefault).ExecuteCommandAsync();
        await Repository.FakeDeleteAsync(entity);
    }

    private async Task<ChannelDefinition> ValidatePackageAsync(long gameAppId, string packageName, string platformCode, long channelId, long? excludeId = null)
    {
        var gameApp = await _gameAppRepository.GetFirstAsync(u => u.Id == gameAppId);
        if (gameApp == null)
        {
            throw BusinessError.Oh("游戏应用不存在");
        }

        var exists = excludeId.HasValue
                         ? await Repository.IsAnyAsync(u => u.PackageName == packageName && u.Id != excludeId.Value)
                         : await Repository.IsAnyAsync(u => u.PackageName == packageName);
        if (exists)
        {
            throw BusinessError.Oh("包名已存在");
        }

        var platformExists = await _globalPlatformRepository.IsAnyAsync(u => u.Name == platformCode && (u.IsEnabled == true || u.IsEnabled == null));
        if (!platformExists)
        {
            throw BusinessError.Oh("平台不存在或已停用");
        }

        return await ResolveAvailableChannelAsync(channelId);
    }

    private async Task ApplyImmutableFieldsAsync(UpdateGameAppPackageRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh("分包不存在");
        input.AppId = existing.GameAppId;
        input.PackageName = existing.PackageName;
        input.PlatformCode = existing.PlatformCode;
        input.ChannelId = existing.ChannelId;
    }

    private async Task ValidateParentPackageAsync(long gameAppId, long? parentPackageId, PackageKindEnum packageKind, long? currentId = null)
    {
        EnsurePackageKindConsistency(parentPackageId, packageKind);

        if (!parentPackageId.HasValue || parentPackageId.Value <= 0)
        {
            return;
        }

        if (currentId.HasValue && parentPackageId.Value == currentId.Value)
        {
            throw BusinessError.Oh("子包不能继承自身");
        }

        var parent = await Repository.GetFirstAsync(u => u.Id == parentPackageId.Value) ?? throw BusinessError.Oh("母包不存在");
        if (parent.GameAppId != gameAppId)
        {
            throw BusinessError.Oh("子包只能继承同应用下的母包");
        }

        if (parent.ParentPackageId.HasValue)
        {
            throw BusinessError.Oh("子包最多只能继承一层母包");
        }
    }

    /// <summary>
    /// 校验 <see cref="PackageKindEnum"/> 与母包关系的一致性：声明为子包（Derived）却未指定母包属于自相矛盾，直接拒绝。
    /// 其余无母包组合（Normal/Master）合法，留待 <see cref="NormalizePackageKind"/> 在写入侧进一步规整。
    /// </summary>
    internal static void EnsurePackageKindConsistency(long? parentPackageId, PackageKindEnum packageKind)
    {
        var hasParent = parentPackageId.HasValue && parentPackageId.Value > 0;
        if (!hasParent && packageKind == PackageKindEnum.Derived)
        {
            throw BusinessError.Oh("子包必须指定母包");
        }
    }

    /// <summary>
    /// 规整 <see cref="TenantGameAppPackageEntity.PackageKind"/> 与 <see cref="TenantGameAppPackageEntity.ParentPackageId"/> 的一致性：
    /// 指定母包即视为子包，强制 <see cref="PackageKindEnum.Derived"/>（自动纠正 Master/Normal→Derived），避免运行时按子包语义回退时字段表达与实际关系冲突。
    /// </summary>
    internal static void NormalizePackageKind(TenantGameAppPackageEntity entity)
    {
        if (entity.ParentPackageId.HasValue)
        {
            entity.PackageKind = PackageKindEnum.Derived;
        }
    }

    private async Task CheckPackageQuotaAsync(long gameAppId)
    {
        if (_currentUserContext.SuperAdmin)
        {
            return;
        }

        var quota = await _tenantQuotaService.GetEffectiveQuotaAsync(_currentUserContext.TenantId);
        var currentCount = await Repository.AsQueryable().Where(u => u.GameAppId == gameAppId).CountAsync();
        if (currentCount >= quota.MaxPackageCountPerApp)
        {
            throw BusinessError.Oh($"分包数已达上限（{quota.MaxPackageCountPerApp}），无法新增");
        }
    }

    private async Task FillSnapshotsAsync(TenantGameAppPackageEntity entity)
    {
        var platform = await _globalPlatformRepository.GetFirstAsync(u => u.Name == entity.PlatformCode);
        entity.PlatformName = platform?.Name ?? entity.PlatformCode;
        if (entity.ChannelId > 0)
        {
            var channel = await FindAvailableChannelByIdAsync(entity.ChannelId);
            if (channel != null)
            {
                FillChannel(entity, channel);
                return;
            }
        }

        entity.ChannelName = entity.ChannelName ?? entity.ChannelCode;
    }

    private async Task<ChannelDefinition> ResolveAvailableChannelAsync(long channelId)
    {
        var channel = channelId > 0 ? await FindAvailableChannelByIdAsync(channelId) : null;
        return channel ?? throw BusinessError.Oh("渠道不存在或已停用");
    }

    private async Task<ChannelDefinition> FindAvailableChannelByIdAsync(long channelId)
    {
        var globalChannel = await _globalChannelDefinitionRepository.AsQueryable()
                                                                    .Filter(null, true)
                                                                    .Where(u => u.IsDeleted == false)
                                                                    .Where(u => (u.IsEnabled == true || u.IsEnabled == null) &&
                                                                                u.Scope == ChannelScopeEnum.System)
                                                                    .FirstAsync(u => u.Id == channelId);
        if (globalChannel != null)
        {
            return globalChannel;
        }

        return await _tenantChannelDefinitionRepository.AsQueryable()
                                                       .Filter(null, true)
                                                       .Where(u => u.IsDeleted == false)
                                                       .Where(u => (u.IsEnabled == true || u.IsEnabled == null) &&
                                                                   u.TenantId == _currentUserContext.TenantId)
                                                       .FirstAsync(u => u.Id == channelId);
    }

    private static void FillChannel(TenantGameAppPackageEntity entity, ChannelDefinition channel)
    {
        entity.ChannelId = channel.Id;
        entity.ChannelCode = string.IsNullOrWhiteSpace(channel.Code) ? channel.Name : channel.Code;
        entity.ChannelName = channel.Name;
    }

    private static string FormatPackageLabel(string packageName, string packageVersion)
    {
        return string.IsNullOrWhiteSpace(packageVersion)
                   ? packageName
                   : $"{packageName} / {packageVersion}";
    }

    private async Task ClearOtherDefaultsAsync(long gameAppId, long currentPackageId)
    {
        await Repository.AsUpdateable()
                        .SetColumns(u => u.IsDefault == false)
                        .Where(u => u.GameAppId == gameAppId && u.Id != currentPackageId && u.IsDefault)
                        .ExecuteCommandAsync();
    }
}
#pragma warning restore CS8602, CS8629