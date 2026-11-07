using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.User;

/// <summary>
/// 当前登录用户上下文（HttpContextAccessor 当前用户访问器； 由 UserManager 更名）。
/// </summary>
public class CurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public long UserId
    {
        get
        {
            var idText = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimConst.UserId)?.Value;
            return long.TryParse(idText, out var id) ? id : 0;
        }
    }

    public string? Account
    {
        get { return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimConst.Account)?.Value; }
    }

    public string? RealName
    {
        get { return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimConst.RealName)?.Value; }
    }

    public bool SuperAdmin
    {
        get => _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimConst.AccountType)?.Value == ((int)AccountTypeEnum.SuperAdmin).ToString();
    }

    public string? OpenId
    {
        get { return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimConst.OpenId)?.Value; }
    }

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
}
