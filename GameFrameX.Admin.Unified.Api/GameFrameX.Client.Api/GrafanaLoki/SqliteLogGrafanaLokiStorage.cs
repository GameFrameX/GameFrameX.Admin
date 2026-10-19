// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using SQLite;

namespace GameFrameX.Client.Api.GrafanaLoki;

/// <summary>
/// SQLite日志存储
/// </summary>
public class SqliteLogGrafanaLokiStorage : ILogGrafanaLokiStorage
{
    private readonly object _fileLock = new();
    private readonly SQLiteConnection _sqLiteConnection = null!;
    private readonly TableQuery<GrafanaLokiAnalyticsEntry> _tableQuery = null!;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logFilePath">日志文件路径</param>
    public SqliteLogGrafanaLokiStorage(string logFilePath = "grafana_loki.db")
    {
        try
        {
            // 确保路径以应用程序持久化数据路径为基础
            var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFilePath);
            _sqLiteConnection = new SQLiteConnection(logDirectory);
            var tableInfo = _sqLiteConnection.GetTableInfo(nameof(GrafanaLokiAnalyticsEntry));
            if (tableInfo == null || tableInfo.Count == 0)
            {
                _sqLiteConnection.CreateTable<GrafanaLokiAnalyticsEntry>();
            }

            _tableQuery = _sqLiteConnection.Table<GrafanaLokiAnalyticsEntry>();
        }
        catch (Exception e)
        {
            LogHelper.Fatal(e);
        }
    }

    /// <summary>
    /// 保存日志
    /// </summary>
    /// <param name="logs">日志条目列表</param>
    public void SaveLog(GrafanaLokiAnalyticsEntry logs)
    {
        lock (_fileLock)
        {
            try
            {
                _sqLiteConnection.Insert(logs);
            }
            catch (Exception ex)
            {
                LogHelper.Error($"保存日志失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 加载待处理的日志
    /// </summary>
    /// <param name="maxBatchSize"></param>
    /// <returns>日志条目列表</returns>
    public List<GrafanaLokiAnalyticsEntry> LoadPendingLogs(int maxBatchSize)
    {
        lock (_fileLock)
        {
            try
            {
                return _tableQuery.OrderBy(m => m.CreateTime).Take(maxBatchSize).ToList();
            }
            catch (Exception ex)
            {
                LogHelper.Error($"加载日志失败: {ex.Message}");
            }

            return new List<GrafanaLokiAnalyticsEntry>();
        }
    }


    /// <summary>
    /// 清除已处理的日志
    /// </summary>
    /// <param name="ids">已处理的日志条目Id列表</param>
    public void ClearProcessedLogs(List<string> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            return;
        }

        lock (_fileLock)
        {
            try
            {
                foreach (var logId in ids)
                {
                    _sqLiteConnection.Delete<GrafanaLokiAnalyticsEntry>(logId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"清除已处理日志失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 清除所有日志
    /// </summary>
    public void ClearAllLogs()
    {
        lock (_fileLock)
        {
            try
            {
                _sqLiteConnection.DeleteAll<GrafanaLokiAnalyticsEntry>();
            }
            catch (Exception ex)
            {
                LogHelper.Error($"清除所有日志失败: {ex.Message}");
            }
        }
    }
}