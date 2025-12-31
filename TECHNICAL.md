# 技術說明文件

## 程式架構

### 主要組件

1. **Form1.cs** - 主要使用者界面
   - 程式列表管理
   - 系統托盤整合
   - 配置管理

2. **WindowMonitor.cs** - 核心監控邏輯
   - Windows 事件攔截
   - 視窗狀態追蹤
   - 關閉操作轉換為最小化

3. **Program.cs** - 應用程式入口點

## 技術實現細節

### Windows API Hooks

本應用程式使用 `SetWinEventHook` API 來監聽系統級的視窗事件：

```csharp
SetWinEventHook(EVENT_OBJECT_DESTROY, EVENT_OBJECT_DESTROY,
    IntPtr.Zero, hookDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
```

監聽的事件類型：
- **EVENT_OBJECT_DESTROY (0x8001)** - 視窗即將被銷毀時觸發
- **EVENT_OBJECT_HIDE (0x8003)** - 視窗被隱藏時觸發
- **EVENT_SYSTEM_MINIMIZESTART (0x0016)** - 視窗開始最小化時觸發（用於區分用戶主動最小化）

### 工作原理

1. **程式啟動**
   - 載入配置檔案（config.json）
   - 初始化 Windows 事件 hook
   - 開始追蹤指定程式的視窗

2. **視窗追蹤**
   - 每 500ms 掃描一次所有監控程式的視窗
   - 記錄視窗控制碼（HWND）和可見性狀態
   - 維護視窗與程式的對應關係

3. **關閉攔截**
   - 當監測到 EVENT_OBJECT_HIDE 或 EVENT_OBJECT_DESTROY 事件
   - 檢查該視窗是否屬於監控列表中的程式
   - 如果視窗正在被關閉（而非用戶主動最小化）
   - 調用 `ShowWindow(hwnd, SW_MINIMIZE)` 將視窗最小化

4. **狀態檢查**
   - 使用 `IsIconic()` 判斷視窗是否已經最小化
   - 使用 `IsWindowVisible()` 判斷視窗可見性
   - 區分用戶主動最小化和程式關閉操作

### 關鍵 API 函數

| API 函數 | 用途 |
|---------|------|
| SetWinEventHook | 設置全域視窗事件鉤子 |
| UnhookWinEvent | 移除事件鉤子 |
| GetWindowThreadProcessId | 獲取視窗所屬的程式 ID |
| ShowWindow | 控制視窗顯示狀態 |
| IsWindow | 檢查視窗控制碼是否有效 |
| IsWindowVisible | 檢查視窗是否可見 |
| IsIconic | 檢查視窗是否最小化 |

## 限制與注意事項

### 已知限制

1. **權限限制**
   - 無法攔截以更高權限運行的程式（例如：以管理員身份運行的程式）
   - 建議以管理員身份運行 WinAppSmall 以獲得更好的兼容性

2. **某些程式的特殊行為**
   - 某些程式使用自定義的視窗關閉邏輯，可能繞過標準的 Windows 消息處理
   - 例如：某些瀏覽器、遊戲程式可能有特殊的關閉機制

3. **多視窗程式**
   - 目前主要追蹤每個程式的主視窗（MainWindowHandle）
   - 子視窗或多文件介面（MDI）的次要視窗可能無法完全攔截

4. **效能考量**
   - 全域 hook 會輕微影響系統效能
   - 每 500ms 的輪詢會有少量 CPU 使用

### 無法攔截的情況

以下情況無法攔截程式關閉：

1. 透過工作管理員強制結束程式
2. 程式內部調用 `Environment.Exit()` 或 `Application.Exit()`
3. 系統關機或登出
4. 程式崩潰
5. 透過命令列 `taskkill` 強制結束

### 建議使用場景

適合使用的情況：
- ✅ 常用的生產力工具（如記事本、小算盤）
- ✅ 聊天軟體（希望誤關時保持在後台）
- ✅ 音樂播放器
- ✅ 文字編輯器

不建議使用的情況：
- ❌ 系統關鍵程式
- ❌ 安全相關的應用
- ❌ 需要及時釋放資源的程式

## 配置檔案格式

`config.json` 範例：

```json
[
  "notepad.exe",
  "calc.exe",
  "chrome.exe"
]
```

## 編譯選項

### Debug 版本
```bash
dotnet build -c Debug
```

### Release 版本
```bash
dotnet build -c Release
```

### 獨立可執行檔（包含 .NET Runtime）
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

這會生成一個獨立的 EXE 檔案，不需要安裝 .NET Runtime。

## 未來改進方向

1. **更精確的關閉偵測**
   - 使用低層級的鍵盤/滑鼠 hook 偵測 Alt+F4 和點擊關閉按鈕
   - 使用 CBT hook 進行更早期的攔截

2. **更好的 UI**
   - 新增程式圖示顯示
   - 新增快速切換監控狀態的功能
   - 新增視窗預覽功能

3. **更多功能**
   - 設定不同程式的不同行為（最小化/隱藏/移到托盤）
   - 設定白名單（某些視窗可以正常關閉）
   - 快速鍵支援

4. **效能優化**
   - 減少輪詢頻率
   - 使用更高效的視窗追蹤機制
   - 減少記憶體使用

## 疑難排解

### 程式無法攔截某個應用

1. 確認程式名稱正確（包含 .exe 副檔名）
2. 嘗試以管理員身份運行 WinAppSmall
3. 檢查目標程式是否正在運行
4. 查看狀態列的活動視窗數量是否正確

### 程式沒有最小化反而關閉了

1. 某些程式可能使用特殊的關閉機制
2. 嘗試以管理員身份運行
3. 檢查是否是透過工作管理員或其他方式強制關閉

### 系統效能問題

1. 減少監控的程式數量
2. 關閉不需要監控的程式
3. 檢查是否有其他衝突的監控軟體

## 授權與免責聲明

本工具為開源專案，使用者需自行承擔使用風險。開發者不對因使用本工具造成的任何資料遺失或系統問題負責。
