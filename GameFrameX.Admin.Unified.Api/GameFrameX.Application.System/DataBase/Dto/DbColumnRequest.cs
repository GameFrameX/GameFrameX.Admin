namespace GameFrameX.Application.System.DataBase.Dto;

public class DbColumnRequest
{
    public string ConfigId { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public string DbColumnName { get; set; } = string.Empty;

    public string DataType { get; set; } = string.Empty;

    public int Length { get; set; }

    public string ColumnDescription { get; set; } = string.Empty;

    public int IsNullable { get; set; }

    public int IsIdentity { get; set; }

    public int IsPrimarykey { get; set; }

    public int DecimalDigits { get; set; }
}