using GameFrameX.Core.Enum;
using GameFrameX.Core.Const;
using GameFrameX.Core.Option;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 系统租户表种子数据
/// </summary>
public class SystemTenantSeedData : ISqlSugarEntitySeedData<SystemTenantEntity>
{
    private static DbConnectionOptions? DbConnectionOptions;

    /// <summary>
    /// 绑定数据库配置；种子数据由 SqlSugar 反射无参创建，配置需在启动期显式传入。
    /// </summary>
    public static void Configure(DbConnectionOptions dbConnectionOptions)
    {
        DbConnectionOptions = dbConnectionOptions ?? throw new ArgumentNullException(nameof(dbConnectionOptions));
    }

    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SystemTenantEntity> HasData()
    {
        var defaultTenantConfigId = TenantDefaultDataTemplates.MainTenantId.ToString();
        var defaultTenantDbConfig = (DbConnectionOptions ?? throw new InvalidOperationException("SystemTenantSeedData 尚未配置数据库连接。")).ConnectionConfigs
                                                                                                                                   .FirstOrDefault(u => u.ConfigId?.ToString() == defaultTenantConfigId);
        if (defaultTenantDbConfig == null || string.IsNullOrWhiteSpace(defaultTenantDbConfig.ConnectionString))
        {
            throw new InvalidOperationException($"默认租户库连接配置缺失，请在 DbConnection.ConnectionConfigs 中配置 ConfigId={defaultTenantConfigId}。");
        }

        return new[]
        {
            new SystemTenantEntity { Id = TenantDefaultDataTemplates.MainTenantId, OrganizationId = 1300000000101, UserId = 1300000000111, Host = "alianblank.com", TenantType = TenantTypeEnum.Db, DbType = defaultTenantDbConfig.DbType, Connection = defaultTenantDbConfig.ConnectionString, ConfigId = defaultTenantConfigId, Remark = "系统默认租户", PackageId = TenantDefaultDataTemplates.EnterprisePackageId, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
        };
    }
}