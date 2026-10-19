using GameFrameX.Client.Api.GameAnnouncement.Dto;
using GameFrameX.Core.Const;
using GameFrameX.Entity.Tenant.Game.Announcement;

namespace GameFrameX.Client.Api.GameAnnouncement;

/// <summary>
/// 公告控制器
/// </summary>
/// <remarks>
/// 类名自 TenantGameAnnouncementEntityServiceController 清理而来（GFX-708）；调用方（游戏客户端）依赖既有路径，
/// 显式 [Route] 锁定旧 token 路由解析结果 api/TenantGameAnnouncementEntityService/[action]，确保路由零变化。
/// 不继承 BaseController&lt;T&gt;：其类级 token 路由 [controller] 会随类名解析为 api/GameAnnouncement 叠加出别名端点；
/// 本类直接继承 Controller 并自持仓储，路由仅显式声明一条。
/// </remarks>
[Route("api/TenantGameAnnouncementEntityService/[action]")]
[ApiController]
public class GameAnnouncementController : Controller
{
    private readonly SqlSugarRepository<TenantGameAnnouncementEntity> _repository;

    /// <summary>
    ///
    /// </summary>
    /// <param name="repository"></param>
    public GameAnnouncementController(SqlSugarRepository<TenantGameAnnouncementEntity> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 获取游戏公告列表
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(List<TenantGameAnnouncementEntityResponse>))]
    public async Task<HttpJsonResultData<List<TenantGameAnnouncementEntityResponse>>> List([FromQuery] GameAnnouncementListRequest input)
    {
        var res = await GetActiveAnnouncements(null, NormalizeLanguage(input?.Language));
        return HttpJsonResultData<List<TenantGameAnnouncementEntityResponse>>.Success(res);
    }

    /// <summary>
    /// 根据区服id获取公告列表数据
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(List<TenantGameAnnouncementEntityResponse>))]
    public async Task<HttpJsonResultData<List<TenantGameAnnouncementEntityResponse>>> ListByAreaId([FromQuery] GameAnnouncementListRequest input)
    {
        var language = NormalizeLanguage(input?.Language);
        var areaId = input?.Id > 0 ? input.Id : null;
        var res = await GetActiveAnnouncements(areaId, language);
        return HttpJsonResultData<List<TenantGameAnnouncementEntityResponse>>.Success(res);
    }

    private async Task<List<TenantGameAnnouncementEntityResponse>> GetActiveAnnouncements(long? areaId, string language)
    {
        var result = new List<TenantGameAnnouncementEntityResponse>();
        if (areaId.HasValue)
        {
            result.AddRange(await GetAnnouncementBucket(areaId.Value, language));
        }

        result.AddRange(await GetAnnouncementBucket(null, language));
        return result.OrderBy(u => u.AreaId.HasValue ? 0 : 1)
                     .ThenBy(u => u.Priority)
                     .ThenByDescending(u => u.CreatedTime)
                     .ToList();
    }

    private async Task<List<TenantGameAnnouncementEntityResponse>> GetAnnouncementBucket(long? areaId, string language)
    {
        if (!string.Equals(language, GameDimensionConst.LanguageDefault, StringComparison.OrdinalIgnoreCase))
        {
            var localized = await QueryActiveAnnouncements(areaId, language, false);
            if (localized.Count > 0)
            {
                return localized;
            }
        }

        return await QueryActiveAnnouncements(areaId, GameDimensionConst.LanguageDefault, true);
    }

    private Task<List<TenantGameAnnouncementEntityResponse>> QueryActiveAnnouncements(long? areaId, string language, bool includeEmptyLanguage)
    {
        var now = TimerHelper.UnixTimeSeconds();
#pragma warning disable CS8629 // WhereIF 条件守卫已确保 areaId.Value 仅在 HasValue=true 时进入表达式树
        var areaIdValue = areaId.GetValueOrDefault();
#pragma warning restore CS8629
        return _repository.AsQueryable()
                          .Where(u => u.IsActive && u.EndTime > now && u.StartTime < now)
                          .WhereIF(areaId.HasValue, u => u.AreaId == areaIdValue)
                          .WhereIF(!areaId.HasValue, u => u.AreaId == null)
                          .WhereIF(!includeEmptyLanguage, u => u.Language == language)
                          .WhereIF(includeEmptyLanguage, u => u.Language == language || u.Language == null || u.Language == string.Empty)
                          .OrderBy(u => u.Priority)
                          .Select<TenantGameAnnouncementEntityResponse>()
                          .ToListAsync();
    }

    private static string NormalizeLanguage(string? language)
    {
        return string.IsNullOrWhiteSpace(language) ? GameDimensionConst.LanguageDefault : language.Trim();
    }
}