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
            WiFindUs.Eye.SelectableEntityGroup selectableEntityGroup1 = new WiFindUs.Eye.SelectableEntityGroup();
            WiFindUs.Eye.SelectableEntityGroup selectableEntityGroup2 = new WiFindUs.Eye.SelectableEntityGroup();
            WiFindUs.Eye.SelectableEntityGroup selectableEntityGroup3 = new WiFindUs.Eye.SelectableEntityGroup();
            WiFindUs.Eye.SelectableEntityGroup selectableEntityGroup4 = new WiFindUs.Eye.SelectableEntityGroup();
            this.workingAreaToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this.windowStatusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.windowSplitter = new System.Windows.Forms.SplitContainer();
            this.workingAreaSplitter = new System.Windows.Forms.SplitContainer();
            this.entityTabs = new WiFindUs.Controls.ThemedTabControl();
            this.devicesTab = new System.Windows.Forms.TabPage();
            this.devicesFlowPanel = new WiFindUs.Eye.Controls.EntityList();
            this.usersTab = new System.Windows.Forms.TabPage();
            this.usersFlowPanel = new WiFindUs.Eye.Controls.EntityList();
            this.incidentsTab = new System.Windows.Forms.TabPage();
            this.incidentsFlowPanel = new WiFindUs.Eye.Controls.EntityList();
            this.nodesTab = new System.Windows.Forms.TabPage();
            this.nodesFlowPanel = new WiFindUs.Eye.Controls.EntityList();
            this.controlsOuterSplitter = new System.Windows.Forms.SplitContainer();
            this.actionTabs = new WiFindUs.Controls.ThemedTabControl();
            this.actionsTab = new System.Windows.Forms.TabPage();
            this.controlsInnerSplitter = new System.Windows.Forms.SplitContainer();
            this.infoTabs = new WiFindUs.Controls.ThemedTabControl();
            this.infoTab = new System.Windows.Forms.TabPage();
            this.consoleTab = new System.Windows.Forms.TabPage();
            this.consolePanel = new WiFindUs.Controls.ConsolePanel();
            this.mapTabs = new WiFindUs.Controls.ThemedTabControl();
            this.minimapTab = new System.Windows.Forms.TabPage();
            this.workingAreaToolStripContainer.SuspendLayout();
            this.windowStatusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.windowSplitter)).BeginInit();
            this.windowSplitter.Panel1.SuspendLayout();
            this.windowSplitter.Panel2.SuspendLayout();
            this.windowSplitter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.workingAreaSplitter)).BeginInit();
            this.workingAreaSplitter.Panel1.SuspendLayout();
            this.workingAreaSplitter.Panel2.SuspendLayout();
            this.workingAreaSplitter.SuspendLayout();
            this.entityTabs.SuspendLayout();
            this.devicesTab.SuspendLayout();
            this.usersTab.SuspendLayout();
            this.incidentsTab.SuspendLayout();
            this.nodesTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlsOuterSplitter)).BeginInit();
            this.controlsOuterSplitter.Panel1.SuspendLayout();
            this.controlsOuterSplitter.Panel2.SuspendLayout();
            this.controlsOuterSplitter.SuspendLayout();
            this.actionTabs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlsInnerSplitter)).BeginInit();
            this.controlsInnerSplitter.Panel1.SuspendLayout();
            this.controlsInnerSplitter.Panel2.SuspendLayout();
            this.controlsInnerSplitter.SuspendLayout();
            this.infoTabs.SuspendLayout();
            this.consoleTab.SuspendLayout();
            this.mapTabs.SuspendLayout();
            this.SuspendLayout();
            // 
            // workingAreaToolStripContainer
            // 
            // 
            // workingAreaToolStripContainer.ContentPanel
            // 
            this.workingAreaToolStripContainer.ContentPanel.Margin = new System.Windows.Forms.Padding(0);
            this.workingAreaToolStripContainer.ContentPanel.Size = new System.Drawing.Size(483, 282);
            this.workingAreaToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.workingAreaToolStripContainer.Location = new System.Drawing.Point(0, 0);
            this.workingAreaToolStripContainer.Name = "workingAreaToolStripContainer";
            this.workingAreaToolStripContainer.Size = new System.Drawing.Size(483, 307);
            this.workingAreaToolStripContainer.TabIndex = 0;
            this.workingAreaToolStripContainer.Text = "toolStripContainer1";
            // 
            // windowStatusStrip
            // 
            this.windowStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.windowStatusStrip.Location = new System.Drawing.Point(0, 540);
            this.windowStatusStrip.Name = "windowStatusStrip";
            this.windowStatusStrip.Size = new System.Drawing.Size(784, 22);
            this.windowStatusStrip.TabIndex = 1;
            this.windowStatusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(0, 17);
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
            this.windowSplitter.Panel1.Controls.Add(this.workingAreaSplitter);
            // 
            // windowSplitter.Panel2
            // 
            this.windowSplitter.Panel2.Controls.Add(this.controlsOuterSplitter);
            this.windowSplitter.Size = new System.Drawing.Size(784, 540);
            this.windowSplitter.SplitterDistance = 307;
            this.windowSplitter.SplitterWidth = 1;
            this.windowSplitter.TabIndex = 2;
            // 
            // workingAreaSplitter
            // 
            this.workingAreaSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.workingAreaSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.workingAreaSplitter.IsSplitterFixed = true;
            this.workingAreaSplitter.Location = new System.Drawing.Point(0, 0);
            this.workingAreaSplitter.Margin = new System.Windows.Forms.Padding(0);
            this.workingAreaSplitter.Name = "workingAreaSplitter";
            // 
            // workingAreaSplitter.Panel1
            // 
            this.workingAreaSplitter.Panel1.Controls.Add(this.entityTabs);
            // 
            // workingAreaSplitter.Panel2
            // 
            this.workingAreaSplitter.Panel2.Controls.Add(this.workingAreaToolStripContainer);
            this.workingAreaSplitter.Size = new System.Drawing.Size(784, 307);
            this.workingAreaSplitter.SplitterDistance = 300;
            this.workingAreaSplitter.SplitterWidth = 1;
            this.workingAreaSplitter.TabIndex = 1;
            // 
            // entityTabs
            // 
            this.entityTabs.Controls.Add(this.devicesTab);
            this.entityTabs.Controls.Add(this.usersTab);
            this.entityTabs.Controls.Add(this.incidentsTab);
            this.entityTabs.Controls.Add(this.nodesTab);
            this.entityTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entityTabs.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.entityTabs.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.entityTabs.ItemSize = new System.Drawing.Size(70, 30);
            this.entityTabs.Location = new System.Drawing.Point(0, 0);
            this.entityTabs.Margin = new System.Windows.Forms.Padding(0);
            this.entityTabs.Name = "entityTabs";
            this.entityTabs.Padding = new System.Drawing.Point(0, 0);
            this.entityTabs.SelectedIndex = 0;
            this.entityTabs.Size = new System.Drawing.Size(300, 307);
            this.entityTabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.entityTabs.TabIndex = 1;
            // 
            // devicesTab
            // 
            this.devicesTab.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.devicesTab.Controls.Add(this.devicesFlowPanel);
            this.devicesTab.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.devicesTab.Location = new System.Drawing.Point(4, 24);
            this.devicesTab.Margin = new System.Windows.Forms.Padding(0);
            this.devicesTab.Name = "devicesTab";
            this.devicesTab.Size = new System.Drawing.Size(292, 279);
            this.devicesTab.TabIndex = 0;
            this.devicesTab.Text = "Devices";
            // 
            // devicesFlowPanel
            // 
            this.devicesFlowPanel.AutoScroll = true;
            this.devicesFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.devicesFlowPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.devicesFlowPanel.Location = new System.Drawing.Point(0, 0);
            this.devicesFlowPanel.Margin = new System.Windows.Forms.Padding(0);
            this.devicesFlowPanel.Name = "devicesFlowPanel";
            selectableEntityGroup1.CaptureNotifies = false;
            this.devicesFlowPanel.SelectionGroup = selectableEntityGroup1;
            this.devicesFlowPanel.Size = new System.Drawing.Size(292, 279);
            this.devicesFlowPanel.TabIndex = 0;
            this.devicesFlowPanel.WrapContents = false;
            // 
            // usersTab
            // 
            this.usersTab.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.usersTab.Controls.Add(this.usersFlowPanel);
            this.usersTab.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.usersTab.Location = new System.Drawing.Point(4, 24);
            this.usersTab.Name = "usersTab";
            this.usersTab.Size = new System.Drawing.Size(292, 279);
            this.usersTab.TabIndex = 1;
            this.usersTab.Text = "Users";
            // 
            // usersFlowPanel
            // 
            this.usersFlowPanel.AutoScroll = true;
            this.usersFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.usersFlowPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.usersFlowPanel.Location = new System.Drawing.Point(0, 0);
            this.usersFlowPanel.Margin = new System.Windows.Forms.Padding(0);
            this.usersFlowPanel.Name = "usersFlowPanel";
            selectableEntityGroup2.CaptureNotifies = false;
            this.usersFlowPanel.SelectionGroup = selectableEntityGroup2;
            this.usersFlowPanel.Size = new System.Drawing.Size(292, 279);
            this.usersFlowPanel.TabIndex = 1;
            this.usersFlowPanel.WrapContents = false;
            // 
            // incidentsTab
            // 
            this.incidentsTab.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.incidentsTab.Controls.Add(this.incidentsFlowPanel);
            this.incidentsTab.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.incidentsTab.Location = new System.Drawing.Point(4, 34);
            this.incidentsTab.Name = "incidentsTab";
            this.incidentsTab.Size = new System.Drawing.Size(292, 269);
            this.incidentsTab.TabIndex = 2;
            this.incidentsTab.Text = "Incidents";
            // 
            // incidentsFlowPanel
            // 
            this.incidentsFlowPanel.AutoScroll = true;
            this.incidentsFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.incidentsFlowPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.incidentsFlowPanel.Location = new System.Drawing.Point(0, 0);
            this.incidentsFlowPanel.Margin = new System.Windows.Forms.Padding(0);
            this.incidentsFlowPanel.Name = "incidentsFlowPanel";
            selectableEntityGroup3.CaptureNotifies = false;
            this.incidentsFlowPanel.SelectionGroup = selectableEntityGroup3;
            this.incidentsFlowPanel.Size = new System.Drawing.Size(292, 269);
            this.incidentsFlowPanel.TabIndex = 2;
            this.incidentsFlowPanel.WrapContents = false;
            // 
            // nodesTab
            // 
            this.nodesTab.Controls.Add(this.nodesFlowPanel);
            this.nodesTab.Location = new System.Drawing.Point(4, 24);
            this.nodesTab.Name = "nodesTab";
            this.nodesTab.Size = new System.Drawing.Size(292, 276);
            this.nodesTab.TabIndex = 3;
            this.nodesTab.Text = "Nodes";
            this.nodesTab.UseVisualStyleBackColor = true;
            // 
            // nodesFlowPanel
            // 
            this.nodesFlowPanel.AutoScroll = true;
            this.nodesFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nodesFlowPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.nodesFlowPanel.Location = new System.Drawing.Point(0, 0);
            this.nodesFlowPanel.Margin = new System.Windows.Forms.Padding(0);
            this.nodesFlowPanel.Name = "nodesFlowPanel";
            selectableEntityGroup4.CaptureNotifies = false;
            this.nodesFlowPanel.SelectionGroup = selectableEntityGroup4;
            this.nodesFlowPanel.Size = new System.Drawing.Size(292, 276);
            this.nodesFlowPanel.TabIndex = 0;
            this.nodesFlowPanel.WrapContents = false;
            // 
            // controlsOuterSplitter
            // 
            this.controlsOuterSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlsOuterSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.controlsOuterSplitter.IsSplitterFixed = true;
            this.controlsOuterSplitter.Location = new System.Drawing.Point(0, 0);
            this.controlsOuterSplitter.Margin = new System.Windows.Forms.Padding(0);
            this.controlsOuterSplitter.Name = "controlsOuterSplitter";
            // 
            // controlsOuterSplitter.Panel1
            // 
            this.controlsOuterSplitter.Panel1.Controls.Add(this.actionTabs);
            // 
            // controlsOuterSplitter.Panel2
            // 
            this.controlsOuterSplitter.Panel2.Controls.Add(this.controlsInnerSplitter);
            this.controlsOuterSplitter.Size = new System.Drawing.Size(784, 232);
            this.controlsOuterSplitter.SplitterDistance = 300;
            this.controlsOuterSplitter.SplitterWidth = 1;
            this.controlsOuterSplitter.TabIndex = 0;
            // 
            // actionTabs
            // 
            this.actionTabs.Controls.Add(this.actionsTab);
            this.actionTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionTabs.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.actionTabs.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.actionTabs.ItemSize = new System.Drawing.Size(80, 30);
            this.actionTabs.Location = new System.Drawing.Point(0, 0);
            this.actionTabs.Margin = new System.Windows.Forms.Padding(0);
            this.actionTabs.Name = "actionTabs";
            this.actionTabs.Padding = new System.Drawing.Point(0, 0);
            this.actionTabs.SelectedIndex = 0;
            this.actionTabs.Size = new System.Drawing.Size(300, 232);
            this.actionTabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.actionTabs.TabIndex = 0;
            // 
            // actionsTab
            // 
            this.actionsTab.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.actionsTab.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.actionsTab.Location = new System.Drawing.Point(4, 34);
            this.actionsTab.Margin = new System.Windows.Forms.Padding(0);
            this.actionsTab.Name = "actionsTab";
            this.actionsTab.Size = new System.Drawing.Size(292, 194);
            this.actionsTab.TabIndex = 0;
            this.actionsTab.Text = "Actions";
            // 
            // controlsInnerSplitter
            // 
            this.controlsInnerSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlsInnerSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.controlsInnerSplitter.IsSplitterFixed = true;
            this.controlsInnerSplitter.Location = new System.Drawing.Point(0, 0);
            this.controlsInnerSplitter.Margin = new System.Windows.Forms.Padding(0);
            this.controlsInnerSplitter.Name = "controlsInnerSplitter";
            // 
            // controlsInnerSplitter.Panel1
            // 
            this.controlsInnerSplitter.Panel1.Controls.Add(this.infoTabs);
            // 
            // controlsInnerSplitter.Panel2
            // 
            this.controlsInnerSplitter.Panel2.Controls.Add(this.mapTabs);
            this.controlsInnerSplitter.Size = new System.Drawing.Size(483, 232);
            this.controlsInnerSplitter.SplitterDistance = 235;
            this.controlsInnerSplitter.SplitterWidth = 1;
            this.controlsInnerSplitter.TabIndex = 0;
            // 
            // infoTabs
            // 
            this.infoTabs.Controls.Add(this.infoTab);
            this.infoTabs.Controls.Add(this.consoleTab);
            this.infoTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoTabs.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.infoTabs.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.infoTabs.ItemSize = new System.Drawing.Size(100, 30);
            this.infoTabs.Location = new System.Drawing.Point(0, 0);
            this.infoTabs.Margin = new System.Windows.Forms.Padding(0);
            this.infoTabs.Name = "infoTabs";
            this.infoTabs.Padding = new System.Drawing.Point(0, 0);
            this.infoTabs.SelectedIndex = 0;
            this.infoTabs.Size = new System.Drawing.Size(235, 232);
            this.infoTabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.infoTabs.TabIndex = 0;
            // 
            // infoTab
            // 
            this.infoTab.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.infoTab.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.infoTab.Location = new System.Drawing.Point(4, 24);
            this.infoTab.Margin = new System.Windows.Forms.Padding(0);
            this.infoTab.Name = "infoTab";
            this.infoTab.Size = new System.Drawing.Size(227, 204);
            this.infoTab.TabIndex = 0;
            this.infoTab.Text = "Information";
            // 
            // consoleTab
            // 
            this.consoleTab.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.consoleTab.Controls.Add(this.consolePanel);
            this.consoleTab.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.consoleTab.Location = new System.Drawing.Point(4, 34);
            this.consoleTab.Margin = new System.Windows.Forms.Padding(0);
            this.consoleTab.Name = "consoleTab";
            this.consoleTab.Size = new System.Drawing.Size(227, 194);
            this.consoleTab.TabIndex = 1;
            this.consoleTab.Text = "Console";
            // 
            // consolePanel
            // 
            this.consolePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.consolePanel.Location = new System.Drawing.Point(0, 0);
            this.consolePanel.Margin = new System.Windows.Forms.Padding(0);
            this.consolePanel.Name = "consolePanel";
            this.consolePanel.Size = new System.Drawing.Size(227, 194);
            this.consolePanel.TabIndex = 0;
            // 
            // mapTabs
            // 
            this.mapTabs.Controls.Add(this.minimapTab);
            this.mapTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapTabs.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.mapTabs.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.mapTabs.ItemSize = new System.Drawing.Size(80, 30);
            this.mapTabs.Location = new System.Drawing.Point(0, 0);
            this.mapTabs.Margin = new System.Windows.Forms.Padding(0);
            this.mapTabs.Name = "mapTabs";
            this.mapTabs.Padding = new System.Drawing.Point(0, 0);
            this.mapTabs.SelectedIndex = 0;
            this.mapTabs.Size = new System.Drawing.Size(247, 232);
            this.mapTabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.mapTabs.TabIndex = 0;
            // 
            // minimapTab
            // 
            this.minimapTab.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.minimapTab.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.minimapTab.Location = new System.Drawing.Point(4, 34);
            this.minimapTab.Margin = new System.Windows.Forms.Padding(0);
            this.minimapTab.Name = "minimapTab";
            this.minimapTab.Size = new System.Drawing.Size(239, 194);
            this.minimapTab.TabIndex = 0;
            this.minimapTab.Text = "Minimap";
            // 
            // DispatcherForm
            // 
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.windowSplitter);
            this.Controls.Add(this.windowStatusStrip);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "DispatcherForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.workingAreaToolStripContainer.ResumeLayout(false);
            this.workingAreaToolStripContainer.PerformLayout();
            this.windowStatusStrip.ResumeLayout(false);
            this.windowStatusStrip.PerformLayout();
            this.windowSplitter.Panel1.ResumeLayout(false);
            this.windowSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.windowSplitter)).EndInit();
            this.windowSplitter.ResumeLayout(false);
            this.workingAreaSplitter.Panel1.ResumeLayout(false);
            this.workingAreaSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.workingAreaSplitter)).EndInit();
            this.workingAreaSplitter.ResumeLayout(false);
            this.entityTabs.ResumeLayout(false);
            this.devicesTab.ResumeLayout(false);
            this.usersTab.ResumeLayout(false);
            this.incidentsTab.ResumeLayout(false);
            this.nodesTab.ResumeLayout(false);
            this.controlsOuterSplitter.Panel1.ResumeLayout(false);
            this.controlsOuterSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.controlsOuterSplitter)).EndInit();
            this.controlsOuterSplitter.ResumeLayout(false);
            this.actionTabs.ResumeLayout(false);
            this.controlsInnerSplitter.Panel1.ResumeLayout(false);
            this.controlsInnerSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.controlsInnerSplitter)).EndInit();
            this.controlsInnerSplitter.ResumeLayout(false);
            this.infoTabs.ResumeLayout(false);
            this.consoleTab.ResumeLayout(false);
            this.mapTabs.ResumeLayout(false);
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
        private WiFindUs.Controls.ThemedTabControl mapTabs;
        private System.Windows.Forms.TabPage minimapTab;
        private WiFindUs.Controls.ThemedTabControl actionTabs;
        private System.Windows.Forms.TabPage actionsTab;
        private WiFindUs.Controls.ConsolePanel consolePanel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.SplitContainer workingAreaSplitter;
        private WiFindUs.Controls.ThemedTabControl entityTabs;
        private System.Windows.Forms.TabPage devicesTab;
        private System.Windows.Forms.TabPage usersTab;
        private System.Windows.Forms.TabPage incidentsTab;
        private WiFindUs.Eye.Controls.EntityList devicesFlowPanel;
        private WiFindUs.Eye.Controls.EntityList usersFlowPanel;
        private WiFindUs.Eye.Controls.EntityList incidentsFlowPanel;
        private System.Windows.Forms.TabPage nodesTab;
        private Controls.EntityList nodesFlowPanel;


    }
}

