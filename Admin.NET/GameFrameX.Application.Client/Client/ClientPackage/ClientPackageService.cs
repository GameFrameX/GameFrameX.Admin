using GameFrameX.Application.Base.Service;
using GameFrameX.Application.Client.Client.ClientPackage.Dto;
using GameFrameX.Core.Extension;

namespace GameFrameX.Application.Client.Client.ClientPackage;

/// <summary>
/// 包管理服务
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public class ClientPackageService : BaseSelectService<Entity.Client.ClientPackage>
{
    private readonly SqlSugarRepository<Entity.Client.ClientPackage> _repository;

    public ClientPackageService(SqlSugarRepository<Entity.Client.ClientPackage> repository) : base(repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 分页查询包管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<ClientPackageOutput>> Page(ClientPackageInput input)
    {
        var query = _repository.AsQueryable()
                .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                    u.Name.Contains(input.SearchKey.Trim())
                )
                .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                .Select<ClientPackageOutput>()
            ;
        query = query.OrderBuilder(input, "", "CreateTime");
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加包管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddClientPackageInput input)
    {
        var entity = input.Adapt<Entity.Client.ClientPackage>();
        await _repository.InsertAsync(entity);
    }

    /// <summary>
    /// 删除包管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteClientPackageInput input)
    {
        var entity = await _repository.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _repository.FakeDeleteAsync(entity); //假删除
        //await _rep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 更新包管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateClientPackageInput input)
    {
        var entity = input.Adapt<Entity.Client.ClientPackage>();
        await _repository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取包管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<Entity.Client.ClientPackage> Get([FromQuery] QueryByIdClientPackageInput input)
    {
        return await _repository.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取包管理列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<ClientPackageOutput>> List([FromQuery] ClientPackageInput input)
    {
        return await _repository.AsQueryable().Select<ClientPackageOutput>().ToListAsync();
    }
}