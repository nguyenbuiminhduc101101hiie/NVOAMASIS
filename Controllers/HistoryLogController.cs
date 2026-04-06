// Controllers/HistoryLogController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using NVOAMASIS.Components.UserList.Pages;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class HistoryLogController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    public HistoryLogController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }


    [HttpPost]
    public async Task<IActionResult> CreateLog([FromBody] HistoryLog log)
    {
        //_context.HistoryLogs.Add(log);
        //await _context.SaveChangesAsync();
        try
        {
            _context.HistoryLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            return StatusCode(500, "Lỗi lưu database: " + ex.Message);
        }

        return Ok();
    }


    // ✅ API GET để lấy toàn bộ log
    [HttpGet("get")]
    public async Task<IActionResult> GetAllLogs()
    {
        var logs = await _context.HistoryLogs
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync();
        return Ok(logs);
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] FormDataModel data)
    {
        if (data == null)
            return BadRequest("No data");

        var entity = new M_Test_getapi
        {
    
            Ten = data.ten,
            Email = data.email,
            Sdt= data.sdt,
        
        };

        _context.Test_getapi.Add(entity);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Saved" });
    }
    public class FormDataModel
    { 
        public string ten { get; set; }
 
        public string email { get; set; }

        public string sdt { get; set; }

    }

    //[HttpPost("login")]
    //[AllowAnonymous]
    //public IActionResult Login([FromBody] AuthUser login)
    //{
    //    if (login.Usr == "sgn-gslog-admin" && login.Pass_viettel == "As12345678")
    //    {
    //        var claims = new[]
    //        {
    //        new Claim(ClaimTypes.Name, login.Usr)
    //    };

    //        var secretKey = _configuration["JwtSettings:SecretKey"];
    //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    //        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    //        var token = new JwtSecurityToken(
    //            issuer: "yourdomain.com",
    //            audience: "yourdomain.com",
    //            claims: claims,
    //            expires: DateTime.Now.AddHours(1),
    //            signingCredentials: creds);

    //        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    //        return Ok(new { token = tokenString });
    //    }

    //    return Unauthorized();
    //}
}
