using GraafschapLeenAuto.Api.Services;
using GraafschapLeenAuto.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GraafschapLeenAuto.Api.Controllers;

[AllowAnonymous]
[Controller]
[Route("[controller]")]
public class AuthController(AuthService authService) : ControllerBase
{
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
}
