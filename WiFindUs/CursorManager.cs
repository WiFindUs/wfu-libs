using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiFindUs
{
	public static class CursorManager
	{
		private static bool hidden = false;

		public static void Hide()
		{
			if (hidden)
				return;
			System.Windows.Forms.Cursor.Hide();
			hidden = true;
		}

		public static void Show()
		{
			if (!hidden)
				return;
			System.Windows.Forms.Cursor.Show();
			hidden = false;
		}
	}
}
