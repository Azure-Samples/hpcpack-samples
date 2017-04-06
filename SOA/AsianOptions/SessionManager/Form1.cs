using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Hpc.Scheduler.Session;
using System.Threading;

namespace SessionManager
{
    public partial class Form1 : Form
    {
        private static Session session = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void headNode_TextChanged(object sender, EventArgs e)
        {

        }

        private void serviceName_TextChanged(object sender, EventArgs e)
        {

        }

        private void sharedSessionCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void createSessionButton_Click(object sender, EventArgs e)
        {
            sessionCreationProgressBar.Visible = true;
            sessionCreationProgressBar.Minimum = 0;
            sessionCreationProgressBar.Maximum = 100;
            sessionCreationProgressBar.Value = 80;
            sessionCreationProgressBar.Update();

            if (headNode.Text == null)
            {
                MessageBox.Show("You must provide a head node name");
                return;
            }

            if (serviceName.Text == null)
            {
                MessageBox.Show("You must provide a service name");
                return;
            }

            SessionStartInfo info = new SessionStartInfo(headNode.Text, serviceName.Text);
            info.ShareSession = sharedSessionCheckBox.Checked;
      
            info.Secure = false;
            info.MinimumUnits = (int) minNumericUpDown.Value;
            info.MaximumUnits = (int) maxNumericUpDown.Value;
            info.ServiceJobName = serviceJobName.Text;
            info.BrokerSettings.SessionIdleTimeout = 12 * 3600;  // 12 hours

            Session.SetInterfaceMode(false, IntPtr.Zero); //set interface mode to non console

            IAsyncResult result = Session.BeginCreateSession(info, null, null);

            sessionCreationProgressBar.Value = 100;
            sessionCreationProgressBar.Update();

            session = Session.EndCreateSession(result);

            sessionCreationProgressBar.Visible = false;
            session.AutoClose = info.ShareSession == true ? false : true;
            statusLabel.Visible = true;
            statusLabel.Text = string.Format("Session {0} Created", session.Id); 
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void closeSessionButton_Click(object sender, EventArgs e)
        {
            statusLabel.Text = string.Format("Closing session {0}...", session.Id);
            Session.CloseSession(headNode.Text, session.Id);
            statusLabel.Visible = true;
            statusLabel.Text = "Session closed";
        }
    }
}
