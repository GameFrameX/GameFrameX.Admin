namespace GameFrameX.Core.Service;

/// <summary>
/// 敏感字段「******」输出脱敏契约的全仓唯一实现。
/// <para>占位契约：API 输出（分页/详情）把敏感字段替换为 <see cref="MaskValue"/> 占位符，明文永不回显；
/// 更新回传时若入参等于 <see cref="MaskValue"/>，视为「用户未修改」，保留数据库原值，避免脱敏回写覆盖真实密钥；
/// 新增（Add）场景禁止提交占位值——数据库没有可保留的原值，占位落库等于凭据永久丢失。</para>
/// <para>守护测试 <c>SensitiveFieldMaskGuardTests</c> 禁止 Core 与测试工程之外再出现同语义常量定义或 "******" 字面量赋值。</para>
/// </summary>
public static class SensitiveFieldMask
{
    /// <summary>
    /// 脱敏占位字符串。
    /// </summary>
    public const string MaskValue = "******";

    /// <summary>
    /// 判断入参是否为脱敏占位值，用于更新时保留原密钥、新增时拒绝占位提交。
    /// </summary>
    /// <param name="input">前端回传的敏感字段值。</param>
    /// <returns>为 <c>******</c> 返回 true，表示应保留数据库原值（或拒绝新增提交）。</returns>
    public static bool IsMasked(string? input)
    {
        return input == MaskValue;
    }

    /// <summary>
    /// 若入参为脱敏占位值，则返回数据库原值；否则返回入参新值。
    /// </summary>
    /// <param name="input">前端回传值。</param>
    /// <param name="dbValue">数据库当前值。</param>
    /// <returns>最终应落库的值；null 一律归一为空串。</returns>
    public static string PreserveIfMasked(string? input, string? dbValue)
    {
        return IsMasked(input) ? dbValue ?? string.Empty : input ?? string.Empty;
    }

    /// <summary>
    /// 输出脱敏：值非空时替换为占位符，空值原样返回（空敏感字段不显示假占位）。
    /// </summary>
    /// <param name="value">数据库中的敏感字段值。</param>
    /// <returns>应输出给前端的值。</returns>
    public static string? MaskIfNotEmpty(string? value)
    {
        return string.IsNullOrEmpty(value) ? value : MaskValue;
    }
}
