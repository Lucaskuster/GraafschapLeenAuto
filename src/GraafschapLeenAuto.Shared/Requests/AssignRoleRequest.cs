using System.ComponentModel.DataAnnotations;

namespace GraafschapLeenAuto.Shared.Requests;

public class AssignRoleRequest
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int RoleId { get; set; }
}