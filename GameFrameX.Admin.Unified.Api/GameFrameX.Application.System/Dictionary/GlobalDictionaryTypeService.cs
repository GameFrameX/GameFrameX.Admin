using GameFrameX.Application.System.Dictionary.Dto;
using GameFrameX.Core.Authorization;
using GameFrameX.Entity.Global;

namespace GameFrameX.Application.System.Dictionary;

/// <summary>
/// 系统字典类型服务
/// </summary>
[ApiDescriptionSettings(Order = 430)]
public class GlobalDictionaryTypeService : IAdminApiController
{
    private readonly SqlSugarRepository<GlobalDictionaryTypeEntity> _globalDictionaryTypeRepository;
    private readonly GlobalDictionaryDataService _globalDictionaryDataService;

    public GlobalDictionaryTypeService(SqlSugarRepository<GlobalDictionaryTypeEntity> globalDictionaryTypeRepository,
        GlobalDictionaryDataService globalDictionaryDataService)
    {
        _globalDictionaryTypeRepository = globalDictionaryTypeRepository;
        _globalDictionaryDataService = globalDictionaryDataService;
    }

    /// <summary>
    /// 获取字典类型分页列表
    /// </summary>
    /// <returns></returns>
    [HttpPost(CrudActionRoutes.Page)]
    [ApiDescriptionSettings(Name = "Page")]
    [DisplayName("获取字典类型分页列表")]
    public async Task<SqlSugarPagedList<GlobalDictionaryTypeResponse>> PageAsync(PageDictionaryTypeRequest input)
    {
        var code = input.Code?.Trim() ?? string.Empty;
        var name = input.Name?.Trim() ?? string.Empty;
        return await _globalDictionaryTypeRepository.AsQueryable()
                                       .WhereIF(!string.IsNullOrEmpty(code), u => u.Code.Contains(code))
                                       .WhereIF(!string.IsNullOrEmpty(name), u => u.Name.Contains(name))
                                       .OrderBy(u => new { u.OrderNo, u.Code, })
                                       .Select<GlobalDictionaryTypeResponse>()
                                       .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取字典类型列表
    /// </summary>
    /// <returns></returns>
    [HttpGet(CrudActionRoutes.List)]
    [ApiDescriptionSettings(Name = "GetList")]
    [DisplayName("获取字典类型列表")]
    public async Task<List<GlobalDictionaryTypeResponse>> GetListAsync()
    {
        return await _globalDictionaryTypeRepository.AsQueryable()
                                       .OrderBy(u => new { u.OrderNo, u.Code, })
                                       .Select<GlobalDictionaryTypeResponse>()
                                       .ToListAsync();
    }

    /// <summary>
    /// 获取字典类型-值列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("dataList")]
    [ApiDescriptionSettings(Name = "GetDataList")]
    [AllowAnonymous]
    [DisplayName("获取字典类型-值列表")]
    public async Task<List<GlobalDictionaryDataResponse>> GetDataListAsync([FromQuery] GetDataDictionaryTypeRequest input)
    {
        var dictionaryType = await _globalDictionaryTypeRepository.GetFirstAsync(u => u.Code == input.Code);
        if (dictionaryType == null)
        {
            throw BusinessError.Oh(DictionaryErrorCode.DictionaryTypeNotFound);
        }

        return await _globalDictionaryDataService.GetDictionaryDataListByDictionaryTypeIdAsync(dictionaryType.Id);
    }

    /// <summary>
    /// 添加字典类型
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost(CrudActionRoutes.Add)]
    [ApiDescriptionSettings(Name = "Add")]
    [DisplayName("添加字典类型")]
    public async Task AddDictionaryTypeAsync(AddDictionaryTypeRequest input)
    {
        var isExist = await _globalDictionaryTypeRepository.IsAnyAsync(u => u.Code == input.Code);
        if (isExist)
        {
            throw BusinessError.Oh(DictionaryErrorCode.DictionaryTypeAlreadyExists);
        }

        await _globalDictionaryTypeRepository.InsertAsync(input.Adapt<GlobalDictionaryTypeEntity>());
    }

    /// <summary>
    /// 更新字典类型
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost(CrudActionRoutes.Update)]
    [ApiDescriptionSettings(Name = "Update")]
    [DisplayName("更新字典类型")]
    public async Task UpdateDictionaryTypeAsync(UpdateDictionaryTypeRequest input)
    {
        var isExist = await _globalDictionaryTypeRepository.IsAnyAsync(u => u.Id == input.Id);
        if (!isExist)
        {
            throw BusinessError.Oh(DictionaryErrorCode.DictionaryTypeNotFound);
        }

        isExist = await _globalDictionaryTypeRepository.IsAnyAsync(u => u.Code == input.Code && u.Id != input.Id);
        if (isExist)
        {
            throw BusinessError.Oh(DictionaryErrorCode.DictionaryTypeAlreadyExists);
        }

        await _globalDictionaryTypeRepository.Context.UpdateWithOptimisticLockAsync(input.Adapt<GlobalDictionaryTypeEntity>());
    }

    /// <summary>
    /// 删除字典类型
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost(CrudActionRoutes.Delete)]
    [ApiDescriptionSettings(Name = "Delete")]
    [DisplayName("删除字典类型")]
    public async Task DeleteDictionaryTypeAsync(DeleteDictionaryTypeRequest input)
    {
        var dictionaryType = await _globalDictionaryTypeRepository.GetFirstAsync(u => u.Id == input.Id);
        if (dictionaryType == null)
        {
            throw BusinessError.Oh(DictionaryErrorCode.DictionaryTypeNotFound);
        }

        // 删除字典值
        await _globalDictionaryTypeRepository.DeleteAsync(dictionaryType);
        await _globalDictionaryDataService.DeleteDictionaryDataAsync(input.Id);
    }

    /// <summary>
    /// 获取字典类型详情
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet(CrudActionRoutes.Detail)]
    [ApiDescriptionSettings(Name = "GetDetail")]
    [DisplayName("获取字典类型详情")]
    public async Task<GlobalDictionaryTypeResponse> GetDetailAsync([FromQuery] DictionaryTypeRequest input)
    {
        return (await _globalDictionaryTypeRepository.GetFirstAsync(u => u.Id == input.Id))?.Adapt<GlobalDictionaryTypeResponse>();
    }

    /// <summary>
    /// 修改字典类型状态
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost(CrudActionRoutes.SetStatus)]
    [ApiDescriptionSettings(Name = "SetStatus")]
    [DisplayName("修改字典类型状态")]
    public async Task SetStatusAsync(DictionaryTypeRequest input)
    {
        var dictionaryType = await _globalDictionaryTypeRepository.GetFirstAsync(u => u.Id == input.Id);
        if (dictionaryType == null)
        {
            throw BusinessError.Oh(DictionaryErrorCode.DictionaryTypeNotFound);
        }

        dictionaryType.IsEnabled = input.IsEnabled;
        await _globalDictionaryTypeRepository.Context.UpdateWithOptimisticLockAsync(dictionaryType);
    }

    /// <summary>
    /// 获取所有字典集合
    /// </summary>
    /// <returns></returns>
    [HttpGet("allDictionaryList")]
    [ApiDescriptionSettings(Name = "GetAllDictionaryList")]
    [AuthenticatedOnly]
    [AllowAnonymous]
    [DisplayName("获取所有字典集合")]
    public async Task<List<GlobalDictionaryTypeResponse>> GetAllDictionaryListAsync()
    {
        var dictionaryList = await _globalDictionaryTypeRepository.AsQueryable()
                                               .Includes(u => u.Children)
                                               .OrderBy(u => new { u.OrderNo, u.Code, })
                                               .ToListAsync();
        // 字典数据项排序
        dictionaryList.ForEach(u => u.Children = u.Children.OrderBy(c => c.OrderNo).ThenBy(c => c.Code).ToList());
        return dictionaryList.Adapt<List<GlobalDictionaryTypeResponse>>();
    }
}