using MiniExcelLibs;
using MiniExcelLibs.Attributes;

namespace GameFrameX.Application.System.Logging.Dto;

/// <summary>
/// 导出日志数据
/// </summary>
public class ExportLogResponse
{
    [ExcelColumn(Name = "记录器类别名称")] public string LogName { get; set; } = string.Empty;

    [ExcelColumn(Name = "日志级别")] public string LogLevel { get; set; } = string.Empty;

    [ExcelColumn(Name = "事件Id")] public string EventId { get; set; } = string.Empty;

    [ExcelColumn(Name = "日志消息")] public string Message { get; set; } = string.Empty;

    [ExcelColumn(Name = "异常对象")] public string Exception { get; set; } = string.Empty;

    [ExcelColumn(Name = "当前状态值")] public string State { get; set; } = string.Empty;

    [ExcelIgnore] public long? LogDateTime { get; set; }

    /// <summary>
    /// 日志记录时间（本地时间字符串，供 Excel 导出展示）。
    /// </summary>
    [ExcelColumn(Name = "日志记录时间")]
    public string LogDateTimeText
    {
        get { return TimerHelper.TimestampSecondsToDateTime(LogDateTime, true) is { } value ? value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : string.Empty; }
    }

    [ExcelColumn(Name = "线程Id")] public int ThreadId { get; set; }

    [ExcelColumn(Name = "请求跟踪Id")] public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// 将日志数据导出为 Excel 文件结果
    /// </summary>
    public static async Task<IActionResult> ExportAsExcelAsync(List<ExportLogResponse> logList, string fileNameSuffix)
    {
        var stream = new MemoryStream();
        await stream.SaveAsAsync(logList, excelType: ExcelType.XLSX);
        stream.Position = 0;
        return new FileStreamResult(stream, "application/octet-stream")
        {
            FileDownloadName = DateTime.Now.ToString("yyyyMMddHHmm") + fileNameSuffix + ".xlsx",
        };
    }
}