using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using GameFrameX.Application.Dashboard.GameHome;
using GameFrameX.Core.Api;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 总览域菜单种子（partial；由 <see cref="GlobalMenuSeedData.BuildData"/> 聚合）。
/// </summary>
public partial class GlobalMenuSeedData
{
    private static IEnumerable<GlobalMenuEntity> DashboardMenus()
    {
        return new GlobalMenuEntity[]
        {
            new()
            {
                Id = StableSeedId.Menu("route:dashboard"),
                Key = "route:dashboard",
                Pid = 0,
                Type = MenuTypeEnum.Dir,
                Name = "dashboard",
                Path = "/dashboard",
                Component = "Layout",
                Redirect = null,
                Permission = null,
                Title = "总览",
                TitleCode = "message.menu.dashboard",
                Icon = "fa-solid fa-house",
                OutLink = null,
                IsIframe = false,
                IsHide = false,
                IsKeepAlive = true,
                IsAffix = false,
                OrderNo = 0,
                IsEnabled = true,
                Remark = null,
                CreatedTime = 1706499485,
                UpdateTime = 1706793684,
                SuperAdminOnly = false,
                AllowAdminWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("route:home"),
                Key = "route:home",
                Pid = StableSeedId.Menu("route:dashboard"),
                Type = MenuTypeEnum.Menu,
                Name = "home",
                Path = "/dashboard/home",
                Component = "/dashboard/home/index",
                Redirect = null,
                Permission = null,
                Title = "运营首页",
                TitleCode = "message.menu.home",
                Icon = "fa-solid fa-house",
                OutLink = null,
                IsIframe = false,
                IsHide = false,
                IsKeepAlive = true,
                IsAffix = true,
                OrderNo = 100,
                IsEnabled = true,
                Remark = null,
                CreatedTime = 1706499485,
                UpdateTime = 1706793684,
                SuperAdminOnly = false,
                AllowAdminWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("gameHome:GetOnline"),
                Key = "gameHome:GetOnline",
                Pid = StableSeedId.Menu("route:home"),
                Type = MenuTypeEnum.Button,
                Name = null,
                Path = null,
                Component = null,
                Redirect = null,
                Permission = MenuSeedRouteName.Of<GameHomeService>(nameof(GameHomeService.GetOnlineAsync)),
                Title = "查询在线玩家",
                TitleCode = null,
                Icon = null,
                OutLink = null,
                IsIframe = false,
                IsHide = false,
                IsKeepAlive = true,
                IsAffix = false,
                OrderNo = 200,
                IsEnabled = true,
                Remark = null,
                CreatedTime = 1783612800,
                UpdateTime = 1783612800,
                SuperAdminOnly = false,
                AllowAdminWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
        };
    }
}