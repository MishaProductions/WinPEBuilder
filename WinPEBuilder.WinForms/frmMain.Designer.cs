namespace WinPEBuilder.WinForms
{
    partial class frmMain
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
            this.label1 = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.chkDWM = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkUWP = new System.Windows.Forms.CheckBox();
            this.chkExplorer = new System.Windows.Forms.CheckBox();
            this.chkLogonUI = new System.Windows.Forms.CheckBox();
            this.btnBuild = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.SourceTab = new System.Windows.Forms.TabPage();
            this.btnSelectISO = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtIsoPath = new System.Windows.Forms.TextBox();
            this.OptionsTab = new System.Windows.Forms.TabPage();
            this.OutputTab = new System.Windows.Forms.TabPage();
            this.btnSelectVHDOutput = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtOutFile = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.radISO = new System.Windows.Forms.RadioButton();
            this.radVHD = new System.Windows.Forms.RadioButton();
            this.ProgressTab = new System.Windows.Forms.TabPage();
            this.lblProgress = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.VHDDialog = new System.Windows.Forms.SaveFileDialog();
            this.SelISODialog = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SourceTab.SuspendLayout();
            this.OptionsTab.SuspendLayout();
            this.OutputTab.SuspendLayout();
            this.ProgressTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "MishaPE builder";
            // 
            // lblVersion
            // 
            this.lblVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(13, 425);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(91, 13);
            this.lblVersion.TabIndex = 1;
            this.lblVersion.Text = "Version: loading";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.flowLayoutPanel1);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(410, 167);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.chkDWM);
            this.flowLayoutPanel1.Controls.Add(this.panel1);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(6, 21);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(398, 140);
            this.flowLayoutPanel1.TabIndex = 1;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // chkDWM
            // 
            this.chkDWM.AutoSize = true;
            this.chkDWM.Checked = true;
            this.chkDWM.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDWM.Location = new System.Drawing.Point(3, 3);
            this.chkDWM.Name = "chkDWM";
            this.chkDWM.Size = new System.Drawing.Size(55, 17);
            this.chkDWM.TabIndex = 3;
            this.chkDWM.Text = "DWM";
            this.chkDWM.UseVisualStyleBackColor = true;
            this.chkDWM.CheckedChanged += new System.EventHandler(this.chkDWM_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.chkUWP);
            this.panel1.Controls.Add(this.chkExplorer);
            this.panel1.Controls.Add(this.chkLogonUI);
            this.panel1.Location = new System.Drawing.Point(3, 26);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 100);
            this.panel1.TabIndex = 1;
            // 
            // chkUWP
            // 
            this.chkUWP.AutoSize = true;
            this.chkUWP.Location = new System.Drawing.Point(3, 3);
            this.chkUWP.Name = "chkUWP";
            this.chkUWP.Size = new System.Drawing.Size(96, 17);
            this.chkUWP.TabIndex = 4;
            this.chkUWP.Text = "UWP Support";
            this.chkUWP.UseVisualStyleBackColor = true;
            // 
            // chkExplorer
            // 
            this.chkExplorer.AutoSize = true;
            this.chkExplorer.Checked = true;
            this.chkExplorer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExplorer.Location = new System.Drawing.Point(3, 49);
            this.chkExplorer.Name = "chkExplorer";
            this.chkExplorer.Size = new System.Drawing.Size(68, 17);
            this.chkExplorer.TabIndex = 0;
            this.chkExplorer.Text = "Explorer";
            this.chkExplorer.UseVisualStyleBackColor = true;
            // 
            // chkLogonUI
            // 
            this.chkLogonUI.AutoSize = true;
            this.chkLogonUI.Checked = true;
            this.chkLogonUI.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLogonUI.Location = new System.Drawing.Point(3, 26);
            this.chkLogonUI.Name = "chkLogonUI";
            this.chkLogonUI.Size = new System.Drawing.Size(114, 17);
            this.chkLogonUI.TabIndex = 2;
            this.chkLogonUI.Text = "LogonUI support";
            this.chkLogonUI.UseVisualStyleBackColor = true;
            // 
            // btnBuild
            // 
            this.btnBuild.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBuild.Location = new System.Drawing.Point(459, 415);
            this.btnBuild.Name = "btnBuild";
            this.btnBuild.Size = new System.Drawing.Size(75, 23);
            this.btnBuild.TabIndex = 7;
            this.btnBuild.Text = "Build";
            this.btnBuild.UseVisualStyleBackColor = true;
            this.btnBuild.Click += new System.EventHandler(this.btnBuild_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.SourceTab);
            this.tabControl1.Controls.Add(this.OptionsTab);
            this.tabControl1.Controls.Add(this.OutputTab);
            this.tabControl1.Controls.Add(this.ProgressTab);
            this.tabControl1.Location = new System.Drawing.Point(18, 41);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(516, 368);
            this.tabControl1.TabIndex = 8;
            // 
            // SourceTab
            // 
            this.SourceTab.Controls.Add(this.btnSelectISO);
            this.SourceTab.Controls.Add(this.label3);
            this.SourceTab.Controls.Add(this.txtIsoPath);
            this.SourceTab.Location = new System.Drawing.Point(4, 22);
            this.SourceTab.Name = "SourceTab";
            this.SourceTab.Padding = new System.Windows.Forms.Padding(3);
            this.SourceTab.Size = new System.Drawing.Size(508, 342);
            this.SourceTab.TabIndex = 1;
            this.SourceTab.Text = "Source";
            this.SourceTab.UseVisualStyleBackColor = true;
            // 
            // btnSelectISO
            // 
            this.btnSelectISO.Location = new System.Drawing.Point(389, 10);
            this.btnSelectISO.Name = "btnSelectISO";
            this.btnSelectISO.Size = new System.Drawing.Size(30, 23);
            this.btnSelectISO.TabIndex = 11;
            this.btnSelectISO.Text = "...";
            this.btnSelectISO.UseVisualStyleBackColor = true;
            this.btnSelectISO.Click += new System.EventHandler(this.btnSelectISO_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(135, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Path to Windows 11 ISO:";
            // 
            // txtIsoPath
            // 
            this.txtIsoPath.Location = new System.Drawing.Point(147, 10);
            this.txtIsoPath.Name = "txtIsoPath";
            this.txtIsoPath.Size = new System.Drawing.Size(236, 22);
            this.txtIsoPath.TabIndex = 9;
            // 
            // OptionsTab
            // 
            this.OptionsTab.Controls.Add(this.groupBox1);
            this.OptionsTab.Location = new System.Drawing.Point(4, 24);
            this.OptionsTab.Name = "OptionsTab";
            this.OptionsTab.Padding = new System.Windows.Forms.Padding(3);
            this.OptionsTab.Size = new System.Drawing.Size(508, 340);
            this.OptionsTab.TabIndex = 0;
            this.OptionsTab.Text = "Options";
            this.OptionsTab.UseVisualStyleBackColor = true;
            // 
            // OutputTab
            // 
            this.OutputTab.Controls.Add(this.btnSelectVHDOutput);
            this.OutputTab.Controls.Add(this.label6);
            this.OutputTab.Controls.Add(this.txtOutFile);
            this.OutputTab.Controls.Add(this.label5);
            this.OutputTab.Controls.Add(this.label4);
            this.OutputTab.Controls.Add(this.radISO);
            this.OutputTab.Controls.Add(this.radVHD);
            this.OutputTab.Location = new System.Drawing.Point(4, 24);
            this.OutputTab.Name = "OutputTab";
            this.OutputTab.Size = new System.Drawing.Size(508, 340);
            this.OutputTab.TabIndex = 3;
            this.OutputTab.Text = "Output";
            this.OutputTab.UseVisualStyleBackColor = true;
            // 
            // btnSelectVHDOutput
            // 
            this.btnSelectVHDOutput.Location = new System.Drawing.Point(235, 59);
            this.btnSelectVHDOutput.Name = "btnSelectVHDOutput";
            this.btnSelectVHDOutput.Size = new System.Drawing.Size(25, 23);
            this.btnSelectVHDOutput.TabIndex = 6;
            this.btnSelectVHDOutput.Text = "...";
            this.btnSelectVHDOutput.UseVisualStyleBackColor = true;
            this.btnSelectVHDOutput.Click += new System.EventHandler(this.btnSelectVHDOutput_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(27, 62);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Output:";
            // 
            // txtOutFile
            // 
            this.txtOutFile.Location = new System.Drawing.Point(81, 59);
            this.txtOutFile.Name = "txtOutFile";
            this.txtOutFile.Size = new System.Drawing.Size(148, 22);
            this.txtOutFile.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(26, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(203, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Exports the Windows Image as a VHD.";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(26, 116);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(342, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Exports the Windows Image as a bootable ISO file. Coming soon!";
            // 
            // radISO
            // 
            this.radISO.AutoSize = true;
            this.radISO.Enabled = false;
            this.radISO.Location = new System.Drawing.Point(26, 96);
            this.radISO.Name = "radISO";
            this.radISO.Size = new System.Drawing.Size(103, 17);
            this.radISO.TabIndex = 1;
            this.radISO.Text = "ISO File output";
            this.radISO.UseVisualStyleBackColor = true;
            // 
            // radVHD
            // 
            this.radVHD.AutoSize = true;
            this.radVHD.Checked = true;
            this.radVHD.Location = new System.Drawing.Point(26, 23);
            this.radVHD.Name = "radVHD";
            this.radVHD.Size = new System.Drawing.Size(89, 17);
            this.radVHD.TabIndex = 0;
            this.radVHD.TabStop = true;
            this.radVHD.Text = "VHD Output";
            this.radVHD.UseVisualStyleBackColor = true;
            // 
            // ProgressTab
            // 
            this.ProgressTab.Controls.Add(this.lblProgress);
            this.ProgressTab.Controls.Add(this.progressBar1);
            this.ProgressTab.Location = new System.Drawing.Point(4, 24);
            this.ProgressTab.Name = "ProgressTab";
            this.ProgressTab.Size = new System.Drawing.Size(508, 340);
            this.ProgressTab.TabIndex = 2;
            this.ProgressTab.Text = "Progress";
            this.ProgressTab.UseVisualStyleBackColor = true;
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(3, 24);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(111, 13);
            this.lblProgress.TabIndex = 1;
            this.lblProgress.Text = "Building please wait";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(3, 40);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(502, 23);
            this.progressBar1.TabIndex = 0;
            // 
            // VHDDialog
            // 
            this.VHDDialog.Filter = "VHD File|*.vhd";
            // 
            // SelISODialog
            // 
            this.SelISODialog.FileName = "openFileDialog1";
            this.SelISODialog.Filter = "Windows 11 ISO files|*.iso";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 450);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnBuild);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Name = "frmMain";
            this.Text = "Misha PE Builder";
            this.groupBox1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.SourceTab.ResumeLayout(false);
            this.SourceTab.PerformLayout();
            this.OptionsTab.ResumeLayout(false);
            this.OutputTab.ResumeLayout(false);
            this.OutputTab.PerformLayout();
            this.ProgressTab.ResumeLayout(false);
            this.ProgressTab.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkExplorer;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox chkLogonUI;
        private System.Windows.Forms.CheckBox chkDWM;
        private System.Windows.Forms.CheckBox chkUWP;
        private System.Windows.Forms.Button btnBuild;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage SourceTab;
        private System.Windows.Forms.TabPage OptionsTab;
        private System.Windows.Forms.Button btnSelectISO;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtIsoPath;
        private System.Windows.Forms.TabPage ProgressTab;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblProgress;
        private TabPage OutputTab;
        private RadioButton radISO;
        private RadioButton radVHD;
        private Label label4;
        private Label label5;
        private Label label6;
        private TextBox txtOutFile;
        private Button btnSelectVHDOutput;
        private SaveFileDialog VHDDialog;
        private OpenFileDialog SelISODialog;
    }
}

