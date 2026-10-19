using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace GameFrameX.Web.Api.ApiRouting;

/// <summary>
/// Admin API 端点元数据读取规则。
/// </summary>
public static class AdminApiEndpointMetadata
{
    /// <summary>
    /// 读取端点 HTTP method；显式路由约定写入 <see cref="IHttpMethodMetadata"/>，旧 Attribute metadata 继续兼容。
    /// </summary>
    public static IReadOnlyList<string> GetHttpMethods(IEnumerable<object> endpointMetadata)
    {
        var methods = endpointMetadata.OfType<IHttpMethodMetadata>()
                                      .SelectMany(m => m.HttpMethods ?? Array.Empty<string>())
                                      .Concat(endpointMetadata.OfType<HttpMethodAttribute>().SelectMany(m => m.HttpMethods ?? Array.Empty<string>()))
                                      .Where(m => !string.IsNullOrWhiteSpace(m))
                                      .Distinct(StringComparer.OrdinalIgnoreCase)
                                      .ToList();

        if (methods.Count == 0)
        {
            methods.Add("POST");
        }

        return methods;
    }
}