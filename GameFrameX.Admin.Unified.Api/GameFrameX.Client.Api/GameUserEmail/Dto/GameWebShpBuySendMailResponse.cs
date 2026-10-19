namespace GameFrameX.Client.Api.GameUserEmail.Dto;

public sealed class GameWebShpBuySendMailResponse
{
    // /// <summary>
    // /// 角色错误列表
    // /// </summary>
    // [System.ComponentModel.Description("角色错误列表")]
    // public ListAsync<RespPostMailItem> RoleFails { get; set; } = new ListAsync<RespPostMailItem>();

    /// <summary>
    /// 返回的错误码
    /// </summary>
    public int ErrorCode { get; set; }
}