namespace GameFrameX.Application.Platform.DataBase.Dto;

public class UpdateDbTableRequest
{
    public string ConfigId { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public string OldTableName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}