// namespace GameFrameX.Application.Game;
//
//     /// <summary>
//     /// 游戏账号基础输入参数
//     /// </summary>
//     public class GameUserEntityBaseInput
//     {
//         /// <summary>
//         /// 账号
//         /// </summary>
//         public virtual string UserName { get; set; }
//         
//         /// <summary>
//         /// 密码
//         /// </summary>
//         public virtual string Password { get; set; }
//         
//         /// <summary>
//         /// 状态
//         /// </summary>
//         public virtual GameUserStatus Status { get; set; }
//         
//         /// <summary>
//         /// 昵称
//         /// </summary>
//         public virtual string Name { get; set; }
//         
//         /// <summary>
//         /// 描述
//         /// </summary>
//         public virtual string Description { get; set; }
//         
//         /// <summary>
//         /// 创建人
//         /// </summary>
//         public virtual string? CreateUserName { get; set; }
//         
//         /// <summary>
//         /// 最后更新人
//         /// </summary>
//         public virtual string? UpdateUserName { get; set; }
//         
//     }
//
//     /// <summary>
//     /// 游戏账号分页查询输入参数
//     /// </summary>
//     public class GameUserEntityInput : BasePageInput
//     {
//         /// <summary>
//         /// 关键字查询
//         /// </summary>
//         public string? SearchKey { get; set; }
//
//         /// <summary>
//         /// Id
//         /// </summary>
//         public long? Id { get; set; }
//         
//         /// <summary>
//         /// 账号
//         /// </summary>
//         public string? UserName { get; set; }
//         
//         /// <summary>
//         /// 状态
//         /// </summary>
//         public GameUserStatus? Status { get; set; }
//         
//         /// <summary>
//         /// 昵称
//         /// </summary>
//         public string? Name { get; set; }
//         
//     }
//
//     /// <summary>
//     /// 游戏账号增加输入参数
//     /// </summary>
//     public class AddGameUserEntityInput : GameUserEntityBaseInput
//     {
//         /// <summary>
//         /// 账号
//         /// </summary>
//         [Required(ErrorMessage = "账号不能为空")]
//         public override string UserName { get; set; }
//         
//         /// <summary>
//         /// 密码
//         /// </summary>
//         [Required(ErrorMessage = "密码不能为空")]
//         public override string Password { get; set; }
//         
//         /// <summary>
//         /// 状态
//         /// </summary>
//         [Required(ErrorMessage = "状态不能为空")]
//         public override GameUserStatus Status { get; set; }
//         
//         /// <summary>
//         /// 昵称
//         /// </summary>
//         [Required(ErrorMessage = "昵称不能为空")]
//         public override string Name { get; set; }
//         
//     }
//
//     /// <summary>
//     /// 游戏账号删除输入参数
//     /// </summary>
//     public class DeleteGameUserEntityInput : BaseIdInput
//     {
//     }
//
//     /// <summary>
//     /// 游戏账号更新输入参数
//     /// </summary>
//     public class UpdateGameUserEntityInput : GameUserEntityBaseInput
//     {
//         /// <summary>
//         /// Id
//         /// </summary>
//         [Required(ErrorMessage = "Id不能为空")]
//         public long Id { get; set; }
//         
//     }
//
//     /// <summary>
//     /// 游戏账号主键查询输入参数
//     /// </summary>
//     public class QueryByIdGameUserEntityInput : DeleteGameUserEntityInput
//     {
//
//     }
