using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportHub.API.Contracts;
using SupportHub.Domain.Interfaces.Application;
using SupportHub.Domain.Models;

namespace SupportHub.API.Controllers;

[Authorize(Roles = nameof(Roles.SystemAdmin))]
public class SystemAdminsController : BaseController
{
    private readonly ISystemAdminsService _systemAdminService;
    private readonly IMapper _mapper;
    private readonly ILogger<SystemAdminsController> _logger;

    public SystemAdminsController(
        ISystemAdminsService systemAdminService,
        IMapper mapper,
        ILogger<SystemAdminsController> logger)
    {
        _systemAdminService = systemAdminService;
        _mapper = mapper;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var users = await _systemAdminService.GetAsync();

        var response = _mapper.Map<User[], GetUserResponse[]>(users);

        return Ok(response);
    }
    
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> Get([FromRoute] Guid userId)
    {
        var user = await _systemAdminService.GetByIdAsync(userId);

        if (user.IsFailure)
        {
            _logger.LogError("{error}", user.Error);
            return BadRequest(user.Error);
        }

        var response = _mapper.Map<User, GetUserResponse>(user.Value);

        return Ok(response);
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Delete(Guid userId)
    {
        if (UserId.Value == userId)
        {
            return BadRequest("You cannot delete yourself");
        }

        var response = await _systemAdminService.Delete(userId);

        if (response.IsFailure)
        {
            _logger.LogError("{error}", response.Error);
            return BadRequest(response.Error);
        }

        return Ok(response.IsSuccess);
    }
}