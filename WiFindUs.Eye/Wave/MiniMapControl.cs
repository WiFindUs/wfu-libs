using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiFindUs.Controls;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave
{
    public class MiniMapControl : Control, IThemeable
    {
        private Theme theme;
        private MapScene scene;
        private Rectangle mapArea = Rectangle.Empty;
        private bool mouseEntered = false;
        private bool mouseDown = false;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public bool IsDesignMode
        {
            get
            {
                return DesignMode || this.IsDesignMode();
            }
        }

        public MapScene Scene
        {
            get { return scene; }
            set
            {
                if (value == scene)
                    return;
                if (scene != null)
                    scene.CameraChanged -= scene_CameraChanged;
                scene = value;
                if (scene != null)
                    scene.CameraChanged += scene_CameraChanged;
                Refresh();
            }
        }

        public Theme Theme
        {
            get
            {
                return theme;
            }
            set
            {
                if (value == null || value == theme)
                    return;

                theme = value;
                BackColor = theme.ControlLightColour;
                Font = theme.WindowFont;
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public MiniMapControl()
        {
            Margin = new Padding(0);
            TabStop = false;
            
            if (IsDesignMode)
                return;
            
            ResizeRedraw = false;
            DoubleBuffered = true;
            SetStyle(
                System.Windows.Forms.ControlStyles.UserPaint |
                System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
                System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,
                true);
            UpdateStyles();
        }

        public Point LocationToScreen(ILocation loc)
        {
            if (loc == null
                || !loc.HasLatLong
                || scene == null
                || scene.BaseTile == null
                || scene.BaseTile.Region == null)
                return Point.Empty;
            return Scene.BaseTile.Region.LocationToScreen(mapArea, loc);
        }

        public ILocation ScreenToLocation(Point point)
        {
            return new Location(
                scene.BaseTile.Region.NorthWest.Latitude - ((point.Y - mapArea.Top) / (float)mapArea.Height) * scene.BaseTile.Region.LatitudinalSpan,
                scene.BaseTile.Region.NorthWest.Longitude + ((point.X - mapArea.Left) / (float)mapArea.Width) * scene.BaseTile.Region.LongitudinalSpan
                );
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RecalculateMapArea();
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (IsDesignMode || Scene == null || Scene.BaseTile == null)
                return;

            e.Graphics.Clear(theme.ControlDarkColour);
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            //draw base image
            if (Scene.BaseTile.TileImage != null)
                e.Graphics.DrawImage(Scene.BaseTile.TileImage, mapArea);

            //generate frustum poly
            Point[] points = new Point[4];
            points[0] = Scene.CameraNorthWest == null ? new Point(mapArea.Left, mapArea.Top) : LocationToScreen(Scene.CameraNorthWest);
            points[1] = Scene.CameraNorthEast == null ? new Point(mapArea.Right, mapArea.Top) : LocationToScreen(Scene.CameraNorthEast);
            points[2] = Scene.CameraSouthEast == null ? new Point(mapArea.Right, mapArea.Bottom) : LocationToScreen(Scene.CameraSouthEast);
            points[3] = Scene.CameraSouthWest == null ? new Point(mapArea.Left, mapArea.Bottom) : LocationToScreen(Scene.CameraSouthWest);

            //darken non-focal area
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(points);
            System.Drawing.Region region = new System.Drawing.Region(path);
            region.Xor(ClientRectangle);
            using (Brush b = new SolidBrush(Color.FromArgb(180,0,0,0)))
                e.Graphics.FillRegion(b,region);

            //draw outline
            using (Pen p = new Pen(theme.HighlightMidColour, 1.5f))
                e.Graphics.DrawPolygon(p, points);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                mouseDown = true;
                if (mouseEntered)
                    MoveByMouse(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (mouseDown && mouseEntered)
                MoveByMouse(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                mouseDown = false;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            mouseEntered = true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            mouseEntered = false;
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void MoveByMouse(MouseEventArgs e)
        {
            if (scene == null
                || scene.BaseTile == null
                || scene.BaseTile.Region == null)
                return;

            scene.CameraTarget = scene.LocationToVector(
                ScreenToLocation(
                new Point(
                    e.X < mapArea.Left ? mapArea.Left : (e.X > mapArea.Right ? mapArea.Right : e.X),
                    e.Y < mapArea.Top ? mapArea.Top : (e.Y > mapArea.Bottom ? mapArea.Bottom : e.Y)
                    )));
        }

        private void RecalculateMapArea()
        {
            int size = Math.Min(ClientRectangle.Width, ClientRectangle.Height);
            mapArea = new Rectangle(ClientRectangle.Width / 2 - size / 2,
                ClientRectangle.Height / 2 - size / 2,
                size, size);
        }

        private void scene_CameraChanged(MapScene obj)
        {
            Refresh();
        }
    }
}
