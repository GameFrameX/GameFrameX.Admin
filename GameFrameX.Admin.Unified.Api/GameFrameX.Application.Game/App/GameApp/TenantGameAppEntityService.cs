using System.Security.Cryptography;
using GameFrameX.Application.Game.App.GameApp.Dto;
using GameFrameX.Core.Security;
using GameFrameX.Core.Service;
using GameFrameX.Core.Service.TenantPackage;
using GameFrameX.Core.Service.TenantPackage.Dto;
using GameFrameX.Core.Service.User;

#pragma warning disable CS8602, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.App.GameApp;

/// <summary>
/// 游戏应用管理服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameAppEntityService
    : GenericCrudSelectService<Entity.Tenant.Game.TenantGameAppEntity,
        AddGameAppRequest, UpdateGameAppRequest,
        GameAppResponse, PageGameAppRequest>
{
    private readonly TenantQuotaService _tenantQuotaService;
    private readonly CurrentUserContext _currentUserContext;

    public TenantGameAppEntityService(
        SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> repository,
        TenantQuotaService tenantQuotaService,
        CurrentUserContext currentUserContext)
        : base(repository)
    {
        _tenantQuotaService = tenantQuotaService;
        _currentUserContext = currentUserContext;
    }

    protected override ISugarQueryable<GameAppResponse> BuildPageQuery(PageGameAppRequest input)
    {
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .WhereIF(input.IsActive.HasValue, u => u.IsEnabled == input.IsActive)
                         .Select<GameAppResponse>();
    }

    /// <summary>
    /// 重写基类 AddAsync：校验租户应用配额后创建应用。
    /// <para>密钥采用 hash-first：明文永不落库，仅在返回实体上一次性回传（<see cref="CreateEntityAsync"/>）。</para>
    /// </summary>
    public override async Task<GameAppResponse> AddAsync(AddGameAppRequest input)
    {
        await CheckGameAppQuotaAsync();
        return (await CreateEntityAsync(input)).Adapt<GameAppResponse>();
    }

    /// <summary>
    /// 分页查询（AppSecret 脱敏）
    /// </summary>
    public override async Task<SqlSugarPagedList<GameAppResponse>> PageAsync(PageGameAppRequest input)
    {
        var result = await base.PageAsync(input);
        foreach (var item in result.Items)
        {
            item.AppSecret = SensitiveFieldMask.MaskIfNotEmpty(item.AppSecret);
        }

        return result;
    }

    /// <summary>
    /// 详情（AppSecret 脱敏）
    /// </summary>
    public override async Task<GameAppResponse> GetAsync([FromQuery] BaseIdRequest input)
    {
        var entity = await InnerGetAsync(input);
        if (entity == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        entity.AppSecret = SensitiveFieldMask.MaskIfNotEmpty(entity.AppSecret);
        return entity.Adapt<GameAppResponse>();
    }

    /// <summary>
    /// 获取当前租户启用应用列表
    /// </summary>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<GameAppResponse>> ListAsync()
    {
        var list = await Repository.AsQueryable()
                                   .Where(u => u.IsEnabled != false)
                                   .OrderBy(u => u.CreatedTime)
                                   .Select<GameAppResponse>()
                                   .ToListAsync();
        foreach (var item in list)
        {
            item.AppSecret = SensitiveFieldMask.MaskIfNotEmpty(item.AppSecret);
        }

        return list;
    }

    /// <summary>
    /// 重置应用密钥：生成新明文 → 写 PBKDF2-SHA256 hash + 算法标识 + 轮换时间，清空遗留明文。
    /// <para>明文仅在返回 Output 上一次性回传（供前端展示一次），不落库；Hash 字段不在 Output，天然不出 wire。</para>
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "ResetSecret")]
    public async Task<GameAppResponse> ResetSecretAsync(BaseIdRequest input)
    {
        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh("应用不存在");
        }

        var plaintext = GenerateAppSecret();
        entity.AppSecretHash = AppSecretHasher.Hash(plaintext);
        entity.AppSecretHashAlgorithm = AppSecretHasher.Algorithm;
        // 显式推进 UpdateTime：Client.Api AppCacheService 以 MAX(UpdateTime) 为缓存失效信号，
        // 不推进则轮换后的新 hash 对已缓存进程不生效（旧 secret 复活）。
        // 以 SQL 级 MAX 为基准 +1：实体属性读回的 UpdateTime 在部分映射/AOP 路径下为 null（实测），
        // 同一秒内连续轮换也必须严格递增，否则迟到的同秒轮换对客户端不可见（C395 关联性实测）
        var nowSeconds = TimerHelper.UnixTimeSeconds();
        entity.LastRotatedTime = nowSeconds;
        var maxUpdateTime = await Repository.AsQueryable().MaxAsync(u => u.UpdateTime);
        entity.UpdateTime = Math.Max(nowSeconds, (maxUpdateTime ?? 0) + 1);
#pragma warning disable CS8625 // 实体属性置 null 后通过 UpdateColumns 持久化（DB 列允许 null），用于清除遗留明文
        entity.AppSecret = null;
#pragma warning restore CS8625

        await Repository.AsUpdateable(entity)
                        .UpdateColumns(e => new { e.AppSecretHash, e.AppSecretHashAlgorithm, e.LastRotatedTime, e.UpdateTime, e.AppSecret, })
                        .ExecuteCommandAsync();

        // 一次性明文，仅供前端展示一次，不落库
        entity.AppSecret = plaintext;
        return entity.Adapt<GameAppResponse>();
    }

    private async Task<Entity.Tenant.Game.TenantGameAppEntity> CreateEntityAsync(AddGameAppRequest input)
    {
        var plaintext = GenerateAppSecret();
        var entity = input.Adapt<Entity.Tenant.Game.TenantGameAppEntity>();
        // 永不持久化明文：只写 hash + 算法 + 轮换时间
#pragma warning disable CS8625 // 实体属性在内存中置 null 用于脱敏，不持久化
        entity.AppSecret = null;
#pragma warning restore CS8625
        entity.AppSecretHash = AppSecretHasher.Hash(plaintext);
        entity.AppSecretHashAlgorithm = AppSecretHasher.Algorithm;
        entity.LastRotatedTime = TimerHelper.UnixTimeSeconds();
        entity.IsEnabled = true;
        await Repository.InsertAsync(entity);

        // 一次性明文，仅供前端展示一次
        entity.AppSecret = plaintext;
#pragma warning disable CS8625 // 实体属性在内存中置 null 用于脱敏，不持久化
        entity.AppSecretHash = null;
#pragma warning restore CS8625
        return entity;
    }

    private async Task CheckGameAppQuotaAsync()
    {
        if (_currentUserContext.SuperAdmin)
        {
            return;
        }

        var quota = await _tenantQuotaService.GetEffectiveQuotaAsync(_currentUserContext.TenantId);
        var currentCount = await Repository.AsQueryable().CountAsync();
        var error = GetCreateAppQuotaError(currentCount, quota);
        if (!string.IsNullOrWhiteSpace(error))
        {
            throw BusinessError.Oh(error);
        }
    }

    /// <summary>
    /// 校验租户应用数量是否超出配额；超限返回错误消息，否则返回 null。
    /// <para>超级管理员跳过配额限制（<see cref="CheckGameAppQuotaAsync"/>）。</para>
    /// </summary>
    /// <param name="currentCount">当前租户已有应用数</param>
    /// <param name="quota">租户生效配额</param>
    /// <returns>超限错误消息；未超限返回 null</returns>
    public static string? GetCreateAppQuotaError(int currentCount, EffectiveQuotaResponse quota)
    {
        if (currentCount >= quota.MaxGameAppCount)
        {
            return $"应用数已达上限（{quota.MaxGameAppCount}），无法新增";
        }

        return null;
    }

    private static string GenerateAppSecret()
    {
        var bytes = new byte[16];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
#pragma warning restore CS8602, CS8629