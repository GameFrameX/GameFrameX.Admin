using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 单控制台运营中心的共享根目录，叶子菜单由各业务域维护。
/// </summary>
public partial class GlobalMenuSeedData
{
    private static IEnumerable<GlobalMenuEntity> OperationsRootMenus()
    {
        return new GlobalMenuEntity[]
        {
            new()
            {
                Id = StableSeedId.Menu("route:serverCenter"),
                Key = "route:serverCenter",
                Pid = 0,
                Type = MenuTypeEnum.Dir,
                Name = "serverCenter",
                Path = "/serverCenter",
                Component = "Layout",
                Title = "区服",
                TitleCode = "message.menu.serverCenter",
                Icon = "fa-solid fa-server",
                IsKeepAlive = true,
                OrderNo = 100,
                IsEnabled = true,
                CreatedTime = 1793808000,
                UpdateTime = 1793808000,
                SuperAdminOnly = false,
                AllowAdminWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("route:noticeCenter"),
                Key = "route:noticeCenter",
                Pid = 0,
                Type = MenuTypeEnum.Dir,
                Name = "noticeCenter",
                Path = "/noticeCenter",
                Component = "Layout",
                Title = "内容运营",
                TitleCode = "message.menu.noticeCenter",
                Icon = "fa-solid fa-bullhorn",
                IsKeepAlive = true,
                OrderNo = 200,
                IsEnabled = true,
                CreatedTime = 1793808000,
                UpdateTime = 1793808000,
                SuperAdminOnly = false,
                AllowAdminWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("route:gmCenter"),
                Key = "route:gmCenter",
                Pid = 0,
                Type = MenuTypeEnum.Dir,
                Name = "gmCenter",
                Path = "/gmCenter",
                Component = "Layout",
                Title = "在线运维",
                TitleCode = "message.menu.gmCenter",
                Icon = "fa-solid fa-terminal",
                IsKeepAlive = true,
                OrderNo = 300,
                IsEnabled = true,
                CreatedTime = 1793808000,
                UpdateTime = 1793808000,
                SuperAdminOnly = false,
                AllowAdminWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("route:commerceCenter"),
                Key = "route:commerceCenter",
                Pid = 0,
                Type = MenuTypeEnum.Dir,
                Name = "commerceCenter",
                Path = "/commerceCenter",
                Component = "Layout",
                Title = "商业化",
                TitleCode = "message.menu.commerceCenter",
                Icon = "fa-solid fa-cart-shopping",
                IsKeepAlive = true,
                OrderNo = 500,
                IsEnabled = true,
                CreatedTime = 1793808000,
                UpdateTime = 1793808000,
                SuperAdminOnly = false,
                AllowAdminWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("route:clientDelivery"),
                Key = "route:clientDelivery",
                Pid = 0,
                Type = MenuTypeEnum.Dir,
                Name = "clientDelivery",
                Path = "/clientDelivery",
                Component = "Layout",
                Title = "C 端交付",
                TitleCode = "message.menu.clientDelivery",
                Icon = "fa-solid fa-cloud-arrow-down",
                IsKeepAlive = true,
                OrderNo = 600,
                IsEnabled = true,
                CreatedTime = 1793808000,
                UpdateTime = 1793808000,
                SuperAdminOnly = false,
                AllowAdminWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("route:audit"),
                Key = "route:audit",
                Pid = 0,
                Type = MenuTypeEnum.Dir,
                Name = "audit",
                Path = "/audit",
                Component = "Layout",
                Title = "审计",
                TitleCode = "message.menu.audit",
                Icon = "fa-solid fa-clipboard-list",
                IsKeepAlive = true,
                OrderNo = 700,
                IsEnabled = true,
                CreatedTime = 1793808000,
                UpdateTime = 1793808000,
                SuperAdminOnly = false,
                AllowAdminWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
        };
    }
}
