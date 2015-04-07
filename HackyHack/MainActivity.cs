using Android.App;
using Android.OS;
using Android.Content.PM;
using RPCoreLib;

namespace HackyHack
{
	// the ConfigurationChanges flags set here keep the EGL context
	// from being destroyed whenever the device is rotated or the
	// keyboard is shown (highly recommended for all GL apps)
	[Activity (Label = "HackyHack",
				ConfigurationChanges=ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,
				MainLauncher = true,
	           Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen")]
	public class MainActivity : Activity
	{
		GLView1 view;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			//this.RequestWindowFeature(WindowFeatures.NoTitle);

			// Create our OpenGL view, and display it
			RPGlobals.g.App = this;
			view = new GLView1(this);
			SetContentView(view);
		}

		protected override void OnPause()
		{
			// never forget to do this!
			base.OnPause();
			view.Pause();
		}

		protected override void OnResume()
		{
			// never forget to do this!
			base.OnResume();
			view.Resume();
		}

		protected override void OnDestroy()
		{
			if (view != null)
			{
				view.Dispose();
				view = null;
			}
			base.OnDestroy();
		}
	}
}


