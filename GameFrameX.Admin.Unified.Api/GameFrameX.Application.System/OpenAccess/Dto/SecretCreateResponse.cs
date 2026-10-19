namespace GameFrameX.Application.System.OpenAccess.Dto;

/// <summary>
/// 开放接口密钥创建结果
/// </summary>
public class SecretCreateResponse
{
    /// <summary>
    /// 新生成的开放接口密钥明文（Base64 编码的随机串）。
    /// <para>安全语义：该明文仅在本次响应中一次性下发，用于拷贝到开放接口身份的 AccessSecret 字段；获取后请立即妥善保存，后续无法再次查询。</para>
    /// </summary>
    public string Secret { get; set; } = null!;
}