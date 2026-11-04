namespace GameFrameX.Application.Platform.OpenAccess.GameAppOpenAccess.Dto;

/// <summary>
/// 应用开放访问凭证签发 / 轮换结果。
/// <para><see cref="AccessSecret"/> 仅在签发 / 轮换响应中返回一次，前端须立即保存；后续列表 / 详情不再输出。</para>
/// </summary>
public class GameAppOpenAccessSecretResponse
{
    /// <summary>
    /// 凭证 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 访问 Key。
    /// </summary>
    public string AccessKey { get; set; } = null!;

    /// <summary>
    /// 访问密钥明文。仅在签发 / 轮换响应中返回一次。
    /// </summary>
    public string AccessSecret { get; set; } = null!;

    /// <summary>
    /// 接口范围（JSON 数组字符串）。
    /// </summary>
    public string ScopesJson { get; set; } = null!;
}