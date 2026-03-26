using System.ComponentModel.DataAnnotations;

namespace TokenApi.DTOs;

/// <summary>
/// Token 請求 DTO
/// </summary>
public record TokenRequestDto
{
    /// <summary>
    /// 客戶端識別碼
    /// </summary>
    [Required(ErrorMessage = "ClientId 為必填欄位")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "ClientId 長度需在 1-100 字元之間")]
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// 目標類型/位置識別（用於後端判斷目標 URL）
    /// 例如："dashboard", "admin", "report", "module-a" 等
    /// </summary>
    [Required(ErrorMessage = "TargetType 為必填欄位")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "TargetType 長度需在 1-100 字元之間")]
    public string TargetType { get; init; } = string.Empty;

    /// <summary>
    /// 額外參數（用於後端判斷目標 URL）
    /// 例如：地區、部門、或其他業務相關參數
    /// </summary>
    public Dictionary<string, string>? Parameters { get; init; }

    /// <summary>
    /// 自訂宣告資料 (選填，將包含在 JWT payload 中)
    /// </summary>
    public Dictionary<string, object>? Claims { get; init; }
}

/// <summary>
/// Token 回應 DTO
/// </summary>
public record TokenResponseDto
{
    /// <summary>
    /// JWT Token 字串
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// 帶有 Token 的完整目標 URL (query string 格式)
    /// </summary>
    public string RedirectUrl { get; init; } = string.Empty;

    /// <summary>
    /// Token 過期時間 (UTC)
    /// </summary>
    public DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Token 有效秒數
    /// </summary>
    public int ExpiresInSeconds { get; init; }
}

/// <summary>
/// Token 驗證請求 DTO (SPA 端使用)
/// </summary>
public record TokenValidateRequestDto
{
    /// <summary>
    /// 要驗證的 JWT Token
    /// </summary>
    [Required(ErrorMessage = "Token 為必填欄位")]
    public string Token { get; init; } = string.Empty;
}

/// <summary>
/// Token 驗證回應 DTO (SPA 端使用)
/// </summary>
public record TokenValidateResponseDto
{
    /// <summary>
    /// 驗證是否成功
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Token 是否已過期
    /// </summary>
    public bool IsExpired { get; init; }

    /// <summary>
    /// Token 是否已使用
    /// </summary>
    public bool IsUsed { get; init; }

    /// <summary>
    /// 客戶端識別碼
    /// </summary>
    public string? ClientId { get; init; }

    /// <summary>
    /// 目標 URL 名稱
    /// </summary>
    public string? TargetUrlName { get; init; }

    /// <summary>
    /// Token 中的自訂宣告資料
    /// </summary>
    public Dictionary<string, object>? Claims { get; init; }

    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 驗證時間 (UTC)
    /// </summary>
    public DateTime ValidatedAt { get; init; } = DateTime.UtcNow;
}
