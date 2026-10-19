namespace GameFrameX.Application.System.DataBase.Dto;

public class UpdateDbColumnRequest
{
    public string ConfigId { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public string ColumnName { get; set; } = string.Empty;

    public string OldColumnName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}