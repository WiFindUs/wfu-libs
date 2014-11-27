namespace WiFindUs.Eye.Dispatcher
{
    partial class DispatcherForm
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
            this.workingAreaToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this.windowStatusStrip = new System.Windows.Forms.StatusStrip();
            this.windowSplitter = new System.Windows.Forms.SplitContainer();
            this.controlsOuterSplitter = new System.Windows.Forms.SplitContainer();
            this.controlsInnerSplitter = new System.Windows.Forms.SplitContainer();
            this.infoTabs = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.workingAreaToolStripContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.windowSplitter)).BeginInit();
            this.windowSplitter.Panel1.SuspendLayout();
            this.windowSplitter.Panel2.SuspendLayout();
            this.windowSplitter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlsOuterSplitter)).BeginInit();
            this.controlsOuterSplitter.Panel2.SuspendLayout();
            this.controlsOuterSplitter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlsInnerSplitter)).BeginInit();
            this.controlsInnerSplitter.Panel1.SuspendLayout();
            this.controlsInnerSplitter.SuspendLayout();
            this.infoTabs.SuspendLayout();
            this.SuspendLayout();
            // 
            // workingAreaToolStripContainer
            // 
            // 
            // workingAreaToolStripContainer.ContentPanel
            // 
            this.workingAreaToolStripContainer.ContentPanel.Margin = new System.Windows.Forms.Padding(0);
            this.workingAreaToolStripContainer.ContentPanel.Size = new System.Drawing.Size(928, 324);
            this.workingAreaToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.workingAreaToolStripContainer.Location = new System.Drawing.Point(0, 0);
            this.workingAreaToolStripContainer.Name = "workingAreaToolStripContainer";
            this.workingAreaToolStripContainer.Size = new System.Drawing.Size(928, 349);
            this.workingAreaToolStripContainer.TabIndex = 0;
            this.workingAreaToolStripContainer.Text = "toolStripContainer1";
            // 
            // windowStatusStrip
            // 
            this.windowStatusStrip.Location = new System.Drawing.Point(0, 542);
            this.windowStatusStrip.Name = "windowStatusStrip";
            this.windowStatusStrip.Size = new System.Drawing.Size(928, 22);
            this.windowStatusStrip.TabIndex = 1;
            this.windowStatusStrip.Text = "statusStrip1";
            // 
            // windowSplitter
            // 
            this.windowSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.windowSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.windowSplitter.IsSplitterFixed = true;
            this.windowSplitter.Location = new System.Drawing.Point(0, 0);
            this.windowSplitter.Margin = new System.Windows.Forms.Padding(0);
            this.windowSplitter.Name = "windowSplitter";
            this.windowSplitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // windowSplitter.Panel1
            // 
            this.windowSplitter.Panel1.Controls.Add(this.workingAreaToolStripContainer);
            // 
            // windowSplitter.Panel2
            // 
            this.windowSplitter.Panel2.Controls.Add(this.controlsOuterSplitter);
            this.windowSplitter.Size = new System.Drawing.Size(928, 542);
            this.windowSplitter.SplitterDistance = 349;
            this.windowSplitter.SplitterWidth = 1;
            this.windowSplitter.TabIndex = 2;
            // 
            // controlsOuterSplitter
            // 
            this.controlsOuterSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlsOuterSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.controlsOuterSplitter.IsSplitterFixed = true;
            this.controlsOuterSplitter.Location = new System.Drawing.Point(0, 0);
            this.controlsOuterSplitter.Name = "controlsOuterSplitter";
            // 
            // controlsOuterSplitter.Panel2
            // 
            this.controlsOuterSplitter.Panel2.Controls.Add(this.controlsInnerSplitter);
            this.controlsOuterSplitter.Size = new System.Drawing.Size(928, 192);
            this.controlsOuterSplitter.SplitterDistance = 280;
            this.controlsOuterSplitter.SplitterWidth = 1;
            this.controlsOuterSplitter.TabIndex = 0;
            // 
            // controlsInnerSplitter
            // 
            this.controlsInnerSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlsInnerSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.controlsInnerSplitter.IsSplitterFixed = true;
            this.controlsInnerSplitter.Location = new System.Drawing.Point(0, 0);
            this.controlsInnerSplitter.Name = "controlsInnerSplitter";
            // 
            // controlsInnerSplitter.Panel1
            // 
            this.controlsInnerSplitter.Panel1.Controls.Add(this.infoTabs);
            this.controlsInnerSplitter.Size = new System.Drawing.Size(647, 192);
            this.controlsInnerSplitter.SplitterDistance = 431;
            this.controlsInnerSplitter.SplitterWidth = 1;
            this.controlsInnerSplitter.TabIndex = 0;
            // 
            // infoTabs
            // 
            this.infoTabs.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.infoTabs.Controls.Add(this.tabPage1);
            this.infoTabs.Controls.Add(this.tabPage2);
            this.infoTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoTabs.Location = new System.Drawing.Point(0, 0);
            this.infoTabs.Name = "infoTabs";
            this.infoTabs.SelectedIndex = 0;
            this.infoTabs.Size = new System.Drawing.Size(431, 192);
            this.infoTabs.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(423, 163);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(423, 163);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // DispatcherForm
            // 
            this.ClientSize = new System.Drawing.Size(928, 564);
            this.Controls.Add(this.windowSplitter);
            this.Controls.Add(this.windowStatusStrip);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "DispatcherForm";
            this.workingAreaToolStripContainer.ResumeLayout(false);
            this.workingAreaToolStripContainer.PerformLayout();
            this.windowSplitter.Panel1.ResumeLayout(false);
            this.windowSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.windowSplitter)).EndInit();
            this.windowSplitter.ResumeLayout(false);
            this.controlsOuterSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.controlsOuterSplitter)).EndInit();
            this.controlsOuterSplitter.ResumeLayout(false);
            this.controlsInnerSplitter.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.controlsInnerSplitter)).EndInit();
            this.controlsInnerSplitter.ResumeLayout(false);
            this.infoTabs.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer workingAreaToolStripContainer;
        private System.Windows.Forms.StatusStrip windowStatusStrip;
        private System.Windows.Forms.SplitContainer windowSplitter;
        private System.Windows.Forms.SplitContainer controlsOuterSplitter;
        private System.Windows.Forms.SplitContainer controlsInnerSplitter;
        private System.Windows.Forms.TabControl infoTabs;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;


    }
}

