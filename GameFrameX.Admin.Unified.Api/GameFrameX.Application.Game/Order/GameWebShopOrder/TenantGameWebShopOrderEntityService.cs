using GameFrameX.Application.Game.ServerManager;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Mail.GameMail.Dto;
using GameFrameX.Application.Game.Order.GameWebShopOrder.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using GameFrameX.Foundation.Http.Extension;
using GameFrameX.Foundation.Http.Normalization;
using GameFrameX.Foundation.Json;

#pragma warning disable CS8602, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Order.GameWebShopOrder;

/// <summary>
/// 页面商城订单
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameWebShopOrderEntityService : GenericCrudSelectService<Entity.Tenant.Game.TenantGameWebShopOrderEntity, AddTenantGameWebShopOrderEntityRequest, UpdateTenantGameWebShopOrderEntityRequest, TenantGameWebShopOrderEntityResponse, PageTenantGameWebShopOrderEntityRequest>
{
    private readonly SqlSugarRepository<SystemConfigEntity> _configService;
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameWebShopEntity> _webShopService;
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAreaEntity> _gameAreaService;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// 页面商城订单订单失败
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="configService">系统参数</param>
    /// <param name="webShopService"></param>
    /// <param name="gameAreaService">区服信息</param>
    /// <param name="httpClient"></param>
    /// <returns></returns>
    public TenantGameWebShopOrderEntityService(
        SqlSugarRepository<Entity.Tenant.Game.TenantGameWebShopOrderEntity> repository,
        SqlSugarRepository<SystemConfigEntity> configService,
        SqlSugarRepository<Entity.Tenant.Game.TenantGameWebShopEntity> webShopService,
        SqlSugarRepository<Entity.Tenant.Game.TenantGameAreaEntity> gameAreaService,
        HttpClient httpClient) : base(repository)
    {
        _configService = configService;
        _webShopService = webShopService;
        _gameAreaService = gameAreaService;
        _httpClient = httpClient;
    }

    /// <summary>
    /// 新增前：商城订单由客户端商城下单 / 支付回调链路产生，事实数据 AppId 必须来自可信写入链路；
    /// 禁止管理后台直接创建商城订单，避免写入 AppId=0 违反事实数据应用作用域铁律。
    /// </summary>
    protected override Task OnAddingAsync(AddTenantGameWebShopOrderEntityRequest input)
    {
        throw new Exception("商城订单由客户端商城下单链路产生，不支持通过管理后台直接创建");
    }

    /// <summary>
    /// 构建分页查询条件
    /// </summary>
    protected override ISugarQueryable<TenantGameWebShopOrderEntityResponse> BuildPageQuery(PageTenantGameWebShopOrderEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "网页商城订单");
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                                      u.Name.Contains(input.SearchKey.Trim())
                         )
                         .Where(u => u.AppId == appId)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Title), u => u.Name.Contains(input.Title.Trim()))
                         .Select<TenantGameWebShopOrderEntityResponse>();
    }

    /// <summary>
    /// 重新发送订单
    /// </summary>
    /// <param name="input"></param>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Redo")]
    public async Task RedoAsync(BaseUpdateRequest input)
    {
        var entity = await Repository.GetByIdAsync(input.Id);
        if (entity == null)
        {
            throw new Exception("该实例不存在");
        }

        if (entity.OrderStatus)
        {
            throw new Exception("已经补发过");
        }

        //获取服务器信息
        var gameAreaEntity = await _gameAreaService.AsQueryable().Filter(null, true).Where(u => u.IsDeleted == false).SingleAsync(u => u.Id == entity.AreaId);
        if (gameAreaEntity.ServerStatus != GameServerStatus.Online)
        {
            throw new Exception("服务器不在线");
        }

        //查询商品
        var shop = await _webShopService.GetByIdAsync(entity.GameShopId);
        var list = JsonHelper.Deserialize<List<GamePropItem>>(shop.ItemJson);

        // 批量预加载所有商品，避免 N+1 查询
        var goodsIds = list.Select(p => p.Id).Distinct().ToList();
        var goodsDict = (await Repository.AsSugarClient()
                                         .Queryable<Entity.Tenant.Game.TenantGameGoodsEntity>()
                                         .Where(m => goodsIds.Contains(m.Id))
                                         .ToListAsync())
            .ToDictionary(m => m.Id);

        var newList = new List<GamePropItem>();
        foreach (var propertyItem in list)
        {
            if (goodsDict.TryGetValue(propertyItem.Id, out var gameGoodsEntity))
            {
                propertyItem.Id = gameGoodsEntity.ConfigId;
                newList.Add(propertyItem);
            }
        }

        //获取邮件的标题
        var reqPostMailRequest = new PostMailRequest
        {
            Roles = new List<long>
            {
                entity.RoleId,
            },
            Title = (await _configService.GetFirstAsync(g => g.Code == "web_shop_title"))?.Value ?? string.Empty,
            SubTitle = (await _configService.GetFirstAsync(g => g.Code == "web_shop_sub_title"))?.Value ?? string.Empty,
            Content = (await _configService.GetFirstAsync(g => g.Code == "web_shop_content"))?.Value ?? string.Empty,
            PostType = false,
            ExpirationTime = TimerHelper.UnixTimeSeconds() + 30 * 24 * 60 * 60,
            Items = newList,
            IsTranslate = true,
        };
        var content = await _httpClient.PostJsonToStringAsync(GameServerEndpointUrlBuilder.Build(gameAreaEntity, GameServerEndpointKindEnum.Fulfillment, "post_mail"), reqPostMailRequest);
        var resultData = content.ToHttpJsonResult<PostMailResponse>();
        if (resultData.IsSuccess)
        {
            entity.OrderStatus = true;
            await Repository.AsSugarClient().Updateable(entity).ExecuteCommandAsync();
        }
        else
        {
            throw new Exception("邮件发送失败,重新尝试");
        }
    }
}
#pragma warning restore CS8602, CS8629