namespace GameFrameX.Core.Option;

/// <summary>
/// 种子设置
/// </summary>
public sealed class SeedSettings
{
    /// <summary>
    /// 启用种子初始化。默认 true：启动即执行种子补缺，配置文件可省略本节点。
    /// </summary>
    public bool EnableInitSeed { get; set; } = true;

    /// <summary>
    /// 启用种子增量更新。默认 false：执行全部种子类——普通种子只补缺（insert-only，不覆盖已有行），
    /// 带 <c>[IncreSeed]</c> 特性的权威收敛种子仍按 seed 值覆盖更新。
    /// 设为 true 时收窄为只执行权威收敛种子（大库快速启动）。
    /// </summary>
    public bool EnableIncreSeed { get; set; }
}
