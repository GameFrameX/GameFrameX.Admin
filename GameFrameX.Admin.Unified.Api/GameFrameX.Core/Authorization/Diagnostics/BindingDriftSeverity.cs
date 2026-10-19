using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Authorization.Diagnostics;

/// <summary>
/// 漂移严重级别。
/// </summary>
[AdminEnum]
public enum BindingDriftSeverity
{
    /// <summary>信息性，不阻断授权但提示数据健康。</summary>
    Warning = 1,

    /// <summary>会导致授权判定错误（fail-closed / 漂移授权），必须修复。</summary>
    Error = 2,
}