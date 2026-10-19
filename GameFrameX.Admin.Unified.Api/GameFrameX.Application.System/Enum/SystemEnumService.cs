using System.Reflection;
using GameFrameX.Application.System.Enum.Dto;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.System.Enum;

/// <summary>
/// 系统枚举服务。公开的枚举集合由 <see cref="AdminEnumAttribute"/> 显式标记控制，
/// 取代历史的 <c>Configuration/Enum.json</c> 程序集白名单。
/// </summary>
[ApiDescriptionSettings(Order = 275)]
public class SystemEnumService : IAdminApiController
{
    /// <summary>
    /// 获取所有枚举类型（仅返回带 <see cref="AdminEnumAttribute"/> 标记的枚举）。
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("获取所有枚举类型")]
    public List<EnumTypeResponse> GetEnumTypeList()
    {
        var enumTypeList = ProjectTypeCatalog.EffectiveTypes
                                             .Where(u => u.IsEnum && u.GetCustomAttribute<AdminEnumAttribute>() != null)
                                             .OrderBy(u => u.Name)
                                             .ToList();

        var result = new List<EnumTypeResponse>();
        foreach (var item in enumTypeList)
        {
            result.Add(GetEnumDescription(item));
        }

        return result;
    }

    private EnumTypeResponse GetEnumDescription(Type type)
    {
        var description = type.Name;
        var attrs = type.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attrs.Any())
        {
            var att = ((DescriptionAttribute[])attrs)[0];
            description = att.Description;
        }

        return new EnumTypeResponse
        {
            TypeDescribe = description,
            TypeName = type.Name,
            TypeRemark = description,
        };
    }

    /// <summary>
    /// 通过枚举类型获取枚举值集合。仅允许读取带 <see cref="AdminEnumAttribute"/> 标记的枚举，
    /// 未标记的枚举按 <see cref="ProtocolErrorCode.NoEnumType"/> 拒绝，避免按名绕过公开边界。
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("通过枚举类型获取枚举值集合")]
    public List<EnumItem> GetEnumDataList([FromQuery] EnumRequest input)
    {
        var enumType = ProjectTypeCatalog.EffectiveTypes.FirstOrDefault(u => u.IsEnum && u.Name == input.EnumName && u.GetCustomAttribute<AdminEnumAttribute>() != null);
        if (enumType is not { IsEnum: true, })
        {
            throw BusinessError.Oh(ProtocolErrorCode.NoEnumType);
        }

        return enumType.EnumToList();
    }

    /// <summary>
    /// 通过实体的字段名获取相关枚举值集合（目前仅支持枚举类型）。
    /// 字段枚举类型必须带 <see cref="AdminEnumAttribute"/> 标记，否则按 <see cref="ProtocolErrorCode.NoEnumType"/> 拒绝。
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("通过实体的字段名获取相关枚举值集合")]
    public List<EnumItem> GetEnumDataListByField([FromQuery] QueryEnumDataRequest input)
    {
        // 获取实体类型属性
        var entityType = ProjectTypeCatalog.EffectiveTypes.FirstOrDefault(u => u.Name == input.EntityName) ?? throw BusinessError.Oh(CommonErrorCode.NotFound);

        // 获取字段类型
        var fieldType = entityType.GetProperties().FirstOrDefault(u => u.Name == input.FieldName)?.PropertyType;
        if (fieldType is not { IsEnum: true, } || fieldType.GetCustomAttribute<AdminEnumAttribute>() == null)
        {
            throw BusinessError.Oh(ProtocolErrorCode.NoEnumType);
        }

        return fieldType.EnumToList();
    }
}