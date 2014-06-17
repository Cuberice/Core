namespace Core
{
	partial class TestForm
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
			this.grid = new System.Windows.Forms.DataGridView();
			this.button1 = new System.Windows.Forms.Button();
			this.btnTestInsert = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.AllowUserToDeleteRows = false;
			this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.grid.Location = new System.Drawing.Point(12, 104);
			this.grid.Name = "grid";
			this.grid.Size = new System.Drawing.Size(1116, 332);
			this.grid.TabIndex = 0;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(13, 13);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(129, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "Generate Test Data";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.GenerateTestData_Click);
			// 
			// btnTestInsert
			// 
			this.btnTestInsert.Location = new System.Drawing.Point(148, 13);
			this.btnTestInsert.Name = "btnTestInsert";
			this.btnTestInsert.Size = new System.Drawing.Size(129, 23);
			this.btnTestInsert.TabIndex = 2;
			this.btnTestInsert.Text = "Test Insert";
			this.btnTestInsert.UseVisualStyleBackColor = true;
			this.btnTestInsert.Click += new System.EventHandler(this.TestInsert_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(148, 42);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(129, 23);
			this.button2.TabIndex = 3;
			this.button2.Text = "Test Update";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.TestUpdate_Click);
			// 
			// TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1140, 448);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.btnTestInsert);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.grid);
			this.Name = "TestForm";
			this.Text = "TestForm";
			this.Shown += new System.EventHandler(this.TestForm_Shown);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView grid;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button btnTestInsert;
		private System.Windows.Forms.Button button2;
	}
}