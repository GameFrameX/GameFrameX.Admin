using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.User;

/// <summary>
/// 当前登录用户上下文（HttpContextAccessor 当前用户访问器；GFX-706 由 UserManager 更名）。
/// </summary>
public class CurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private long _tenantId;

    public long UserId
    {
        get
        {
            var idText = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimConst.UserId)?.Value;
            return long.TryParse(idText, out var id) ? id : 0;
        }
    }

    public long TenantId
    {
        get
        {
            var tId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimConst.TenantId)?.Value;
            return string.IsNullOrWhiteSpace(tId) ? _tenantId : long.Parse(tId);
        }
        set { _tenantId = value; }
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

    public long OrganizationId
    {
        get
        {
            var orgId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimConst.OrganizationId)?.Value;
            return string.IsNullOrWhiteSpace(orgId) ? 0 : long.Parse(orgId);
        }
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