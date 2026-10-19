using GameFrameX.Foundation.Orm.Entity;
using GameFrameX.Foundation.Utility;
using SqlSugar;
using GameFrameX.Foundation.Utility.DistributedSystem.Snowflake;

namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// SqlSugar AOP 数据审计辅助类
/// </summary>
public static class SqlSugarAopHelper
{
    /// <summary>
    /// 处理基础数据审计：雪花ID、创建时间、更新时间
    /// </summary>
    /// <param name="oldValue">旧值</param>
    /// <param name="entityInfo">实体信息</param>
    public static void HandleBaseAudit(object oldValue, DataFilterModel entityInfo)
    {
        if (entityInfo.OperationType == DataFilterType.InsertByObject)
        {
            // 主键(long类型)且没有值的---赋值雪花Id
            if (entityInfo.EntityColumnInfo.IsPrimarykey && entityInfo.EntityColumnInfo.PropertyInfo.PropertyType == typeof(long))
            {
                var id = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);
                if (id == null || (long)id == 0)
                {
                    entityInfo.SetValue(SnowFlakeIdHelper.Instance.NextId());
                }
            }

            if (entityInfo.PropertyName == nameof(EntityBase.CreatedTime))
            {
                entityInfo.SetValue(TimerHelper.UnixTimeSeconds());
            }
        }

        if (entityInfo.OperationType == DataFilterType.UpdateByObject)
        {
            if (entityInfo.PropertyName == nameof(EntityBase.UpdateTime))
            {
                entityInfo.SetValue(TimerHelper.UnixTimeSeconds());
            }
        }
    }
}