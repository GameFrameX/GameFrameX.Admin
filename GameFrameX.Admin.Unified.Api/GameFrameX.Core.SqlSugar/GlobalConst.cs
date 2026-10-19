namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// 全局常量。
/// </summary>
public static class GlobalConst
{
    /// <summary>
    /// 历史 API 密钥请求头。
    /// </summary>
    public const string ApiKeyHeader = "GameFrameX-Api-Key";

    /// <summary>
    /// 应用标识请求头。
    /// </summary>
    public const string AppIdHeader = "GameFrameX-App-Id";

    /// <summary>
    /// 应用密钥请求头。
    /// </summary>
    public const string AppSecretHeader = "GameFrameX-App-Secret";

    /// <summary>
    /// 租户标识请求头。
    /// </summary>
    public const string TenantIdHeader = "GameFrameX-Tenant-Id";

    /// <summary>
    /// 租户签名密钥请求头。
    /// </summary>
    public const string TenantSecretHeader = "GameFrameX-Tenant-Secret";

    /// <summary>
    /// 请求内容签名请求头。
    /// </summary>
    public const string SignatureHeader = "GameFrameX-Signature";

    /// <summary>
    /// 租户主机请求头。
    /// </summary>
    public const string TenantHostHeader = "GameFrameX-Tenant-Host";

    /// <summary>
    /// SuperAdmin 代管租户请求头。
    /// </summary>
    public const string TargetTenantIdHeader = "GameFrameX-Target-Tenant-Id";

    /// <summary>
    /// HttpContext.Items 中保存已解析租户 ID 的键（TenantResolutionMiddleware 写入，
    /// 共享层中间件读取；C8 T4a 收敛前各 host 中间件各自定义常量，收敛后统一在此）。
    /// </summary>
    public const string TenantIdContextKey = "TenantId";

    /// <summary>
    /// 包标识请求头。
    /// </summary>
    public const string PackageIdHeader = "GameFrameX-Package-Id";

    /// <summary>
    /// 包名称请求头。
    /// </summary>
    public const string PackageNameHeader = "GameFrameX-Package-Name";
}