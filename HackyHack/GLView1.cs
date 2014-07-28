using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Platform.Android;
using Android.Graphics;
using Android.Views;
using Android.Content;
using Android.Util;

namespace HackyHack
{
	public class GLView1 : AndroidGameView
	{
		SimpleGestureProcessor SGP;
		Stopwatch Watch;

		public GLView1(Context context) : base (context)
		{
			SGP = new SimpleGestureProcessor(context);
			Watch = new Stopwatch();
		}

		public void CloseApp()
		{
			Globals.g.bQuit = true;
		}

		// This gets called when the drawing surface is ready
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Globals.g.AppName = "HackyHack";
			Globals.g.AGView = this;
			Globals.g.GameContext = Context;

			Renderer.r.InitializeViewport();

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

			Renderer.r.White = ContentManager.cm.LoadResourceToTexture(Resource.Drawable.white_square, "white", null);

			ContentManager.cm.LoadResourceToTexture(Resource.Drawable.tb_left_on, "tb_left_on", null);
			ContentManager.cm.LoadResourceToTexture(Resource.Drawable.tb_left_off, "tb_left_off", null);
			ContentManager.cm.LoadResourceToTexture(Resource.Drawable.tb_right_on, "tb_right_on", null);
			ContentManager.cm.LoadResourceToTexture(Resource.Drawable.tb_right_off, "tb_right_off", null);
			ContentManager.cm.LoadResourceToTexture(Resource.Drawable.tb_bracket_left_up, "tb_bracket_left_up", null);
			ContentManager.cm.LoadResourceToTexture(Resource.Drawable.tb_bracket_left_down, "tb_bracket_left_down", null);
			ContentManager.cm.LoadResourceToTexture(Resource.Drawable.tb_bracket_right_up, "tb_bracket_right_up", null);
			ContentManager.cm.LoadResourceToTexture(Resource.Drawable.tb_bracket_right_down, "tb_bracket_right_down", null);

			ContentManager.cm.LoadStockFont(Typeface.Default, "default", 20);
			ContentManager.cm.LoadStockFont(Typeface.Monospace, "mono", 20);
			ContentManager.cm.LoadAssetFont("slider", 26);

			UIManager.ui.InitForScreen(Renderer.r.ScreenRect.Right, Renderer.r.ScreenRect.Bottom);
			SGP.HandleSingleTap += UIManager.ui.HandleSingleTap;
			SGP.HandleScrollStart += UIManager.ui.HandleScrollStart;
			SGP.HandleScrollChange += UIManager.ui.HandleScrollChange;
			SGP.HandleScaleStart += UIManager.ui.HandleScaleStart;
			SGP.HandleScaleChange += UIManager.ui.HandleScaleChange;
			SGP.HandleFling += UIManager.ui.HandleFling;
			SGP.HandleRelease += UIManager.ui.HandleRelease;

			// create the menu
			UIManager.ui.Root.InitMainMenu();

			// Run the render loop
			Watch.Start();
			Run();
		}

		// This method is called everytime the context needs
		// to be recreated. Use it to set any egl-specific settings
		// prior to context creation
		//
		// In this particular case, we demonstrate how to set
		// the graphics mode and fallback in case the device doesn't
		// support the defaults
		protected override void CreateFrameBuffer()
		{
			// the default GraphicsMode that is set consists of (16, 16, 0, 0, 2, false)
			try
			{
				Log.Verbose(Globals.g.AppName, "Loading with default settings");
				GraphicsMode = new AndroidGraphicsMode(16, 16, 8, 0, 2, false);

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer();
				return;
			}
			catch (Exception ex)
			{
				Log.Verbose(Globals.g.AppName, "{0}", ex);
			}

			// this is a graphics setting that sets everything to the lowest mode possible so
			// the device returns a reliable graphics setting.
			try
			{
				Log.Verbose("GLCube", "Loading with custom Android settings (low mode)");
				GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false);

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer();
				return;
			}
			catch (Exception ex)
			{
				Log.Verbose(Globals.g.AppName, "{0}", ex);
			}
			throw new Exception("Can't load egl, aborting");
		}

		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged(w, h, oldw, oldh);

			if (Renderer.r.White != null)
			{
				Renderer.r.InitializeViewport();
				UIManager.ui.ScreenChanged();
			}
		}

		// we use this to trap and custom process touch events
		public override bool OnTouchEvent(MotionEvent e)
		{
			if (!SGP.OnTouchEvent(e)) return base.OnTouchEvent(e);

			return true;
		}

		// we use this to trap and pipe to the UIManager
		public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
		{
			if (UIManager.ui.bTrapKeyInput)
			{
				UIManager.ui.HandleKeyInput(keyCode, e);
				return true;
			}
			else return base.OnKeyDown(keyCode, e);
		}

		// This gets called on each frame render by the underlying thread
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			float dt = Watch.ElapsedMilliseconds / 1000f;
			Watch.Restart();
			Globals.g.RunningTime += dt;

			// you only need to call this if you have delegates
			// registered that you want to have called
			//base.OnRenderFrame(e);

			// need to tell UIManager to process queued input first
			UIManager.ui.ProcessInputEvents();

			// run the game logic loop here

			// update the UI
			UIManager.ui.Update(dt);
			// prepare the renderer for this frame
			Renderer.r.InitFrame();
			// render the UI
			UIManager.ui.Render();

			// this finalizes rendering
			SwapBuffers();

			if (Globals.g.bQuit)
			{
				Globals.g.App.Finish();
			}
		}
	}
}

