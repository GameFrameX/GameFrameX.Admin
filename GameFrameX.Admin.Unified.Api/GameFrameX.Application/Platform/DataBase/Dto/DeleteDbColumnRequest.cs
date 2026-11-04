namespace GameFrameX.Application.Platform.DataBase.Dto;

public class DeleteDbColumnRequest
{
    public string ConfigId { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public string DbColumnName { get; set; } = string.Empty;
}