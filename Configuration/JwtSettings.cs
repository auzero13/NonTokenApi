namespace TokenApi.Configuration;

/// <summary>
/// JWT 設定選項
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// 設定鍵值常數
    /// </summary>
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// 簽章金鑰 (至少 32 字元，建議 64 字元以上)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 簽發者
    /// </summary>
    public string Issuer { get; set; } = "TokenApi";

    /// <summary>
    /// 驗證對象
    /// </summary>
    public string Audience { get; set; } = "TokenApi";

    /// <summary>
    /// Token 有效秒數 (預設 60 秒)
    /// </summary>
    public int ExpirationSeconds { get; set; } = 60;

    /// <summary>
    /// Token 參數名稱 (用於 URL query string)
    /// </summary>
    public string TokenParameterName { get; set; } = "token";
}
