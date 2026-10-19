using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 内容运营 / 奖励模板菜单种子。
/// </summary>
public partial class GlobalMenuSeedData
{
    private const int RewardTemplateSeedTime = 1774228858;

    private static IEnumerable<GlobalMenuEntity> ContentRewardTemplateMenus()
    {
        return new[]
        {
            Menu("route:rewardTemplate", "route:content", "rewardTemplate", "/contentCenter/reward", null, "奖励模板", "message.menu.rewardTemplate", "fa-solid fa-gift", 300, MenuAppScopeEnum.App),
            Menu("route:gameRewardTemplateGroupEntity", "route:rewardTemplate", "gameRewardTemplateGroupEntity", "/app/contentCenter/reward/gameRewardTemplateGroupEntity", "/app/contentCenter/reward/gameRewardTemplateGroupEntity/index", "奖励模板分组", "message.menu.gameRewardTemplateGroupEntity", "fa-solid fa-folder-open", 100, MenuAppScopeEnum.App),
            Menu("route:gameRewardTemplateEntity", "route:rewardTemplate", "gameRewardTemplateEntity", "/app/contentCenter/reward/gameRewardTemplateEntity", "/app/contentCenter/reward/gameRewardTemplateEntity/index", "奖励模板", "message.menu.gameRewardTemplateEntity", "fa-solid fa-gift", 200, MenuAppScopeEnum.App),
            Button("gameRewardTemplateGroupEntity:page", "route:gameRewardTemplateGroupEntity", "tenantGameRewardTemplateGroupEntity:page", "查询", 100),
            Button("gameRewardTemplateGroupEntity:detail", "route:gameRewardTemplateGroupEntity", "tenantGameRewardTemplateGroupEntity:detail", "详情", 110),
            Button("gameRewardTemplateGroupEntity:Select", "route:gameRewardTemplateGroupEntity", "tenantGameRewardTemplateGroupEntity:Select", "选择", 115),
            Button("gameRewardTemplateGroupEntity:add", "route:gameRewardTemplateGroupEntity", "tenantGameRewardTemplateGroupEntity:add", "新增", 120),
            Button("gameRewardTemplateGroupEntity:update", "route:gameRewardTemplateGroupEntity", "tenantGameRewardTemplateGroupEntity:update", "编辑", 130),
            Button("gameRewardTemplateGroupEntity:delete", "route:gameRewardTemplateGroupEntity", "tenantGameRewardTemplateGroupEntity:delete", "删除", 140),
            Button("gameRewardTemplateEntity:page", "route:gameRewardTemplateEntity", "tenantGameRewardTemplateEntity:page", "查询", 100),
            Button("gameRewardTemplateEntity:detail", "route:gameRewardTemplateEntity", "tenantGameRewardTemplateEntity:detail", "详情", 110),
            Button("gameRewardTemplateEntity:Select", "route:gameRewardTemplateEntity", "tenantGameRewardTemplateEntity:Select", "选择", 115),
            Button("gameRewardTemplateEntity:CopySnapshot", "route:gameRewardTemplateEntity", "tenantGameRewardTemplateEntity:CopySnapshot", "复制快照", 118),
            Button("gameRewardTemplateEntity:add", "route:gameRewardTemplateEntity", "tenantGameRewardTemplateEntity:add", "新增", 120),
            Button("gameRewardTemplateEntity:update", "route:gameRewardTemplateEntity", "tenantGameRewardTemplateEntity:update", "编辑", 130),
            Button("gameRewardTemplateEntity:delete", "route:gameRewardTemplateEntity", "tenantGameRewardTemplateEntity:delete", "删除", 140),
        };
    }

    private static GlobalMenuEntity Menu(string key, string parentKey, string name, string path, string? component, string title, string titleCode, string icon, int orderNo, MenuAppScopeEnum appScope)
    {
        return new GlobalMenuEntity
        {
            Id = StableSeedId.Menu(key),
            Key = key,
            Pid = StableSeedId.Menu(parentKey),
            Type = component == null ? MenuTypeEnum.Dir : MenuTypeEnum.Menu,
            Name = name,
            Path = path,
            AppScope = appScope,
            Component = component,
            Redirect = null,
            Permission = null,
            Title = title,
            TitleCode = titleCode,
            Icon = icon,
            OutLink = null,
            IsIframe = false,
            IsHide = false,
            IsKeepAlive = true,
            IsAffix = false,
            OrderNo = orderNo,
            IsEnabled = true,
            Remark = null,
            CreatedTime = RewardTemplateSeedTime,
            UpdateTime = RewardTemplateSeedTime,
            SyncRoles = (int)(TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager | TenantMenuSyncRole.GmSupport | TenantMenuSyncRole.ContentOps | TenantMenuSyncRole.MarketingOps | TenantMenuSyncRole.DataAnalyst),
            SuperAdminOnly = false,
            AllowTenantWhenSuperAdminOnly = false,
            IsDeleted = false,
        };
    }

    private static GlobalMenuEntity Button(string key, string parentKey, string permission, string title, int orderNo)
    {
        return new GlobalMenuEntity
        {
            Id = StableSeedId.Menu(key),
            Key = key,
            Pid = StableSeedId.Menu(parentKey),
            Type = MenuTypeEnum.Button,
            Name = null,
            Path = null,
            Component = null,
            Redirect = null,
            Permission = permission,
            Title = title,
            TitleCode = null,
            Icon = null,
            OutLink = null,
            IsIframe = false,
            IsHide = false,
            IsKeepAlive = true,
            IsAffix = false,
            OrderNo = orderNo,
            IsEnabled = true,
            Remark = null,
            CreatedTime = RewardTemplateSeedTime,
            UpdateTime = RewardTemplateSeedTime,
            SyncRoles = (int)(TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager | TenantMenuSyncRole.GmSupport | TenantMenuSyncRole.ContentOps | TenantMenuSyncRole.MarketingOps | TenantMenuSyncRole.DataAnalyst),
            SuperAdminOnly = false,
            AllowTenantWhenSuperAdminOnly = false,
            IsDeleted = false,
        };
    }
}