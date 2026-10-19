using System.ComponentModel;
using System.Security.Cryptography;
using GameFrameX.Foundation.Logger;
using GameFrameX.Application.Game.Tenant.ClientApiPolicy.TenantClientApiPolicy.Dto;
using GameFrameX.Core.Enum.Errors;
using GameFrameX.Core.Service;
using GameFrameX.Core.Service.Tenant;

#pragma warning disable CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Tenant.ClientApiPolicy.TenantClientApiPolicy;

/// <summary>
/// 租户 Client API 运行策略管理服务（C378 四表合一：Token / 登录票据 / 区服输出 / 订单策略单页管理）。
/// <para>作用域 (TenantId, AppId) 唯一，AppId=0 为租户默认行；令牌节仅 AppId=0 行被运行时消费。</para>
/// <para>敏感字段（SigningKey）承载 C368 空值防护语义：新增空白自动生成 64-hex、更新空白与 <c>******</c> 同义保留原值；
/// 输出恒为脱敏占位；该密钥纯服务端内部使用，任何响应均不回显明文。</para>
/// <para>签名密钥以 <see cref="TenantSecretCrypto"/> 密文落库（C371）：PreserveIfMasked 先换回库中原值（密文形态），
/// Encrypt 幂等跳过已密文值，新明文在落库前加密。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 112)]
public class TenantClientApiPolicyEntityService
    : GenericCrudService<GameFrameX.Entity.Tenant.ClientApiConfig.TenantClientApiPolicyEntity,
        AddTenantClientApiPolicyRequest, UpdateTenantClientApiPolicyRequest,
        TenantClientApiPolicyResponse, PageTenantClientApiPolicyRequest>
{
    /// <summary>
    /// 票据固定到期时间上限（秒）：30 天（C142 决策口径）。
    /// </summary>
    private const long TicketExpireSecondsMax = 30L * 24 * 60 * 60;

    private readonly GameFrameX.Core.Service.User.CurrentUserContext _currentUserContext;

    public TenantClientApiPolicyEntityService(SqlSugarRepository<GameFrameX.Entity.Tenant.ClientApiConfig.TenantClientApiPolicyEntity> repository,
                                              GameFrameX.Core.Service.User.CurrentUserContext currentUserContext)
        : base(repository)
    {
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// 轮换策略签名密钥（C373）：生成新 64-hex 加密落库，旧值即刻作废——该密钥纯服务端内部使用，
    /// 无双轨需求（区别于 HMAC 密钥环/票据 RSA 的租约退场模型）。
    /// <para>响应不回显密钥（C368 背景已论证：任何响应均不回显明文/密文）。</para>
    /// </summary>
    [HttpPost("rotate")]
    [ApiDescriptionSettings(Name = "Rotate")]
    [DisplayName("轮换 Client API 策略签名密钥")]
    public async Task RotateAsync(BaseIdRequest input)
    {
        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        var plain = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        entity.SigningKey = TenantSecretCrypto.Encrypt(plain) ?? string.Empty;
        entity.UpdateTime = TimerHelper.UnixTimeSeconds();
        await Repository.AsUpdateable(entity)
                        .UpdateColumns(e => new { e.SigningKey, e.UpdateTime, })
                        .ExecuteCommandAsync();

        // 审计（C373）：密钥生命周期操作结构化审计行；DB 操作日志管线断链期先落应用日志，管线修复后统一接入
        LogHelper.Info($"[KeyAudit] action=PolicySigningKeyRotate tenant={_currentUserContext.TenantId} app={entity.AppId} policyId={entity.Id} oldKeyRevoked=immediately operator={_currentUserContext.Account}");
    }

    protected override ISugarQueryable<TenantClientApiPolicyResponse> BuildPageQuery(PageTenantClientApiPolicyRequest input)
    {
        var searchKey = input.SearchKey?.Trim();
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u => u.Remark.Contains(searchKey))
                         .WhereIF(input.AppId.HasValue, u => u.AppId == input.AppId.Value)
                         .WhereIF(input.IsActive.HasValue, u => u.IsEnabled == input.IsActive.Value)
                         .Select<TenantClientApiPolicyResponse>();
    }

    /// <summary>
    /// 新增运行策略（(TenantId, AppId) 作用域唯一）。SigningKey 空白时服务端生成 64-hex（C368）。
    /// </summary>
    [DisplayName("新增 Client API 运行策略")]
    public override async Task<TenantClientApiPolicyResponse> AddAsync(AddTenantClientApiPolicyRequest input)
    {
        await CheckUniqueScopeAsync(input.AppId, null);
        ValidateTicketExpireTime(input.TicketExpireTime);

        var entity = input.Adapt<GameFrameX.Entity.Tenant.ClientApiConfig.TenantClientApiPolicyEntity>();
        entity.SigningKey = ResolveSigningKeyForStorage(input.SigningKey, null);
        entity.IsEnabled = true;
        await Repository.InsertAsync(entity);
        var response = entity.Adapt<TenantClientApiPolicyResponse>();
        // 与 Page/Detail 同口径：SigningKey 不出 wire（库内密文形态也不回显，掩码替代）
        response.SigningKey = SensitiveFieldMask.MaskValue;
        return response;
    }

    /// <summary>
    /// 更新运行策略。SigningKey 回传 <c>******</c> 或留空均保留原值（C368：空白与掩码同义），新明文加密落库。
    /// </summary>
    [DisplayName("更新 Client API 运行策略")]
    public override async Task UpdateAsync(UpdateTenantClientApiPolicyRequest input)
    {
        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id);
        if (entity == null)
        {
            {
                throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
            }
        }

        await CheckUniqueScopeAsync(input.AppId, input.Id);
        ValidateTicketExpireTime(input.TicketExpireTime);

        entity.AppId = input.AppId;
        entity.SigningKey = ResolveSigningKeyForStorage(input.SigningKey, entity.SigningKey);
        entity.LifetimeMinutes = input.LifetimeMinutes;
        entity.TicketExpireTime = input.TicketExpireTime;
        entity.EnableKeyAutoCreate = input.EnableKeyAutoCreate;
        entity.EnableReturnHttpManager = input.EnableReturnHttpManager;
        entity.EnableSandBoxRecharge = input.EnableSandBoxRecharge;
        entity.Remark = input.Remark;
        entity.RowVersion = input.RowVersion;

        await Repository.AsUpdateable(entity)
                        .UpdateColumns(e => new
                        {
                            e.AppId,
                            e.SigningKey,
                            e.LifetimeMinutes,
                            e.TicketExpireTime,
                            e.EnableKeyAutoCreate,
                            e.EnableReturnHttpManager,
                            e.EnableSandBoxRecharge,
                            e.Remark,
                        })
                        .ExecuteCommandWithOptLockAsync(true);
    }

    /// <summary>
    /// 分页查询（敏感字段脱敏）。
    /// </summary>
    [HttpPost(CrudActionRoutes.Page)]
    [ApiDescriptionSettings(Name = "Page")]
    [DisplayName("分页查询 Client API 运行策略")]
    public override async Task<SqlSugarPagedList<TenantClientApiPolicyResponse>> PageAsync(PageTenantClientApiPolicyRequest input)
    {
        var result = await base.PageAsync(input);
        foreach (var item in result.Items)
        {
            item.SigningKey = SensitiveFieldMask.MaskValue;
        }

        return result;
    }

    /// <summary>
    /// 详情（敏感字段脱敏）。
    /// </summary>
    [HttpGet(CrudActionRoutes.Detail)]
    [ApiDescriptionSettings(Name = "Detail")]
    [DisplayName("Client API 运行策略详情")]
    public override async Task<TenantClientApiPolicyResponse> GetAsync([FromQuery] BaseIdRequest input)
    {
        var entity = await InnerGetAsync(input);
        if (entity == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        entity.SigningKey = SensitiveFieldMask.MaskValue;
        return entity.Adapt<TenantClientApiPolicyResponse>();
    }

    /// <summary>
    /// 解析签名密钥最终落库值（C368 + C371 组合语义）。
    /// <para>新增：空白 → CSPRNG 生成 64-hex；非空明文透传。</para>
    /// <para>更新：<c>******</c> 或空白 → 保留库中原值（密文形态）；新明文透传。</para>
    /// <para>最终值经 <see cref="TenantSecretCrypto.Encrypt"/>：明文加密为 <c>enc:sec:v1:</c> 密文，已密文值幂等透传。</para>
    /// </summary>
    private static string ResolveSigningKeyForStorage(string? inputKey, string? storedKey)
    {
        string plain;
        if (SensitiveFieldMask.IsMasked(inputKey) || string.IsNullOrWhiteSpace(inputKey))
        {
            if (string.IsNullOrEmpty(storedKey))
            {
                // 新增（无库中原值）或库中为空：空白提交 → 服务端生成 64-hex（CSPRNG 32 字节，与传输签名/会话密钥同源风格）
                plain = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
            }
            else
            {
                // 保留库中原值：明文形态（存量）或密文形态（C371 加密后）均由 Encrypt 幂等处理
                plain = storedKey;
            }
        }
        else
        {
            plain = inputKey.Trim();
        }

        return TenantSecretCrypto.Encrypt(plain) ?? string.Empty;
    }

    /// <summary>
    /// 校验同一 (TenantId, AppId) 作用域唯一性。
    /// </summary>
    private async Task CheckUniqueScopeAsync(long appId, long? excludeId)
    {
        var exists = excludeId.HasValue
                         ? await Repository.IsAnyAsync(x => x.AppId == appId && x.Id != excludeId.Value)
                         : await Repository.IsAnyAsync(x => x.AppId == appId);
        if (exists)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordAlreadyExists, "当前作用域（应用或租户默认）的 Client API 运行策略已存在");
        }
    }

    /// <summary>
    /// 校验票据固定到期时间：0 放行（永不过期）；非 0 必须晚于当前时间且不超过 30 天（C142）。
    /// </summary>
    private static void ValidateTicketExpireTime(long ticketExpireTime)
    {
        if (ticketExpireTime == 0)
        {
            return;
        }

        var now = TimerHelper.UnixTimeSeconds();
        if (ticketExpireTime <= now)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, "票据到期时间必须晚于当前时间");
        }

        if (ticketExpireTime > now + TicketExpireSecondsMax)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, "票据到期时间不能超过 30 天");
        }
    }
}
#pragma warning restore CS8604, CS8629
