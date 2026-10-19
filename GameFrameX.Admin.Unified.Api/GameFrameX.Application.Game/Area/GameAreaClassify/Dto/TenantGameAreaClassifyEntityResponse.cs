namespace GameFrameX.Application.Game.Area.GameAreaClassify.Dto;

/// <summary>
/// 游戏大区分类输出参数
/// </summary>
public class TenantGameAreaClassifyEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGameAreaClassifyEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 应用Id。运营内容应用作用域（TenantId + AppId），必须 &gt; 0。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 本地化Key
    /// </summary>
    public string LocalizationKey { get; set; } = null!;

    /// <summary>
    /// 大区服务器 HTTP管理器URL
    /// </summary>
    public string HttpManagerUrl { get; set; } = string.Empty;

    /// <summary>
    /// 镜像仓库地址
    /// </summary>
    public string DockerRegister { get; set; } = null!;

    /// <summary>
    /// 镜像名称
    /// </summary>
    public string ImageName { get; set; } = null!;

    /// <summary>
    /// 仓库用户名
    /// </summary>
    public string DockerUserName { get; set; } = null!;

    /// <summary>
    /// 容器网络名
    /// </summary>
    public string ContainerNetName { get; set; } = null!;

    /// <summary>
    /// 容器网络子网
    /// </summary>
    public string ContainerSubnet { get; set; } = null!;

    /// <summary>
    /// 容器网络网关
    /// </summary>
    public string ContainerGateway { get; set; } = null!;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}