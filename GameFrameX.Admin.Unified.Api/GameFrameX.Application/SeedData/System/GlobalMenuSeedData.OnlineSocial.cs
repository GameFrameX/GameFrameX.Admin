using GameFrameX.Core.Api;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using GameFrameX.Application.Game.Online.Social;
using GameFrameX.Core.Api;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// Online 社交治理 域菜单种子（partial；由 <see cref="GlobalMenuSeedData.BuildData"/> 聚合；change C350 / GFX-754）。
/// <para>
/// 容器 route:onlineCenter 由 C340 的 <see cref="GlobalMenuSeedData.OnlineMenus"/>（GlobalMenuSeedData.Online.cs）正式提供；
/// 本文件原按 C340 A0 冻结契约预置的容器在合并 main 时移除（两容器 Icon/时间戳形态不同，同 Id 双行会破坏 seed upsert 幂等），
/// messageAudit / reportCase 叶子直接挂载正式容器之下。
/// </para>
/// </summary>
public partial class GlobalMenuSeedData
{
    private static IEnumerable<GlobalMenuEntity> OnlineSocialMenus()
    {
        return new GlobalMenuEntity[]
        {
            new()
            {
                Id = StableSeedId.Menu("route:messageAudit"),
                Key = "route:messageAudit",
                Pid = StableSeedId.Menu("route:onlineCenter"),
                Type = MenuTypeEnum.Menu,
                Name = "messageAudit",
                Path = "/app/onlineCenter/messageAudit",
                AppScope = MenuAppScopeEnum.App,
                Component = "/app/onlineCenter/messageAudit/index",
                Redirect = null,
                Permission = null,
                Title = "消息审计",
                TitleCode = "message.menu.messageAudit",
                Icon = "fa-solid fa-magnifying-glass",
                OutLink = null,
                IsIframe = false,
                IsHide = false,
                IsKeepAlive = true,
                IsAffix = false,
                OrderNo = 100,
                IsEnabled = true,
                Remark = null,
                CreatedTime = 1783612800,
                UpdateTime = 1783612800,
                SyncRoles = (int)(TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager),
                SuperAdminOnly = false,
                AllowTenantWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("route:reportCase"),
                Key = "route:reportCase",
                Pid = StableSeedId.Menu("route:onlineCenter"),
                Type = MenuTypeEnum.Menu,
                Name = "reportCase",
                Path = "/app/onlineCenter/reportCase",
                AppScope = MenuAppScopeEnum.App,
                Component = "/app/onlineCenter/reportCase/index",
                Redirect = null,
                Permission = null,
                Title = "举报处置",
                TitleCode = "message.menu.reportCase",
                Icon = "fa-solid fa-flag",
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
                SyncRoles = (int)(TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager),
                SuperAdminOnly = false,
                AllowTenantWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("messageAudit:page"),
                Key = "messageAudit:page",
                Pid = StableSeedId.Menu("route:messageAudit"),
                Type = MenuTypeEnum.Button,
                Name = null,
                Path = null,
                Component = null,
                Redirect = null,
                Permission = MenuSeedRouteName.Of<OnlineMessageAuditQueryService>(nameof(OnlineMessageAuditQueryService.PageAsync)),
                Title = "查询",
                TitleCode = null,
                Icon = null,
                OutLink = null,
                IsIframe = false,
                IsHide = false,
                IsKeepAlive = true,
                IsAffix = false,
                OrderNo = 100,
                IsEnabled = true,
                Remark = null,
                CreatedTime = 1783612800,
                UpdateTime = 1783612800,
                SyncRoles = (int)(TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager),
                SuperAdminOnly = false,
                AllowTenantWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("reportCase:page"),
                Key = "reportCase:page",
                Pid = StableSeedId.Menu("route:reportCase"),
                Type = MenuTypeEnum.Button,
                Name = null,
                Path = null,
                Component = null,
                Redirect = null,
                Permission = MenuSeedRouteName.Of<OnlineReportCaseService>(nameof(OnlineReportCaseService.PageAsync)),
                Title = "查询",
                TitleCode = null,
                Icon = null,
                OutLink = null,
                IsIframe = false,
                IsHide = false,
                IsKeepAlive = true,
                IsAffix = false,
                OrderNo = 100,
                IsEnabled = true,
                Remark = null,
                CreatedTime = 1783612800,
                UpdateTime = 1783612800,
                SyncRoles = (int)(TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager),
                SuperAdminOnly = false,
                AllowTenantWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                Id = StableSeedId.Menu("reportCase:handle"),
                Key = "reportCase:handle",
                Pid = StableSeedId.Menu("route:reportCase"),
                Type = MenuTypeEnum.Button,
                Name = null,
                Path = null,
                Component = null,
                Redirect = null,
                Permission = MenuSeedRouteName.Of<OnlineReportCaseService>(nameof(OnlineReportCaseService.HandleAsync)),
                Title = "处置",
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
                SyncRoles = (int)TenantMenuSyncRole.TenantAdmin,
                SuperAdminOnly = false,
                AllowTenantWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
            new()
            {
                // 处罚联动命令手动重试入口（高危；处置流内部经 ExecuteLinkAsync 不吃本权限，本按钮供运营在页面手动补发）。
                Id = StableSeedId.Menu("penaltyLinkCommand:execute"),
                Key = "penaltyLinkCommand:execute",
                Pid = StableSeedId.Menu("route:reportCase"),
                Type = MenuTypeEnum.Button,
                Name = null,
                Path = null,
                Component = null,
                Redirect = null,
                Permission = MenuSeedRouteName.Of<OnlinePenaltyLinkCommandService>(nameof(OnlinePenaltyLinkCommandService.ExecuteAsync)),
                Title = "处罚联动",
                TitleCode = null,
                Icon = null,
                OutLink = null,
                IsIframe = false,
                IsHide = false,
                IsKeepAlive = true,
                IsAffix = false,
                OrderNo = 300,
                IsEnabled = true,
                Remark = null,
                CreatedTime = 1783612800,
                UpdateTime = 1783612800,
                SyncRoles = (int)TenantMenuSyncRole.TenantAdmin,
                SuperAdminOnly = false,
                AllowTenantWhenSuperAdminOnly = false,
                IsDeleted = false,
            },
        };
    }
}
