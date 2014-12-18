using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WiFindUs.Controls
{   
    public class Theme : IDisposable
    {
        private bool readOnly = false;
        private bool disposed = false;
        //fonts
        private Font windowFont = new Font("Segoe UI", 9.0f);
        private Font titleFont = new Font("Segoe UI", 32.0f);
        private Font subtitleFont = new Font("Segoe UI", 14.0f);
        private Font consoleFont = new Font("Consolas", 10.0f);
        //brushes
        private ThemeBrush controlLight = new ThemeBrush(ColorTranslator.FromHtml("#2d2d30"));
        private ThemeBrush controlMid = new ThemeBrush(ColorTranslator.FromHtml("#252526"));
        private ThemeBrush controlDark = new ThemeBrush(ColorTranslator.FromHtml("#1e1e1e"));
        private ThemeBrush highlightLight = new ThemeBrush(ColorTranslator.FromHtml("#1c97ea"));
        private ThemeBrush highlightMid = new ThemeBrush(ColorTranslator.FromHtml("#007acc"));
        private ThemeBrush error = new ThemeBrush(ColorTranslator.FromHtml("#df3f26"));
        private ThemeBrush ok = new ThemeBrush(ColorTranslator.FromHtml("#39903c"));
        private ThemeBrush warning = new ThemeBrush(ColorTranslator.FromHtml("#FF6600"));
        private ThemeBrush textLight = new ThemeBrush(ColorTranslator.FromHtml("#FFFFFF"));
        private ThemeBrush textMid = new ThemeBrush(ColorTranslator.FromHtml("#BBBBBB"));
        private ThemeBrush textDark = new ThemeBrush(ColorTranslator.FromHtml("#999999"));

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public bool ReadOnly
        {
            get { return readOnly; }
            set { if (!readOnly) readOnly = value; }
        }

        public Font WindowFont
        {
            get { return windowFont; }
            set
            {
                if (readOnly || value == null || value == windowFont)
                    return;
                if (windowFont != null)
                    windowFont.Dispose();
                windowFont = value;
            }
        }
        public Font TitleFont
        {
            get { return titleFont; }
            set
            {
                if (readOnly || value == null || value == titleFont)
                    return;
                if (titleFont != null)
                    titleFont.Dispose();
                titleFont = value;
            }
        }
        public Font SubtitleFont
        {
            get { return subtitleFont; }
            set
            {
                if (readOnly || value == null || value == subtitleFont)
                    return;
                if (subtitleFont != null)
                    subtitleFont.Dispose();
                subtitleFont = value;
            }
        }
        public Font ConsoleFont
        {
            get { return consoleFont; }
            set
            {
                if (readOnly || value == null || value == consoleFont)
                    return;
                if (consoleFont != null)
                    consoleFont.Dispose();
                consoleFont = value;
            }
        }
        public Color ControlLightColour
        {
            get { return controlLight.Color; }
            set { if (!readOnly) controlLight.Color = value; }
        }
        public Brush ControlLightBrush
        {
            get { return controlLight.Brush; }
        }
        public Color ControlMidColour
        {
            get { return controlMid.Color; }
            set { if (!readOnly) controlMid.Color = value; }
        }
        public Brush ControlMidBrush
        {
            get { return controlMid.Brush; }
        }
        public Color ControlDarkColour
        {
            get { return controlDark.Color; }
            set { if (!readOnly) controlDark.Color = value; }
        }
        public Brush ControlDarkBrush
        {
            get { return controlDark.Brush; }
        }
        public Color HighlightLightColour
        {
            get { return highlightLight.Color; }
            set { if (!readOnly) highlightLight.Color = value; }
        }
        public Brush HighlightLightBrush
        {
            get { return highlightLight.Brush; }
        }
        public Color HighlightMidColour
        {
            get { return highlightMid.Color; }
            set { if (!readOnly) highlightMid.Color = value; }
        }
        public Brush HighlightMidBrush
        {
            get { return highlightMid.Brush; }
        }
        public Color ErrorColour
        {
            get { return error.Color; }
            set { if (!readOnly) error.Color = value; }
        }
        public Brush ErrorBrush
        {
            get { return error.Brush; }
        }
        public Color WarningColour
        {
            get { return warning.Color; }
            set { if (!readOnly) warning.Color = value; }
        }
        public Brush WarningBrush
        {
            get { return warning.Brush; }
        }
        public Color OKColour
        {
            get { return ok.Color; }
            set { if (!readOnly) ok.Color = value; }
        }
        public Brush OKBrush
        {
            get { return ok.Brush; }
        }
        public Color TextLightColour
        {
            get { return textLight.Color; }
            set { if (!readOnly) textLight.Color = value; }
        }
        public Brush TextLightBrush
        {
            get { return textLight.Brush; }
        }
        public Color TextMidColour
        {
            get { return textMid.Color; }
            set { if (!readOnly) textMid.Color = value; }
        }
        public Brush TextMidBrush
        {
            get { return textMid.Brush; }
        }
        public Color TextDarkColour
        {
            get { return textDark.Color; }
            set { if (!readOnly) textDark.Color = value; }
        }
        public Brush TextDarkBrush
        {
            get { return textDark.Brush; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public Theme()
        {

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
                //fonts
                if (windowFont != null)
                    windowFont.Dispose();
                if (titleFont != null)
                    titleFont.Dispose();
                if (subtitleFont != null)
                    subtitleFont.Dispose();
                if (consoleFont != null)
                    consoleFont.Dispose();

                //brushes
                if (controlLight != null)
                    controlLight.Dispose();
                if (controlMid != null)
                    controlMid.Dispose();
                if (controlDark != null)
                    controlDark.Dispose();
                if (highlightLight != null)
                    highlightLight.Dispose();
                if (highlightMid != null)
                    highlightMid.Dispose();
                if (error != null)
                    error.Dispose();
                if (ok != null)
                    ok.Dispose();
                if (warning != null)
                    warning.Dispose();
                if (textLight != null)
                    textLight.Dispose();
                if (textMid != null)
                    textMid.Dispose();
                if (textDark != null)
                    textDark.Dispose();

            }

            disposed = true;
        }
       
    }
}
