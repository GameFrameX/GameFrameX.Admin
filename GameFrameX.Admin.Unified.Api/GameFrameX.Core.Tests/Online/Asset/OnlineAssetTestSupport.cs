using GameFrameX.Application.ServerCenter.Online;
using GameFrameX.Core.Const;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;
using GameFrameX.Foundation.Orm.Entity;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace GameFrameX.Core.Tests.Online.Asset;

// C340 / GFX-752（vault:C4）资产域 Service 单测共享基座：SQLite 专属 ConfigId 隔离 + IOnlineServerClient fake +
// GameApp/Area 作用域种子 + 当前用户上下文构造。各 Service 测试只关心断言本身，不重复构造基座。
//
// C283 命名对齐守护：本文件以「共享基座」单一类型 `OnlineAssetTestSupport` 立名；
// IOnlineServerClient fake 与 OnlineCall 记录类都内嵌为私有嵌套类型，避免平级多类型破规。
internal static class OnlineAssetTestSupport
{
    public const long A = 4001;
    public const long App100 = 100;
    public const long Area1001 = 2001;
    public const long Area1002 = 2002;

    internal sealed class FakeOnlineServerClient : IOnlineServerClient
    {
        private readonly Dictionary<string, Exception> _failures = new(StringComparer.Ordinal);

        public List<OnlineCall> Calls { get; } = new();

        public void FailOn(string action, Exception exception)
        {
            _failures[action] = exception;
        }

        public void SetPayload(object payload)
        {
            Payload = payload;
        }

        private object Payload { get; set; } = new { };

        public Task<TResponse> PostAsync<TResponse>(string action, object request, OnlineScopeContext scope) where TResponse : class
        {
            Calls.Add(new OnlineCall(action, request, scope.ServerId));
            if (_failures.TryGetValue(action, out var failure))
            {
                throw failure;
            }

            return Task.FromResult((TResponse)Payload);
        }

        public Task<TResponse> GetAsync<TResponse>(string action, OnlineScopeContext scope) where TResponse : class
        {
            throw new NotSupportedException("资产查询/命令不使用 GET 通道（Admin 侧详情端点虽为 GET，内部经 IOnlineServerClient.PostAsync 透传）");
        }
    }

    internal sealed record OnlineCall(string Action, object Request, long ServerId);

    public static SqlSugarRepository<T> Repository<T>(SqlSugarScopeProvider db) where T : EntityBase, new()
    {
        return new SqlSugarRepository<T> { Context = db, };
    }

    public static CurrentUserContext CreateUserContext(long id = A)
    {
        var identity = new global::System.Security.Claims.ClaimsIdentity("TestAuth");
        identity.AddClaim(new global::System.Security.Claims.Claim("Id", id.ToString()));
        var httpContext = new DefaultHttpContext { User = new global::System.Security.Claims.ClaimsPrincipal(identity), };
        var accessor = new HttpContextAccessor { HttpContext = httpContext, };
        return new CurrentUserContext(accessor);
    }

    public static GameAreaEntity NewArea(long id, long appId, long now)
    {
        return new GameAreaEntity
        {
            Id = id,
            Name = $"area-{id}",
            Description = string.Empty,
            HttpManageUrl = "https://game-server.test",
            TcpSocketUrl = "tcp://game-server.test",
            WebSocketUrl = "wss://game-server.test",
            ServerVersion = "1.0.0",
            ServerType = "default",
            LocalizationKey = "area.key",
            AllowedPlayerProfile = "{}",
            ServerEvents = "[]",
            AccessControls = "{}",
            ServerContainerId = string.Empty,
            ServerParams = "{}",
            AppVersion = "1.0.0",
            GmApiPrefix = "gm/api",
            ControlApiPrefix = "ctl",
            OnlineAdminApiPrefix = "online/admin",
            CreatedTime = now,
            IsDeleted = false,
        };
    }

    public static async Task SeedScopeAsync(SqlSugarScopeProvider db, bool secondArea = false)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await db.Insertable(NewArea(Area1001, App100, now)).ExecuteCommandAsync();
        if (secondArea)
        {
            await db.Insertable(NewArea(Area1002, App100, now)).ExecuteCommandAsync();
        }
    }
}
