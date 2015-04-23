using System;
using RPCoreLib;

namespace HackyHack
{
	public class UIHackyRoot : UIRoot
	{
		public UITaskBar Taskbar;
		public UIMenu TopMenu;

		public Vector2 MaxWindowSize = new Vector2();

		public UIHackyRoot()
		{
		}

		public override bool ProcessInputEvent(EInputEvent ie, float x, float y, float px, float py)
		{
			if (base.ProcessInputEvent(ie, x, y, px, py)) return true;

			//if (ie == EInputEvent.SingleTap) CreateTestWindow();

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
			Taskbar = new UITaskBar();
			AddChild(Taskbar);
			Taskbar.ResetWithParent();
			Taskbar.bVisible = false;

			TopMenu = new UIMenu(UIManager.ui.UIMediumTextFont);
			AddChild(TopMenu);
			TopMenu.TaskBar = Taskbar;
			TopMenu.bVisible = false;

			TopMenu.RootItem = new UIMenuItem("System", TopMenu);
			UIMenuItem ht = TopMenu.RootItem.AddSubItem("Hack Tests", null);
			ht.AddSubItem("Firewall", TestFirewallHack);
			TopMenu.RootItem.AddSubItem("Create Test Window", CreateTestWindow);
			TopMenu.RootItem.AddSubItem("Shutdown", RPGlobals.g.GameView.CloseApp);

			ProcessScreenChanged();

			MaxWindowSize.X = Bounds.X - Taskbar.Bounds.Y - TopMenu.Bounds.Y;
			MaxWindowSize.Y = Bounds.Y;
		}

		public override void OpenMainMenu()
		{
			// open main menu here
			// for now, make visible the menu and taskbar
			TopMenu.bVisible = true;
			Taskbar.bVisible = true;
		}

		public void TestFirewallHack()
		{
			Firewall fw = new Firewall();
			fw.ConnectionValidationTimeMin = 0.5f;
			fw.ConnectionValidationTimeRange = 1f;
			fw.ActivityLevel = 0.125;
			fw.TrafficComplexity = 5f;
			fw.Size = 1;
			UIWindowHackFirewall whf = new UIWindowHackFirewall(fw);
			OpenWindow(whf);
			whf.Resize(whf.Bounds.X, whf.Bounds.Y);
			whf.MoveTo(70, TopMenu.Bounds.Y + 10);
		}
	}
}

