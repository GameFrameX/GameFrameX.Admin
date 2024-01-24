using GameFrameX.Core;
using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Game;

    /// <summary>
    /// 游戏角色基础输入参数
    /// </summary>
    public class GameRoleEntityBaseInput
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public virtual long UserId { get; set; }
        
        /// <summary>
        /// 区服Id
        /// </summary>
        public virtual long AreaId { get; set; }
        
        /// <summary>
        /// 名称
        /// </summary>
        public virtual string Name { get; set; }
        
        /// <summary>
        /// 详细描述
        /// </summary>
        public virtual string Description { get; set; }
        
        /// <summary>
        /// 角色当前的等级
        /// </summary>
        public virtual long? Level { get; set; }
        
        /// <summary>
        /// 角色最后一次登录时间
        /// </summary>
        public virtual DateTime? LastLoginTime { get; set; }
        
    }

    /// <summary>
    /// 游戏角色分页查询输入参数
    /// </summary>
    public class GameRoleEntityInput : BasePageInput
    {
        /// <summary>
        /// 关键字查询
        /// </summary>
        public string? SearchKey { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public long? UserId { get; set; }
        
        /// <summary>
        /// 区服Id
        /// </summary>
        public long? AreaId { get; set; }
        
        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// 角色当前的等级
        /// </summary>
        public long? Level { get; set; }
        
    }

    /// <summary>
    /// 游戏角色增加输入参数
    /// </summary>
    public class AddGameRoleEntityInput : GameRoleEntityBaseInput
    {
    }

    /// <summary>
    /// 游戏角色删除输入参数
    /// </summary>
    public class DeleteGameRoleEntityInput : BaseIdInput
    {
    }

    /// <summary>
    /// 游戏角色更新输入参数
    /// </summary>
    public class UpdateGameRoleEntityInput : GameRoleEntityBaseInput
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [Required(ErrorMessage = "主键Id不能为空")]
        public long Id { get; set; }
        
    }

    /// <summary>
    /// 游戏角色主键查询输入参数
    /// </summary>
    public class QueryByIdGameRoleEntityInput : DeleteGameRoleEntityInput
    {

    }
