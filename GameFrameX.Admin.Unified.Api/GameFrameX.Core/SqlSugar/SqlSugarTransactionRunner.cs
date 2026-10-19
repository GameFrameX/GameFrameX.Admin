namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// SqlSugar 显式事务执行器。
/// </summary>
/// <remarks>
/// action filter 与内部服务方法共用本执行器；同一异步调用链中已存在事务时直接复用，避免内部方法提前提交外层业务事务。
/// </remarks>
public sealed class SqlSugarTransactionRunner
{
    private static readonly AsyncLocal<int> CurrentDepth = new();
    private readonly ISqlSugarClient _sqlSugarClient;

    /// <summary>
    /// 创建事务执行器。
    /// </summary>
    /// <param name="sqlSugarClient">当前 SqlSugar 客户端。</param>
    public SqlSugarTransactionRunner(ISqlSugarClient sqlSugarClient)
    {
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 在事务中执行异步操作，操作完成即提交，异常时回滚。
    /// </summary>
    /// <param name="action">需要放入事务边界的操作。</param>
    public async Task ExecuteAsync(Func<Task> action)
    {
        await ExecuteAsync(async () =>
        {
            await action();
            return true;
        });
    }

    /// <summary>
    /// 在事务中执行异步操作，返回 false 时回滚但不吞掉调用方后续异常处理。
    /// </summary>
    /// <param name="action">返回是否提交事务的操作。</param>
    public async Task ExecuteAsync(Func<Task<bool>> action)
    {
        if (CurrentDepth.Value > 0)
        {
            await action();
            return;
        }

        var tenant = _sqlSugarClient.AsTenant();
        tenant.BeginTran();
        CurrentDepth.Value++;
        try
        {
            var shouldCommit = await action();
            if (shouldCommit)
            {
                tenant.CommitTran();
            }
            else
            {
                tenant.RollbackTran();
            }
        }
        catch
        {
            tenant.RollbackTran();
            throw;
        }
        finally
        {
            CurrentDepth.Value--;
        }
    }
}