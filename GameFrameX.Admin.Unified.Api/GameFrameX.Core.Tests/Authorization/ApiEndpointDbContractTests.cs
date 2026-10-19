using GameFrameX.Entity.Global;

namespace GameFrameX.Core.Tests.Authorization;

public sealed class ApiEndpointDbContractTests
{
    [Theory]
    [InlineData(typeof(GlobalApiEndpointEntity), "ux_global_api_endpoint_method_route")]
    [InlineData(typeof(GlobalMenuEndpointEntity), "ux_global_menu_endpoint_menu_endpoint")]
    public void EndpointPermissionEntities_DeclareUniqueIndexes(Type entityType, string indexName)
    {
        var indexes = entityType.GetCustomAttributes(false)
                                .Where(u => u.GetType().Name.Contains("SugarIndex", StringComparison.Ordinal))
                                .Select(u => new
                                {
                                    Name = u.GetType().GetProperty("IndexName")?.GetValue(u) as string,
                                    IsUnique = u.GetType().GetProperty("IsUnique")?.GetValue(u) as bool? == true,
                                })
                                .ToArray();

        Assert.Contains(indexes, u => u.Name == indexName && u.IsUnique);
    }
}