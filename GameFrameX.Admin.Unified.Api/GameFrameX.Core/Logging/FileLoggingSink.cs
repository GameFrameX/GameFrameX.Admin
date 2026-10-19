using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace GameFrameX.Core.Logging;

/// <summary>
/// 文件日志 Sink：按注册期配置（级别过滤 + 文件名规则）把 <see cref="LogMessage"/> 以 JSON 行追加到
/// <c>data/logs/{yyyyMMdd}_{Level}.log</c>。C363 恢复文件日志（历史框架退役后 data/logs 不再生成）。
/// <para>行内容：<see cref="ProjectLoggingOptions.MessageFormatRule"/> 命中（如 loggingMonitor JSON）优先；
/// 未命中回退 <see cref="ProjectLoggingOptions.MessageFormat"/> 指定的整条 LogMessage JSON。写入失败按
/// <see cref="ProjectLoggingOptions.HandleWriteError"/> 启用回退文件，仍失败则吞并——日志 I/O 永不影响业务线程。</para>
/// <para>ponytail: 单文件无大小滚动（旧 Logging.json 的 FileSizeLimitBytes/MaxRollingFiles 暂未恢复，按天分文件已满足验收）；
/// 升级路径是写入前检查文件长度并轮转序号后缀。</para>
/// </summary>
internal sealed class FileLoggingSink
{
    private readonly ProjectLoggingOptions _options;
    private readonly string _contentRootPath;

    public FileLoggingSink(ProjectLoggingOptions options, string contentRootPath)
    {
        _options = options;
        _contentRootPath = contentRootPath;
    }

    /// <summary>
    /// 注册期配置（级别过滤 WriteFilter / 文件名规则 / 行格式规则），供调度器分发时读取。
    /// </summary>
    public ProjectLoggingOptions Options => _options;

    /// <summary>
    /// 按 Sink 过滤语义写入一批日志消息。调用方保证异常隔离，此处再兜底 HandleWriteError 回退。
    /// </summary>
    /// <param name="messages">调度器分发的消息批次（已过 WriteFilter）。</param>
    public async Task WriteAsync(IReadOnlyList<LogMessage> messages)
    {
        if (messages.Count == 0)
        {
            return;
        }

        var lines = new StringBuilder();
        foreach (var message in messages)
        {
            var line = FormatLine(message);
            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.AppendLine(line);
            }
        }

        if (lines.Length == 0)
        {
            return;
        }

        await WriteLinesWithFallbackAsync(lines);
    }

    private string FormatLine(LogMessage message)
    {
        var customLine = _options.MessageFormatRule?.Invoke(message);
        if (!string.IsNullOrWhiteSpace(customLine))
        {
            return customLine!;
        }

        return SerializeMessage(message, _options.MessageFormat == LoggerFormatter.JsonIndented);
    }

    private async Task WriteLinesWithFallbackAsync(StringBuilder lines)
    {
        var fileName = ResolveFileName();
        try
        {
            await WriteFileAsync(fileName, lines);
            return;
        }
        catch (Exception writeError)
        {
            // 写入失败（磁盘/占用等）：按注册期 HandleWriteError 决定回退文件后重试一次
            var writeErrorReport = new ProjectLoggingWriteError { CurrentFileName = fileName, };
            try
            {
                _options.HandleWriteError?.Invoke(writeErrorReport);
            }
            catch
            {
                // 回退配置自身异常时忽略，直接放弃本批文件写入
                return;
            }

            if (string.Equals(writeErrorReport.CurrentFileName, fileName, StringComparison.Ordinal))
            {
                return;
            }

            try
            {
                await WriteFileAsync(writeErrorReport.CurrentFileName, lines);
            }
            catch
            {
                // 回退文件仍失败：放弃本批，不让日志 I/O 异常传播到调度器外
            }
        }
    }

    private string ResolveFileName()
    {
        // FileNameRule 由注册期闭包提供（LoggingSetup 按「当天 + 本 Sink 级别」格式化模板）
        var fileName = _options.FileNameRule != null ? _options.FileNameRule(_options.FileName) : _options.FileName;
        return Path.IsPathRooted(fileName) ? fileName : Path.Combine(_contentRootPath, fileName);
    }

    private static async Task WriteFileAsync(string fileName, StringBuilder lines)
    {
        var directory = Path.GetDirectoryName(fileName);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.AppendAllTextAsync(fileName, lines.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// 整条 LogMessage 序列化为 JSON 行（回退格式；Monitor 消息由 MessageFormatRule 直接输出 loggingMonitor JSON）。
    /// </summary>
    private static string SerializeMessage(LogMessage message, bool indented)
    {
        return JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = indented,
        });
    }
}
