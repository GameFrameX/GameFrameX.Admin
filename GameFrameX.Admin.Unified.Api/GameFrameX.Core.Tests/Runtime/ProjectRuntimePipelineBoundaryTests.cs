using GameFrameX.Core.Extension;
using GameFrameX.Core.Extension.Json;

namespace GameFrameX.Core.Tests.Runtime;

public sealed class ProjectRuntimePipelineBoundaryTests
{
    [Fact]
    public void ProjectJsonSerializer_KeepsAdminJsonContract()
    {
        var json = ProjectJsonSerializer.Default.Serialize(new RuntimeJsonSample
        {
            UserId = 123,
            CreatedTime = new DateTime(2026, 7, 27, 8, 9, 10, DateTimeKind.Local),
        });

        Assert.Contains("\"UserId\":\"123\"", json);
        Assert.Contains("\"CreatedTime\":\"2026-07-27 08:09:10\"", json);

        var roundTripped = ProjectJsonSerializer.Default.Deserialize<RuntimeJsonSample>("{\"userId\":\"456\",\"createdTime\":\"2026-07-27 08:09:10\"}");
        Assert.NotNull(roundTripped);
        Assert.Equal(456, roundTripped!.UserId);
    }

    [Fact]
    public void ObjectExtension_UsesProjectJsonSerializerContract()
    {
        var json = new RuntimeJsonSample { UserId = 789, }.ToJson();

        Assert.Contains("\"UserId\":\"789\"", json);
        Assert.Equal(789, json.ToObject<RuntimeJsonSample>().UserId);
    }

    [Fact]
    public void StartupAndLogging_UseProjectRuntimeSetupEntrypoints()
    {
        var root = FindRepoRoot();
        var webStartup = File.ReadAllText(Path.Combine(root, "GameFrameX.Web.Api/Startup.cs"));
        var loggingSetup = File.ReadAllText(Path.Combine(root, "GameFrameX.Core/Logging/LoggingSetup.cs"));

        // C204（GFX-633）：Application.Game/Startup.cs 已删除，调度注册收敛到 Web.Core/Startup.cs 一处。
        // Admin 与 Game 两套调度入口都在 Web.Core 启动管线注册，且均使用项目封装而非历史框架遗留 .AddSchedule(。
        Assert.Contains("AddAdminSchedule", webStartup);
        Assert.Contains("AddGameSchedule", webStartup);
        Assert.DoesNotContain(".AddSchedule(", webStartup);
        Assert.Contains("AddProjectDatabaseOperationLogging", loggingSetup);
        Assert.Contains("AddProjectElasticSearchLogging", loggingSetup);
        Assert.DoesNotContain("AddDatabaseLogging<", loggingSetup);
    }

    [Fact]
    public void ObjectExtensionSource_DoesNotCallLegacyJsonStaticFacade()
    {
        var root = FindRepoRoot();
        var source = File.ReadAllText(Path.Combine(root, "GameFrameX.Core.Extension/ObjectExtension.cs"));

        Assert.Contains("ProjectJsonSerializer.Default", source);
        Assert.DoesNotContain("JSON.GetJsonSerializer()", source);
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "GameFrameX.Admin.Unified.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Cannot find GameFrameX.Admin.Unified.slnx from test working directory.");
    }

    private sealed class RuntimeJsonSample
    {
        public long UserId { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}