namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 公众号方法通用请求基类。
/// <para>Admin 会话本身无隐式 AppId；调用方必须显式传入 <see cref="AppId"/>，resolver 仅在 <c>AppId=0</c> 时选择租户默认公众号配置。</para>
/// </summary>
public abstract class WechatAppScopedRequest
{
    /// <summary>
    /// 应用 ID。0/null 表示租户默认公众号配置；&gt; 0 表示应用级覆盖。
    /// </summary>
    public long? AppId { get; set; }
}