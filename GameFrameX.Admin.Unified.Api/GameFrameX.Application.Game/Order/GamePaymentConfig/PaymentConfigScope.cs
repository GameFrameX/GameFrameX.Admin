#pragma warning disable CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Order.GamePaymentConfig;

public sealed record PaymentConfigScope(long? GameAppId, long? PackageId, long GameAppIdNormalized, long PackageIdNormalized);