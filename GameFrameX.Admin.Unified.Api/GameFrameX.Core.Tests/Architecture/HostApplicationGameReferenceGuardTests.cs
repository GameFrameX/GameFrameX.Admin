using System.Reflection;

namespace GameFrameX.Core.Tests.Architecture;

// RemoteConfig 并入 Application.Game（游戏业务域聚合）后的引用边界守护测试。
// Client.Api / Hub.Api 两个 host 对 GameFrameX.Application.Game 命名空间的可触达面实行
// fail-closed 白名单：host 程序集内所有类型（Controller / Service / 闭包等）的构造函数参数、
// 字段、属性、方法参数与返回值中出现的 Application.Game 类型，必须逐一登记在下方白名单内。
// 扩展 Client/Hub 对 Application.Game 的可触达面，需在本测试白名单登记并说明理由；
// 未登记的新引用直接失败，防止 Game 域业务类型向 host 层无序扩散。
public sealed class HostApplicationGameReferenceGuardTests
{
    // 白名单 = 2026-08-21 RemoteConfig 并入批次合并后现状基线（首跑采集）
    private static readonly HashSet<string> ClientApiAllowedReferences = new(StringComparer.Ordinal)
    {
        // GameUserController 封禁/踢下线联动处罚执行（DI 注入接口）
        "GameFrameX.Application.Game.Penalty.GamePenalty.ITenantGamePenaltyEnforcementService",
        // GameUserController 用户密码校验（C214 Argon2id 路径）
        "GameFrameX.Application.Game.User.GameUser.TenantGameUserPasswordService",
        // GameOrderController 订单发货（Client.Api 支付回调后履约）
        "GameFrameX.Application.Game.Order.GameOrder.TenantGameOrderShipService",
        // C345 灰度判定原语唯一化：RemoteConfigRolloutMatcher 委托 Application.Game 共享实现，
        // 与活动/功能开关投放共用同一套取模 + 白名单旁路语义（原 Client.Api 私有副本已收敛）。
        "GameFrameX.Application.Game.App.Rollout.StableDeviceRolloutMatcher",
        // C347 活动/功能开关客户端通道：GameFeatureSwitchController 消费 C345 冻结的共享投放解析器，
        // Result/EffectiveItem 为解析器返回值与元素类型（控制器内做 Unix 秒 wire 转换）。
        "GameFrameX.Application.Game.App.FeatureSwitch.FeatureSwitchDeliveryResolver",
        "GameFrameX.Application.Game.App.FeatureSwitch.FeatureSwitchDeliveryResult",
        "GameFrameX.Application.Game.App.FeatureSwitch.FeatureSwitchEffectiveItem",
    };

    private static readonly HashSet<string> HubApiAllowedReferences = new(StringComparer.Ordinal)
    {
        // 原区服增删停三个 DTO（物理文件在 Hub.Api/Dto/GameArea/）曾命名空间挂在 Application.Game 根，
        // 归位到 GameFrameX.Hub.Api.Dto.GameArea 后，Hub.Api 对 Application.Game 零可触达；新增引用须登记并说明理由。
        // C347 活动/功能开关服务器同步通道：FeatureSwitchSyncController 消费 C345 冻结的共享投放解析器，
        // Result/EffectiveItem 为解析器返回值与元素类型（控制器内做 Unix 秒 wire 转换）。
        "GameFrameX.Application.Game.App.FeatureSwitch.FeatureSwitchDeliveryResolver",
        "GameFrameX.Application.Game.App.FeatureSwitch.FeatureSwitchDeliveryResult",
        "GameFrameX.Application.Game.App.FeatureSwitch.FeatureSwitchEffectiveItem",
    };

    [Fact]
    public void ClientApi_ReferencesToApplicationGame_AreWhitelisted()
    {
        var hostAssembly = Assembly.Load("GameFrameX.Client.Api");
        var actual = CollectApplicationGameReferences(hostAssembly);

        var unexpected = actual.Except(ClientApiAllowedReferences).Order(StringComparer.Ordinal).ToList();
        Assert.True(unexpected.Count == 0,
                    "GameFrameX.Client.Api 出现未登记的 Application.Game 类型引用。扩展 Client/Hub 对 Application.Game 的可触达面，" +
                    "需在本测试白名单登记并说明理由。未登记引用:" + Environment.NewLine + string.Join(Environment.NewLine, unexpected));
    }

    [Fact]
    public void HubApi_ReferencesToApplicationGame_AreWhitelisted()
    {
        var hostAssembly = Assembly.Load("GameFrameX.Hub.Api");
        var actual = CollectApplicationGameReferences(hostAssembly);

        var unexpected = actual.Except(HubApiAllowedReferences).Order(StringComparer.Ordinal).ToList();
        Assert.True(unexpected.Count == 0,
                    "GameFrameX.Hub.Api 出现未登记的 Application.Game 类型引用。扩展 Client/Hub 对 Application.Game 的可触达面，" +
                    "需在本测试白名单登记并说明理由。未登记引用:" + Environment.NewLine + string.Join(Environment.NewLine, unexpected));
    }

    /// <summary>枚举 host 程序集所有类型声明成员（构造函数参数 / 字段 / 属性 / 方法参数与返回值）中出现的 Application.Game 类型全名。</summary>
    private static HashSet<string> CollectApplicationGameReferences(Assembly hostAssembly)
    {
        var references = new HashSet<string>(StringComparer.Ordinal);
        const BindingFlags memberFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (var type in hostAssembly.GetTypes())
        {
            foreach (var constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                foreach (var parameter in constructor.GetParameters())
                {
                    Collect(parameter.ParameterType, references);
                }
            }

            foreach (var field in type.GetFields(memberFlags))
            {
                Collect(field.FieldType, references);
            }

            foreach (var property in type.GetProperties(memberFlags))
            {
                Collect(property.PropertyType, references);
            }

            foreach (var method in type.GetMethods(memberFlags))
            {
                if (method.IsSpecialName)
                {
                    continue;
                }

                Collect(method.ReturnType, references);
                foreach (var parameter in method.GetParameters())
                {
                    Collect(parameter.ParameterType, references);
                }
            }
        }

        return references;
    }

    /// <summary>递归展开数组 / 指针 / byref 元素类型与泛型参数位置，收集命名空间以 GameFrameX.Application.Game 开头的类型。</summary>
    private static void Collect(Type? type, HashSet<string> references)
    {
        if (type is null)
        {
            return;
        }

        if (type.IsArray || type.IsPointer || type.IsByRef)
        {
            Collect(type.GetElementType(), references);
            return;
        }

        if (type.IsGenericParameter)
        {
            return;
        }

        if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            foreach (var argument in type.GetGenericArguments())
            {
                Collect(argument, references);
            }

            return;
        }

        if (type.Namespace is not null && type.Namespace.StartsWith("GameFrameX.Application.Game", StringComparison.Ordinal))
        {
            references.Add(type.FullName ?? type.Namespace);
        }
    }
}
