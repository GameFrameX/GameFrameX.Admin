using GameFrameX.Core.Service;
using GameFrameX.Application.Game.ServerManager;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Hub.Api.Dto.WebHook;
using Microsoft.AspNetCore.Authorization;

namespace GameFrameX.Hub.Api.Controllers;

/// <summary>
/// Tapd WebHook控制器
/// </summary>
/// <remarks>
/// WebHook 为匿名调用（[AllowAnonymous]），无法从 JWT 解析租户；租户由 <c>HubTenantMiddleware</c>
/// 从 <c>GameFrameX-Tenant-Id</c> 请求头解析并写入 <c>HttpContext.Items</c>，Repository 据此路由到对应租户库。
/// 无租户头时中间件返回 401。
/// <para>类级 [Authorize] 维持 fail-closed 默认（C236 随 BaseController 副本回收显式化）：新增 action 默认要求 JWT，匿名须逐 action 显式 [AllowAnonymous]。</para>
/// </remarks>
[Authorize]
public class TapdWebHookController : BaseController<TenantGameAreaEntity>
{
    private readonly HttpClient _httpClient;

    /// <summary>
    ///
    /// </summary>
    /// <param name="service"></param>
    /// <param name="httpClient"></param>
    public TapdWebHookController(Repository<TenantGameAreaEntity> service, HttpClient httpClient) : base(service)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// 接收Tapd WebHook 任务或缺陷完成事件
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> WebHookTaskFinish()
    {
        var streamReader = new StreamReader(Request.Body);
        var body = await streamReader.ReadToEndAsync();
        Console.WriteLine(body);
        var tapdWebHookData = JsonHelper.Deserialize<TapdWebHookData>(body);
        var id = int.Parse(tapdWebHookData.Event.Id.Substring(tapdWebHookData.Event.Id.Length - 8));

        var gameAreaEntity = await PRepository.GetFirstAsync(m => m.Id == id);
        if (gameAreaEntity == null)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.NotFound();
        }

        var gameAreaClassifyEntity = await PRepository.AsSugarClient().Queryable<TenantGameAreaClassifyEntity>().Where(m => m.Id == gameAreaEntity.ClassifyId).FirstAsync();
        var url = gameAreaClassifyEntity.HttpManagerUrl;

        if (!string.IsNullOrEmpty(url))
        {
            try
            {
                // 执行停止
                var result = await _httpClient.PostJsonToStringAsync(url + "api/Containers/Stop", new BaseRequest { Id = gameAreaEntity.ServerContainerId, });
                LogHelper.Info("触发 从服务器 终止容器 CI/CD 结果:" + result);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        // 删除
        await PRepository.FakeDeleteAsync(gameAreaEntity);
        return HttpJsonResultData<BaseOperationAckResponse>.Success();
    }
}