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
            WiFindUs.Controls.Theme theme1 = new WiFindUs.Controls.Theme();
            WiFindUs.Controls.Theme theme2 = new WiFindUs.Controls.Theme();
            WiFindUs.Controls.Theme theme3 = new WiFindUs.Controls.Theme();
            this.workingAreaToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this.windowStatusStrip = new System.Windows.Forms.StatusStrip();
            this.windowSplitter = new System.Windows.Forms.SplitContainer();
            this.controlsOuterSplitter = new System.Windows.Forms.SplitContainer();
            this.mapTabs = new WiFindUs.Controls.ThemedTabControl();
            this.minimapTab = new System.Windows.Forms.TabPage();
            this.controlsInnerSplitter = new System.Windows.Forms.SplitContainer();
            this.infoTabs = new WiFindUs.Controls.ThemedTabControl();
            this.infoTab = new System.Windows.Forms.TabPage();
            this.consoleTab = new System.Windows.Forms.TabPage();
            this.actionTabs = new WiFindUs.Controls.ThemedTabControl();
            this.actionsTab = new System.Windows.Forms.TabPage();
            this.consolePanel1 = new WiFindUs.Controls.ConsolePanel();
            this.workingAreaToolStripContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.windowSplitter)).BeginInit();
            this.windowSplitter.Panel1.SuspendLayout();
            this.windowSplitter.Panel2.SuspendLayout();
            this.windowSplitter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlsOuterSplitter)).BeginInit();
            this.controlsOuterSplitter.Panel1.SuspendLayout();
            this.controlsOuterSplitter.Panel2.SuspendLayout();
            this.controlsOuterSplitter.SuspendLayout();
            this.mapTabs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlsInnerSplitter)).BeginInit();
            this.controlsInnerSplitter.Panel1.SuspendLayout();
            this.controlsInnerSplitter.Panel2.SuspendLayout();
            this.controlsInnerSplitter.SuspendLayout();
            this.infoTabs.SuspendLayout();
            this.consoleTab.SuspendLayout();
            this.actionTabs.SuspendLayout();
            this.SuspendLayout();
            // 
            // workingAreaToolStripContainer
            // 
            // 
            // workingAreaToolStripContainer.ContentPanel
            // 
            this.workingAreaToolStripContainer.ContentPanel.Margin = new System.Windows.Forms.Padding(0);
            this.workingAreaToolStripContainer.ContentPanel.Size = new System.Drawing.Size(928, 345);
            this.workingAreaToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.workingAreaToolStripContainer.Location = new System.Drawing.Point(0, 0);
            this.workingAreaToolStripContainer.Name = "workingAreaToolStripContainer";
            this.workingAreaToolStripContainer.Size = new System.Drawing.Size(928, 370);
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
            this.windowSplitter.SplitterDistance = 370;
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
            // controlsOuterSplitter.Panel1
            // 
            this.controlsOuterSplitter.Panel1.Controls.Add(this.mapTabs);
            // 
            // controlsOuterSplitter.Panel2
            // 
            this.controlsOuterSplitter.Panel2.Controls.Add(this.controlsInnerSplitter);
            this.controlsOuterSplitter.Size = new System.Drawing.Size(928, 171);
            this.controlsOuterSplitter.SplitterDistance = 280;
            this.controlsOuterSplitter.SplitterWidth = 1;
            this.controlsOuterSplitter.TabIndex = 0;
            // 
            // mapTabs
            // 
            this.mapTabs.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.mapTabs.Controls.Add(this.minimapTab);
            this.mapTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapTabs.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.mapTabs.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.mapTabs.ItemSize = new System.Drawing.Size(80, 20);
            this.mapTabs.Location = new System.Drawing.Point(0, 0);
            this.mapTabs.Name = "mapTabs";
            this.mapTabs.SelectedIndex = 0;
            this.mapTabs.Size = new System.Drawing.Size(280, 171);
            this.mapTabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.mapTabs.TabIndex = 0;
            theme1.ConsoleFont = new System.Drawing.Font("Consolas", 10F);
            theme1.ControlDarkColour = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            theme1.ControlLightColour = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            theme1.ControlMidColour = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            theme1.ErrorColour = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(63)))), ((int)(((byte)(38)))));
            theme1.HighlightLightColour = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            theme1.HighlightMidColour = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            theme1.OKColour = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(144)))), ((int)(((byte)(60)))));
            theme1.ReadOnly = false;
            theme1.SubtitleFont = new System.Drawing.Font("Segoe UI", 14F);
            theme1.TextDarkColour = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            theme1.TextLightColour = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            theme1.TextMidColour = System.Drawing.Color.FromArgb(((int)(((byte)(187)))), ((int)(((byte)(187)))), ((int)(((byte)(187)))));
            theme1.TitleFont = new System.Drawing.Font("Segoe UI", 32F);
            theme1.WarningColour = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(153)))), ((int)(((byte)(34)))));
            theme1.WindowFont = new System.Drawing.Font("Segoe UI", 9F);
            this.mapTabs.Theme = theme1;
            // 
            // minimapTab
            // 
            this.minimapTab.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.minimapTab.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.minimapTab.Location = new System.Drawing.Point(4, 4);
            this.minimapTab.Margin = new System.Windows.Forms.Padding(0);
            this.minimapTab.Name = "minimapTab";
            this.minimapTab.Size = new System.Drawing.Size(272, 143);
            this.minimapTab.TabIndex = 0;
            this.minimapTab.Text = "Minimap";
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
            // 
            // controlsInnerSplitter.Panel2
            // 
            this.controlsInnerSplitter.Panel2.Controls.Add(this.actionTabs);
            this.controlsInnerSplitter.Size = new System.Drawing.Size(647, 171);
            this.controlsInnerSplitter.SplitterDistance = 452;
            this.controlsInnerSplitter.SplitterWidth = 1;
            this.controlsInnerSplitter.TabIndex = 0;
            // 
            // infoTabs
            // 
            this.infoTabs.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.infoTabs.Controls.Add(this.infoTab);
            this.infoTabs.Controls.Add(this.consoleTab);
            this.infoTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoTabs.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.infoTabs.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.infoTabs.ItemSize = new System.Drawing.Size(100, 20);
            this.infoTabs.Location = new System.Drawing.Point(0, 0);
            this.infoTabs.Name = "infoTabs";
            this.infoTabs.SelectedIndex = 0;
            this.infoTabs.Size = new System.Drawing.Size(452, 171);
            this.infoTabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.infoTabs.TabIndex = 0;
            theme2.ConsoleFont = new System.Drawing.Font("Consolas", 10F);
            theme2.ControlDarkColour = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            theme2.ControlLightColour = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            theme2.ControlMidColour = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            theme2.ErrorColour = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(63)))), ((int)(((byte)(38)))));
            theme2.HighlightLightColour = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            theme2.HighlightMidColour = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            theme2.OKColour = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(144)))), ((int)(((byte)(60)))));
            theme2.ReadOnly = false;
            theme2.SubtitleFont = new System.Drawing.Font("Segoe UI", 14F);
            theme2.TextDarkColour = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            theme2.TextLightColour = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            theme2.TextMidColour = System.Drawing.Color.FromArgb(((int)(((byte)(187)))), ((int)(((byte)(187)))), ((int)(((byte)(187)))));
            theme2.TitleFont = new System.Drawing.Font("Segoe UI", 32F);
            theme2.WarningColour = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(153)))), ((int)(((byte)(34)))));
            theme2.WindowFont = new System.Drawing.Font("Segoe UI", 9F);
            this.infoTabs.Theme = theme2;
            // 
            // infoTab
            // 
            this.infoTab.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.infoTab.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.infoTab.Location = new System.Drawing.Point(4, 4);
            this.infoTab.Margin = new System.Windows.Forms.Padding(0);
            this.infoTab.Name = "infoTab";
            this.infoTab.Size = new System.Drawing.Size(444, 143);
            this.infoTab.TabIndex = 0;
            this.infoTab.Text = "Information";
            // 
            // consoleTab
            // 
            this.consoleTab.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.consoleTab.Controls.Add(this.consolePanel1);
            this.consoleTab.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.consoleTab.Location = new System.Drawing.Point(4, 4);
            this.consoleTab.Margin = new System.Windows.Forms.Padding(0);
            this.consoleTab.Name = "consoleTab";
            this.consoleTab.Size = new System.Drawing.Size(444, 143);
            this.consoleTab.TabIndex = 1;
            this.consoleTab.Text = "Console";
            // 
            // actionTabs
            // 
            this.actionTabs.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.actionTabs.Controls.Add(this.actionsTab);
            this.actionTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionTabs.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.actionTabs.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.actionTabs.ItemSize = new System.Drawing.Size(80, 20);
            this.actionTabs.Location = new System.Drawing.Point(0, 0);
            this.actionTabs.Name = "actionTabs";
            this.actionTabs.SelectedIndex = 0;
            this.actionTabs.Size = new System.Drawing.Size(194, 171);
            this.actionTabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.actionTabs.TabIndex = 0;
            theme3.ConsoleFont = new System.Drawing.Font("Consolas", 10F);
            theme3.ControlDarkColour = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            theme3.ControlLightColour = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            theme3.ControlMidColour = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            theme3.ErrorColour = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(63)))), ((int)(((byte)(38)))));
            theme3.HighlightLightColour = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            theme3.HighlightMidColour = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            theme3.OKColour = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(144)))), ((int)(((byte)(60)))));
            theme3.ReadOnly = false;
            theme3.SubtitleFont = new System.Drawing.Font("Segoe UI", 14F);
            theme3.TextDarkColour = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            theme3.TextLightColour = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            theme3.TextMidColour = System.Drawing.Color.FromArgb(((int)(((byte)(187)))), ((int)(((byte)(187)))), ((int)(((byte)(187)))));
            theme3.TitleFont = new System.Drawing.Font("Segoe UI", 32F);
            theme3.WarningColour = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(153)))), ((int)(((byte)(34)))));
            theme3.WindowFont = new System.Drawing.Font("Segoe UI", 9F);
            this.actionTabs.Theme = theme3;
            // 
            // actionsTab
            // 
            this.actionsTab.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.actionsTab.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.actionsTab.Location = new System.Drawing.Point(4, 4);
            this.actionsTab.Margin = new System.Windows.Forms.Padding(0);
            this.actionsTab.Name = "actionsTab";
            this.actionsTab.Size = new System.Drawing.Size(186, 143);
            this.actionsTab.TabIndex = 0;
            this.actionsTab.Text = "Actions";
            // 
            // consolePanel1
            // 
            this.consolePanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.consolePanel1.Location = new System.Drawing.Point(0, 0);
            this.consolePanel1.Margin = new System.Windows.Forms.Padding(0);
            this.consolePanel1.Name = "consolePanel1";
            this.consolePanel1.Size = new System.Drawing.Size(444, 143);
            this.consolePanel1.TabIndex = 0;
            this.consolePanel1.Theme = null;
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
            this.controlsOuterSplitter.Panel1.ResumeLayout(false);
            this.controlsOuterSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.controlsOuterSplitter)).EndInit();
            this.controlsOuterSplitter.ResumeLayout(false);
            this.mapTabs.ResumeLayout(false);
            this.controlsInnerSplitter.Panel1.ResumeLayout(false);
            this.controlsInnerSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.controlsInnerSplitter)).EndInit();
            this.controlsInnerSplitter.ResumeLayout(false);
            this.infoTabs.ResumeLayout(false);
            this.consoleTab.ResumeLayout(false);
            this.actionTabs.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer workingAreaToolStripContainer;
        private System.Windows.Forms.StatusStrip windowStatusStrip;
        private System.Windows.Forms.SplitContainer windowSplitter;
        private System.Windows.Forms.SplitContainer controlsOuterSplitter;
        private System.Windows.Forms.SplitContainer controlsInnerSplitter;
        private WiFindUs.Controls.ThemedTabControl infoTabs;
        private System.Windows.Forms.TabPage infoTab;
        private System.Windows.Forms.TabPage consoleTab;
        private Controls.ThemedTabControl mapTabs;
        private System.Windows.Forms.TabPage minimapTab;
        private Controls.ThemedTabControl actionTabs;
        private System.Windows.Forms.TabPage actionsTab;
        private Controls.ConsolePanel consolePanel1;


    }
}

