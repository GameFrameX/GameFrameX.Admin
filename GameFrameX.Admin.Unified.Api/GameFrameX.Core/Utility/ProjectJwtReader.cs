using System.IdentityModel.Tokens.Jwt;

namespace GameFrameX.Core.Utility;

/// <summary>
/// JWT 读取兼容入口。
/// </summary>
public static class ProjectJwtReader
{
    /// <summary>
    /// 读取 JWT 内容；仅用于取 claim，不执行签名校验。
    /// </summary>
    public static JwtSecurityToken? ReadJwtToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        const string bearerPrefix = "Bearer ";
        var normalizedToken = token.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
                                  ? token[bearerPrefix.Length..]
                                  : token;
        return new JwtSecurityTokenHandler().CanReadToken(normalizedToken)
                   ? new JwtSecurityTokenHandler().ReadJwtToken(normalizedToken)
                   : null;
    }
}