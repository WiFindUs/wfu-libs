using System;
using System.Drawing;
using WiFindUs.Extensions;

namespace WiFindUs.Themes
{
	public class ThemeColourSet : IDisposable
	{
		public enum Tone
		{
			Lighter = 0,
			Light = 1,
			Mid = 2,
			Dark = 3,
			Darker = 4
		}		
		private Color baseColour;
		private ThemeColour[] colours = new ThemeColour[5];
		private bool disposed = false;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		/// <summary>
		/// The base colour of this colour set. All created colours will be tones of this colour.
		/// </summary>
		public Color BaseColour
		{
			get { return baseColour; }
			private set { baseColour = value; }
		}

		/// <summary>
		/// ThemeColour created as a much lighter tone of BaseColour.
		/// </summary>
		public ThemeColour Lighter
		{
			get { return GetColour(Tone.Lighter); }
		}

		/// <summary>
		/// ThemeColour created as a lighter tone of BaseColour.
		/// </summary>
		public ThemeColour Light
		{
			get { return GetColour(Tone.Light); }
		}

		/// <summary>
		/// ThemeColour created using BaseColour.
		/// </summary>
		public ThemeColour Mid
		{
			get { return GetColour(Tone.Mid); }
		}

		/// <summary>
		/// ThemeColour created as a darker tone of BaseColour.
		/// </summary>
		public ThemeColour Dark
		{
			get { return GetColour(Tone.Dark); }
		}

		/// <summary>
		/// ThemeColour created as a much darker tone of BaseColour.
		/// </summary>
		public ThemeColour Darker
		{
			get { return GetColour(Tone.Darker); }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public ThemeColourSet(Color color)
		{
			BaseColour = color;
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public ThemeColour GetColour(Tone tone)
		{
			uint index = (uint)tone;
			if (colours[index] == null)
			{
				float adjustment = 0.0f;
				switch (tone)
				{
					case Tone.Lighter: adjustment += 0.3f; break;
					case Tone.Light: adjustment += 0.1f; break;
					case Tone.Dark: adjustment -= 0.1f; break;
					case Tone.Darker: adjustment -= 0.3f; break;
				}
				colours[index] = new ThemeColour(baseColour.Adjust(adjustment));
			}
			return colours[index];
		}

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
				Destroy();

			disposed = true;
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void Destroy()
		{
			for (int i = 0; i < colours.Length; i++)
			{
				if (colours[i] != null)
				{
					colours[i].Dispose();
					colours[i] = null;
				}
			}
		}
	}
}
