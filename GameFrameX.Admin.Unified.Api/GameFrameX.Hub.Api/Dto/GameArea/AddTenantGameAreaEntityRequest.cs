using System.ComponentModel.DataAnnotations;
using GameFrameX.Core.BaseInputOutput;

namespace GameFrameX.Hub.Api.Dto.GameArea;

/// <summary>
/// 游戏区服增加输入参数
/// </summary>
public class AddTenantGameAreaEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    [Required(ErrorMessage = "详细描述不能为空")]
    public string? Description { get; set; }

    /// <summary>
    /// 大区分类Id
    /// </summary>
    [Required(ErrorMessage = "大区分类Id不能为空")]
    public long ClassifyId { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [Required(ErrorMessage = "是否启用不能为空")]
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Http 管理连接地址
    /// </summary>
    [Required(ErrorMessage = "Http 管理连接地址不能为空")]
    public string HttpManageUrl { get; set; } = null!;

    /// <summary>
    /// TcpSocket 连接地址
    /// </summary>
    [Required(ErrorMessage = "TcpSocket 连接地址不能为空")]
    public string? TcpSocketUrl { get; set; }

    /// <summary>
    /// WebSocket连接地址
    /// </summary>
    [Required(ErrorMessage = "WebSocket连接地址不能为空")]
    public string? WebSocketUrl { get; set; }

    /// <summary>
    /// 服务器的游戏模式类型
    /// </summary>
    [Required(ErrorMessage = "服务器的游戏模式类型不能为空")]
    public string ServerType { get; set; } = null!;

    /// <summary>
    /// 服务器的游戏版本
    /// </summary>
    public string? ServerVersion { get; set; }

    /// <summary>
    /// 服务器的 Docker 容器 ID
    /// </summary>
    public string? ServerContainerId { get; set; }
}