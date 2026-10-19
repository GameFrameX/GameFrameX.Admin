namespace GameFrameX.Application.Game.TimeJob.GameTimeJob.Dto;

/// <summary>
/// 游戏定时任务增加输入参数
/// </summary>
public class AddTenantGameTimeJobEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 应用Id。运营内容应用作用域，必须 &gt; 0；由 Admin 后台当前应用上下文写入。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 名称。此前 AddRequest 缺此声明：表单提交的 name 被绑定丢弃，Adapt 后实体 Name=null，
    /// 实体列 NOT NULL 导致 insert 500（cron 同因，报错先撞上 cron）。
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Cron 表达式（实体列 NOT NULL）
    /// </summary>
    [Required(ErrorMessage = "Cron表达式不能为空")]
    public string Cron { get; set; } = null!;

    /// <summary>
    /// 是否一次性任务
    /// </summary>
    public bool IsOnce { get; set; }

    /// <summary>
    /// 任务来源。系统标记非用户输入：手动新增由服务端固定写 "Manual"（实体列 NOT NULL）。
    /// </summary>
    public string Source { get; set; } = null!;
}
