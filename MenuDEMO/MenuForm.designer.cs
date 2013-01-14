namespace MenuDEMO
{
	partial class MenuForm
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
			this.FormList = new System.Windows.Forms.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.OpenFormButton = new System.Windows.Forms.Button();
			this.CloseButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// FormList
			// 
			this.FormList.FormattingEnabled = true;
			this.FormList.Location = new System.Drawing.Point(28, 56);
			this.FormList.Name = "FormList";
			this.FormList.Size = new System.Drawing.Size(237, 95);
			this.FormList.TabIndex = 0;
			this.FormList.DoubleClick += new System.EventHandler(this.SelectFormHandler);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(25, 39);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(102, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Select form to open:";
			// 
			// OpenFormButton
			// 
			this.OpenFormButton.Location = new System.Drawing.Point(38, 181);
			this.OpenFormButton.Name = "OpenFormButton";
			this.OpenFormButton.Size = new System.Drawing.Size(75, 23);
			this.OpenFormButton.TabIndex = 2;
			this.OpenFormButton.Text = "&Open Form";
			this.OpenFormButton.UseVisualStyleBackColor = true;
			this.OpenFormButton.Click += new System.EventHandler(this.SelectFormHandler);
			// 
			// CloseButton
			// 
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = new System.Drawing.Point(173, 181);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(75, 23);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.Text = "&Close";
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// MainMenuForm
			// 
			this.AcceptButton = this.OpenFormButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CloseButton;
			this.ClientSize = new System.Drawing.Size(292, 235);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.OpenFormButton);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.FormList);
			this.Name = "MainMenuForm";
			this.Text = "Form Menu";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox FormList;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button OpenFormButton;
		private System.Windows.Forms.Button CloseButton;
	}
}