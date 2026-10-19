namespace GameFrameX.Application.Game.TimeJob.GameTimeJob.Dto;

/// <summary>
/// 游戏定时任务更新输入参数
/// </summary>
public class UpdateTenantGameTimeJobEntityRequest : BaseUpdateRequest
{
    /// <summary>
    /// 应用Id。不可变（服务端从既有实体回填）；声明于 DTO 仅为契约对称。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 名称。此前 UpdateRequest 缺此声明：编辑提交的 name 被绑定丢弃，Adapt 后实体 Name=null，
    /// 又被 IgnoreColumns(true)（忽略 null 列）跳过 —— update 200 但名称静默不更新（与兑换码渠道问题同模式）。
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Cron 表达式
    /// </summary>
    [Required(ErrorMessage = "Cron表达式不能为空")]
    public string Cron { get; set; } = null!;

    /// <summary>
    /// 是否一次性任务
    /// </summary>
    public bool IsOnce { get; set; }
}
