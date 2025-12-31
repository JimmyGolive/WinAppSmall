namespace WinAppSmall;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        listViewPrograms = new ListView();
        columnHeader1 = new ColumnHeader();
        columnHeader2 = new ColumnHeader();
        btnAdd = new Button();
        btnRemove = new Button();
        txtProgramName = new TextBox();
        label1 = new Label();
        notifyIcon = new NotifyIcon(components);
        contextMenuStrip = new ContextMenuStrip(components);
        toolStripMenuItemShow = new ToolStripMenuItem();
        toolStripMenuItemExit = new ToolStripMenuItem();
        lblStatus = new Label();
        contextMenuStrip.SuspendLayout();
        SuspendLayout();
        // 
        // listViewPrograms
        // 
        listViewPrograms.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
        listViewPrograms.FullRowSelect = true;
        listViewPrograms.Location = new Point(12, 70);
        listViewPrograms.Name = "listViewPrograms";
        listViewPrograms.Size = new Size(760, 300);
        listViewPrograms.TabIndex = 0;
        listViewPrograms.UseCompatibleStateImageBehavior = false;
        listViewPrograms.View = View.Details;
        // 
        // columnHeader1
        // 
        columnHeader1.Text = "程式名稱";
        columnHeader1.Width = 300;
        // 
        // columnHeader2
        // 
        columnHeader2.Text = "狀態";
        columnHeader2.Width = 450;
        // 
        // btnAdd
        // 
        btnAdd.Location = new Point(542, 32);
        btnAdd.Name = "btnAdd";
        btnAdd.Size = new Size(100, 30);
        btnAdd.TabIndex = 1;
        btnAdd.Text = "添加";
        btnAdd.UseVisualStyleBackColor = true;
        btnAdd.Click += btnAdd_Click;
        // 
        // btnRemove
        // 
        btnRemove.Location = new Point(672, 32);
        btnRemove.Name = "btnRemove";
        btnRemove.Size = new Size(100, 30);
        btnRemove.TabIndex = 2;
        btnRemove.Text = "移除";
        btnRemove.UseVisualStyleBackColor = true;
        btnRemove.Click += btnRemove_Click;
        // 
        // txtProgramName
        // 
        txtProgramName.Location = new Point(120, 35);
        txtProgramName.Name = "txtProgramName";
        txtProgramName.PlaceholderText = "例如: notepad.exe";
        txtProgramName.Size = new Size(400, 27);
        txtProgramName.TabIndex = 3;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(12, 38);
        label1.Name = "label1";
        label1.Size = new Size(99, 20);
        label1.TabIndex = 4;
        label1.Text = "程式名稱:";
        // 
        // notifyIcon
        // 
        notifyIcon.ContextMenuStrip = contextMenuStrip;
        notifyIcon.Text = "WinAppSmall";
        notifyIcon.Visible = true;
        notifyIcon.DoubleClick += notifyIcon_DoubleClick;
        // 
        // contextMenuStrip
        // 
        contextMenuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItemShow, toolStripMenuItemExit });
        contextMenuStrip.Name = "contextMenuStrip";
        contextMenuStrip.Size = new Size(125, 52);
        // 
        // toolStripMenuItemShow
        // 
        toolStripMenuItemShow.Name = "toolStripMenuItemShow";
        toolStripMenuItemShow.Size = new Size(124, 24);
        toolStripMenuItemShow.Text = "顯示";
        toolStripMenuItemShow.Click += toolStripMenuItemShow_Click;
        // 
        // toolStripMenuItemExit
        // 
        toolStripMenuItemExit.Name = "toolStripMenuItemExit";
        toolStripMenuItemExit.Size = new Size(124, 24);
        toolStripMenuItemExit.Text = "退出";
        toolStripMenuItemExit.Click += toolStripMenuItemExit_Click;
        // 
        // lblStatus
        // 
        lblStatus.AutoSize = true;
        lblStatus.Location = new Point(12, 380);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(99, 20);
        lblStatus.TabIndex = 5;
        lblStatus.Text = "狀態: 運行中";
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(9F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(784, 411);
        Controls.Add(lblStatus);
        Controls.Add(label1);
        Controls.Add(txtProgramName);
        Controls.Add(btnRemove);
        Controls.Add(btnAdd);
        Controls.Add(listViewPrograms);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "WinAppSmall - 程式最小化工具";
        FormClosing += Form1_FormClosing;
        Load += Form1_Load;
        contextMenuStrip.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private ListView listViewPrograms;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private Button btnAdd;
    private Button btnRemove;
    private TextBox txtProgramName;
    private Label label1;
    private NotifyIcon notifyIcon;
    private ContextMenuStrip contextMenuStrip;
    private ToolStripMenuItem toolStripMenuItemShow;
    private ToolStripMenuItem toolStripMenuItemExit;
    private Label lblStatus;
}
