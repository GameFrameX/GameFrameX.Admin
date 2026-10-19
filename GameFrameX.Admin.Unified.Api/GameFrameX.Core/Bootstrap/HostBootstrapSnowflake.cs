using Microsoft.AspNetCore.Builder;

namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// 三 host（Client / Hub / Web）共享的雪花 ID 启动初始化（C261，GFX-668；C360 紧凑布局与 WorkerId 防撞）。
/// <para>应用紧凑短 ID 布局（2026-09-15 决策冻结：epoch 2026-09-01 / Worker 3 位 0-7 / DataCenter 0 位 / 序列 10 位）
/// 并按配置初始化 WorkerId。多实例同库部署时 ID 唯一性仅由 (时间戳, WorkerId, 序列) 保证——
/// 同库写入的每个实例必须互斥配置 <c>SnowFlake:WorkerId</c>（环境变量 SnowFlake__WorkerId，0-7），
/// 越界在启动期显式失败（单数据中心同库写入实例上限 8）。</para>
/// </summary>
public static class HostBootstrapSnowflake
{
    /// <summary>
    /// 显式初始化雪花 ID 生成器：应用紧凑布局 → 解析 WorkerId（<c>SnowFlake:WorkerId</c>，缺省回落 host 兜底值）
    /// → 校验合法域 → 预热 <see cref="SnowFlakeIdHelper.Instance"/>。
    /// </summary>
    /// <param name="builder">Host 配置。</param>
    /// <param name="workerIdIfUnconfigured">
    /// 配置缺省时的兜底 WorkerId（各 host 错开：Web.Api 配置 1 / Hub 兜底 2 / Client 兜底 3，单机全默认部署互不冲突）。
    /// </param>
    public static void UseSnowFlake(this WebApplicationBuilder builder, int workerIdIfUnconfigured)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var workerId = SnowFlakeExtension.ResolveWorkerId(builder.Configuration, workerIdIfUnconfigured);
        SnowFlakeExtension.InitSnowFlake(workerId);
    }
}