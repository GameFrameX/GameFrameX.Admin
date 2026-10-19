namespace GameFrameX.Core.Service.User.Dto;

public class DeleteUserRequest : BaseIdRequest
{
    /// <summary>
    /// 机构Id
    /// </summary>
    public long OrganizationId { get; set; }
}