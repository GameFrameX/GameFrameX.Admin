using GameFrameX.Entity.Tenant.ClientApiConfig;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.RemoteConfig;
using GameFrameX.Entity.Tenant.Game.Release;
using GameFrameX.Entity.Tenant.Game.FeatureSwitch;
using SqlSugar;

namespace GameFrameX.DatabaseMigration;

internal static class ConfigurationCollisionGuard
{
    private static readonly (Type EntityType, string[] Keys, bool UniqueTarget)[] Rules =
    {
        (typeof(TenantClientApiPolicyEntity), Array.Empty<string>(), true),
        (typeof(TenantAdvertisementCallbackConfigEntity), new[] { "Provider" }, true),
        (typeof(TenantEmailConfigEntity), new[] { "Purpose" }, false),
        (typeof(TenantGamePaymentConfigEntity), new[] { "PackageIdNormalized", "PayChannel", "IsSandbox" }, true),
        (typeof(TenantSessionKeyEntity), new[] { "Kid" }, true),
        (typeof(TenantAppSignatureKeyEntity), new[] { "Kid" }, true),
        (typeof(TenantGameLoginTicketKeyEntity), new[] { "Kid" }, true),
        (typeof(TenantGameAppRemoteConfigEntity), new[] { "Channel", "Platform", "AppVersion", "Package", "Language" }, false),
        (typeof(TenantGameAppFeatureSwitchEntity), new[] { "SwitchKey", "Audience", "Channel", "Platform", "Package", "Language" }, false),
        (typeof(TenantGameAppReleaseVersionEntity), new[] { "Channel", "Platform", "OldVersion", "AppVersion", "Package", "Language" }, true),
        (typeof(TenantGameAppAssetPackageVersionEntity), new[] { "Channel", "Platform", "AppVersion", "Package", "Language", "AssetPackageName" }, true),
    };

    internal static void Validate(ISqlSugarClient database, bool legacyScope)
    {
        foreach (var rule in Rules)
        {
            if (!legacyScope && !rule.UniqueTarget)
            {
                continue;
            }

            var entity = database.EntityMaintenance.GetEntityInfo(rule.EntityType);
            if (!database.DbMaintenance.IsAnyTable(entity.DbTableName, false))
            {
                continue;
            }

            var columns = database.DbMaintenance.GetColumnInfosByTableName(entity.DbTableName, false)
                .Select(column => column.DbColumnName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var scopes = new[] { "app_id", "game_app_id", "game_app_id_normalized" }.Where(columns.Contains).ToArray();
            if (legacyScope && scopes.Length == 0)
            {
                continue;
            }

            var keys = rule.Keys.Select(property => entity.Columns.Single(column => column.PropertyName == property).DbColumnName).ToArray();
            if (keys.Any(key => !columns.Contains(key)))
            {
                throw new InvalidOperationException($"表 {entity.DbTableName} 缺少业务键列，无法安全验证配置碰撞。");
            }

            var keySql = keys.Length == 0 ? "'single' AS configuration_key" : string.Join(", ", keys.Select(Quote));
            var grouping = keys.Length == 0 ? "configuration_key" : string.Join(", ", keys.Select(Quote));
            var predicate = columns.Contains("is_deleted") ? " WHERE is_deleted = false" : string.Empty;
            var rows = $"SELECT {keySql} FROM {Quote(entity.DbTableName)}{predicate}";
            if (legacyScope)
            {
                // 同业务键的默认层与应用层不能按 Id 或优先级擅自选胜者。
                rows += $" GROUP BY {grouping}, {string.Join(", ", scopes.Select(Quote))}";
            }

            var collisions = database.Ado.GetInt($"SELECT COUNT(*) FROM (SELECT {grouping} FROM ({rows}) candidates GROUP BY {grouping} HAVING COUNT(*) > 1) conflicts");
            if (collisions > 0)
            {
                throw new InvalidOperationException($"表 {entity.DbTableName} 存在配置作用域碰撞；请先人工确认并清理重复业务配置，迁移不会自动选取或覆盖。");
            }
        }
    }

    private static string Quote(string identifier)
    {
        return "\"" + identifier.Replace("\"", "\"\"") + "\"";
    }
}
