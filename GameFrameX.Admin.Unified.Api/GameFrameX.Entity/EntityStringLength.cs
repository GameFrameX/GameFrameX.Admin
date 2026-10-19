namespace GameFrameX.Entity;

/// <summary>
/// 实体层字符串长度常量。
/// 档位规则（C281，GFX-689）：系统最低字符串长度 256，旧档全量翻倍收敛为七档 256~16384；
/// 守护测试 <c>EntityStringLengthContractTests</c> 锁定最小档 ≥ 256，禁止新增小于 256 的档位。
/// </summary>
public static class EntityStringLength
{
    public const int Length256 = 256;
    public const int Length512 = 512;
    public const int Length1024 = 1024;
    public const int Length2048 = 2048;
    public const int Length4096 = 4096;
    public const int Length8192 = 8192;
    public const int Length16384 = 16384;
}
