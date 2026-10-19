namespace GameFrameX.Application.Game.DockerTag.GameDockerTag.Dto;

/// <summary>
/// 游戏Docker标签增加输入参数
/// </summary>
// AddAsync 必须继承 BaseAddRequest（空基类），不应携带主键/版本号。
// 此前继承 TenantGameDockerTagEntityBaseRequest（其又继承 BaseUpdateRequest），会把 [Required] Id/Version 带入 AddAsync 校验，
// 导致新增报 {"Version":["版本号不能为空"]}。业务字段在此直接声明，与 AddGameAppRequest 约定一致。
public class AddTenantGameDockerTagEntityRequest : BaseAddRequest
{
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
    /// tag
    /// </summary>
    public string ServerVersion { get; set; } = null!;

    /// <summary>
    /// 应用Id。运营内容应用作用域，必须 &gt; 0；由 Admin 后台当前应用上下文写入。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }
}