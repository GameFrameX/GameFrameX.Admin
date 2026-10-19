using GameFrameX.Entity.Base;
using GameFrameX.Entity.Routing;
using GameFrameX.Foundation.Orm.Attribute;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// 仅供乐观锁并发测试使用的平台表实体。使用独立实体而非 SystemConfigEntity，避免全量测试运行时
/// SqlSugar 静态 EntityInfo 缓存被其他 SystemConfigEntity 测试提前写入未转换表名，导致本类
/// EntityNameService 建表名与 insert 缓存表名不一致而 no such table。
/// </summary>
[PlatformTable]
[SugarTable("optimistic_test_entity")]
internal sealed class OptimisticTestEntity : EntityBasic
{
    public string? Value { get; set; }
}