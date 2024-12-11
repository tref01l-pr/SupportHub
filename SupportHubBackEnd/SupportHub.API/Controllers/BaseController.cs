using System.Net.Mime;
using System.Security.Claims;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace SupportHub.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class BaseController : ControllerBase
{
    protected Result<Guid> UserId
    {
        get
        {
            var claim = HttpContext.User
                .Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            if (claim is null)
            {
                return Result.Failure<Guid>($"{nameof(claim)} cannot be null.");
            }

            var success = Guid.TryParse(claim.Value, out var userId);
            if (!success)
            {
                return Result.Failure<Guid>($"{nameof(userId)} cannot parse.");
            }

            return userId;
        }
    }
    
    protected Result<int> CompanyId
    {
        get
        {
            var claim = HttpContext.User
                .Claims
                .FirstOrDefault(x => x.Type == "CompanyId");

            if (claim is null)
            {
                return Result.Failure<int>($"{nameof(claim)} cannot be null.");
            }

            var success = int.TryParse(claim.Value, out var companyId);
            if (!success)
            {
                return Result.Failure<int>($"{nameof(companyId)} cannot parse.");
            }

            return companyId;
        }
    }

    /*protected Result<string> RefreshToken
    {
        get
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (refreshToken is null)
            {
                return Result.Failure<string>($"{nameof(refreshToken)} cannot be null.");
            }
            
            [
        }
    }*/
}