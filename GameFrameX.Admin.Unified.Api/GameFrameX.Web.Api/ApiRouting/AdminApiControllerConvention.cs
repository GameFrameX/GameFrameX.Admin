using GameFrameX.Core.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace GameFrameX.Web.Api.ApiRouting;

public sealed class AdminApiControllerConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            if (!typeof(IAdminApiController).IsAssignableFrom(controller.ControllerType.AsType()))
            {
                continue;
            }

            var controllerSegment = AdminApiRouteRules.GetControllerRouteSegment(controller.ControllerType.AsType());
            controller.ControllerName = controllerSegment;
            controller.ApiExplorer.GroupName = AdminApiRouteRules.GetApiExplorerGroupName(controller.ControllerType.AsType());

            foreach (var action in controller.Actions)
            {
                ApplyActionRoute(controllerSegment, action);
            }
        }
    }

    private static void ApplyActionRoute(string controllerSegment, ActionModel action)
    {
        var httpMethodAttributes = action.Attributes.OfType<HttpMethodAttribute>().ToArray();
        action.Selectors.Clear();

        if (httpMethodAttributes.Length == 0)
        {
            var method = AdminApiRouteRules.GetDefaultHttpMethod(action.ActionMethod);
            var actionSegment = AdminApiRouteRules.GetActionRouteSegment(action.ActionMethod, null);
            var routeParameterName = GetRouteParameterName(action, method);
            var routeTemplate = AdminApiRouteRules.CombineRoute(controllerSegment, actionSegment, null);
            if (!string.IsNullOrEmpty(routeParameterName))
            {
                routeTemplate += $"/{{{routeParameterName}}}";
            }

            AddSelector(action, new[] { method, }, routeTemplate);
            ApplyParameterBinding(action, method, routeParameterName);
            return;
        }

        foreach (var attribute in httpMethodAttributes)
        {
            var actionSegment = AdminApiRouteRules.GetActionRouteSegment(action.ActionMethod, attribute);
            var methods = attribute.HttpMethods.Any() ? attribute.HttpMethods.ToArray() : new[] { "POST", };
            AddSelector(action, methods, AdminApiRouteRules.CombineRoute(controllerSegment, actionSegment, attribute.Template));
            ApplyParameterBinding(action, methods[0], null);
        }
    }

    private static void AddSelector(ActionModel action, IReadOnlyList<string> httpMethods, string routeTemplate)
    {
        var selector = new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(routeTemplate)),
        };
        foreach (var attribute in action.Controller.Attributes.Concat(action.Attributes))
        {
            selector.EndpointMetadata.Add(attribute);
        }

        selector.ActionConstraints.Add(new HttpMethodActionConstraint(httpMethods));
        selector.EndpointMetadata.Add(new HttpMethodMetadata(httpMethods));
        action.Selectors.Add(selector);
    }

    private static void ApplyParameterBinding(ActionModel action, string httpMethod, string? routeParameterName)
    {
        foreach (var parameter in action.Parameters)
        {
            if (parameter.BindingInfo?.BindingSource != null)
            {
                continue;
            }

            if (string.Equals(parameter.ParameterName, routeParameterName, StringComparison.OrdinalIgnoreCase))
            {
                parameter.BindingInfo = new BindingInfo { BindingSource = BindingSource.Path, };
                continue;
            }

            if (IsFormFile(parameter.ParameterInfo.ParameterType))
            {
                parameter.BindingInfo = new BindingInfo { BindingSource = BindingSource.FormFile, };
                continue;
            }

            parameter.BindingInfo = new BindingInfo
            {
                BindingSource = IsBodyParameter(parameter.ParameterInfo.ParameterType, httpMethod)
                                    ? BindingSource.Body
                                    : BindingSource.Query,
            };
        }
    }

    private static bool IsBodyParameter(Type type, string httpMethod)
    {
        if (!string.Equals(httpMethod, "POST", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(httpMethod, "PUT", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(httpMethod, "PATCH", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var targetType = Nullable.GetUnderlyingType(type) ?? type;
        return targetType != typeof(string)
               && !targetType.IsPrimitive
               && !targetType.IsEnum
               && targetType != typeof(decimal)
               && targetType != typeof(Guid)
               && targetType != typeof(DateTime)
               && targetType != typeof(DateTimeOffset);
    }

    private static bool IsFormFile(Type type)
    {
        if (typeof(IFormFile).IsAssignableFrom(type))
        {
            return true;
        }

        return type.IsGenericType
               && typeof(IEnumerable<IFormFile>).IsAssignableFrom(type);
    }

    private static string? GetRouteParameterName(ActionModel action, string httpMethod)
    {
        if (!string.Equals(httpMethod, "GET", StringComparison.OrdinalIgnoreCase) || action.Parameters.Count != 1)
        {
            return null;
        }

        var parameter = action.Parameters[0];
        if (parameter.BindingInfo?.BindingSource != null || !IsRouteParameter(parameter.ParameterInfo.ParameterType))
        {
            return null;
        }

        return parameter.ParameterName;
    }

    private static bool IsRouteParameter(Type type)
    {
        var targetType = Nullable.GetUnderlyingType(type) ?? type;
        return targetType == typeof(string)
               || targetType.IsPrimitive
               || targetType.IsEnum
               || targetType == typeof(decimal)
               || targetType == typeof(Guid);
    }
}