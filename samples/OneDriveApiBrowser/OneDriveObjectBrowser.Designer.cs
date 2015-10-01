namespace OneDriveApiBrowser
{
    partial class OneDriveObjectBrowser
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.propertyGridBrowser = new System.Windows.Forms.PropertyGrid();
            this.textBoxRawJson = new System.Windows.Forms.TextBox();
            this.treeViewProperties = new System.Windows.Forms.TreeView();
            this.contextMenuStripProperty = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel2 = new System.Windows.Forms.Panel();
            this.comboBoxPropertyFormat = new System.Windows.Forms.ComboBox();
            this.labelSelectedItemProperties = new System.Windows.Forms.Label();
            this.contextMenuStripProperty.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // propertyGridBrowser
            // 
            this.propertyGridBrowser.Location = new System.Drawing.Point(15, 278);
            this.propertyGridBrowser.Margin = new System.Windows.Forms.Padding(2);
            this.propertyGridBrowser.Name = "propertyGridBrowser";
            this.propertyGridBrowser.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.propertyGridBrowser.Size = new System.Drawing.Size(211, 84);
            this.propertyGridBrowser.TabIndex = 11;
            this.propertyGridBrowser.ToolbarVisible = false;
            this.propertyGridBrowser.Visible = false;
            // 
            // textBoxRawJson
            // 
            this.textBoxRawJson.Location = new System.Drawing.Point(15, 165);
            this.textBoxRawJson.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxRawJson.Multiline = true;
            this.textBoxRawJson.Name = "textBoxRawJson";
            this.textBoxRawJson.ReadOnly = true;
            this.textBoxRawJson.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxRawJson.Size = new System.Drawing.Size(213, 109);
            this.textBoxRawJson.TabIndex = 10;
            this.textBoxRawJson.Visible = false;
            // 
            // treeViewProperties
            // 
            this.treeViewProperties.ContextMenuStrip = this.contextMenuStripProperty;
            this.treeViewProperties.Location = new System.Drawing.Point(15, 36);
            this.treeViewProperties.Margin = new System.Windows.Forms.Padding(2);
            this.treeViewProperties.Name = "treeViewProperties";
            this.treeViewProperties.Size = new System.Drawing.Size(213, 126);
            this.treeViewProperties.TabIndex = 8;
            // 
            // contextMenuStripProperty
            // 
            this.contextMenuStripProperty.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyValueToolStripMenuItem,
            this.copyRowToolStripMenuItem});
            this.contextMenuStripProperty.Name = "contextMenuStripProperty";
            this.contextMenuStripProperty.Size = new System.Drawing.Size(154, 52);
            // 
            // copyValueToolStripMenuItem
            // 
            this.copyValueToolStripMenuItem.Name = "copyValueToolStripMenuItem";
            this.copyValueToolStripMenuItem.Size = new System.Drawing.Size(153, 24);
            this.copyValueToolStripMenuItem.Text = "Copy Value";
            this.copyValueToolStripMenuItem.Click += new System.EventHandler(this.copyValueToolStripMenuItem_Click);
            // 
            // copyRowToolStripMenuItem
            // 
            this.copyRowToolStripMenuItem.Name = "copyRowToolStripMenuItem";
            this.copyRowToolStripMenuItem.Size = new System.Drawing.Size(153, 24);
            this.copyRowToolStripMenuItem.Text = "Copy Row";
            this.copyRowToolStripMenuItem.Click += new System.EventHandler(this.copyRowToolStripMenuItem_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.comboBoxPropertyFormat);
            this.panel2.Controls.Add(this.labelSelectedItemProperties);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(468, 21);
            this.panel2.TabIndex = 9;
            // 
            // comboBoxPropertyFormat
            // 
            this.comboBoxPropertyFormat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxPropertyFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPropertyFormat.FormattingEnabled = true;
            this.comboBoxPropertyFormat.Items.AddRange(new object[] {
            "JSON",
            "TreeView",
            "Object Model"});
            this.comboBoxPropertyFormat.Location = new System.Drawing.Point(379, 0);
            this.comboBoxPropertyFormat.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxPropertyFormat.Name = "comboBoxPropertyFormat";
            this.comboBoxPropertyFormat.Size = new System.Drawing.Size(90, 21);
            this.comboBoxPropertyFormat.TabIndex = 5;
            this.comboBoxPropertyFormat.SelectedIndexChanged += new System.EventHandler(this.comboBoxPropertyFormat_SelectedIndexChanged);
            // 
            // labelSelectedItemProperties
            // 
            this.labelSelectedItemProperties.AutoSize = true;
            this.labelSelectedItemProperties.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelSelectedItemProperties.Location = new System.Drawing.Point(0, 0);
            this.labelSelectedItemProperties.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelSelectedItemProperties.Name = "labelSelectedItemProperties";
            this.labelSelectedItemProperties.Size = new System.Drawing.Size(144, 15);
            this.labelSelectedItemProperties.TabIndex = 4;
            this.labelSelectedItemProperties.Text = "Selected Item Properties:";
            // 
            // OneDriveObjectBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.propertyGridBrowser);
            this.Controls.Add(this.textBoxRawJson);
            this.Controls.Add(this.treeViewProperties);
            this.Controls.Add(this.panel2);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "OneDriveObjectBrowser";
            this.Size = new System.Drawing.Size(468, 372);
            this.contextMenuStripProperty.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGridBrowser;
        private System.Windows.Forms.TextBox textBoxRawJson;
        private System.Windows.Forms.TreeView treeViewProperties;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ComboBox comboBoxPropertyFormat;
        private System.Windows.Forms.Label labelSelectedItemProperties;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripProperty;
        private System.Windows.Forms.ToolStripMenuItem copyValueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyRowToolStripMenuItem;
    }
}
