namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// Admin 开放访问错误码（段 313，见 <see cref="ErrorSegmentRegistry"/>；C265 自旧 D 码体系平移）。
/// </summary>
public enum OpenAccessErrorCode
{
    /// <summary>
    /// 身份标识已存在
    /// </summary>
    [ErrorText("身份标识已存在", "Identity already exists")]
    IdentityAlreadyExists = 31300001,

    /// <summary>
    /// 应用不存在或不属于当前租户
    /// </summary>
    [ErrorText("应用不存在或不属于当前租户", "App does not exist or does not belong to the current tenant")]
    AppNotFoundOrNotOwned = 31300002,

    /// <summary>
    /// 绑定用户不存在或不属于当前租户
    /// </summary>
    [ErrorText("绑定用户不存在或不属于当前租户", "Bound user does not exist or does not belong to the current tenant")]
    BoundUserNotFoundOrNotOwned = 31300003,

    /// <summary>
    /// 凭证不存在或不属于当前应用
    /// </summary>
    [ErrorText("凭证不存在或不属于当前应用", "Credential does not exist or does not belong to the current app")]
    CredentialNotFoundOrNotOwned = 31300004,

    /// <summary>
    /// 凭证已撤销，不可启用
    /// </summary>
    [ErrorText("凭证已撤销，不可启用", "The credential is revoked and cannot be enabled")]
    CredentialRevokedEnableForbidden = 31300005,

    /// <summary>
    /// 接口范围为空或格式无效
    /// </summary>
    [ErrorText("接口范围为空或格式无效", "Scopes are empty or invalid")]
    ScopesEmptyOrInvalid = 31300006,
}