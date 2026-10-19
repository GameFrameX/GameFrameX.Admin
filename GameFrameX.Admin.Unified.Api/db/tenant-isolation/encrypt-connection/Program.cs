using GameFrameX.Foundation.Encryption;

// db/tenant-isolation/encrypt-connection：租户库连接串加密运维工具（GFX-727 / C318）。
// 语义对齐 GameFrameX.Core.Service.Tenant.TenantConnectionCrypto.Encrypt（C317）：
//   - 输出恒为 enc:v1:<Base64>（AesHelper AES-GCM + PBKDF2，密钥取 GFX_TENANT_CONN_ENC_KEY）；
//   - 已加密（enc:v1: 前缀）或空值原样透传（幂等）；
//   - 未配置密钥时以非零码退出（运维脚本不得静默落明文，与运行时「降级直存」不同：这是人工迁移入口）。
// 用法：echo 'Host=...;Password=...' | dotnet run --project db/tenant-isolation/encrypt-connection
//      dotnet run --project db/tenant-isolation/encrypt-connection 'Host=...;Password=...'

if (args.Length > 1)
{
    Console.Error.WriteLine("用法：encrypt-connection [连接串]（或从 stdin 读取，仅接受一个参数）");
    return 2;
}

var plain = args.Length == 1 ? args[0] : Console.In.ReadToEnd().TrimEnd('\r', '\n');

if (string.IsNullOrWhiteSpace(plain) || plain.StartsWith("enc:v1:", StringComparison.Ordinal))
{
    Console.Out.Write(plain);
    return 0;
}

var key = Environment.GetEnvironmentVariable("GFX_TENANT_CONN_ENC_KEY");
if (string.IsNullOrWhiteSpace(key))
{
    Console.Error.WriteLine("环境变量 GFX_TENANT_CONN_ENC_KEY 未配置，无法加密。三 host 必须共享同一密钥，否则迁移后解密失败。");
    return 3;
}

Console.Out.Write("enc:v1:" + AesHelper.Encrypt(plain, key));
return 0;
