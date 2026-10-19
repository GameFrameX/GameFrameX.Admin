namespace GameFrameX.Core.Extension.Option;

/// <summary>
/// Foundation 雪花 Id 配置选项（基于 GameFrameX.Foundation.Utility.DistributedSystem.Snowflake，紧凑短 ID 布局），
/// 绑定 appsettings.json 的 "SnowFlake" 节。
/// <para>多实例互斥契约（2026-09-15 紧凑布局决策）：同库写入的每个实例必须互斥配置 WorkerId（0-7）；
/// 越界在启动期显式失败，单数据中心同库写入实例上限 8。DataCenter 维度已移除（恒 0）。</para>
/// </summary>
[OptionsSettings("SnowFlake")]
public sealed class SnowFlakeIdOptions : IConfigurableOptions
{
    /// <summary>
    /// 工作节点 Id（0-7）。同库写入的全部实例（Web.Api 多副本 / Hub / Client）必须互斥配置；
    /// 越界（超出 0-7）启动期显式失败。环境变量入口 SnowFlake__WorkerId。
    /// </summary>
    public int WorkerId { get; set; } = 1;
}
