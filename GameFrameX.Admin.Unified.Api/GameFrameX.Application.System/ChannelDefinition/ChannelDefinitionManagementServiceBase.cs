using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Application.System.ChannelDefinition.Dto;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Application.System.ChannelDefinition;

/// <summary>
/// 渠道定义管理服务基类
/// </summary>
public abstract class ChannelDefinitionManagementServiceBase<TEntity>
    : GenericCrudSelectService<TEntity,
        AddChannelDefinitionRequest, UpdateChannelDefinitionRequest,
        ChannelDefinitionResponse, PageChannelDefinitionRequest>
    where TEntity : GameFrameX.Entity.System.ChannelDefinition, new()
{
    private readonly SqlSugarRepository<TenantGameAppPackageEntity> _packageRepository;
    private readonly SqlSugarRepository<GlobalChannelDefinitionEntity>? _globalChannelRepository;
    private readonly CurrentUserContext _currentUserContext;

    /// <summary>
    /// 渠道定义管理服务基类
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="globalChannelRepository"></param>
    /// <param name="packageRepository"></param>
    /// <param name="currentUserContext"></param>
    protected ChannelDefinitionManagementServiceBase(
        SqlSugarRepository<TEntity> repository,
        SqlSugarRepository<GlobalChannelDefinitionEntity>? globalChannelRepository,
        SqlSugarRepository<TenantGameAppPackageEntity> packageRepository,
        CurrentUserContext currentUserContext)
        : base(repository)
    {
        _packageRepository = packageRepository;
        _globalChannelRepository = globalChannelRepository;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// 当前服务管理的渠道作用域
    /// </summary>
    protected abstract ChannelScopeEnum ManagedScope { get; }

    /// <summary>
    /// 构建分页查询
    /// </summary>
    protected override ISugarQueryable<ChannelDefinitionResponse> BuildPageQuery(PageChannelDefinitionRequest input)
    {
        var searchKey = input.SearchKey?.Trim();
        var code = input.Code?.Trim();
        var platformCode = input.PlatformCode?.Trim();
        var channelType = input.ChannelType;
        var isEnabled = input.IsEnabled;

        return Repository.AsQueryable()
                         .Filter(null, true)
                         // Filter(null,true) 绕过租户过滤的同时也关掉了软删全局过滤，显式恢复（Api#287）
                         .Where(u => u.IsDeleted == false)
                         .Where(u => u.Scope == ManagedScope)
                         .WhereIF(ManagedScope == ChannelScopeEnum.Tenant, u => u.TenantId == _currentUserContext.TenantId)
                         .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                      u.Name.Contains(searchKey!) || u.Code.Contains(searchKey!))
                         .WhereIF(!string.IsNullOrWhiteSpace(code), u => u.Code.Contains(code!))
                         .WhereIF(!string.IsNullOrWhiteSpace(platformCode), u => u.PlatformCode == platformCode)
                         .WhereIF(channelType.HasValue, u => u.ChannelType == channelType!.Value)
                         .WhereIF(isEnabled.HasValue, u => u.IsEnabled == isEnabled)
                         .Select<ChannelDefinitionResponse>();
    }

    protected override async Task OnAddingAsync(AddChannelDefinitionRequest input)
    {
        input.Scope = ManagedScope;
        Normalize(input);
        EnsureScopePermission();
        await CheckCodeUniqueAsync(input.Code, input.Scope);
    }

    protected override async Task OnAddedAsync(AddChannelDefinitionRequest input, TEntity entity)
    {
        ApplyScope(entity, input.Scope);
        await Repository.AsUpdateable(entity)
                        .UpdateColumns(u => new
                        {
                            u.Code,
                            u.Scope,
                            u.TenantId,
                            u.PlatformCode,
                            u.ChannelType,
                            u.IsSystemLocked,
                        })
                        .ExecuteCommandAsync();
    }

    protected override async Task OnUpdatingAsync(UpdateChannelDefinitionRequest input)
    {
        input.Scope = ManagedScope;
        Normalize(input);

        var existing = await Repository.AsQueryable()
                                       .Filter(null, true)
                                       .FirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh("渠道不存在");

        if (existing.Scope != ManagedScope)
        {
            throw BusinessError.Oh("渠道不属于当前管理入口");
        }

        if (ManagedScope == ChannelScopeEnum.System && !_currentUserContext.SuperAdmin)
        {
            throw BusinessError.Oh("租户不能编辑系统渠道");
        }

        if (ManagedScope == ChannelScopeEnum.Tenant && existing.TenantId != _currentUserContext.TenantId)
        {
            throw BusinessError.Oh("只能编辑本租户渠道");
        }

        EnsureScopePermission();
        await CheckCodeUniqueAsync(input.Code, input.Scope, input.Id);

        var referenced = await _packageRepository.IsAnyAsync(u => u.ChannelId == input.Id);
        if (referenced && !string.Equals(existing.Code, input.Code, StringComparison.OrdinalIgnoreCase))
        {
            throw BusinessError.Oh("渠道已被分包引用，不能修改编码");
        }

        if (referenced && existing.Scope != input.Scope)
        {
            throw BusinessError.Oh("渠道已被分包引用，不能修改作用域");
        }
    }

    protected override Task OnUpdatedAsync(UpdateChannelDefinitionRequest input, TEntity before, TEntity after)
    {
        ApplyScope(after, input.Scope);
        return Repository.AsUpdateable(after)
                         .UpdateColumns(u => new
                         {
                             u.Code,
                             u.Name,
                             u.Description,
                             u.IsDefault,
                             u.Scope,
                             u.TenantId,
                             u.PlatformCode,
                             u.ChannelType,
                             u.IsSystemLocked,
                             u.IsEnabled,
                         })
                         .Where(u => u.Id == input.Id)
                         .IgnoreColumns(true)
                         .ExecuteCommandAsync();
    }

    /// <summary>
    /// 查询当前用户可用渠道
    /// </summary>
    protected async Task<List<AvailableChannelDefinitionResponse>> QueryAvailableAsync([FromQuery] BaseSelectPageRequest input)
    {
        var searchKey = input?.SearchKey?.Trim();
        var pageSize = Math.Min(input.PageSize > 0 ? input.PageSize : BaseSelectPageRequest.DefaultPageSize, BaseSelectPageRequest.MaxPageSize);
        var channels = new List<GameFrameX.Entity.System.ChannelDefinition>();
        if (ManagedScope == ChannelScopeEnum.Tenant && _globalChannelRepository != null)
        {
            channels.AddRange(await _globalChannelRepository.AsQueryable()
                                                            .Filter(null, true)
                                                            .Where(u => u.IsDeleted == false)
                                                            .Where(u => (u.IsEnabled == true || u.IsEnabled == null) &&
                                                                        u.Scope == ChannelScopeEnum.System)
                                                            .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                                                         u.Name.Contains(searchKey!) || u.Code.Contains(searchKey!))
                                                            .OrderBy(u => u.Name)
                                                            .Take(pageSize)
                                                            .ToListAsync());
        }

        channels.AddRange(await Repository.AsQueryable()
                                          .Filter(null, true)
                                          .Where(u => u.IsDeleted == false)
                                          .Where(u => (u.IsEnabled == true || u.IsEnabled == null) &&
                                                      u.TenantId == _currentUserContext.TenantId)
                                          .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                                       u.Name.Contains(searchKey!) || u.Code.Contains(searchKey!))
                                          .OrderBy(u => u.Name)
                                          .Take(pageSize)
                                          .ToListAsync());

        return channels.Select(u => new AvailableChannelDefinitionResponse
                       {
                           Id = u.Id,
                           Code = string.IsNullOrWhiteSpace(u.Code) ? u.Name : u.Code,
                           Name = u.Name,
                           DisplayName = u.Name,
                           Scope = u.Scope,
                           ChannelType = u.ChannelType,
                           PlatformCode = string.IsNullOrWhiteSpace(u.PlatformCode) ? "all" : u.PlatformCode,
                       })
                       .ToList();
    }

    /// <summary>
    /// 删除渠道
    /// </summary>
    public override async Task DeleteAsync(BaseDeleteRequest input)
    {
        var entity = await Repository.AsQueryable()
                                     .Filter(null, true)
                                     .FirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh("渠道不存在");
        if (entity.Scope != ManagedScope)
        {
            throw BusinessError.Oh("渠道不属于当前管理入口");
        }

        if (ManagedScope == ChannelScopeEnum.System && !_currentUserContext.SuperAdmin)
        {
            throw BusinessError.Oh("租户不能删除系统渠道");
        }

        if (ManagedScope == ChannelScopeEnum.Tenant && entity.TenantId != _currentUserContext.TenantId)
        {
            throw BusinessError.Oh("只能删除本租户渠道");
        }

        var referenced = await _packageRepository.IsAnyAsync(u => u.ChannelId == input.Id);
        if (referenced)
        {
            throw BusinessError.Oh("渠道已被分包引用，不能删除，请停用");
        }

        await Repository.FakeDeleteAsync(entity);
    }

    private async Task CheckCodeUniqueAsync(string code, ChannelScopeEnum scope, long? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw BusinessError.Oh("渠道编码不能为空");
        }

        bool existsSystemCode;
        if (ManagedScope == ChannelScopeEnum.System || _globalChannelRepository == null)
        {
            existsSystemCode = excludeId.HasValue
                                   ? await Repository.AsQueryable().Filter(null, true)
                                                     .AnyAsync(u => u.Scope == ChannelScopeEnum.System &&
                                                                    u.Code == code &&
                                                                    u.Id != excludeId.Value)
                                   : await Repository.AsQueryable().Filter(null, true)
                                                     .AnyAsync(u => u.Scope == ChannelScopeEnum.System && u.Code == code);
        }
        else
        {
            existsSystemCode = excludeId.HasValue
                                   ? await _globalChannelRepository.AsQueryable().Filter(null, true)
                                                                   .AnyAsync(u => u.Scope == ChannelScopeEnum.System &&
                                                                                  u.Code == code &&
                                                                                  u.Id != excludeId.Value)
                                   : await _globalChannelRepository.AsQueryable().Filter(null, true)
                                                                   .AnyAsync(u => u.Scope == ChannelScopeEnum.System && u.Code == code);
        }

        if (scope == ChannelScopeEnum.System)
        {
            if (existsSystemCode)
            {
                throw BusinessError.Oh("系统渠道编码已存在");
            }

            return;
        }

        if (existsSystemCode)
        {
            throw BusinessError.Oh("租户渠道编码不能与系统渠道重复");
        }

        var tenantId = _currentUserContext.TenantId;
        var existsTenantCode = excludeId.HasValue
                                   ? await Repository.AsQueryable().Filter(null, true)
                                                     .AnyAsync(u => u.Scope == ChannelScopeEnum.Tenant &&
                                                                    u.TenantId == tenantId &&
                                                                    u.Code == code &&
                                                                    u.Id != excludeId.Value)
                                   : await Repository.AsQueryable().Filter(null, true)
                                                     .AnyAsync(u => u.Scope == ChannelScopeEnum.Tenant &&
                                                                    u.TenantId == tenantId &&
                                                                    u.Code == code);
        if (existsTenantCode)
        {
            throw BusinessError.Oh("租户渠道编码已存在");
        }
    }

    private void EnsureScopePermission()
    {
        if (ManagedScope == ChannelScopeEnum.System && !_currentUserContext.SuperAdmin)
        {
            throw BusinessError.Oh("只有超级管理员可以维护系统渠道");
        }
    }

    private void ApplyScope(GameFrameX.Entity.System.ChannelDefinition entity, ChannelScopeEnum scope)
    {
        entity.Scope = scope;
        entity.TenantId = scope == ChannelScopeEnum.System ? 0 : _currentUserContext.TenantId;
        entity.PlatformCode = string.IsNullOrWhiteSpace(entity.PlatformCode) ? "all" : entity.PlatformCode.Trim();
        entity.Code = entity.Code?.Trim() ?? string.Empty;
    }

    private static void Normalize(AddChannelDefinitionRequest input)
    {
        input.Code = input.Code?.Trim() ?? string.Empty;
        input.Name = input.Name?.Trim();
        input.PlatformCode = string.IsNullOrWhiteSpace(input.PlatformCode) ? "all" : input.PlatformCode.Trim();
        input.IsSystemLocked = input.Scope == ChannelScopeEnum.System || input.IsSystemLocked;
    }

    private static void Normalize(UpdateChannelDefinitionRequest input)
    {
        input.Code = input.Code?.Trim() ?? string.Empty;
        input.Name = input.Name?.Trim();
        input.PlatformCode = string.IsNullOrWhiteSpace(input.PlatformCode) ? "all" : input.PlatformCode.Trim();
        input.IsSystemLocked = input.Scope == ChannelScopeEnum.System || input.IsSystemLocked;
    }
}