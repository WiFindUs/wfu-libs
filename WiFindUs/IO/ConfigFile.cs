using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Globalization;

namespace WiFindUs
{
	public class ConfigFile
	{	
		protected static readonly Regex KEY
			= new Regex("[a-zA-Z0-9_\\-.]+", RegexOptions.Compiled);
		protected static readonly Regex COMMENT
			= new Regex(@"//.*$|/\*.*\*/", RegexOptions.Compiled);
		protected static readonly Regex COMMENT_START
			= new Regex(@"/\*.*$", RegexOptions.Compiled);
		protected static readonly Regex COMMENT_END
			= new Regex(@"^.*\*/", RegexOptions.Compiled);
		protected static readonly Regex KVP
			= new Regex("^(" + KEY.ToString() + ")(\\s*\\[\\s*0*([0-9]*)\\s*\\])?\\s*[:=]\\s*(.+)\\s*$");
		protected static readonly Regex EMPTY
			= new Regex(@"^\s*$", RegexOptions.Compiled);
		protected static readonly Regex TRUE
			= new Regex("^\\s*(1|TRUE|YES|ON)\\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		protected const String NUM = "[-+]?[0-9]+(?:[.][0-9]*)?";
		protected const String NUM_PERCENT = NUM + "[%]";
		protected static readonly Regex COLOUR_RGBA
			= new Regex("rgba?\\(\\s*(" + NUM_PERCENT + "?)\\s*,\\s*(" + NUM_PERCENT + "?)\\s*,\\s*(" + NUM_PERCENT + "?)\\s*(?:,\\s*(" + NUM_PERCENT + "?)\\s*)?\\)",
				RegexOptions.Compiled | RegexOptions.IgnoreCase);
		protected static readonly Regex COLOUR_HSLA
			= new Regex("hsla?\\(\\s*(" + NUM + ")\\s*,\\s*(" + NUM_PERCENT + ")\\s*,\\s*(" + NUM_PERCENT + ")\\s*(?:,\\s*(" + NUM_PERCENT + "?)\\s*)?\\)",
				RegexOptions.Compiled | RegexOptions.IgnoreCase);
		protected static readonly Regex COLOUR_HEX
			= new Regex("#([0-9A-F]{3}(?:[0-9A-F]{3}(?:[0-9A-F]{2})?)?)",
				RegexOptions.Compiled | RegexOptions.IgnoreCase);
		protected static readonly Regex COLOUR_KEYWORD
			= new Regex("black|silver|gray|white|maroon|red|purple|fuchsia|green|lime|olive|yellow|navy|blue|teal|aqua|transparent",
				RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private Dictionary<String, List<String>> kvps = new Dictionary<String, List<String>>();

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public List<String> this[String key]
        {
            get
            {
                List<String> values = kvps[CheckKey(key)];
                if (values == null)
                    kvps[key] = values = new List<String>();
                return values;
            }
            set
            {
                kvps[CheckKey(key)] = value ?? new List<String>();
            }
        }

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Instantiates an empty ConfigFile.
		/// </summary>
		public ConfigFile()
		{

		}

		/// <summary>
		/// Instantiates a ConfigFile using a string path.
		/// </summary>
		/// <param name="file">The string object representing the config file's path.</param>
		public ConfigFile(String file)
		{
			try
			{
				Read(file);
			}
			catch (Exception e)
			{
				Debugger.E("A " + e.GetType().FullName + " was thrown reading file "
					+ (file == null ? "'null'" : file) + "; it has been ignored.");
			}
		}

		/// <summary>
		/// Instantiates a ConfigFile using a list of string file paths.
		/// </summary>
		/// <param name="files">The iterable list of file paths to load. Any that are not found or throw an exception will be skipped.</param>
		public ConfigFile(IEnumerable<String> files)
		{
			Read(files);
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public void Read(String file)
		{
			if (file == null)
				throw new ArgumentNullException("file");
			if (!File.Exists(file))
				throw new FileNotFoundException(file + " does not exist.");
		
			try
			{
				readFile(file);
			}
			catch (IOException e)
			{
				Debugger.Ex(e);
			}
		}

		public void Read(IEnumerable<String> files)
		{
			if (files == null)
				throw new ArgumentNullException("files");
		
			foreach (String file in files)
			{
				try
				{
					Read(file);
				}
				catch (Exception e)
				{
					Debugger.E("A " + e.GetType().FullName + " was thrown reading file "
						+ (file == null ? "'null'" : file) + "; it has been ignored.");
				}
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("ConfigFile[");
			if (kvps.Count > 0)
			{
				sb.Append("\n");
				foreach (KeyValuePair<String, List<String>> kvp in kvps)
				{
					if (kvp.Key == null || kvp.Value == null || kvp.Value.Count == 0)
						continue;
					for (int i = 0; i < kvp.Value.Count; i++)
					{
						String value = kvp.Value[i];
						if (value == null || value.Length == 0)
							continue;
						sb.Append("    " + kvp.Key + (kvp.Value.Count > 0 ? "[" + i + "]" : "") + ": " + value + "\n");
					}
				}
			}
			else
				sb.Append(" ");
			sb.Append("]");
			return sb.ToString();
		}

		/////////////////////////////////////////////////////////////////////
		// GETTERS
		/////////////////////////////////////////////////////////////////////

		public int Get(String key, int index, int defaultValue)
		{
			int result;
			if (Int32.TryParse(GetValue(key, index), out result))
				return result;
			return defaultValue;
		}

		public int Get(String key, int defaultValue)
		{
			return Get(key, 0, defaultValue);
		}

		public float Get(String key, int index, float defaultValue)
		{
			float result;
			if (Single.TryParse(GetValue(key, index), out result))
				return result;
			return defaultValue;
		}

		public float Get(String key, float defaultValue)
		{
			return Get(key, 0, defaultValue);
		}

		public double Get(String key, int index, double defaultValue)
		{
			double result;
			if (Double.TryParse(GetValue(key, index), out result))
				return result;
			return defaultValue;
		}

		public double Get(String key, double defaultValue)
		{
			return Get(key, 0, defaultValue);
		}

		public bool Get(String key, int index, bool defaultValue)
		{
			if (TRUE.IsMatch(GetValue(key, index)))
				return true;
			return false;
		}

		public bool Get(String key, bool defaultValue)
		{
			return Get(key, 0, defaultValue);
		}

		public String Get(String key, int index, String defaultValue)
		{
			String result = GetValue(key, index);
			if (result.Length == 0)
				return defaultValue;
			return result;
		}

		public String Get(String key, String defaultValue)
		{
			return Get(key, 0, defaultValue);
		}

		public Color Get(String key, int index, Color defaultValue)
		{
			String result = GetValue(key, index);
			if (result.Length == 0)
				return defaultValue;

			//rgba
			Match match = COLOUR_RGBA.Match(result);
			if (match.Success)
			{
				byte r = ColorByteFromRegex(match.Groups[1].Value);
				byte g = ColorByteFromRegex(match.Groups[2].Value);
				byte b = ColorByteFromRegex(match.Groups[3].Value);
				float a = 1.0f;
				if (match.Groups.Count >= 5 && match.Groups[4].Value.Length > 0)
					a = PercentageFloatFromRegex(match.Groups[4].Value);
				return Color.FromArgb(Convert.ToInt32(a * 255.0f), r, g, b);
			}

			//hsla
			match = COLOUR_HSLA.Match(result);
			if (match.Success)
			{
				float h = (((Single.Parse(match.Groups[1].Value) % 360.0f) + 360.0f) % 360.0f) / 360.0f;
				float s = PercentageFloatFromRegex(match.Groups[2].Value);
				float l = PercentageFloatFromRegex(match.Groups[3].Value);
				float a = 1.0f;
				if (match.Groups.Count >= 5 && match.Groups[4].Value.Length > 0)
					a = PercentageFloatFromRegex(match.Groups[4].Value);

				float v, r, g, b;
				r = l;   // default to gray
				g = l;
				b = l;
				v = (l <= 0.5f) ? (l * (1.0f + s)) : (l + s - l * s);

				if (v > 0)
				{
					float m;
					float sv;
					int sextant;
					float fract, vsf, mid1, mid2;

					m = l + l - v;
					sv = (v - m) / v;
					h *= 6.0f;
					sextant = (int)h;
					fract = h - sextant;
					vsf = v * sv * fract;
					mid1 = m + vsf;
					mid2 = v - vsf;
					switch (sextant)
					{
						case 0:
							r = v;
							g = mid1;
							b = m;
							break;
						case 1:
							r = mid2;
							g = v;
							b = m;
							break;
						case 2:
							r = m;
							g = v;
							b = mid1;
							break;
						case 3:
							r = m;
							g = mid2;
							b = v;
							break;
						case 4:
							r = mid1;
							g = m;
							b = v;
							break;
						case 5:
							r = v;
							g = m;
							b = mid2;
							break;
					}
				}
				return Color.FromArgb(Convert.ToInt32(a * 255.0f), Convert.ToInt32(r * 255.0f), Convert.ToInt32(g * 255.0f), Convert.ToInt32(b * 255.0f));
			}

			//hex
			match = COLOUR_HEX.Match(result);
			if (match.Success)
			{
				string col = match.Groups[1].Value;
				if (col.Length == 3)
					col = col.Substring(0, 1) + col.Substring(0, 1) + col.Substring(1, 1) + col.Substring(1, 1) + col.Substring(2, 1) + col.Substring(2, 1);
				if (col.Length == 6)
					col += "FF";

				byte r = Byte.Parse(col.Substring(0, 2), NumberStyles.HexNumber);
				byte g = Byte.Parse(col.Substring(2, 2), NumberStyles.HexNumber);
				byte b = Byte.Parse(col.Substring(4, 2), NumberStyles.HexNumber);
				byte a = Byte.Parse(col.Substring(6, 2), NumberStyles.HexNumber);
				return Color.FromArgb(a, r, g, b);
			}

			//keywords
			match = COLOUR_KEYWORD.Match(result);
			if (match.Success)
			{
				byte a = 255;
				byte r = 255;
				byte g = 255;
				byte b = 255;

				//see http://www.w3.org/TR/css3-color/#colorunits for values
				switch (match.Value)
				{
					case "black": r = 0; g = 0; b = 0; break;
					case "silver": r = 192; g = 192; b = 192; break;
					case "gray": r = 128; g = 128; b = 128; break;
					//case "white": ... break; //white by default
					case "maroon": r = 128; g = 0; b = 0; break;
					case "red": r = 255; g = 0; b = 0; break;
					case "purple": r = 128; g = 0; b = 128; break;
					case "fuchsia": r = 255; g = 0; b = 255; break;
					case "green": r = 0; g = 128; b = 0; break;
					case "lime": r = 0; g = 255; b = 0; break;
					case "olive": r = 128; g = 128; b = 0; break;
					case "yellow": r = 255; g = 255; b = 0; break;
					case "navy": r = 0; g = 0; b = 128; break;
					case "blue": r = 0; g = 0; b = 255; break;
					case "teal": r = 0; g = 128; b = 128; break;
					case "aqua": r = 0; g = 255; b = 255; break;
					case "transparent": r = 0; g = 0; b = 0; a = 0; break;
				}
				return Color.FromArgb(a, r, g, b);
			}

			return defaultValue;
		}

		public Color Get(String key, Color defaultValue)
		{
			return Get(key, 0, defaultValue);
		}

		/////////////////////////////////////////////////////////////////////
		// SETTERS
		/////////////////////////////////////////////////////////////////////

		public ConfigFile Set(String key, int index, int value)
		{
			SetValue(key, index, value.ToString());
			return this;
		}

		public ConfigFile Set(String key, int value)
		{
			return Set(key, 0, value);
		}

		public ConfigFile Set(String key, int index, float value)
		{
			SetValue(key, index, value.ToString());
			return this;
		}

		public ConfigFile Set(String key, float value)
		{
			return Set(key, 0, value);
		}

		public ConfigFile Set(String key, int index, double value)
		{
			SetValue(key, index, value.ToString());
			return this;
		}

		public ConfigFile Set(String key, double value)
		{
			return Set(key, 0, value);
		}

		public ConfigFile Set(String key, int index, bool value)
		{
			SetValue(key, index, value.ToString());
			return this;
		}

		public ConfigFile Set(String key, bool value)
		{
			return Set(key, 0, value);
		}

		public ConfigFile Set(String key, int index, String value)
		{
			SetValue(key, index, value);
			return this;
		}

		public ConfigFile Set(String key, String value)
		{
			return Set(key, 0, value);
		}

		public ConfigFile Set(String key, int index, Color value)
		{
			SetValue(key, index, value == null ? "#FFFFFFFF"
				: "#" + value.R.ToString("X2") + value.G.ToString("X2") + value.B.ToString("X2") + value.A.ToString("X2"));
			return this;
		}

		public ConfigFile Set(String key, Color value)
		{
			return Set(key, 0, value);
		}

		/////////////////////////////////////////////////////////////////////
		// DEFAULTERS
		/////////////////////////////////////////////////////////////////////

		public ConfigFile Default(String key, int index, int defaultValue, int min, int max)
		{
			return Set(key, index, Math.Min(Math.Max(Get(key, defaultValue), min), max));
		}

		public ConfigFile Default(String key, int index, int defaultValue)
		{
			return Default(key, index, defaultValue, Int32.MinValue, Int32.MaxValue);
		}

		public ConfigFile Default(String key, int defaultValue)
		{
			return Default(key, 0, defaultValue);
		}

		public ConfigFile Default(String key, int index, float defaultValue, float min, float max)
		{
			return Set(key, index, Math.Min(Math.Max(Get(key, defaultValue), min), max));
		}

		public ConfigFile Default(String key, int index, float defaultValue)
		{
			return Default(key, index, defaultValue, Single.MinValue, Single.MaxValue);
		}

		public ConfigFile Default(String key, float defaultValue)
		{
			return Default(key, 0, defaultValue);
		}

		public ConfigFile Default(String key, int index, double defaultValue, double min, double max)
		{
			return Set(key, index, Math.Min(Math.Max(Get(key, defaultValue), min), max));
		}

		public ConfigFile Default(String key, int index, double defaultValue)
		{
			return Default(key, index, defaultValue, Double.MinValue, Double.MaxValue);
		}

		public ConfigFile Default(String key, double defaultValue)
		{
			return Default(key, 0, defaultValue);
		}

		public ConfigFile Default(String key, int index, String defaultValue)
		{
			return Set(key, index, Get(key, defaultValue));
		}

		public ConfigFile Default(String key, String defaultValue)
		{
			return Default(key, 0, defaultValue);
		}

		public ConfigFile Default(String key, int index, bool defaultValue)
		{
			return Set(key, index, Get(key, defaultValue));
		}

		public ConfigFile Default(String key, bool defaultValue)
		{
			return Default(key, 0, defaultValue);
		}

		public ConfigFile Default(String key, int index, Color defaultValue)
		{
			return Set(key, index, Get(key, defaultValue));
		}

		public ConfigFile Default(String key, Color defaultValue)
		{
			return Default(key, 0, defaultValue);
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		public void readFile(String file)
		{
			string[] lines = System.IO.File.ReadAllLines(file);

			bool inComment = false;
			List<String> validLines = new List<String>();
			for (int i = 0; i < lines.Length; i++)
			{
				//handle termination of comment blocks
				if (inComment)
				{
					if (COMMENT_END.IsMatch(lines[i]))
					{
						lines[i] = COMMENT_END.Replace(lines[i], "");
						inComment = false;
					}
					else
						continue;
				}

				//delete inline comments (// and /* ... */ on one line)
				lines[i] = COMMENT.Replace(lines[i], "");

				//handle starting of comment blocks
				if (COMMENT_START.IsMatch(lines[i]))
				{
					lines[i] = COMMENT_START.Replace(lines[i], "");
					inComment = true;
				}

				//if after processing comments the line still has content, add it
				//(trim whitespace)
				if (!EMPTY.IsMatch(lines[i]))
					validLines.Add(lines[i].Trim());
			}

			//now parse the valid lines for kvps
			foreach (String line in validLines)
			{
				Match match = KVP.Match(line);
				if (!match.Success)
					continue;

				//determine if this was passed with an array index
				int index = 0;
				if (match.Groups[2].Value != null && match.Groups[2].Value.Length > 0)
					index = match.Groups[3].Value != null && match.Groups[3].Value.Length > 0 ? Int32.Parse(match.Groups[3].Value) : -1;

				//test for an explicit string
				String value = match.Groups[4].Value ?? "";
				if (value.Length >= 2 &&
					((value[0] == '\'' && value[value.Length - 1] == '\'')
					|| (value[0] == '"' && value[value.Length - 1] == '"')))
				{
					value = value.Substring(1,value.Length-2)
							.Replace("\\n", "\n")
							.Replace("\\t", "\t");
				}

				//set value
				SetValue(match.Groups[1].Value, index, value);
			}
		}

		private static String CheckKey(String key)
		{
			if (key == null)
				throw new ArgumentNullException("key");
			key = key.Trim().ToLower();
			if (key.Length == 0)
				throw new ArgumentException("Parameter 'key' cannot be an empty string.");
			else if (!KEY.IsMatch(key))
				throw new ArgumentException("Parameter 'key' contains invalid characters.");
			return key;
		}

		private void SetValue(String key, int index, String value)
		{
			//get key, value and value list
			List<String> values = this[key];
			value = value ?? "";
			index = index < 0 ? values.Count : index;

			//add value, resize array if necessary
			if (index >= values.Count)
			{
				if (values.Capacity <= index)
					values.Capacity = (int)(index * 1.5);
				int count = values.Count;
				for (int i = count; i < index; i++)
					values.Add("");
				values.Add(value);
			}
			else
			{
				values[index] = value;
			}
		}

		private String GetValue(String key, int index)
		{
			if (index < 0)
				return "";
			List<String> values = this[key];
			if (index >= values.Count)
				return "";
			return values[index];
		}

		private static byte ColorByteFromRegex(string input)
		{
			if (input.EndsWith("%"))
			{
				input = input.Substring(0, input.Length - 1);
				return Convert.ToByte(Convert.ToInt32(2.55f * Single.Parse(input)) % 256);
			}

			return Convert.ToByte(Int32.Parse(input) % 256);
		}

		private static float PercentageFloatFromRegex(string input)
		{
			if (input.EndsWith("%"))
			{
				input = input.Substring(0, input.Length - 1);
				return 0.01f * Single.Parse(input);
			}
			return Single.Parse(input);
		}
	}
}
