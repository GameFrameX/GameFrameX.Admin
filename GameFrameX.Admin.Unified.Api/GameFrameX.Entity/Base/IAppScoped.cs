namespace GameFrameX.Entity.Base;

/// <summary>
/// 应用作用域实体契约：实体带有应用维度 <see cref="AppId"/>，数据归属某个具体应用。
/// <para>用于统一应用作用域实体的契约，支撑 <c>AppScopeRules</c> 实体级校验、<c>AppScopeQueryFilter</c> 泛型过滤等共享逻辑，
/// 避免每个服务重复手写。值级合法性（事实数据 <c>AppId &gt; 0</c>、配置覆盖 <c>AppId &gt;= 0</c>，<c>0</c> = 租户默认）由 <c>AppScopeRules</c> 在写入路径强制。</para>
/// <para>本接口只声明契约，<b>不改变实体继承</b>：实体仍按需继承 <see cref="EntityBasic"/> / <see cref="EntityBasicSelect"/> 等，
/// 另以接口实现叠加应用作用域语义（组合优于继承，C# 单继承下避免与 Name/Description 等基类维度产生组合爆炸）。</para>
/// </summary>
public interface IAppScoped
{
    /// <summary>
    /// 应用 ID。事实数据（订单/邮件/兑换码/角色等）必须 <c>&gt; 0</c>；配置类（支付/客户端配置等）<c>&gt;= 0</c>，<c>0</c> 表示租户默认配置。
    /// </summary>
    long AppId { get; set; }
}