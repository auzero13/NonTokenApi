# TokenApi 使用說明與安全性分析報告

本文件旨在說明 `TokenApi` 專案的功能用途、API 使用方式，並針對其採用的安全性規範進行業界標準分析。

---

## 第一部分：TokenApi 使用說明與用途

### 1. 專案概述 (Project Overview)
`TokenApi` 是一個基於 .NET 8 構建的**無狀態權杖服務**。它專門設計用於提供短效期的 JWT (JSON Web Tokens)，協助前端 SPA 或不同系統間進行安全跳轉與身份傳遞。此服務不儲存任何 Session 資訊，完全依賴數位簽章來驗證權杖的合法性。

### 2. 核心用途 (Core Purpose)
*   **安全跳轉與導向**：當用戶從系統 A 需要跳轉到系統 B 時，透過此 API 產生一個內含身份資訊的短效期權杖，並將其附加在目標 URL 中。
*   **身份傳遞與委派**：在無狀態架構下，將用戶的 Client ID 或自訂權限（Claims）封裝在權杖中，讓目標系統能在不重新登入的情況下辨識用戶。
*   **SPA 驗證**：前端單頁應用程式（SPA）在載入時，可透過此服務驗證 URL 中的權杖是否有效，進而決定是否允許進入特定功能。

### 3. API 使用說明

#### 3.1 產生存取權杖 (Generate Token)
*   **端點**：`POST /api/token/generate`
*   **用途**：根據目標類型解析 URL 並產生簽署後的權杖。
*   **請求範例 (JSON)**：
    ```json
    {
      "clientId": "InternalApp01",
      "targetType": "Dashboard",
      "parameters": { "region": "North" },
      "claims": { "role": "Admin", "dept": "IT" }
    }
    ```
*   **回應範例**：
    ```json
    {
      "token": "eyJhbGciOiJIUzI1Ni...",
      "redirectUrl": "https://target-app.com/dashboard?token=eyJhbGci...",
      "expiresAt": "2026-03-26T14:05:00Z"
    }
    ```

#### 3.2 驗證權杖 (Validate/Verify Token)
*   **前端驗證**：`POST /api/token/validate` (傳入 Token DTO)
*   **後端快速驗證**：`GET /api/token/verify?token={token}`
*   **用途**：確認權杖未過期且簽章正確。若驗證成功，會回傳該權杖內含的所有 Claims 資訊。

### 4. 權杖生命週期與配置
*   **演算法**：HMAC-SHA256 (HS256)
*   **效期設計**：預設為 **60 秒**。此短效期設計確保權杖僅作為「一次性跳轉」使用，大幅降低被截獲後惡意使用的風險。
*   **配置位置**：於 `appsettings.json` 的 `JwtSettings` 段落配置 `SecretKey` 與 `ExpirationSeconds`。

---

## 第二部分：安全性規範分析報告

### 1. 採用的安全性標準
本專案嚴格遵循以下業界標準：
*   **JWT (RFC 7519)**：使用 JSON Web Token 標準封裝資訊，具備自我包含（Self-contained）特性，不需查閱資料庫即可驗證身份。
*   **JWS (JSON Web Signature)**：透過 HS256 簽署，確保 Token 在傳輸過程中未被篡改。
*   **無狀態設計 (Stateless)**：服務端不保存狀態，避免了 Session 固定攻擊 (Session Fixation) 的風險，並提高水平擴展能力。

### 2. 業界廣泛程度
**極高。** 
JWT 目前是現代 Web 開發（特別是微服務架構與 SPA）中身份驗證的**事實標準 (De facto standard)**。各大身份業者如 Auth0, Okta, Azure AD 以及雲端 API Gateway 均原生支援此規範。

### 3. 安全性優點評估
*   **精確的時鐘容差 (Clock Skew)**：專案中配置 `ClockSkew = TimeSpan.Zero`，代表不允許任何時間誤差，確保 60 秒到期即失效。
*   **抽象目標解析 (URL Obfuscation)**：呼叫者僅提供 `TargetType`，避免內部系統實體路徑直接暴露給前端。
*   **Payload 完整性**：透過數位簽章保護 Claims，防止用戶自行修改參數。

### 4. 安全規範分析建議
1.  **重放攻擊預防 (Replay Attack)**：建議在驗證後將 Token ID (`jti`) 記錄於快取並標記為已使用，實現真正的「一次性 Token」。
2.  **端點存取控制**：確保 `/api/token/generate` 端點僅允許受信任的內部 IP 或持有特定 API Key 的服務呼叫。
3.  **機敏資訊處理**：請確保 Claims 中**不要包含密碼、個資 (PII) 或未加密的敏感數據**。

---
*文件產生日期：2026年3月26日*
