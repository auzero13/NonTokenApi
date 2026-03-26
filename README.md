# NonTokenApi - Stateless JWT Service

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512bd4.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

NonTokenApi 是一個基於 .NET 8 構建的**無狀態權杖服務**。專為系統間的安全跳轉與身份傳遞而設計，提供短效期、高安全性的 JWT (JSON Web Tokens) 解決方案。

---

## 🚀 核心功能 (Core Features)

- **短效期權杖產生**：預設 60 秒效期，大幅降低 Token 被截獲重放的風險。
- **無狀態驗證 (Stateless)**：服務端不儲存 Session，具備極佳的水平擴展能力。
- **抽象目標導向**：透過 `TargetType` 解析 URL，避免在前端暴露內部實體路徑。
- **動態 Claims 支援**：支援在權杖中封裝自訂商務邏輯與權限資訊。
- **標準化整合**：遵循 JWT (RFC 7519) 與 HS256 簽署規範。

---

## 🛠 技術棧 (Tech Stack)

- **Framework**: .NET 8 Web API
- **Security**: JWT Bearer Authentication (Microsoft.AspNetCore.Authentication.JwtBearer)
- **Documentation**: Swagger/OpenAPI
- **CI/CD Friendly**: 易於容器化並整合至微服務架構中。

---

## 📖 快速入門 (Quick Start)

### 1. 配置
在 `appsettings.json` 中設定您的 JWT 密鑰與效期：

```json
"JwtSettings": {
  "SecretKey": "YOUR_SUPER_SECRET_KEY_HERE",
  "ExpirationSeconds": 60
}
```

### 2. 產生存取權杖 (POST /api/token/generate)
```bash
curl -X POST http://localhost:5000/api/token/generate \
-H "Content-Type: application/json" \
-d '{
  "clientId": "App01",
  "targetType": "Dashboard",
  "claims": { "role": "Admin" }
}'
```

### 3. 驗證權杖
前端或目標系統可呼叫 `/api/token/validate` 或 `/api/token/verify` 來確保權杖的合法性。

---

## 📂 相關文件 (Documentation)

更詳細的 API 使用說明與安全性規範分析，請參閱：
👉 [**TokenApi 使用說明與安全性分析報告**](./Document/TokenApi_Documentation.md)

---

## 📄 授權 (License)

本專案採用 MIT 授權條款。詳情請參閱 [LICENSE](LICENSE) 檔案。
