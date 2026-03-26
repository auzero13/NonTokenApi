using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TokenApi.Configuration;
using TokenApi.DTOs;

namespace TokenApi.Services;

/// <summary>
/// 無狀態 JWT Token 服務實作
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ITargetUrlResolver _urlResolver;
    private readonly ILogger<TokenService> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public TokenService(
        IOptions<JwtSettings> jwtSettings,
        ITargetUrlResolver urlResolver,
        ILogger<TokenService> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _urlResolver = urlResolver;
        _logger = logger;
    }

    /// <summary>
    /// 生成無狀態 JWT Token
    /// </summary>
    public Task<TokenResponseDto> GenerateTokenAsync(
        string clientId,
        string targetType,
        Dictionary<string, string>? parameters,
        Dictionary<string, object>? claims = null)
    {
        // 透過 Resolver 解析目標 URL
        var targetUrl = _urlResolver.Resolve(clientId, targetType, parameters);

        if (string.IsNullOrEmpty(targetUrl))
        {
            _logger.LogWarning(
                "無法解析目標 URL: ClientId={ClientId}, TargetType={TargetType}",
                clientId, targetType);

            throw new InvalidOperationException(
                $"無法解析目標 URL: TargetType={targetType}");
        }

        // 生成唯一 Token ID
        var tokenId = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;
        var expiresAt = now.AddSeconds(_jwtSettings.ExpirationSeconds);

        // 建立宣告（不包含 target_url，避免洩漏）
        var tokenClaims = BuildClaims(clientId, targetType, tokenId, now, expiresAt, claims);

        // 生成 Token
        var token = GenerateJwtToken(tokenClaims);

        // 建立帶 Token 的重導向 URL
        var redirectUrl = BuildRedirectUrl(targetUrl, token);

        _logger.LogInformation(
            "Token 已生成: TokenId={TokenId}, ClientId={ClientId}, TargetType={TargetType}, ExpiresAt={ExpiresAt}",
            tokenId, clientId, targetType, expiresAt);

        return Task.FromResult(new TokenResponseDto
        {
            Token = token,
            RedirectUrl = redirectUrl,
            ExpiresAt = expiresAt,
            ExpiresInSeconds = _jwtSettings.ExpirationSeconds
        });
    }

    /// <summary>
    /// 驗證 JWT Token (完全無狀態，不依賴資料庫)
    /// </summary>
    public Task<TokenValidateResponseDto> ValidateTokenAsync(string token)
    {
        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };

            // 驗證並解析 Token
            var principal = _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;

            // 取得宣告資料
            var tokenId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var clientId = jwtToken.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;
            var targetType = jwtToken.Claims.FirstOrDefault(c => c.Type == "target_type")?.Value;

            // 取得自訂宣告
            var customClaims = jwtToken.Claims
                .Where(c => !IsStandardClaim(c.Type))
                .ToDictionary(c => c.Type, c => (object)c.Value);

            _logger.LogInformation(
                "Token 驗證成功: TokenId={TokenId}, ClientId={ClientId}, TargetType={TargetType}",
                tokenId, clientId, targetType);

            return Task.FromResult(new TokenValidateResponseDto
            {
                IsValid = true,
                IsExpired = false,
                IsUsed = false,
                ClientId = clientId,
                TargetUrlName = targetType,
                Claims = customClaims.Count > 0 ? customClaims : null
            });
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning("Token 已過期: {Message}", ex.Message);

            var claims = ParseTokenClaims(token);
            return Task.FromResult(new TokenValidateResponseDto
            {
                IsValid = false,
                IsExpired = true,
                IsUsed = false,
                ClientId = claims?.GetValueOrDefault("client_id")?.ToString(),
                TargetUrlName = claims?.GetValueOrDefault("target_type")?.ToString(),
                Claims = claims,
                ErrorMessage = "Token 已過期"
            });
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning("Token 驗證失敗: {Message}", ex.Message);
            return Task.FromResult(new TokenValidateResponseDto
            {
                IsValid = false,
                IsExpired = false,
                IsUsed = false,
                ErrorMessage = $"Token 驗證失敗: {ex.Message}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token 驗證發生錯誤");
            return Task.FromResult(new TokenValidateResponseDto
            {
                IsValid = false,
                IsExpired = false,
                IsUsed = false,
                ErrorMessage = $"Token 驗證發生錯誤: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// 解析 Token 取得宣告資料 (不驗證簽章)
    /// </summary>
    public Dictionary<string, object>? ParseTokenClaims(string token)
    {
        try
        {
            var jwtToken = _tokenHandler.ReadJwtToken(token);
            return jwtToken.Claims
                .ToDictionary(c => c.Type, c => (object)c.Value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "無法解析 Token");
            return null;
        }
    }

    #region Private Methods

    private List<Claim> BuildClaims(
        string clientId,
        string targetType,
        string tokenId,
        DateTime now,
        DateTime expiresAt,
        Dictionary<string, object>? customClaims)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, tokenId),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Exp, new DateTimeOffset(expiresAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer),
            new(JwtRegisteredClaimNames.Aud, _jwtSettings.Audience),
            new("client_id", clientId),
            new("target_type", targetType)  // 改用 target_type，不暴露實際 URL
        };

        if (customClaims != null)
        {
            foreach (var claim in customClaims)
            {
                var claimValue = claim.Value switch
                {
                    string s => s,
                    int i => i.ToString(),
                    long l => l.ToString(),
                    bool b => b.ToString().ToLower(),
                    DateTime dt => dt.ToString("O"),
                    _ => JsonSerializer.Serialize(claim.Value)
                };
                claims.Add(new Claim(claim.Key, claimValue));
            }
        }

        return claims;
    }

    private string GenerateJwtToken(List<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddSeconds(_jwtSettings.ExpirationSeconds),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = credentials
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    private string BuildRedirectUrl(string baseUrl, string token)
    {
        var separator = baseUrl.Contains('?') ? "&" : "?";
        return $"{baseUrl}{separator}{_jwtSettings.TokenParameterName}={Uri.EscapeDataString(token)}";
    }

    private static bool IsStandardClaim(string claimType)
    {
        return claimType.StartsWith("iss") ||
               claimType.StartsWith("aud") ||
               claimType.StartsWith("exp") ||
               claimType.StartsWith("nbf") ||
               claimType.StartsWith("iat") ||
               claimType.StartsWith("jti") ||
               claimType.StartsWith("client_id") ||
               claimType.StartsWith("target_type");
    }

    #endregion
}
