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
        //colours
        private Color controlLightColour = ColorTranslator.FromHtml("#2d2d30");
        private Color controlMidColour = ColorTranslator.FromHtml("#252526");
        private Color controlDarkColour = ColorTranslator.FromHtml("#1e1e1e");
        private Color highlightColour = ColorTranslator.FromHtml("#007acc");
        private Color errorColour = ColorTranslator.FromHtml("#df3f26");
        private Color okColour = ColorTranslator.FromHtml("#39903c");
        private Color warningColour = ColorTranslator.FromHtml("#f09922");

        private Color textLightColour = ColorTranslator.FromHtml("#FFFFFF");
        private Color textMidColour = ColorTranslator.FromHtml("#BBBBBB");
        private Color textDarkColour = ColorTranslator.FromHtml("#999999");
        private Brush textLightBrush, textMidBrush, textDarkBrush;

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
            get { return controlLightColour; }
            set { if (!readOnly) controlLightColour = value; }
        }
        public Color ControlDarkColour
        {
            get { return controlDarkColour; }
            set { if (!readOnly) controlDarkColour = value; }
        }
        public Color HighlightColour
        {
            get { return highlightColour; }
            set { if (!readOnly) highlightColour = value; }
        }
        public Color ErrorColour
        {
            get { return errorColour; }
            set { if (!readOnly) errorColour = value; }
        }
        public Color WarningColour
        {
            get { return warningColour; }
            set { if (!readOnly) warningColour = value; }
        }
        public Color OKColour
        {
            get { return okColour; }
            set { if (!readOnly) okColour = value; }
        }
        public Color ControlMidColour
        {
            get { return controlMidColour; }
            set { if (!readOnly) controlMidColour = value; }
        }
        public Color TextLightColour
        {
            get { return textLightColour; }
            set
            {
                if (readOnly || textLightColour.Equals(value))
                    return;
                textLightColour = value;
                if (textLightBrush != null)
                    textLightBrush.Dispose();
                textLightBrush = new SolidBrush(textLightColour);
            }
        }
        public Color TextMidColour
        {
            get { return textMidColour; }
            set
            {
                if (readOnly || textMidColour.Equals(value))
                    return;
                textMidColour = value;
                if (textMidBrush != null)
                    textMidBrush.Dispose();
                textMidBrush = new SolidBrush(textMidColour);
            }
        }
        public Color TextDarkColour
        {
            get { return textDarkColour; }
            set
            {
                if (readOnly || textDarkColour.Equals(value))
                    return;
                textDarkColour = value;
                if (textDarkBrush != null)
                    textDarkBrush.Dispose();
                textDarkBrush = new SolidBrush(textDarkColour);
            }
        }
        public Brush TextLightBrush
        {
            get { return textLightBrush; }
        }
        public Brush TextMidBrush
        {
            get { return textMidBrush; }
        }
        public Brush TextDarkBrush
        {
            get { return textDarkBrush; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public Theme()
        {
            textLightBrush = new SolidBrush(textLightColour);
            textMidBrush = new SolidBrush(textMidColour);
            textDarkBrush = new SolidBrush(textDarkColour);
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
                if (windowFont != null)
                    windowFont.Dispose();
                if (titleFont != null)
                    titleFont.Dispose();
                if (subtitleFont != null)
                    subtitleFont.Dispose();
                if (textLightBrush != null)
                    textLightBrush.Dispose();
                if (textMidBrush != null)
                    textMidBrush.Dispose();
                if (textDarkBrush != null)
                    textDarkBrush.Dispose();
            }

            disposed = true;
        }
       
    }
}
