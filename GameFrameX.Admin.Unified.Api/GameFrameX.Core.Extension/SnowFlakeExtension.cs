using System.Globalization;
using System.Reflection;
using GameFrameX.Core.Extension.Option;
using GameFrameX.Foundation.Utility.DistributedSystem.Snowflake;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameFrameX.Core.Extension;

/// <summary>
/// 基于 Foundation Snowflake（<see cref="SnowFlakeIdHelper"/>）的紧凑短 ID 布局应用与 WorkerId 装配扩展。
/// <para>布局（2026-09-15 决策冻结，gfx-doc/decisions/2026-09-15-compact-snowflake-id.md）：
/// epoch 2026-09-01 / WorkerId 3 位（0-7）/ DataCenter 0 位（维度移除，恒 0）/ 序列 10 位，
/// 产出 13-14 位数字（&lt; 2^53 JS 安全域）。ID 唯一性仅由 (时间戳, WorkerId, 序列) 保证。</para>
/// <para>多实例互斥采用显式配置路径（Redis/Cluster 适配已随 C170 / GFX-181 退役，进程内缓存的租约无法跨进程互斥）：
/// 同库写入的每个实例（Web.Api 多副本 / Hub / Client）必须互斥配置 <c>SnowFlake:WorkerId</c>，
/// 越界（&gt; 7）在启动期显式失败——单数据中心同库实例上限 8。</para>
/// </summary>
public static class SnowFlakeExtension
{
    /// <summary>
    /// 紧凑布局部署纪元：2026-09-01T00:00:00Z（毫秒）。随决策冻结，落库后不可更改。
    /// </summary>
    internal const long CompactEpochMilliseconds = 1788220800000L;

    /// <summary>
    /// 紧凑布局 WorkerId 位宽：3（合法域 0-7）。
    /// </summary>
    internal const int CompactWorkerIdBits = 3;

    /// <summary>
    /// 紧凑布局数据中心位宽：0（单数据中心，维度移除，DataCenterId 恒 0）。
    /// </summary>
    internal const int CompactDatacenterIdBits = 0;

    /// <summary>
    /// 紧凑布局序列位宽：10（每 WorkerId 每毫秒 1024 个）。
    /// </summary>
    internal const int CompactSequenceBits = 10;

    /// <summary>
    /// Foundation 雪花紧凑布局单数据中心同库写入实例上限：8（WorkerId 合法域 0-7）。
    /// </summary>
    internal const int WorkerIdCount = 8;

    /// <summary>
    /// 应用紧凑短 ID 布局（Foundation 静态配置，必须在首次访问 <see cref="SnowFlakeIdHelper.Instance"/> 前生效）。
    /// <para>幂等：布局常量重复赋值无害。但若 <see cref="SnowFlakeIdHelper.Instance"/> 已创建则抛出——
    /// 单例创建后布局 statics 的赋值将静默失效，本进程会退回旧布局生成 18 位长 ID，
    /// 与其它紧凑布局进程的 ID 空间重叠（数据完整性风险），必须显式失败而非静默降级。</para>
    /// </summary>
    public static void ApplyCompactLayout()
    {
        var workerField = SnowflakeWorkerField();
        if (workerField is null || workerField.GetValue(null) is not null)
        {
            throw new InvalidOperationException(
                "紧凑雪花布局必须在首次访问 SnowFlakeIdHelper.Instance 之前应用：单例已创建（或 Foundation _worker 字段不可见），" +
                "继续启动会以旧布局生成 18 位长 ID 并与紧凑布局进程的 ID 空间重叠。请检查启动装配顺序，确保布局应用先于任何 NextId 调用。");
        }

        SnowFlakeIdHelper.BaseTime = CompactEpochMilliseconds;
        SnowFlakeIdHelper.WorkerIdBits = CompactWorkerIdBits;
        SnowFlakeIdHelper.DatacenterIdBits = CompactDatacenterIdBits;
        SnowFlakeIdHelper.SequenceBits = CompactSequenceBits;
        // DataCenter 维度移除（0 位）后 Foundation 默认 DataCenterId=1 越界（maxDatacenterId=0），
        // 首次触单例会抛 ArgumentException——布局应用必须同步收敛为 0。
        SnowFlakeIdHelper.DataCenterId = 0;
    }

    /// <summary>
    /// 校验 WorkerId 处于紧凑布局合法域 [0, <see cref="WorkerIdCount"/>)。
    /// <para>越界即启动失败（fail fast）：同库写入实例超过 8 个时无法互斥配置，属不可部署状态。</para>
    /// </summary>
    /// <param name="workerId">待校验的 WorkerId。</param>
    internal static void ValidateWorkerId(int workerId)
    {
        if (workerId < 0 || workerId >= WorkerIdCount)
        {
            throw new InvalidOperationException(
                $"SnowFlake WorkerId={workerId} 超出紧凑布局合法域 [0,{WorkerIdCount - 1}]（单数据中心同库写入实例上限 8）。" +
                "请为同库写入的每个实例（Web.Api 副本 / Hub / Client）互斥配置 SnowFlake:WorkerId（环境变量 SnowFlake__WorkerId）；" +
                "超过 8 个同库实例无法部署，启动失败为预期行为。");
        }
    }

    /// <summary>
    /// 从配置解析 WorkerId：<c>SnowFlake:WorkerId</c> 缺省（null/空白）时回落到 host 兜底默认值。
    /// <para>配置了值但不是合法纯数字（如 -1 / abc / 带空白）时抛 <see cref="InvalidOperationException"/>：
    /// 配置错误若静默回退兜底值，会以「看似正常」的 WorkerId 与其它实例撞库（同库写主键冲突），必须显式失败。</para>
    /// </summary>
    /// <param name="configuration">Host 配置。</param>
    /// <param name="workerIdIfUnconfigured">配置缺省时的兜底 WorkerId（各 host 错开，保证单机全默认部署互不冲突）。</param>
    /// <returns>解析后的 WorkerId（未校验合法域，由 <see cref="ValidateWorkerId"/> 承接）。</returns>
    public static int ResolveWorkerId(IConfiguration configuration, int workerIdIfUnconfigured)
    {
        var configured = configuration["SnowFlake:WorkerId"];
        if (string.IsNullOrWhiteSpace(configured))
        {
            return workerIdIfUnconfigured;
        }

        if (!int.TryParse(configured, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new InvalidOperationException(
                $"SnowFlake:WorkerId 配置值 \"{configured}\" 不是合法的非负整数（环境变量 SnowFlake__WorkerId 须为 0-7 纯数字）。" +
                "配置错误不允许静默回退兜底值——会以兜底 WorkerId 与其它同库实例撞库。");
        }

        return parsed;
    }

    /// <summary>
    /// 注册 Foundation Snowflake ID 生成器（紧凑布局 + 显式配置互斥）：
    /// 应用布局 → 校验 <see cref="SnowFlakeIdOptions.WorkerId"/> 合法域 → 初始化并预热单例。
    /// </summary>
    public static void AddSnowFlakeIdGenerator(this IServiceCollection services, SnowFlakeIdOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        InitSnowFlake(options.WorkerId);
    }

    /// <summary>
    /// 应用紧凑布局并以指定 WorkerId 初始化 Foundation Snowflake 单例（DataCenterId 恒 0）。
    /// 必须在首次访问 <see cref="SnowFlakeIdHelper.Instance"/> 前调用。
    /// </summary>
    /// <param name="workerId">工作节点 Id（0-7，越界抛 <see cref="InvalidOperationException"/>）。</param>
    public static void InitSnowFlake(int workerId)
    {
        ApplyCompactLayout();
        ValidateWorkerId(workerId);

        SnowFlakeIdHelper.WorkId = workerId;
        SnowFlakeIdHelper.DataCenterId = 0;
        // 触发单例创建：首次访问后 WorkId/DataCenterId 不可更改
        _ = SnowFlakeIdHelper.Instance;
    }

    /// <summary>
    /// SqlSugar 主键回填兜底取号（经 <c>StaticConfig.CustomSnowFlakeFunc</c> 消费，C361 / GFX-804）。
    /// <para>为什么需要兜底：该 func 由 <c>SqlSugarSnowflakeFallbackInitializer</c>（ModuleInitializer）进程级安装，
    /// 覆盖一切加载 Core.Extension 的进程——包括未走三 host 启动装配的路径（一次性工具 / 单测）。
    /// Foundation 单例已初始化（生产 host 启动期 <see cref="InitSnowFlake"/> 显式互斥 WorkerId 先行）时直接复用单例；
    /// 未初始化时兜底应用紧凑布局并以 WorkerId=0 初始化——该类进程不参与生产多实例部署，WorkerId=0 无撞库面。</para>
    /// <para>与 <see cref="ApplyCompactLayout"/> 抛错语义的差异：此处仅在「单例确实未创建」时应用布局，
    /// 不存在「先检查后应用」之间的重复创建窗口问题；Foundation <c>_worker</c> 反射字段不可见（版本漂移）时
    /// 同样经 <see cref="ApplyCompactLayout"/> 显式抛错，绝不静默产 18-19 位长 ID。</para>
    /// </summary>
    /// <returns>紧凑布局雪花 Id（13-14 位，&lt; 2^53）。</returns>
    internal static long NextSqlSugarFallbackId()
    {
        // ponytail: 「检查 _worker → 应用布局」无锁，未初始化进程并发首触存在竞窗——
        // Foundation Instance 创建自身带锁（Monitor.Enter），竞窗内交错最坏是 ApplyCompactLayout
        // 显式抛错（绝不静默产长 ID / 重复 ID）；生产 host 启动期单线程先行 InitSnowFlake，
        // 测试进程经 ModuleInitializer 先行应用布局，均不落此窗。加锁取号热路径得不偿失，不收。
        var worker = SnowflakeWorkerField()?.GetValue(null);
        if (worker is null)
        {
            ApplyCompactLayout();
            SnowFlakeIdHelper.WorkId = 0;
            SnowFlakeIdHelper.DataCenterId = 0;
            // 触发单例创建：首次访问后布局与 WorkerId=0 锁定，后续取号全部复用
            _ = SnowFlakeIdHelper.Instance;
        }

        return SnowFlakeIdHelper.Instance.NextId();
    }

    /// <summary>
    /// Foundation <c>SnowFlakeIdHelper._worker</c> 静态单例字段的反射句柄。
    /// <para>SnowFlakeIdHelper 未公开「单例已创建」查询，反射 _worker 是唯一检测手段；
    /// 字段缺失（Foundation 版本漂移）时调用方显式失败，防御不因版本升级而静默失效。</para>
    /// </summary>
    /// <returns>字段句柄；Foundation 结构漂移时为 null。</returns>
    private static FieldInfo? SnowflakeWorkerField()
    {
        return typeof(SnowFlakeIdHelper).GetField("_worker", BindingFlags.NonPublic | BindingFlags.Static);
    }
}
