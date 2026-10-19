namespace GameFrameX.Client.Api.GameUserArchive.Models;

/// <summary>
/// 存档 user 分表行（客户端 SyncModel:UploadData 手工装配的 UserBasicInfo）
/// </summary>
public class GameUserArchiveUserBranch
{
    /// <summary>
    /// 金币
    /// </summary>
    public string Gold { get; set; } = string.Empty;

    /// <summary>
    /// 宝石
    /// </summary>
    public string Gem { get; set; } = string.Empty;

    /// <summary>
    /// 跳过券
    /// </summary>
    public string SkipProp { get; set; } = string.Empty;

    /// <summary>
    /// 等级
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// 经验
    /// </summary>
    public string Exp { get; set; } = string.Empty;

    /// <summary>
    /// 任务进度（客户端 Lua number 上送：chapterId*1000+已完成数；裸数字对 string 绑定 400，改 long 直收，
    /// DB 旧 string 数字靠宽容读取兼容）
    /// </summary>
    public long Progress { get; set; }

    /// <summary>
    /// 体力
    /// </summary>
    public string Energy { get; set; } = string.Empty;

    /// <summary>
    /// 订单天数（客户端 Lua number 上送）
    /// </summary>
    public long Day { get; set; }
}