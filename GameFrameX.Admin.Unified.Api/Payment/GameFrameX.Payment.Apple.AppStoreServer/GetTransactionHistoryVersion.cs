namespace GameFrameX.Payment.Apple.AppStoreServer;

/// <summary>
/// 获取交易历史记录 API 版本枚举
/// </summary>
public enum GetTransactionHistoryVersion
{
    /// <summary>
    /// App Store Server API V1 版本
    /// </summary>
    V1,

    /// <summary>
    /// App Store Server API V2 版本（推荐，功能更完整）
    /// </summary>
    V2,
}