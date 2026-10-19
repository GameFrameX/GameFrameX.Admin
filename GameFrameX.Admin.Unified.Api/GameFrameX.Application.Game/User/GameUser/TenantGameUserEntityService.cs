using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.User.GameUser;
using GameFrameX.Application.Game.User.GameUser.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Extensions;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.User.GameUser;

/// <summary>
/// 游戏账号服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 111)]
public class TenantGameUserEntityService : GenericCrudSelectService<Entity.Tenant.Game.TenantGameUserEntity, AddTenantGameUserEntityByMailRequest, UpdateTenantGameUserEntityRequest, TenantGameUserEntityResponse, PageTenantGameUserEntityRequest>
{
    private readonly TenantGameUserPasswordService _tenantGameUserPasswordService;

    public TenantGameUserEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGameUserEntity> repository,
        TenantGameUserPasswordService tenantGameUserPasswordService) : base(repository)
    {
        _tenantGameUserPasswordService = tenantGameUserPasswordService;
    }

    /// <summary>
    /// 构建分页查询条件
    /// </summary>
    protected override ISugarQueryable<TenantGameUserEntityResponse> BuildPageQuery(PageTenantGameUserEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "游戏账号");
        return Repository.AsQueryable()
                         .WhereIF(input.Id > 0, u => u.Id == input.Id)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.UserName), u => u.UserName.Contains(input.UserName.Trim()))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .WhereIF(input.Status.HasValue, u => u.Status == input.Status.Value)
                         .Where(u => SqlFunc.Subqueryable<Entity.Tenant.Game.TenantGameRoleEntity>()
                                            .Where(r => r.AppId == appId && r.UserId == u.Id)
                                            .Any())
                         .Select<TenantGameUserEntityResponse>();
    }

    /// <summary>
    /// 通过邮箱增加账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "addByMail")]
    public async Task<TenantGameUserEntityResponse> AddByMailAsync(AddTenantGameUserEntityByMailRequest input)
    {
        //判断邮箱是否已经存在
        var entity = await Repository.GetFirstAsync(x => x.Email == input.Email);
        if (entity.IsNotNull())
        {
            throw new Exception("该邮箱已经被使用");
        }

        entity = input.Adapt<Entity.Tenant.Game.TenantGameUserEntity>();
        entity.EmailPassword = _tenantGameUserPasswordService.Hash(input.EmailPassword);
        entity.UserName = input.Email;
        entity.Password = _tenantGameUserPasswordService.Hash(input.Password);
        await Repository.InsertAsync(entity);

        return entity.Adapt<TenantGameUserEntityResponse>();
    }

    /// <summary>
    /// 封禁游戏账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Ban")]
    public async Task BanAsync(DeleteTenantGameUserEntityRequest input)
    {
        await EnsureUserBelongsToCurrentAppAsync(input);
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
    public async Task UnsealAsync(DeleteTenantGameUserEntityRequest input)
    {
        await EnsureUserBelongsToCurrentAppAsync(input);
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        entity.Status = GameUserStatus.Active;
        await Repository.AsUpdateable(entity).IgnoreColumns(true).ExecuteCommandWithOptLockAsync(true);
    }

    /// <summary>
    /// 游戏账号是租户级账号；应用工作台操作账号前，必须确认该账号在当前应用下至少存在一个角色。
    /// </summary>
    private async Task EnsureUserBelongsToCurrentAppAsync(DeleteTenantGameUserEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "游戏账号");
        var exists = await Repository.AsSugarClient()
                                     .Queryable<Entity.Tenant.Game.TenantGameRoleEntity>()
                                     .AnyAsync(u => u.AppId == appId && u.UserId == input.Id);
        if (!exists)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }
    }
}
#pragma warning restore CS8602, CS8604, CS8629