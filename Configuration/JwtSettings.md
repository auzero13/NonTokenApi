# JWT 設定參數說明

## JwtSettings 配置項

| 參數 | 類型 | 預設值 | 說明 |
|------|------|--------|------|
| **SecretKey** | string | 空字串 | JWT 簽章金鑰，**必須設定**。用於簽發和驗證 Token。建議至少 32 字元，最好 64 字元以上 |
| **Issuer** | string | "TokenApi" | Token 簽發者識別。驗證時會檢查 Token 的 `iss` 宣告是否匹配 |
| **Audience** | string | "TokenApi" | Token 預定接收者。驗證時會檢查 Token 的 `aud` 宣告是否匹配 |
| **ExpirationSeconds** | int | 60 | Token 有效時間（秒）。預設 60 秒，Token 過期後無法驗證通過 |
| **TokenParameterName** | string | "token" | 在 URL query string 中附加 Token 時使用的參數名稱 |

---

## 各參數詳細說明

### 🔐 SecretKey（簽章金鑰）

**最重要且必須設定的參數！**

```json
"SecretKey": "YourSuperSecretKeyHere_MustBeAtLeast32Characters_Long_ForSecurity!"
```

- **用途**：使用 HMAC-SHA256 演算法對 Token 進行數位簽章
- **安全性**：金鑰必須保密，只有 Token 簽發方和驗證方知道
- **長度**：
  - 最小長度：32 字元（256 bits，符合 HS256 要求）
  - 建議長度：64 字元以上
  - 可包含：大小寫字母、數字、特殊符號
- **產生方式**：可使用隨機字串產生器或密碼管理工具

⚠️ **警告**：
- 生產環境必須使用強隨機產生的金鑰
- 金鑰外洩 = 任何人可偽造 Token
- 定期輪替金鑰是良好安全實踐

---

### 🏢 Issuer（簽發者）

```json
"Issuer": "TokenApi"
```

- **用途**：識別誰簽發了這個 Token
- **Token 中的宣告**：`iss`: "TokenApi"
- **驗證邏輯**：驗證時檢查 Token 的 `iss` 是否與此設定匹配
- **使用情境**：
  - 多個 Token 服務時，區分來源
  - 防止不同系統間的 Token 被濫用

---

### 👥 Audience（接收者）

```json
"Audience": "TokenApi"
```

- **用途**：識別這個 Token 是給誰使用的
- **Token 中的宣告**：`aud`: "TokenApi"
- **驗證邏輯**：驗證時檢查 Token 的 `aud` 是否與此設定匹配
- **使用情境**：
  - Token 服務和目標 WEB 系統約定相同的 Audience
  - 防止 Token 被轉發到其他不相關系統

---

### ⏱️ ExpirationSeconds（有效時間）

```json
"ExpirationSeconds": 60
```

- **預設值**：60 秒
- **Token 中的宣告**：`exp`: 過期時間的 Unix 時間戳
- **用途**：限制 Token 的使用時間視窗，降低被竊取後濫用的風險
- **建議值**：
  - 極短暫操作：30-60 秒
  - 一般轉址：60-120 秒
  - 可根據實際需求調整

⚠️ **注意**：設太短可能導致用戶操作來不及完成，設太長安全性降低

---

### 🔗 TokenParameterName（URL 參數名稱）

```json
"TokenParameterName": "token"
```

- **用途**：決定 Token 附加在 URL 時使用的參數名稱
- **產生的 URL**：`https://example.com/page?token=eyJhbGciOiJIUzI1NiIs...`
- **可選值**：
  - `"token"`（預設）
  - `"jwt"`
  - `"access_token"`
  - 或任何自訂名稱

---

## 設定範例

### 開發環境（appsettings.Development.json）

```json
{
  "JwtSettings": {
    "SecretKey": "DevelopmentSecretKey_MustBeAtLeast32Characters_Long!",
    "Issuer": "TokenApi-Dev",
    "Audience": "TokenApi-Dev",
    "ExpirationSeconds": 60,
    "TokenParameterName": "token"
  }
}
```

### 生產環境（appsettings.json）

```json
{
  "JwtSettings": {
    "SecretKey": "YourProductionSecretKey_ShouldBeRandomAndAtLeast64CharsLong!!!",
    "Issuer": "TokenApi",
    "Audience": "YourTargetWebSystem",
    "ExpirationSeconds": 60,
    "TokenParameterName": "token"
  }
}
```

---

## Token 中的宣告對應

當 Token 生成後，JWT Payload 會包含以下宣告：

```json
{
  "jti": "550e8400e29b41d4a716446655440000",  // 唯一 Token ID
  "iat": 1711353600,                            // 簽發時間（Unix 時間戳）
  "exp": 1711353660,                            // 過期時間（Unix 時間戳）
  "iss": "TokenApi",                            // 簽發者
  "aud": "TokenApi",                            // 接收者
  "client_id": "web-app-001",                   // 客戶端 ID
  "target_url": "https://target-web.com/page",  // 目標 URL
  "userId": "12345",                            // 自訂宣告（如果有）
  "role": "admin"                               // 自訂宣告（如果有）
}
```

---

## 目標 WEB 驗證時需要什麼

目標 WEB 系統驗證 Token 時，需要知道：

1. **SecretKey**：與此服務相同的金鑰
2. **Issuer**：檢查 Token 的 `iss` 是否匹配
3. **Audience**：檢查 Token 的 `aud` 是否匹配
4. **時效驗證**：檢查 `exp` 是否已過期

這些資訊需要在 Token 簽發方和驗證方之間**事先約定**。
