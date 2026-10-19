using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using GameFrameX.Foundation.Orm.Entity;
using SqlSugar;

namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// SqlSugar 实体元数据配置
/// </summary>
public static class SqlSugarEntityConfig
{
    /// <summary>
    /// 配置实体名称和列的公共服务
    /// </summary>
    /// <param name="extServices">外部服务配置对象</param>
    public static void ConfigureEntityServices(ConfigureExternalServices extServices)
    {
        extServices.EntityNameService = (type, entity) =>
        {
            entity.IsDisabledDelete = true; // 禁止删除非 sqlsugar 创建的列
            // 只处理当前类型直接声明的 [SugarTable] 实体；投影 DTO 可能继承实体字段，但不应参与物理表边界映射。
            if (!type.IsDefined(typeof(SugarTable), false))
            {
                return;
            }

            if (!entity.DbTableName.Contains('_'))
            {
                entity.DbTableName = UtilMethods.ToUnderLine(entity.DbTableName); // 驼峰转下划线
            }

            entity.DbTableName = SqlSugarEntityBoundary.GetPhysicalTableName(type, entity.DbTableName);
        };
        extServices.EntityService = (type, column) =>
        {
            // 处理主键
            if (type.GetCustomAttributes<KeyAttribute>().Any())
            {
                column.IsPrimarykey = true;
            }

            if (column.IsPrimarykey == false)
            {
                // 处理实体属性
                if (type.PropertyType.IsGenericType && type.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    column.IsNullable = true;
                }

                // 处理乐观锁
                var versionType = Nullable.GetUnderlyingType(type.PropertyType) ?? type.PropertyType;
                if (type.Name == nameof(EntityBase.RowVersion) && versionType == typeof(long) && !type.GetCustomAttributes<SugarColumn>().Any())
                {
                    column.IsEnableUpdateVersionValidation = true;
                    // Foundation 2.7.0 将乐观锁版本列由 Version 重命名为 RowVersion；显式固定历史物理列名为 Version，
                    // 再经下方 UtilMethods.ToUnderLine 管线还原为 version，与 2.5.2 的物理列名保持逐字一致。
                    // column.DbColumnName = "Version";
                }

                // 处理实体属性-非主键且可空
                if (new NullabilityInfoContext().Create(type).WriteState is NullabilityState.Nullable)
                {
                    column.IsNullable = true;
                }
            }

            // 处理实体属性-非空设置
            if (type.GetCustomAttributes<RequiredAttribute>().Any())
            {
                column.IsNullable = false;
            }

            // 处理注释
            if (type.GetCustomAttributes<DescriptionAttribute>().Any())
            {
                column.ColumnDescription = type.GetCustomAttributes<DescriptionAttribute>().First().Description;
            }

            if (!column.IsIgnore && !column.DbColumnName.Contains('_'))
            {
                column.DbColumnName = UtilMethods.ToUnderLine(column.DbColumnName); // 驼峰转下划线
            }
        };
    }
}