namespace GameFrameX.Client.Api.Const;

/// <summary>
/// Client API 本地常量。三 host 共享的维度默认值常量已收敛至 GameDimensionConst（GameFrameX.Core.Const），
/// 本类仅保留 Client API 特有差异项。
/// </summary>
public static class ConstValue
{
    /// <summary>
    /// 应用作用域默认值，0 表示租户默认配置（非应用级覆盖）
    /// </summary>
    public const long AppIdDefault = 0;
}