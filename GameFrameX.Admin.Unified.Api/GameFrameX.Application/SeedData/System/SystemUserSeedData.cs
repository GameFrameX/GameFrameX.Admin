using GameFrameX.Core.Enum;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Utility;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 平台主库用户种子数据。
/// <para>superadmin 初始密码两条路径（C322 随机打印机制的替代）：①环境变量 <c>GFX_SUPERADMIN_INITIAL_PASSWORD</c>
/// 有值时按该值 Argon2id 哈希入库并标记 <c>PasswordConfigured=true</c>（无头部署场景）；②未设置时密码留空、
/// 标记 <c>PasswordConfigured=false</c>，等待登录页首装向导设置（有浏览器场景，密码不经控制台）。</para>
/// <para>种子 insert-only 语义保证只有首次安装真正写入，已存在行不会被覆盖重置，首装提示也只打印一次。</para>
/// </summary>
public class SystemUserSeedData : ISqlSugarEntitySeedData<SystemUserEntity>, ISeedDataInsertCallback
{
    /// <summary>
    /// superadmin 初始密码注入环境变量名（无头部署场景；留空走登录页首装向导）。
    /// </summary>
    public const string InitialPasswordEnvironmentVariable = "GFX_SUPERADMIN_INITIAL_PASSWORD";

    private bool _passwordConfigured;

    /// <summary>
    /// 本次 <see cref="HasData"/> 构建是否采用"已配置"密码（环境变量注入）。
    /// </summary>
    public bool PasswordConfigured => _passwordConfigured;

    /// <summary>
    /// 构建 SuperAdmin 平台账号。
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SystemUserEntity> HasData()
    {
        var initialPassword = Environment.GetEnvironmentVariable(InitialPasswordEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(initialPassword))
        {
            _passwordConfigured = false;
            return BuildData(string.Empty, false);
        }

        _passwordConfigured = true;
        return BuildData(CryptogramUtility.HashPassword(initialPassword), true);
    }

    /// <summary>
    /// 首装回调：superadmin 行本次真正插入数据库时打印初始化指引（重启不插入则不打印，避免输出未生效的假指引）。
    /// </summary>
    /// <param name="insertedRowCount">本次实际插入的行数。</param>
    public void OnSeedRowsInserted(int insertedRowCount)
    {
        if (insertedRowCount <= 0)
        {
            return;
        }

        Console.WriteLine("====================================================");
        Console.WriteLine("【首次安装】平台超级管理员初始账号：superadmin");
        if (_passwordConfigured)
        {
            Console.WriteLine($"【首次安装】初始密码已通过环境变量 {InitialPasswordEnvironmentVariable} 配置。");
            Console.WriteLine("【首次安装】请立即登录并按需修改密码！");
        }
        else
        {
            Console.WriteLine("【首次安装】初始密码未配置：请打开管理后台登录页，按向导完成管理员密码初始化。");
            Console.WriteLine("（无浏览器环境可设置环境变量 " + InitialPasswordEnvironmentVariable + " 后重启）");
        }

        Console.WriteLine("====================================================");
    }

    /// <summary>
    /// 构建 SuperAdmin 平台账号数据。
    /// </summary>
    /// <param name="encryptPasswod"></param>
    /// <param name="passwordConfigured">密码是否已完成初始化配置。</param>
    /// <returns></returns>
    public static IEnumerable<SystemUserEntity> BuildData(string encryptPasswod, bool passwordConfigured = true)
    {
        return new[]
        {
            new SystemUserEntity
            {
                Id = 1300000000101,
                Account = "superadmin",
                Password = encryptPasswod,
                PasswordConfigured = passwordConfigured,
                NickName = "超级管理员",
                RealName = "超级管理员",
                Avatar = "",
                Phone = "13884998275",
                BirthdayTime = new DateTimeOffset(DateTime.Parse("1986-06-28")).ToUnixTimeSeconds(),
                Sex = GenderEnum.Male,
                AccountType = AccountTypeEnum.SuperAdmin,
                Remark = "超级管理员",
                CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(),
                TenantId = 0,
                UpdateCount = 0,
                Nation = "汉",
                IdCardNum = "",
                Email = "",
                Address = "",
                PoliticalOutlook = "",
                College = "",
                OfficePhone = "",
                EmergencyContact = "",
                EmergencyPhone = "",
                EmergencyAddress = "",
                Introduction = "",
                JobNum = "",
                PosLevel = "",
                PosTitle = "",
                Expertise = "",
                OfficeZone = "",
                Office = "",
                LastLoginIp = "",
                LastLoginAddress = "",
                LastLoginDevice = "",
                Signature = "",
            },
        };
    }
}
