using System.Reflection;
using GameFrameX.Application.Game.Order.GameOrderFulfillment;
using GameFrameX.Application.Game.Order.GameOrderFulfillment.Dto;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.GameOrder;

/// <summary>
/// 权益发货账本 Admin API 契约自检（C120 / GFX-122）。
/// 编译期即 failing-without-implementation（服务/DTO 类型不存在则编译失败）；运行期断言路由方法与过滤字段不漂移。
/// </summary>
public class TenantGameOrderFulfillmentEntityServiceContractTests
{
    [Fact]
    public void Service_Exposes_Required_Read_And_Management_Actions()
    {
        var methodNames = typeof(TenantGameOrderFulfillmentEntityService)
                          .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                          .Select(m => m.Name)
                          .ToHashSet();

        // 只读端点
        Assert.Contains("PageAsync", methodNames);
        Assert.Contains("GetAsync", methodNames);
        Assert.Contains("ExportAsync", methodNames);
        // 高风险动作端点
        Assert.Contains("RefundAsync", methodNames);
        Assert.Contains("CompensateAsync", methodNames);
        Assert.Contains("ResendAsync", methodNames);
    }

    [Fact]
    public void PageInput_Carries_Required_Filter_Fields()
    {
        var props = typeof(PageTenantGameOrderFulfillmentEntityRequest)
                    .GetProperties()
                    .Select(p => p.Name)
                    .ToHashSet();

        Assert.Contains("AppId", props);
        Assert.Contains("OrderId", props);
        Assert.Contains("RoleId", props);
        Assert.Contains("AreaId", props);
        Assert.Contains("PayChannel", props);
        Assert.Contains("ProviderTransactionId", props);
        Assert.Contains("Type", props);
        Assert.Contains("Status", props);
        Assert.Contains("Sku", props);
        Assert.Contains("AccountId", props);
        Assert.Contains("OperatorId", props);
        Assert.Contains("CreateTimeRange", props);
    }

    [Fact]
    public void PageInput_Status_Is_FulfillmentStatusEnum_Not_OrderStatusEnum()
    {
        var statusProperty = typeof(PageTenantGameOrderFulfillmentEntityRequest).GetProperty("Status");
        Assert.NotNull(statusProperty);
        Assert.Equal(typeof(FulfillmentStatusEnum?), statusProperty!.PropertyType);
    }

    [Fact]
    public void ActionInput_Carries_Id_And_Reason()
    {
        var props = typeof(FulfillmentActionRequest)
                    .GetProperties()
                    .Select(p => p.Name)
                    .ToHashSet();

        Assert.Contains("Id", props);
        Assert.Contains("Reason", props);
    }

    [Fact]
    public void ActionEndpoints_Return_ObjectOutput_Not_StringEnvelope()
    {
        // C223 / GFX-644：Admin 端点 result 统一对象契约。三个高风险动作端点返回 Task<FulfillmentActionResponse>，
        // 失败抛 BusinessException 走 AdminResultFilter；不再暴露 HttpJsonResultData<string> 双重信封。
        var expected = typeof(Task<FulfillmentActionResponse>);
        foreach (var methodName in new[] { "RefundAsync", "CompensateAsync", "ResendAsync", })
        {
            var method = typeof(TenantGameOrderFulfillmentEntityService)
                .GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(method);
            Assert.Equal(expected, method!.ReturnType);
        }
    }

    [Fact]
    public void Service_Exposes_No_HttpJsonResultData_Endpoints()
    {
        // C223：Admin 服务边界内不再有 HttpJsonResultData 返回签名（对外 string 生态收敛在 Client.Api / TenantGameOrderShipService）。
        var offender = typeof(TenantGameOrderFulfillmentEntityService)
                       .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                       .Where(m => m.DeclaringType == typeof(TenantGameOrderFulfillmentEntityService))
                       .FirstOrDefault(m => m.ReturnType.IsGenericType
                                            && m.ReturnType.GetGenericArguments()[0].Name.StartsWith("HttpJsonResultData"));
        Assert.Null(offender);
    }

    [Fact]
    public void ActionOutput_Carries_OrderId_And_Type()
    {
        var props = typeof(FulfillmentActionResponse)
                    .GetProperties()
                    .Select(p => p.Name)
                    .ToHashSet();

        Assert.Contains("OrderId", props);
        Assert.Contains("Type", props);
        Assert.Equal(typeof(FulfillmentTypeEnum), typeof(FulfillmentActionResponse).GetProperty("Type")!.PropertyType);
    }
}