using TokenApi.DTOs;

namespace TokenApi.Services;

/// <summary>
/// Token 服務介面
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// 生成無狀態 JWT Token
    /// </summary>
    /// <param name="clientId">客戶端識別碼</param>
    /// <param name="targetType">目標類型</param>
    /// <param name="parameters">額外參數</param>
    /// <param name="claims">自訂宣告</param>
    /// <returns>Token 回應資訊</returns>
    Task<TokenResponseDto> GenerateTokenAsync(
        string clientId,
        string targetType,
        Dictionary<string, string>? parameters,
        Dictionary<string, object>? claims = null);

    /// <summary>
    /// 驗證 JWT Token (無狀態，不檢查資料庫)
    /// </summary>
    /// <param name="token">JWT Token 字串</param>
    /// <returns>驗證結果</returns>
    Task<TokenValidateResponseDto> ValidateTokenAsync(string token);

    /// <summary>
    /// 解析 Token 取得宣告資料 (不驗證簽章)
    /// </summary>
    /// <param name="token">JWT Token 字串</param>
    /// <returns>宣告資料字典</returns>
    Dictionary<string, object>? ParseTokenClaims(string token);
}
