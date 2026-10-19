using System.ComponentModel;
using GameFrameX.Core.Const;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Service;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.Tenant.ObjectStorage;
using GameFrameX.Entity.Tenant.ObjectStorage;
using GameFrameX.ObjectStorage.Abstract;

#pragma warning disable CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Tenant.Integration.TenantObjectStorageConfig;

/// <summary>
/// 租户对象存储配置管理服务：CRUD + 提供商/Schema 查询 + 连通性测试。
/// 业务规则：Provider 创建后不可变更；同租户至多一条启用；ConfigJson 白名单规范化与脱敏。
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 111)]
public class TenantObjectStorageConfigEntityService
    : GenericCrudService<TenantObjectStorageConfigEntity,
        AddTenantObjectStorageConfigRequest, UpdateTenantObjectStorageConfigRequest,
        TenantObjectStorageConfigResponse, PageTenantObjectStorageConfigRequest>
{
    /// <summary>
    /// 启用切换进程级串行锁。同租户“停用其它 + 写当前启用”必须串行，避免并发 AddAsync/UpdateAsync 产生多条启用配置。
    /// <para>注：当前为单实例部署的进程级锁；未来水平扩展为多实例时，需升级为分布式锁或 DB 级唯一约束。</para>
    /// </summary>
    private static readonly SemaphoreSlim ActiveConfigTransitionLock = new(1, 1);

    private readonly IObjectStorageConfigSchemaProvider _schemaProvider;
    private readonly IObjectStorageAdapterFactory _adapterFactory;

    public TenantObjectStorageConfigEntityService(
        SqlSugarRepository<TenantObjectStorageConfigEntity> repository,
        IObjectStorageConfigSchemaProvider schemaProvider,
        IObjectStorageAdapterFactory adapterFactory)
        : base(repository)
    {
        _schemaProvider = schemaProvider;
        _adapterFactory = adapterFactory;
    }

    protected override ISugarQueryable<TenantObjectStorageConfigResponse> BuildPageQuery(PageTenantObjectStorageConfigRequest input)
    {
        var searchKey = input.SearchKey?.Trim();
        var provider = NormalizeProvider(input.Provider);
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                      (u.Provider ?? string.Empty).Contains(searchKey) ||
                                      (u.Remark ?? string.Empty).Contains(searchKey))
                         .WhereIF(!string.IsNullOrWhiteSpace(provider), u => u.Provider == provider)
                         .WhereIF(input.IsEnabled.HasValue, u => (u.IsEnabled ?? false) == input.IsEnabled.Value)
                         .Select<TenantObjectStorageConfigResponse>();
    }

    /// <summary>
    /// 新增配置。校验 Provider 已注册、规范化 ConfigJson；若启用则在事务内停用同租户其它配置。
    /// </summary>
    public override async Task<TenantObjectStorageConfigResponse> AddAsync(AddTenantObjectStorageConfigRequest input)
    {
        var provider = NormalizeProvider(input.Provider);
        EnsureProviderRegistered(provider);

        var entity = input.Adapt<TenantObjectStorageConfigEntity>();
        entity.Provider = provider;
#pragma warning disable CS8625 // 第 4 参数 existingConfigJson=null 表示新增场景无旧值，跨项目方法签名不可改
        entity.ConfigJson = ObjectStorageConfigNormalizer.Normalize(_schemaProvider, provider, input.ConfigJson ?? string.Empty, null);
#pragma warning restore CS8625
        // 凭证静态加密收口（C370）：规范化之后、落库之前对 schema 敏感字段加密
        entity.ConfigJson = TenantObjectStorageConfigCrypto.EncryptConfigJson(provider, entity.ConfigJson);

        await ActiveConfigTransitionLock.WaitAsync();
        try
        {
            var tran = await Repository.Context.Ado.UseTranAsync(async () =>
            {
                if (entity.IsEnabled == true)
                {
                    await DeactivateOthersAsync(entity.Id);
                }

                await Repository.InsertAsync(entity);
            });
            if (!tran.IsSuccess)
            {
                throw BusinessError.Oh("保存对象存储配置失败：" + (tran.ErrorException?.Message ?? tran.ErrorMessage));
            }
        }
        finally
        {
            ActiveConfigTransitionLock.Release();
        }

        return entity.Adapt<TenantObjectStorageConfigResponse>();
    }

    /// <summary>
    /// 更新配置。Provider 必须与既有记录一致；ConfigJson 规范化（****** 沿用旧值）；启用时事务停用其它。
    /// </summary>
    public override async Task UpdateAsync(UpdateTenantObjectStorageConfigRequest input)
    {
        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh("对象存储配置不存在");
        }

        var provider = NormalizeProvider(input.Provider);
        if (!string.Equals(provider, entity.Provider, StringComparison.OrdinalIgnoreCase))
        {
            throw BusinessError.Oh("存储提供商不可变更，如需切换请新建配置");
        }

        entity.PathTemplate = input.PathTemplate;
        entity.MaxFileSizeBytes = input.MaxFileSizeBytes;
        entity.AllowedContentTypesJson = input.AllowedContentTypesJson;
        entity.EnableMd5 = input.EnableMd5;
        entity.IsEnabled = input.IsEnabled;
        entity.Remark = input.Remark;
        entity.ConfigJson = ObjectStorageConfigNormalizer.Normalize(_schemaProvider, entity.Provider, input.ConfigJson ?? string.Empty, entity.ConfigJson);
        // 凭证静态加密收口（C370）：Normalize 的 ****** 保留语义从旧值（密文）取回的仍是密文，Encrypt 幂等跳过；新明文字段在此加密
        entity.ConfigJson = TenantObjectStorageConfigCrypto.EncryptConfigJson(entity.Provider, entity.ConfigJson);
        entity.RowVersion = input.RowVersion;

        await ActiveConfigTransitionLock.WaitAsync();
        try
        {
            var tran = await Repository.Context.Ado.UseTranAsync(async () =>
            {
                if (entity.IsEnabled == true)
                {
                    await DeactivateOthersAsync(entity.Id);
                }

                await Repository.AsUpdateable(entity)
                                .UpdateColumns(e => new
                                {
                                    e.PathTemplate,
                                    e.MaxFileSizeBytes,
                                    e.AllowedContentTypesJson,
                                    e.EnableMd5,
                                    e.IsEnabled,
                                    e.Remark,
                                    e.ConfigJson,
                                    e.RowVersion,
                                })
                                .ExecuteCommandWithOptLockAsync(true);
            });
            if (!tran.IsSuccess)
            {
                throw BusinessError.Oh("更新对象存储配置失败：" + (tran.ErrorException?.Message ?? tran.ErrorMessage));
            }
        }
        finally
        {
            ActiveConfigTransitionLock.Release();
        }
    }

    /// <summary>
    /// 分页查询，ConfigJson 整体脱敏。
    /// </summary>
    [HttpPost(CrudActionRoutes.Page)]
    [ApiDescriptionSettings(Name = "Page")]
    public override async Task<SqlSugarPagedList<TenantObjectStorageConfigResponse>> PageAsync(PageTenantObjectStorageConfigRequest input)
    {
        var result = await base.PageAsync(input);
        foreach (var item in result.Items)
        {
            item.ConfigJson = SensitiveFieldMask.MaskValue;
        }

        return result;
    }

    /// <summary>
    /// 详情，ConfigJson 字段级脱敏（仅敏感字段替换为 ******）。
    /// </summary>
    [HttpGet(CrudActionRoutes.Detail)]
    [ApiDescriptionSettings(Name = "Detail")]
    public override async Task<TenantObjectStorageConfigResponse> GetAsync([FromQuery] BaseIdRequest input)
    {
        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh("对象存储配置不存在");
        }

        entity.ConfigJson = ObjectStorageConfigNormalizer.MaskFields(_schemaProvider, entity.Provider, entity.ConfigJson);
        return entity.Adapt<TenantObjectStorageConfigResponse>();
    }

    /// <summary>
    /// 获取所有支持的存储提供商清单。
    /// </summary>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Providers")]
    public List<ObjectStorageProviderItem> GetProviders()
    {
        return _schemaProvider.GetProviders();
    }

    /// <summary>
    /// 获取指定提供商的配置字段 Schema。
    /// </summary>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Schema")]
    public ObjectStorageConfigSchema GetSchema([FromQuery] string provider)
    {
        var normalized = NormalizeProvider(provider);
        return _schemaProvider.GetSchema(normalized);
    }

    /// <summary>
    /// 使用已保存配置测试连通性（不回显凭证）。
    /// </summary>
    [HttpGet]
    [ApiDescriptionSettings(Name = "TestConnection")]
    public async Task<bool> TestConnectionAsync([FromQuery] BaseIdRequest input)
    {
        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh("对象存储配置不存在");
        }

        if (!_adapterFactory.TryResolve(entity.Provider, out var adapter))
        {
            throw BusinessError.Oh("不支持的存储提供商: " + entity.Provider);
        }

        var context = _adapterFactory.BuildContext(entity.Provider, TenantObjectStorageConfigCrypto.DecryptConfigJson(entity.Provider, entity.ConfigJson), entity.PathTemplate);
        return await adapter.TestConnectionAsync(context, default);
    }

    private async Task DeactivateOthersAsync(long currentId)
    {
        await Repository.AsUpdateable()
                        .SetColumns(e => new TenantObjectStorageConfigEntity { IsEnabled = false, })
                        .Where(e => e.IsEnabled == true && e.Id != currentId)
                        .ExecuteCommandAsync();
    }

    private static void EnsureProviderRegistered(string provider)
    {
        if (!ObjectStorageProviderRegistry.TryGet(provider, out _))
        {
            throw BusinessError.Oh("不支持的存储提供商: " + provider);
        }
    }

    private static string NormalizeProvider(string provider)
    {
        return (provider ?? string.Empty).Trim().ToLowerInvariant();
    }
}
#pragma warning restore CS8604, CS8629