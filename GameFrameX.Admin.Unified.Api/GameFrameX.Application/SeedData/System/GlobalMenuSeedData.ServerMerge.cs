using GameFrameX.Application.ServerCenter.GameAreaMerge;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Application.SeedData.System;

/// <summary>区服合服向导及全部端点权限。</summary>
public partial class GlobalMenuSeedData
{
    private static IEnumerable<GlobalMenuEntity> ServerMergeMenus()
    {
        return new GlobalMenuEntity[]
        {
            new()
            {
                Id = StableSeedId.Menu("route:gameAreaMerge"),
                Key = "route:gameAreaMerge",
                Pid = StableSeedId.Menu("route:serverCenter"),
                Type = MenuTypeEnum.Menu,
                Name = "gameAreaMerge",
                Path = "/serverCenter/gameAreaMerge",
                Component = "/serverCenter/gameAreaMerge/index",
                Title = "合服",
                TitleCode = "message.menu.gameAreaMerge",
                Icon = "fa-solid fa-server",
                IsKeepAlive = true,
                OrderNo = 120,
                IsEnabled = true,
                CreatedTime = 1793808000,
                SuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("gameAreaMerge:add"),
                Key = "gameAreaMerge:add",
                Pid = StableSeedId.Menu("route:gameAreaMerge"),
                Type = MenuTypeEnum.Button,
                Permission = MenuSeedRouteName.Of<GameAreaMergeService>(nameof(GameAreaMergeService.AddAsync)),
                Title = "新建",
                IsEnabled = true,
                CreatedTime = 1793808000,
                SuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("gameAreaMerge:page"),
                Key = "gameAreaMerge:page",
                Pid = StableSeedId.Menu("route:gameAreaMerge"),
                Type = MenuTypeEnum.Button,
                Permission = MenuSeedRouteName.Of<GameAreaMergeService>(nameof(GameAreaMergeService.PageAsync)),
                Title = "查询",
                IsEnabled = true,
                CreatedTime = 1793808000,
                SuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("gameAreaMerge:detail"),
                Key = "gameAreaMerge:detail",
                Pid = StableSeedId.Menu("route:gameAreaMerge"),
                Type = MenuTypeEnum.Button,
                Permission = MenuSeedRouteName.Of<GameAreaMergeService>(nameof(GameAreaMergeService.DetailAsync)),
                Title = "详情",
                IsEnabled = true,
                CreatedTime = 1793808000,
                SuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("gameAreaMerge:dryRun"),
                Key = "gameAreaMerge:dryRun",
                Pid = StableSeedId.Menu("route:gameAreaMerge"),
                Type = MenuTypeEnum.Button,
                Permission = MenuSeedRouteName.Of<GameAreaMergeService>(nameof(GameAreaMergeService.DryRunAsync)),
                Title = "预演",
                IsEnabled = true,
                CreatedTime = 1793808000,
                SuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("gameAreaMerge:approve"),
                Key = "gameAreaMerge:approve",
                Pid = StableSeedId.Menu("route:gameAreaMerge"),
                Type = MenuTypeEnum.Button,
                Permission = MenuSeedRouteName.Of<GameAreaMergeService>(nameof(GameAreaMergeService.ApproveAsync)),
                Title = "批准",
                IsEnabled = true,
                CreatedTime = 1793808000,
                SuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("gameAreaMerge:cancel"),
                Key = "gameAreaMerge:cancel",
                Pid = StableSeedId.Menu("route:gameAreaMerge"),
                Type = MenuTypeEnum.Button,
                Permission = MenuSeedRouteName.Of<GameAreaMergeService>(nameof(GameAreaMergeService.CancelAsync)),
                Title = "取消",
                IsEnabled = true,
                CreatedTime = 1793808000,
                SuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("gameAreaMerge:execute"),
                Key = "gameAreaMerge:execute",
                Pid = StableSeedId.Menu("route:gameAreaMerge"),
                Type = MenuTypeEnum.Button,
                Permission = MenuSeedRouteName.Of<GameAreaMergeService>(nameof(GameAreaMergeService.ExecuteAsync)),
                Title = "执行",
                IsEnabled = true,
                CreatedTime = 1793808000,
                SuperAdminOnly = false,
                IsDeleted = false,
            },
        };
    }
}
