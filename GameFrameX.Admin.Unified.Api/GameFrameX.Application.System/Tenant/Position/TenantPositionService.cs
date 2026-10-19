using GameFrameX.Application.System.Tenant.Position.Dto;
using GameFrameX.Core.Authorization;
using GameFrameX.Core.Service.User;

namespace GameFrameX.Application.System.Tenant.Position;

/// <summary>
/// 系统职位服务
/// </summary>
[ApiDescriptionSettings(Order = 460)]
public class TenantPositionService : IAdminApiController
{
    private readonly CurrentUserContext _currentUserContext;
    private readonly SqlSugarRepository<TenantPositionEntity> _systemPosRepository;
    private readonly TenantUserExtensionOrganizationService _systemUserExtOrganizationService;
    private readonly IServiceProvider _serviceProvider;

    public TenantPositionService(CurrentUserContext currentUserContext,
        SqlSugarRepository<TenantPositionEntity> systemPosRepository,
        TenantUserExtensionOrganizationService systemUserExtOrganizationService,
        IServiceProvider serviceProvider)
    {
        _currentUserContext = currentUserContext;
        _systemPosRepository = systemPosRepository;
        _systemUserExtOrganizationService = systemUserExtOrganizationService;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 获取职位列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AuthenticatedOnly]
    [DisplayName("获取职位列表")]
    [ApiDescriptionSettings(Name = "GetList")]
    public async Task<List<TenantPositionResponse>> GetListAsync([FromQuery] PosRequest input)
    {
        return await _systemPosRepository.AsQueryable()
                                  .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name))
                                  .WhereIF(!string.IsNullOrWhiteSpace(input.Code), u => u.Code.Contains(input.Code))
                                  .OrderBy(u => u.OrderNo)
                                  .Select<TenantPositionResponse>()
                                  .ToListAsync();
    }

    /// <summary>
    /// 增加职位
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add")]
    [HttpPost]
    [DisplayName("增加职位")]
    public async Task AddPosAsync(AddPosRequest input)
    {
        if (await _systemPosRepository.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code))
        {
            throw BusinessError.Oh(PositionErrorCode.PositionAlreadyExists);
        }

        await _systemPosRepository.InsertAsync(input.Adapt<TenantPositionEntity>());
    }

    /// <summary>
    /// 更新职位
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新职位")]
    public async Task UpdatePosAsync(UpdatePosRequest input)
    {
        if (await _systemPosRepository.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code && u.Id != input.Id))
        {
            throw BusinessError.Oh(PositionErrorCode.PositionAlreadyExists);
        }

        var systemPos = await _systemPosRepository.GetByIdAsync(input.Id) ?? throw BusinessError.Oh(PositionErrorCode.PositionNotFound);
        if (!_currentUserContext.SuperAdmin && systemPos.CreatedId != _currentUserContext.UserId)
        {
            throw BusinessError.Oh(PositionErrorCode.NoPermissionForPosition);
        }

        await _systemPosRepository.AsUpdateable(input.Adapt<TenantPositionEntity>()).IgnoreColumns(true).ExecuteCommandWithOptLockAsync(true);
    }

    /// <summary>
    /// 删除职位
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除职位")]
    public async Task DeletePosAsync(DeletePosRequest input)
    {
        var systemPos = await _systemPosRepository.GetFirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh(PositionErrorCode.PositionNotFound);
        if (!_currentUserContext.SuperAdmin && systemPos.CreatedId != _currentUserContext.UserId)
        {
            throw BusinessError.Oh(PositionErrorCode.NoPermissionForPosition);
        }

        // 该职位下是否有用户
        var hasPosEmp = await _serviceProvider.GetRequiredService<SqlSugarRepository<TenantUserEntity>>()
                                              .IsAnyAsync(u => u.PosId == input.Id);
        if (hasPosEmp)
        {
            throw BusinessError.Oh(PositionErrorCode.PositionHasUsersDeleteForbidden);
        }

        // 该附属职位下是否有用户
        var hasExtPosEmp = await _systemUserExtOrganizationService.HasUserPosAsync(input.Id);
        if (hasExtPosEmp)
        {
            throw BusinessError.Oh(PositionErrorCode.PositionHasUsersDeleteForbidden);
        }

        await _systemPosRepository.DeleteAsync(u => u.Id == input.Id);
    }
}