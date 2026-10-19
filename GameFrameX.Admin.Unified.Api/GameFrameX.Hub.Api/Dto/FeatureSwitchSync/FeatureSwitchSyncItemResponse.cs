namespace GameFrameX.Hub.Api.Dto.FeatureSwitchSync;

/// <summary>
/// 单条生效开关的投放投影（C347 Hub 通道，与 Client.Api 客户端通道同形）。
/// <para><see cref="ValueJson"/> 为开关参数 JSON 原文，游戏服务器自行解析（服务端不做 schema 校验）。</para>
/// </summary>
public class FeatureSwitchSyncItemResponse
{
    /// <summary>
    /// 开关键
    /// </summary>
    public string SwitchKey { get; set; } = null!;

    /// <summary>
    /// 开关参数 JSON 原文
    /// </summary>
    public string ValueJson { get; set; } = null!;

    /// <summary>
    /// 生效开始时间（Unix 秒，UTC）；null 表示不限
    /// </summary>
    public long? StartTime { get; set; }

    /// <summary>
    /// 生效结束时间（Unix 秒，UTC）；null 表示不限
    /// </summary>
    public long? EndTime { get; set; }
}
