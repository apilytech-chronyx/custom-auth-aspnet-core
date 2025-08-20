using CustomAuth.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CustomAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {

        // This endpoint only requires standard JWT validation
        [HttpGet("jwt-only")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult JwtOnlyEndpoint()
        {
            return Ok(new { Message = "JWT validated successfully" });
        }

        // This endpoint requires combined authentication (JWT + user profile)
        [HttpPost("full-auth")]
        [Authorize(AuthenticationSchemes = "CombinedScheme")]
        public IActionResult FullAuthEndpoint([FromBody] RequestData requestData)
        {
            // Move this to asp.net core middleware
            if (Request.Headers.TryGetValue("x-soap-operation", out var soapOperation))
            {
                HttpContext.TrackChange("OperationName", "", soapOperation);
            }

            // Access claims from both JWT and user profile. Eventually one day jwt only. Nothing changes here
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var permissions = User.Claims.Where(c => c.Type == "Permission").Select(c => c.Value);

            HttpContext.TrackChange("FirstName", "old value", requestData.SampleData1);

            return Ok(new { UserId = userId, Permissions = permissions });
        }
    }
}
