using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TokenApi.DTOs;
using TokenApi.Services;

namespace TokenApi.Controllers;

/// <summary>
/// Token API 控制器 - 負責生成與驗證 JWT Token
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<TokenController> _logger;

    public TokenController(ITokenService tokenService, ILogger<TokenController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// 生成無狀態 JWT Token 並回傳帶有 Token 的重導向 URL
    /// </summary>
    /// <param name="request">Token 請求資料</param>
    /// <returns>包含 Token 和重導向 URL 的回應</returns>
    /// <remarks>
    /// 範例請求:
    /// 
    ///     POST /api/token/generate
    ///     {
    ///         "clientId": "web-app-001",
    ///         "targetType": "dashboard",
    ///         "parameters": {
    ///             "region": "tw",
    ///             "department": "sales"
    ///         },
    ///         "claims": {
    ///             "userId": "12345",
    ///             "role": "admin"
    ///         }
    ///     }
    /// 
    /// </remarks>
    /// <response code="200">成功生成 Token</response>
    /// <response code="400">請求參數錯誤</response>
    /// <response code="404">無法解析目標 URL</response>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateToken([FromBody] TokenRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _tokenService.GenerateTokenAsync(
                request.ClientId,
                request.TargetType,
                request.Parameters,
                request.Claims);

            _logger.LogInformation(
                "Token 已成功生成: ClientId={ClientId}, TargetType={TargetType}",
                request.ClientId, request.TargetType);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "生成 Token 失敗: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成 Token 時發生錯誤");
            return StatusCode(500, new { error = "內部伺服器錯誤" });
        }
    }

    /// <summary>
    /// 驗證 JWT Token (供 SPA 前端使用)
    /// </summary>
    /// <param name="request">驗證請求，包含要驗證的 Token</param>
    /// <returns>Token 驗證結果</returns>
    /// <remarks>
    /// 此端點供 SPA 前端呼叫，用於驗證從 query string 取得的 Token 是否有效。
    /// 
    /// 範例請求:
    /// 
    ///     POST /api/token/validate
    ///     {
    ///         "token": "eyJhbGciOiJIUzI1NiIs..."
    ///     }
    /// 
    /// </remarks>
    /// <response code="200">驗證完成 (不論 Token 是否有效)</response>
    /// <response code="400">請求參數錯誤</response>
    [HttpPost("validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenValidateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateToken([FromBody] TokenValidateRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new TokenValidateResponseDto
            {
                IsValid = false,
                ErrorMessage = "Token 不可為空"
            });
        }

        var result = await _tokenService.ValidateTokenAsync(request.Token);

        if (result.IsValid)
        {
            _logger.LogInformation("Token 驗證成功: ClientId={ClientId}", result.ClientId);
        }
        else
        {
            _logger.LogWarning("Token 驗證失敗: {Error}", result.ErrorMessage);
        }

        return Ok(result);
    }

    /// <summary>
    /// 快速驗證 Token (供目標 WEB 使用，使用約定金鑰驗證)
    /// </summary>
    /// <param name="token">要驗證的 JWT Token</param>
    /// <returns>Token 驗證結果</returns>
    /// <remarks>
    /// 此端點供目標 WEB 系統使用，透過約定金鑰驗證 Token 是否有效。
    /// Token 會在 60 秒後過期。
    /// </remarks>
    /// <response code="200">驗證完成</response>
    /// <response code="401">Token 無效或已過期</response>
    [HttpGet("verify")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenValidateResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyToken([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new TokenValidateResponseDto
            {
                IsValid = false,
                ErrorMessage = "Token 不可為空"
            });
        }

        var result = await _tokenService.ValidateTokenAsync(token);

        if (!result.IsValid)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }
}
