namespace SessionManager
{
    partial class Form1
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
            this.headNode = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.serviceName = new System.Windows.Forms.TextBox();
            this.sharedSessionCheckBox = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.serviceJobName = new System.Windows.Forms.TextBox();
            this.createSessionButton = new System.Windows.Forms.Button();
            this.closeSessionButton = new System.Windows.Forms.Button();
            this.status = new System.Windows.Forms.Label();
            this.minNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.minimalCores = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.maxNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.sessionCreationProgressBar = new System.Windows.Forms.ProgressBar();
            this.statusLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.minNumericUpDown)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxNumericUpDown)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(39, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Head node";
            // 
            // headNode
            // 
            this.headNode.Location = new System.Drawing.Point(163, 39);
            this.headNode.Name = "headNode";
            this.headNode.Size = new System.Drawing.Size(202, 20);
            this.headNode.TabIndex = 1;
            this.headNode.Text = AsianOptions.Config.headNode;
            this.headNode.TextChanged += new System.EventHandler(this.headNode_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(39, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Service name";
            // 
            // serviceName
            // 
            this.serviceName.Location = new System.Drawing.Point(139, 63);
            this.serviceName.Name = "serviceName";
            this.serviceName.Size = new System.Drawing.Size(202, 20);
            this.serviceName.TabIndex = 3;
            this.serviceName.Text = "AsianOptionsService";
            this.serviceName.TextChanged += new System.EventHandler(this.serviceName_TextChanged);
            // 
            // sharedSessionCheckBox
            // 
            this.sharedSessionCheckBox.AutoSize = true;
            this.sharedSessionCheckBox.Checked = true;
            this.sharedSessionCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.sharedSessionCheckBox.Location = new System.Drawing.Point(18, 134);
            this.sharedSessionCheckBox.Name = "sharedSessionCheckBox";
            this.sharedSessionCheckBox.Size = new System.Drawing.Size(241, 17);
            this.sharedSessionCheckBox.TabIndex = 4;
            this.sharedSessionCheckBox.Text = "This session is to be shared by multiple clients";
            this.sharedSessionCheckBox.UseVisualStyleBackColor = true;
            this.sharedSessionCheckBox.CheckedChanged += new System.EventHandler(this.sharedSessionCheckBox_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(39, 114);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Service Job Name";
            // 
            // serviceJobName
            // 
            this.serviceJobName.Location = new System.Drawing.Point(163, 111);
            this.serviceJobName.Name = "serviceJobName";
            this.serviceJobName.Size = new System.Drawing.Size(202, 20);
            this.serviceJobName.TabIndex = 6;
            this.serviceJobName.Text = "AsianOptions";
            // 
            // createSessionButton
            // 
            this.createSessionButton.Location = new System.Drawing.Point(23, 283);
            this.createSessionButton.Name = "createSessionButton";
            this.createSessionButton.Size = new System.Drawing.Size(91, 23);
            this.createSessionButton.TabIndex = 7;
            this.createSessionButton.Text = "Create Session";
            this.createSessionButton.UseVisualStyleBackColor = true;
            this.createSessionButton.Click += new System.EventHandler(this.createSessionButton_Click);
            // 
            // closeSessionButton
            // 
            this.closeSessionButton.Location = new System.Drawing.Point(146, 283);
            this.closeSessionButton.Name = "closeSessionButton";
            this.closeSessionButton.Size = new System.Drawing.Size(95, 23);
            this.closeSessionButton.TabIndex = 8;
            this.closeSessionButton.Text = "Close Session";
            this.closeSessionButton.UseVisualStyleBackColor = true;
            this.closeSessionButton.Click += new System.EventHandler(this.closeSessionButton_Click);
            // 
            // status
            // 
            this.status.AutoSize = true;
            this.status.Location = new System.Drawing.Point(42, 283);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(0, 13);
            this.status.TabIndex = 9;
            this.status.Visible = false;
            this.status.Click += new System.EventHandler(this.label4_Click);
            // 
            // minNumericUpDown
            // 
            this.minNumericUpDown.Location = new System.Drawing.Point(111, 43);
            this.minNumericUpDown.Name = "minNumericUpDown";
            this.minNumericUpDown.Size = new System.Drawing.Size(58, 20);
            this.minNumericUpDown.TabIndex = 10;
            this.minNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // minimalCores
            // 
            this.minimalCores.AutoSize = true;
            this.minimalCores.Location = new System.Drawing.Point(6, 45);
            this.minimalCores.Name = "minimalCores";
            this.minimalCores.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.minimalCores.Size = new System.Drawing.Size(78, 13);
            this.minimalCores.TabIndex = 11;
            this.minimalCores.Text = "Minimum Cores";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.maxNumericUpDown);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.minimalCores);
            this.groupBox1.Controls.Add(this.minNumericUpDown);
            this.groupBox1.Location = new System.Drawing.Point(24, 194);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(378, 72);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Resource Requirements";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(190, 45);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Maximum Cores";
            // 
            // maxNumericUpDown
            // 
            this.maxNumericUpDown.Location = new System.Drawing.Point(277, 43);
            this.maxNumericUpDown.Name = "maxNumericUpDown";
            this.maxNumericUpDown.Size = new System.Drawing.Size(59, 20);
            this.maxNumericUpDown.TabIndex = 13;
            this.maxNumericUpDown.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.maxNumericUpDown.ValueChanged += new System.EventHandler(this.numericUpDown2_ValueChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.serviceName);
            this.groupBox2.Controls.Add(this.sharedSessionCheckBox);
            this.groupBox2.Location = new System.Drawing.Point(24, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(378, 166);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Session Information";
            // 
            // sessionCreationProgressBar
            // 
            this.sessionCreationProgressBar.Location = new System.Drawing.Point(301, 283);
            this.sessionCreationProgressBar.Name = "sessionCreationProgressBar";
            this.sessionCreationProgressBar.Size = new System.Drawing.Size(100, 23);
            this.sessionCreationProgressBar.TabIndex = 14;
            this.sessionCreationProgressBar.Visible = false;
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(21, 325);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(61, 13);
            this.statusLabel.TabIndex = 15;
            this.statusLabel.Text = "statusLabel";
            this.statusLabel.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(427, 362);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.sessionCreationProgressBar);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.status);
            this.Controls.Add(this.closeSessionButton);
            this.Controls.Add(this.createSessionButton);
            this.Controls.Add(this.serviceJobName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.headNode);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox2);
            this.Name = "Form1";
            this.Text = "Session Manager";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.minNumericUpDown)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxNumericUpDown)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox headNode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox serviceName;
        private System.Windows.Forms.CheckBox sharedSessionCheckBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox serviceJobName;
        private System.Windows.Forms.Button createSessionButton;
        private System.Windows.Forms.Button closeSessionButton;
        private System.Windows.Forms.Label status;
        private System.Windows.Forms.NumericUpDown minNumericUpDown;
        private System.Windows.Forms.Label minimalCores;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown maxNumericUpDown;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ProgressBar sessionCreationProgressBar;
        private System.Windows.Forms.Label statusLabel;
    }
}

