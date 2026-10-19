namespace GameFrameX.Application.Game.Area.GameAreaClassify.Dto;

/// <summary>
/// 游戏大区分类增加输入参数
/// </summary>
// AddAsync 必须继承 BaseAddRequest（空基类），不应携带主键/版本号。
// 此前继承 TenantGameAreaClassifyEntityBaseRequest（其又继承 BaseUpdateRequest），会把 [Required] Id/Version 带入 AddAsync 校验，
// 导致新增报 {"Version":["版本号不能为空"]}。业务字段在此直接声明（与 TenantGameAreaClassifyEntityBaseRequest 镜像），与 AddGameAppRequest 约定一致。
public class AddTenantGameAreaClassifyEntityRequest : BaseAddRequest
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