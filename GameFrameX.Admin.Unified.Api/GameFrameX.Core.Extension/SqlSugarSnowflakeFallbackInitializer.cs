using System.Runtime.CompilerServices;
using SqlSugar;

namespace GameFrameX.Core.Extension;

/// <summary>
/// SqlSugar 雪花 ID 进程级接管初始化器（C361 / GFX-804）。
/// <para>SqlSugar 对 long 主键的默认值插入回填经 <c>StaticConfig.CustomSnowFlakeFunc</c> 取号
/// （反编译 SqlSugarCore 5.1.4.216 证实 <c>IdWorker.NextId()</c> 首先消费该 func，非空即 Invoke 返回）。
/// 此前该 func 只在 <c>SqlSugarSetup.AddSqlSugar</c>（仅 Web.Api 装配）安装——Hub / Client
/// （走 <c>SqlSugarClientConfig.AddSqlSugarClientService</c>）与未装配 SqlSugarSetup 的进程一律退回
/// SqlSugar 内置 18-19 位长布局。本初始化器在 Core.Extension 模块加载时进程级安装 func，
/// 指向 Foundation 紧凑布局兜底取号，杜绝长 ID 来源。</para>
/// <para>与宿主装配的关系：三 host 启动期先行显式初始化（Web.Api 经 <c>AddSqlSugar</c> →
/// <c>AddSnowFlakeIdGenerator</c>，Hub / Client 经 <c>HostBootstrapSnowflake.Init</c>，互斥 WorkerId），
/// func 命中「单例已创建」分支直接复用；仅未初始化进程落到 WorkerId=0 兜底（见
/// <see cref="SnowFlakeExtension.NextSqlSugarFallbackId"/>）。</para>
/// </summary>
internal static class SqlSugarSnowflakeFallbackInitializer
{
    [ModuleInitializer]
    internal static void Install()
    {
        StaticConfig.CustomSnowFlakeFunc = SnowFlakeExtension.NextSqlSugarFallbackId;
    }
}
