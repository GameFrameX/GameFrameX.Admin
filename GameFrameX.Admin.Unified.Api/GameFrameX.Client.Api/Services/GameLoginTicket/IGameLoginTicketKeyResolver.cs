namespace GameFrameX.Client.Api.Services.GameLoginTicket;

/// <summary>
/// Game 登录票据签名 Key 解析服务。
/// </summary>
public interface IGameLoginTicketKeyResolver
{
    /// <summary>
    /// 按租户和应用解析当前签名 Key（取生效中最新一把）。
    /// </summary>
    Task<GameLoginTicketSigningKey> ResolveSigningKeyAsync(long tenantId, long appId);

    /// <summary>
    /// 获取当前可用的全部公钥（含手动轮换尚未到期的旧 key），供 Server 验签在途票据。
    /// </summary>
    Task<IReadOnlyCollection<GameLoginTicketPublicKey>> GetPublicKeysAsync(long tenantId, long appId);
}