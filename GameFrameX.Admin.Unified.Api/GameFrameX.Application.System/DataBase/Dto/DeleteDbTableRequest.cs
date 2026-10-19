namespace GameFrameX.Application.System.DataBase.Dto;

public class DeleteDbTableRequest
{
    public string ConfigId { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;
}