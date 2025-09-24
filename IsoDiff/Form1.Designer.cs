namespace FolderDiff;

partial class Form1
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        menuStrip1 = new MenuStrip();
        fileToolStripMenuItem = new ToolStripMenuItem();
        createPatchesToolStripMenuItem = new ToolStripMenuItem();
        applyPatchesToolStripMenuItem = new ToolStripMenuItem();
        exitToolStripMenuItem = new ToolStripMenuItem();
        btnStart = new Button();
        lblFolderPathLabel = new Label();
        lblFolderPath = new Label();
        lblSelectParentFolder = new Label();
        cboParentFolder = new ComboBox();
        lblProcessingFileLabel = new Label();
        txtProcessingFile = new TextBox();
        label1 = new Label();
        chkDeleteFoldersOnComplete = new CheckBox();
        lblProcessingMode = new Label();
        cboProcessingMode = new ComboBox();
        menuStrip1.SuspendLayout();
        SuspendLayout();
        // 
        // menuStrip1
        // 
        menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
        menuStrip1.Location = new Point(0, 0);
        menuStrip1.Name = "menuStrip1";
        menuStrip1.Size = new Size(879, 24);
        menuStrip1.TabIndex = 0;
        menuStrip1.Text = "menuStrip1";
        // 
        // fileToolStripMenuItem
        // 
        fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { createPatchesToolStripMenuItem, applyPatchesToolStripMenuItem, exitToolStripMenuItem });
        fileToolStripMenuItem.Name = "fileToolStripMenuItem";
        fileToolStripMenuItem.Size = new Size(37, 20);
        fileToolStripMenuItem.Text = "File";
        // 
        // createPatchesToolStripMenuItem
        // 
        createPatchesToolStripMenuItem.Name = "createPatchesToolStripMenuItem";
        createPatchesToolStripMenuItem.Size = new Size(152, 22);
        createPatchesToolStripMenuItem.Text = "&Create Patches";
        createPatchesToolStripMenuItem.Click += createPatchesToolStripMenuItem_Click;
        // 
        // applyPatchesToolStripMenuItem
        // 
        applyPatchesToolStripMenuItem.Name = "applyPatchesToolStripMenuItem";
        applyPatchesToolStripMenuItem.Size = new Size(152, 22);
        applyPatchesToolStripMenuItem.Text = "&Apply Patches";
        applyPatchesToolStripMenuItem.Click += applyPatchesToolStripMenuItem_Click;
        // 
        // exitToolStripMenuItem
        // 
        exitToolStripMenuItem.Name = "exitToolStripMenuItem";
        exitToolStripMenuItem.Size = new Size(152, 22);
        exitToolStripMenuItem.Text = "E&xit";
        exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
        // 
        // btnStart
        // 
        btnStart.Location = new Point(345, 470);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(75, 23);
        btnStart.TabIndex = 1;
        btnStart.Text = "Start";
        btnStart.UseVisualStyleBackColor = true;
        btnStart.Click += btnStart_Click;
        // 
        // lblFolderPathLabel
        // 
        lblFolderPathLabel.AutoSize = true;
        lblFolderPathLabel.Location = new Point(54, 157);
        lblFolderPathLabel.Name = "lblFolderPathLabel";
        lblFolderPathLabel.Size = new Size(67, 15);
        lblFolderPathLabel.TabIndex = 2;
        lblFolderPathLabel.Text = "Folder Path";
        // 
        // lblFolderPath
        // 
        lblFolderPath.AutoSize = true;
        lblFolderPath.Location = new Point(197, 157);
        lblFolderPath.Name = "lblFolderPath";
        lblFolderPath.Size = new Size(48, 15);
        lblFolderPath.TabIndex = 3;
        lblFolderPath.Text = "thePath";
        // 
        // lblSelectParentFolder
        // 
        lblSelectParentFolder.AutoSize = true;
        lblSelectParentFolder.Location = new Point(54, 208);
        lblSelectParentFolder.Name = "lblSelectParentFolder";
        lblSelectParentFolder.Size = new Size(111, 15);
        lblSelectParentFolder.TabIndex = 4;
        lblSelectParentFolder.Text = "Select Parent Folder";
        // 
        // cboParentFolder
        // 
        cboParentFolder.DropDownStyle = ComboBoxStyle.DropDownList;
        cboParentFolder.FormattingEnabled = true;
        cboParentFolder.Location = new Point(197, 200);
        cboParentFolder.Name = "cboParentFolder";
        cboParentFolder.Size = new Size(655, 23);
        cboParentFolder.TabIndex = 5;
        // 
        // lblProcessingFileLabel
        // 
        lblProcessingFileLabel.AutoSize = true;
        lblProcessingFileLabel.Location = new Point(73, 386);
        lblProcessingFileLabel.Name = "lblProcessingFileLabel";
        lblProcessingFileLabel.Size = new Size(85, 15);
        lblProcessingFileLabel.TabIndex = 6;
        lblProcessingFileLabel.Text = "Processing File";
        // 
        // txtProcessingFile
        // 
        txtProcessingFile.Enabled = false;
        txtProcessingFile.ForeColor = SystemColors.WindowText;
        txtProcessingFile.Location = new Point(190, 385);
        txtProcessingFile.Name = "txtProcessingFile";
        txtProcessingFile.ReadOnly = true;
        txtProcessingFile.Size = new Size(662, 23);
        txtProcessingFile.TabIndex = 11;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(51, 260);
        label1.Name = "label1";
        label1.Size = new Size(0, 15);
        label1.TabIndex = 12;
        // 
        // chkDeleteFoldersOnComplete
        // 
        chkDeleteFoldersOnComplete.AutoSize = true;
        chkDeleteFoldersOnComplete.Location = new Point(53, 295);
        chkDeleteFoldersOnComplete.Name = "chkDeleteFoldersOnComplete";
        chkDeleteFoldersOnComplete.Size = new Size(248, 19);
        chkDeleteFoldersOnComplete.TabIndex = 13;
        chkDeleteFoldersOnComplete.Text = "Delete Non-Parent Folders on Completion";
        chkDeleteFoldersOnComplete.UseVisualStyleBackColor = true;
        // 
        // lblProcessingMode
        // 
        lblProcessingMode.AutoSize = true;
        lblProcessingMode.Location = new Point(54, 244);
        lblProcessingMode.Name = "lblProcessingMode";
        lblProcessingMode.Size = new Size(98, 15);
        lblProcessingMode.TabIndex = 14;
        lblProcessingMode.Text = "Processing Mode";
        // 
        // cboProcessingMode
        // 
        cboProcessingMode.DropDownStyle = ComboBoxStyle.DropDownList;
        cboProcessingMode.FormattingEnabled = true;
        cboProcessingMode.Items.AddRange(new object[] { "BsDiff", "Chunked BsDiff (experimental)" });
        cboProcessingMode.Location = new Point(197, 247);
        cboProcessingMode.Name = "cboProcessingMode";
        cboProcessingMode.Size = new Size(223, 23);
        cboProcessingMode.TabIndex = 15;
        cboProcessingMode.SelectedIndexChanged += cboProcessingMode_SelectedIndexChanged;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(879, 496);
        Controls.Add(cboProcessingMode);
        Controls.Add(lblProcessingMode);
        Controls.Add(chkDeleteFoldersOnComplete);
        Controls.Add(label1);
        Controls.Add(txtProcessingFile);
        Controls.Add(lblProcessingFileLabel);
        Controls.Add(cboParentFolder);
        Controls.Add(lblSelectParentFolder);
        Controls.Add(lblFolderPath);
        Controls.Add(lblFolderPathLabel);
        Controls.Add(btnStart);
        Controls.Add(menuStrip1);
        MainMenuStrip = menuStrip1;
        Name = "Form1";
        Text = "FolderDiff";
        Load += Form1_Load;
        menuStrip1.ResumeLayout(false);
        menuStrip1.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private MenuStrip menuStrip1;
    private ToolStripMenuItem fileToolStripMenuItem;
    private ToolStripMenuItem createPatchesToolStripMenuItem;
    private Button btnStart;
    private Label lblFolderPathLabel;
    private Label lblFolderPath;
    private Label lblSelectParentFolder;
    private ComboBox cboParentFolder;
    private ToolStripMenuItem exitToolStripMenuItem;
    private ToolStripMenuItem applyPatchesToolStripMenuItem;
    private Label lblProcessingFileLabel;
    private TextBox txtProcessingFile;
    private Label label1;
    private CheckBox chkDeleteFoldersOnComplete;
    private Label lblProcessingMode;
    private ComboBox cboProcessingMode;
}
