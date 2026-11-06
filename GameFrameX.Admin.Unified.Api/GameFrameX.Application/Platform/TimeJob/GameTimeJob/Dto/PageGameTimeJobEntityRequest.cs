namespace GameFrameX.Application.Platform.TimeJob.GameTimeJob.Dto;

/// <summary>
/// 游戏定时任务分页查询输入参数
/// </summary>
public class PageGameTimeJobEntityRequest : BasePageRequest
{

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;
}