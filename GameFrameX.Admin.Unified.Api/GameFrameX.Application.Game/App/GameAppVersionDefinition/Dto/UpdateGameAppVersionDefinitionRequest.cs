namespace GameFrameX.Application.Game.App.GameAppVersionDefinition.Dto;

/// <summary>
/// 版本管理更新输入参数
/// </summary>
public class UpdateGameAppVersionDefinitionRequest : BaseUpdateRequest
{
    /// <summary>
    /// 游戏应用Id。不可变：服务层从既有实体回填，禁止跨应用迁移。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string? Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public virtual string? Description { get; set; }
}