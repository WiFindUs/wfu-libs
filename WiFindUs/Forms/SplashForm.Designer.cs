namespace WiFindUs.Forms
{
	partial class SplashForm
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
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.loadingWorker = new System.ComponentModel.BackgroundWorker();
			this.SuspendLayout();
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar.Location = new System.Drawing.Point(21, 641);
			this.progressBar.Margin = new System.Windows.Forms.Padding(0);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(483, 27);
			this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.progressBar.TabIndex = 0;
			// 
			// loadingWorker
			// 
			this.loadingWorker.WorkerReportsProgress = true;
			this.loadingWorker.WorkerSupportsCancellation = true;
			// 
			// SplashForm
			// 
			this.ClientSize = new System.Drawing.Size(525, 692);
			this.ControlBox = false;
			this.Controls.Add(this.progressBar);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(525, 692);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(525, 692);
			this.Name = "SplashForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ProgressBar progressBar;
		private System.ComponentModel.BackgroundWorker loadingWorker;

	}
}