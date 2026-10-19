namespace GameFrameX.Hub.Api.Dto.FeatureSwitchSync;

/// <summary>
/// 功能开关服务器同步响应（C347 版本化差异协议，两通道同形）。
/// <para><see cref="Version"/> 由投放解析器对有效候选集做 canonical md5 生成（小写 32 位 hex）：
/// 任一业务字段变化或行集合增减必变；活动时间窗进出自然变版本，消费方短轮询即可感知差异。</para>
/// </summary>
public class FeatureSwitchSyncResponse
{
    /// <summary>
    /// 当前投放版本号（小写 32 位 md5 hex）
    /// </summary>
    public string Version { get; set; } = null!;

    /// <summary>
    /// 请求携带的 Version 与当前版本相同时为 true，此时 <see cref="Switches"/> 为空数组（Version 仍返回）
    /// </summary>
    public bool Unchanged { get; set; }

    /// <summary>
    /// 命中的生效开关集合（按开关键排序；Unchanged=true 时为空数组）
    /// </summary>
    public List<FeatureSwitchSyncItemResponse> Switches { get; set; } = new();

    /// <summary>
    /// 服务器当前时间（Unix 毫秒）；不作为授权依据
    /// </summary>
    public long ServerTime { get; set; }
}
