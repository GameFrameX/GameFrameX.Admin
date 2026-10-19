using GameFrameX.Application.Game.User.GameUser;

namespace GameFrameX.Core.Tests.GameUserPassword;

/// <summary>
/// TenantGameUserPasswordService 单测：纯 Argon2id 哈希 / 校验，非法格式（含历史 PBKDF2/MD5）一律 false。
/// <para>常量时间比对由 Foundation <c>PasswordHashHelper.Verify</c> 算法层保证，此处不重复测时间维度。</para>
/// <para>C198 → C215 迁移：内部从 PBKDF2（AppSecretHasher）换为 Argon2id（Foundation.PasswordHashHelper）。
/// 接口签名（<c>Hash(string) / Verify(string,string)</c>）保持不变，19 处调用方零改动。</para>
/// </summary>
public class TenantGameUserPasswordServiceTests
{
    private readonly TenantGameUserPasswordService _service = new();

    [Fact]
    public void Hash_Then_Verify_Should_Succeed()
    {
        var hash = _service.Hash("Player@123456");

        Assert.True(_service.Verify("Player@123456", hash));
    }

    [Fact]
    public void Verify_Wrong_Password_Should_Return_False()
    {
        var hash = _service.Hash("correct-password");

        Assert.False(_service.Verify("wrong-password", hash));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Hash_Null_Or_Empty_Plaintext_Should_Not_Throw_And_Verify_Consistently(string? plaintext)
    {
        // TenantGameUserPasswordService 层 plaintext ?? ""，Hash 不抛；Verify 同口径
#pragma warning disable CS8604 // 测试意图：传入 null 验证生产端 null 处理（plaintext ?? ""）
        var hash = _service.Hash(plaintext);

        Assert.True(_service.Verify(plaintext, hash));
#pragma warning restore CS8604
        Assert.False(_service.Verify("non-empty", hash));
    }

    [Theory]
    // 空存储
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    // 历史 MD5 字符串（旧无盐 / 单盐 MD5 残留）—— 非法格式，必须一律 false，不认旧 MD5
    [InlineData("d41d8cd98f00b204e9800998ecf8427e")] // 空串 MD5
    [InlineData("5f4dcc3b5aa765d61d8327deb882cf99")] // "password" 的 MD5
    [InlineData("e10adc3949ba59abbe56e057f20f883e")] // "123456" 的 MD5
    // 旧 PBKDF2 自解析格式（大写 / 无 $ 包裹）—— 非 Argon2id PHC 前缀
    [InlineData("PBKDF2-SHA256.100000.c2FsdA.aGFzaA")]
    [InlineData("PBKDF2-SHA256.invalid")]
    [InlineData("MD5-SHA256.100000.c2FsdA.aGFzaA")]
    // 段数 / 前缀错误
    [InlineData("PBKDF2-SHA256.abc.c2FsdA.aGFzaA")]
    [InlineData("PBKDF2-SHA256.100000.!!!not-base64!!!.also-bad")]
    public void Verify_Invalid_Stored_Format_Should_Return_False(string? stored)
    {
        // 纯 Argon2id 校验：任何非 $argon2id$ PHC 格式（含历史 MD5 / PBKDF2）一律 false
#pragma warning disable CS8604 // 测试意图：传入 null/历史格式验证 Verify 对非法存储一律 false
        Assert.False(_service.Verify("any-plaintext", stored));
#pragma warning restore CS8604
    }

    [Fact]
    public void Hash_Should_Produce_Argon2id_Phc_Format()
    {
        var hash = _service.Hash("any-plaintext");

        Assert.StartsWith("$argon2id$", hash);
    }

    [Fact]
    public void Hash_Should_Differ_Each_Call_Due_To_Random_Salt()
    {
        // 每用户随机盐：同一明文两次 Hash 结果不同
        var first = _service.Hash("same-plaintext");
        var second = _service.Hash("same-plaintext");

        Assert.NotEqual(first, second);
        // 但两者都能通过 Verify
        Assert.True(_service.Verify("same-plaintext", first));
        Assert.True(_service.Verify("same-plaintext", second));
    }

    [Fact]
    public void EnumerationDecoyHash_Should_Be_Valid_Argon2id_And_Not_Match_Real_Password()
    {
        // 枚举诱饵哈希：合法 Argon2id PHC 格式（供登录账号不存在路径跑假 Verify，使耗时与密码错误路径对齐），
        // 但不匹配任意真实玩家密码
        Assert.StartsWith("$argon2id$", TenantGameUserPasswordService.EnumerationDecoyHash);
        Assert.False(_service.Verify("any-user-password", TenantGameUserPasswordService.EnumerationDecoyHash));
    }
}