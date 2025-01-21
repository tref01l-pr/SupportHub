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
using SupportHub.Domain.Interfaces.Infrastructure;
using SupportHub.Domain.Models;
using SupportHub.Domain.Options;
using ForgotPasswordRequest = SupportHub.API.Contracts.ForgotPasswordRequest;
using LoginRequest = SupportHub.API.Contracts.LoginRequest;
using ResetPasswordRequest = SupportHub.API.Contracts.ResetPasswordRequest;

namespace SupportHub.API.Controllers;

public class UsersAccountController : BaseController
{
    private readonly ILogger _logger;
    private readonly JWTSecretOptions _options;
    private readonly SmtpOptions _smtpOptions;
    private readonly UserManager<UserEntity> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ISessionsRepository _sessionsRepository;
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly ICompaniesService _companiesService;
    private readonly ICompaniesRepository _companyRepository;
    private readonly IEmailSmtpService _emailSmtpService;
    private readonly IUsersService _usersService;

    public UsersAccountController(
        ILogger<UsersAccountController> logger,
        IOptions<JWTSecretOptions> options,
        IOptions<SmtpOptions> smtpOptions,
        UserManager<UserEntity> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ITransactionsRepository transactionsRepository,
        ISessionsRepository sessionsRepository,
        ICompaniesService companiesService,
        ICompaniesRepository companyRepository,
        IEmailSmtpService emailSmtpService,
        IUsersService usersService)
    {
        _logger = logger;
        _options = options.Value;
        _smtpOptions = smtpOptions.Value;
        _userManager = userManager;
        _roleManager = roleManager;
        _sessionsRepository = sessionsRepository;
        _transactionsRepository = transactionsRepository;
        _companiesService = companiesService;
        _companyRepository = companyRepository;
        _emailSmtpService = emailSmtpService;
        _usersService = usersService;
    }

    [AllowAnonymous]
    [HttpGet("hello")]
    public IActionResult Hello()
    {
        return Ok("Hello");
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

            var companyResult = Company.Create(request.CompanyName, string.Empty);
            if (companyResult.IsFailure)
            {
                throw new Exception(companyResult.Error);
            }

            if ((await _companiesService.GetByUrlAsync<Company>(companyResult.Value.Url)).Value != null)
            {
                throw new Exception("Company with such name is already existing. Try to use another name.");
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
        var company = await _companiesService.GetByUrlAsync<Company>(companyName);

        if (company.IsFailure)
        {
            return BadRequest(company.Error);
        }

        if (company.Value == null)
        {
            return BadRequest("Company with such name does not exist.");
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
            CompanyId = company.Value.Id,
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
    [HttpPost("/{companyUrl}/login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogIn([FromRoute] string companyUrl, [FromBody] LoginRequest request)
    {
        var companyExistResult = await _companiesService.GetByUrlAsync<Company>(companyUrl);
        if (companyExistResult.IsFailure)
        {
            return BadRequest(companyExistResult.Error);
        }

        if (companyExistResult.Value == null)
        {
            return BadRequest("Company not found.");
        }

        var userResult =
            await _usersService.GetByEmailAndCompanyIdAsync<UserEntity>(request.Email, companyExistResult.Value.Id);

        if (userResult.IsFailure)
        {
            return BadRequest(userResult.Error);
        }

        if (userResult.Value is null)
        {
            return BadRequest("User not found.");
        }

        if (companyExistResult.Value.Id != userResult.Value.CompanyId)
        {
            return BadRequest("User does not belong to this company.");
        }

        var isSuccess = await _userManager
            .CheckPasswordAsync(userResult.Value, request.Password);

        if (!isSuccess)
        {
            return BadRequest("Password is incorrect.");
        }

        var roles = await _userManager.GetRolesAsync(userResult.Value);
        var role = roles.FirstOrDefault();
        if (role is null)
        {
            return BadRequest("Role isn't exist.");
        }

        var userInformation =
            new UserInformation(userResult.Value.UserName, userResult.Value.Id, role, companyExistResult.Value.Id,
                companyExistResult.Value.Name);
        var accessToken = JwtHelper.CreateAccessToken(userInformation, _options);
        var refreshToken = JwtHelper.CreateRefreshToken(userInformation, _options);

        //TODO возможно не нужно юзать сессии
        var session = Session.Create(userResult.Value.Id, accessToken, refreshToken);
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
            Id = userResult.Value.Id,
            Role = role,
            AccessToken = accessToken,
            Nickname = userResult.Value.UserName,
            Email = userResult.Value.Email,
        });
    }

    [AllowAnonymous]
    [HttpPost("/{companyUrl}/forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgetPassword([FromRoute] string companyUrl,
        [FromBody] ForgotPasswordRequest request)
    {
        var companyExistResult = await _companiesService.GetByUrlAsync<Company>(companyUrl);
        if (companyExistResult.IsFailure)
        {
            return BadRequest(companyExistResult.Error);
        }

        if (companyExistResult.Value == null)
        {
            return BadRequest("Company not found.");
        }

        var userResult =
            await _usersService.GetByEmailAndCompanyIdAsync<UserEntity>(request.Email, companyExistResult.Value.Id);

        if (userResult.IsFailure)
        {
            return BadRequest(userResult.Error);
        }

        if (userResult.Value is null)
        {
            return BadRequest("User not found.");
        }

        var userRoles = await _userManager.GetRolesAsync(userResult.Value);

        /*if (!(userRoles.Contains(nameof(Roles.Owner)) || userRoles.Contains(nameof(Roles.SystemAdmin))))
        {
            return BadRequest("User is not owner or system admin.");
        }*/

        var token = await _userManager.GeneratePasswordResetTokenAsync(userResult.Value);

        var emailResult =
            await _emailSmtpService.SendForgetPasswordToken(_smtpOptions, userResult.Value.Email, token,
                request.ReturnUrl, userResult.Value.Id);

        if (emailResult.IsFailure)
        {
            return BadRequest(emailResult.Error);
        }

        return Ok("Message was send. Check your email.");
    }

    [AllowAnonymous]
    [HttpPost("/{companyUrl}/reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromRoute] string companyUrl,
        [FromBody] ResetPasswordRequest request)
    {
        var companyExistResult = await _companiesService.GetByUrlAsync<Company>(companyUrl);
        if (companyExistResult.IsFailure)
        {
            return BadRequest(companyExistResult.Error);
        }

        if (companyExistResult.Value == null)
        {
            return BadRequest("Company not found.");
        }

        var userResult =
            await _usersService.GetById<UserEntity>(request.Id);

        if (userResult.IsFailure)
        {
            return BadRequest(userResult.Error);
        }

        if (userResult.Value is null)
        {
            return BadRequest("User not found.");
        }

        var userRoles = await _userManager.GetRolesAsync(userResult.Value);
        /*if (!(userRoles.Contains(nameof(Roles.Owner)) || userRoles.Contains(nameof(Roles.SystemAdmin))))
        {
            return BadRequest("User is not owner or system admin.");
        }*/

        if (userResult.Value.CompanyId != companyExistResult.Value.Id)
        {
            return BadRequest("User does not belong to this company.");
        }

        var result = await _userManager.ResetPasswordAsync(userResult.Value, request.Token, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok("Password was reset.");
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
            Nickname = userInformation.Value.UserName,
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