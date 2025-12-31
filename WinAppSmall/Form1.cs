using System.Text.Json;

namespace WinAppSmall;

public partial class Form1 : Form
{
    private const string ConfigFile = "config.json";
    private HashSet<string> monitoredPrograms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private System.Windows.Forms.Timer timer;
    private bool isExiting = false;
    private WindowMonitor? windowMonitor;

    public Form1()
    {
        InitializeComponent();
        
        // Create system tray icon
        notifyIcon.Icon = SystemIcons.Application;
        
        // Initialize timer to update UI
        timer = new System.Windows.Forms.Timer();
        timer.Interval = 2000; // Update every 2 seconds
        timer.Tick += Timer_Tick;
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        LoadConfig();
        RefreshProgramList();
        StartMonitoring();
        timer.Start();
        
        UpdateStatusLabel();
    }

    private void StartMonitoring()
    {
        if (windowMonitor != null)
        {
            windowMonitor.UpdateMonitoredPrograms(monitoredPrograms);
        }
        else
        {
            windowMonitor = new WindowMonitor(monitoredPrograms);
            windowMonitor.StatusChanged += WindowMonitor_StatusChanged;
        }
    }

    private void WindowMonitor_StatusChanged(object? sender, string message)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => WindowMonitor_StatusChanged(sender, message)));
            return;
        }
        
        notifyIcon.ShowBalloonTip(2000, "WinAppSmall", message, ToolTipIcon.Info);
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        RefreshProgramList();
        UpdateStatusLabel();
    }

    private void LoadConfig()
    {
        try
        {
            if (File.Exists(ConfigFile))
            {
                var json = File.ReadAllText(ConfigFile);
                var programs = JsonSerializer.Deserialize<List<string>>(json);
                if (programs != null)
                {
                    monitoredPrograms = new HashSet<string>(programs, StringComparer.OrdinalIgnoreCase);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"載入配置失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveConfig()
    {
        try
        {
            var json = JsonSerializer.Serialize(monitoredPrograms.ToList(), new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFile, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存配置失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshProgramList()
    {
        listViewPrograms.Items.Clear();
        
        foreach (var program in monitoredPrograms)
        {
            var item = new ListViewItem(program);
            var processes = System.Diagnostics.Process.GetProcessesByName(Path.GetFileNameWithoutExtension(program));
            var status = processes.Length > 0 ? $"運行中 ({processes.Length} 個實例)" : "未運行";
            item.SubItems.Add(status);
            listViewPrograms.Items.Add(item);
        }
    }

    private void UpdateStatusLabel()
    {
        int totalWindows = windowMonitor?.GetMonitoredWindowCount() ?? 0;
        lblStatus.Text = $"狀態: 運行中 | 監控程式: {monitoredPrograms.Count} | 活動窗口: {totalWindows}";
    }

    private void btnAdd_Click(object? sender, EventArgs e)
    {
        var programName = txtProgramName.Text.Trim();
        
        if (string.IsNullOrWhiteSpace(programName))
        {
            MessageBox.Show("請輸入程式名稱", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        
        if (!programName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            programName += ".exe";
        }
        
        if (monitoredPrograms.Contains(programName))
        {
            MessageBox.Show("該程式已在監控列表中", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        
        monitoredPrograms.Add(programName);
        SaveConfig();
        RefreshProgramList();
        txtProgramName.Clear();
        StartMonitoring(); // Restart monitoring with new program
        UpdateStatusLabel();
        
        MessageBox.Show($"已添加 {programName} 到監控列表。\n\n當您點擊該程式視窗的關閉按鈕(X)時，程式將最小化而不是關閉。", 
            "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnRemove_Click(object? sender, EventArgs e)
    {
        if (listViewPrograms.SelectedItems.Count == 0)
        {
            MessageBox.Show("請選擇要移除的程式", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        
        var programName = listViewPrograms.SelectedItems[0].Text;
        monitoredPrograms.Remove(programName);
        SaveConfig();
        RefreshProgramList();
        StartMonitoring(); // Restart monitoring
        UpdateStatusLabel();
    }

    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!isExiting)
        {
            e.Cancel = true;
            this.Hide();
            // ShowBalloonTip removed to prevent excessive notifications when minimizing to system tray
        }
        else
        {
            timer.Stop();
            windowMonitor?.Dispose();
        }
    }

    private void notifyIcon_DoubleClick(object? sender, EventArgs e)
    {
        ShowMainWindow();
    }

    private void toolStripMenuItemShow_Click(object? sender, EventArgs e)
    {
        ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.BringToFront();
        this.Activate();
    }

    private void toolStripMenuItemExit_Click(object? sender, EventArgs e)
    {
        isExiting = true;
        Application.Exit();
    }
}
