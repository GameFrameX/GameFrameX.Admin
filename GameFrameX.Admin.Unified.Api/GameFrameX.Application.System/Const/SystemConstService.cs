using GameFrameX.Application.System.Const.Dto;
using GameFrameX.Core.Authorization;
using GameFrameX.Core.Const;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Foundation.Orm.Attribute;

namespace GameFrameX.Application.System.Const;

/// <summary>
/// 系统常量服务
/// </summary>
[ApiDescriptionSettings(Order = 280)]
public class SystemConstService : IAdminApiController
{
    private readonly SystemCacheService _systemCacheService;

    public SystemConstService(SystemCacheService systemCacheService)
    {
        _systemCacheService = systemCacheService;
    }

    /// <summary>
    /// 获取所有常量列表
    /// </summary>
    /// <returns></returns>
    [AuthenticatedOnly]
    [AllowAnonymous]
    [DisplayName("获取所有常量列表")]
    [ApiDescriptionSettings(Name = "GetList")]
    public async Task<List<ConstResponse>> GetListAsync()
    {
        var key = $"{CacheConst.KeyConst}list";
        var constlist = _systemCacheService.Get<List<ConstResponse>>(key);
        if (constlist == null)
        {
            var typeList = GetConstAttributeList();
            constlist = typeList.Select(u => new ConstResponse
            {
                Name = u.CustomAttributes.ToList().FirstOrDefault()?.ConstructorArguments.ToList().FirstOrDefault().Value?.ToString() ?? u.Name,
                Code = u.Name,
                Data = GetDataAsync(Convert.ToString(u.Name)),
            }).ToList();
            _systemCacheService.Set(key, constlist);
        }

        return await Task.FromResult(constlist);
    }

    /// <summary>
    /// 根据类名获取常量数据
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("根据类名获取常量数据")]
    [ApiDescriptionSettings(Name = "GetData")]
    public async Task<List<ConstResponse>> GetDataAsync([Required] string typeName)
    {
        var key = $"{CacheConst.KeyConst}{typeName.ToUpper()}";
        var constlist = _systemCacheService.Get<List<ConstResponse>>(key);
        if (constlist == null)
        {
            var typeList = GetConstAttributeList();
            var type = typeList.FirstOrDefault(u => u.Name == typeName) ?? throw BusinessError.Oh("常量类型不存在");

            var isEnum = type.BaseType?.Name == "Enum";
            constlist = type.GetFields()
                            .Where(isEnum, u => u.FieldType.Name == typeName)
                            .Select(u => new ConstResponse
                            {
                                Name = u.Name,
                                Code = isEnum ? Convert.ToInt32(u.GetValue(null) ?? 0) : u.GetValue(null) ?? string.Empty,
                            }).ToList();
            _systemCacheService.Set(key, constlist);
        }

        return await Task.FromResult(constlist);
    }

    /// <summary>
    /// 获取常量特性类型列表
    /// </summary>
    /// <returns></returns>
    private List<Type> GetConstAttributeList()
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(u => u.GetTypes())
                        .Where(u => u.CustomAttributes.Any(c => c.AttributeType == typeof(ConstAttribute))).ToList();
    }
}