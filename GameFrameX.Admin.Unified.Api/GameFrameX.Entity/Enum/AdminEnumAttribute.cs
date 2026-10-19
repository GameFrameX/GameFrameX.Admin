namespace GameFrameX.Entity.Enum;

/// <summary>
/// 标记可在 Admin 枚举服务公开的枚举类型。
/// </summary>
/// <remarks>
/// 只有带 <c>[AdminEnum]</c> 的枚举才会出现在 <c>/api/systemEnum/enumTypeList</c> 的类型列表中，
/// 也才允许被 <c>/api/systemEnum/enumDataList</c> 按名称读取、被 <c>/api/systemEnum/enumDataListByField</c>
/// 按实体字段读取。未标记的枚举既不会出现在类型列表中，也不能通过名称或实体字段被绕过读取。
/// 该标记取代历史的 <c>Configuration/Enum.json</c> 程序集白名单：公开边界由代码契约显式声明，
/// 不再依赖配置文件字符串，避免白名单漂移与按名读取绕过。
/// </remarks>
[AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
public sealed class AdminEnumAttribute : Attribute
{
}