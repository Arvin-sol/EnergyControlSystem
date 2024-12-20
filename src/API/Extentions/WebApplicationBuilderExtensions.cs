using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace API.Extentions;

public static class WebApplicationBuilderExtensions
{
    private static TokenValidationParameters GetValidationparameters()
    {
        var res = ValidationParameters.GetValidationparameters();
        return res;
    }
    public static WebApplicationBuilder AddAppAuthetication(this WebApplicationBuilder builder)
    {

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {

            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = GetValidationparameters();
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {

                    if (context.Exception != null)
                        throw new AppException(ApiResultStatusCode.UnAuthorized, "Authentication failed.", HttpStatusCode.Unauthorized, context.Exception, null);

                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {

                    if (context.AuthenticateFailure != null)
                        throw new AppException(ApiResultStatusCode.UnAuthorized, "Authenticate failure.", HttpStatusCode.Unauthorized, context.AuthenticateFailure, null);
                    throw new AppException(ApiResultStatusCode.UnAuthorized, "You are unauthorized to access this resource.", HttpStatusCode.Unauthorized);

                },
                OnTokenValidated = context =>
                {
                    // Decompress the roles from the token
                    var rolesClaim = context.Principal.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                    var serviceChannelClaim = context.Principal.Claims.FirstOrDefault(c => c.Type == "serviceChannel");
                    const string ServiceChannelClaimType = "serviceChannel";
                    var identity = (ClaimsIdentity)context.Principal.Identity;


                    identity.AddClaim(new Claim(ServiceChannelClaimType, serviceChannelClaim.ToString()));
                    if (rolesClaim != null)
                    {
                        string compressedRoles = rolesClaim.Value;
                        List<string> roles = ClaimOptimizationHelper.DecompressRoles(compressedRoles);
                        string rolesString = string.Join(",", roles);

                        // Remove the compressed roles claim

                        identity.RemoveClaim(rolesClaim);

                        // Add the decompressed roles as claims to the principal
                        identity.AddClaim(new Claim(ClaimTypes.Role, rolesString));

                        // Add roles as claims to the token
                        var userRoles = rolesString.Split(',');
                        foreach (var role in userRoles)
                            identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }

                    return Task.CompletedTask;
                }
            };
        });

        return builder;
    }

}
