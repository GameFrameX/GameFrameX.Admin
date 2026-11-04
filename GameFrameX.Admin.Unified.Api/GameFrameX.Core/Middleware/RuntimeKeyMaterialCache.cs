using System.Security.Cryptography;
using System.Text;
using GameFrameX.Core.Service.Tenant;
using Microsoft.Extensions.Caching.Memory;

namespace GameFrameX.Core.Middleware;

/// <summary>有界缓存解密材料；启用、撤销和租约状态仍由每次定点查询核对。</summary>
internal static class RuntimeKeyMaterialCache
{
    private static readonly MemoryCache Materials = new(new MemoryCacheOptions { SizeLimit = 1024 });

    internal static string Decrypt(string storedSecret)
    {
        var environmentKey = Environment.GetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable) ?? string.Empty;
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(environmentKey + "\0" + storedSecret)));
        return Materials.GetOrCreate(fingerprint, entry =>
        {
            entry.Size = 1;
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);
            return TenantSecretCrypto.Decrypt(storedSecret) ?? string.Empty;
        })!;
    }
}
