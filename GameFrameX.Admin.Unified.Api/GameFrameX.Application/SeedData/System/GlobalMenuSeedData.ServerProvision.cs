using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Application.ServerCenter.GameAreaProvision;
using GameFrameX.Core.Api;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Application.SeedData.System;

public partial class GlobalMenuSeedData
{
    private static IEnumerable<GlobalMenuEntity> ServerProvisionMenus()
    {
        return new GlobalMenuEntity[]
        {
            new()
            {
                Id = StableSeedId.Menu("route:gameAreaProvision"),
                Key = "route:gameAreaProvision",
                Pid = StableSeedId.Menu("route:serverCenter"),
                Type = MenuTypeEnum.Menu,
                Name = "gameAreaProvision",
                Path = "/serverCenter/gameAreaProvision",
                Component = "/serverCenter/gameAreaProvision/index",
                Title = "开服向导",
                TitleCode = "message.menu.gameAreaProvision",
                Icon = "fa-solid fa-server",
                OrderNo = 110,
                IsEnabled = true,
                IsDeleted = false,
                IsKeepAlive = true,
                SuperAdminOnly = false,
            },
            new()
            {
                Id = StableSeedId.Menu("gameAreaProvision:templates"),
                Key = "gameAreaProvision:templates",
                Pid = StableSeedId.Menu("route:gameAreaProvision"),
                Type = MenuTypeEnum.Button,
                Permission = MenuSeedRouteName.Of<GameAreaProvisionService>(nameof(GameAreaProvisionService.TemplatesAsync)).ToLowerInvariant(),
                Title = "模板",
                OrderNo = 100,
                IsEnabled = true,
                IsDeleted = false,
                SuperAdminOnly = false,
            },
            new()
            {
                Id = StableSeedId.Menu("gameAreaProvision:preview"),
                Key = "gameAreaProvision:preview",
                Pid = StableSeedId.Menu("route:gameAreaProvision"),
                Type = MenuTypeEnum.Button,
                Permission = MenuSeedRouteName.Of<GameAreaProvisionService>(nameof(GameAreaProvisionService.PreviewAsync)).ToLowerInvariant(),
                Title = "预览",
                OrderNo = 101,
                IsEnabled = true,
                IsDeleted = false,
                SuperAdminOnly = false,
            },
            new()
            {
                Id = StableSeedId.Menu("gameAreaProvision:create"),
                Key = "gameAreaProvision:create",
                Pid = StableSeedId.Menu("route:gameAreaProvision"),
                Type = MenuTypeEnum.Button,
                Permission = MenuSeedRouteName.Of<GameAreaProvisionService>(nameof(GameAreaProvisionService.CreateAsync)).ToLowerInvariant(),
                Title = "创建区服",
                OrderNo = 102,
                IsEnabled = true,
                IsDeleted = false,
                SuperAdminOnly = false,
            },
            new()
            {
                Id = StableSeedId.Menu("gameAreaProvision:execute"),
                Key = "gameAreaProvision:execute",
                Pid = StableSeedId.Menu("route:gameAreaProvision"),
                Type = MenuTypeEnum.Button,
                Permission = MenuSeedRouteName.Of<GameAreaProvisionService>(nameof(GameAreaProvisionService.ExecuteAsync)).ToLowerInvariant(),
                Title = "执行开服",
                OrderNo = 103,
                IsEnabled = true,
                IsDeleted = false,
                SuperAdminOnly = false,
            },
            new()
            {
                Id = StableSeedId.Menu("gameAreaProvision:status"),
                Key = "gameAreaProvision:status",
                Pid = StableSeedId.Menu("route:gameAreaProvision"),
                Type = MenuTypeEnum.Button,
                Permission = MenuSeedRouteName.Of<GameAreaProvisionService>(nameof(GameAreaProvisionService.StatusAsync)).ToLowerInvariant(),
                Title = "查询状态",
                OrderNo = 104,
                IsEnabled = true,
                IsDeleted = false,
                SuperAdminOnly = false,
            },
        };
    }
}
