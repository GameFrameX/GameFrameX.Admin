namespace GameFrameX.Core.Const;

/// <summary>
/// 游戏客户端维度回退默认值常量（三 host 共享）。
/// <para>客户端请求携带的维度参数（渠道 / 包名 / 平台 / 语言 / 子渠道 / 程序版本）缺省时按这些默认值
/// 匹配租户库中的“默认配置”行；原 Client.Api / Hub.Api 各自的 ConstValue 副本已收敛至此。</para>
/// </summary>
public static class GameDimensionConst
{
    /// <summary>
    /// 主渠道默认值
    /// </summary>
    public const string ChannelNameDefault = "default";

    /// <summary>
    /// 包名默认值
    /// </summary>
    public const string PackageNameDefault = "default";

    /// <summary>
    /// 平台默认值
    /// </summary>
    public const string PlatformDefault = "default";

    /// <summary>
    /// 语言默认值
    /// </summary>
    public const string LanguageDefault = "default";

    /// <summary>
    /// 子渠道默认值
    /// </summary>
    public const string SubChannelNameDefault = "default";

    /// <summary>
    /// 程序版本默认值
    /// </summary>
    public const string AppVersionDefault = "default";
}