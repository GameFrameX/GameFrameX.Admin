using System.Security.Cryptography;
using System.Text;
using GameFrameX.Application.Game.App.Rollout;
using GameFrameX.Core.Const;
using GameFrameX.Entity.Tenant.Game.FeatureSwitch;

namespace GameFrameX.Application.Game.App.FeatureSwitch;

/// <summary>
/// 活动/功能开关投放解析器（Client.Api 与 Hub.Api 共享的唯一实现，两通道不得各自实现解析）。
/// <para>解析分三步：① 按租户 + 应用取候选行（应用行 + 租户默认层行）；② 在候选行上算投放版本号
/// <c>Version</c>（Enabled 且在时间窗内的行集合，未做作用域匹配与设备灰度过滤）；③ 做作用域匹配 + 设备灰度
/// 过滤后，每个开关键选出一个 winner。</para>
/// <para>判定权原则：本解析器只负责「下发什么」，不承载授权语义。玩法/经济 gate 由消费方业务端点
/// 依据开关值与时间窗实时判定。</para>
/// </summary>
public sealed class FeatureSwitchDeliveryResolver
{
    /// <summary>
    /// 应用作用域命中加分。应用行必须压过租户默认层行
    /// </summary>
    private const int AppScopeScore = 64;

    /// <summary>
    /// 平台作用域命中加分
    /// </summary>
    private const int PlatformScopeScore = 32;

    /// <summary>
    /// 包名作用域命中加分
    /// </summary>
    private const int PackageScopeScore = 16;

    /// <summary>
    /// 语言作用域命中加分
    /// </summary>
    private const int LanguageScopeScore = 4;

    /// <summary>
    /// 渠道作用域命中加分
    /// </summary>
    private const int ChannelScopeScore = 2;

    private readonly SqlSugarRepository<TenantGameAppFeatureSwitchEntity> _repository;

    /// <summary>
    /// 初始化活动/功能开关投放解析器。
    /// </summary>
    /// <param name="repository">开关注册表仓储（自带租户与软删除过滤）。</param>
    public FeatureSwitchDeliveryResolver(SqlSugarRepository<TenantGameAppFeatureSwitchEntity> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 查询候选行并解析出本次投放结果。
    /// </summary>
    /// <param name="query">投放解析入参。</param>
    /// <returns>投放版本号与生效开关集合。</returns>
    public async Task<FeatureSwitchDeliveryResult> ResolveAsync(FeatureSwitchDeliveryQuery query)
    {
        // 候选查询只按应用作用域收口：版本号定义为「（租户, 应用, Audience）视角」的配置指纹，
        // 若在 SQL 层按调用方的平台/包名/语言/渠道裁剪，同一份配置会因调用方维度不同而得名多个版本号，
        // 使差异协议在维度变化时误报 Unchanged。作用域匹配下移到 Resolve 的行过滤。
        var candidates = await _repository.AsQueryable()
                                          .Where(u => u.AppId == query.AppId || u.AppId == null || u.AppId == 0)
                                          .ToListAsync();
        return Resolve(candidates, query);
    }

    /// <summary>
    /// 在给定候选行上解析投放结果（纯函数，便于对语义做确定性断言）。
    /// </summary>
    /// <param name="candidates">候选行集合，顺序不参与计算。</param>
    /// <param name="query">投放解析入参。</param>
    /// <returns>投放版本号与生效开关集合。</returns>
    public static FeatureSwitchDeliveryResult Resolve(IReadOnlyList<TenantGameAppFeatureSwitchEntity> candidates, FeatureSwitchDeliveryQuery query)
    {
        var effective = candidates.Where(row => IsEffective(row, query)).ToList();
        var switches = effective.Where(row => IsScopeMatch(row, query))
                                .Where(row => IsRolloutHit(row, query))
                                .GroupBy(row => row.SwitchKey, StringComparer.Ordinal)
                                .Select(group => SelectWinner(group, query))
                                .OrderBy(item => item.SwitchKey, StringComparer.Ordinal)
                                .ToList();
        return new FeatureSwitchDeliveryResult(BuildVersion(effective), switches);
    }

    /// <summary>
    /// 有效候选行过滤：启用 + 消费方匹配 + 落在时间窗内。
    /// <para>本集合是版本号的输入，故此处刻意不做作用域匹配与设备灰度过滤。</para>
    /// </summary>
    /// <param name="row">候选行。</param>
    /// <param name="query">投放解析入参。</param>
    /// <returns>有效返回 true，否则返回 false。</returns>
    private static bool IsEffective(TenantGameAppFeatureSwitchEntity row, FeatureSwitchDeliveryQuery query)
    {
        if (row.IsEnabled == false)
        {
            return false;
        }

        if (!IsAudienceMatch(row.Audience, query.Audience))
        {
            return false;
        }

        return IsInTimeWindow(row, query.NowUtc);
    }

    /// <summary>
    /// 消费方过滤：both 对两个通道均下发，其余按通道视角取同名单边。
    /// </summary>
    /// <param name="audience">行的消费方标记。</param>
    /// <param name="target">本次请求的通道视角。</param>
    /// <returns>匹配返回 true，否则返回 false。</returns>
    private static bool IsAudienceMatch(string audience, FeatureSwitchAudience target)
    {
        if (audience == FeatureSwitchAudienceValues.Both)
        {
            return true;
        }

        if (target == FeatureSwitchAudience.Client)
        {
            return audience == FeatureSwitchAudienceValues.Client;
        }

        return audience == FeatureSwitchAudienceValues.Server;
    }

    /// <summary>
    /// 时间窗过滤：区间语义为左闭右开（nowUtc == StartTime 视为已开始，nowUtc == EndTime 视为已结束）。
    /// <para>实体列为 Unix 秒（UTC），此处统一换算为 UTC 时刻比较，不依赖进程本地时区。</para>
    /// </summary>
    /// <param name="row">候选行。</param>
    /// <param name="nowUtc">本次判定的 UTC 服务器时间。</param>
    /// <returns>落在窗内返回 true，否则返回 false。</returns>
    private static bool IsInTimeWindow(TenantGameAppFeatureSwitchEntity row, DateTime nowUtc)
    {
        if (row.StartTime.HasValue && nowUtc < ToUtc(row.StartTime.Value))
        {
            return false;
        }

        if (row.EndTime.HasValue && nowUtc >= ToUtc(row.EndTime.Value))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 作用域匹配：Default 哨兵行对任意调用方均匹配；具体值行要求调用方携带同一维度值。
    /// <para>调用方缺该维度上下文（传 null）时只可能命中 Default 哨兵行——服务器通道四维均传 null 即此情形。</para>
    /// </summary>
    /// <param name="row">候选行。</param>
    /// <param name="query">投放解析入参。</param>
    /// <returns>四维全部匹配返回 true，否则返回 false。</returns>
    private static bool IsScopeMatch(TenantGameAppFeatureSwitchEntity row, FeatureSwitchDeliveryQuery query)
    {
        return IsDimensionMatch(row.Platform, query.Platform, GameDimensionConst.PlatformDefault)
               && IsDimensionMatch(row.Language, query.Language, GameDimensionConst.LanguageDefault)
               && IsDimensionMatch(row.Channel, query.Channel, GameDimensionConst.ChannelNameDefault)
               && IsPackageMatch(row.Package, query.PackageCandidates);
    }

    /// <summary>
    /// 单维匹配：行值等于 Default 哨兵即放行，否则要求调用方值与之逐字符相等。
    /// </summary>
    /// <param name="rowValue">行的该维度取值。</param>
    /// <param name="queryValue">调用方携带的该维度取值，null 表示无该维度上下文。</param>
    /// <param name="defaultValue">该维度的 Default 哨兵值。</param>
    /// <returns>匹配返回 true，否则返回 false。</returns>
    private static bool IsDimensionMatch(string rowValue, string? queryValue, string defaultValue)
    {
        if (rowValue == defaultValue)
        {
            return true;
        }

        return queryValue != null && string.Equals(rowValue, queryValue, StringComparison.Ordinal);
    }

    /// <summary>
    /// 包名匹配：Default 包名哨兵行对任意调用方均匹配；其余要求命中调用方传入的母包候选之一。
    /// </summary>
    /// <param name="rowValue">行的包名。</param>
    /// <param name="packageCandidates">调用方传入的包名候选（母包候选已由调用方解析）；null / 空表示无候选。</param>
    /// <returns>匹配返回 true，否则返回 false。</returns>
    private static bool IsPackageMatch(string rowValue, IReadOnlyList<string>? packageCandidates)
    {
        if (rowValue == GameDimensionConst.PackageNameDefault)
        {
            return true;
        }

        return packageCandidates != null && packageCandidates.Contains(rowValue, StringComparer.Ordinal);
    }

    /// <summary>
    /// 设备灰度过滤：客户端通道走稳定设备标识取模 + 白名单旁路；服务器通道无设备身份，一律放行。
    /// </summary>
    /// <param name="row">候选行。</param>
    /// <param name="query">投放解析入参。</param>
    /// <returns>命中返回 true，否则返回 false。</returns>
    private static bool IsRolloutHit(TenantGameAppFeatureSwitchEntity row, FeatureSwitchDeliveryQuery query)
    {
        if (query.Audience == FeatureSwitchAudience.Server)
        {
            return true;
        }

        return StableDeviceRolloutMatcher.IsMatch(
            tenantId: query.TenantId,
            appId: row.AppId,
            recordId: row.Id,
            deviceId: query.DeviceId,
            rolloutPercent: row.RolloutPercent,
            deviceWhitelistJson: row.DeviceWhitelistJson);
    }

    /// <summary>
    /// 同开关键内选 winner：作用域评分降序 → 投放优先级降序 → 主键降序（新行优先）。
    /// </summary>
    /// <param name="group">同开关键的候选行（已通过作用域与灰度过滤）。</param>
    /// <param name="query">投放解析入参，提供评分所需的作用域上下文。</param>
    /// <returns>该开关键的投放投影。</returns>
    private static FeatureSwitchEffectiveItem SelectWinner(IGrouping<string, TenantGameAppFeatureSwitchEntity> group, FeatureSwitchDeliveryQuery query)
    {
        var winner = group.OrderByDescending(row => CalculateScore(row, query))
                          .ThenByDescending(row => row.Priority)
                          .ThenByDescending(row => row.Id)
                          .First();
        return new FeatureSwitchEffectiveItem(winner.SwitchKey, winner.ValueJson, ToNullableUtc(winner.StartTime), ToNullableUtc(winner.EndTime));
    }

    /// <summary>
    /// 作用域评分：App 64 / Platform 32 / Package 16 / Language 4 / Channel 2。
    /// <para>租户默认层行（AppId 为 null / 0）不拿应用分，故应用覆盖行恒压过租户默认行；
    /// 调用方未携带的维度（传 null）不参与计分，命中 Default 哨兵行因而恒得 0 分。</para>
    /// </summary>
    /// <param name="row">候选行。</param>
    /// <param name="query">投放解析入参。</param>
    /// <returns>作用域评分。</returns>
    private static int CalculateScore(TenantGameAppFeatureSwitchEntity row, FeatureSwitchDeliveryQuery query)
    {
        var score = 0;

        if (row.AppId.HasValue && row.AppId.Value > 0 && row.AppId.Value == query.AppId.GetValueOrDefault())
        {
            score += AppScopeScore;
        }

        if (query.Platform != null && string.Equals(row.Platform, query.Platform, StringComparison.Ordinal))
        {
            score += PlatformScopeScore;
        }

        if (query.PackageCandidates != null && query.PackageCandidates.Contains(row.Package, StringComparer.Ordinal))
        {
            score += PackageScopeScore;
        }

        if (query.Language != null && string.Equals(row.Language, query.Language, StringComparison.Ordinal))
        {
            score += LanguageScopeScore;
        }

        if (query.Channel != null && string.Equals(row.Channel, query.Channel, StringComparison.Ordinal))
        {
            score += ChannelScopeScore;
        }

        return score;
    }

    /// <summary>
    /// 投放版本号：对有效候选行投影固定业务字段、按 (SwitchKey, Id) 排序后取 canonical JSON 的 MD5（小写 32 位 hex）。
    /// <para>构造保证：任一业务字段变化或行集合增减必变；行序与设备灰度结论不影响。审计列（CreatedTime /
    /// UpdatedName / RowVersion 等）刻意不入投影，避免「只改备注」以外的保存动作造成版本号空转。</para>
    /// </summary>
    /// <param name="effective">有效候选行集合。</param>
    /// <returns>小写 32 位 hex 版本号。</returns>
    private static string BuildVersion(IReadOnlyList<TenantGameAppFeatureSwitchEntity> effective)
    {
        var projection = effective.OrderBy(row => row.SwitchKey, StringComparer.Ordinal)
                                  .ThenBy(row => row.Id)
                                  .Select(row => new
                                  {
                                      row.Id,
                                      row.AppId,
                                      row.SwitchKey,
                                      row.DisplayName,
                                      row.Description,
                                      row.Audience,
                                      row.ValueJson,
                                      row.StartTime,
                                      row.EndTime,
                                      row.Platform,
                                      row.Package,
                                      row.Language,
                                      row.Channel,
                                      row.Priority,
                                      row.RolloutPercent,
                                      row.DeviceWhitelistJson,
                                  })
                                  .ToList();
        var json = JsonSerializer.Serialize(projection);
        return Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(json))).ToLowerInvariant();
    }

    /// <summary>
    /// Unix 秒转 UTC 时刻。
    /// </summary>
    /// <param name="unixSeconds">Unix 秒。</param>
    /// <returns>UTC 时刻。</returns>
    private static DateTime ToUtc(long unixSeconds)
    {
        return DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;
    }

    /// <summary>
    /// 可空 Unix 秒转可空 UTC 时刻。
    /// </summary>
    /// <param name="unixSeconds">Unix 秒；null 表示不限。</param>
    /// <returns>UTC 时刻；null 表示不限。</returns>
    private static DateTime? ToNullableUtc(long? unixSeconds)
    {
        return unixSeconds.HasValue ? ToUtc(unixSeconds.Value) : null;
    }
}
