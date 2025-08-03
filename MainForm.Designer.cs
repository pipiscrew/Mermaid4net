namespace
    Mermaid4net
{
    partial class MainForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.optNum = new System.Windows.Forms.RadioButton();
            this.optLetter = new System.Windows.Forms.RadioButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.chkDecision = new System.Windows.Forms.CheckBox();
            this.chkDecisionTask = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(49)))), ((int)(((byte)(110)))));
            this.button1.Location = new System.Drawing.Point(253, 61);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(87, 43);
            this.button1.TabIndex = 2;
            this.button1.Text = "export";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(49)))), ((int)(((byte)(110)))));
            this.label1.Location = new System.Drawing.Point(7, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 31);
            this.label1.TabIndex = 4;
            this.label1.Text = "Source assembly :";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtFile
            // 
            this.txtFile.AllowDrop = true;
            this.txtFile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFile.Location = new System.Drawing.Point(8, 32);
            this.txtFile.Name = "txtFile";
            this.txtFile.ReadOnly = true;
            this.txtFile.Size = new System.Drawing.Size(332, 23);
            this.txtFile.TabIndex = 1;
            this.txtFile.Text = "drag & drop the assembly here";
            this.txtFile.DragDrop += new System.Windows.Forms.DragEventHandler(this.textBox1_DragDrop);
            this.txtFile.DragEnter += new System.Windows.Forms.DragEventHandler(this.textBox1_DragEnter);
            // 
            // optNum
            // 
            this.optNum.AutoSize = true;
            this.optNum.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(49)))), ((int)(((byte)(110)))));
            this.optNum.Location = new System.Drawing.Point(13, 64);
            this.optNum.Name = "optNum";
            this.optNum.Size = new System.Drawing.Size(74, 19);
            this.optNum.TabIndex = 3;
            this.optNum.Text = "numeric";
            this.optNum.UseVisualStyleBackColor = true;
            // 
            // optLetter
            // 
            this.optLetter.AutoSize = true;
            this.optLetter.Checked = true;
            this.optLetter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(49)))), ((int)(((byte)(110)))));
            this.optLetter.Location = new System.Drawing.Point(13, 85);
            this.optLetter.Name = "optLetter";
            this.optLetter.Size = new System.Drawing.Size(67, 19);
            this.optLetter.TabIndex = 0;
            this.optLetter.TabStop = true;
            this.optLetter.Text = "letter";
            this.optLetter.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Mermaid4net.Properties.Resources.mermaid;
            this.pictureBox1.Location = new System.Drawing.Point(236, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(113, 31);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // chkDecision
            // 
            this.chkDecision.AutoSize = true;
            this.chkDecision.Checked = true;
            this.chkDecision.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDecision.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(49)))), ((int)(((byte)(110)))));
            this.chkDecision.Location = new System.Drawing.Point(102, 65);
            this.chkDecision.Name = "chkDecision";
            this.chkDecision.Size = new System.Drawing.Size(117, 19);
            this.chkDecision.TabIndex = 6;
            this.chkDecision.Text = "show decision";
            this.chkDecision.UseVisualStyleBackColor = true;
            this.chkDecision.CheckStateChanged += new System.EventHandler(this.chkDecision_CheckStateChanged);
            // 
            // chkDecisionTask
            // 
            this.chkDecisionTask.AutoSize = true;
            this.chkDecisionTask.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.chkDecisionTask.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(49)))), ((int)(((byte)(110)))));
            this.chkDecisionTask.Location = new System.Drawing.Point(119, 82);
            this.chkDecisionTask.Name = "chkDecisionTask";
            this.chkDecisionTask.Size = new System.Drawing.Size(128, 30);
            this.chkDecisionTask.TabIndex = 7;
            this.chkDecisionTask.Text = "with Task support\r\n(experimental)";
            this.chkDecisionTask.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(351, 109);
            this.Controls.Add(this.chkDecisionTask);
            this.Controls.Add(this.chkDecision);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.optLetter);
            this.Controls.Add(this.optNum);
            this.Controls.Add(this.txtFile);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mermaid for .net";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.RadioButton optNum;
        private System.Windows.Forms.RadioButton optLetter;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox chkDecision;
        private System.Windows.Forms.CheckBox chkDecisionTask;
    }
}