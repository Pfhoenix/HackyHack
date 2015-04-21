using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Platform.Android;
using Android.Graphics;
using Android.Views;
using Android.Content;
using Android.Util;
using RPCoreLib;

namespace HackyHack
{
	public class GLView1 : RPGameView
	{
		public GLView1(Context context) : base (context)
		{
			RPGlobals.g.AppName = "HackyHack";
		}

		protected override void CreateUIRoot()
		{
			UIManager.ui.Root = new UIHackyRoot();
		}

		protected override void LoadInitialContent()
		{
			base.LoadInitialContent();

			ContentManager.cm.LoadStockFont(Typeface.Monospace, "mono-large", 40);

			Texture t = ContentManager.cm.LoadResourceToTexture(Resource.Drawable.window_map, "window_bg", null);
			t.SetUVsByCoords(0, 0, 74, 72);
			t = ContentManager.cm.LoadResourceToTexture(Resource.Drawable.window_map, "window_ul_corner", null);
			t.SetUVsByCoords(0, 73, 42, 22);
			t.Width = 61;
			t.Height = 32;
			t = ContentManager.cm.LoadResourceToTexture(Resource.Drawable.window_map, "window_title_bg", null);
			t.SetUVsByCoords(66, 73, 1, 22);
			t.Height = 32;
			t = ContentManager.cm.LoadResourceToTexture(Resource.Drawable.window_map, "window_ll_corner", null);
			t.SetUVsByCoords(0, 96, 22, 22);
			t.Width = 32;
			t.Height = 32;
			t = ContentManager.cm.LoadResourceToTexture(Resource.Drawable.window_map, "window_lr_corner", null);
			t.SetUVsByCoords(23, 96, 22, 22);
			t.Width = 32;
			t.Height = 32;
			t = ContentManager.cm.LoadResourceToTexture(Resource.Drawable.window_map, "window_ur_corner", null);
			t.SetUVsByCoords(43, 73, 22, 22);
			t.Width = 32;
			t.Height = 32;
		}
	}
}

