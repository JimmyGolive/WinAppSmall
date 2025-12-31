# WinAppSmall - Windows 程式最小化工具

小工具:可以指定Windows那些的程式 點選 右上角 X 不要關閉程式 只做程式的最小化到工作列

## 功能說明

這是一個 Windows 應用程式，可以攔截指定程式的關閉按鈕（視窗右上角的 X），當使用者點擊關閉按鈕時，程式不會真的關閉，而是最小化到工作列。

### 主要功能

- ✅ 添加要監控的程式名稱（例如：notepad.exe、chrome.exe）
- ✅ 自動偵測並監控指定程式的視窗
- ✅ 攔截關閉按鈕點擊事件，改為最小化操作
- ✅ 系統托盤圖示，可隱藏到系統托盤後台運行
- ✅ 自動保存配置，重啟後仍保留監控列表
- ✅ 即時顯示監控狀態和活動視窗數量

## 系統需求

- Windows 作業系統 (Windows 10/11)
- .NET 8.0 Runtime 或更高版本

## 編譯和運行

### 編譯專案

```bash
cd WinAppSmall
dotnet build
```

### 運行程式

```bash
cd WinAppSmall
dotnet run
```

或者直接運行編譯後的可執行檔：

```bash
cd WinAppSmall/bin/Debug/net8.0-windows
./WinAppSmall.exe
```

### 發布獨立可執行檔

```bash
cd WinAppSmall
dotnet publish -c Release -r win-x64 --self-contained
```

發布後的檔案位於：`WinAppSmall/bin/Release/net8.0-windows/win-x64/publish/`

## 使用說明

### 1. 啟動程式

運行 WinAppSmall.exe，主視窗會顯示程式管理介面。

### 2. 添加要監控的程式

1. 在「程式名稱」文字框中輸入要監控的程式名稱
   - 例如：`notepad.exe`（記事本）
   - 例如：`chrome.exe`（Chrome 瀏覽器）
   - 例如：`calc.exe`（計算機）
2. 點擊「添加」按鈕
3. 程式會自動添加到監控列表

### 3. 移除監控的程式

1. 在列表中選擇要移除的程式
2. 點擊「移除」按鈕

### 4. 測試功能

1. 啟動一個已添加到監控列表的程式（例如記事本）
2. 點擊該程式視窗右上角的關閉按鈕（X）
3. 程式會最小化到工作列，而不是關閉

### 5. 系統托盤

- 點擊 WinAppSmall 主視窗的關閉按鈕，程式會最小化到系統托盤
- 雙擊系統托盤圖示可重新顯示主視窗
- 右鍵點擊系統托盤圖示可選擇「顯示」或「退出」

## 技術實現

本工具使用以下技術：

- **C# Windows Forms**：建立使用者介面
- **Windows API Hooks (SetWinEventHook)**：監聽視窗事件
- **EVENT_OBJECT_DESTROY 和 EVENT_OBJECT_HIDE**：攔截視窗關閉/隱藏事件
- **ShowWindow API**：將視窗最小化
- **Process Management**：追蹤和管理監控的程式
- **JSON 配置**：持久化保存監控列表

## 注意事項

⚠️ **重要說明**

1. 本工具需要在 Windows 作業系統上運行
2. 某些系統程式或有特殊權限的程式可能無法被監控
3. 程式必須以程式名稱（例如 notepad.exe）來指定，不是視窗標題
4. 監控會在程式啟動後自動生效，無需重啟已運行的程式
5. 配置會自動保存在 `config.json` 檔案中

## 常見問題

### Q: 為什麼某些程式無法被監控？

A: 某些程式使用特殊的關閉機制或有較高的權限，可能無法被本工具攔截。建議以管理員身份運行 WinAppSmall。

### Q: 如何完全關閉被監控的程式？

A: 可以透過以下方式關閉：
- 使用工作管理員（Task Manager）結束程式
- 在程式內部使用「檔案」→「結束」等選單選項
- 暫時從 WinAppSmall 移除該程式的監控

### Q: 程式會影響效能嗎？

A: 本工具使用 Windows 系統事件機制，對系統資源消耗極低，不會明顯影響效能。

## 授權

本專案為開源專案，可自由使用和修改。

## 貢獻

歡迎提交 Issue 和 Pull Request！

