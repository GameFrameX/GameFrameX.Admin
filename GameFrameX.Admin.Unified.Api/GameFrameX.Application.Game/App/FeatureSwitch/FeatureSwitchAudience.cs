namespace GameFrameX.Application.Game.App.FeatureSwitch;

/// <summary>
/// 开关投放的通道视角（下发方向）。
/// <para>与实体列 <c>Audience</c> 的 client / server / both 取值不同：本枚举描述「本次是谁在拉」，
/// 由解析器映射为行过滤——客户端通道取 client|both，服务器通道取 server|both。</para>
/// </summary>
public enum FeatureSwitchAudience
{
    /// <summary>
    /// 客户端通道（Client.Api 聚合下发，参与设备灰度）
    /// </summary>
    Client,

    /// <summary>
    /// 游戏服务器通道（Hub.Api 凭证同步，无设备身份，不做设备灰度）
    /// </summary>
    Server,
}
