using GameFrameX.Core.Service.Base;
using Microsoft.AspNetCore.Mvc.Routing;

namespace GameFrameX.Core.Tests.Authorization;

public sealed class CrudActionRouteContractTests
{
    [Theory]
    [InlineData(typeof(GenericCrudService<,,,,>), "PageAsync", CrudActionRoutes.Page, "POST")]
    [InlineData(typeof(GenericCrudService<,,,,>), "AddAsync", CrudActionRoutes.Add, "POST")]
    [InlineData(typeof(GenericCrudService<,,,,>), "UpdateAsync", CrudActionRoutes.Update, "POST")]
    [InlineData(typeof(GenericCrudService<,,,,>), "GetAsync", CrudActionRoutes.Detail, "GET")]
    [InlineData(typeof(BaseService<>), "DeleteAsync", CrudActionRoutes.Delete, "POST")]
    [InlineData(typeof(GenericCrudSelectService<,,,,>), "SelectAsync", CrudActionRoutes.Select, "POST")]
    [InlineData(typeof(GenericCrudSelectService<,,,,>), "SelectStringAsync", CrudActionRoutes.SelectString, "POST")]
    public void CrudBaseActions_DeclareStableRouteTemplates(Type serviceType, string methodName, string routeTemplate, string httpMethod)
    {
        var method = serviceType.GetMethod(methodName);
        Assert.NotNull(method);

        var attribute = method.GetCustomAttributes(typeof(HttpMethodAttribute), true)
                              .Cast<HttpMethodAttribute>()
                              .Single();

        Assert.Equal(routeTemplate, attribute.Template);
        Assert.Contains(httpMethod, attribute.HttpMethods);
    }
}