using GameFrameX.Application.Platform.Job;
using GameFrameX.Application.ServerCenter;
using GameFrameX.Application.NoticeCenter.GameMail.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Http.Extension;
using GameFrameX.Foundation.Http.Normalization;
using GameFrameX.Foundation.Json;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.NoticeCenter.GameMail;

/// <summary>
/// 游戏邮件服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameMailService
    : GenericCrudService<Entity.Tenant.Game.TenantGameMailEntity,
        AddTenantGameMailEntityRequest, UpdateTenantGameMailEntityRequest,
        GameMailResponse, PageTenantGameMailEntityRequest>
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAreaEntity> _gameAreaEntityRepository;
    private readonly HttpClient _httpClientFactory;

    /// <summary>
    ///
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="systemTenantGameAreaEntity"></param>
    /// <param name="httpClientFactory"></param>
    public GameMailService(SqlSugarRepository<Entity.Tenant.Game.TenantGameMailEntity> repository, SqlSugarRepository<Entity.Tenant.Game.TenantGameAreaEntity> systemTenantGameAreaEntity, HttpClient httpClientFactory)
        : base(repository)
    {
        _httpClientFactory = httpClientFactory;
        _gameAreaEntityRepository = systemTenantGameAreaEntity;
    }

    protected override ISugarQueryable<GameMailResponse> BuildPageQuery(PageTenantGameMailEntityRequest input)
    {
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                                      u.Title.Contains(input.SearchKey.Trim()))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Title), u => u.Title.Contains(input.Title.Trim()))
                         .WhereIF(input.IsAllPost.HasValue, u => u.IsAllPost == input.IsAllPost.Value)
                         .WhereIF(input.Status.HasValue, u => u.Status == input.Status.Value)
                         .WhereIF(input.MailType.HasValue, u => u.MailType == input.MailType.Value)
                         .Select<GameMailResponse>();
    }

    /// <summary>
    /// 新增前校验角色并解析附件道具。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameMailEntityRequest input)
    {

        if (!input.IsAllPost)
        {
            var playerIdList = JsonHelper.Deserialize<List<string>>(input.PlayerIdListJson);
            var listAsync = await Repository.AsSugarClient().Queryable<Entity.Tenant.Game.TenantGameRoleEntity>().Where(m => playerIdList.Contains(m.Id.ToString())).ToListAsync();
            if (listAsync.Count != playerIdList.Count)
            {
                throw new Exception("有玩家的ID不存在");
            }
        }

        if (!input.AttachmentsJson.IsNullOrEmpty())
        {
            var list = JsonHelper.Deserialize<List<GamePropItem>>(input.AttachmentsJson);
            input.AttachmentsJson = string.Empty;

            // 批量预加载所有商品，避免 N+1 查询
            var goodsIds = list.Select(p => p.Id).Distinct().ToList();
            var goodsDict = (await Repository.AsSugarClient()
                                             .Queryable<Entity.Tenant.Game.TenantGameGoodsEntity>()
                                             .Where(m => goodsIds.Contains(m.Id))
                                             .ToListAsync())
                .ToDictionary(m => m.Id);

            var newList = new List<GamePropItem>();
            foreach (var attachmentItem in list)
            {
                if (goodsDict.TryGetValue(attachmentItem.Id, out var gameGoodsEntity))
                {
                    attachmentItem.ItemId = gameGoodsEntity.ConfigId;
                    newList.Add(attachmentItem);
                }
            }

            input.AttachmentsJson = JsonHelper.Serialize(newList, EmbeddedJsonOptions.Instance);
        }
    }

    /// <summary>
    /// 更新前校验记录存在。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameMailEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }
    }

    /// <summary>
    /// 发送游戏邮件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Send")]
    public async Task SendAsync(BaseIdRequest input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        if (entity.Status == GameMailStatus.Invalid)
        {
            throw new Exception("邮件已过期了,无法发送");
        }

        if (entity.Status == GameMailStatus.Sent)
        {
            throw new Exception("邮件已发送过了。不要重复发送");
        }

        List<long> playerIdArray;

        // 如果不是全区发的获取角色列表
        if (entity.IsAllPost)
        {
            playerIdArray = Repository.AsSugarClient().Queryable<Entity.Tenant.Game.TenantGameRoleEntity>().Where(u => entity.AreaId == u.AreaId).ToList(m => m.Id);
        }
        else
        {
            var playerIdList = JsonHelper.Deserialize<List<string>>(entity.PlayerIdListJson);
            playerIdArray = Repository.AsSugarClient().Queryable<Entity.Tenant.Game.TenantGameRoleEntity>().Where(u => entity.AreaId == u.AreaId && playerIdList.Contains(u.Id.ToString())).ToList(m => m.Id);
        }

        //获取所有的符合要求的邮件
        var expirationTimestamp = entity.ExpirationTime;
        var reqPostMailRequest = new PostMailRequest
        {
            Roles = playerIdArray,
            Title = entity.Title,
            SubTitle = entity.SubTitle ?? "",
            Content = entity.Body,
            PostType = entity.IsAllPost,
            ExpirationTime = expirationTimestamp,
            MailType = entity.MailType,
            IsTranslate = entity.IsTranslate ?? false,
        };
        if (!entity.AttachmentsJson.IsNullOrEmpty())
        {
            // 发送邮件
            var items = JsonHelper.Deserialize<List<GamePropItem>>(entity.AttachmentsJson);
            reqPostMailRequest.Items = items;
            foreach (var gamePropItemEntity in reqPostMailRequest.Items)
            {
                gamePropItemEntity.Id = gamePropItemEntity.ItemId;
            }
        }

        var gameAreaEntity = await _gameAreaEntityRepository.GetSingleAsync(u => u.Id == entity.AreaId);
        var content = await _httpClientFactory.PostJsonToStringAsync(GameServerEndpointUrlBuilder.Build(gameAreaEntity, GameServerEndpointKindEnum.Fulfillment, "post_mail"), reqPostMailRequest);
        var resultData = content.ToHttpJsonResult<PostMailResponse>();
        if (resultData.IsSuccess)
        {
            entity.Status = GameMailStatus.Sent;
            await Repository.AsSugarClient().Updateable(entity).ExecuteCommandAsync();
            return;
        }

        throw new Exception("邮件已发送失败" + resultData.Message);
    }
}
#pragma warning restore CS8602, CS8604, CS8629