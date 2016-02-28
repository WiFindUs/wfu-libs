using System.Drawing;
using System.Windows.Forms;

namespace WiFindUs
{
    public static class CursorManager
	{
		private static bool hidden = false;
		private static bool clipped = false;

		public static void Hide()
		{
			if (hidden)
				return;
            Cursor.Hide();
			hidden = true;
		}

		public static void Show()
		{
			if (!hidden)
				return;
            Cursor.Show();
			hidden = false;
		}

		public static void Clip(Rectangle clipRect)
		{
			if (clipped)
				return;
			clipped = true;
            Cursor.Clip = clipRect;
		}

		public static void Clip(Control control)
		{
			Clip(control.RectangleToScreen(control.Bounds));
		}

		public static void Unclip()
		{
			if (!clipped)
				return;
			clipped = false;
            Cursor.Clip = Rectangle.Empty;
		}

		public static void Shift(int dx, int dy)
		{
			Point newP = new Point(
                Cursor.Position.X + dx,
                Cursor.Position.Y + dy
				);

			Debugger.C("{0} + {1},{2} = {3}", Cursor.Position, dx, dy, newP);



            Cursor.Position = newP;
		}

		public static void Set(int x, int y)
		{
            Cursor.Position = new Point(x, y);
		}

		public static void Center(Rectangle r)
		{
            Cursor.Position = new Point(r.Left + r.Width/2, r.Top+r.Height/2);
		}

		public static void Center(Control control)
		{
			Center(control.RectangleToScreen(control.Bounds));
		}
	}
}
