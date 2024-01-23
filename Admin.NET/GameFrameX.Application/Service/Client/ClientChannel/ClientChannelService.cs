using GameFrameX.Application.Const;
using GameFrameX.Application.Service.Client.ClientChannel.Dto;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Extension;
using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Application.Service.Client.ClientChannel;

/// <summary>
/// 渠道服务
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class ClientChannelService : GetSelectBaseService<Entity.Client.ClientChannel>
{
    private readonly SqlSugarRepository<Entity.Client.ClientChannel> _rep;

    public ClientChannelService(SqlSugarRepository<Entity.Client.ClientChannel> rep) : base(rep)
    {
        _rep = rep;
    }

    /// <summary>
    /// 分页查询渠道
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<ClientChannelOutput>> Page(ClientChannelInput input)
    {
        var query = _rep.AsQueryable()
                .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                    u.Name.Contains(input.SearchKey.Trim())
                )
                .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                .Select<ClientChannelOutput>()
            ;
        query = query.OrderBuilder(input, "", "CreateTime");
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加渠道
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddClientChannelInput input)
    {
        var entity = input.Adapt<Entity.Client.ClientChannel>();
        await _rep.InsertAsync(entity);
    }

    /// <summary>
    /// 删除渠道
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteClientChannelInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity); //假删除
        //await _rep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 更新渠道
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateClientChannelInput input)
    {
        var entity = input.Adapt<Entity.Client.ClientChannel>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取渠道
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<Entity.Client.ClientChannel> Get([FromQuery] QueryByIdClientChannelInput input)
    {
        return await _rep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取渠道列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<ClientChannelOutput>> List([FromQuery] ClientChannelInput input)
    {
        return await _rep.AsQueryable().Select<ClientChannelOutput>().ToListAsync();
    }
}