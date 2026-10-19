using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 区服分类
/// </summary>
[SugarTable(null, "区服分类")]
[SugarIndex("idx_tenant_game_area_classify_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameAreaClassifyEntity : EntityBasicSelect, IAppScoped
{
    /// <summary>
    /// 应用Id。运营内容应用作用域（TenantId + AppId），必须 &gt; 0。
    /// <para>来源：Admin 后台当前应用上下文写入。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 本地化Key
    /// </summary>
    [SugarColumn(ColumnDescription = "本地化Key", Length = EntityStringLength.Length1024)]
    public string LocalizationKey { get; set; } = null!;

    /// <summary>
    /// 大区服务器 HTTP管理器URL
    /// </summary>
    public string HttpManagerUrl { get; set; } = string.Empty;


    /// <summary>
    /// 镜像仓库地址
    /// </summary>
    [SugarColumn(ColumnDescription = "镜像仓库地址", Length = EntityStringLength.Length256)]
    public string DockerRegister { get; set; } = null!;

    /// <summary>
    /// 容器网络网关
    /// </summary>
    [SugarColumn(ColumnDescription = "镜像名称", Length = EntityStringLength.Length256)]
    public string ImageName { get; set; } = null!;

    /// <summary>
    /// 仓库用户名
    /// </summary>
    [SugarColumn(ColumnDescription = "仓库用户名", Length = EntityStringLength.Length256)]
    public string DockerUserName { get; set; } = null!;

    /// <summary>
    /// 仓库密码
    /// </summary>
    [SugarColumn(ColumnDescription = "仓库密码", Length = EntityStringLength.Length256)]
    public string DockerPassword { get; set; } = null!;

    /// <summary>
    /// 本地化Key
    /// </summary>
    [SugarColumn(ColumnDescription = "容器网络名", Length = EntityStringLength.Length256)]
    public string ContainerNetName { get; set; } = null!;

    /// <summary>
    /// 容器网络子网
    /// </summary>
    [SugarColumn(ColumnDescription = "容器网络子网", Length = EntityStringLength.Length256)]
    public string ContainerSubnet { get; set; } = null!;

    /// <summary>
    /// 容器网络网关
    /// </summary>
    [SugarColumn(ColumnDescription = "容器网络网关", Length = EntityStringLength.Length256)]
    public string ContainerGateway { get; set; } = null!;
}