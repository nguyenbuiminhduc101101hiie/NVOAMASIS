using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models;

public class AuthUser
{
    [Key]
    public Guid UsrId { get; set; }
    public string? Usr { get; set; }
    public string? Name { get; set; }
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
	public string? Email { get; set; }
    public string? Pass_viettel { get; set; } 
	public string? Manager2 { get; set; }
    public string? NickName { get; set; }
    public string? Department { get; set; }
    public string? Branch { get; set; }
    public string? Zaloid { get; set; }
    public string? Roles_Dept { get; set; }
    public string? CompanyCode { get; set; }

    public bool Send_OTP_login { get; set; }

}
