namespace GameFrameX.Entity.Routing;

/// <summary>
/// 标记业务表（物理表名不带前缀）。
/// </summary>
/// <remarks>
/// 归属：主数据库，承载游戏运营业务数据与管理员身份、权限、业务日志。
/// 路由：必须解析到主库，禁止根据连接或用户切换物理表名。
/// 边界：与 <see cref="PlatformTableAttribute"/>、<see cref="GlobalTableAttribute"/> 互斥，每个受管持久化实体只能标注其中一个。
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class BusinessTableAttribute : Attribute
{
}
