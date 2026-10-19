using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Const;

/// <summary>
/// Attribute 形式的默认租户基础数据模板。
/// </summary>
[TenantOrganizationTemplate("publisher", "发行运营中心", "GFX-PUBLISHER", "publisher", Level = 1, OrderNo = 100, Remark = "默认游戏发行与运营主体")]
[TenantOrganizationTemplate("studio_thunder", "雷霆工作室", "GFX-STUDIO-THUNDER", "studio", ParentKey = "publisher", Level = 2, OrderNo = 100, Remark = "核心 MMORPG 项目研发与运营工作室")]
[TenantOrganizationTemplate("studio_spark", "星火工作室", "GFX-STUDIO-SPARK", "studio", ParentKey = "publisher", Level = 2, OrderNo = 100, Remark = "休闲与小游戏项目工作室")]
[TenantOrganizationTemplate("ops_platform", "平台运营中心", "GFX-OPS-PLATFORM", "project", ParentKey = "publisher", Level = 2, OrderNo = 100, Remark = "跨游戏活动、支付、渠道和玩家服务运营")]
[TenantOrganizationTemplate("ops_channel", "渠道运营组", "GFX-OPS-CHANNEL", "channel_ops", ParentKey = "ops_platform", Level = 3, OrderNo = 100, Remark = "渠道接入、投放和联运运营")]
[TenantOrganizationTemplate("ops_support", "玩家服务组", "GFX-OPS-SUPPORT", "support_ops", ParentKey = "ops_platform", Level = 3, OrderNo = 100, Remark = "客服、社群和玩家反馈处理")]
[TenantOrganizationTemplate("game_fantasy", "幻想纪元项目", "GAME-FANTASY", "project", Level = 1, OrderNo = 100, Remark = "幻想纪元游戏项目组织")]
[TenantOrganizationTemplate("game_fantasy_cn", "国服运营大区", "GAME-FANTASY-CN", "server_group", ParentKey = "game_fantasy", Level = 2, OrderNo = 100, Remark = "国服区服运营与活动配置")]
[TenantOrganizationTemplate("game_fantasy_global", "海外运营大区", "GAME-FANTASY-GLOBAL", "server_group", ParentKey = "game_fantasy", Level = 2, OrderNo = 100, Remark = "海外区服运营与本地化协作")]
[TenantOrganizationTemplate("game_fantasy_channel", "幻想纪元渠道组", "GAME-FANTASY-CHANNEL", "channel_ops", ParentKey = "game_fantasy", Level = 2, OrderNo = 100, Remark = "幻想纪元渠道、平台和分包管理")]
[TenantOrganizationTemplate("game_pixel", "像素远征项目", "GAME-PIXEL", "project", Level = 1, OrderNo = 100, Remark = "像素远征游戏项目组织")]
[TenantOrganizationTemplate("game_pixel_qa", "测试服运营组", "GAME-PIXEL-QA", "server_group", ParentKey = "game_pixel", Level = 2, OrderNo = 100, Remark = "测试服、灰度服和预发布环境运营")]
[TenantOrganizationTemplate("game_pixel_live", "正式服运营组", "GAME-PIXEL-LIVE", "server_group", ParentKey = "game_pixel", Level = 2, OrderNo = 100, Remark = "正式服区服维护和活动执行")]
[TenantOrganizationTemplate("game_pixel_support", "像素远征客服组", "GAME-PIXEL-SUPPORT", "support_ops", ParentKey = "game_pixel", Level = 2, OrderNo = 100, Remark = "像素远征玩家服务与社群支持")]
[TenantRoleTemplate("sys_admin", "系统管理员", DataScopeEnum.All, TenantMenuSyncRole.TenantAdmin, OrderNo = 1, Remark = "平台/租户系统保留角色")]
[TenantRoleTemplate("ops_manager", "运营负责人", DataScopeEnum.All, TenantMenuSyncRole.OpsManager, OrderNo = 10, Remark = "活动、公告、邮件、兑换码、商品总览")]
[TenantRoleTemplate("gm_support", "客服GM", DataScopeEnum.DeptChild, TenantMenuSyncRole.GmSupport, OrderNo = 20, Remark = "玩家查询、封禁、订单处理、补偿发放")]
[TenantRoleTemplate("content_ops", "内容运营", DataScopeEnum.Dept, TenantMenuSyncRole.ContentOps, OrderNo = 30, Remark = "公告、邮件、模板、语言、平台配置")]
[TenantRoleTemplate("server_ops", "区服运维", DataScopeEnum.Dept, TenantMenuSyncRole.ServerOps, OrderNo = 40, Remark = "区服、分类、Docker、发布相关")]
[TenantRoleTemplate("payment_ops", "支付运营", DataScopeEnum.Self, TenantMenuSyncRole.PaymentOps, OrderNo = 50, Remark = "支付配置、商品、订单、充值链路")]
[TenantRoleTemplate("marketing_ops", "市场活动", DataScopeEnum.Self, TenantMenuSyncRole.MarketingOps, OrderNo = 60, Remark = "兑换码、活动素材、邮件触达")]
[TenantRoleTemplate("data_analyst", "数据分析", DataScopeEnum.Self, TenantMenuSyncRole.DataAnalyst, OrderNo = 70, Remark = "只读查询、导出、报表")]
[TenantRoleTemplate("audit_security", "审计安全", DataScopeEnum.All, TenantMenuSyncRole.AuditSecurity, OrderNo = 80, Remark = "日志、差异、权限变更审计")]
[TenantRoleTemplate("app_config_admin", "应用配置管理员", DataScopeEnum.Dept, TenantMenuSyncRole.AppConfigAdmin, OrderNo = 90, Remark = "应用、分包、远程配置、版本维护")]
[TenantPositionTemplate("general_manager", "总经理", OrderNo = 1, Remark = "游戏业务经营与组织管理负责人")]
[TenantPositionTemplate("ops_director", "运营负责人", OrderNo = 10, Remark = "游戏运营、活动、渠道和玩家服务负责人")]
[TenantPositionTemplate("tech_director", "技术负责人", OrderNo = 20, Remark = "游戏服务端、客户端和平台技术负责人")]
[TenantPositionTemplate("product_director", "产品负责人", OrderNo = 30, Remark = "游戏产品规划、版本节奏和商业化负责人")]
[TenantPositionTemplate("producer", "项目制作人", OrderNo = 40, Remark = "单款游戏项目制作与跨团队协调负责人")]
[TenantPositionTemplate("project_manager", "项目经理", OrderNo = 50, Remark = "项目排期、交付跟进和资源协调")]
[TenantPositionTemplate("game_designer", "游戏策划", OrderNo = 60, Remark = "玩法、系统、数值和活动配置设计")]
[TenantPositionTemplate("server_engineer", "服务端工程师", OrderNo = 70, Remark = "游戏服务端、后台服务和运维工具开发")]
[TenantPositionTemplate("client_engineer", "客户端工程师", OrderNo = 80, Remark = "Unity 客户端、SDK 接入和资源流程开发")]
[TenantPositionTemplate("qa_engineer", "测试工程师", OrderNo = 90, Remark = "功能测试、回归测试和发布验收")]
[TenantPositionTemplate("game_operator", "游戏运营", OrderNo = 100, Remark = "日常运营、公告邮件、补偿和区服配置")]
[TenantPositionTemplate("event_operator", "活动运营", OrderNo = 110, Remark = "活动策划、礼包、兑换码和触达运营")]
[TenantPositionTemplate("channel_operator", "渠道运营", OrderNo = 120, Remark = "渠道接入、联运平台和分包配置运营")]
[TenantPositionTemplate("support_specialist", "客服专员", OrderNo = 130, Remark = "玩家工单、反馈、封禁和补偿协助处理")]
[TenantPositionTemplate("data_analyst", "数据分析师", OrderNo = 140, Remark = "运营数据、收入数据和玩家行为分析")]
[TenantPositionTemplate("finance_specialist", "财务专员", OrderNo = 150, Remark = "支付对账、订单核查和收入结算")]
internal sealed class TenantDefaultTemplateDefinitions
{
}
