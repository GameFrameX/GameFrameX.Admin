using GameFrameX.Core.Service.TenantMenuSync;

namespace GameFrameX.Core.Tests.TenantMenuSync;

/// <summary>
/// C353 T6：队列去重与解除占位语义（同租户至多一个待处理项；执行中可重新入队）。
/// </summary>
public class TenantMenuSyncQueueTests
{
    [Fact]
    public async Task TryEnqueue_DeduplicatesSameTenant()
    {
        var queue = new TenantMenuSyncQueue();
        using (var cts = new CancellationTokenSource())
        {
            try
            {
                Assert.True(queue.TryEnqueue(1001L, "scan:state-missing"));
                Assert.False(queue.TryEnqueue(1001L, "scan:catalog-lagged"));

                var item = await queue.ReadAsync(cts.Token);
                Assert.Equal(1001L, item.TenantId);
                Assert.Equal("scan:state-missing", item.Reason);
            }
            finally
            {
                queue.Complete();
                cts.Cancel();
            }
        }
    }

    [Fact]
    public async Task MarkDequeued_AllowsReEnqueue()
    {
        var queue = new TenantMenuSyncQueue();
        using (var cts = new CancellationTokenSource())
        {
            try
            {
                Assert.True(queue.TryEnqueue(1001L, "first"));
                var item = await queue.ReadAsync(cts.Token);
                queue.MarkDequeued(item.TenantId);
                Assert.True(queue.TryEnqueue(1001L, "second"));

                var next = await queue.ReadAsync(cts.Token);
                Assert.Equal("second", next.Reason);
            }
            finally
            {
                queue.Complete();
                cts.Cancel();
            }
        }
    }

    [Fact]
    public async Task ReadAsync_AfterComplete_DrainsRemainingThenThrows()
    {
        var queue = new TenantMenuSyncQueue();
        Assert.True(queue.TryEnqueue(1001L, "one"));
        queue.Complete();

        var item = await queue.ReadAsync(CancellationToken.None);
        Assert.Equal(1001L, item.TenantId);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => queue.ReadAsync(new CancellationToken(true)));
    }

    [Fact]
    public async Task ScheduleDelayed_EnqueuesAfterDelay()
    {
        var queue = new TenantMenuSyncQueue();
        using (var cts = new CancellationTokenSource(5000))
        {
            try
            {
                queue.ScheduleDelayed(1001L, "retry-backoff", TimeSpan.FromMilliseconds(50));

                var item = await queue.ReadAsync(cts.Token);
                Assert.Equal(1001L, item.TenantId);
                Assert.Equal("retry-backoff", item.Reason);
            }
            finally
            {
                queue.Complete();
                cts.Cancel();
            }
        }
    }
}
