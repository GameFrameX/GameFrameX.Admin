namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 游戏业务种子主键常量。段位沿用 3195020XX82437（XX 顺序分配，66-71 为既有种子占用）；
/// 行 Id 固定跨环境稳定，交叉引用（区服→分类、商品→目录、奖励明细→物品）统一取此处常量，禁止散落魔法数字。
/// </summary>
public static class GameBusinessSeedIds
{
    /// <summary>区服分类 default 行（GameAreaClassifySeedData）。</summary>
    public const long AreaClassifyId = 31950207282437;

    /// <summary>物品分组（资源分类）default 行。</summary>
    public const long GoodsCatalogueId = 31950207382437;

    /// <summary>游戏物品（资源）default 行。</summary>
    public const long GoodsId = 31950207482437;

    /// <summary>游戏物品的配置 ID（游戏客户端道具表键，GameGoodsService 按 ConfigId 查重）。</summary>
    public const int GoodsConfigId = 10001;

    /// <summary>游戏内商品 default 行。</summary>
    public const long CommodityId = 31950207582437;

    /// <summary>游戏区服 default 行。</summary>
    public const long AreaId = 31950207682437;

    /// <summary>游戏公告 default 行。</summary>
    public const long AnnouncementId = 31950207782437;

    /// <summary>游戏内横幅公告 default 行。</summary>
    public const long BannerAnnouncementId = 31950207882437;

    /// <summary>邮件模板分类 default 行。</summary>
    public const long MailTemplateCategoryId = 31950207982437;

    /// <summary>邮件模板 default 行。</summary>
    public const long MailTemplateId = 31950208082437;

    /// <summary>游戏邮件 default 行（草稿，不自动投递）。</summary>
    public const long MailId = 31950208182437;

    /// <summary>奖励模板分组 default 行。</summary>
    public const long RewardTemplateGroupId = 31950208282437;

    /// <summary>奖励模板 default 行。</summary>
    public const long RewardTemplateId = 31950208382437;

    /// <summary>容器镜像标签 default 行。</summary>
    public const long DockerTagId = 31950208582437;

    /// <summary>功能开关 maintenance default 行。</summary>
    public const long FeatureSwitchId = 31950208682437;

    /// <summary>游戏应用分包 default 行（渠道引用既有 business-default 业务渠道种子）。</summary>
    public const long AppPackageId = 31950208782437;

    /// <summary>网页商城 default 行（默认下架）。</summary>
    public const long WebShopId = 31950208882437;

    /// <summary>客户端资源包版本 default 行（DefaultPackage × 1.0.0）。</summary>
    public const long AssetPackageVersionId = 31950208982437;

    /// <summary>通用兑换码 default 行。</summary>
    public const long RedeemCodeId = 31950208482437;
}
