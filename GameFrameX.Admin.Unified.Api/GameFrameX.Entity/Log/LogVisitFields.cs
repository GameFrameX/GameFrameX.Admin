using GameFrameX.Foundation.Orm.Entity;
using Microsoft.Extensions.Logging;

namespace GameFrameX.Entity.Log;

/// <summary>
/// 访问日志公共字段基类。不映射物理表，仅供 <see cref="SystemLogVisitEntity"/> 与 <see cref="TenantLogVisitEntity"/> 复用字段，
/// 避免单个 CLR 实体按运行上下文切换物理表。
/// </summary>
public class LogVisitFields : EntityTenantBase
{
    /// <summary>
    /// 模块名称
    /// </summary>
    [SugarColumn(ColumnDescription = "模块名称", Length = EntityStringLength.Length512)]
    public string? ControllerName { get; set; }

    /// <summary>
    /// 方法名称
    ///</summary>
    [SugarColumn(ColumnDescription = "方法名称", Length = EntityStringLength.Length512)]
    public string? ActionName { get; set; }

    /// <summary>
    /// 显示名称
    ///</summary>
    [SugarColumn(ColumnDescription = "显示名称", Length = EntityStringLength.Length512)]
    public string? DisplayTitle { get; set; }

    /// <summary>
    /// 执行状态
    /// </summary>
    [SugarColumn(ColumnDescription = "执行状态", Length = EntityStringLength.Length256)]
    public string? Status { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    [SugarColumn(ColumnDescription = "IP地址", Length = EntityStringLength.Length512)]
    public string? RemoteIp { get; set; }

    /// <summary>
    /// 登录地点
    /// </summary>
    [SugarColumn(ColumnDescription = "登录地点", Length = EntityStringLength.Length256)]
    public string? Location { get; set; }

    /// <summary>
    /// 经度
    /// </summary>
    [SugarColumn(ColumnDescription = "经度")]
    public double? Longitude { get; set; }

    /// <summary>
    /// 维度
    /// </summary>
    [SugarColumn(ColumnDescription = "维度")]
    public double? Latitude { get; set; }

    /// <summary>
    /// 浏览器
    /// </summary>
    [SugarColumn(ColumnDescription = "浏览器", Length = EntityStringLength.Length2048)]
    public string? Browser { get; set; }

    /// <summary>
    /// 操作系统
    /// </summary>
    [SugarColumn(ColumnDescription = "操作系统", Length = EntityStringLength.Length512)]
    public string? Os { get; set; }

    /// <summary>
    /// 操作用时
    /// </summary>
    [SugarColumn(ColumnDescription = "操作用时")]
    public long? Elapsed { get; set; }

    /// <summary>
    /// 日志时间
    /// </summary>
    [SugarColumn(ColumnDescription = "日志时间")]
    public long? LogDateTime { get; set; }

    /// <summary>
    /// 日志级别
    /// </summary>
    [SugarColumn(ColumnDescription = "日志级别")]
    public LogLevel? LogLevel { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    [SugarColumn(ColumnDescription = "账号", Length = EntityStringLength.Length256)]
    public string? Account { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "真实姓名", Length = EntityStringLength.Length256)]
    public string? RealName { get; set; }
}