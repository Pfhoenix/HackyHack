using System;
using Android.Graphics;

namespace HackyHack
{
	public class Font
	{
		public readonly static char CHAR_START = (char)32;
		public readonly static char CHAR_END = (char)126;
		public readonly static char CHAR_NUM = (char)(CHAR_END - CHAR_START + 2);
		public readonly static char CHAR_NONE = (char)32;
		public readonly static char CHAR_UNKNOWN = (char)(CHAR_NUM - 1);

		public readonly static int FONT_SIZE_MIN = 6;
		public readonly static int FONT_SIZ_MAX = 180;

		//public readonly static int CHAR_BATCH_SIZE = 100;

		//----

		public string Name;
		public int Size;

		public int PadX, PadY;
		public float Ascent;
		public float Descent;
		public Texture[] Characters;
		public float CharHeight;


		Font()
		{
			Characters = new Texture[CHAR_NUM];
		}

		public static Font Load(string name, int size, int padX, int padY)
		{
			Typeface tf = Typeface.CreateFromAsset(Globals.g.GameContext.Assets, name + ".ttf");
			Font f = Load(tf, name, size, padX, padY);
			return f;
		}

		public static Font Load(Typeface tf, string name, int size, int padX, int padY)
		{
			Font f = new Font();
			f.Name = name;
			f.Size = size;
			f.PadX = padX;
			f.PadY = padY;
			float[] CharWidths = new float[CHAR_NUM];
			int CellWidth, CellHeight;
			float CharWidthMax = 0;

			Paint p = new Paint();
			p.AntiAlias = true;
			p.TextSize = size;
			p.Color = Color.White;
			p.SetTypeface(tf);

			Paint.FontMetrics fm = p.GetFontMetrics();
			f.CharHeight = (float)Math.Ceiling(Math.Abs(fm.Bottom) + Math.Abs(fm.Top));
			f.Ascent = (float)Math.Ceiling(Math.Abs(fm.Ascent));
			f.Descent = (float)Math.Ceiling(Math.Abs(fm.Descent));

			// determine the width of each character (including unknown character) and the maximum character widthh
			char[] s = new char[2];
			float[] w = { 0f, 0f };
			int i = 0;
			for (char c = CHAR_START; c <= CHAR_END; c++, i++)
			{
				s[0] = c;
				p.GetTextWidths(s, 0, 1, w);
				CharWidths[i] = w[0];
				if (w[0] > CharWidthMax) CharWidthMax = w[0];
			}
			s[0] = CHAR_NONE;
			p.GetTextWidths(s, 0, 1, w);
			CharWidths[i] = w[0];
			if (w[0] > CharWidthMax) CharWidthMax = w[0];

			// find the maximum size, validate, and setup cell sizes
			CellWidth = (int)CharWidthMax + (2 * f.PadX);
			CellHeight = (int)f.CharHeight + (2 * f.PadY);
			int max = CellWidth > CellHeight ? CellWidth : CellHeight;
			if ((max < FONT_SIZE_MIN) || (max > FONT_SIZ_MAX)) return null;

			// set texture size based on max font size (width or height)
			// NOTE: these values are fixed, based on the defined characters
			// when changing start/end characters, this will need adjustment too
			int texsize;
			if (max <= 24) texsize = 256;
			else if (max <= 40) texsize = 512;
			else if (max <= 80) texsize = 1024;
			else texsize = 2048;

			// create an empty bitmap (alpha only)
			Bitmap bitmap = Bitmap.CreateBitmap(texsize, texsize, Bitmap.Config.Argb8888);
			Canvas canvas = new Canvas(bitmap);
			bitmap.EraseColor(0);

			// render each of the characters to the canvas
			float x = f.PadX;
			float y = CellHeight - 1 - f.Descent - f.PadY;
			for (char c = CHAR_START; c <= CHAR_END; c++)
			{
				s[0] = c;
				canvas.DrawText(s, 0, 1, x, y, p);
				x += CellWidth;
				if ((x + CellWidth - f.PadX) > texsize)
				{
					x = f.PadX;
					y += CellHeight;
				}
			}
			s[0] = CHAR_NONE;
			canvas.DrawText(s, 0, 1, x, y, p);

			// create the OpenGL texture
			TextureInfo ti = ContentManager.cm.LoadBitmapToTextureInfo(bitmap, true);
			bitmap.Recycle();

			// generate a Texture for each character
			f.Characters = new Texture[CHAR_NUM];
			Texture t;
			float ch = ((float)CellHeight - 1 + f.PadY) / texsize;
			x = 0;
			y = 0;
			for (int c = 0; c < CHAR_NUM; c++)
			{
				t = new Texture(f.Name + "_" + c, ti, null);
				t.UVs[0] = t.UVs[4] = x / texsize;
				t.UVs[1] = t.UVs[3] = y / texsize;
				t.UVs[2] = t.UVs[6] = t.UVs[0] + (CharWidths[c] - 1 + f.PadX) / texsize;
				t.UVs[5] = t.UVs[7] = t.UVs[1] + ch;
				t.Width = (int)CharWidths[c];
				t.Height = CellHeight;

				f.Characters[c] = t;

				x += CellWidth;
				if (x + CellWidth > texsize)
				{
					x = 0;
					y += CellHeight;
				}
			}

			return f;
		}

		public void MeasureChar(char c, Vector2 v)
		{
			c -= CHAR_START;
			if ((c < 0) || (c > CHAR_NUM)) c = CHAR_UNKNOWN;
			v.X = Characters[c].Width;
			v.Y = CharHeight;
		}

		public Vector2 MeasureChar(char c)
		{
			Vector2 v = new Vector2();
			MeasureChar(c, v);
			return v;
		}

		public Vector2 MeasureText(char[] chars)
		{
			Vector2 v = new Vector2();
			if (chars == null) return v;
			if (chars.Length == 0) return v;
			char c;
			for (int i = 0; i < chars.Length; i++)
			{
				c = (char)(chars[i] - CHAR_START);
				if ((c < 0) || (c > CHAR_NUM)) c = CHAR_UNKNOWN;
				v.X += Characters[c].Width;
			}
			v.Y = CharHeight;
			return v;
		}

		public Vector2 MeasureText(string s)
		{
			Vector2 v = new Vector2();
			if (string.IsNullOrEmpty(s)) return v;
			char c;
			for (int i = 0; i < s.Length; i++)
			{
				c = (char)(s[i] - CHAR_START);
				if ((c < 0) || (c > CHAR_NUM)) c = CHAR_UNKNOWN;
				v.X += Characters[c].Width;
			}
			v.Y = CharHeight;
			return v;
		}

		public string GetTextForLimit(string s, float limit, out float ltl)
		{
			ltl = 0;
			string t = "";
			if (string.IsNullOrEmpty(s)) return "";
			char c;
			for (int i = 0; i < s.Length; i++)
			{
				c = (char)(s[i] - CHAR_START);
				if ((c < 0) || (c > CHAR_NUM)) c = CHAR_UNKNOWN;
				if (ltl + Characters[c].Width > limit) break;
				t += (char)(c + CHAR_START);
				ltl += Characters[c].Width;
			}

			return t;
		}
	}
}

