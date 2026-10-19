namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// 种子插入回调接口。
/// <para>种子执行器（<c>SqlSugarSetup.InitDatabase</c>）在主键分支本次实际插入数据库行数大于 0 时回调，
/// 用于「仅首次安装」语义的提示输出（如 superadmin 随机初始密码打印）；行已存在而跳过插入时不回调，
/// 保证重启等重复初始化不会输出未生效的内容。</para>
/// </summary>
public interface ISeedDataInsertCallback
{
    /// <summary>
    /// 本次启动该种子实际插入数据库的行数回调（仅在插入行数大于 0 时触发）。
    /// </summary>
    /// <param name="insertedRowCount">本次实际插入的行数。</param>
    void OnSeedRowsInserted(int insertedRowCount);
}
