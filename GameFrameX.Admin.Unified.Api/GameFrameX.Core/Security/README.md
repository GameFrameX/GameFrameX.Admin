# AppSecretHasher

`AppSecretHasher` 为 `TenantGameAppEntity.AppSecret`（应用 API 密钥）提供 **PBKDF2-SHA256** 哈希与校验：

- 输出自解析格式 `PBKDF2-SHA256.{iterations}.{base64(salt)}.{base64(hash)}`
- 每密钥随机盐 + 100k 迭代
- `CryptographicOperations.FixedTimeEquals` 常量时间比对

## 为什么是 PBKDF2 而非 Argon2id

2026-08-11 全项目 **用户密码**已统一迁到 Argon2id（C214 系统用户 / C215 玩家），但 `AppSecretHasher` **有意保留 PBKDF2-SHA256**，不是遗漏：

| 维度          | 用户密码（已切 Argon2id）         | AppSecret（保留 PBKDF2）                                                                        |
|---------------|-----------------------------------|-------------------------------------------------------------------------------------------------|
| 来源          | 用户自选，可能弱、易字典/GPU 爆破 | **平台生成的高熵随机密钥**（`AppSecret` 由系统在创建/重置应用时随机生成，非人记忆、非用户输入） |
| Argon2id 价值 | 内存硬抗 GPU/ASIC，专克用户弱密码 | 高熵输入下，PBKDF2 与 Argon2id 的安全差距可忽略                                                 |
| 威胁模型      | 凭据库泄露后被离线爆破            | 高熵串本身抗爆破；PBKDF2(100k) 已提供足够计算成本                                               |

**一句话：Argon2id 解决的是"用户弱密码"，AppSecret 没这个问题。**

## 何时应重新评估

- 若 `AppSecret` 改为支持租户自定义（低熵、人输入）→ 应回迁 Argon2id（或至少大幅提高 PBKDF2 迭代数）
- 若全项目强制单一哈希算法成为运维/审计的硬要求 → 另立 change 评估迁移成本与一致性收益（需同步改 AppSecret 生成/校验/HMAC 签名链路，并处理历史密钥兼容）
- 若 OWASP Password Storage Cheat Sheet 将 PBKDF2 从可接受清单移除 → 跟进

## 参考

- C214（system-management，commit `8cc588b7`）：Admin 系统用户密码 → Argon2id
- C215（game-business，commit `0fe48dec`）：GameUser 玩家密码 → Argon2id
- [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)：Argon2id 为首选；PBKDF2-SHA256（推荐 ≥600k 迭代）为可接受次选
