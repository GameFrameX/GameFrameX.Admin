using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GameFrameX.Application.Game.App.Rollout;

/// <summary>
/// 记录级设备灰度判定原语（取模 + 白名单旁路）的唯一实现。
/// <para>被远程配置投放（Client.Api <c>RemoteConfigRolloutMatcher</c>）与活动/功能开关投放
/// （<c>FeatureSwitchDeliveryResolver</c>）共用，保证两类投放的灰度语义逐位一致。</para>
/// <para>判定顺序：白名单命中直接通过 → 比例 &gt;= 100 全量通过 → 比例 &lt;= 0 或设备标识缺失不通过 →
/// 稳定取模桶号小于比例时通过。取模材料含记录主键，因此同一设备在不同记录上的命中互不相关。</para>
/// </summary>
public static class StableDeviceRolloutMatcher
{
    /// <summary>
    /// 判断记录是否命中当前设备的灰度。
    /// </summary>
    /// <param name="tenantId">当前租户 ID，参与稳定取模材料，避免跨租户同设备命中一致。</param>
    /// <param name="appId">记录的应用作用域，参与稳定取模材料。</param>
    /// <param name="recordId">记录主键，参与稳定取模材料，使同一设备对不同记录独立判定。</param>
    /// <param name="deviceId">客户端稳定设备标识；缺失时只允许命中全量投放。</param>
    /// <param name="rolloutPercent">灰度比例 0-100，越界值按边界收敛。</param>
    /// <param name="deviceWhitelistJson">设备白名单 JSON 数组；命中白名单绕过灰度比例。</param>
    /// <returns>命中返回 true，否则返回 false。</returns>
    public static bool IsMatch(
        long tenantId,
        long? appId,
        long recordId,
        string? deviceId,
        int rolloutPercent,
        string? deviceWhitelistJson)
    {
        var normalizedDeviceId = NormalizeDeviceId(deviceId);
        if (!string.IsNullOrEmpty(normalizedDeviceId) && IsWhitelisted(deviceWhitelistJson, normalizedDeviceId))
        {
            return true;
        }

        var percent = Math.Clamp(rolloutPercent, 0, 100);
        if (percent >= 100)
        {
            return true;
        }

        if (percent <= 0 || string.IsNullOrEmpty(normalizedDeviceId))
        {
            return false;
        }

        var bucket = GetStableBucket(tenantId, appId, normalizedDeviceId, recordId);
        return bucket < percent;
    }

    /// <summary>
    /// 判断设备标识是否命中白名单 JSON 数组（标识比较前统一去空白并转小写）。
    /// </summary>
    /// <param name="whitelistJson">设备白名单 JSON 数组原文；非数组或非法 JSON 一律视为未命中。</param>
    /// <param name="normalizedDeviceId">已归一化的设备标识。</param>
    /// <returns>命中返回 true，否则返回 false。</returns>
    private static bool IsWhitelisted(string? whitelistJson, string normalizedDeviceId)
    {
        if (string.IsNullOrWhiteSpace(whitelistJson))
        {
            return false;
        }

        try
        {
            using (var doc = JsonDocument.Parse(whitelistJson))
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                {
                    return false;
                }

                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String &&
                        string.Equals(NormalizeDeviceId(item.GetString()), normalizedDeviceId, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }
        }
        catch (JsonException)
        {
            return false;
        }

        return false;
    }

    /// <summary>
    /// 按「租户 + 应用 + 设备标识 + 记录主键」的 SHA-256 取前 4 字节生成 0-99 稳定桶号。
    /// <para>同一组合恒定得同一桶号，故设备在同一记录上的灰度结论稳定，不随请求次数抖动。</para>
    /// </summary>
    /// <param name="tenantId">当前租户 ID。</param>
    /// <param name="appId">记录的应用作用域。</param>
    /// <param name="normalizedDeviceId">已归一化的设备标识。</param>
    /// <param name="recordId">记录主键。</param>
    /// <returns>0-99 的稳定桶号。</returns>
    private static int GetStableBucket(long tenantId, long? appId, string normalizedDeviceId, long recordId)
    {
        var material = $"{tenantId}:{appId.GetValueOrDefault()}:{normalizedDeviceId}:{recordId}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        var value = BitConverter.ToUInt32(bytes, 0);
        return (int)(value % 100);
    }

    /// <summary>
    /// 归一化设备标识（去首尾空白 + 转小写），保证大小写与空白差异不改变灰度结论。
    /// </summary>
    /// <param name="deviceId">原始设备标识。</param>
    /// <returns>归一化后的设备标识；空值返回空字符串。</returns>
    private static string NormalizeDeviceId(string? deviceId)
    {
        return deviceId?.Trim().ToLowerInvariant() ?? string.Empty;
    }
}
