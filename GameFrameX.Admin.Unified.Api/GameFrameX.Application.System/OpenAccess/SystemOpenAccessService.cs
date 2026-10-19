using GameFrameX.Application.System.OpenAccess.Dto;
using GameFrameX.Core.Const;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Foundation.Http.Normalization;

namespace GameFrameX.Application.System.OpenAccess;

/// <summary>
/// 开放接口身份服务
/// </summary>
[ApiDescriptionSettings(Order = 244)]
public class SystemOpenAccessService : IAdminApiController
{
    private readonly SqlSugarRepository<SystemOpenAccessEntity> _systemOpenAccessRepository;
    private readonly SystemCacheService _systemCacheService;
    private readonly TenantDbConnectionProvider _tenantDbConnectionProvider;

    /// <summary>
    /// 开放接口身份服务构造函数
    /// </summary>
    public SystemOpenAccessService(SqlSugarRepository<SystemOpenAccessEntity> systemOpenAccessRepository,
        SystemCacheService systemCacheService,
        TenantDbConnectionProvider tenantDbConnectionProvider)
    {
        _systemOpenAccessRepository = systemOpenAccessRepository;
        _systemCacheService = systemCacheService;
        _tenantDbConnectionProvider = tenantDbConnectionProvider;
    }

    /// <summary>
    /// 获取开放接口身份分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取开放接口身份分页列表")]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<OpenAccessResponse>> PageAsync(PageOpenAccessRequest input)
    {
        var accessKey = input.AccessKey?.Trim() ?? string.Empty;
        return await _systemOpenAccessRepository.AsQueryable()
                                         .LeftJoin<SystemTenantEntity>((u, a) => u.BindTenantId == a.Id)
                                         .WhereIF(!string.IsNullOrWhiteSpace(accessKey), (u, a) => u.AccessKey.Contains(accessKey))
                                         .Select((u, a) => new OpenAccessResponse
                                         {
                                             BindUserAccount = u.BindUserId.ToString(),
                                             BindTenantName = a.Remark ?? string.Empty,
                                         }, true)
                                         .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加开放接口身份
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add")]
    [HttpPost]
    [DisplayName("增加开放接口身份")]
    public async Task AddOpenAccessAsync(AddOpenAccessRequest input)
    {
        if (await _systemOpenAccessRepository.AsQueryable().AnyAsync(u => u.AccessKey == input.AccessKey && u.Id != input.Id))
        {
            throw BusinessError.Oh(OpenAccessErrorCode.IdentityAlreadyExists);
        }

        var openAccess = input.Adapt<SystemOpenAccessEntity>();
        // 凭证静态加密收口（C370）：AccessSecret 以 enc:sec:v1: 密文落库
        openAccess.AccessSecret = TenantSecretCrypto.Encrypt(openAccess.AccessSecret) ?? string.Empty;
        await _systemOpenAccessRepository.InsertAsync(openAccess);
    }

    /// <summary>
    /// 更新开放接口身份
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新开放接口身份")]
    public async Task UpdateOpenAccessAsync(UpdateOpenAccessRequest input)
    {
        if (await _systemOpenAccessRepository.AsQueryable().AnyAsync(u => u.AccessKey == input.AccessKey && u.Id != input.Id))
        {
            throw BusinessError.Oh(OpenAccessErrorCode.IdentityAlreadyExists);
        }

        var openAccess = input.Adapt<SystemOpenAccessEntity>();
        // 凭证静态加密收口（C370）：幂等——存量密文行重写不换密文
        openAccess.AccessSecret = TenantSecretCrypto.Encrypt(openAccess.AccessSecret) ?? string.Empty;
        _systemCacheService.Remove(CacheConst.KeyOpenAccess + openAccess.AccessKey);

        await _systemOpenAccessRepository.Context.UpdateWithOptimisticLockAsync(openAccess);
    }

    /// <summary>
    /// 删除开放接口身份
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除开放接口身份")]
    public async Task DeleteOpenAccessAsync(DeleteOpenAccessRequest input)
    {
        var openAccess = await _systemOpenAccessRepository.GetFirstAsync(u => u.Id == input.Id);
        if (openAccess != null)
        {
            _systemCacheService.Remove(CacheConst.KeyOpenAccess + openAccess.AccessKey);
        }

        await _systemOpenAccessRepository.DeleteAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 创建密钥
    /// </summary>
    /// <returns></returns>
    [DisplayName("创建密钥")]
    [ApiDescriptionSettings(Name = "CreateSecret")]
    public Task<HttpJsonResultData<SecretCreateResponse>> CreateSecretAsync()
    {
        return Task.FromResult(HttpJsonResultData<SecretCreateResponse>.Success(new SecretCreateResponse { Secret = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..^2], }));
    }

    /// <summary>
    /// 根据 Key 获取对象
    /// <para>凭证解密收口（C370）：解密发生在 <see cref="SystemCacheService.GetOrAdd"/> 缓存 miss 的加载边界——
    /// 缓存持有明文实体（进程级，配置语义），命中路径零 PBKDF2 派生；写入侧更新时 Remove 使缓存失效。</para>
    /// </summary>
    /// <param name="accessKey"></param>
    /// <returns></returns>
    [NonAction]
    public Task<SystemOpenAccessEntity> GetByKeyAsync([FromQuery] string accessKey)
    {
        return Task.FromResult(
            _systemCacheService.GetOrAdd(CacheConst.KeyOpenAccess + accessKey, _ =>
            {
                var entity = _systemOpenAccessRepository.AsQueryable().First(u => u.AccessKey == accessKey);
                if (entity != null)
                {
                    entity.AccessSecret = TenantSecretCrypto.Decrypt(entity.AccessSecret) ?? string.Empty;
                }

                return entity;
            })
        );
    }

    /// <summary>
    /// 从绑定租户库读取开放接口身份绑定的租户用户和组织。
    /// </summary>
    /// <param name="openAccess"></param>
    /// <returns></returns>
    [NonAction]
    public BoundTenantUser GetBoundTenantUser(SystemOpenAccessEntity openAccess)
    {
        var tenantDb = _tenantDbConnectionProvider.GetTenantDbConnectionScope(openAccess.BindTenantId);
        if (tenantDb == null)
        {
            throw BusinessError.Oh(TenantErrorCode.DefaultTenantStatusChangeForbidden);
        }

        tenantDb.QueryFilter.ClearAndBackup();
        try
        {
            var user = tenantDb.Queryable<TenantUserEntity>().First(u => u.Id == openAccess.BindUserId);
            if (user == null)
            {
                throw BusinessError.Oh(SystemUserErrorCode.AccountNotFound);
            }

            var org = tenantDb.Queryable<TenantOrganizationEntity>().First(u => u.Id == user.OrganizationId);
            return new BoundTenantUser(user, org);
        }
        finally
        {
            tenantDb.QueryFilter.Restore();
        }
    }
}