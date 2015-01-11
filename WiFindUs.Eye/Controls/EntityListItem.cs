using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiFindUs.Controls;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Controls
{
    public class EntityListItem : ThemedPanel
    {
        private ISelectableEntity entity;
        private Image image;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISelectableEntity Entity
        {
            get { return entity; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image Image
        {
            get { return image; }
            protected set
            {
                if (image == value)
                    return;
                image = value;
                Refresh();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected virtual Color ImagePlaceholderColour
        {
            get { return MouseHovering || entity.Selected ? Theme.ControlLightColour : Theme.ControlDarkColour;  }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected virtual String EntityDetailString
        {
            get { return ""; }
        }
        
        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public EntityListItem(ISelectableEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity", "Entity cannot be null!");
            this.entity = entity;
            entity.SelectedChanged += OnEntitySelectedChanged;
        }

        //

        public override void OnThemeChanged()
        {
            base.OnThemeChanged();

            SuspendLayout();
            Height = CalculateHeight();
            ResumeLayout(true);
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Focus();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            SuspendLayout();
            Height = CalculateHeight();
            ResumeLayout(true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (MouseHovering)
                e.Graphics.Clear(Theme.HighlightLightColour);
            else if (entity.Selected)
                e.Graphics.Clear(Theme.HighlightMidColour);
            else
                base.OnPaintBackground(e);

            using (Pen p = new Pen(Theme.ControlDarkColour))
                e.Graphics.DrawLine(p, 0, ClientRectangle.Height-1, ClientRectangle.Width, ClientRectangle.Height-1);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SetQuality(GraphicsExtensions.GraphicsQuality.High);

            if (image != null)
                e.Graphics.DrawImage(image, 4, 4, 40, 40);
            else
            {
                using (Brush brush = new SolidBrush(ImagePlaceholderColour))
                    e.Graphics.FillRectangle(brush,
                        4, 4, 40, 40);
            }

            //entity text
            string text = entity.ToString();
            SizeF sz = e.Graphics.MeasureString(
                text,
                Font,
                ClientRectangle.Width,
                StringFormat.GenericTypographic);
            e.Graphics.DrawString(
                text,
                Font,
                Theme.TextLightBrush,
                new Point(48, 4),
                StringFormat.GenericTypographic);

            //detail string
            text = EntityDetailString;
            if (text.Length <= 0)
                return;
            e.Graphics.DrawString(
                text,
                Font,
                Theme.TextMidBrush,
                new Point(48, 4 + (int)sz.Height),
                StringFormat.GenericTypographic);
        }

        protected override void OnMouseHoverChanged()
        {
            base.OnMouseHoverChanged();
            Refresh();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Focus();
                if (Control.ModifierKeys == Keys.Control)
                    entity.SelectionGroup.ToggleSelection(entity);
                else if (Control.ModifierKeys == Keys.Shift)
                {

                }
                else
                    entity.SelectionGroup.SetSelection(entity);
            }
            else
                base.OnMouseClick(e);
        }

        protected virtual void OnEntitySelectedChanged(ISelectableEntity entity)
        {
            if (entity != this.entity)
                return;
            Refresh();
        }

        protected virtual int CalculateHeight()
        {
            return Math.Max(48, System.Windows.Forms.TextRenderer.MeasureText("1\n2\n3", Font).Height + 8);
        }
    }
}
