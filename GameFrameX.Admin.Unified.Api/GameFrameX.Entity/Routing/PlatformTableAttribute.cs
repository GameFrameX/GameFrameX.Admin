namespace GameFrameX.Entity.Routing;

/// <summary>
/// 标记平台主库私有表（物理前缀 <c>system_</c>）。
/// </summary>
/// <remarks>
/// 归属：主数据库，仅平台与 SuperAdmin 可读写，承载平台身份、租户注册、平台私有配置等平台私有数据。
/// 路由：必须解析到主库，禁止根据连接或用户切换物理表名，禁止 fallback 到租户库或日志库。
/// 边界：与 <see cref="TenantTableAttribute"/>、<see cref="GlobalTableAttribute"/> 互斥，每个受管持久化实体只能标注其中一个。
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PlatformTableAttribute : Attribute
{
}