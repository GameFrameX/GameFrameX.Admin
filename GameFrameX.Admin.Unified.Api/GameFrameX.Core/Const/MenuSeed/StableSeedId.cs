using System.Security.Cryptography;

namespace GameFrameX.Core.Const.MenuSeed;

/// <summary>
/// 根据稳定业务键派生种子主键，避免菜单和角色菜单授权在源码中维护魔法数字 ID。
/// <para>派生口径（2026-09-15 紧凑短 ID 决策）：SHA256 首个 Int64 取 46 位掩码（≤14 位数字），
/// 与运行时紧凑雪花同一数量级；全量种子 ~8800 键碰撞概率 ~10^-6 量级，且碰撞会在种子插入期
/// 以主键冲突显式报错（非静默）。掩码随决策冻结：改掩码 = 存量种子 Id 全变、授权引用断裂。</para>
/// </summary>
public static class StableSeedId
{
    /// <summary>
    /// 种子 Id 派生掩码位数（46 位，产出 ≤14 位数字，决策冻结不可更改）。
    /// </summary>
    private const long SeedIdMask = (1L << 46) - 1;

    /// <summary>
    /// 派生菜单主键。
    /// </summary>
    public static long Menu(string key)
    {
        return PositiveHash("menu", key);
    }

    /// <summary>
    /// 派生角色菜单主键。
    /// </summary>
    public static long RoleMenu(string roleCode, string menuKey)
    {
        return PositiveHash("role-menu", roleCode + "|" + menuKey);
    }

    private static long PositiveHash(string scope, string key)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(scope + ":" + key));
        var value = BitConverter.ToInt64(bytes, 0) & SeedIdMask;
        return value == 0 ? PositiveHash(scope, key + "|fallback") : value;
    }
}