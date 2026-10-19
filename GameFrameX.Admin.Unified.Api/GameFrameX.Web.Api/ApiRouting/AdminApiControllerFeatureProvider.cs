using System.Reflection;
using GameFrameX.Core.Api;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace GameFrameX.Web.Api.ApiRouting;

public sealed class AdminApiControllerFeatureProvider : ControllerFeatureProvider
{
    protected override bool IsController(TypeInfo typeInfo)
    {
        if (typeof(IAdminApiController).IsAssignableFrom(typeInfo.AsType()))
        {
            return typeInfo.IsClass
                   && typeInfo.IsPublic
                   && !typeInfo.IsAbstract
                   && !typeInfo.ContainsGenericParameters;
        }

        return base.IsController(typeInfo);
    }
}