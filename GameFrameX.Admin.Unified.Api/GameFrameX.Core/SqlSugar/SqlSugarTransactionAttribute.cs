namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// SqlSugar 请求事务过滤器。
/// </summary>
/// <remarks>
/// 标记在动态 API action 或普通 MVC action 上时，使用当前 <see cref="ISqlSugarClient"/> 的多租户事务入口包裹整次 action 执行。
/// 这是 C38 迁移后的显式事务边界，替代 历史框架 隐式工作单元注册。
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class SqlSugarTransactionAttribute : Attribute, IAsyncActionFilter, IOrderedFilter
{
    /// <summary>
    /// 事务过滤器执行顺序。保持在普通 action filter 外层，避免业务 filter 在事务外写入数据库。
    /// </summary>
    public int Order { get; set; } = -1000;

    /// <summary>
    /// 执行 action 并在成功时提交事务，异常时回滚事务。
    /// </summary>
    /// <param name="context">当前 action 执行上下文。</param>
    /// <param name="next">后续 action 委托。</param>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var transactionRunner = context.HttpContext.RequestServices.GetRequiredService<SqlSugarTransactionRunner>();

        await transactionRunner.ExecuteAsync(async () =>
        {
            var executedContext = await next();
            return executedContext.Exception == null || executedContext.ExceptionHandled;
        });
    }
}