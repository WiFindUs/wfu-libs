﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WiFindUs.Extensions;

namespace WiFindUs.Themes
{
	public class ThemedListBox : ListBox, IThemeable
	{
		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsDesignMode
		{
			get { return DesignMode || this.IsDesignMode(); }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public ThemedListBox()
		{
			Margin = new Padding(0);
			DrawMode = DrawMode.OwnerDrawVariable;
			BorderStyle = BorderStyle.None;
			IntegralHeight = false;

			if (IsDesignMode)
				return;

			SetStyle(
				System.Windows.Forms.ControlStyles.UserPaint |
				System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
				System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,
				true);
			UpdateStyles();
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public virtual void ApplyTheme(ITheme theme)
		{
			if (theme == null)
				return;
			BackColor = theme.Background.Light.Colour;
			ForeColor = theme.Foreground.Light.Colour;
			Font = theme.Controls.Normal.Regular;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (IsDesignMode)
				return;
			ApplyTheme(Theme.Current);
			Theme.ThemeChanged += ApplyTheme;
		}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			if (this.Items.Count == 0)
				return;

			e.DrawBackground();
			ThemedListBoxItem displayItem = Items[e.Index] as ThemedListBoxItem;
			if (displayItem != null)
				displayItem.DrawListboxItem(e);
			else
				e.Graphics.DrawString(this.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), new PointF(e.Bounds.X, e.Bounds.Y));
			e.DrawFocusRectangle();
		}

		protected override void OnMeasureItem(MeasureItemEventArgs e)
		{
			base.OnMeasureItem(e);
			if (IsDesignMode || e.Index < 0)
				return;

			ThemedListBoxItem displayItem = Items[e.Index] as ThemedListBoxItem;
			if (displayItem != null)
				e.ItemHeight = displayItem.MeasureItemHeight(this, e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			System.Drawing.Region iRegion = new System.Drawing.Region(e.ClipRectangle);
			e.Graphics.FillRegion(new SolidBrush(this.BackColor), iRegion);
			if (this.Items.Count > 0)
			{
				for (int i = 0; i < this.Items.Count; ++i)
				{
					System.Drawing.Rectangle irect = this.GetItemRectangle(i);
					if (e.ClipRectangle.IntersectsWith(irect))
					{
						OnDrawItem(new DrawItemEventArgs(e.Graphics, Font,
							irect, i,
							((SelectionMode == SelectionMode.One && SelectedIndex == i)
							|| (SelectionMode == SelectionMode.MultiSimple && SelectedIndices.Contains(i))
							|| (SelectionMode == SelectionMode.MultiExtended && SelectedIndices.Contains(i)))
								? DrawItemState.Selected : DrawItemState.Default,
							this.ForeColor, this.BackColor));
						iRegion.Complement(irect);
					}
				}
			}
		}
	}

	public interface ThemedListBoxItem
	{
		int MeasureItemHeight(ThemedListBox host, MeasureItemEventArgs e);
		void DrawListboxItem(DrawItemEventArgs e);
	}
}
