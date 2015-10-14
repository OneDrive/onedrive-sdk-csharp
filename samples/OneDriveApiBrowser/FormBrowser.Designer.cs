namespace OneDriveApiBrowser
{
    partial class FormBrowser
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.flowLayoutContents = new System.Windows.Forms.FlowLayoutPanel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.flowLayoutPanelBreadcrumb = new System.Windows.Forms.FlowLayoutPanel();
            this.linkLabelOneDriveRoot = new System.Windows.Forms.LinkLabel();
            this.objectBrowser = new OneDriveApiBrowser.OneDriveObjectBrowser();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.signInAadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.signInMsaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.signOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.getChangesHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteSelectedItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uploadFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.simpleUploadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.simpleIDbasedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chunkedUploadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bITSParallelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.createFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.downloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSelectedFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pickerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFromOneDriveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uploadToOneDriveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.flowLayoutPanelBreadcrumb.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutContents
            // 
            this.flowLayoutContents.AutoScroll = true;
            this.flowLayoutContents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutContents.Location = new System.Drawing.Point(3, 42);
            this.flowLayoutContents.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.flowLayoutContents.Name = "flowLayoutContents";
            this.flowLayoutContents.Size = new System.Drawing.Size(661, 402);
            this.flowLayoutContents.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 32);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.flowLayoutContents);
            this.splitContainer1.Panel1.Controls.Add(this.flowLayoutPanelBreadcrumb);
            this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(3, 0, 0, 3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.objectBrowser);
            this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(0, 0, 3, 3);
            this.splitContainer1.Size = new System.Drawing.Size(988, 447);
            this.splitContainer1.SplitterDistance = 664;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 5;
            // 
            // flowLayoutPanelBreadcrumb
            // 
            this.flowLayoutPanelBreadcrumb.Controls.Add(this.linkLabelOneDriveRoot);
            this.flowLayoutPanelBreadcrumb.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanelBreadcrumb.Location = new System.Drawing.Point(3, 0);
            this.flowLayoutPanelBreadcrumb.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.flowLayoutPanelBreadcrumb.Name = "flowLayoutPanelBreadcrumb";
            this.flowLayoutPanelBreadcrumb.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.flowLayoutPanelBreadcrumb.Size = new System.Drawing.Size(661, 42);
            this.flowLayoutPanelBreadcrumb.TabIndex = 2;
            this.flowLayoutPanelBreadcrumb.Visible = false;
            // 
            // linkLabelOneDriveRoot
            // 
            this.linkLabelOneDriveRoot.AutoSize = true;
            this.linkLabelOneDriveRoot.Location = new System.Drawing.Point(2, 2);
            this.linkLabelOneDriveRoot.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.linkLabelOneDriveRoot.Name = "linkLabelOneDriveRoot";
            this.linkLabelOneDriveRoot.Size = new System.Drawing.Size(58, 15);
            this.linkLabelOneDriveRoot.TabIndex = 0;
            this.linkLabelOneDriveRoot.TabStop = true;
            this.linkLabelOneDriveRoot.Text = "OneDrive";
            this.linkLabelOneDriveRoot.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelBreadcrumb_LinkClicked);
            // 
            // objectBrowser
            // 
            this.objectBrowser.DisplayFormat = OneDriveApiBrowser.PropertyDisplayFormat.TreeNode;
            this.objectBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.objectBrowser.Location = new System.Drawing.Point(0, 0);
            this.objectBrowser.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.objectBrowser.Name = "objectBrowser";
            this.objectBrowser.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.objectBrowser.SelectedItem = null;
            this.objectBrowser.Size = new System.Drawing.Size(318, 444);
            this.objectBrowser.TabIndex = 0;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(893, 6);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(92, 15);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 4;
            this.progressBar1.Value = 100;
            this.progressBar1.Visible = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.uploadFileToolStripMenuItem,
            this.downloadToolStripMenuItem,
            this.pickerToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 6);
            this.menuStrip1.Size = new System.Drawing.Size(988, 32);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.signInAadToolStripMenuItem,
            this.signInMsaToolStripMenuItem,
            this.signOutToolStripMenuItem,
            this.toolStripMenuItem3,
            this.getChangesHereToolStripMenuItem,
            this.deleteSelectedItemToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // signInAadToolStripMenuItem
            // 
            this.signInAadToolStripMenuItem.Name = "signInAadToolStripMenuItem";
            this.signInAadToolStripMenuItem.Size = new System.Drawing.Size(258, 24);
            this.signInAadToolStripMenuItem.Text = "Sign In to AAD...";
            this.signInAadToolStripMenuItem.Click += new System.EventHandler(this.signInAadToolStripMenuItem_Click);
            // 
            // signInMsaToolStripMenuItem
            // 
            this.signInMsaToolStripMenuItem.Name = "signInMsaToolStripMenuItem";
            this.signInMsaToolStripMenuItem.Size = new System.Drawing.Size(258, 24);
            this.signInMsaToolStripMenuItem.Text = "Sign In to MSA...";
            this.signInMsaToolStripMenuItem.Click += new System.EventHandler(this.signInMsaToolStripMenuItem_Click);
            // 
            // signOutToolStripMenuItem
            // 
            this.signOutToolStripMenuItem.Name = "signOutToolStripMenuItem";
            this.signOutToolStripMenuItem.Size = new System.Drawing.Size(258, 24);
            this.signOutToolStripMenuItem.Text = "Sign Out";
            this.signOutToolStripMenuItem.Visible = false;
            this.signOutToolStripMenuItem.Click += new System.EventHandler(this.signOutToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(255, 6);
            // 
            // getChangesHereToolStripMenuItem
            // 
            this.getChangesHereToolStripMenuItem.Name = "getChangesHereToolStripMenuItem";
            this.getChangesHereToolStripMenuItem.Size = new System.Drawing.Size(258, 24);
            this.getChangesHereToolStripMenuItem.Text = "Get Changes Here";
            this.getChangesHereToolStripMenuItem.Click += new System.EventHandler(this.getChangesHereToolStripMenuItem_Click);

            // 
            // deleteSelectedItemToolStripMenuItem
            // 
            this.deleteSelectedItemToolStripMenuItem.Name = "deleteSelectedItemToolStripMenuItem";
            this.deleteSelectedItemToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteSelectedItemToolStripMenuItem.Size = new System.Drawing.Size(258, 24);
            this.deleteSelectedItemToolStripMenuItem.Text = "Delete Selected Item...";
            this.deleteSelectedItemToolStripMenuItem.Click += new System.EventHandler(this.deleteSelectedItemToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(255, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(258, 24);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // uploadFileToolStripMenuItem
            // 
            this.uploadFileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.simpleUploadToolStripMenuItem,
            this.simpleIDbasedToolStripMenuItem,
            this.chunkedUploadToolStripMenuItem,
            this.bITSParallelToolStripMenuItem,
            this.toolStripMenuItem2,
            this.createFolderToolStripMenuItem});
            this.uploadFileToolStripMenuItem.Name = "uploadFileToolStripMenuItem";
            this.uploadFileToolStripMenuItem.Size = new System.Drawing.Size(70, 24);
            this.uploadFileToolStripMenuItem.Text = "Upload";
            // 
            // simpleUploadToolStripMenuItem
            // 
            this.simpleUploadToolStripMenuItem.Name = "simpleUploadToolStripMenuItem";
            this.simpleUploadToolStripMenuItem.Size = new System.Drawing.Size(213, 24);
            this.simpleUploadToolStripMenuItem.Text = "Simple - Path-based";
            this.simpleUploadToolStripMenuItem.Click += new System.EventHandler(this.simpleUploadToolStripMenuItem_Click);

            // 
            // simpleIDbasedToolStripMenuItem
            // 
            this.simpleIDbasedToolStripMenuItem.Name = "simpleIDbasedToolStripMenuItem";
            this.simpleIDbasedToolStripMenuItem.Size = new System.Drawing.Size(213, 24);
            this.simpleIDbasedToolStripMenuItem.Text = "Simple - ID-based";
            this.simpleIDbasedToolStripMenuItem.Click += new System.EventHandler(this.simpleIDbasedToolStripMenuItem_Click);

            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(210, 6);

            // 
            // createFolderToolStripMenuItem
            // 
            this.createFolderToolStripMenuItem.Name = "createFolderToolStripMenuItem";
            this.createFolderToolStripMenuItem.Size = new System.Drawing.Size(213, 24);
            this.createFolderToolStripMenuItem.Text = "Create Folder...";
            this.createFolderToolStripMenuItem.Click += new System.EventHandler(this.createFolderToolStripMenuItem_Click);

            // 
            // downloadToolStripMenuItem
            // 
            this.downloadToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveSelectedFileToolStripMenuItem});
            this.downloadToolStripMenuItem.Name = "downloadToolStripMenuItem";
            this.downloadToolStripMenuItem.Size = new System.Drawing.Size(90, 24);
            this.downloadToolStripMenuItem.Text = "Download";
            // 
            // saveSelectedFileToolStripMenuItem
            // 
            this.saveSelectedFileToolStripMenuItem.Name = "saveSelectedFileToolStripMenuItem";
            this.saveSelectedFileToolStripMenuItem.Size = new System.Drawing.Size(206, 24);
            this.saveSelectedFileToolStripMenuItem.Text = "Save Selected File...";
            this.saveSelectedFileToolStripMenuItem.Click += new System.EventHandler(this.saveSelectedFileToolStripMenuItem_Click);

            // 
            // pickerToolStripMenuItem
            // 
            this.pickerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFromOneDriveToolStripMenuItem,
            this.uploadToOneDriveToolStripMenuItem});
            this.pickerToolStripMenuItem.Name = "pickerToolStripMenuItem";
            this.pickerToolStripMenuItem.Size = new System.Drawing.Size(60, 24);
            this.pickerToolStripMenuItem.Text = "Picker";
            // 
            // openFromOneDriveToolStripMenuItem
            // 
            this.openFromOneDriveToolStripMenuItem.Name = "openFromOneDriveToolStripMenuItem";
            this.openFromOneDriveToolStripMenuItem.Size = new System.Drawing.Size(225, 24);
            this.openFromOneDriveToolStripMenuItem.Text = "Open from OneDrive...";
            this.openFromOneDriveToolStripMenuItem.Click += new System.EventHandler(this.openFromOneDriveToolStripMenuItem_Click);
            // 
            // uploadToOneDriveToolStripMenuItem
            // 
            this.uploadToOneDriveToolStripMenuItem.Name = "uploadToOneDriveToolStripMenuItem";
            this.uploadToOneDriveToolStripMenuItem.Size = new System.Drawing.Size(225, 24);
            this.uploadToOneDriveToolStripMenuItem.Text = "Upload to OneDrive...";
            // 
            // FormBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(988, 479);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "FormBrowser";
            this.Text = "OneDrive API Browser";
            this.Load += new System.EventHandler(this.FormBrowser_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.flowLayoutPanelBreadcrumb.ResumeLayout(false);
            this.flowLayoutPanelBreadcrumb.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutContents;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelBreadcrumb;
        private System.Windows.Forms.LinkLabel linkLabelOneDriveRoot;
        private OneDriveObjectBrowser objectBrowser;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem signInAadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem signInMsaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem signOutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uploadFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem simpleUploadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chunkedUploadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem simpleIDbasedToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem createFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem deleteSelectedItemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem getChangesHereToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bITSParallelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pickerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFromOneDriveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uploadToOneDriveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem downloadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveSelectedFileToolStripMenuItem;
    }
}

