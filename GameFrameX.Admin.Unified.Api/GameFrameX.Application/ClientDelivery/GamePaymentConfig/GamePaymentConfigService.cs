using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Application.ClientDelivery.GamePaymentConfig.Dto;
using GameFrameX.Payment.Abstract;
using GameFrameX.Core.Service;
using GameFrameX.Core.Service.Tenant;

#pragma warning disable CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.ClientDelivery.GamePaymentConfig;

/// <summary>
/// 游戏支付配置管理服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GamePaymentConfigService
    : GenericCrudService<Entity.Tenant.Game.TenantGamePaymentConfigEntity,
        AddGamePaymentConfigRequest, UpdateGamePaymentConfigRequest,
        GamePaymentConfigResponse, PageGamePaymentConfigRequest>
{
    private readonly SqlSugarRepository<TenantGameAppPackageEntity> _packageRepository;
    private readonly IPaymentConfigSchemaProvider _schemaProvider;

    public GamePaymentConfigService(
        SqlSugarRepository<Entity.Tenant.Game.TenantGamePaymentConfigEntity> repository,
        SqlSugarRepository<TenantGameAppPackageEntity> packageRepository,
        IPaymentConfigSchemaProvider schemaProvider)
        : base(repository)
    {
        _packageRepository = packageRepository;
        _schemaProvider = schemaProvider;
    }

    protected override ISugarQueryable<GamePaymentConfigResponse> BuildPageQuery(PageGamePaymentConfigRequest input)
    {
        var searchKey = input.SearchKey?.Trim();
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                      u.PayChannel.Contains(searchKey) ||
                                      u.Remark.Contains(searchKey))
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
        var scope = BuildScope(input.PackageId);
        await ValidateScopeAsync(scope.PackageId);
        await CheckUniqueAsync(scope.PackageId, payChannel, input.IsSandbox);

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
        var scope = BuildScope(input.PackageId);
        await ValidateScopeAsync(scope.PackageId);
        await CheckUniqueAsync(scope.PackageId, payChannel, input.IsSandbox, input.Id);

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
                            e.PackageId,
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

    public static PaymentConfigScope BuildScope(long? packageId)
    {
        var normalizedPackageId = NormalizeNullableId(packageId);
        return new PaymentConfigScope(normalizedPackageId == 0 ? null : normalizedPackageId, normalizedPackageId);
    }

    public static Entity.Tenant.Game.TenantGamePaymentConfigEntity? PickBestConfig(
        IEnumerable<Entity.Tenant.Game.TenantGamePaymentConfigEntity> configs,
        long? packageId)
    {
        var scope = BuildScope(packageId);
        return configs.OrderByDescending(config => GetScopePriority(config, scope))
                      .ThenByDescending(config => config.Id)
                      .FirstOrDefault(config => GetScopePriority(config, scope) > 0);
    }

    private async Task CheckUniqueAsync(long? packageId, string payChannel, bool isSandbox, long? excludeId = null)
    {
        var packageIdNormalized = BuildScope(packageId).PackageIdNormalized;
        var exists = await Repository.AsQueryable()
                                     .Where(x => x.PackageIdNormalized == packageIdNormalized &&
                                                 x.PayChannel == payChannel && x.IsSandbox == isSandbox)
                                     .WhereIF(excludeId.HasValue, x => x.Id != excludeId.Value)
                                     .AnyAsync();
        if (exists)
        {
            throw BusinessError.Oh("同一配置范围、支付渠道和沙盒标记下的支付配置已存在");
        }
    }

    private async Task ValidateScopeAsync(long? packageId)
    {
        if (packageId.HasValue && !await _packageRepository.IsAnyAsync(u => u.Id == packageId.Value && u.IsEnabled == true))
        {
            throw BusinessError.Oh("分包不存在或已禁用");
        }
    }

    private static void ApplyScope(Entity.Tenant.Game.TenantGamePaymentConfigEntity entity, PaymentConfigScope scope)
    {
        entity.PackageId = scope.PackageId;
        entity.PackageIdNormalized = scope.PackageIdNormalized;
    }

    private static long NormalizeNullableId(long? id)
    {
        var idValue = id.GetValueOrDefault();
        return idValue > 0 ? idValue : 0;
    }

    private static int GetScopePriority(Entity.Tenant.Game.TenantGamePaymentConfigEntity config, PaymentConfigScope scope)
    {
        if (scope.PackageIdNormalized > 0 && config.PackageIdNormalized == scope.PackageIdNormalized)
        {
            return 2;
        }

        return config.PackageIdNormalized == 0 ? 1 : 0;
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
            using (var document = JsonDocument.Parse(configJson))
            {
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw BusinessError.Oh("支付配置必须是JSON对象");
                }
            }
        }
        catch (JsonException ex)
        {
            throw BusinessError.Oh($"支付配置JSON格式错误: {ex.Message}");
        }
    }
}
#pragma warning restore CS8604, CS8629