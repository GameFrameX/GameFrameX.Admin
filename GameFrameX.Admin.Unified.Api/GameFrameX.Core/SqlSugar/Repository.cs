using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// 共享宿主仓储，使用唯一主库连接。
/// </summary>
public class Repository<T> : SqlSugarRepository<T> where T : EntityBase, new()
{
    public Repository(ISqlSugarClient db) : base(db)
    {
    }
}
