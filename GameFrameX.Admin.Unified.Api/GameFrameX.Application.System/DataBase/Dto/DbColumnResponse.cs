namespace GameFrameX.Application.System.DataBase.Dto;

public class DbColumnResponse
{
    public string TableName { get; set; } = string.Empty;

    public int TableId { get; set; }

    public string DbColumnName { get; set; } = string.Empty;

    public string PropertyName { get; set; } = string.Empty;

    public string DataType { get; set; } = string.Empty;

    public object PropertyType { get; set; } = null!;

    public int Length { get; set; }

    public string ColumnDescription { get; set; } = string.Empty;

    public string DefaultValue { get; set; } = string.Empty;

    public bool IsNullable { get; set; }

    public bool IsIdentity { get; set; }

    public bool IsPrimarykey { get; set; }

    public object Value { get; set; } = null!;

    public int DecimalDigits { get; set; }

    public int Scale { get; set; }

    public bool IsArray { get; set; }

    public bool IsJson { get; set; }

    public bool? IsUnsigned { get; set; }

    public int CreateTableFieldSort { get; set; }

    internal object SqlParameterDbType { get; set; } = null!;
}