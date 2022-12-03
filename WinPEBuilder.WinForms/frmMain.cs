using WinPEBuilder.Core;

namespace WinPEBuilder.WinForms
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            tabControl1.TabPages.Remove(ProgressTab);
        }

        private void chkDWM_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDWM.Checked)
            {
                chkUWP.Enabled = true;
                chkExplorer.Enabled = true;
                chkLogonUI.Enabled = true;
            }
            else
            {
                chkUWP.Enabled = false;
                chkExplorer.Enabled = false;
                chkLogonUI.Enabled = false;

                //uncheck
                chkUWP.Checked = false;
                chkExplorer.Checked = false;
                chkLogonUI.Checked = false;
            }
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtIsoPath.Text))
            {
                MessageBox.Show("You must select an ISO in the sources tab", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(txtOutFile.Text))
            {
                MessageBox.Show("You must select an output type/file in the output tab.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            tabControl1.TabPages.Add(ProgressTab);

            HideTabs();
            btnBuild.Visible = false;

            //create options
            var options = new BuilderOptions();
            if (radVHD.Checked)
            {
                options.OutputType = BuilderOptionsOutputType.VHD;
                options.Output = txtOutFile.Text;
            }
            else
            {
                throw new NotImplementedException("ISO file output not implemented");
            }

            var builder = new Builder(options, txtIsoPath.Text, Application.StartupPath + @"\work\");



            builder.OnProgress += Builder_OnProgress;
            builder.Start();
        }

        private void Builder_OnProgress(bool error, int progress, string message)
        {
            if (this.InvokeRequired)
            {
                Invoke(ProgressHandler, error, progress, message);
            }
            else
            {
                ProgressHandler(error, progress, message);
            }
        }
        private void ProgressHandler(bool error, int progress, string message)
        {
            lblProgress.Text = message;
            progressBar1.Value = progress;
            if (error)
            {
                //the builder should have stopped
                MessageBox.Show("Error occured while building: " + message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowTabs();
                btnBuild.Visible = true;
            }
        }
        private void HideTabs()
        {
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
        }
        private void ShowTabs()
        {
            tabControl1.Appearance = TabAppearance.Normal;
            tabControl1.ItemSize = new Size(48, 18);
            tabControl1.SizeMode = TabSizeMode.Normal;
        }

        private void btnSelectVHDOutput_Click(object sender, EventArgs e)
        {
            if (VHDDialog.ShowDialog() == DialogResult.OK)
            {
                txtOutFile.Text = VHDDialog.FileName;
            }
        }

        private void btnSelectISO_Click(object sender, EventArgs e)
        {
            if (SelISODialog.ShowDialog() == DialogResult.OK)
            {
                txtIsoPath.Text = SelISODialog.FileName;
            }
        }
    }
}
