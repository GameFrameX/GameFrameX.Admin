// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Client.Api.GrafanaLoki;

/// <summary>
/// 日志存储接口
/// </summary>
public interface ILogGrafanaLokiStorage
{
    /// <summary>
    /// 保存日志
    /// </summary>
    /// <param name="gameAnalyticsEntry"></param>
    void SaveLog(GrafanaLokiAnalyticsEntry gameAnalyticsEntry);

    /// <summary>
    /// 加载待处理的日志
    /// </summary>
    /// <param name="maxBatchSize"></param>
    /// <returns>日志条目列表</returns>
    List<GrafanaLokiAnalyticsEntry> LoadPendingLogs(int maxBatchSize);

    /// <summary>
    /// 清除已处理的日志
    /// </summary>
    /// <param name="ids">已处理的日志条目Id列表</param>
    void ClearProcessedLogs(List<string> ids);

    /// <summary>
    /// 清除所有日志
    /// </summary>
    void ClearAllLogs();
}