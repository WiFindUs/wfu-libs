namespace WiFindUs.Controls
{
    partial class ConsolePanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.console = new WiFindUs.Controls.ConsoleTextBox();
            this.controls = new System.Windows.Forms.FlowLayoutPanel();
            this.verboseToggle = new System.Windows.Forms.CheckBox();
            this.infoToggle = new System.Windows.Forms.CheckBox();
            this.warningToggle = new System.Windows.Forms.CheckBox();
            this.errorToggle = new System.Windows.Forms.CheckBox();
            this.exceptionToggle = new System.Windows.Forms.CheckBox();
            this.consoleToggle = new System.Windows.Forms.CheckBox();
            this.input = new System.Windows.Forms.TextBox();
            this.controls.SuspendLayout();
            this.SuspendLayout();
            // 
            // console
            // 
            this.console.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.console.Dock = System.Windows.Forms.DockStyle.Top;
            this.console.Location = new System.Drawing.Point(0, 0);
            this.console.Margin = new System.Windows.Forms.Padding(0);
            this.console.Name = "console";
            this.console.ReadOnly = true;
            this.console.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.console.Size = new System.Drawing.Size(662, 410);
            this.console.TabIndex = 0;
            this.console.TabStop = false;
            this.console.Text = "";
            // 
            // controls
            // 
            this.controls.Controls.Add(this.verboseToggle);
            this.controls.Controls.Add(this.infoToggle);
            this.controls.Controls.Add(this.warningToggle);
            this.controls.Controls.Add(this.errorToggle);
            this.controls.Controls.Add(this.exceptionToggle);
            this.controls.Controls.Add(this.consoleToggle);
            this.controls.Controls.Add(this.input);
            this.controls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.controls.Location = new System.Drawing.Point(0, 414);
            this.controls.Margin = new System.Windows.Forms.Padding(0);
            this.controls.Name = "controls";
            this.controls.Size = new System.Drawing.Size(662, 22);
            this.controls.TabIndex = 1;
            this.controls.WrapContents = false;
            // 
            // verboseToggle
            // 
            this.verboseToggle.Appearance = System.Windows.Forms.Appearance.Button;
            this.verboseToggle.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.verboseToggle.FlatAppearance.BorderSize = 0;
            this.verboseToggle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.verboseToggle.Location = new System.Drawing.Point(0, 1);
            this.verboseToggle.Margin = new System.Windows.Forms.Padding(0, 1, 1, 1);
            this.verboseToggle.Name = "verboseToggle";
            this.verboseToggle.Size = new System.Drawing.Size(24, 20);
            this.verboseToggle.TabIndex = 0;
            this.verboseToggle.Text = "V";
            this.verboseToggle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.verboseToggle.UseVisualStyleBackColor = true;
            // 
            // infoToggle
            // 
            this.infoToggle.Appearance = System.Windows.Forms.Appearance.Button;
            this.infoToggle.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.infoToggle.FlatAppearance.BorderSize = 0;
            this.infoToggle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.infoToggle.Location = new System.Drawing.Point(25, 1);
            this.infoToggle.Margin = new System.Windows.Forms.Padding(0, 1, 1, 1);
            this.infoToggle.Name = "infoToggle";
            this.infoToggle.Size = new System.Drawing.Size(24, 20);
            this.infoToggle.TabIndex = 1;
            this.infoToggle.Text = "I";
            this.infoToggle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.infoToggle.UseVisualStyleBackColor = true;
            // 
            // warningToggle
            // 
            this.warningToggle.Appearance = System.Windows.Forms.Appearance.Button;
            this.warningToggle.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.warningToggle.FlatAppearance.BorderSize = 0;
            this.warningToggle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.warningToggle.Location = new System.Drawing.Point(50, 1);
            this.warningToggle.Margin = new System.Windows.Forms.Padding(0, 1, 1, 1);
            this.warningToggle.Name = "warningToggle";
            this.warningToggle.Size = new System.Drawing.Size(24, 20);
            this.warningToggle.TabIndex = 2;
            this.warningToggle.Text = "W";
            this.warningToggle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.warningToggle.UseVisualStyleBackColor = true;
            // 
            // errorToggle
            // 
            this.errorToggle.Appearance = System.Windows.Forms.Appearance.Button;
            this.errorToggle.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.errorToggle.FlatAppearance.BorderSize = 0;
            this.errorToggle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.errorToggle.Location = new System.Drawing.Point(75, 1);
            this.errorToggle.Margin = new System.Windows.Forms.Padding(0, 1, 1, 1);
            this.errorToggle.Name = "errorToggle";
            this.errorToggle.Size = new System.Drawing.Size(24, 20);
            this.errorToggle.TabIndex = 3;
            this.errorToggle.Text = "E";
            this.errorToggle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.errorToggle.UseVisualStyleBackColor = true;
            // 
            // exceptionToggle
            // 
            this.exceptionToggle.Appearance = System.Windows.Forms.Appearance.Button;
            this.exceptionToggle.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.exceptionToggle.FlatAppearance.BorderSize = 0;
            this.exceptionToggle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.exceptionToggle.Location = new System.Drawing.Point(100, 1);
            this.exceptionToggle.Margin = new System.Windows.Forms.Padding(0, 1, 1, 1);
            this.exceptionToggle.Name = "exceptionToggle";
            this.exceptionToggle.Size = new System.Drawing.Size(24, 20);
            this.exceptionToggle.TabIndex = 4;
            this.exceptionToggle.Text = "X";
            this.exceptionToggle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.exceptionToggle.UseVisualStyleBackColor = true;
            // 
            // consoleToggle
            // 
            this.consoleToggle.Appearance = System.Windows.Forms.Appearance.Button;
            this.consoleToggle.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.consoleToggle.FlatAppearance.BorderSize = 0;
            this.consoleToggle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.consoleToggle.Location = new System.Drawing.Point(125, 1);
            this.consoleToggle.Margin = new System.Windows.Forms.Padding(0, 1, 1, 1);
            this.consoleToggle.Name = "consoleToggle";
            this.consoleToggle.Size = new System.Drawing.Size(24, 20);
            this.consoleToggle.TabIndex = 5;
            this.consoleToggle.Text = "C";
            this.consoleToggle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.consoleToggle.UseVisualStyleBackColor = true;
            // 
            // input
            // 
            this.input.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.input.Location = new System.Drawing.Point(150, 1);
            this.input.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.input.Name = "input";
            this.input.Size = new System.Drawing.Size(488, 20);
            this.input.TabIndex = 6;
            this.input.WordWrap = false;
            this.input.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.InputKeyPress);
            // 
            // ConsolePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.controls);
            this.Controls.Add(this.console);
            this.Name = "ConsolePanel";
            this.Size = new System.Drawing.Size(662, 436);
            this.controls.ResumeLayout(false);
            this.controls.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ConsoleTextBox console;
        private System.Windows.Forms.FlowLayoutPanel controls;
        private System.Windows.Forms.CheckBox verboseToggle;
        private System.Windows.Forms.CheckBox infoToggle;
        private System.Windows.Forms.CheckBox warningToggle;
        private System.Windows.Forms.CheckBox errorToggle;
        private System.Windows.Forms.CheckBox exceptionToggle;
        private System.Windows.Forms.CheckBox consoleToggle;
        private System.Windows.Forms.TextBox input;

    }
}
