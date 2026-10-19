namespace GameFrameX.Core.Middleware;

/// <summary>
/// 协议版本头（X-Proto-Ver）豁免标记：实现本接口的 endpoint metadata 不受缺头拒绝约束。
/// <para>用于外部平台服务端回调（TopOn / Pangle / AdMob / 支付宝 notify 等 S2S 入口）——对端不是游戏客户端，
/// 不会携带 X-Proto-Ver，缺头拒绝只面向游戏客户端接入方。</para>
/// </summary>
public interface IProtoVersionExempt
{
}
