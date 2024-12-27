using Common.Extension;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Confige = Common.Utilities;

namespace API.Extentions;

public static class ValidationParameters
{
    public static TokenValidationParameters GetValidationParameters()
    {
        var secretKey = Encoding.UTF8.GetBytes(Confige.ConfigurationManager.GetValue("JwtSettings:SecretKey"));
        var encryptionKey = Encoding.UTF8.GetBytes(Confige.ConfigurationManager.GetValue("JwtSettings:EncryptKey"));
        var audience = Confige.ConfigurationManager.GetValue("JwtSettings:Audience");
        var issuer = Confige.ConfigurationManager.GetValue("JwtSettings:Issuer");

        var validationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.Zero, // default: 5 min
            RequireSignedTokens = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),

            RequireExpirationTime = true,
            ValidateLifetime = true,

            ValidateAudience = true, //default : false
            ValidAudience = audience,

            ValidateIssuer = true, //default : false
            ValidIssuer = issuer,

            TokenDecryptionKey = new SymmetricSecurityKey(encryptionKey)
        };

        return validationParameters;
    }
}