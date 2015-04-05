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
			System.Windows.Forms.TabPage tabDevices;
			WiFindUs.Eye.SelectableEntityGroup selectableEntityGroup5 = new WiFindUs.Eye.SelectableEntityGroup();
			WiFindUs.Eye.SelectableEntityGroup selectableEntityGroup6 = new WiFindUs.Eye.SelectableEntityGroup();
			WiFindUs.Eye.SelectableEntityGroup selectableEntityGroup7 = new WiFindUs.Eye.SelectableEntityGroup();
			WiFindUs.Eye.SelectableEntityGroup selectableEntityGroup8 = new WiFindUs.Eye.SelectableEntityGroup();
			this.listDevices = new WiFindUs.Eye.Controls.EntityList();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.labelStatus = new System.Windows.Forms.ToolStripStatusLabel();
			this.splitterLeft = new WiFindUs.Themes.ThemedSplitContainer();
			this.tabsLeft = new WiFindUs.Themes.ThemedTabControl();
			this.tabUsers = new System.Windows.Forms.TabPage();
			this.listUsers = new WiFindUs.Eye.Controls.EntityList();
			this.tabNodes = new System.Windows.Forms.TabPage();
			this.listNodes = new WiFindUs.Eye.Controls.EntityList();
			this.splitterRight = new WiFindUs.Themes.ThemedSplitContainer();
			this.tabsMiddle = new WiFindUs.Themes.ThemedTabControl();
			this.tab3DMap = new System.Windows.Forms.TabPage();
			this.map = new WiFindUs.Eye.Wave.Controls.MapControl();
			this.splitterRightMiddle = new WiFindUs.Themes.ThemedSplitContainer();
			this.tabsTopRight = new WiFindUs.Themes.ThemedTabControl();
			this.tabIncidents = new System.Windows.Forms.TabPage();
			this.listIncidents = new WiFindUs.Eye.Controls.EntityList();
			this.tabsBottomRight = new WiFindUs.Themes.ThemedTabControl();
			this.tab2DMap = new System.Windows.Forms.TabPage();
			this.minimap = new WiFindUs.Eye.Wave.Controls.MiniMapControl();
			tabDevices = new System.Windows.Forms.TabPage();
			tabDevices.SuspendLayout();
			this.statusStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitterLeft)).BeginInit();
			this.splitterLeft.Panel1.SuspendLayout();
			this.splitterLeft.Panel2.SuspendLayout();
			this.splitterLeft.SuspendLayout();
			this.tabsLeft.SuspendLayout();
			this.tabUsers.SuspendLayout();
			this.tabNodes.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitterRight)).BeginInit();
			this.splitterRight.Panel1.SuspendLayout();
			this.splitterRight.Panel2.SuspendLayout();
			this.splitterRight.SuspendLayout();
			this.tabsMiddle.SuspendLayout();
			this.tab3DMap.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitterRightMiddle)).BeginInit();
			this.splitterRightMiddle.Panel1.SuspendLayout();
			this.splitterRightMiddle.Panel2.SuspendLayout();
			this.splitterRightMiddle.SuspendLayout();
			this.tabsTopRight.SuspendLayout();
			this.tabIncidents.SuspendLayout();
			this.tabsBottomRight.SuspendLayout();
			this.tab2DMap.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabDevices
			// 
			tabDevices.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
			tabDevices.Controls.Add(this.listDevices);
			tabDevices.Font = new System.Drawing.Font("Segoe UI", 9F);
			tabDevices.Location = new System.Drawing.Point(4, 34);
			tabDevices.Margin = new System.Windows.Forms.Padding(0);
			tabDevices.Name = "tabDevices";
			tabDevices.Size = new System.Drawing.Size(266, 502);
			tabDevices.TabIndex = 0;
			tabDevices.Text = "Devices";
			// 
			// listDevices
			// 
			this.listDevices.AutoScroll = true;
			this.listDevices.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listDevices.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.listDevices.Location = new System.Drawing.Point(0, 0);
			this.listDevices.Margin = new System.Windows.Forms.Padding(0);
			this.listDevices.Name = "listDevices";
			selectableEntityGroup5.CaptureNotifies = false;
			this.listDevices.SelectionGroup = selectableEntityGroup5;
			this.listDevices.Size = new System.Drawing.Size(266, 502);
			this.listDevices.TabIndex = 0;
			this.listDevices.WrapContents = false;
			// 
			// statusStrip
			// 
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelStatus});
			this.statusStrip.Location = new System.Drawing.Point(0, 540);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Size = new System.Drawing.Size(784, 22);
			this.statusStrip.TabIndex = 1;
			this.statusStrip.Text = "statusStrip1";
			// 
			// labelStatus
			// 
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(769, 17);
			this.labelStatus.Spring = true;
			// 
			// splitterLeft
			// 
			this.splitterLeft.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitterLeft.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitterLeft.Location = new System.Drawing.Point(0, 0);
			this.splitterLeft.Margin = new System.Windows.Forms.Padding(0);
			this.splitterLeft.Name = "splitterLeft";
			// 
			// splitterLeft.Panel1
			// 
			this.splitterLeft.Panel1.Controls.Add(this.tabsLeft);
			this.splitterLeft.Panel1MinSize = 150;
			// 
			// splitterLeft.Panel2
			// 
			this.splitterLeft.Panel2.Controls.Add(this.splitterRight);
			this.splitterLeft.Size = new System.Drawing.Size(784, 540);
			this.splitterLeft.SplitterDistance = 274;
			this.splitterLeft.TabIndex = 2;
			// 
			// tabsLeft
			// 
			this.tabsLeft.Controls.Add(tabDevices);
			this.tabsLeft.Controls.Add(this.tabUsers);
			this.tabsLeft.Controls.Add(this.tabNodes);
			this.tabsLeft.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabsLeft.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
			this.tabsLeft.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.tabsLeft.ItemSize = new System.Drawing.Size(70, 30);
			this.tabsLeft.Location = new System.Drawing.Point(0, 0);
			this.tabsLeft.Margin = new System.Windows.Forms.Padding(0);
			this.tabsLeft.Name = "tabsLeft";
			this.tabsLeft.Padding = new System.Drawing.Point(0, 0);
			this.tabsLeft.SelectedIndex = 0;
			this.tabsLeft.Size = new System.Drawing.Size(274, 540);
			this.tabsLeft.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
			this.tabsLeft.TabIndex = 1;
			// 
			// tabUsers
			// 
			this.tabUsers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
			this.tabUsers.Controls.Add(this.listUsers);
			this.tabUsers.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.tabUsers.Location = new System.Drawing.Point(4, 34);
			this.tabUsers.Name = "tabUsers";
			this.tabUsers.Size = new System.Drawing.Size(266, 502);
			this.tabUsers.TabIndex = 1;
			this.tabUsers.Text = "Users";
			// 
			// listUsers
			// 
			this.listUsers.AutoScroll = true;
			this.listUsers.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listUsers.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.listUsers.Location = new System.Drawing.Point(0, 0);
			this.listUsers.Margin = new System.Windows.Forms.Padding(0);
			this.listUsers.Name = "listUsers";
			selectableEntityGroup6.CaptureNotifies = false;
			this.listUsers.SelectionGroup = selectableEntityGroup6;
			this.listUsers.Size = new System.Drawing.Size(266, 502);
			this.listUsers.TabIndex = 1;
			this.listUsers.WrapContents = false;
			// 
			// tabNodes
			// 
			this.tabNodes.Controls.Add(this.listNodes);
			this.tabNodes.Location = new System.Drawing.Point(4, 34);
			this.tabNodes.Name = "tabNodes";
			this.tabNodes.Size = new System.Drawing.Size(266, 502);
			this.tabNodes.TabIndex = 3;
			this.tabNodes.Text = "Nodes";
			this.tabNodes.UseVisualStyleBackColor = true;
			// 
			// listNodes
			// 
			this.listNodes.AutoScroll = true;
			this.listNodes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listNodes.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.listNodes.Location = new System.Drawing.Point(0, 0);
			this.listNodes.Margin = new System.Windows.Forms.Padding(0);
			this.listNodes.Name = "listNodes";
			selectableEntityGroup7.CaptureNotifies = false;
			this.listNodes.SelectionGroup = selectableEntityGroup7;
			this.listNodes.Size = new System.Drawing.Size(266, 502);
			this.listNodes.TabIndex = 0;
			this.listNodes.WrapContents = false;
			// 
			// splitterRight
			// 
			this.splitterRight.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitterRight.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitterRight.Location = new System.Drawing.Point(0, 0);
			this.splitterRight.Margin = new System.Windows.Forms.Padding(0);
			this.splitterRight.Name = "splitterRight";
			// 
			// splitterRight.Panel1
			// 
			this.splitterRight.Panel1.Controls.Add(this.tabsMiddle);
			// 
			// splitterRight.Panel2
			// 
			this.splitterRight.Panel2.Controls.Add(this.splitterRightMiddle);
			this.splitterRight.Panel2MinSize = 150;
			this.splitterRight.Size = new System.Drawing.Size(506, 540);
			this.splitterRight.SplitterDistance = 232;
			this.splitterRight.TabIndex = 1;
			// 
			// tabsMiddle
			// 
			this.tabsMiddle.Controls.Add(this.tab3DMap);
			this.tabsMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabsMiddle.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
			this.tabsMiddle.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.tabsMiddle.ItemSize = new System.Drawing.Size(80, 30);
			this.tabsMiddle.Location = new System.Drawing.Point(0, 0);
			this.tabsMiddle.Margin = new System.Windows.Forms.Padding(0);
			this.tabsMiddle.Name = "tabsMiddle";
			this.tabsMiddle.Padding = new System.Drawing.Point(0, 0);
			this.tabsMiddle.SelectedIndex = 0;
			this.tabsMiddle.Size = new System.Drawing.Size(232, 540);
			this.tabsMiddle.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
			this.tabsMiddle.TabIndex = 2;
			// 
			// tab3DMap
			// 
			this.tab3DMap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
			this.tab3DMap.Controls.Add(this.map);
			this.tab3DMap.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.tab3DMap.Location = new System.Drawing.Point(4, 34);
			this.tab3DMap.Margin = new System.Windows.Forms.Padding(0);
			this.tab3DMap.Name = "tab3DMap";
			this.tab3DMap.Size = new System.Drawing.Size(224, 502);
			this.tab3DMap.TabIndex = 0;
			this.tab3DMap.Text = "3D Map";
			// 
			// map
			// 
			this.map.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.map.Dock = System.Windows.Forms.DockStyle.Fill;
			this.map.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.map.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.map.Location = new System.Drawing.Point(0, 0);
			this.map.Margin = new System.Windows.Forms.Padding(0);
			this.map.Name = "map";
			this.map.Size = new System.Drawing.Size(224, 502);
			this.map.TabIndex = 0;
			this.map.TabStop = false;
			this.map.Text = "map";
			// 
			// splitterRightMiddle
			// 
			this.splitterRightMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitterRightMiddle.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitterRightMiddle.Location = new System.Drawing.Point(0, 0);
			this.splitterRightMiddle.Margin = new System.Windows.Forms.Padding(0);
			this.splitterRightMiddle.Name = "splitterRightMiddle";
			this.splitterRightMiddle.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitterRightMiddle.Panel1
			// 
			this.splitterRightMiddle.Panel1.Controls.Add(this.tabsTopRight);
			// 
			// splitterRightMiddle.Panel2
			// 
			this.splitterRightMiddle.Panel2.Controls.Add(this.tabsBottomRight);
			this.splitterRightMiddle.Panel2MinSize = 150;
			this.splitterRightMiddle.Size = new System.Drawing.Size(270, 540);
			this.splitterRightMiddle.SplitterDistance = 303;
			this.splitterRightMiddle.TabIndex = 0;
			// 
			// tabsTopRight
			// 
			this.tabsTopRight.Controls.Add(this.tabIncidents);
			this.tabsTopRight.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabsTopRight.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
			this.tabsTopRight.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.tabsTopRight.ItemSize = new System.Drawing.Size(80, 30);
			this.tabsTopRight.Location = new System.Drawing.Point(0, 0);
			this.tabsTopRight.Margin = new System.Windows.Forms.Padding(0);
			this.tabsTopRight.Name = "tabsTopRight";
			this.tabsTopRight.Padding = new System.Drawing.Point(0, 0);
			this.tabsTopRight.SelectedIndex = 0;
			this.tabsTopRight.Size = new System.Drawing.Size(270, 303);
			this.tabsTopRight.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
			this.tabsTopRight.TabIndex = 1;
			// 
			// tabIncidents
			// 
			this.tabIncidents.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
			this.tabIncidents.Controls.Add(this.listIncidents);
			this.tabIncidents.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.tabIncidents.Location = new System.Drawing.Point(4, 34);
			this.tabIncidents.Margin = new System.Windows.Forms.Padding(0);
			this.tabIncidents.Name = "tabIncidents";
			this.tabIncidents.Size = new System.Drawing.Size(262, 265);
			this.tabIncidents.TabIndex = 0;
			this.tabIncidents.Text = "Incidents";
			// 
			// listIncidents
			// 
			this.listIncidents.AutoScroll = true;
			this.listIncidents.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listIncidents.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.listIncidents.Location = new System.Drawing.Point(0, 0);
			this.listIncidents.Margin = new System.Windows.Forms.Padding(0);
			this.listIncidents.Name = "listIncidents";
			selectableEntityGroup8.CaptureNotifies = false;
			this.listIncidents.SelectionGroup = selectableEntityGroup8;
			this.listIncidents.Size = new System.Drawing.Size(262, 265);
			this.listIncidents.TabIndex = 2;
			this.listIncidents.WrapContents = false;
			// 
			// tabsBottomRight
			// 
			this.tabsBottomRight.Controls.Add(this.tab2DMap);
			this.tabsBottomRight.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabsBottomRight.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
			this.tabsBottomRight.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.tabsBottomRight.ItemSize = new System.Drawing.Size(80, 30);
			this.tabsBottomRight.Location = new System.Drawing.Point(0, 0);
			this.tabsBottomRight.Margin = new System.Windows.Forms.Padding(0);
			this.tabsBottomRight.Name = "tabsBottomRight";
			this.tabsBottomRight.Padding = new System.Drawing.Point(0, 0);
			this.tabsBottomRight.SelectedIndex = 0;
			this.tabsBottomRight.Size = new System.Drawing.Size(270, 233);
			this.tabsBottomRight.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
			this.tabsBottomRight.TabIndex = 0;
			// 
			// tab2DMap
			// 
			this.tab2DMap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
			this.tab2DMap.Controls.Add(this.minimap);
			this.tab2DMap.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.tab2DMap.Location = new System.Drawing.Point(4, 34);
			this.tab2DMap.Margin = new System.Windows.Forms.Padding(0);
			this.tab2DMap.Name = "tab2DMap";
			this.tab2DMap.Size = new System.Drawing.Size(262, 195);
			this.tab2DMap.TabIndex = 0;
			this.tab2DMap.Text = "2D Map";
			// 
			// minimap
			// 
			this.minimap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
			this.minimap.Dock = System.Windows.Forms.DockStyle.Fill;
			this.minimap.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.minimap.Location = new System.Drawing.Point(0, 0);
			this.minimap.Margin = new System.Windows.Forms.Padding(0);
			this.minimap.Name = "minimap";
			this.minimap.Size = new System.Drawing.Size(262, 195);
			this.minimap.TabIndex = 0;
			this.minimap.TabStop = false;
			this.minimap.Text = "minimap";
			// 
			// DispatcherForm
			// 
			this.ClientSize = new System.Drawing.Size(784, 562);
			this.Controls.Add(this.splitterLeft);
			this.Controls.Add(this.statusStrip);
			this.MinimumSize = new System.Drawing.Size(800, 600);
			this.Name = "DispatcherForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			tabDevices.ResumeLayout(false);
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.splitterLeft.Panel1.ResumeLayout(false);
			this.splitterLeft.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitterLeft)).EndInit();
			this.splitterLeft.ResumeLayout(false);
			this.tabsLeft.ResumeLayout(false);
			this.tabUsers.ResumeLayout(false);
			this.tabNodes.ResumeLayout(false);
			this.splitterRight.Panel1.ResumeLayout(false);
			this.splitterRight.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitterRight)).EndInit();
			this.splitterRight.ResumeLayout(false);
			this.tabsMiddle.ResumeLayout(false);
			this.tab3DMap.ResumeLayout(false);
			this.splitterRightMiddle.Panel1.ResumeLayout(false);
			this.splitterRightMiddle.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitterRightMiddle)).EndInit();
			this.splitterRightMiddle.ResumeLayout(false);
			this.tabsTopRight.ResumeLayout(false);
			this.tabIncidents.ResumeLayout(false);
			this.tabsBottomRight.ResumeLayout(false);
			this.tab2DMap.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
		private WiFindUs.Themes.ThemedSplitContainer splitterLeft;
		private WiFindUs.Themes.ThemedSplitContainer splitterRightMiddle;
        private WiFindUs.Themes.ThemedTabControl tabsBottomRight;
        private System.Windows.Forms.TabPage tab2DMap;
        private System.Windows.Forms.ToolStripStatusLabel labelStatus;
		private WiFindUs.Themes.ThemedSplitContainer splitterRight;
		private WiFindUs.Themes.ThemedTabControl tabsLeft;
        private WiFindUs.Eye.Controls.EntityList listDevices;
        private WiFindUs.Eye.Controls.EntityList listUsers;
        private WiFindUs.Eye.Controls.EntityList listIncidents;
        private System.Windows.Forms.TabPage tabNodes;
		private Controls.EntityList listNodes;
		private WiFindUs.Themes.ThemedTabControl tabsTopRight;
        private System.Windows.Forms.TabPage tabIncidents;
		private WiFindUs.Themes.ThemedTabControl tabsMiddle;
        private System.Windows.Forms.TabPage tab3DMap;
		private System.Windows.Forms.TabPage tabUsers;
		private Wave.Controls.MiniMapControl minimap;
		private Wave.Controls.MapControl map;
    }
}

