using System.Text.Json;
using GameFrameX.Core.Extension;
using GameFrameX.Core.Extension.Json;

namespace GameFrameX.Core.Utility;

/// <summary>
/// 提供 Admin Host 统一使用的 JSON 序列化选项。
/// </summary>
public static class JsonSerializerOptionsProvider
{
    /// <summary>
    /// 配置保持既有 Admin JSON 契约的 System.Text.Json 选项。
    /// </summary>
    /// <param name="options">待配置的序列化选项。</param>
    public static void Configure(JsonSerializerOptions options)
    {
        ProjectJsonSerializerOptions.Configure(options);
    }
}