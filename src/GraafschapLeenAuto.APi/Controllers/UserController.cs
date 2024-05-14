using GraafschapLeenAuto.Api.Entities;
using GraafschapLeenAuto.Api.Services;
using GraafschapLeenAuto.Shared.Enums;
using GraafschapLeenAuto.Shared.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GraafschapLeenAuto.APi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(UserService userService) : ControllerBase
{
    private readonly UserService userService = userService;

    [Authorize]
    [HttpGet]
    public IActionResult GetUsers()
    {
        var users = userService.GetUsers();
        return Ok(users);
    }

    // Door het toevoegen van de fallback policy mag je hier niet bij als je niet ingelogd bent
    [HttpGet("{id}")]
    public IActionResult GetUser(int id)
    {
        throw new NotImplementedException();
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    public IActionResult CreateUser([FromBody] User user)
    {
        var createdUser = userService.CreateUser(user);

        return Ok(createdUser);
    }

    [Authorize(Roles = nameof(UserRole.User))]
    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, [FromBody] User user)
    {
        throw new NotImplementedException();
    }

    [Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.User))]
    [HttpPatch("assign-role")]
    public IActionResult AssignRole([FromBody] AssignRoleRequest request)
    {
        var result = userService.AssignRole(request);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
