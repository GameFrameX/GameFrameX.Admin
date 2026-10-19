using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Role.Dto;
using GameFrameX.Core.Service.Tenant.Dto;
using GameFrameX.Core.Service.TenantMenuSync;
using GameFrameX.Core.Service.User.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;
using GameFrameX.Foundation.Http.Normalization;

namespace GameFrameX.Core.Service.Tenant;

/// <summary>
/// 系统租户管理服务
/// </summary>
[ApiDescriptionSettings(Order = 390)]
public class SystemTenantService : IAdminApiController
{
    private readonly SqlSugarRepository<SystemTenantEntity> _systemTenantRepository;
    private readonly SqlSugarRepository<SystemTenantMenuSyncStateEntity> _systemTenantMenuSyncStateRepository;
    private readonly SystemConfigService _systemConfigService;
    private readonly SystemCacheService _systemCacheService;
    private readonly TenantDbConnectionProvider _tenantDbConnectionProvider;
    private readonly PostgreSqlTenantDatabaseProvisioner _tenantDatabaseProvisioner;
    private readonly ILogger<SystemTenantService> _logger;
    private readonly DbConnectionOptions _dbOptions;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly TenantMenuSyncQueue _tenantMenuSyncQueue;

    public SystemTenantService(SqlSugarRepository<SystemTenantEntity> systemTenantRepository,
        SqlSugarRepository<SystemTenantMenuSyncStateEntity> systemTenantMenuSyncStateRepository,
        SystemConfigService systemConfigService,
        SystemCacheService systemCacheService,
        TenantDbConnectionProvider tenantDbConnectionProvider,
        PostgreSqlTenantDatabaseProvisioner tenantDatabaseProvisioner,
        ILogger<SystemTenantService> logger,
        IOptions<DbConnectionOptions> dbOptions,
        ISqlSugarClient sqlSugarClient,
        IServiceProvider serviceProvider,
        TenantMenuSyncQueue tenantMenuSyncQueue)
    {
        _systemTenantRepository = systemTenantRepository;
        _systemTenantMenuSyncStateRepository = systemTenantMenuSyncStateRepository;
        _systemConfigService = systemConfigService;
        _systemCacheService = systemCacheService;
        _tenantDbConnectionProvider = tenantDbConnectionProvider;
        _tenantDatabaseProvisioner = tenantDatabaseProvisioner;
        _logger = logger;
        _dbOptions = dbOptions.Value;
        _sqlSugarClient = sqlSugarClient;
        _serviceProvider = serviceProvider;
        _tenantMenuSyncQueue = tenantMenuSyncQueue;
    }

    /// <summary>
    /// 获取租户分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取租户分页列表")]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<TenantResponse>> PageAsync(PageTenantRequest input)
    {
        // Name 匹配的是租户库组织名（HydrateTenantAdminAsync 填充），主库 SQL 无法跨库过滤，
        // 与下方 Phone 一致改为 hydrate 后内存过滤
        // superadmin 上下文会跳过全局 QueryFilter（SqlSugarSetup 超管豁免），
        // 软删除租户会漏进列表；显式过滤与 GenericCrudService 默认 BuildPageQuery 一致
        var page = await _systemTenantRepository.AsQueryable()
                                         .Where(u => u.IsDeleted == false)
                                         .OrderBy(u => u.Id)
                                         .Select(u => new TenantResponse
                                         {
                                             Id = u.Id,
                                             OrganizationId = u.OrganizationId,
                                             UserId = u.UserId,
                                             TenantType = u.TenantType,
                                             DbType = u.DbType,
                                             Connection = u.Connection,
                                             ConfigId = u.ConfigId,
                                             Remark = u.Remark,
                                             IsEnabled = u.IsEnabled,
                                             CreatedTime = u.CreatedTime,
                                             Host = u.Host,
                                             ExpireTime = u.ExpireTime,
                                             PackageId = u.PackageId,
                                             GracePeriodDays = u.GracePeriodDays,
                                             Name = u.Remark ?? string.Empty,
                                         })
                                         .ToPagedListAsync(input.Page, input.PageSize);

        var items = page.Items.ToList();
        foreach (var tenant in items)
        {
            await HydrateTenantAdminAsync(tenant);
        }

        await HydrateTenantMenuSyncStatusAsync(items);

        if (!string.IsNullOrWhiteSpace(input.Phone))
        {
            var phone = input.Phone.Trim();
            items = items.Where(u => !string.IsNullOrWhiteSpace(u.Phone) && u.Phone.Contains(phone)).ToList();
            page.Items = items;
        }

        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            var name = input.Name.Trim();
            items = items.Where(u => (u.Name ?? string.Empty).Contains(name) || (u.Remark ?? string.Empty).Contains(name) || (u.Host ?? string.Empty).Contains(name)).ToList();
            page.Items = items;
        }

        return page;
    }

    /// <summary>
    /// 按本页租户 Id 集合批量回填套餐菜单同步状态（C358；主库 <c>system_tenant_menu_sync_state_entity</c> 同 scope 单批查询）。
    /// 状态行缺失 ⇒ <see cref="TenantMenuSyncStatusEnum.Pending"/> + 三字段 null；与 C353「缺失即待同步」语义一致。
    /// </summary>
    private async Task HydrateTenantMenuSyncStatusAsync(List<TenantResponse> tenants)
    {
        if (tenants.Count == 0)
        {
            return;
        }

        var ids = tenants.Select(u => u.Id).ToArray();
        var states = await _systemTenantMenuSyncStateRepository.AsQueryable()
                                            .Where(u => ids.Contains(u.Id))
                                            .ToListAsync();
        var stateByTenantId = states.ToDictionary(u => u.Id);

        foreach (var tenant in tenants)
        {
            if (!stateByTenantId.TryGetValue(tenant.Id, out var state))
            {
                tenant.MenuSyncStatus = TenantMenuSyncStatusEnum.Pending;
                tenant.MenuSyncLastSyncTime = null;
                tenant.MenuSyncLastError = null;
                tenant.MenuSyncNextRetryTime = null;
                continue;
            }

            tenant.MenuSyncStatus = state.Status;
            tenant.MenuSyncLastSyncTime = state.LastSyncTime;
            tenant.MenuSyncLastError = state.LastError;
            tenant.MenuSyncNextRetryTime = state.NextRetryTime;
        }
    }

    /// <summary>
    /// 获取库隔离的租户列表（租户菜单同步范围枚举口径，与 TenantMenuSyncWorker.IsInSyncScope 同口径；
    /// C354 T6 起默认租户纳入——Enterprise 套餐菜单启动幂等补全先于同步首扫）。
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<SystemTenantEntity>> GetTenantDbListAsync()
    {
        return await _systemTenantRepository.GetListAsync(u => u.TenantType == TenantTypeEnum.Db
                                                        && u.IsEnabled == true
                                                        && u.ConfigId != SqlSugarConst.MainConfigId);
    }

    /// <summary>
    /// 增加租户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add")]
    [HttpPost]
    [DisplayName("增加租户")]
    public async Task AddTenantAsync(AddTenantRequest input)
    {
        await AddTenantCoreAsync(input);
    }

    /// <summary>
    /// 新增租户核心逻辑（管理端 Add 与邮箱自助注册共用）。
    /// </summary>
    /// <param name="input">租户入参</param>
    /// <param name="overrideEncryptedPassword">覆盖租管密码（已加密），邮箱自助注册传入；缺省回落系统配置默认密码</param>
    /// <returns>新建的租户管理员；租户库已存在管理员时返回 <c>null</c></returns>
    internal async Task<TenantUserEntity?> AddTenantCoreAsync(AddTenantRequest input, string? overrideEncryptedPassword = null)
    {
        ValidateOrdinaryTenantInput(input);

        var isExist = await _systemTenantRepository.AsQueryable().Filter(null, true).AnyAsync(u => u.Remark == input.Name);
        if (isExist)
        {
            throw BusinessError.Oh(TenantErrorCode.TenantNameAlreadyExists);
        }

        // SimpleClient.ChangeRepository 无参重载对带参构造仓储必抛 MissingMethodException（SqlSugar 5.1.4.216，GFX-682），
        // 统一改为经 DI 解析 open generic 仓储，由构造函数按实体边界路由主库。
        var systemUserRepository = _serviceProvider.GetRequiredService<SqlSugarRepository<SystemUserEntity>>();
        isExist = await systemUserRepository.AsQueryable().Filter(null, true).AnyAsync(u => u.Account == input.AdminAccount);
        if (isExist)
        {
            throw BusinessError.Oh(TenantErrorCode.TenantAdminNameAlreadyExists);
        }

        // C354 T1：创建即套餐分配——校验可用性（存在且未软删且启用），悬空套餐直接拒绝创建
        var package = await FindPackageAsync(input.PackageId);
        ValidatePackageUsableForAssignment(input.PackageId, package);

        var tenant = CreateOrdinaryTenantForPersistence(input);
        ProvisionTenantDatabase(tenant);
        await _systemTenantRepository.InsertAsync(tenant);
        var adminUser = await InitializeTenantDatabaseAsync(tenant, input, overrideEncryptedPassword);

        await CacheTenantAsync();

        return adminUser;
    }

    /// <summary>
    /// 设置租户状态
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("设置租户状态")]
    [ApiDescriptionSettings(Name = "SetStatus")]
    public async Task<int> SetStatusAsync(TenantRequest input)
    {
        var tenant = await _systemTenantRepository.GetFirstAsync(u => u.Id == input.Id);
        if (tenant == null || IsProtectedDefaultTenant(tenant))
        {
            throw BusinessError.Oh(TenantErrorCode.DefaultTenantStatusChangeForbidden);
        }

        tenant.IsEnabled = input.IsEnabled;
        var rows = await _systemTenantRepository.AsUpdateable(tenant).UpdateColumns(u => new { u.IsEnabled, }).ExecuteCommandAsync();
        await CacheTenantAsync(input.Id);
        return rows;
    }

    /// <summary>
    /// 新增租户初始化
    /// </summary>
    /// <param name="tenant"></param>
    private async Task<TenantUserEntity?> InitializeTenantDatabaseAsync(SystemTenantEntity tenant, AddTenantRequest input, string? overrideEncryptedPassword = null)
    {
        var tenantId = tenant.Id;
        var tenantName = input.Name;
        var tenantDb = CreateTenantDatabaseConnection(tenant);
        if (await TenantAdminExistsAsync(tenantDb, tenantId))
        {
            return null;
        }

        // 初始化普通租户库中的运营组织、角色和职位基础模板。
        var orgsByKey = await InitTenantOrganizationsAsync(tenantDb, tenantId, tenantName);
        var rolesByCode = await InitTenantRolesAsync(tenantDb, tenantId);
        var positionsByCode = await InitTenantPositionsAsync(tenantDb, tenantId);

        var adminOrg = orgsByKey[TenantDefaultDataTemplates.RootOrganizationKey];
        var adminRole = rolesByCode[TenantDefaultDataTemplates.AdminRoleCode];
        var adminPosition = positionsByCode[TenantDefaultDataTemplates.AdminPositionCode];

        // 初始化系统账号
        var configuredPassword = await _systemConfigService.GetConfigValueAsync<string>(CommonConst.SysPassword);
        var newUser = CreateTenantAdminUser(new TenantResponse
        {
            Id = tenant.Id,
            AdminAccount = input.AdminAccount,
            Email = input.Email,
            Phone = input.Phone,
            Name = input.Name,
        }, ResolveTenantAdminEncryptedPassword(overrideEncryptedPassword, configuredPassword), adminOrg.Id, adminPosition.Id);
        newUser = await tenantDb.Insertable(newUser).ExecuteReturnEntityAsync();

        // 关联用户及角色
        var newUserRole = new TenantUserRoleEntity
        {
            RoleId = adminRole.Id,
            UserId = newUser.Id,
        };
        await tenantDb.Insertable(newUserRole).ExecuteCommandAsync();

        // 关联租户运营组织和管理员用户
        tenant.UserId = newUser.Id;
        tenant.OrganizationId = adminOrg.Id;
        await _systemTenantRepository.AsUpdateable(new SystemTenantEntity
        {
            Id = tenant.Id,
            UserId = newUser.Id,
            OrganizationId = adminOrg.Id,
            TenantType = TenantTypeEnum.Db,
            DbType = tenant.DbType,
            Connection = tenant.Connection,
            ConfigId = tenant.Id.ToString(),
        }).UpdateColumns(u => new { u.UserId, u.OrganizationId, u.TenantType, u.DbType, u.Connection, u.ConfigId, }).ExecuteCommandAsync();

        // 默认租户管理员角色菜单集合（C354 T1：有套餐 ⇒ 全量默认授权 ∩ 套餐闭包）。
        var allowedAdminMenuIds = await ResolveInitialAdminMenuIdsAsync(tenant.PackageId);
        var roleMenus = CreateTenantAdminRoleMenus(adminRole.Id, allowedAdminMenuIds);
        await tenantDb.Insertable(roleMenus).ExecuteCommandAsync();

        await DefaultTenantDatabaseSeeder.SeedDefaultTenantBusinessDataIfEmptyAsync(tenantDb, tenantId, newUser.Id);

        return newUser;
    }

    /// <summary>
    /// 构建租户管理员角色初始授权（C354 T1）。
    /// </summary>
    /// <param name="roleId">租户管理员角色 Id</param>
    /// <param name="allowedMenuIds">套餐允许集；null = 未分配套餐，回落全量 <see cref="TenantAdminMenuDefaults.MenuIds"/>（未分配 = 不限，与 C353 §3.2 对齐）</param>
    internal static List<TenantRoleMenuEntity> CreateTenantAdminRoleMenus(long roleId, IReadOnlyCollection<long>? allowedMenuIds = null)
    {
        var menuIds = allowedMenuIds ?? TenantAdminMenuDefaults.MenuIds;
        return menuIds.Select(menuId => new TenantRoleMenuEntity
        {
            Id = SnowFlakeIdHelper.Instance.NextId(),
            RoleId = roleId,
            MenuId = menuId,
        }).ToList();
    }

    /// <summary>
    /// 解析新租户 admin 角色初始授权允许集（C354 T1）：
    /// PackageId == null ⇒ 全量 <see cref="TenantAdminMenuDefaults.MenuIds"/>（未分配套餐 = 不限）；
    /// 有套餐 ⇒ 全量默认授权 ∩ 套餐闭包（创建入口已校验套餐可用，正常路径不产生 Dangling）。
    /// </summary>
    /// <param name="packageId">租户套餐 Id（null = 未分配）</param>
    internal async Task<IReadOnlyList<long>> ResolveInitialAdminMenuIdsAsync(long? packageId)
    {
        if (packageId == null)
        {
            return TenantAdminMenuDefaults.MenuIds;
        }

        var ceiling = await TenantMenuSyncApplier.ComputeCeilingAsync(_sqlSugarClient, new SystemTenantEntity
        {
            Id = 0,
            PackageId = packageId,
        });
        return IntersectDefaultAdminMenuIds(ceiling);
    }

    /// <summary>
    /// 全量默认租户管理员授权 ∩ 套餐闭包（C354 T1 交集语义单元，供创建期初始授权与守护测试共用；
    /// 仅 Bounded 形态有意义，无套餐 / 悬空由调用方前置分支处理）。
    /// </summary>
    internal static IReadOnlyList<long> IntersectDefaultAdminMenuIds(TenantMenuSyncPackageCeiling ceiling)
    {
        return TenantAdminMenuDefaults.MenuIds.Where(u => ceiling.MenuIds.Contains(u)).ToList();
    }

    internal static SystemTenantEntity CreateOrdinaryTenantForPersistence(AddTenantRequest input)
    {
        var tenant = input.Adapt<SystemTenantEntity>();
        tenant.Id = tenant.Id > 0 ? tenant.Id : SnowFlakeIdHelper.Instance.NextId();
        tenant.TenantType = TenantTypeEnum.Db;
        tenant.DbType = DbType.PostgreSQL;
        tenant.ConfigId = tenant.Id.ToString();
        tenant.Remark = input.Name;
        tenant.CreatedTime = tenant.CreatedTime == 0 ? TimerHelper.UnixTimeSeconds() : tenant.CreatedTime;
        // AddTenantRequest 继承 TenantResponse（RowVersion null），Adapt 会把实体默认 0 覆盖为 null，
        // 插入后 row_version 为 NULL，FakeDeleteAsync 的乐观锁 WHERE row_version=0 永不命中导致删除 409。兜底回 0（与 InnerAddAsync 一致）。
        tenant.RowVersion ??= 0;
        return tenant;
    }

    /// <summary>
    /// 解析租管登录密文：邮箱自助注册传入的覆盖密码优先，缺省回落系统配置默认密码加密。
    /// </summary>
    /// <param name="overrideEncryptedPassword">覆盖密码（已加密）</param>
    /// <param name="configuredPassword">系统配置默认密码（明文）</param>
    /// <returns>租管登录密文</returns>
    internal static string ResolveTenantAdminEncryptedPassword(string? overrideEncryptedPassword, string configuredPassword)
    {
        return overrideEncryptedPassword ?? CryptogramUtility.HashPassword(configuredPassword);
    }

    private SqlSugarScopeProvider CreateTenantDatabaseConnection(SystemTenantEntity tenant)
    {
        if (!CanCreateTenantDatabase(tenant))
        {
            throw BusinessError.Oh(TenantErrorCode.TenantDbTypeForbidden);
        }

        var mainConfig = _dbOptions.ConnectionConfigs.FirstOrDefault(u => u.ConfigId.ToString() == SqlSugarConst.MainConfigId);
        if (mainConfig == null)
        {
            throw new InvalidOperationException("主库连接配置缺失。");
        }

        // 兼容两种形态：内存实体（ProvisionTenantDatabase 后）为密文，库里读回的可能是密文或存量明文，统一解密后使用
        var tenantConnectionString = TenantConnectionCrypto.Decrypt(tenant.Connection);
        if (string.Equals(mainConfig.ConnectionString, tenantConnectionString, StringComparison.OrdinalIgnoreCase))
        {
            throw BusinessError.Oh(TenantErrorCode.TenantDbTypeForbidden);
        }

        // 初始化开关随代码默认值（C278）：普通租户创建时同样自动建库建表；
        // 租户库通常已由 PostgreSqlTenantDatabaseProvisioner 预建，CreateDatabase 幂等跳过
        var config = new DbConnectionConfig
        {
            ConfigId = tenant.Id.ToString(),
            DbType = tenant.DbType,
            ConnectionString = tenantConnectionString,
        };

        var iTenant = _sqlSugarClient.AsTenant();
        SqlSugarSetup.InitTenantDatabase(iTenant, config);
        return iTenant.GetConnectionScope(tenant.Id.ToString());
    }

    private static async Task<Dictionary<string, TenantOrganizationEntity>> InitTenantOrganizationsAsync(SqlSugarScopeProvider tenantDb, long tenantId, string tenantName)
    {
        var orgsByKey = new Dictionary<string, TenantOrganizationEntity>(StringComparer.Ordinal);
        foreach (var template in TenantDefaultDataTemplates.OrganizationTemplates)
        {
            var org = new TenantOrganizationEntity
            {
                TenantId = tenantId,
                Pid = template.ParentKey == null ? 0 : orgsByKey[template.ParentKey].Id,
                Name = template.Key == TenantDefaultDataTemplates.RootOrganizationKey ? tenantName : template.Name,
                Code = template.Code,
                Type = template.Type,
                Level = template.Level,
                OrderNo = template.OrderNo,
                Remark = template.Key == TenantDefaultDataTemplates.RootOrganizationKey ? tenantName : template.Remark,
            };
            org = await tenantDb.Insertable(org).ExecuteReturnEntityAsync();
            orgsByKey.Add(template.Key, org);
        }

        return orgsByKey;
    }

    private static async Task<Dictionary<string, TenantRoleEntity>> InitTenantRolesAsync(SqlSugarScopeProvider tenantDb, long tenantId)
    {
        var rolesByCode = new Dictionary<string, TenantRoleEntity>(StringComparer.Ordinal);
        foreach (var template in TenantDefaultDataTemplates.RoleTemplates)
        {
            var role = new TenantRoleEntity
            {
                TenantId = tenantId,
                Name = template.Name,
                Code = template.Code,
                OrderNo = template.OrderNo,
                DataScope = template.DataScope,
                Remark = template.Remark,
            };
            role = await tenantDb.Insertable(role).ExecuteReturnEntityAsync();
            rolesByCode.Add(template.Code, role);
        }

        return rolesByCode;
    }

    private static async Task<Dictionary<string, TenantPositionEntity>> InitTenantPositionsAsync(SqlSugarScopeProvider tenantDb, long tenantId)
    {
        var positionsByCode = new Dictionary<string, TenantPositionEntity>(StringComparer.Ordinal);
        foreach (var template in TenantDefaultDataTemplates.PositionTemplates)
        {
            var position = new TenantPositionEntity
            {
                TenantId = tenantId,
                Name = template.Name,
                Code = template.Code,
                OrderNo = template.OrderNo,
                Remark = template.Remark,
            };
            position = await tenantDb.Insertable(position).ExecuteReturnEntityAsync();
            positionsByCode.Add(template.Code, position);
        }

        return positionsByCode;
    }

    [NonAction]
    public static TenantUserEntity CreateTenantAdminUser(TenantResponse tenant, string encryptedPassword, long orgId, long posId)
    {
        return new TenantUserEntity
        {
            TenantId = tenant.Id,
            Account = tenant.AdminAccount,
            Password = encryptedPassword,
            NickName = "租管",
            RealName = "租管",
            CreatedName = "租管",
            UpdatedName = "",
            DeletedName = "",
            Avatar = "",
            Email = tenant.Email ?? "",
            Phone = tenant.Phone ?? "",
            AccountType = AccountTypeEnum.SystemAdmin,
            OrganizationId = orgId,
            PosId = posId,
            BirthdayTime = new DateTimeOffset(DateTime.Parse("1986-06-28")).ToUnixTimeSeconds(),
            Remark = "租管" + tenant.Name,
            Nation = "",
            IdCardNum = "",
            Address = "",
            PoliticalOutlook = "",
            College = "",
            OfficePhone = "",
            EmergencyContact = "",
            EmergencyPhone = "",
            EmergencyAddress = "",
            Introduction = "",
            JobNum = "",
            PosLevel = "",
            PosTitle = "",
            Expertise = "",
            OfficeZone = "",
            Office = "",
            LastLoginIp = "",
            LastLoginAddress = "",
            LastLoginDevice = "",
            Signature = "",
        };
    }

    /// <summary>
    /// 删除租户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除租户")]
    public async Task DeleteTenantAsync(DeleteTenantRequest input)
    {
        var tenant = await _systemTenantRepository.GetFirstAsync(u => u.Id == input.Id);
        // 默认租户已经是独立库租户，仍然属于系统初始化根租户，禁止删除。
        if (IsProtectedDefaultTenant(tenant) || IsDefaultTenantId(input.Id))
        {
            throw BusinessError.Oh(TenantErrorCode.DeleteDefaultTenantForbidden);
        }

        if (tenant == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        // 仅软删除主库租户记录，不连接租户独立库、不清理库内业务数据。
        await _systemTenantRepository.FakeDeleteAsync(tenant);

        // C353：同步状态行随租户删除一并清理，避免状态残留导致扫描计划器拿死租户做版本比对
        var syncStateRepository = _serviceProvider.GetRequiredService<SqlSugarRepository<SystemTenantMenuSyncStateEntity>>();
        await syncStateRepository.AsDeleteable().Where(u => u.Id == input.Id).ExecuteCommandAsync();

        await CacheTenantAsync(input.Id);
    }

    /// <summary>
    /// 更新租户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新租户")]
    public async Task UpdateTenantAsync(UpdateTenantRequest input)
    {
        var tenant = await _systemTenantRepository.GetFirstAsync(u => u.Id == input.Id);
        if (tenant == null)
        {
            throw BusinessError.Oh(TenantErrorCode.DefaultTenantStatusChangeForbidden);
        }

        ValidateOrdinaryTenantInput(input);

        // C354 T5：套餐分配/变更通道（C353 触发②唯一挂接点）——
        // PackageId == null = 解除套餐（未分配 = 不限语义）；有值须存在且启用（悬空拒绝）
        var package = await FindPackageAsync(input.PackageId);
        ValidateTenantPackageAssignment(tenant, package, input.PackageId);
        var packageChanged = tenant.PackageId != input.PackageId;

        var tenantDb = GetTenantDbConnectionScope(input.Id);
        await UpdateTenantIdentityDataAsync(tenantDb, input);

        // 白名单更新主库可编辑列；复用 CreateOrdinaryTenantForPersistence（Add 专用）会全列覆盖，
        // 把 Remark 替换成组织名、CreatedTime 重置为当前时间
        await _systemTenantRepository.AsUpdateable(new SystemTenantEntity
                       {
                           Id = input.Id,
                           Connection = TenantConnectionCrypto.Encrypt(input.Connection),
                           Host = input.Host,
                           ExpireTime = input.ExpireTime,
                           GracePeriodDays = input.GracePeriodDays,
                           Remark = input.Remark,
                           PackageId = input.PackageId,
                       })
                       .UpdateColumns(u => new { u.Connection, u.Host, u.ExpireTime, u.GracePeriodDays, u.Remark, u.PackageId, u.UpdateTime, u.UpdatedId, u.UpdatedName })
                       .ExecuteCommandAsync();

        await CacheTenantAsync(input.Id);

        if (packageChanged)
        {
            // C353 触发②：租户套餐分配/变更成功即入队该租户菜单同步（ceiling 变化 → role-menu 异步收敛）
            _tenantMenuSyncQueue.TryEnqueue(input.Id, "package-assigned");
        }
    }

    /// <summary>
    /// 按套餐 Id 查主库套餐行（含软删行；null PackageId 直接返回 null）。
    /// </summary>
    private async Task<GlobalPackageEntity?> FindPackageAsync(long? packageId)
    {
        if (packageId == null)
        {
            return null;
        }

        var globalPackageRepository = _serviceProvider.GetRequiredService<SqlSugarRepository<GlobalPackageEntity>>();
        return await globalPackageRepository.AsQueryable()
                                  .Filter(null, true)
                                  .FirstAsync(u => u.Id == packageId);
    }

    /// <summary>
    /// 校验套餐可用于分配（创建/变更共用）：有值时必须存在、未软删且启用。
    /// </summary>
    /// <param name="packageId">请求的套餐 Id（null = 未分配，放行）</param>
    /// <param name="package">按 Id 查得的套餐行</param>
    internal static void ValidatePackageUsableForAssignment(long? packageId, GlobalPackageEntity? package)
    {
        if (packageId == null)
        {
            return;
        }

        if (package == null || package.IsDeleted == true || package.IsEnabled != true)
        {
            throw BusinessError.Oh(TenantErrorCode.TenantPackageNotUsable);
        }
    }

    /// <summary>
    /// 校验租户套餐变更（C354 T5）：套餐可用性 + 默认租户套餐不可改
    /// （Enterprise 由启动修复锁定为「全部非 SuperAdminOnly 种子菜单」天花板，禁止经 API 收窄）。
    /// </summary>
    /// <param name="tenant">既有租户行</param>
    /// <param name="package">请求套餐 Id 对应的套餐行（PackageId 为 null 时为 null）</param>
    /// <param name="requestedPackageId">请求的套餐 Id（null = 解除套餐）</param>
    internal static void ValidateTenantPackageAssignment(SystemTenantEntity tenant, GlobalPackageEntity? package, long? requestedPackageId)
    {
        ValidatePackageUsableForAssignment(requestedPackageId, package);

        if (IsProtectedDefaultTenant(tenant) && requestedPackageId != tenant.PackageId)
        {
            throw BusinessError.Oh(TenantErrorCode.DefaultTenantPackageChangeForbidden);
        }
    }

    internal static async Task UpdateTenantIdentityDataAsync(SqlSugarScopeProvider tenantDb, UpdateTenantRequest input)
    {
        if (tenantDb == null)
        {
            throw BusinessError.Oh(TenantErrorCode.DefaultTenantStatusChangeForbidden);
        }

        var isExist = await tenantDb.Queryable<TenantOrganizationEntity>()
                                    .Filter(null, true)
                                    .AnyAsync(u => u.Name == input.Name && u.Id != input.OrganizationId);
        if (isExist)
        {
            throw BusinessError.Oh(TenantErrorCode.TenantNameAlreadyExists);
        }

        isExist = await tenantDb.Queryable<TenantUserEntity>()
                                .Filter(null, true)
                                .AnyAsync(u => u.Account == input.AdminAccount && u.Id != input.UserId);
        if (isExist)
        {
            throw BusinessError.Oh(TenantErrorCode.TenantAdminNameAlreadyExists);
        }

        await tenantDb.Updateable<TenantOrganizationEntity>()
                      .SetColumns(u => new TenantOrganizationEntity { Name = input.Name, })
                      .Where(u => u.Id == input.OrganizationId && u.TenantId == input.Id)
                      .ExecuteCommandAsync();

        await tenantDb.Updateable<TenantUserEntity>()
                      .SetColumns(u => new TenantUserEntity { Account = input.AdminAccount, Phone = input.Phone, Email = input.Email, })
                      .Where(u => u.Id == input.UserId && u.TenantId == input.Id)
                      .ExecuteCommandAsync();
    }

    /// <summary>
    /// 重置租户管理员密码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("重置租户管理员密码")]
    [ApiDescriptionSettings(Name = "ResetPwd")]
    public async Task<HttpJsonResultData<ResetPwdResponse>> ResetPwdAsync(TenantUserRequest input)
    {
        var tenant = await GetTenantByAdminUserIdAsync(input.UserId);
        if (tenant == null)
        {
            throw BusinessError.Oh(SystemUserErrorCode.AccountNotFound);
        }

        var password = await _systemConfigService.GetConfigValueAsync<string>(CommonConst.SysPassword);
        var encryptPassword = CryptogramUtility.HashPassword(password);
        var tenantDb = GetTenantDbConnectionScope(tenant.Id);
        await tenantDb.Updateable<TenantUserEntity>()
                      .SetColumns(u => new TenantUserEntity { Password = encryptPassword, })
                      .Where(u => u.Id == input.UserId)
                      .ExecuteCommandAsync();
        return HttpJsonResultData<ResetPwdResponse>.Success(new ResetPwdResponse
        {
            NewPassword = password,
        });
    }

    /// <summary>
    /// 缓存所有租户
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    [NonAction]
    public async Task CacheTenantAsync(long tenantId = 0)
    {
        // 移除 ISqlSugarClient 中的库连接
        if (tenantId > 0)
        {
            _systemTenantRepository.AsTenant().RemoveConnection(tenantId);
        }

        var tenantList = await _systemTenantRepository.GetListAsync();
        _systemCacheService.Set(CacheConst.KeyTenant, tenantList);
    }

    /// <summary>
    /// 创建租户数据库
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "CreateDb")]
    [HttpPost]
    [DisplayName("创建租户数据库")]
    public async Task CreateDbAsync(TenantRequest input)
    {
        var tenant = await _systemTenantRepository.GetSingleAsync(u => u.Id == input.Id);
        if (tenant == null)
        {
            return;
        }

        if (tenant.TenantType != TenantTypeEnum.Db || IsProtectedDefaultTenant(tenant))
        {
            throw BusinessError.Oh(TenantErrorCode.TenantDbTypeForbidden);
        }

        ProvisionTenantDatabase(tenant);
        await _systemTenantRepository.AsUpdateable(tenant)
                              .UpdateColumns(u => new { u.TenantType, u.DbType, u.Connection, u.ConfigId, })
                              .ExecuteCommandAsync();

        if (!CanCreateTenantDatabase(tenant))
        {
            throw BusinessError.Oh(TenantErrorCode.TenantDbTypeForbidden);
        }

        if (await IsTenantDatabaseInitializedAsync(tenant))
        {
            throw BusinessError.Oh(TenantErrorCode.TenantDatabaseAlreadyCreated);
        }

        var tenantDb = CreateTenantDatabaseConnection(tenant);
        if (await TenantAdminExistsAsync(tenantDb, tenant.Id))
        {
            throw BusinessError.Oh(TenantErrorCode.TenantDatabaseAlreadyCreated);
        }

        await InitTenantDatabaseBaseDataAsync(tenant, tenantDb);
    }

    internal static void ValidateOrdinaryTenantInput(AddTenantRequest input)
    {
        if (input.TenantType != TenantTypeEnum.Db)
        {
            throw BusinessError.Oh(TenantErrorCode.IdIsolationForbiddenForNormalTenant);
        }

        if (input.DbType != DbType.PostgreSQL)
        {
            throw BusinessError.Oh(TenantErrorCode.TenantDbTypeForbidden);
        }

        input.TenantType = TenantTypeEnum.Db;
        input.DbType = DbType.PostgreSQL;
    }

    internal static bool CanCreateTenantDatabase(SystemTenantEntity tenant)
    {
        return tenant != null
               && tenant.TenantType == TenantTypeEnum.Db
               && tenant.DbType == DbType.PostgreSQL
               && !IsProtectedDefaultTenant(tenant)
               && !string.IsNullOrWhiteSpace(tenant.Connection);
    }

    private void ProvisionTenantDatabase(SystemTenantEntity tenant)
    {
        var mainConfig = _dbOptions.ConnectionConfigs.FirstOrDefault(u => u.ConfigId.ToString() == SqlSugarConst.MainConfigId);
        if (mainConfig == null)
        {
            throw new InvalidOperationException("主库连接配置缺失。");
        }

        var plan = _tenantDatabaseProvisioner.Provision(tenant.Id, mainConfig, ResolveProvisioningConfig(mainConfig));
        tenant.TenantType = TenantTypeEnum.Db;
        tenant.DbType = DbType.PostgreSQL;
        tenant.ConfigId = tenant.Id.ToString();
        // 落库前统一加密：实体 Connection 之后只以密文形态持久化，主串等值比较与建连一律经 TenantConnectionCrypto 解密
        tenant.Connection = TenantConnectionCrypto.Encrypt(plan.TenantConnectionString);
    }

    /// <summary>
    /// 解析建库专用连接：显式配置 <see cref="DbConnectionOptions.ProvisioningConnectionString"/>（gfx_provisioner，非 superuser）优先；
    /// 缺省回落主库连接并告警（降级模式，自断对 superuser 无效）。
    /// </summary>
    private DbConnectionConfig ResolveProvisioningConfig(DbConnectionConfig mainConfig)
    {
        if (string.IsNullOrWhiteSpace(_dbOptions.ProvisioningConnectionString))
        {
            _logger.LogWarning("DbConnection.ProvisioningConnectionString 未配置，租户建库回落使用主库连接；建议配置 gfx_provisioner 专用账号（CREATEROLE+CREATEDB 非 superuser）分离建库通道。");
            return mainConfig;
        }

        return new DbConnectionConfig
        {
            DbType = mainConfig.DbType,
            ConnectionString = _dbOptions.ProvisioningConnectionString,
        };
    }

    private async Task InitTenantDatabaseBaseDataAsync(SystemTenantEntity tenant, SqlSugarScopeProvider tenantDb)
    {
        var sourceOrganizationList = await tenantDb.Queryable<TenantOrganizationEntity>().Filter(null, true).Where(u => u.TenantId == tenant.Id).ToListAsync();
        var sourceRoleList = await tenantDb.Queryable<TenantRoleEntity>().Filter(null, true).Where(u => u.TenantId == tenant.Id).ToListAsync();
        var sourcePosList = await tenantDb.Queryable<TenantPositionEntity>().Filter(null, true).Where(u => u.TenantId == tenant.Id).ToListAsync();
        var sourceUserList = await tenantDb.Queryable<TenantUserEntity>().Filter(null, true).Where(u => u.TenantId == tenant.Id).ToListAsync();
        var sourceUserIds = sourceUserList.Select(u => u.Id).ToList();
        var sourceRoleIds = sourceRoleList.Select(u => u.Id).ToList();
        var sourceUserRoleList = await tenantDb.Queryable<TenantUserRoleEntity>().Filter(null, true).Where(u => sourceUserIds.Contains(u.UserId)).ToListAsync();
        var sourceRoleMenuList = await tenantDb.Queryable<TenantRoleMenuEntity>().Filter(null, true).Where(u => sourceRoleIds.Contains(u.RoleId)).ToListAsync();

        await UpsertTenantDbDataAsync(tenantDb, sourceOrganizationList);
        await UpsertTenantDbDataAsync(tenantDb, sourceRoleList);
        await UpsertTenantDbDataAsync(tenantDb, sourcePosList);
        await UpsertTenantDbDataAsync(tenantDb, sourceUserList);
        await UpsertTenantDbDataAsync(tenantDb, sourceUserRoleList);
        await UpsertTenantDbDataAsync(tenantDb, sourceRoleMenuList);
        await DefaultTenantDatabaseSeeder.SeedDefaultTenantBusinessDataIfEmptyAsync(tenantDb, tenant.Id, tenant.UserId);
    }

    private static async Task UpsertTenantDbDataAsync<T>(SqlSugarScopeProvider tenantDb, List<T> data) where T : class, new()
    {
        if (data.Count == 0)
        {
            return;
        }

        var entityInfo = tenantDb.EntityMaintenance.GetEntityInfo<T>();
        var primaryKeyColumns = entityInfo.Columns.Where(u => u.IsPrimarykey).Select(u => u.PropertyName).ToArray();
        if (primaryKeyColumns.Length == 0)
        {
            await tenantDb.Insertable(data).ExecuteCommandAsync();
            return;
        }

        tenantDb.QueryFilter.ClearAndBackup();
        try
        {
            var storageable = tenantDb.Storageable(data);
            storageable.WhereColumns(primaryKeyColumns);
            var storage = storageable.ToStorage();
            await storage.AsInsertable.ExecuteCommandAsync();
            await storage.AsUpdateable.ExecuteCommandAsync();
        }
        finally
        {
            tenantDb.QueryFilter.Restore();
        }
    }

    private async Task<bool> IsTenantDatabaseInitializedAsync(SystemTenantEntity tenant)
    {
        try
        {
            var tenantDb = GetTenantDbConnectionScope(tenant.Id);
            return tenantDb != null && await TenantAdminExistsAsync(tenantDb, tenant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "租户库尚未初始化或暂不可连接，允许继续创建。TenantId={TenantId}, ConfigId={ConfigId}", tenant.Id, tenant.ConfigId);
            return false;
        }
    }

    private static Task<bool> TenantAdminExistsAsync(SqlSugarScopeProvider tenantDb, long tenantId)
    {
        return tenantDb.Queryable<TenantUserEntity>()
                       .AnyAsync(u => u.TenantId == tenantId && u.AccountType == AccountTypeEnum.SystemAdmin);
    }

    /// <summary>
    /// 获取租户下的用户列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取租户下的用户列表")]
    [ApiDescriptionSettings(Name = "UserList")]
    public async Task<List<TenantUserResponse>> UserListAsync(TenantIdRequest input)
    {
        if (input.TenantId <= 0)
        {
            return new List<TenantUserResponse>();
        }

        var tenantDb = GetTenantDbConnectionScope(input.TenantId);
        if (tenantDb == null)
        {
            return new List<TenantUserResponse>();
        }

        return await tenantDb.Queryable<TenantUserEntity>()
                             .Filter(null, true)
                             .Where(u => u.TenantId == input.TenantId)
                             .Select<TenantUserResponse>()
                             .ToListAsync();
    }

    /// <summary>
    /// 获取租户数据库连接
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public SqlSugarScopeProvider GetTenantDbConnectionScope(long tenantId)
    {
        return _tenantDbConnectionProvider.GetTenantDbConnectionScope(tenantId);
    }

    private async Task HydrateTenantAdminAsync(TenantResponse tenant)
    {
        try
        {
            var tenantDb = GetTenantDbConnectionScope(tenant.Id);
            if (tenantDb == null)
            {
                return;
            }

            var org = await tenantDb.Queryable<TenantOrganizationEntity>().FirstAsync(u => u.Id == tenant.OrganizationId);
            var user = await tenantDb.Queryable<TenantUserEntity>().FirstAsync(u => u.Id == tenant.UserId);
            tenant.Name = org?.Name ?? tenant.Remark ?? string.Empty;
            tenant.AdminAccount = user?.Account ?? string.Empty;
            tenant.Phone = user?.Phone ?? string.Empty;
            tenant.Email = user?.Email ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "读取租户管理员信息失败，已跳过租户库补全。TenantId={TenantId}, ConfigId={ConfigId}", tenant.Id, tenant.ConfigId);
        }
    }

    private Task<SystemTenantEntity> GetTenantByAdminUserIdAsync(long userId)
    {
        return _systemTenantRepository.AsQueryable().Filter(null, true).FirstAsync(u => u.UserId == userId);
    }

    internal static bool IsProtectedDefaultTenant(SystemTenantEntity tenant)
    {
        return tenant != null && (IsDefaultTenantId(tenant.Id) || tenant.ConfigId == SqlSugarConst.MainConfigId);
    }

    internal static bool IsDefaultTenantId(long tenantId)
    {
        return tenantId == TenantDefaultDataTemplates.MainTenantId;
    }
}