using GameFrameX.Application.Platform.OpenAccess.Dto;
using GameFrameX.Core.Const;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Business;
using GameFrameX.Foundation.Http.Normalization;

namespace GameFrameX.Application.Platform.OpenAccess;

/// <summary>
/// 开放接口身份服务
/// </summary>
[ApiDescriptionSettings(Order = 244)]
public class SystemOpenAccessService : IAdminApiController
{
    private readonly SqlSugarRepository<SystemOpenAccessEntity> _systemOpenAccessRepository;
    private readonly SystemCacheService _systemCacheService;
    private readonly ISqlSugarClient _dbConnectionProvider;

    /// <summary>
    /// 开放接口身份服务构造函数
    /// </summary>
    public SystemOpenAccessService(SqlSugarRepository<SystemOpenAccessEntity> systemOpenAccessRepository,
        SystemCacheService systemCacheService,
        ISqlSugarClient dbConnectionProvider)
    {
        _systemOpenAccessRepository = systemOpenAccessRepository;
        _systemCacheService = systemCacheService;
        _dbConnectionProvider = dbConnectionProvider;
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
                                         .WhereIF(!string.IsNullOrWhiteSpace(accessKey), u => u.AccessKey.Contains(accessKey))
                                         .Select(u => new OpenAccessResponse
                                         {
                                             BindUserAccount = u.BindUserId.ToString(),
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
        // 凭证静态加密收口：AccessSecret 以 enc:sec:v1: 密文落库
        openAccess.AccessSecret = SecretCrypto.Encrypt(openAccess.AccessSecret) ?? string.Empty;
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
        // 凭证静态加密收口：幂等——存量密文行重写不换密文
        openAccess.AccessSecret = SecretCrypto.Encrypt(openAccess.AccessSecret) ?? string.Empty;
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
    /// <para>凭证解密收口：解密发生在 <see cref="SystemCacheService.GetOrAdd"/> 缓存 miss 的加载边界——
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
                    entity.AccessSecret = SecretCrypto.Decrypt(entity.AccessSecret) ?? string.Empty;
                }

                return entity;
            })
        );
    }

    /// <summary>
    /// 从主库读取开放接口身份绑定的用户。
    /// </summary>
    /// <param name="openAccess"></param>
    /// <returns></returns>
    [NonAction]
    public BoundUser GetBoundUser(SystemOpenAccessEntity openAccess)
    {
        var mainDb = _dbConnectionProvider;
        mainDb.QueryFilter.ClearAndBackup();
        try
        {
            var user = mainDb.Queryable<UserEntity>().First(u => u.Id == openAccess.BindUserId);
            if (user == null)
            {
                throw BusinessError.Oh(SystemUserErrorCode.AccountNotFound);
            }

            return new BoundUser(user);
        }
        finally
        {
            mainDb.QueryFilter.Restore();
        }
    }
}
