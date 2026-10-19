using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Entity.Global;
using SqlSugar;

namespace GameFrameX.Core.Tests.System;

internal static class MenuSeedCatalogTestData
{
    public static void LoadSampleMenus()
    {
        ResetMenuSeedCatalogDbCache();
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_menu_seed_{Guid.NewGuid():N}.db");
        try
        {
            using var db = new SqlSugarClient(new ConnectionConfig
            {
                DbType = DbType.Sqlite,
                ConnectionString = $"DataSource={dbPath}",
                InitKeyType = InitKeyType.Attribute,
                IsAutoCloseConnection = true,
            });
            db.CodeFirst.InitTables<GlobalMenuEntity>();
            db.Insertable(BuildSampleMenus()).ExecuteCommand();
            MenuSeedCatalog.LoadFromDb(db);
        }
        finally
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }

    internal static GlobalMenuEntity[] BuildSampleMenus()
    {
        var menus = GlobalMenuSeedData.BuildData().Select(menu => new GlobalMenuEntity
        {
            Id = menu.Id,
            Pid = menu.Pid,
            Type = menu.Type,
            Key = menu.Key,
            Name = menu.Name ?? string.Empty,
            Path = menu.Path ?? string.Empty,
            Component = menu.Component ?? string.Empty,
            Redirect = menu.Redirect ?? string.Empty,
            Permission = menu.Permission ?? string.Empty,
            Title = menu.Title,
            TitleCode = menu.TitleCode ?? string.Empty,
            Icon = menu.Icon ?? string.Empty,
            IsIframe = menu.IsIframe,
            OutLink = menu.OutLink ?? string.Empty,
            IsHide = menu.IsHide,
            IsKeepAlive = menu.IsKeepAlive,
            IsAffix = menu.IsAffix,
            OrderNo = menu.OrderNo,
            Remark = menu.Remark ?? string.Empty,
            SuperAdminOnly = menu.SuperAdminOnly,
            SyncRoles = menu.SyncRoles,
            AllowTenantWhenSuperAdminOnly = menu.AllowTenantWhenSuperAdminOnly,
            IsEnabled = menu.IsEnabled,
            IsDeleted = false,
            TenantId = 0,
            CreatedId = 0,
            CreatedName = "test",
            CreatedTime = menu.CreatedTime,
            UpdatedId = 0,
            UpdatedName = "test",
            UpdateTime = menu.UpdateTime ?? menu.CreatedTime,
            UpdateCount = 0,
            DeletedId = 0,
            DeletedName = string.Empty,
            DeleteTime = 0,
        }).ToArray();

        return menus;
    }

    private static void ResetMenuSeedCatalogDbCache()
    {
        var flags = global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static;
        typeof(MenuSeedCatalog).GetField("_dbMenus", flags)!.SetValue(null, null);
        typeof(MenuSeedCatalog).GetField("_dbDefinitions", flags)!.SetValue(null, null);
        typeof(MenuSeedCatalog).GetField("_dbDefinitionsByKey", flags)!.SetValue(null, null);
    }
}