using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES11;
using Android.Graphics;
using Android.Opengl;
using Android.Util;

namespace HackyHack
{
	public sealed class ContentManager
	{
		readonly static ContentManager _cm = new ContentManager();
		public static ContentManager cm
		{
			get { return _cm; }
		}

		Dictionary<int, TextureInfo> ResourceToNativeTex;
		List<Texture> Textures;
		List<Font> Fonts;
		BitmapFactory.Options BFO;



		ContentManager()
		{
			ResourceToNativeTex = new Dictionary<int, TextureInfo>(50);
			Textures = new List<Texture>();
			Fonts = new List<Font>();

			BFO = new BitmapFactory.Options();
			BFO.InDither = false;
			BFO.InScaled = false;
		}

		#region TEXTURE LOADING
		public Texture LoadResourceToTexture(int rid, string name, float[] uvs)
		{
			TextureInfo ti;
			Texture t;
			ResourceToNativeTex.TryGetValue(rid, out ti);

			if (ti == null)
			{
				uint tid;
				GL.GenTextures(1, out tid);
				if (tid == 0)
				{
					// need to log that there was an error creating a new texture
					Log.Verbose(Globals.g.AppName, "Unable to create a new OpenGL texture!");
					return null;
				}
				Bitmap b = null;
				try
				{
					b = BitmapFactory.DecodeResource(Globals.g.GameContext.Resources, rid, BFO);

					ti = LoadBitmapToTextureInfo(b, false);
					ResourceToNativeTex[rid] = ti;

					t = new Texture(name, ti, uvs);
					Textures.Add(t);
					
					b.Recycle();

					return t;
				}
				catch (Exception e)
				{
					if (b != null) b.Recycle();
					Log.Verbose(Globals.g.AppName, e.Message + "\n" + e.StackTrace);
					return null;
				}
			}

			t = new Texture(name, ti, uvs);
			Textures.Add(t);

			return t;
		}

		public TextureInfo LoadBitmapToTextureInfo(Bitmap b, bool bSmooth)
		{
			uint tid;
			GL.GenTextures(1, out tid);
			if (tid == 0)
			{
				// need to log that there was an error creating a new texture
				Log.Verbose(Globals.g.AppName, "Unable to create a new OpenGL texture!");
				return null;
			}
			GL.BindTexture(All.Texture2D, tid);
			GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Nearest);
			GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Linear);//(bSmooth ? All.Linear : All.Nearest));
			GL.TexParameter(All.Texture2D, All.TextureWrapS, (int)All.ClampToEdge);
			GL.TexParameter(All.Texture2D, All.TextureWrapT, (int)All.ClampToEdge);
			GLUtils.TexImage2D((int)All.Texture2D, 0, b, 0);

			return new TextureInfo(tid, b.Width, b.Height);
		}
		
		public Texture FindTexture(string name)
		{
			return Textures.Find(tx => tx.Name == name);
		}
		#endregion

		#region FONT LOADING
		public void LoadStockFont(Typeface tf, string name, int size)
		{
			Font f = Font.Load(tf, name, size, 2, 0);
			Fonts.Add(f);
		}

		public void LoadAssetFont(string name, int size)
		{
			Font f = Font.Load(name, size, 2, 0);
			Fonts.Add(f);
		}

		public Font GetFont(string name)
		{
			return Fonts.Find(ff => (ff.Name == name));
		}
		#endregion
	}
}

