using GameFrameX.Application.System.DataBase.Dto;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Option;
using Microsoft.Extensions.Options;

namespace GameFrameX.Application.System.DataBase;

/// <summary>
/// 系统数据库管理服务
/// </summary>
[ApiDescriptionSettings(Order = 250)]
public class SystemDatabaseService : IAdminApiController
{
    private readonly ISqlSugarClient _db;
    private readonly DbConnectionOptions _dbConnectionOptions;

    public SystemDatabaseService(ISqlSugarClient db,
        IOptions<DbConnectionOptions> dbConnectionOptions)
    {
        _db = db;
        _dbConnectionOptions = dbConnectionOptions.Value;
    }

    /// <summary>
    /// 获取库列表
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取库列表")]
    public List<string> GetList()
    {
        return _dbConnectionOptions.ConnectionConfigs.Select(u => u.ConfigId.ToString() ?? string.Empty).ToList();
    }

    /// <summary>
    /// 获取字段列表
    /// </summary>
    /// <param name="input">查询条件（表名 + ConfigId）</param>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("获取字段列表")]
    [HttpGet("columnList")]
    public List<DbColumnResponse> GetColumnList([FromQuery] GetColumnListRequest input)
    {
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        if (string.IsNullOrWhiteSpace(input.TableName))
        {
            return new List<DbColumnResponse>();
        }

        EnsurePhysicalTableAllowed(input.ConfigId, input.TableName);
        return db.DbMaintenance.GetColumnInfosByTableName(input.TableName, false).Adapt<List<DbColumnResponse>>();
    }

    /// <summary>
    /// 获取数据库数据类型列表
    /// </summary>
    /// <param name="configId"></param>
    /// <returns></returns>
    [DisplayName("获取数据库数据类型列表")]
    public List<string> GetDbTypeList(string configId = SqlSugarConst.MainConfigId)
    {
        var db = _db.AsTenant().GetConnectionScope(configId);
        return db.DbMaintenance.GetDbTypes().OrderBy(u => u).ToList();
    }

    /// <summary>
    /// 增加列
    /// </summary>
    /// <param name="input"></param>
    [ApiDescriptionSettings(Name = "AddColumn")]
    [HttpPost]
    [DisplayName("增加列")]
    public void AddColumn(DbColumnRequest input)
    {
        EnsurePhysicalTableAllowed(input.ConfigId, input.TableName);
        var column = new DbColumnInfo
        {
            ColumnDescription = input.ColumnDescription,
            DbColumnName = input.DbColumnName,
            IsIdentity = input.IsIdentity == 1,
            IsNullable = input.IsNullable == 1,
            IsPrimarykey = input.IsPrimarykey == 1,
            Length = input.Length,
            DecimalDigits = input.DecimalDigits,
            DataType = input.DataType,
        };
        var db = GetValidConnection(input.ConfigId);
        db.DbMaintenance.AddColumn(input.TableName, column);
        db.DbMaintenance.AddColumnRemark(input.DbColumnName, input.TableName, input.ColumnDescription);
        if (column.IsPrimarykey)
        {
            db.DbMaintenance.AddPrimaryKey(input.TableName, input.DbColumnName);
        }
    }

    /// <summary>
    /// 删除列
    /// </summary>
    /// <param name="input"></param>
    [ApiDescriptionSettings(Name = "DeleteColumn")]
    [HttpPost]
    [DisplayName("删除列")]
    public void DeleteColumn(DeleteDbColumnRequest input)
    {
        EnsurePhysicalTableAllowed(input.ConfigId, input.TableName);
        var db = GetValidConnection(input.ConfigId);
        db.DbMaintenance.DropColumn(input.TableName, input.DbColumnName);
    }

    /// <summary>
    /// 编辑列
    /// </summary>
    /// <param name="input"></param>
    [ApiDescriptionSettings(Name = "UpdateColumn")]
    [HttpPost]
    [DisplayName("编辑列")]
    public void UpdateColumn(UpdateDbColumnRequest input)
    {
        EnsurePhysicalTableAllowed(input.ConfigId, input.TableName);
        var db = GetValidConnection(input.ConfigId);
        db.DbMaintenance.RenameColumn(input.TableName, input.OldColumnName, input.ColumnName);
        if (db.DbMaintenance.IsAnyColumnRemark(input.ColumnName, input.TableName))
        {
            db.DbMaintenance.DeleteColumnRemark(input.ColumnName, input.TableName);
        }

        db.DbMaintenance.AddColumnRemark(input.ColumnName, input.TableName, string.IsNullOrWhiteSpace(input.Description) ? input.ColumnName : input.Description);
    }

    /// <summary>
    /// 获取表列表
    /// </summary>
    /// <param name="configId">ConfigId</param>
    /// <returns></returns>
    [DisplayName("获取表列表")]
    public List<DbTableInfo> GetTableList(string configId = SqlSugarConst.MainConfigId)
    {
        var db = _db.AsTenant().GetConnectionScope(configId);
        return db.DbMaintenance.GetTableInfoList(false);
    }

    /// <summary>
    /// 增加表
    /// </summary>
    /// <param name="input"></param>
    [ApiDescriptionSettings(Name = "AddTable")]
    [HttpPost]
    [DisplayName("增加表")]
    public void AddTable(DbTableRequest input)
    {
        EnsurePhysicalTableAllowed(input.ConfigId, input.TableName);
        if (input.DbColumnInfoList == null || !input.DbColumnInfoList.Any())
        {
            throw BusinessError.Oh(DatabaseErrorCode.ColumnsRequired);
        }

        if (input.DbColumnInfoList.GroupBy(u => u.DbColumnName).Any(u => u.Count() > 1))
        {
            throw BusinessError.Oh(DatabaseErrorCode.DuplicateFieldNameForbidden);
        }

        var db = GetValidConnection(input.ConfigId);
        var typeBuilder = db.DynamicBuilder().CreateClass(input.TableName, new SugarTable { TableName = input.TableName, TableDescription = input.Description, });
        input.DbColumnInfoList.ForEach(u =>
        {
            var columnName = u.DbColumnName?.Trim() ?? string.Empty;
            var dbColumnName = UtilMethods.ToUnderLine(columnName);
            // 虚拟类都默认string类型，具体以列数据类型为准
            typeBuilder.CreateProperty(dbColumnName, typeof(string), new SugarColumn
            {
                IsPrimaryKey = u.IsPrimarykey == 1,
                IsIdentity = u.IsIdentity == 1,
                ColumnDataType = u.DataType,
                Length = u.Length,
                IsNullable = u.IsNullable == 1,
                DecimalDigits = u.DecimalDigits,
                ColumnDescription = u.ColumnDescription,
            });
        });
        db.CodeFirst.InitTables(typeBuilder.BuilderType());
    }

    /// <summary>
    /// 删除表
    /// </summary>
    /// <param name="input"></param>
    [ApiDescriptionSettings(Name = "DeleteTable")]
    [HttpPost]
    [DisplayName("删除表")]
    public void DeleteTable(DeleteDbTableRequest input)
    {
        EnsurePhysicalTableAllowed(input.ConfigId, input.TableName);
        var db = GetValidConnection(input.ConfigId);
        db.DbMaintenance.DropTable(input.TableName);
    }

    /// <summary>
    /// 编辑表
    /// </summary>
    /// <param name="input"></param>
    [ApiDescriptionSettings(Name = "UpdateTable")]
    [HttpPost]
    [DisplayName("编辑表")]
    public void UpdateTable(UpdateDbTableRequest input)
    {
        EnsurePhysicalTableAllowed(input.ConfigId, input.OldTableName);
        EnsurePhysicalTableAllowed(input.ConfigId, input.TableName);
        var db = GetValidConnection(input.ConfigId);
        db.DbMaintenance.RenameTable(input.OldTableName, input.TableName);
        try
        {
            if (db.DbMaintenance.IsAnyTableRemark(input.TableName))
            {
                db.DbMaintenance.DeleteTableRemark(input.TableName);
            }
            else
            {
                db.DbMaintenance.AddTableRemark(input.TableName, input.Description);
            }
        }
        catch (NotSupportedException)
        {
            //Ignore 不支持该方法则不处理
        }
    }

    /// <summary>
    /// 校验并获取数据库连接配置
    /// </summary>
    private DbConnectionConfig GetValidConfig(string configId)
    {
        return _dbConnectionOptions.ConnectionConfigs.FirstOrDefault(u => u.ConfigId.ToString() == configId) ?? throw BusinessError.Oh("数据库连接配置不存在");
    }

    /// <summary>
    /// 校验 ConfigId 并获取数据库连接
    /// </summary>
    private SqlSugarScopeProvider GetValidConnection(string configId)
    {
        if (string.IsNullOrWhiteSpace(configId))
        {
            throw BusinessError.Oh("数据库连接配置不存在");
        }

        return _db.AsTenant().GetConnectionScope(configId);
    }

    private static void EnsurePhysicalTableAllowed(string configId, string tableName)
    {
        var databaseOwnership = ResolveDatabaseOwnership(configId);
        if (!SqlSugarEntityBoundary.IsPhysicalTableNameAllowedForDatabase(databaseOwnership, tableName))
        {
            throw BusinessError.Oh($"表名 {tableName} 不符合当前数据库 {configId} 的物理前缀边界。主库只能操作 system_/global_，租户库只能操作 tenant_。");
        }
    }

    private static DatabaseOwnership ResolveDatabaseOwnership(string configId)
    {
        if (configId == SqlSugarConst.MainConfigId)
        {
            return DatabaseOwnership.System;
        }

        return DatabaseOwnership.Tenant;
    }
}