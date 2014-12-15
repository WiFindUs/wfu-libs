﻿using System;
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
                    scene.CameraFrustumChanged -= scene_CameraFrustumChanged;
                scene = value;
                if (scene != null)
                    scene.CameraFrustumChanged += scene_CameraFrustumChanged;
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

            if (IsDesignMode || scene == null || scene.BaseTile == null)
                return;

            //initialize render state
            e.Graphics.Clear(theme.ControlDarkColour);
            e.Graphics.SetQuality(GraphicsExtensions.GraphicsQuality.High);

            //draw base image
            if (scene.BaseTile.TileImage != null)
                e.Graphics.DrawImage(scene.BaseTile.TileImage, mapArea);

            //generate frustum poly
            Point[] points = new Point[4];
            points[0] = scene.CameraNorthWest == null
                ? new Point(mapArea.Left, mapArea.Top) : LocationToScreen(scene.CameraNorthWest);
            points[1] = scene.CameraNorthEast == null
                ? new Point(mapArea.Right, mapArea.Top) : LocationToScreen(scene.CameraNorthEast);
            points[2] = scene.CameraSouthEast == null
                ? new Point(mapArea.Right, mapArea.Bottom) : LocationToScreen(scene.CameraSouthEast);
            points[3] = scene.CameraSouthWest == null
                ? new Point(mapArea.Left, mapArea.Bottom) : LocationToScreen(scene.CameraSouthWest);

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

        private void scene_CameraFrustumChanged(MapScene obj)
        {
            Refresh();
        }
    }
}
