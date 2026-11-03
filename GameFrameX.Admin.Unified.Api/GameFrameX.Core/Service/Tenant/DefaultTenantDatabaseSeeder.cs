using GameFrameX.Entity.Tenant.ObjectStorage;

namespace GameFrameX.Core.Service.Tenant;

public static class DefaultTenantDatabaseSeeder
{
    /// <summary>
    /// 为新装主库构建默认启用的本地对象存储配置。
    /// </summary>
    public static TenantObjectStorageConfigEntity BuildDefaultObjectStorageConfig()
    {
        var createdTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds();
        return new TenantObjectStorageConfigEntity
        {
            Id = SnowFlakeIdHelper.Instance.NextId(),
            Provider = "local",
            PathTemplate = "Upload/{yyyy}/{MM}/{dd}",
            MaxFileSizeBytes = 20971520,
            AllowedContentTypesJson = string.Empty,
            EnableMd5 = true,
            ConfigJson = "{}",
            IsEnabled = true,
            Remark = "默认本地存储",
            CreatedTime = createdTime,
            UpdateCount = 0,
        };
    }

    /// <summary>
    /// 仅在主库对象存储配置为空时写入默认本地配置。
    /// </summary>
    public static void SeedDefaultObjectStorageConfigIfEmpty(SqlSugarScopeProvider database)
    {
        var hasAny = database.Queryable<TenantObjectStorageConfigEntity>()
                             .Filter(null, true)
                             .Any();
        if (hasAny)
        {
            return;
        }

        database.Insertable(BuildDefaultObjectStorageConfig()).ExecuteCommand();
    }

    /// <summary>仅插入缺失的种子行，保留用户修改。</summary>
    internal static void Upsert<T>(SqlSugarScopeProvider database, IEnumerable<T> data) where T : class, new()
    {
        var items = data.ToList();
        if (items.Count == 0)
        {
            return;
        }

        var entityInfo = database.EntityMaintenance.GetEntityInfo<T>();
        var primaryKeyColumns = entityInfo.Columns.Where(u => u.IsPrimarykey).Select(u => u.PropertyName).ToArray();
        if (primaryKeyColumns.Length == 0)
        {
            database.Insertable(items).ExecuteCommand();
            return;
        }

        database.QueryFilter.ClearAndBackup();
        try
        {
            var storageable = database.Storageable(items);
            storageable.WhereColumns(primaryKeyColumns);
            var storage = storageable.ToStorage();
            storage.AsInsertable.ExecuteCommand();
        }
        finally
        {
            database.QueryFilter.Restore();
        }
    }

}
