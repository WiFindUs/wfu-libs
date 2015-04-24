namespace WiFindUs.Eye.Simulator
{
	partial class SimulatorForm
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
			this.map2D1 = new WiFindUs.Eye.Controls.Map2D();
			this.SuspendLayout();
			// 
			// map2D1
			// 
			this.map2D1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.map2D1.Location = new System.Drawing.Point(0, 0);
			this.map2D1.Margin = new System.Windows.Forms.Padding(0);
			this.map2D1.Name = "map2D1";
			this.map2D1.Size = new System.Drawing.Size(896, 666);
			this.map2D1.TabIndex = 0;
			this.map2D1.TabStop = false;
			this.map2D1.Text = "map2D1";
			// 
			// SimulatorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(896, 666);
			this.Controls.Add(this.map2D1);
			this.MinimumSize = new System.Drawing.Size(400, 400);
			this.Name = "SimulatorForm";
			this.Text = "Form1 - Program (Administrator) (Debug build)";
			this.ResumeLayout(false);

		}

		#endregion

		private Controls.Map2D map2D1;
	}
}

