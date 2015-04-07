using System;
using RPCoreLib;

namespace HackyHack
{
	public class UIHackyRoot : UIRoot
	{
		public UITaskBar Taskbar;
		public UIMenu TopMenu;

		public UIHackyRoot()
		{
			Taskbar = new UITaskBar();
			AddChild(Taskbar);
			Taskbar.ResetWithParent();

			TopMenu = new UIMenu(UIManager.ui.UIMediumTextFont);
			AddChild(TopMenu);
			TopMenu.TaskBar = Taskbar;
		}

		public override void ProcessScreenChanged()
		{
			Bounds.X = Renderer.r.ScreenRect.Right;
			Bounds.Y = Renderer.r.ScreenRect.Bottom;

			base.ProcessScreenChanged();
		}

		public override bool ProcessInputEvent(EInputEvent ie, float x, float y, float px, float py)
		{
			if (base.ProcessInputEvent(ie, x, y, px, py)) return true;

			return false;
		}

		public void OpenWindow(UIWindow uiw)
		{
			AddChild(uiw);
			uiw.Open();
			Taskbar.AddWindow(uiw);
		}

		public void CreateTestWindow()
		{
			// spawn a UIWindow
			UIWindow uiw = new UIWindow();
			uiw.Resize(400, 300);
			OpenWindow(uiw);
			Random rng = new Random();
			uiw.MoveTo(rng.Next(50, 400), rng.Next(50, 400));
		}

		public override void InitMainMenu()
		{
			TopMenu.RootItem = new UIMenuItem("System", TopMenu);
			TopMenu.RootItem.AddSubItem("Test1", null);
			UIMenuItem mi = TopMenu.RootItem.AddSubItem("Test2", null);
			TopMenu.RootItem.AddSubItem("blah blah blah", CreateTestWindow);
			TopMenu.RootItem.AddSubItem("Shutdown", RPGlobals.g.GameView.CloseApp);

			UIMenuItem mi2 = null;
			for (int i = 0; i < 30; i++)
			{
				mi2 = mi.AddSubItem("SubTest" + i, null);
			}

			for (int i = 1; i < 11; i++)
			{
				mi2.AddSubItem("SubSubTest" + i, null);
			}
		}
	}
}

