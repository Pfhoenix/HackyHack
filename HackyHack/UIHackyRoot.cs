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
			UIMenuItem fh = ht.AddSubItem("Firewall", null);
			fh.AddSubItem("Easy", TestFirewallHackEasy);
			fh.AddSubItem("Medium", TestFirewallHackMedium);
			fh.AddSubItem("Hard", TestFirewallHackHard);
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

		public void TestFirewallHackEasy()
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

		public void TestFirewallHackMedium()
		{
			Firewall fw = new Firewall();
			fw.ConnectionValidationTimeMin = 0.25f;
			fw.ConnectionValidationTimeRange = 0.5f;
			fw.ActivityLevel = 0.05;
			fw.TrafficComplexity = 5f;
			fw.Size = 2;
			UIWindowHackFirewall whf = new UIWindowHackFirewall(fw);
			OpenWindow(whf);
			whf.Resize(whf.Bounds.X, whf.Bounds.Y);
			whf.MoveTo(70, TopMenu.Bounds.Y + 10);
		}

		public void TestFirewallHackHard()
		{
			Firewall fw = new Firewall();
			fw.ConnectionValidationTimeMin = 0.2f;
			fw.ConnectionValidationTimeRange = 0.33f;
			fw.ActivityLevel = 0.025;
			fw.TrafficComplexity = 5f;
			fw.Size = 3;
			UIWindowHackFirewall whf = new UIWindowHackFirewall(fw);
			OpenWindow(whf);
			whf.Resize(whf.Bounds.X, whf.Bounds.Y);
			whf.MoveTo(70, TopMenu.Bounds.Y + 10);
		}

		public override void Render(float psx, float psy)
		{
			if (!bVisible) return;

			float cpsx = psx + Position.X;
			float cpsy = psy + Position.Y;

			bool scissoring = false;

			if (Children != null)
			{
				UILinkedListNode lln = Children;
				while (lln != null)
				{
					if ((lln.Element is UIWindow) && !scissoring)
					{
						scissoring = true;
						Renderer.r.EnableScissor();
					}
					lln.Element.Render(cpsx, cpsy);
					lln = lln.NextNode;
				}
			}

			Renderer.r.DisableScissor();

			RenderMe(cpsx, cpsy);
		}
	}
}

