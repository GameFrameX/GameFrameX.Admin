using GameFrameX.Application.Base.Service;
using GameFrameX.Application.Client.Client.ClientLanguage.Dto;
using GameFrameX.Core.Extension;

namespace GameFrameX.Application.Client.Client.ClientLanguage;
/// <summary>
/// 语言管理服务
/// </summary>
[ApiDescriptionSettings( Order = 100)]
public class ClientLanguageService : BaseSelectService<Entity.Client.ClientLanguage>
{
    private readonly SqlSugarRepository<Entity.Client.ClientLanguage> _rep;
    public ClientLanguageService(SqlSugarRepository<Entity.Client.ClientLanguage> rep):base(rep)
    {
        _rep = rep;
    }

    /// <summary>
    /// 分页查询语言管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<ClientLanguageOutput>> Page(ClientLanguageInput input)
    {
        var query= _rep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                u.Name.Contains(input.SearchKey.Trim())
            )
            .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
            .Select<ClientLanguageOutput>()
;
        query = query.OrderBuilder(input, "", "CreateTime");
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加语言管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddClientLanguageInput input)
    {
        var entity = input.Adapt<Entity.Client.ClientLanguage>();
        await _rep.InsertAsync(entity);
    }

    /// <summary>
    /// 删除语言管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteClientLanguageInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
        //await _rep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 更新语言管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateClientLanguageInput input)
    {
        var entity = input.Adapt<Entity.Client.ClientLanguage>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取语言管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<Entity.Client.ClientLanguage> Get([FromQuery] QueryByIdClientLanguageInput input)
    {
        return await _rep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取语言管理列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<ClientLanguageOutput>> List([FromQuery] ClientLanguageInput input)
    {
        return await _rep.AsQueryable().Select<ClientLanguageOutput>().ToListAsync();
    }





}

