using System.ComponentModel;
using GameFrameX.Entity.Tenant.Game.App;
using System.Security.Cryptography;
using GameFrameX.Application.Platform.OpenAccess.GameAppOpenAccess.Dto;
using GameFrameX.Core.Service.Tenant;

namespace GameFrameX.Application.Platform.OpenAccess.GameAppOpenAccess;

/// <summary>
/// 应用开放访问凭证管理服务。
/// <para>机器调用凭证（打包上报、CI、流水线）的管理面，落主库，绑定真实用户。</para>
/// <para>对称平台级 <see cref="Entity.System.SystemOpenAccessEntity"/>：System = 平台开放接口身份，GameApp = 应用机器凭证。</para>
/// <para>签发和更新均校验绑定用户存在，AccessSecret 仅签发 / 轮换时一次性返回。</para>
/// </summary>
[ApiDescriptionSettings(Order = 185)]
public class TenantGameAppOpenAccessService : IAdminApiController
{
    private readonly SqlSugarRepository<TenantGameAppOpenAccessEntity> _repository;
    private readonly SqlSugarRepository<TenantUserEntity> _userRepository;

    /// <summary>
    /// 应用开放访问凭证管理服务构造函数。
    /// </summary>
    public TenantGameAppOpenAccessService(
        SqlSugarRepository<TenantGameAppOpenAccessEntity> repository,
        SqlSugarRepository<TenantUserEntity> userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// 获取应用开放访问凭证分页列表（AccessSecret 永不输出）。
    /// </summary>
    /// <param name="input">分页查询输入。</param>
    /// <returns>凭证分页（含绑定用户账号 / 姓名回显）。</returns>
    [DisplayName("获取应用开放访问凭证分页列表")]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<TenantGameAppOpenAccessResponse>> PageAsync(PageTenantGameAppOpenAccessRequest input)
    {
        var accessKey = input.AccessKey?.Trim() ?? string.Empty;
        var scenario = input.Scenario?.Trim() ?? string.Empty;
        var subjectName = input.SubjectName?.Trim() ?? string.Empty;

        return await _repository.AsQueryable().ClearFilter().Where(u => u.IsDeleted == false)
                         .LeftJoin<TenantUserEntity>((u, t) => u.BindUserId == t.Id)
                         .WhereIF(!string.IsNullOrWhiteSpace(accessKey), (u, t) => u.AccessKey.Contains(accessKey))
                         .WhereIF(!string.IsNullOrWhiteSpace(scenario), (u, t) => u.Scenario == scenario)
                         .WhereIF(!string.IsNullOrWhiteSpace(subjectName), (u, t) => u.SubjectName.Contains(subjectName))
                         .WhereIF(input.IsEnabled.HasValue, (u, t) => u.IsEnabled == input.IsEnabled)
                         .Select((u, t) => new TenantGameAppOpenAccessResponse
                         {
                             Id = u.Id,
                             BindUserId = u.BindUserId,
                             Scenario = u.Scenario,
                             Purpose = u.Purpose,
                             SubjectName = u.SubjectName,
                             ScopesJson = u.ScopesJson,
                             AccessKey = u.AccessKey,
                             // AccessSecret 故意不映射，保持默认 null（永不输出）
                             ExpiresTime = u.ExpiresTime,
                             RevokedTime = u.RevokedTime,
                             RevokeReason = u.RevokeReason,
                             IsEnabled = u.IsEnabled,
                             CreatedTime = u.CreatedTime,
                             CreatedName = u.CreatedName,
                             UpdateTime = u.UpdateTime ?? 0,
                             UpdatedName = u.UpdatedName,
                             RowVersion = u.RowVersion ?? 0,
                             BindUserAccount = t.Account ?? string.Empty,
                             BindUserName = t.RealName ?? string.Empty,
                         })
                         .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 签发应用开放访问凭证。AccessKey / AccessSecret 由服务端生成，AccessSecret 仅本次响应一次性返回。
    /// </summary>
    /// <param name="input">签发输入。</param>
    /// <returns>含明文 AccessSecret 的签发结果（仅此一次）。</returns>
    [ApiDescriptionSettings(Name = "Add")]
    [HttpPost]
    [DisplayName("签发应用开放访问凭证")]
    public async Task<TenantGameAppOpenAccessSecretResponse> AddAsync(AddTenantGameAppOpenAccessRequest input)
    {
        await EnsureBindUserExistsAsync(input.BindUserId);
        var scopes = EnsureValidScopes(input.ScopesJson);

        string accessKey;
        do
        {
            accessKey = GenerateAccessKey();
        } while (await _repository.AsQueryable().ClearFilter().Where(u => u.IsDeleted == false).AnyAsync(u => u.AccessKey == accessKey));

        var accessSecret = GenerateAccessSecret();

        var entity = new TenantGameAppOpenAccessEntity
        {
            BindUserId = input.BindUserId,
            Scenario = input.Scenario.Trim(),
            Purpose = input.Purpose.Trim(),
            SubjectName = input.SubjectName.Trim(),
            ScopesJson = NormalizeScopesJson(scopes),
            AccessKey = accessKey,
            AccessSecret = TenantSecretCrypto.Encrypt(accessSecret) ?? string.Empty,
            ExpiresTime = input.ExpiresTime,
        };
        await _repository.InsertAsync(entity);

        return new TenantGameAppOpenAccessSecretResponse
        {
            Id = entity.Id,
            AccessKey = accessKey,
            AccessSecret = accessSecret,
            ScopesJson = entity.ScopesJson,
        };
    }

    /// <summary>
    /// 更新应用开放访问凭证。校验绑定用户存在与 Scopes 有效。
    /// </summary>
    /// <param name="input">更新输入（含客户端乐观锁 RowVersion）。</param>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新应用开放访问凭证")]
    public async Task UpdateAsync(UpdateTenantGameAppOpenAccessRequest input)
    {
        var existing = await GetCredentialAsync(input.Id);
        await EnsureBindUserExistsAsync(input.BindUserId);
        var scopes = EnsureValidScopes(input.ScopesJson);

        existing.BindUserId = input.BindUserId;
        existing.Scenario = input.Scenario.Trim();
        existing.Purpose = input.Purpose.Trim();
        existing.SubjectName = input.SubjectName.Trim();
        existing.ScopesJson = NormalizeScopesJson(scopes);
        existing.ExpiresTime = input.ExpiresTime;
        existing.RowVersion = input.RowVersion;

        await _repository.Context.UpdateWithOptimisticLockAsync(existing);
    }

    /// <summary>
    /// 删除应用开放访问凭证。
    /// </summary>
    /// <param name="input">操作输入（Id）。</param>
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除应用开放访问凭证")]
    public async Task DeleteAsync(TenantGameAppOpenAccessActionRequest input)
    {
        var existing = await GetCredentialAsync(input.Id);
        await _repository.DeleteAsync(existing);
    }

    /// <summary>
    /// 吊销应用开放访问凭证（写 RevokedTime / RevokeReason，并禁用）。
    /// </summary>
    /// <param name="input">操作输入（Id + 原因）。</param>
    [ApiDescriptionSettings(Name = "Revoke")]
    [HttpPost]
    [DisplayName("吊销应用开放访问凭证")]
    public async Task RevokeAsync(TenantGameAppOpenAccessActionRequest input)
    {
        var existing = await GetCredentialAsync(input.Id);

        existing.RevokedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
#pragma warning disable CS8601 // input.Reason 为 string?，实体 RevokeReason 字段在 DB schema 允许 null（= null! 仅满足 CS8618）
        existing.RevokeReason = input.Reason?.Trim();
#pragma warning restore CS8601
        existing.IsEnabled = false;
        existing.RowVersion = input.RowVersion;

        await _repository.Context.UpdateWithOptimisticLockAsync(existing);
    }

    /// <summary>
    /// 启用应用开放访问凭证（已撤销的凭证不可启用）。
    /// </summary>
    /// <param name="input">操作输入（Id）。</param>
    [ApiDescriptionSettings(Name = "Enable")]
    [HttpPost]
    [DisplayName("启用应用开放访问凭证")]
    public async Task EnableAsync(TenantGameAppOpenAccessActionRequest input)
    {
        var existing = await GetCredentialAsync(input.Id);

        if (existing.RevokedTime != null)
        {
            throw BusinessError.Oh(OpenAccessErrorCode.CredentialRevokedEnableForbidden);
        }

        existing.IsEnabled = true;
        existing.RowVersion = input.RowVersion;

        await _repository.Context.UpdateWithOptimisticLockAsync(existing);
    }

    /// <summary>
    /// 禁用应用开放访问凭证。
    /// </summary>
    /// <param name="input">操作输入（Id）。</param>
    [ApiDescriptionSettings(Name = "Disable")]
    [HttpPost]
    [DisplayName("禁用应用开放访问凭证")]
    public async Task DisableAsync(TenantGameAppOpenAccessActionRequest input)
    {
        var existing = await GetCredentialAsync(input.Id);

        existing.IsEnabled = false;
        existing.RowVersion = input.RowVersion;

        await _repository.Context.UpdateWithOptimisticLockAsync(existing);
    }

    /// <summary>
    /// 续期 / 轮换密钥。重置 AccessSecret 并一次性返回（前端须立即保存）。
    /// </summary>
    /// <param name="input">操作输入（Id）。</param>
    /// <returns>含新明文 AccessSecret 的轮换结果（仅此一次）。</returns>
    [ApiDescriptionSettings(Name = "Refresh")]
    [HttpPost]
    [DisplayName("轮换应用开放访问凭证密钥")]
    public async Task<TenantGameAppOpenAccessSecretResponse> RefreshAsync(TenantGameAppOpenAccessActionRequest input)
    {
        var existing = await GetCredentialAsync(input.Id);

        var plainSecret = GenerateAccessSecret();
        existing.AccessSecret = TenantSecretCrypto.Encrypt(plainSecret) ?? string.Empty;
        existing.RowVersion = input.RowVersion;

        await _repository.Context.UpdateWithOptimisticLockAsync(existing);

        return new TenantGameAppOpenAccessSecretResponse
        {
            Id = existing.Id,
            AccessKey = existing.AccessKey,
            AccessSecret = plainSecret,
            ScopesJson = existing.ScopesJson,
        };
    }

    /// <summary>
    /// 生成访问 Key（Guid Base64 去尾填充，主库唯一由签发时循环校验保证）。
    /// </summary>
    /// <returns>访问 Key。</returns>
    public static string GenerateAccessKey()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..^2];
    }

    /// <summary>
    /// 生成访问密钥（32 字节随机 + Base64，签发 / 轮换时一次性返回）。
    /// </summary>
    /// <returns>访问密钥明文。</returns>
    public static string GenerateAccessSecret()
    {
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// 校验并解析接口范围（ScopesJson）。空、非 JSON 数组、空数组或含空项均抛 OA1004。
    /// </summary>
    /// <param name="scopesJson">接口范围 JSON 数组字符串，例如 ["package-report"]。</param>
    /// <returns>解析后的 scope 列表。</returns>
    public static List<string> EnsureValidScopes(string scopesJson)
    {
        if (string.IsNullOrWhiteSpace(scopesJson))
        {
            throw BusinessError.Oh(OpenAccessErrorCode.ScopesEmptyOrInvalid);
        }

        List<string> scopes;
        try
        {
            scopes = JsonSerializer.Deserialize<List<string>>(scopesJson) ?? new List<string>();
        }
        catch (JsonException)
        {
            throw BusinessError.Oh(OpenAccessErrorCode.ScopesEmptyOrInvalid);
        }

        if (scopes.Count == 0 || scopes.Any(string.IsNullOrWhiteSpace))
        {
            throw BusinessError.Oh(OpenAccessErrorCode.ScopesEmptyOrInvalid);
        }

        return scopes;
    }

    /// <summary>
    /// 规范化接口范围为 JSON 数组字符串（保证持久化格式一致）。
    /// </summary>
    /// <param name="scopes">scope 列表。</param>
    /// <returns>JSON 数组字符串。</returns>
    public static string NormalizeScopesJson(List<string> scopes)
    {
        return JsonSerializer.Serialize(scopes);
    }

    /// <summary>
    /// 校验绑定用户存在（不存在抛 OA1001）。
    /// </summary>
    /// <param name="bindUserId">绑定用户 Id。</param>
    private async Task EnsureBindUserExistsAsync(long bindUserId)
    {
        var exists = await _userRepository.AsQueryable().AnyAsync(u => u.Id == bindUserId);
        if (!exists)
        {
            throw BusinessError.Oh(OpenAccessErrorCode.BoundUserNotFoundOrNotOwned);
        }
    }

    /// <summary>
    /// 按主键获取凭证（不存在抛 OA1002）。
    /// </summary>
    /// <param name="id">凭证 Id。</param>
    /// <returns>凭证实体。</returns>
    private async Task<TenantGameAppOpenAccessEntity> GetCredentialAsync(long id)
    {
        var entity = await _repository.AsQueryable().ClearFilter().Where(u => u.IsDeleted == false).FirstAsync(u => u.Id == id);
        if (entity == null)
        {
            throw BusinessError.Oh(OpenAccessErrorCode.CredentialNotFoundOrNotOwned);
        }

        return entity;
    }
}