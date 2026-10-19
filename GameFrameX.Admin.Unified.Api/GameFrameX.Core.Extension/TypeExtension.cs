namespace GameFrameX.Core.Extension;

/// <summary>
/// 类型扩展
/// </summary>
public static partial class TypeExtension
{
    /// <summary>
    /// 判断类型是否实现某个泛型
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="generic">泛型类型</param>
    /// <returns>bool</returns>
    public static bool HasImplementedRawGeneric(this Type type, Type generic)
    {
        // 检查接口类型
        var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);
        if (isTheRawGenericType)
        {
            return true;
        }

        // 检查类型
        var currentType = type;
        while (currentType != null && currentType != typeof(object))
        {
            isTheRawGenericType = IsTheRawGenericType(currentType);
            if (isTheRawGenericType)
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;

        // 判断逻辑
        bool IsTheRawGenericType(Type type)
        {
            return generic == (type.IsGenericType ? type.GetGenericTypeDefinition() : type);
        }
    }
}