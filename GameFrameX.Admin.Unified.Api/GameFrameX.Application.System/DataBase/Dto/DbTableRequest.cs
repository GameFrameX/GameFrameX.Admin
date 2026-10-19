namespace GameFrameX.Application.System.DataBase.Dto;

public class DbTableRequest
{
    public string ConfigId { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<DbColumnRequest> DbColumnInfoList { get; set; } = new();
}