namespace GameFrameX.Application.Game.Area.GameAreaClassify.Dto;

/// <summary>
/// 游戏大区分类实体基类 - 包含 AddInput 和 UpdateInput 的公共字段
/// </summary>
public abstract class TenantGameAreaClassifyEntityBaseRequest : BaseUpdateRequest
{
    /// <summary>
    /// 应用Id。运营内容应用作用域，必须 > 0。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    [Required(ErrorMessage = "详细描述不能为空")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// 本地化Key
    /// </summary>
    public string LocalizationKey { get; set; } = string.Empty;

    /// <summary>
    /// 大区服务器 HTTP管理器URL
    /// </summary>
    public string HttpManagerUrl { get; set; } = string.Empty;

    /// <summary>
    /// 镜像仓库地址
    /// </summary>
    public string DockerRegister { get; set; } = string.Empty;

    /// <summary>
    /// 仓库用户名
    /// </summary>
    public string DockerUserName { get; set; } = string.Empty;

    /// <summary>
    /// 仓库密码
    /// </summary>
    public string DockerPassword { get; set; } = string.Empty;

    /// <summary>
    /// 容器网络名
    /// </summary>
    public string ContainerNetName { get; set; } = string.Empty;

    /// <summary>
    /// 容器网络子网
    /// </summary>
    public string ContainerSubnet { get; set; } = string.Empty;

    /// <summary>
    /// 容器网络网关
    /// </summary>
    public string ContainerGateway { get; set; } = string.Empty;

    /// <summary>
    /// 镜像源
    /// </summary>
    public string ImageName { get; set; } = string.Empty;
}