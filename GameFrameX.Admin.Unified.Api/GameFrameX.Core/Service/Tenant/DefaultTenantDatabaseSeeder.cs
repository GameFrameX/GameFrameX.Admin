using GameFrameX.Entity.Tenant.ObjectStorage;

namespace GameFrameX.Core.Service.Tenant;

public static class DefaultTenantDatabaseSeeder
{
    /// <summary>
    /// 为新租户构建默认启用的本地对象存储配置，保证租户开箱即用、上传不报错。
    /// </summary>
    public static TenantObjectStorageConfigEntity BuildDefaultObjectStorageConfig(long tenantId)
    {
        var createdTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds();
        return new TenantObjectStorageConfigEntity
        {
            Id = SnowFlakeIdHelper.Instance.NextId(),
            TenantId = tenantId,
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
    /// 为新租户在对象存储配置为空时写入默认启用的本地配置，保证租户开箱即用、上传不报错。
    /// </summary>
    public static void SeedDefaultObjectStorageConfigIfEmpty(SqlSugarScopeProvider tenantDb, long tenantId)
    {
        var hasAny = tenantDb.Queryable<TenantObjectStorageConfigEntity>()
                             .Filter(null, true)
                             .Any(u => u.TenantId == tenantId);
        if (hasAny)
        {
            return;
        }

        tenantDb.Insertable(BuildDefaultObjectStorageConfig(tenantId)).ExecuteCommand();
    }

    /// <summary>仅插入缺失的种子行，保留用户修改。</summary>
    internal static void Upsert<T>(SqlSugarScopeProvider tenantDb, IEnumerable<T> data) where T : class, new()
    {
        var items = data.ToList();
        if (items.Count == 0)
        {
            return;
        }

        var entityInfo = tenantDb.EntityMaintenance.GetEntityInfo<T>();
        var primaryKeyColumns = entityInfo.Columns.Where(u => u.IsPrimarykey).Select(u => u.PropertyName).ToArray();
        if (primaryKeyColumns.Length == 0)
        {
            tenantDb.Insertable(items).ExecuteCommand();
            return;
        }

        tenantDb.QueryFilter.ClearAndBackup();
        try
        {
            var storageable = tenantDb.Storageable(items);
            storageable.WhereColumns(primaryKeyColumns);
            var storage = storageable.ToStorage();
            storage.AsInsertable.ExecuteCommand();
        }
        finally
        {
            tenantDb.QueryFilter.Restore();
        }
    }

}
