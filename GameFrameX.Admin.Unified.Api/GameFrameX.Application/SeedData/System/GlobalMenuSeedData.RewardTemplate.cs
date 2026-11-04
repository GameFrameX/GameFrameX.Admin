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
            Menu("route:rewardTemplate", "route:content", "rewardTemplate", "/contentCenter/reward", null, "奖励模板", "message.menu.rewardTemplate", "fa-solid fa-gift", 300, MenuAppScopeEnum.Console),
            Menu("route:gameRewardTemplateGroupEntity", "route:rewardTemplate", "gameRewardTemplateGroupEntity", "/app/contentCenter/reward/gameRewardTemplateGroupEntity", "/app/contentCenter/reward/gameRewardTemplateGroupEntity/index", "奖励模板分组", "message.menu.gameRewardTemplateGroupEntity", "fa-solid fa-folder-open", 100, MenuAppScopeEnum.Console),
            Menu("route:gameRewardTemplateEntity", "route:rewardTemplate", "gameRewardTemplateEntity", "/app/contentCenter/reward/gameRewardTemplateEntity", "/app/contentCenter/reward/gameRewardTemplateEntity/index", "奖励模板", "message.menu.gameRewardTemplateEntity", "fa-solid fa-gift", 200, MenuAppScopeEnum.Console),
            Button("gameRewardTemplateGroupEntity:page", "route:gameRewardTemplateGroupEntity", "gameRewardTemplateGroup:page", "查询", 100),
            Button("gameRewardTemplateGroupEntity:detail", "route:gameRewardTemplateGroupEntity", "gameRewardTemplateGroup:detail", "详情", 110),
            Button("gameRewardTemplateGroupEntity:Select", "route:gameRewardTemplateGroupEntity", "gameRewardTemplateGroup:Select", "选择", 115),
            Button("gameRewardTemplateGroupEntity:add", "route:gameRewardTemplateGroupEntity", "gameRewardTemplateGroup:add", "新增", 120),
            Button("gameRewardTemplateGroupEntity:update", "route:gameRewardTemplateGroupEntity", "gameRewardTemplateGroup:update", "编辑", 130),
            Button("gameRewardTemplateGroupEntity:delete", "route:gameRewardTemplateGroupEntity", "gameRewardTemplateGroup:delete", "删除", 140),
            Button("gameRewardTemplateEntity:page", "route:gameRewardTemplateEntity", "gameRewardTemplate:page", "查询", 100),
            Button("gameRewardTemplateEntity:detail", "route:gameRewardTemplateEntity", "gameRewardTemplate:detail", "详情", 110),
            Button("gameRewardTemplateEntity:Select", "route:gameRewardTemplateEntity", "gameRewardTemplate:Select", "选择", 115),
            Button("gameRewardTemplateEntity:CopySnapshot", "route:gameRewardTemplateEntity", "gameRewardTemplate:CopySnapshot", "复制快照", 118),
            Button("gameRewardTemplateEntity:add", "route:gameRewardTemplateEntity", "gameRewardTemplate:add", "新增", 120),
            Button("gameRewardTemplateEntity:update", "route:gameRewardTemplateEntity", "gameRewardTemplate:update", "编辑", 130),
            Button("gameRewardTemplateEntity:delete", "route:gameRewardTemplateEntity", "gameRewardTemplate:delete", "删除", 140),
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
            SuperAdminOnly = false,
            AllowTenantWhenSuperAdminOnly = false,
            IsDeleted = false,
        };
    }
}