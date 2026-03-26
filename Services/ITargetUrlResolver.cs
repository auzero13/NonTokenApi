namespace TokenApi.Services;

/// <summary>
/// 目標 URL 解析器介面
/// </summary>
public interface ITargetUrlResolver
{
    /// <summary>
    /// 根據客戶端、目標類型和參數解析目標 URL
    /// </summary>
    /// <param name="clientId">客戶端識別碼</param>
    /// <param name="targetType">目標類型</param>
    /// <param name="parameters">額外參數</param>
    /// <returns>解析後的目標 URL，如果無法解析則返回 null</returns>
    string? Resolve(string clientId, string targetType, Dictionary<string, string>? parameters);
}
