using GameFrameX.Application.PlayerCenter.GameUser;
using GameFrameX.Application.PlayerCenter.GameUser.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Extensions;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.PlayerCenter.GameUser;

/// <summary>
/// 游戏账号服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 111)]
public class GameUserService : GenericCrudSelectService<Entity.Business.Game.GameUserEntity, AddGameUserEntityByMailRequest, UpdateGameUserEntityRequest, GameUserResponse, PageGameUserEntityRequest>
{
    private readonly GameUserPasswordService _gameUserPasswordService;

    public GameUserService(SqlSugarRepository<Entity.Business.Game.GameUserEntity> repository,
        GameUserPasswordService gameUserPasswordService) : base(repository)
    {
        _gameUserPasswordService = gameUserPasswordService;
    }

    /// <summary>
    /// 构建分页查询条件
    /// </summary>
    protected override ISugarQueryable<GameUserResponse> BuildPageQuery(PageGameUserEntityRequest input)
    {
        return Repository.AsQueryable()
                         .WhereIF(input.Id > 0, u => u.Id == input.Id)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.UserName), u => u.UserName.Contains(input.UserName.Trim()))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .WhereIF(input.Status.HasValue, u => u.Status == input.Status.Value)
                         .Select<GameUserResponse>();
    }

    /// <summary>
    /// 通用新增入口：与邮箱新增共用同一请求 DTO，直接委托，避免明文密码入库（D-29：绑定列已可空，无空值插入问题）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("新增账号")]
    public override async Task<GameUserResponse> AddAsync(AddGameUserEntityByMailRequest input)
    {
        return await AddByMailAsync(input);
    }

    /// <summary>
    /// 通过邮箱增加账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "addByMail")]
    public async Task<GameUserResponse> AddByMailAsync(AddGameUserEntityByMailRequest input)
    {
        //判断邮箱是否已经存在
        var entity = await Repository.GetFirstAsync(x => x.Email == input.Email);
        if (entity.IsNotNull())
        {
            throw new Exception("该邮箱已经被使用");
        }

        entity = input.Adapt<Entity.Business.Game.GameUserEntity>();
        // Name 为基类必填非空列，请求未携带时兜底（D-29：其余绑定列已可空，无需空串兜底）
        entity.Name ??= string.Empty;
        entity.EmailPassword = _gameUserPasswordService.Hash(input.EmailPassword);
        entity.UserName = input.Email;
        entity.Password = _gameUserPasswordService.Hash(input.Password);
        await Repository.InsertAsync(entity);

        return entity.Adapt<GameUserResponse>();
    }

    /// <summary>
    /// 封禁游戏账号
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Ban")]
    public async Task BanAsync(DeleteGameUserEntityRequest input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        entity.Status = GameUserStatus.Banned;
        await Repository.AsUpdateable(entity).IgnoreColumns(true).ExecuteCommandWithOptLockAsync(true);
    }

    /// <summary>
    /// 解除封禁游戏账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Unseal")]
    public async Task UnsealAsync(DeleteGameUserEntityRequest input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        entity.Status = GameUserStatus.Active;
        await Repository.AsUpdateable(entity).IgnoreColumns(true).ExecuteCommandWithOptLockAsync(true);
    }

}
#pragma warning restore CS8602, CS8604, CS8629