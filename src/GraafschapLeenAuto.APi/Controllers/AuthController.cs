using GraafschapLeenAuto.Api.Services;
using GraafschapLeenAuto.Shared.Enums;
using GraafschapLeenAuto.Shared.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GraafschapLeenAuto.Api.Controllers;


[ApiController]
[Route("[controller]")]
public class AuthController(AuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var response = authService.Login(request);

        if (response == null)
        {
            return Unauthorized();
        }

        return Ok(response);
    }


    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpGet("secret")]
    public IActionResult Secret()
    {
        return Ok("secret");
    }
}
