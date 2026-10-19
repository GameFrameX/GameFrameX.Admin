using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.System.Job.Dto;

/// <summary>
/// 系统作业集群输出参数
/// </summary>
public class SystemJobClusterResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 作业集群Id
    /// </summary>
    public string ClusterId { get; set; } = string.Empty;

    /// <summary>
    /// 描述信息
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public ClusterStatus Status { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdatedTime { get; set; }

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