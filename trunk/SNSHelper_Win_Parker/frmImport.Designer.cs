namespace SNSHelper_Win_Parker
{
    partial class frmImport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmImport));
            this.labelX1 = new DevComponents.DotNetBar.LabelX();
            this.labelX2 = new DevComponents.DotNetBar.LabelX();
            this.labelX4 = new DevComponents.DotNetBar.LabelX();
            this.labelX3 = new DevComponents.DotNetBar.LabelX();
            this.labelX6 = new DevComponents.DotNetBar.LabelX();
            this.labelX5 = new DevComponents.DotNetBar.LabelX();
            this.txtBoard = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.btnImport = new DevComponents.DotNetBar.ButtonX();
            this.btnChooseFile = new DevComponents.DotNetBar.ButtonX();
            this.txtFilePath = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // labelX1
            // 
            this.labelX1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelX1.Location = new System.Drawing.Point(12, 12);
            this.labelX1.Name = "labelX1";
            this.labelX1.Size = new System.Drawing.Size(122, 31);
            this.labelX1.TabIndex = 5;
            this.labelX1.Text = "导入文件格式如下：\r\n";
            // 
            // labelX2
            // 
            this.labelX2.Location = new System.Drawing.Point(32, 49);
            this.labelX2.Name = "labelX2";
            this.labelX2.Size = new System.Drawing.Size(111, 21);
            this.labelX2.TabIndex = 6;
            this.labelX2.Text = "Email,password";
            // 
            // labelX4
            // 
            this.labelX4.Location = new System.Drawing.Point(32, 93);
            this.labelX4.Name = "labelX4";
            this.labelX4.Size = new System.Drawing.Size(235, 21);
            this.labelX4.TabIndex = 9;
            this.labelX4.Text = "×××@×××.×××,××××××";
            // 
            // labelX3
            // 
            this.labelX3.Location = new System.Drawing.Point(32, 76);
            this.labelX3.Name = "labelX3";
            this.labelX3.Size = new System.Drawing.Size(247, 21);
            this.labelX3.TabIndex = 8;
            this.labelX3.Text = "×××@×××.×××,××××××";
            // 
            // labelX6
            // 
            this.labelX6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelX6.ForeColor = System.Drawing.Color.Red;
            this.labelX6.Location = new System.Drawing.Point(246, 53);
            this.labelX6.Name = "labelX6";
            this.labelX6.Size = new System.Drawing.Size(259, 21);
            this.labelX6.TabIndex = 12;
            this.labelX6.Text = "建议：不要一次导入太多帐号，避免出错！";
            // 
            // labelX5
            // 
            this.labelX5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelX5.ForeColor = System.Drawing.Color.Red;
            this.labelX5.Location = new System.Drawing.Point(246, 33);
            this.labelX5.Name = "labelX5";
            this.labelX5.Size = new System.Drawing.Size(242, 21);
            this.labelX5.TabIndex = 11;
            this.labelX5.Text = "注：帐号和密码间用半角的逗号（,）分割";
            // 
            // txtBoard
            // 
            this.txtBoard.BackColor = System.Drawing.Color.White;
            // 
            // 
            // 
            this.txtBoard.Border.Class = "TextBoxBorder";
            this.txtBoard.Location = new System.Drawing.Point(32, 142);
            this.txtBoard.Multiline = true;
            this.txtBoard.Name = "txtBoard";
            this.txtBoard.ReadOnly = true;
            this.txtBoard.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBoard.Size = new System.Drawing.Size(472, 188);
            this.txtBoard.TabIndex = 16;
            // 
            // btnImport
            // 
            this.btnImport.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnImport.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnImport.Enabled = false;
            this.btnImport.Location = new System.Drawing.Point(429, 115);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(75, 21);
            this.btnImport.TabIndex = 15;
            this.btnImport.Text = "导入";
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnChooseFile
            // 
            this.btnChooseFile.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnChooseFile.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnChooseFile.Location = new System.Drawing.Point(347, 115);
            this.btnChooseFile.Name = "btnChooseFile";
            this.btnChooseFile.Size = new System.Drawing.Size(75, 21);
            this.btnChooseFile.TabIndex = 14;
            this.btnChooseFile.Text = "选择文件";
            this.btnChooseFile.Click += new System.EventHandler(this.btnChooseFile_Click);
            // 
            // txtFilePath
            // 
            // 
            // 
            // 
            this.txtFilePath.Border.Class = "TextBoxBorder";
            this.txtFilePath.Location = new System.Drawing.Point(32, 118);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(309, 21);
            this.txtFilePath.TabIndex = 13;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // frmImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(522, 336);
            this.Controls.Add(this.txtBoard);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnChooseFile);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.labelX6);
            this.Controls.Add(this.labelX5);
            this.Controls.Add(this.labelX4);
            this.Controls.Add(this.labelX3);
            this.Controls.Add(this.labelX2);
            this.Controls.Add(this.labelX1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmImport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "帐号批量导入";
            this.Load += new System.EventHandler(this.frmImport_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.LabelX labelX1;
        private DevComponents.DotNetBar.LabelX labelX2;
        private DevComponents.DotNetBar.LabelX labelX4;
        private DevComponents.DotNetBar.LabelX labelX3;
        private DevComponents.DotNetBar.LabelX labelX6;
        private DevComponents.DotNetBar.LabelX labelX5;
        private DevComponents.DotNetBar.Controls.TextBoxX txtBoard;
        private DevComponents.DotNetBar.ButtonX btnImport;
        private DevComponents.DotNetBar.ButtonX btnChooseFile;
        private DevComponents.DotNetBar.Controls.TextBoxX txtFilePath;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}