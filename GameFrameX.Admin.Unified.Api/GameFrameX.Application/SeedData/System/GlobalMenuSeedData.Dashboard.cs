using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using GameFrameX.Application.Game.Home.GameHome;
using GameFrameX.Core.Api;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 工作台 域菜单种子（partial；由 <see cref="GlobalMenuSeedData.BuildData"/> 聚合）。
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
                AppScope = MenuAppScopeEnum.Tenant,
                Component = "Layout",
                Redirect = null,
                Permission = null,
                Title = "工作台",
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
                SyncRoles = (int)(TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager | TenantMenuSyncRole.GmSupport | TenantMenuSyncRole.ContentOps | TenantMenuSyncRole.ServerOps | TenantMenuSyncRole.PaymentOps | TenantMenuSyncRole.MarketingOps | TenantMenuSyncRole.DataAnalyst | TenantMenuSyncRole.AuditSecurity | TenantMenuSyncRole.AppConfigAdmin),
                SuperAdminOnly = false,
                AllowTenantWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("route:home"),
                Key = "route:home",
                Pid = StableSeedId.Menu("route:dashboard"),
                Type = MenuTypeEnum.Menu,
                Name = "home",
                Path = "/tenant/dashboard/home",
                AppScope = MenuAppScopeEnum.Tenant,
                Component = "/tenant/home/index",
                Redirect = null,
                Permission = null,
                Title = "工作台",
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
                SyncRoles = (int)(TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager | TenantMenuSyncRole.GmSupport | TenantMenuSyncRole.ContentOps | TenantMenuSyncRole.ServerOps | TenantMenuSyncRole.PaymentOps | TenantMenuSyncRole.MarketingOps | TenantMenuSyncRole.DataAnalyst | TenantMenuSyncRole.AuditSecurity | TenantMenuSyncRole.AppConfigAdmin),
                SuperAdminOnly = false,
                AllowTenantWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("route:notice"),
                Key = "route:notice",
                Pid = StableSeedId.Menu("route:dashboard"),
                Type = MenuTypeEnum.Menu,
                Name = "notice",
                Path = "/tenant/dashboard/notice",
                AppScope = MenuAppScopeEnum.Tenant,
                Component = "/tenant/home/notice/index",
                Redirect = null,
                Permission = null,
                Title = "站内信",
                TitleCode = "message.menu.notice",
                Icon = "fa-solid fa-bell",
                OutLink = null,
                IsIframe = false,
                IsHide = false,
                IsKeepAlive = true,
                IsAffix = false,
                OrderNo = 101,
                IsEnabled = true,
                Remark = null,
                CreatedTime = 1706499485,
                UpdateTime = 1706793684,
                SyncRoles = (int)(TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager | TenantMenuSyncRole.GmSupport | TenantMenuSyncRole.ContentOps | TenantMenuSyncRole.ServerOps | TenantMenuSyncRole.PaymentOps | TenantMenuSyncRole.MarketingOps | TenantMenuSyncRole.DataAnalyst | TenantMenuSyncRole.AuditSecurity | TenantMenuSyncRole.AppConfigAdmin),
                SuperAdminOnly = false,
                AllowTenantWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("tenantGameHomeEntity:GetOnline"),
                Key = "tenantGameHomeEntity:GetOnline",
                Pid = StableSeedId.Menu("route:home"),
                Type = MenuTypeEnum.Button,
                Name = null,
                Path = null,
                Component = null,
                Redirect = null,
                Permission = MenuSeedRouteName.Of<TenantGameHomeEntityService>(nameof(TenantGameHomeEntityService.GetOnlineAsync)),
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
                SyncRoles = (int)(TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager | TenantMenuSyncRole.GmSupport | TenantMenuSyncRole.ContentOps | TenantMenuSyncRole.ServerOps | TenantMenuSyncRole.PaymentOps | TenantMenuSyncRole.MarketingOps | TenantMenuSyncRole.DataAnalyst | TenantMenuSyncRole.AuditSecurity | TenantMenuSyncRole.AppConfigAdmin),
                SuperAdminOnly = false,
                AllowTenantWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
        };
    }
}