using GameFrameX.Application.System.Dictionary.Dto;
using GameFrameX.Entity.Global;

namespace GameFrameX.Application.System.Dictionary;

/// <summary>
/// 系统字典值服务
/// </summary>
[ApiDescriptionSettings(Order = 420)]
public class GlobalDictionaryDataService : IAdminApiController
{
    private readonly SqlSugarRepository<GlobalDictionaryDataEntity> _globalDictionaryDataRepository;

    public GlobalDictionaryDataService(SqlSugarRepository<GlobalDictionaryDataEntity> globalDictionaryDataRepository)
    {
        _globalDictionaryDataRepository = globalDictionaryDataRepository;
    }

    /// <summary>
    /// 获取字典值分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取字典值分页列表")]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<GlobalDictionaryDataResponse>> PageAsync(PageDictionaryDataRequest input)
    {
        var code = input.Code?.Trim() ?? string.Empty;
        var value = input.Value?.Trim() ?? string.Empty;
        return await _globalDictionaryDataRepository.AsQueryable()
                                       .Where(u => u.DictionaryTypeId == input.DictionaryTypeId)
                                       .WhereIF(!string.IsNullOrEmpty(code), u => u.Code.Contains(code))
                                       .WhereIF(!string.IsNullOrEmpty(value), u => u.Value.Contains(value))
                                       .OrderBy(u => new { u.OrderNo, u.Code, })
                                       .Select<GlobalDictionaryDataResponse>()
                                       .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取字典值列表
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取字典值列表")]
    [ApiDescriptionSettings(Name = "GetList")]
    public async Task<List<GlobalDictionaryDataResponse>> GetListAsync([FromQuery] GetDataDictionaryDataRequest input)
    {
        return await GetDictionaryDataListByDictionaryTypeIdAsync(input.DictionaryTypeId);
    }

    /// <summary>
    /// 增加字典值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add")]
    [HttpPost]
    [DisplayName("增加字典值")]
    public async Task AddDictionaryDataAsync(AddDictionaryDataRequest input)
    {
        var isExist = await _globalDictionaryDataRepository.IsAnyAsync(u => u.Code == input.Code && u.DictionaryTypeId == input.DictionaryTypeId);
        if (isExist)
        {
            throw BusinessError.Oh(DictionaryErrorCode.DictionaryDataAlreadyExists);
        }

        await _globalDictionaryDataRepository.InsertAsync(input.Adapt<GlobalDictionaryDataEntity>());
    }

    /// <summary>
    /// 更新字典值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新字典值")]
    public async Task UpdateDictionaryDataAsync(UpdateDictionaryDataRequest input)
    {
        var isExist = await _globalDictionaryDataRepository.IsAnyAsync(u => u.Id == input.Id);
        if (!isExist)
        {
            throw BusinessError.Oh(DictionaryErrorCode.DictionaryDataNotFound);
        }

        isExist = await _globalDictionaryDataRepository.IsAnyAsync(u => u.Code == input.Code && u.DictionaryTypeId == input.DictionaryTypeId && u.Id != input.Id);
        if (isExist)
        {
            throw BusinessError.Oh(DictionaryErrorCode.DictionaryDataAlreadyExists);
        }

        await _globalDictionaryDataRepository.Context.UpdateWithOptimisticLockAsync(input.Adapt<GlobalDictionaryDataEntity>());
    }

    /// <summary>
    /// 删除字典值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除字典值")]
    public async Task DeleteDictionaryDataAsync(DeleteDictionaryDataRequest input)
    {
        var dictionaryData = await _globalDictionaryDataRepository.GetFirstAsync(u => u.Id == input.Id);
        if (dictionaryData == null)
        {
            throw BusinessError.Oh(DictionaryErrorCode.DictionaryDataNotFound);
        }

        await _globalDictionaryDataRepository.DeleteAsync(dictionaryData);
    }

    /// <summary>
    /// 获取字典值详情
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取字典值详情")]
    [ApiDescriptionSettings(Name = "GetDetail")]
    public async Task<GlobalDictionaryDataResponse> GetDetailAsync([FromQuery] DictionaryDataRequest input)
    {
        return (await _globalDictionaryDataRepository.GetFirstAsync(u => u.Id == input.Id))?.Adapt<GlobalDictionaryDataResponse>();
    }

    /// <summary>
    /// 修改字典值状态
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("修改字典值状态")]
    [ApiDescriptionSettings(Name = "SetStatus")]
    public async Task SetStatusAsync(DictionaryDataRequest input)
    {
        var dictionaryData = await _globalDictionaryDataRepository.GetFirstAsync(u => u.Id == input.Id);
        if (dictionaryData == null)
        {
            throw BusinessError.Oh(DictionaryErrorCode.DictionaryDataNotFound);
        }

        dictionaryData.IsEnabled = input.IsEnabled;
        await _globalDictionaryDataRepository.Context.UpdateWithOptimisticLockAsync(dictionaryData);
    }

    /// <summary>
    /// 根据字典类型Id获取字典值集合
    /// </summary>
    /// <param name="dictionaryTypeId"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<GlobalDictionaryDataResponse>> GetDictionaryDataListByDictionaryTypeIdAsync(long dictionaryTypeId)
    {
        return await _globalDictionaryDataRepository.AsQueryable()
                                       .Where(u => u.DictionaryTypeId == dictionaryTypeId)
                                       .OrderBy(u => new { u.OrderNo, u.Code, })
                                       .Select<GlobalDictionaryDataResponse>()
                                       .ToListAsync();
    }

    /// <summary>
    /// 根据字典类型编码获取字典值集合
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("根据字典类型编码获取字典值集合")]
    [ApiDescriptionSettings(Name = "GetDataList")]
    public async Task<List<GlobalDictionaryDataResponse>> GetDataListAsync(string code)
    {
        var list = await _globalDictionaryDataRepository.Context.Queryable<GlobalDictionaryTypeEntity>()
                                           .LeftJoin<GlobalDictionaryDataEntity>((u, a) => u.Id == a.DictionaryTypeId)
                                           .Where((u, a) => u.Code == code && u.IsEnabled == true && a.IsEnabled == true)
                                           .OrderBy((u, a) => new { a.OrderNo, a.Code, })
                                           .Select((u, a) => a).ToListAsync();
        return list.Adapt<List<GlobalDictionaryDataResponse>>();
    }

    /// <summary>
    /// 根据查询条件获取字典值集合
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("根据查询条件获取字典值集合")]
    [ApiDescriptionSettings(Name = "GetDataList")]
    public async Task<List<GlobalDictionaryDataResponse>> GetDataListAsync([FromQuery] QueryDictionaryDataRequest input)
    {
        var status = input.IsEnabled;
        var list = await _globalDictionaryDataRepository.Context.Queryable<GlobalDictionaryTypeEntity>()
                                           .LeftJoin<GlobalDictionaryDataEntity>((u, a) => u.Id == a.DictionaryTypeId)
                                           .Where((u, a) => u.Code == input.Code)
                                           .WhereIF(status.HasValue, (u, a) => a.IsEnabled == (status ?? false))
                                           .OrderBy((u, a) => new { a.OrderNo, a.Code, })
                                           .Select((u, a) => a).ToListAsync();
        return list.Adapt<List<GlobalDictionaryDataResponse>>();
    }

    /// <summary>
    /// 根据字典类型Id删除字典值
    /// </summary>
    /// <param name="dictionaryTypeId"></param>
    /// <returns></returns>
    [NonAction]
    [ApiDescriptionSettings(Name = "DeleteDictionaryData")]
    public async Task DeleteDictionaryDataAsync(long dictionaryTypeId)
    {
        await _globalDictionaryDataRepository.DeleteAsync(u => u.DictionaryTypeId == dictionaryTypeId);
    }
}