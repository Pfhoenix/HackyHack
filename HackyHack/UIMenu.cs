using System.Collections.Generic;
using OpenTK.Graphics.ES11;
using Android.Graphics;
using RPCoreLib;

namespace HackyHack
{
	public class UIMenuItem
	{
		public UIMenu Menu;

		public delegate void MenuItemFunction();
		public event MenuItemFunction Action;

		public List<UIMenuItem> SubItems;
		public string Text;
		public float LargestChildWidth;
		public float Width;


		public UIMenuItem(string txt, UIMenu m)
		{
			Menu = m;
			Text = txt;
			Vector2 s = Menu.TextFont.MeasureText(Text);
			Width = s.X + Menu.TextPadding.X * 2;
			SubItems = new List<UIMenuItem>();
		}

		public UIMenuItem AddSubItem(string txt, MenuItemFunction action)
		{
			UIMenuItem mi = new UIMenuItem(txt, Menu);
			mi.Action += action;
			SubItems.Add(mi);

			if (mi.Width > LargestChildWidth) LargestChildWidth = mi.Width;

			return mi;
		}

		public void PerformAction()
		{
			if (Action != null) Action();
		}
	}

	public class UIMenu : UIElement
	{
		public Font TextFont;
		public Vector2 TextPadding;
		public UIMenuItem RootItem;
		List<UIMenuItem> OpenItems;
		float ItemHeight;

		readonly float MenuListStartY;

		int ScrollingList = -1;
		readonly float[] ListScroll = { 0, 0 };
		readonly float[] ListSizeLimit = { 0, 0 };
		readonly float[] ListAreaSize = { 0, 0 };

		public UITaskBar TaskBar;
		
		public UIMenu(Font tf)
		{
			bAlwaysOnTop = true;
			bFlingAsScroll = true;
			TextFont = tf;
			OpenItems = new List<UIMenuItem>();
			TextPadding = new Vector2(10, 10);
			ItemHeight = TextFont.CharHeight + TextPadding.Y * 2;
			MenuListStartY = ItemHeight + 5;
		}

		bool IsPointInOpenList(float x, float y)
		{
			if (y > ItemHeight) return false;
			if (OpenItems.Count > 0) return (x <= ListSizeLimit[0]);
			if (x > RootItem.Width) return false;

			return true;
		}

		bool IsPointInMenuList(float x, float y)
		{
			if (OpenItems.Count == 0) return false;
			if (x > OpenItems[OpenItems.Count - 1].LargestChildWidth) return false;
			if (y < MenuListStartY) return false;
			if (y > MenuListStartY + OpenItems[OpenItems.Count - 1].SubItems.Count * ItemHeight) return false;

			return true;
		}

		public override bool IsPointIn(float x, float y, float px, float py)
		{
			if (IsPointInOpenList(x, y)) return true;
			if (IsPointInMenuList(x, y)) return true;

			if ((OpenItems.Count > 0) && (ScrollingList == -1)) OpenItems.Clear();

			return false;
		}

		public override void ProcessScreenChanged()
		{
			ListAreaSize[0] = Renderer.r.ScreenRect.Right;
			ListAreaSize[1] = Renderer.r.ScreenRect.Bottom - MenuListStartY - TaskBar.Bounds.Y;
			ListSizeLimit[0] = 0;
			foreach (UIMenuItem mi in OpenItems) ListSizeLimit[0] += mi.Width;
			ListSizeLimit[1] = OpenItems.Count * ItemHeight;
		}

		public void OpenItemTapped(UIMenuItem mi)
		{
			if ((OpenItems.Count > 1) && (mi == OpenItems[OpenItems.Count - 1])) return;
			if ((OpenItems.Count == 1) && (OpenItems[0] == mi))
			{
				CloseMenu();
				return;
			}

			for (int i = OpenItems.Count - 1; i > -1; i--)
			{
				if (OpenItems[i] == mi) break;
				ListSizeLimit[0] -= OpenItems[i].Width;
				OpenItems.RemoveAt(i);
			}
			ListSizeLimit[1] = OpenItems[OpenItems.Count - 1].SubItems.Count * ItemHeight;
		}

		public override bool ProcessInputEvent(EInputEvent ie, float x, float y, float px, float py)
		{
			if (ie == EInputEvent.ScrollStart)
			{
				if (IsPointInOpenList(x, y)) ScrollingList = 0;
				else if (IsPointInMenuList(x, y)) ScrollingList = 1;
				else ScrollingList = -1;
			}

			if (base.ProcessInputEvent(ie, x, y, px, py))
			{
				if (ie == EInputEvent.Release) ScrollingList = -1;

				return true;
			}

			switch (ie)
			{
				case EInputEvent.SingleTap:
					if (IsPointInOpenList(x, y))
					{
						if (OpenItems.Count > 0)
						{
							float sx = -ListScroll[0];
							foreach (UIMenuItem mi in OpenItems)
							{
								if ((x >= sx) && (x <= sx + mi.Width))
								{
									OpenItemTapped(mi);
									break;
								}
								sx += mi.Width;
							}
						}
						else
						{
							OpenItems.Add(RootItem);
							ListSizeLimit[0] = RootItem.Width;
							ListSizeLimit[1] = RootItem.SubItems.Count * ItemHeight;
							ListScroll[1] = 0;
						}
					}
					else if (IsPointInMenuList(x, y))
					{
						float sy = MenuListStartY - ListScroll[1];
						foreach (UIMenuItem mi in OpenItems[OpenItems.Count - 1].SubItems)
						{
							if ((y >= sy) && (y <= sy + ItemHeight))
							{
								if (mi.SubItems.Count > 0)
								{
									OpenItems.Add(mi);
									ListSizeLimit[0] += mi.Width;
									ListSizeLimit[1] = mi.SubItems.Count * ItemHeight;
									ListScroll[1] = 0;
								}
								else
								{
									mi.PerformAction();
									CloseMenu();
								}
								break;
							}
							sy += ItemHeight;
						}
					}

					return true;

				case EInputEvent.Scroll:
					if (ScrollingList < 0) return true;

					if (ScrollingList == 0) ListScroll[ScrollingList] -= x;
					else if (ScrollingList == 1) ListScroll[ScrollingList] -= y;
					if (ListScroll[ScrollingList] + ListAreaSize[ScrollingList] > ListSizeLimit[ScrollingList]) ListScroll[ScrollingList] = ListSizeLimit[ScrollingList] - ListAreaSize[ScrollingList];
					if (ListScroll[ScrollingList] < 0) ListScroll[ScrollingList] = 0;

					return true;
			}

			return false;
		}

		void CloseMenu()
		{
			OpenItems.Clear();
			ListSizeLimit[0] = 0;
			ListSizeLimit[1] = 0;
		}

		void RenderOpenItem(UIMenuItem mi, float x, float y, Color bc, Color tc)
		{
			Color bgc = new Color(bc);
			// draw shaded border
			bgc.R += 16;
			bgc.G += 16;
			bgc.B += 16;
			Renderer.r.DrawLine(x, y, x + mi.Width, y, 1, bgc);
			Renderer.r.DrawLine(x, y, x, y + ItemHeight, 1, bgc);
			bgc.R = (byte)(bc.R - 16);
			bgc.G = (byte)(bc.G - 16);
			bgc.B = (byte)(bc.B - 16);
			Renderer.r.DrawLine(x, y + ItemHeight, x + mi.Width, y + ItemHeight, 1, bgc);
			Renderer.r.DrawLine(x + mi.Width, y, x + mi.Width, y + ItemHeight, 1, bgc);
			// draw text
			GL.Color4(tc.R, tc.G, tc.B, 255);
			Renderer.r.DrawText(mi.Text, TextFont, x + TextPadding.X, y + TextPadding.Y);
			// draw background
			GL.Color4(0, 0, 0, 255);
			Renderer.r.DrawUI(Renderer.r.White, x, y, mi.Width / 2, ItemHeight / 2, 0);
		}

		void RenderMenuItem(UIMenuItem mi, float w, float x, float y, Color bc, Color tc)
		{
			Color bgc = new Color(bc);
			// draw shaded border
			bgc.R += 16;
			bgc.G += 16;
			bgc.B += 16;
			Renderer.r.DrawLine(x, y, x + w, y, 1, bgc);
			Renderer.r.DrawLine(x, y, x, y + ItemHeight, 1, bgc);
			bgc.R = (byte)(bc.R - 16);
			bgc.G = (byte)(bc.G - 16);
			bgc.B = (byte)(bc.B - 16);
			Renderer.r.DrawLine(x, y + ItemHeight, x + w, y + ItemHeight, 1, bgc);
			Renderer.r.DrawLine(x + w, y, x + w, y + ItemHeight, 1, bgc);
			// draw text
			GL.Color4(tc.R, tc.G, tc.B, 255);
			Renderer.r.DrawText(mi.Text, TextFont, x + TextPadding.X, y + TextPadding.Y);
			// draw background
			GL.Color4(0, 0, 0, 255);
			Renderer.r.DrawUI(Renderer.r.White, x, y, mi.Width / 2, ItemHeight / 2, 0);
		}

		protected override void RenderMe(float px, float py)
		{
			Color bgc = new Color(UIManager.ui.UIColor);
			bgc.R /= 2;
			bgc.G /= 2;
			bgc.B /= 2;
			// render just the menu button
			if (OpenItems.Count == 0) RenderOpenItem(RootItem, 0, 0, bgc, new Color(UIManager.ui.UITextColor.R / 2, UIManager.ui.UITextColor.G / 2, UIManager.ui.UITextColor.B / 2, 255));
			else
			{
				// this is a tracker in screen coordinates
				float Draw;
				// this is a tracker in taskbar local coordinates (area between arrows only)
				float Pos;

				// render the open list
				Draw = ListScroll[0];
				Pos = 0;
				// text color for open items that aren't displaying
				Color tc = new Color();
				float fc = 0.5f;
				float fcpi = (OpenItems.Count == 1) ? 1 : 0.5f / (OpenItems.Count - 1);
				if (OpenItems.Count == 1) fc = 1;
				foreach (UIMenuItem mi in OpenItems)
				{
					if (Pos >= ListAreaSize[0] + ListScroll[0]) break;

					if ((Pos + mi.Width) < ListScroll[0])
					{
						Draw += mi.Width;
						Pos += mi.Width;
						fc += fcpi;
						continue;
					}

					tc.R = (byte)(UIManager.ui.UITextColor.R * fc);
					tc.G = (byte)(UIManager.ui.UITextColor.G * fc);
					tc.B = (byte)(UIManager.ui.UITextColor.B * fc);
					fc += fcpi;
					RenderOpenItem(mi, Draw, 0, bgc, tc);

					Draw += mi.Width;
					Pos += mi.Width;
				}

				// render the menu list
				Draw = MenuListStartY - ListScroll[1];
				Pos = 0;
				float miw = OpenItems[OpenItems.Count - 1].LargestChildWidth;
				foreach (UIMenuItem mi in OpenItems[OpenItems.Count - 1].SubItems)
				{
					if (Pos >= ListAreaSize[1] + ListScroll[1]) break;

					if ((Pos + ItemHeight) < ListScroll[1])
					{
						Draw += ItemHeight;
						Pos += ItemHeight;
						continue;
					}

					RenderMenuItem(mi, miw, 0, Draw, bgc, UIManager.ui.UITextColor);

					Draw += ItemHeight;
					Pos += ItemHeight;
				}
			}
		}
	}
}

