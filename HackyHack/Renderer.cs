using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES11;
using Android.Graphics;

namespace HackyHack
{
	public sealed class Renderer
	{
		readonly static Renderer _r = new Renderer();
		public static Renderer r
		{
			get { return _r; }
		}

		public Rect ScreenRect;

		readonly float[] sprite_verts = {
			-0.5f, -0.5f,
			0.5f, -0.5f,
			-0.5f, 0.5f, 
			0.5f, 0.5f
		};

		readonly float[] ui_verts = {
			0, 0,
			1, 0,
			0, 1,
			1, 1
		};

		readonly float[] line_verts = {
			0, 0,
			1, 1
		};
		
		public readonly float ZFar = 1f;
		public readonly float ZNear = 0f;
		public readonly byte[] ColorWhite = { 255, 255, 255 };
		public Texture White;

		Renderer()
		{
			ScreenRect = new Rect();
		}

		public void InitializeViewport()
		{
			// prepare our viewport and projection matrix
			Globals.g.AGView.GetDrawingRect(Renderer.r.ScreenRect);
			GL.Viewport(ScreenRect.Left, ScreenRect.Top, ScreenRect.Right, ScreenRect.Bottom);
			GL.MatrixMode(All.Projection);
			GL.LoadIdentity();
			GL.Ortho(ScreenRect.Left, ScreenRect.Right, ScreenRect.Bottom, ScreenRect.Top, ZNear, ZFar);

			GL.Disable(All.Lighting);
			GL.Enable(All.Texture2D);
			GL.Enable(All.AlphaTest);
			GL.AlphaFunc(All.Greater, 0);
			GL.Enable(All.DepthTest);
			GL.Enable(All.DepthWritemask);
			GL.DepthFunc(All.Less);
			GL.ClearColor(0, 0, 0, 1f);
			GL.ClearDepth(ZFar);
		}

		public void InitFrame()
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.EnableClientState(All.VertexArray);
			GL.EnableClientState(All.TextureCoordArray);
			GL.MatrixMode(All.Modelview);
			GL.ActiveTexture(All.Texture0);
		}

		public void EnableScissor()
		{
			GL.Enable(All.ScissorTest);
		}

		public void SetScissor(int x, int y, int w, int h)
		{
			GL.Scissor(x, y, w, h);
		}

		public void DisableScissor()
		{
			GL.Disable(All.ScissorTest);
		}

		// this assumes that we're already set to GL.MatrixMode(All.Modelview);
		public void DrawSprite(Texture t, float x, float y, float sx, float sy, float rot)
		{
			GL.LoadIdentity();
			GL.Translate((float)Math.Floor(x), (float)Math.Floor(y), 0);
			GL.Scale(sx * t.Width, sy * t.Height, 1);
			GL.Rotate(rot, 0, 0, -1);

			GL.BindTexture(All.Texture2D, t.NativeInfo.TexID);

			GL.VertexPointer(2, All.Float, 0, sprite_verts);
			GL.TexCoordPointer(2, All.Float, 0, t.UVs);
			GL.DrawArrays(All.TriangleStrip, 0, 4);
		}

		public void DrawUI(Texture t, float x, float y, float sx, float sy, float rot)
		{
			GL.LoadIdentity();
			GL.Translate((float)Math.Floor(x), (float)Math.Floor(y), 0);
			GL.Scale(sx * t.Width, sy * t.Height, 1);
			GL.Rotate(rot, 0, 0, -1);

			GL.BindTexture(All.Texture2D, t.NativeInfo.TexID);

			GL.VertexPointer(2, All.Float, 0, ui_verts);
			GL.TexCoordPointer(2, All.Float, 0, t.UVs);
			GL.DrawArrays(All.TriangleStrip, 0, 4);
		}

		public void DrawTest(uint tid, float x, float y, float w, float h, float rot)
		{
			GL.LoadIdentity();
			GL.Translate((float)Math.Floor(x), (float)Math.Floor(y), 0);
			GL.Scale(w, h, 1);
			GL.Rotate(rot, 0, 0, -1);

			GL.BindTexture(All.Texture2D, tid);

			GL.VertexPointer(2, All.Float, 0, ui_verts);
			GL.TexCoordPointer(2, All.Float, 0, ui_verts);
			GL.DrawArrays(All.TriangleStrip, 0, 4);
		}

		public void DrawLine(float sx, float sy, float ex, float ey, float width, Color color)
		{
			GL.LoadIdentity();

			line_verts[0] = (float)Math.Floor(sx);
			line_verts[1] = (float)Math.Floor(sy);
			line_verts[2] = (float)Math.Floor(ex);
			line_verts[3] = (float)Math.Floor(ey);

			GL.Color4(color.R, color.G, color.B, color.A);
			GL.LineWidth(width);

			GL.BindTexture(All.Texture2D, White.NativeInfo.TexID);
			GL.VertexPointer(2, All.Float, 0, line_verts);
			GL.TexCoordPointer(2, All.Float, 0, ui_verts);
			GL.DrawArrays(All.Lines, 0, 2);
		}

		public float DrawText(string s, Font f, float x, float y, float mx = 0f)
		{
			if (string.IsNullOrEmpty(s)) return 0;

			GL.BindTexture(All.Texture2D, f.Characters[0].NativeInfo.TexID);
			GL.VertexPointer(2, All.Float, 0, ui_verts);

			int c;
			float startx = x;
			for (int i = 0; i < s.Length; i++)
			{
				if ((mx > 0) && (x >= mx)) break;

				c = s[i] - Font.CHAR_START;
				if ((c < 0) || (c > Font.CHAR_NUM)) c = Font.CHAR_UNKNOWN;
				GL.LoadIdentity();
				GL.Translate((float)Math.Floor(x), (float)Math.Floor(y), 0);
				GL.Scale(f.Characters[c].Width, f.Characters[c].Height, 1);
				GL.TexCoordPointer(2, All.Float, 0, f.Characters[c].UVs);
				GL.DrawArrays(All.TriangleStrip, 0, 4);

				x += f.Characters[c].Width;
			}

			return x - startx;
		}

		public float DrawText(char[] chars, Font f, float x, float y)
		{
			if ((chars == null) || (chars.Length == 0)) return 0;

			GL.BindTexture(All.Texture2D, f.Characters[0].NativeInfo.TexID);
			GL.VertexPointer(2, All.Float, 0, ui_verts);

			int c;
			float startx = x;
			for (int i = 0; i < chars.Length; i++)
			{
				c = chars[i] - Font.CHAR_START;
				if ((c < 0) || (c > Font.CHAR_NUM)) c = Font.CHAR_UNKNOWN;
				GL.LoadIdentity();
				GL.Translate((float)Math.Floor(x), (float)Math.Floor(y), 0);
				GL.Scale(f.Characters[c].Width, f.Characters[c].Height, 1);
				GL.TexCoordPointer(2, All.Float, 0, f.Characters[c].UVs);
				GL.DrawArrays(All.TriangleStrip, 0, 4);

				x += f.Characters[c].Width;
			}

			return x - startx;
		}

		public float DrawText(List<char> chars, Font f, float x, float y)
		{
			if ((chars == null) || (chars.Count == 0)) return 0;

			GL.BindTexture(All.Texture2D, f.Characters[0].NativeInfo.TexID);
			GL.VertexPointer(2, All.Float, 0, ui_verts);

			int c;
			float startx = x;
			for (int i = 0; i < chars.Count; i++)
			{
				c = chars[i] - Font.CHAR_START;
				if ((c < 0) || (c > Font.CHAR_NUM)) c = Font.CHAR_UNKNOWN;
				GL.LoadIdentity();
				GL.Translate((float)Math.Floor(x), (float)Math.Floor(y), 0);
				GL.Scale(f.Characters[c].Width, f.Characters[c].Height, 1);
				GL.TexCoordPointer(2, All.Float, 0, f.Characters[c].UVs);
				GL.DrawArrays(All.TriangleStrip, 0, 4);

				x += f.Characters[c].Width;
			}

			return x - startx;
		}
	}
}

