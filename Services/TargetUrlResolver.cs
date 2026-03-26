using Microsoft.Extensions.Logging;

namespace TokenApi.Services;

/// <summary>
/// 目標 URL 解析器實作
/// 根據 clientId、targetType 和參數決定目標 URL
/// </summary>
public class TargetUrlResolver : ITargetUrlResolver
{
    private readonly ILogger<TargetUrlResolver> _logger;

    public TargetUrlResolver(ILogger<TargetUrlResolver> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 解析目標 URL
    /// </summary>
    public string? Resolve(string clientId, string targetType, Dictionary<string, string>? parameters)
    {
        _logger.LogInformation(
            "解析目標 URL: ClientId={ClientId}, TargetType={TargetType}, Parameters={Params}",
            clientId, targetType, parameters != null ? string.Join(",", parameters.Keys) : "無");

        // ==========================================
        // 在此實作您的商業邏輯
        // ==========================================

        var targetUrl = targetType.ToLower() switch
        {
            // 範例：根據 targetType 決定目標 URL
            "dashboard" => ResolveDashboardUrl(clientId, parameters),
            "admin" => ResolveAdminUrl(clientId, parameters),
            "report" => ResolveReportUrl(clientId, parameters),
            "module-a" => ResolveModuleAUrl(clientId, parameters),
            "module-b" => ResolveModuleBUrl(clientId, parameters),

            // 預設處理
            _ => ResolveDefaultUrl(clientId, targetType, parameters)
        };

        if (targetUrl != null)
        {
            _logger.LogInformation("目標 URL 已解析: {TargetUrl}", targetUrl);
        }
        else
        {
            _logger.LogWarning("無法解析目標 URL: ClientId={ClientId}, TargetType={TargetType}", clientId, targetType);
        }

        return targetUrl;
    }

    #region 私有方法 - 實作您的商業邏輯

    /// <summary>
    /// 解析 Dashboard URL
    /// </summary>
    private string? ResolveDashboardUrl(string clientId, Dictionary<string, string>? parameters)
    {
        // 範例邏輯：根據參數決定不同的 dashboard URL
        var baseUrl = "https://dashboard.example.com";

        if (parameters != null)
        {
            // 如果有 region 參數，導向對應區域的 dashboard
            if (parameters.TryGetValue("region", out var region))
            {
                return region.ToLower() switch
                {
                    "tw" => $"https://dashboard-tw.example.com",
                    "us" => $"https://dashboard-us.example.com",
                    "cn" => $"https://dashboard-cn.example.com",
                    _ => baseUrl
                };
            }

            // 如果有 department 參數
            if (parameters.TryGetValue("department", out var department))
            {
                return $"{baseUrl}/dept/{Uri.EscapeDataString(department)}";
            }
        }

        return baseUrl;
    }

    /// <summary>
    /// 解析 Admin URL
    /// </summary>
    private string? ResolveAdminUrl(string clientId, Dictionary<string, string>? parameters)
    {
        // 範例：只有特定 clientId 可以存取 admin
        var allowedClients = new[] { "admin-portal", "super-admin" };

        if (!allowedClients.Contains(clientId.ToLower()))
        {
            _logger.LogWarning("ClientId {ClientId} 無權存取 Admin", clientId);
            return null;
        }

        return "https://admin.example.com";
    }

    /// <summary>
    /// 解析 Report URL
    /// </summary>
    private string? ResolveReportUrl(string clientId, Dictionary<string, string>? parameters)
    {
        var baseUrl = "https://reports.example.com";

        if (parameters != null && parameters.TryGetValue("type", out var type))
        {
            return $"{baseUrl}/{Uri.EscapeDataString(type)}";
        }

        return baseUrl;
    }

    /// <summary>
    /// 解析 ModuleA URL
    /// </summary>
    private string? ResolveModuleAUrl(string clientId, Dictionary<string, string>? parameters)
    {
        // 實作 Module A 的邏輯
        return "https://module-a.example.com";
    }

    /// <summary>
    /// 解析 ModuleB URL
    /// </summary>
    private string? ResolveModuleBUrl(string clientId, Dictionary<string, string>? parameters)
    {
        // 實作 Module B 的邏輯
        return "https://module-b.example.com";
    }

    /// <summary>
    /// 預設 URL 解析
    /// </summary>
    private string? ResolveDefaultUrl(string clientId, string targetType, Dictionary<string, string>? parameters)
    {
        // 可根據需求實作預設邏輯
        // 目前返回 null 表示無法解析
        _logger.LogWarning(
            "未知的 TargetType: {TargetType}，ClientId: {ClientId}",
            targetType, clientId);

        return null;
    }

    #endregion
}
