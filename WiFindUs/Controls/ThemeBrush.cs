using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace WiFindUs.Controls
{
    public class ThemeBrush : IDisposable
    {
        private Color color;
        private Brush brush;
        private bool disposed = false;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public Color Color
        {
            get { return color; }
            set
            {
                if (color.Equals(value))
                    return;
                color = value;
                if (brush != null)
                    brush.Dispose();
                brush = new SolidBrush(color);
            }
        }

        public Brush Brush
        {
            get { return brush; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public ThemeBrush(Color color)
        {
            Color = color;
        }
        
        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (brush != null)
                    brush.Dispose();
            }

            disposed = true;
        }
    }
}
