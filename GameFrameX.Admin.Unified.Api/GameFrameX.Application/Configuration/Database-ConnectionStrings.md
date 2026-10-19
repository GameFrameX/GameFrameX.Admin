# SqlSugar 数据库连接串速查

> 来源：SqlSugar 官方文档 donet5.com「数据库特色」各库页面（2026-08-21 摘录），覆盖 SqlSugarCore 5.1.4.216 `DbType` 枚举全部 34 个取值。
> 示例中的主机 / 库名 / 账号 / 密码均为官方文档演示值，按实际环境替换。

## 用法

切换数据库时，把对应条目的值替换到 [Database.json](./Database.json) 中 main 项的 `DbType` / `ConnectionString` 字段
（主库项必须唯一，勿解开成新的数组元素）；环境级覆盖用成对环境变量 `GFX_MAIN_DB_TYPE` + `GFX_MAIN_DB_CONNECTION_STRING`。

## 速查表

| DbType | 连接串示例 | 备注 |
|---|---|---|
| MySql | `server=localhost;Database=SqlSugar4xTest;Uid=root;Pwd=haosql` | 可选 `Port=3306`、`Charset=utf8mb4` |
| SqlServer | `server=.;uid=sa;pwd=haosql;database=SQLSUGAR4XTEST` | |
| Sqlite | `DataSource=SqlSugar4xTest.sqlite` | 或绝对路径 `DataSource=c:\database\SqlSugar4xTest.sqlite` |
| Oracle | `Data Source=localhost/orcl;User ID=system;Password=haha` | |
| PostgreSQL | `PORT=5432;DATABASE=SqlSugar4xTest;HOST=localhost;PASSWORD=haosql;USER ID=postgres` | 多架构可加 `searchpath=架构名` |
| Dm（达梦） | `Server=localhost;User Id=SYSDBA;PWD=SYSDBA;DATABASE=新DB;SCHEMA=myshcema` | |
| Kdbndp（人大金仓） | `Server=127.0.0.1;Port=54321;UID=SYSTEM;PWD=system;database=SQLSUGAR4XTEST1` | |
| Oscar（神通） | `PORT=2003;DATABASE=osrdb;HOST=localhost;PASSWORD=szoscar55;USER ID=SYSDBA` | |
| MySqlConnector | 连接串同 MySql | .NET Core 高版本直接用 DbType=MySql，仅 .NET Framework 需此值 |
| Access | `Provider=Microsoft.ACE.OleDB.15.0;Data Source=test.accdb` | |
| OpenGauss | `PORT=5432;DATABASE=SqlSugar5Demo;HOST=localhost;PASSWORD=postgres;USER ID=postgres` | |
| QuestDB | `host=localhost;port=8812;username=admin;password=quest;database=qdb;ServerCompatibilityMode=NoTypeLoading;` | |
| HG（瀚高） | `Server=27.151.1;Port=5866;UId=design;Password=123;Database=design;searchpath=design` | |
| ClickHouse | `host=localhost;port=8123;user=default;password=;database=default` | |
| GBase（8s） | `Driver={GBase ODBC DRIVER (64-Bit)};Host=localhost;Service=19088;Server=gbase01;Database=testdb;Protocol=onsoctcp;` | ODBC 驱动 |
| Odbc | `Driver={驱动名};....` | 键值对按目标库 ODBC 驱动文档填写；需 SqlSugar.OdbcCore 5.1.4.160+ 并启动时注册 `InstanceFactory.CustomAssemblies` |
| OceanBaseForOracle | 参考 Oracle 格式 | OceanBase Oracle 模式；官方页面未给显式串（MySQL 模式用 DbType=OceanBase） |
| TDengine | `Host=localhost;Port=6030;Username=root;Password=taosdata;Database=power` | |
| GaussDB | `PORT=5432;DATABASE=SqlSugar5Demo;HOST=localhost;PASSWORD=postgres;USER ID=postgres` | ODBC 方式 |
| OceanBase | `server=localhost;Database=SqlSugar4xTest;Uid=root;Pwd=haosql` | MySQL 模式 |
| Tidb | `server=localhost;Database=SqlSugar4xTest;Uid=root;Pwd=haosql` | |
| Vastbase（海量） | `PORT=5432;DATABASE=SqlSugar4xTest;HOST=localhost;PASSWORD=haosql;USER ID=postgres;No Reset On Close=true` | |
| PolarDB（阿里） | `server=localhost;Database=SqlSugar4xTest;Uid=root;Pwd=haosql` | MySQL 兼容 |
| Doris | `server=localhost;Database=SqlSugar4xTest;Uid=root;Pwd=haosql;Pooling=false` | |
| Xugu（虚谷） | `IP=111.123.17;DB=HOUSE;User=SYSDBA;PWD=abc;Port=5138;AUTO_COMMIT=on;CHAR_SET=UTF8` | |
| GoldenDB | `server=localhost;Database=SqlSugar4xTest;Uid=root;Pwd=haosql;Pooling=false;` | |
| TDSQLForPGODBC | PostgreSQL 兼容串 | 腾讯 TDSQL PG 模式；官方页面未给显式串；需注册 SqlSugar.TDSQLForPGODBC.TDSQLForPGODBCAdapter |
| TDSQLForOracleODBC | Oracle 兼容串 | 腾讯 TDSQL Oracle 模式；官方页面未给显式串；adapter 注册方式同上 |
| TDSQL | `server=localhost;Database=SqlSugar4xTest;Uid=root;Pwd=haosql` | MySQL 内核 |
| HANA | 官方文档页暂无示例（空壳页，截至 2026-08-21） | |
| DB2 | 官方文档页暂无示例（空壳页，提示先用源码、NuGet 未更新，截至 2026-08-21） | |
| GaussDBNative | 串同 GaussDB 格式 | 原生驱动 |
| DuckDB | `DataSource=train_services.db` | |
| MongoDb | `mongodb://root:123456@222.71.212.3:27017/testDB?authSource=admin` | 或 `host=222.71.212.3;Port=27017;Database=testDB;Username=root;Password=123456;authSource=admin` |
| Custom | 连接串由适配器自定 | 自定义 provider：实现适配器并注册 `InstanceFactory.CustomAssemblies` |

## 注意

默认租户库自动推导仅支持 **PostgreSQL / Sqlite / MySQL**；其余 DbType 需按 [Database.json](./Database.json) 文件头第 3 条显式配置租户库。
