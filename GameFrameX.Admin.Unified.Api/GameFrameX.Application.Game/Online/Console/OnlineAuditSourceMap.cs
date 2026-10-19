using System.Text.Json;
using GameFrameX.Application.Game.Online.Console.Dto;
using GameFrameX.Entity.Log;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.Online;

namespace GameFrameX.Application.Game.Online.Console;

/// <summary>
/// 统一审计三来源 → 五维统一字段映射的唯一来源（change C344 / vault:C9 S8.2）。
/// <para>
/// 三来源口径（Admin 侧零事件镜像，全部实时读取原表/透传 Online）：
/// <list type="number">
/// <item><description><b>AdminOperation</b>：<see cref="TenantLogOperationEntity"/>（租户操作日志，
/// MonitorLogging 全 action 自动落库）——操作者取账号，目标对象/作用域从 <c>RequestParam</c> JSON 尽力提取
/// （提取不到即 null，不伪造）；命令类型取 Controller.Action；时间取 LogDateTime。</description></item>
/// <item><description><b>OnlineAuditEvent</b>：Online 审计事件（经 <c>IOnlineServerClient</c> 实时透传）——五维直映。</description></item>
/// <item><description><b>DomainCommandRecord</b>：各域命令事实源表——处罚落账（<see cref="TenantGamePenaltyEntity"/>，C350）
/// 与配置灰度发布记录（<see cref="TenantGameOnlineConfigRolloutEntity"/>，C343）；资产人工发放/撤销的审计在 Online 侧，
/// 由 OnlineAuditEvent 来源覆盖（本地无镜像表）。</description></item>
/// </list>
/// 禁止在 Service 内散落第二套映射——新增来源必须回到本文件登记并同步守护说明。
/// </para>
/// </summary>
public static class OnlineAuditSourceMap
{
    /// <summary>
    /// 操作日志 RequestParam JSON 尽力提取结果（目标标识 + 作用域；提取不到的字段为 null）。
    /// </summary>
    private sealed class RequestJsonProbe
    {
        public string? TargetIdentifier { get; set; }

        public long? AppId { get; set; }

        public long? ServerId { get; set; }
    }

    /// <summary>
    /// RequestParam JSON 中探测目标对象标识的已知键（camelCase / PascalCase 双形态；深度 ≤2 层，覆盖 {"input":{...}} 包裹）。
    /// </summary>
    private static readonly string[] TargetIdentifierKeys =
    {
        "playerId", "PlayerId", "targetId", "TargetId", "targetPlayerId", "TargetPlayerId",
        "penaltyId", "PenaltyId", "ticketId", "TicketId", "matchId", "MatchId",
    };

    /// <summary>
    /// RequestParam JSON 中探测作用域的已知键（appId/serverId 双形态）。
    /// </summary>
    private static readonly string[] ScopeKeys = { "appId", "AppId", "serverId", "ServerId", };

    /// <summary>
    /// Admin 操作日志行 → 统一审计行。
    /// </summary>
    public static OnlineAuditSearchEntryResponse FromAdminOperation(TenantLogOperationEntity row)
    {
        var probe = ExtractIdentifiersFromRequestJson(row.RequestParam);
        return new OnlineAuditSearchEntryResponse
        {
            Source = OnlineAuditSourceKind.AdminOperation,
            OperatorIdentifier = FirstNonEmpty(row.Account, row.RealName),
            TargetPlayerIdentifier = probe.TargetIdentifier,
            AppId = probe.AppId,
            ServerId = probe.ServerId,
            CommandType = BuildAdminOperationCommandType(row),
            OccurredAt = row.LogDateTime ?? row.CreatedTime,
            Summary = FirstNonEmpty(row.DisplayTitle, row.Message),
            CorrelationId = row.TraceId,
        };
    }

    /// <summary>
    /// Online 审计事件行 → 统一审计行（五维直映，零推断）。
    /// </summary>
    public static OnlineAuditSearchEntryResponse FromOnlineAuditEvent(OnlineAuditEventEntryResponse entry)
    {
        return new OnlineAuditSearchEntryResponse
        {
            Source = OnlineAuditSourceKind.OnlineAuditEvent,
            OperatorIdentifier = entry.OperatorId,
            TargetPlayerIdentifier = entry.TargetPlayerId,
            AppId = null,
            ServerId = null,
            CommandType = entry.CommandType,
            OccurredAt = entry.OccurredAt,
            Summary = entry.Summary,
            CorrelationId = entry.CorrelationId,
        };
    }

    /// <summary>
    /// 处罚落账行（C350 事实源）→ 统一审计行。
    /// </summary>
    public static OnlineAuditSearchEntryResponse FromPenaltyRecord(TenantGamePenaltyEntity penalty)
    {
        return new OnlineAuditSearchEntryResponse
        {
            Source = OnlineAuditSourceKind.DomainCommandRecord,
            OperatorIdentifier = penalty.OperatorId.ToString(),
            TargetPlayerIdentifier = penalty.TargetId,
            AppId = penalty.AppId,
            ServerId = penalty.AreaId,
            CommandType = "penalty." + penalty.ActionType,
            OccurredAt = penalty.CreatedTime,
            Summary = penalty.Reason,
            CorrelationId = null,
        };
    }

    /// <summary>
    /// 配置灰度发布记录行（C343 事实源）→ 统一审计行。
    /// </summary>
    public static OnlineAuditSearchEntryResponse FromConfigRolloutRecord(TenantGameOnlineConfigRolloutEntity rollout)
    {
        return new OnlineAuditSearchEntryResponse
        {
            Source = OnlineAuditSourceKind.DomainCommandRecord,
            OperatorIdentifier = rollout.DispatchOperator,
            TargetPlayerIdentifier = null,
            AppId = rollout.AppId,
            ServerId = rollout.ServerId,
            CommandType = "configRollout." + rollout.ConfigKind,
            OccurredAt = rollout.PublishedTime,
            Summary = $"{rollout.ConfigKey} v{rollout.ConfigVersion} {rollout.Status}",
            CorrelationId = null,
        };
    }

    /// <summary>
    /// 从操作日志 RequestParam JSON 尽力提取目标标识与作用域。
    /// <para>
    /// 「尽力提取」边界：只探测固定键集、只下钻一层对象值（覆盖 {"input":{...}} 参数包裹形态）、
    /// 非字符串值（数字 Id）以原值字符串化；解析失败 / 键不存在一律返回 null——不伪造、不全文正则。
    /// </para>
    /// </summary>
    private static RequestJsonProbe ExtractIdentifiersFromRequestJson(string? requestParameterJson)
    {
        var empty = new RequestJsonProbe();
        if (string.IsNullOrWhiteSpace(requestParameterJson))
        {
            return empty;
        }

        try
        {
            using (var document = JsonDocument.Parse(requestParameterJson))
            {
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                {
                    return empty;
                }

                var target = ProbeString(root, TargetIdentifierKeys);
                if (target == null)
                {
                    // 下钻一层：MonitorLogging 序列化的 action 参数体常为 {"input":{...}} 包裹。
                    target = ProbeStringOneLevelDeep(root, TargetIdentifierKeys);
                }

                return new RequestJsonProbe
                {
                    TargetIdentifier = target,
                    AppId = ProbeInt64Deep(root, ScopeKeys[0], ScopeKeys[1]),
                    ServerId = ProbeInt64Deep(root, ScopeKeys[2], ScopeKeys[3]),
                };
            }
        }
        catch (JsonException)
        {
            // RequestParam 非 JSON（旧格式 / 截断）：提取不到即为 null，不推断。
            return empty;
        }
    }

    /// <summary>
    /// 在 JSON 对象内按键集探测第一个非空字符串值。
    /// </summary>
    private static string? ProbeString(JsonElement container, string[] keys)
    {
        foreach (var key in keys)
        {
            if (container.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
            {
                var text = value.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }
            else if (container.TryGetProperty(key, out var numberValue) && numberValue.ValueKind == JsonValueKind.Number)
            {
                // 数字 Id（如 penaltyId=123）：原值字符串化，保留精确文本。
                var raw = numberValue.GetRawText();
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    return raw;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 下钻一层对象值按键集探测（覆盖 {"input":{...}} 参数包裹形态）。
    /// </summary>
    private static string? ProbeStringOneLevelDeep(JsonElement container, string[] keys)
    {
        foreach (var property in container.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                var probed = ProbeString(property.Value, keys);
                if (probed != null)
                {
                    return probed;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 在 JSON 对象内按键对（camelCase/PascalCase）探测数字值。
    /// </summary>
    private static long? ProbeInt64(JsonElement container, string camelKey, string pascalKey)
    {
        foreach (var key in new[] { camelKey, pascalKey, })
        {
            if (container.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    /// <summary>
    /// 作用域数字键探测：先根层，miss 再下钻一层对象值（appId/serverId 常在 {"input":{...}} 包裹内，与目标键同形态）。
    /// </summary>
    private static long? ProbeInt64Deep(JsonElement container, string camelKey, string pascalKey)
    {
        var probed = ProbeInt64(container, camelKey, pascalKey);
        if (probed.HasValue)
        {
            return probed;
        }

        foreach (var property in container.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                probed = ProbeInt64(property.Value, camelKey, pascalKey);
                if (probed.HasValue)
                {
                    return probed;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Admin 操作日志命令类型：Controller.Action 优先，缺失回退 RequestUrl。
    /// </summary>
    private static string BuildAdminOperationCommandType(TenantLogOperationEntity row)
    {
        var controller = FirstNonEmpty(row.ControllerName, string.Empty);
        var action = FirstNonEmpty(row.ActionName, string.Empty);
        if (controller.Length > 0 && action.Length > 0)
        {
            return controller + "." + action;
        }

        return FirstNonEmpty(row.RequestUrl, controller + action);
    }

    private static string? FirstNonEmpty(string? first, string? second)
    {
        if (!string.IsNullOrWhiteSpace(first))
        {
            return first;
        }

        return string.IsNullOrWhiteSpace(second) ? null : second;
    }
}
