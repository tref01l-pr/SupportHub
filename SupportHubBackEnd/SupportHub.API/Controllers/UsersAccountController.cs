using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SupportHub.API.Contracts;
using SupportHub.DataAccess.SqlServer.Entities;
using SupportHub.Domain.Interfaces.Application;
using SupportHub.Domain.Interfaces.DataAccess;
using SupportHub.Domain.Models;
using SupportHub.Domain.Options;

namespace SupportHub.API.Controllers;

public class UsersAccountController : BaseController
{
    private readonly ILogger _logger;
    private readonly JWTSecretOptions _options;
    private readonly UserManager<UserEntity> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ISessionsRepository _sessionsRepository;
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly ICompaniesService _companiesService;
    private readonly ICompaniesRepository _companyRepository;

    public UsersAccountController(
        ILogger<UsersAccountController> logger,
        IOptions<JWTSecretOptions> options,
        UserManager<UserEntity> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ITransactionsRepository transactionsRepository,
        ISessionsRepository sessionsRepository,
        ICompaniesService companiesService,
        ICompaniesRepository companyRepository)
    {
        _logger = logger;
        _options = options.Value;
        _userManager = userManager;
        _roleManager = roleManager;
        _sessionsRepository = sessionsRepository;
        _transactionsRepository = transactionsRepository;
        _companiesService = companiesService;
        _companyRepository = companyRepository;
    }

    [AllowAnonymous]
    [HttpPost("user-and-company-registration")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UserAndCompanyRegistrationAsync(
        [FromBody] UserRegistrationWithCompanyRequest request)
    {
        try
        {
            var beginTransactionResult = await _transactionsRepository.BeginTransactionAsync();
            if (beginTransactionResult.IsFailure)
            {
                throw new Exception(beginTransactionResult.Error);
            }

            if ((await _companiesService.GetByNameAsync<Company>(request.CompanyName)).Value != null)
            {
                throw new Exception("Company with such name is already existing.");
            }

            var companyResult = Company.Create(request.CompanyName);
            if (companyResult.IsFailure)
            {
                throw new Exception(companyResult.Error);
            }

            var companyCreationResult = await _companyRepository.CreateAsync(companyResult.Value);

            if (companyCreationResult.IsFailure)
            {
                throw new Exception(companyCreationResult.Error);
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is not null)
            {
                var error = "User is existing.";
                _logger.LogError("{error}", error);
                throw new Exception(error);
            }

            var newUser = new UserEntity
            {
                UserName = request.Email,
                Email = request.Email,
                CompanyId = companyCreationResult.Value.Id,
            };

            var result = await _userManager.CreateAsync(
                newUser,
                request.Password);

            if (!result.Succeeded)
            {
                _logger.LogError("{errors}", result.Errors);
                throw new Exception(result.Errors.ToString());
            }

            //TODO Test it
            var roleExists = await _roleManager.RoleExistsAsync(nameof(Roles.Owner));
            if (!roleExists)
            {
                var role = new IdentityRole<Guid>
                {
                    Name = nameof(Roles.Owner)
                };

                await _roleManager.CreateAsync(role);
            }

            await _userManager.AddToRoleAsync(newUser, nameof(Roles.Owner));
            await _transactionsRepository.CommitAsync();
        }
        catch (Exception e)
        {
            await _transactionsRepository.RollbackAsync();
            return BadRequest(e.Message);
        }

        return Ok(true);
    }

    [AllowAnonymous]
    [HttpPost("/{companyName}/user-registration")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UserRegistrationByCompanyAsync(
        [FromRoute] string companyName,
        [FromBody] UserRegistrationRequest request)
    {
        var company = await _companyRepository.GetByNameAsync<Company>(companyName);
        if (company == null)
        {
            throw new Exception("Company with such name does not exist.");
        }
        
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is not null)
        {
            var error = "User is existing.";
            _logger.LogError("{error}", error);
            return BadRequest(error);
        }
        
        var newUser = new UserEntity
        {
            UserName = request.Email,
            Email = request.Email,
            CompanyId = company.Id
        };

        var result = await _userManager.CreateAsync(
            newUser,
            request.Password);

        if (!result.Succeeded)
        {
            _logger.LogError("{errors}", result.Errors);
            return BadRequest(result.Errors);
        }

        var roleExists = await _roleManager.RoleExistsAsync(nameof(Roles.User));
        if (!roleExists)
        {
            var role = new IdentityRole<Guid>()
            {
                Name = nameof(Roles.User)
            };

            await _roleManager.CreateAsync(role);
        }

        await _userManager.AddToRoleAsync(newUser, nameof(Roles.User));

        return Ok(true);
    }

    /// <summary>
    /// Users login.
    /// </summary>
    /// <param name="request">Email and password.</param>
    /// <returns>Jwt token.</returns>
    [AllowAnonymous]
    [HttpPost("/{companyName}/login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogIn([FromRoute] string companyName, [FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return BadRequest("Email not found.");
        }

        var company = await _companyRepository.GetByIdAsync<Company>(user.CompanyId);
        if (company.Name != companyName)
        {
            return BadRequest("User does not exist.");
        }

        var isSuccess = await _userManager
            .CheckPasswordAsync(user, request.Password);

        if (!isSuccess)
        {
            return BadRequest("Password is incorrect.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        if (role is null)
        {
            return BadRequest("Role isn't exist.");
        }
        
        var companyInfo = await _companiesService.GetByIdAsync<Company>(company.Id);
        if (companyInfo.IsFailure)
        {
            return BadRequest(companyInfo.Error);
        }

        if (companyInfo.Value is null)
        {
            return BadRequest("Company not found.");
        }

        var userInformation = new UserInformation(user.UserName, user.Id, role, companyInfo.Value.Id, companyInfo.Value.Name);
        var accessToken = JwtHelper.CreateAccessToken(userInformation, _options);
        var refreshToken = JwtHelper.CreateRefreshToken(userInformation, _options);

        //TODO возможно не нужно юзать сессии
        var session = Session.Create(user.Id, accessToken, refreshToken);
        if (session.IsFailure)
        {
            _logger.LogError("{error}", session.Error);
            return BadRequest(session.Error);
        }
        
        var result = await _sessionsRepository.Create(session.Value);

        if (result.IsFailure)
        {
            _logger.LogError("{error}", result.Error);
            return BadRequest(result.Error);
        }

        Response.Cookies.Append(DefaultAuthenticationTypes.ApplicationCookie, refreshToken, new CookieOptions()
        {
            Secure = false,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax
        });

        return Ok(new TokenResponse
        {
            Id = user.Id,
            Role = role,
            AccessToken = accessToken,
            Nickname = user.UserName,
            Email = user.Email,
        });
    }

    /// <summary>
    /// Refresh access token.
    /// </summary>
    /// <param name="request">Access token.</param>
    /// <returns>New access token and refresh token.</returns>
    [AllowAnonymous]
    [HttpPost("refreshaccesstoken")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshAccessToken()
    {
        //Thread.Sleep(10000);
        const bool jwtTokenV2 = false;
        Result<UserInformation> userInformation;
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return BadRequest("Refresh Token can't be null");
        }

        if (jwtTokenV2)
        {
            userInformation = JwtHelper.GetPayloadFromJWTTokenV2(refreshToken, _options);
        }
        else
        {
            var payload = JwtHelper.GetPayloadFromJWTToken(refreshToken, _options);
            userInformation = JwtHelper.ParseUserInformation(payload);
        }

        if (userInformation.IsFailure)
        {
            _logger.LogError("{error}", userInformation.Error);
            return BadRequest(userInformation.Error);
        }

        var resultGet = await _sessionsRepository.GetById(userInformation.Value.UserId);
        if (resultGet.IsFailure)
        {
            _logger.LogError("{error}", resultGet.Error);
            return BadRequest(resultGet.Error);
        }

        if (resultGet.Value.RefreshToken != refreshToken)
        {
            _logger.LogError("error", "Refresh tokens not equals.");
            return BadRequest("Refresh tokens not equals.");
        }

        var accessToken = JwtHelper.CreateAccessToken(userInformation.Value, _options);

        var session = Session.Create(userInformation.Value.UserId, accessToken, resultGet.Value.RefreshToken);
        if (resultGet.IsFailure)
        {
            _logger.LogError("{error}", resultGet.Error);
            return BadRequest(resultGet.Error);
        }

        var result = await _sessionsRepository.Create(session.Value);
        if (result.IsFailure)
        {
            _logger.LogError("{error}", result.Error);
            return BadRequest(result.Error);
        }

        var user = await _userManager.FindByIdAsync(userInformation.Value.UserId.ToString());

        if (user is null)
        {
            _logger.LogError("error", "No user with that id");
            return BadRequest("No user with that id");
        }

        return Ok(new TokenResponse
        {
            Id = userInformation.Value.UserId,
            Role = userInformation.Value.Role,
            AccessToken = accessToken,
            Nickname = userInformation.Value.Nickname,
            Email = user.Email,
        });
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout()
    {
        string? refreshToken = Request.Cookies[DefaultAuthenticationTypes.ApplicationCookie];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return BadRequest("Your refresh token not exist");
        }

        var result = await _sessionsRepository.Delete(UserId.Value);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        Response.Cookies.Delete(DefaultAuthenticationTypes.ApplicationCookie);

        return Ok("Success");
    }
}