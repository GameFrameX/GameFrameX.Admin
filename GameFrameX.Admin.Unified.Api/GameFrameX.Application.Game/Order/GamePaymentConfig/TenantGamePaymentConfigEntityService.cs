using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Order.GamePaymentConfig.Dto;
using GameFrameX.Payment.Abstract;
using GameFrameX.Core.Service;
using GameFrameX.Core.Service.Tenant;

#pragma warning disable CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Order.GamePaymentConfig;

/// <summary>
/// 游戏支付配置管理服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGamePaymentConfigEntityService
    : GenericCrudService<Entity.Tenant.Game.TenantGamePaymentConfigEntity,
        AddGamePaymentConfigRequest, UpdateGamePaymentConfigRequest,
        GamePaymentConfigResponse, PageGamePaymentConfigRequest>
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;
    private readonly SqlSugarRepository<TenantGameAppPackageEntity> _packageRepository;
    private readonly IPaymentConfigSchemaProvider _schemaProvider;

    public TenantGamePaymentConfigEntityService(
        SqlSugarRepository<Entity.Tenant.Game.TenantGamePaymentConfigEntity> repository,
        SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository,
        SqlSugarRepository<TenantGameAppPackageEntity> packageRepository,
        IPaymentConfigSchemaProvider schemaProvider)
        : base(repository)
    {
        _gameAppRepository = gameAppRepository;
        _packageRepository = packageRepository;
        _schemaProvider = schemaProvider;
    }

    protected override ISugarQueryable<GamePaymentConfigResponse> BuildPageQuery(PageGamePaymentConfigRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.GameAppId, "支付配置");
        var searchKey = input.SearchKey?.Trim();
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                      u.PayChannel.Contains(searchKey) ||
                                      u.Remark.Contains(searchKey))
                         .Where(u => u.GameAppIdNormalized == appId)
                         .WhereIF(input.PackageId.HasValue, u => u.PackageIdNormalized == input.PackageId.Value)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.PayChannel), u => u.PayChannel == NormalizeChannel(input.PayChannel))
                         .WhereIF(input.IsSandbox.HasValue, u => u.IsSandbox == input.IsSandbox.Value)
                         .WhereIF(input.IsActive.HasValue, u => u.IsEnabled == input.IsActive.Value)
                         .Select<GamePaymentConfigResponse>();
    }

    /// <summary>
    /// 新增支付配置
    /// </summary>
    public override async Task<GamePaymentConfigResponse> AddAsync(AddGamePaymentConfigRequest input)
    {
        ValidateConfigJson(input.ConfigJson);
        var payChannel = NormalizeChannel(input.PayChannel);
        var scope = BuildInputScope(input.GameAppId, input.PackageId);
        await ValidateScopeAsync(scope.GameAppId, scope.PackageId);
        await CheckUniqueAsync(scope.GameAppId, scope.PackageId, payChannel, input.IsSandbox);

        var entity = input.Adapt<Entity.Tenant.Game.TenantGamePaymentConfigEntity>();
        entity.PayChannel = payChannel;
        ApplyScope(entity, scope);
        entity.IsEnabled = true;
        // C371：校验通过后（明文形态）、落库之前做渠道 schema 驱动的敏感字段级加密
        entity.ConfigJson = PaymentConfigCrypto.EncryptConfigJson(payChannel, input.ConfigJson);
        await Repository.InsertAsync(entity);
        return entity.Adapt<GamePaymentConfigResponse>();
    }

    /// <summary>
    /// 更新支付配置
    /// </summary>
    public override async Task UpdateAsync(UpdateGamePaymentConfigRequest input)
    {
        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh("支付配置不存在");
        }

        var payChannel = NormalizeChannel(input.PayChannel);
        var scope = BuildInputScope(input.GameAppId, input.PackageId);
        await ValidateScopeAsync(scope.GameAppId, scope.PackageId);
        await CheckUniqueAsync(scope.GameAppId, scope.PackageId, payChannel, input.IsSandbox, input.Id);

        ApplyScope(entity, scope);
        entity.PayChannel = payChannel;
        entity.IsSandbox = input.IsSandbox;
        entity.Remark = input.Remark;

        ValidateConfigJson(input.ConfigJson);
        // C371：校验通过后（明文形态）、落库之前加密；前端回传密文原值时幂等跳过（不换密文）
        entity.ConfigJson = PaymentConfigCrypto.EncryptConfigJson(payChannel, input.ConfigJson);
        entity.RowVersion = input.RowVersion;

        await Repository.AsUpdateable(entity)
                        .UpdateColumns(e => new
                        {
                            e.AppId,
                            e.GameAppId,
                            e.PackageId,
                            e.GameAppIdNormalized,
                            e.PackageIdNormalized,
                            e.PayChannel,
                            e.IsSandbox,
                            e.ConfigJson,
                            e.Remark,
                        })
                        .ExecuteCommandWithOptLockAsync(true);
    }

    /// <summary>
    /// 分页查询（配置脱敏）
    /// </summary>
    [HttpPost(CrudActionRoutes.Page)]
    [ApiDescriptionSettings(Name = "Page")]
    public override async Task<SqlSugarPagedList<GamePaymentConfigResponse>> PageAsync(PageGamePaymentConfigRequest input)
    {
        var result = await base.PageAsync(input);
        foreach (var item in result.Items)
        {
            item.ConfigJson = SensitiveFieldMask.MaskValue;
        }

        return result;
    }

    /// <summary>
    /// 详情（配置脱敏）
    /// </summary>
    [HttpGet(CrudActionRoutes.Detail)]
    [ApiDescriptionSettings(Name = "Detail")]
    public override async Task<GamePaymentConfigResponse> GetAsync([FromQuery] BaseIdRequest input)
    {
        var entity = await InnerGetAsync(input);
        return entity == null ? default : entity.Adapt<GamePaymentConfigResponse>();
    }

    /// <summary>
    /// 获取指定支付渠道的字段 schema
    /// </summary>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Schema")]
    public PaymentConfigSchema GetSchema([FromQuery] string payChannel)
    {
        return _schemaProvider.GetSchema(NormalizeChannel(payChannel));
    }

    /// <summary>
    /// 获取所有支持渠道的清单
    /// </summary>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Channels")]
    public List<PaymentConfigChannel> GetChannels()
    {
        return _schemaProvider.GetChannels();
    }

    public static PaymentConfigScope BuildScope(long? gameAppId, long? packageId)
    {
        var normalizedGameAppId = NormalizeNullableId(gameAppId);
        var normalizedPackageId = NormalizeNullableId(packageId);
        return new PaymentConfigScope(
            normalizedGameAppId == 0 ? null : normalizedGameAppId,
            normalizedPackageId == 0 ? null : normalizedPackageId,
            normalizedGameAppId,
            normalizedPackageId);
    }

    public static string? GetScopeError(long? gameAppId, long? packageId)
    {
        var scope = BuildScope(gameAppId, packageId);
        if (scope.PackageId.HasValue && !scope.GameAppId.HasValue)
        {
            return "分包支付配置必须指定应用";
        }

        return null;
    }

    public static Entity.Tenant.Game.TenantGamePaymentConfigEntity? PickBestConfig(
        IEnumerable<Entity.Tenant.Game.TenantGamePaymentConfigEntity> configs,
        long? gameAppId,
        long? packageId)
    {
        var scope = BuildScope(gameAppId, packageId);
        return configs
               .OrderByDescending(config => GetScopePriority(config, scope))
               .FirstOrDefault(config => GetScopePriority(config, scope) > 0);
    }

    private async Task CheckUniqueAsync(long? gameAppId, long? packageId, string payChannel, bool isSandbox, long? excludeId = null)
    {
        var scope = BuildScope(gameAppId, packageId);
        var gameAppIdNormalized = scope.GameAppIdNormalized;
        var packageIdNormalized = scope.PackageIdNormalized;
        var exists = excludeId.HasValue
                         ? await Repository.IsAnyAsync(x => x.GameAppIdNormalized == gameAppIdNormalized &&
                                                             x.PackageIdNormalized == packageIdNormalized &&
                                                             x.PayChannel == payChannel &&
                                                             x.IsSandbox == isSandbox &&
                                                             x.Id != excludeId.Value)
                         : await Repository.IsAnyAsync(x => x.GameAppIdNormalized == gameAppIdNormalized &&
                                                             x.PackageIdNormalized == packageIdNormalized &&
                                                             x.PayChannel == payChannel &&
                                                             x.IsSandbox == isSandbox);

        if (exists)
        {
            throw BusinessError.Oh("同一配置范围、支付渠道和沙盒标记下的支付配置已存在");
        }
    }

    private async Task ValidateScopeAsync(long? gameAppId, long? packageId)
    {
        var error = GetScopeError(gameAppId, packageId);
        if (!string.IsNullOrWhiteSpace(error))
        {
            throw BusinessError.Oh(error);
        }

        if (gameAppId.HasValue && !await _gameAppRepository.IsAnyAsync(u => u.Id == gameAppId.Value))
        {
            throw BusinessError.Oh("游戏应用不存在或无权限");
        }

        if (!packageId.HasValue)
        {
            return;
        }

        var package = await _packageRepository.GetFirstAsync(u => u.Id == packageId.Value);
        if (package == null)
        {
            throw BusinessError.Oh("分包不存在或无权限");
        }

        if (package.GameAppId != gameAppId)
        {
            throw BusinessError.Oh("分包不属于当前应用");
        }
    }

    private static PaymentConfigScope BuildInputScope(long gameAppId, long? packageId)
    {
        var effectiveGameAppId = AppScopeRules.EnsureFactAppId(gameAppId, "支付配置");
        return BuildScope(effectiveGameAppId, packageId);
    }

    private static void ApplyScope(Entity.Tenant.Game.TenantGamePaymentConfigEntity entity, PaymentConfigScope scope)
    {
        entity.GameAppId = scope.GameAppId;
        entity.PackageId = scope.PackageId;
        entity.GameAppIdNormalized = scope.GameAppIdNormalized;
        entity.PackageIdNormalized = scope.PackageIdNormalized;
        entity.AppId = scope.GameAppIdNormalized;
    }

    private static long NormalizeNullableId(long? id)
    {
        var idValue = id.GetValueOrDefault();
        return idValue > 0 ? idValue : 0;
    }

    private static int GetScopePriority(Entity.Tenant.Game.TenantGamePaymentConfigEntity config, PaymentConfigScope scope)
    {
        var configGameAppId = config.GameAppIdNormalized;
        var configPackageId = config.PackageIdNormalized;
        if (scope.GameAppIdNormalized > 0 &&
            scope.PackageIdNormalized > 0 &&
            configGameAppId == scope.GameAppIdNormalized &&
            configPackageId == scope.PackageIdNormalized)
        {
            return 3;
        }

        if (scope.GameAppIdNormalized > 0 &&
            configGameAppId == scope.GameAppIdNormalized &&
            configPackageId == 0)
        {
            return 2;
        }

        return configGameAppId == 0 && configPackageId == 0 ? 1 : 0;
    }

    private static string NormalizeChannel(string payChannel)
    {
        return payChannel?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    private static void ValidateConfigJson(string configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            throw BusinessError.Oh("支付配置不能为空");
        }

        try
        {
            using var document = JsonDocument.Parse(configJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw BusinessError.Oh("支付配置必须是JSON对象");
            }
        }
        catch (JsonException ex)
        {
            throw BusinessError.Oh($"支付配置JSON格式错误: {ex.Message}");
        }
    }
}
#pragma warning restore CS8604, CS8629