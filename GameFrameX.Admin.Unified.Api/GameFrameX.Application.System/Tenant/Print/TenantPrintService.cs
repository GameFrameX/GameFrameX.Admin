using GameFrameX.Application.System.Tenant.Print.Dto;

namespace GameFrameX.Application.System.Tenant.Print;

/// <summary>
/// 系统打印模板服务
/// </summary>
[ApiDescriptionSettings(Order = 305)]
public class TenantPrintService : IAdminApiController
{
    private readonly SqlSugarRepository<TenantPrintEntity> _systemPrintRepository;

    public TenantPrintService(SqlSugarRepository<TenantPrintEntity> systemPrintRepository)
    {
        _systemPrintRepository = systemPrintRepository;
    }

    /// <summary>
    /// 获取打印模板列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取打印模板列表")]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<TenantPrintResponse>> PageAsync(PagePrintRequest input)
    {
        return await _systemPrintRepository.AsQueryable()
                                    .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name))
                                    .OrderBy(u => u.OrderNo)
                                    .Select<TenantPrintResponse>()
                                    .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加打印模板
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add")]
    [HttpPost]
    [DisplayName("增加打印模板")]
    public async Task AddPrintAsync(AddPrintRequest input)
    {
        var isExist = await _systemPrintRepository.IsAnyAsync(u => u.Name == input.Name);
        if (isExist)
        {
            throw BusinessError.Oh(PrintErrorCode.PrintTemplateAlreadyExists);
        }

        await _systemPrintRepository.InsertAsync(input.Adapt<TenantPrintEntity>());
    }

    /// <summary>
    /// 更新打印模板
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新打印模板")]
    public async Task UpdatePrintAsync(UpdatePrintRequest input)
    {
        var isExist = await _systemPrintRepository.IsAnyAsync(u => u.Name == input.Name && u.Id != input.Id);
        if (isExist)
        {
            throw BusinessError.Oh(PrintErrorCode.PrintTemplateAlreadyExists);
        }

        await _systemPrintRepository.AsUpdateable(input.Adapt<TenantPrintEntity>()).IgnoreColumns(true).ExecuteCommandWithOptLockAsync(true);
    }

    /// <summary>
    /// 删除打印模板
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除打印模板")]
    public async Task DeletePrintAsync(DeletePrintRequest input)
    {
        await _systemPrintRepository.DeleteAsync(u => u.Id == input.Id);
    }
}