using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES11;
using Android.Graphics;
using RPCoreLib;

namespace HackyHack
{
	public class UITaskBar : UIElement
	{
		class UITaskBarItem
		{
			public UIWindow Window;
			public float RealWidth;
			public float DisplayWidth;
			public float TextWidth;
			public string DisplayText;
			public float GrowthTime;
			public float ShrinkTime;
		}

		const float AutoScrollTimeFrame = 1f;
		const float AnimationTimeFrame = 0.75f;
		const float TextFadeTimeFrame = 0.15f;

		float ScrollX;
		float TotalItemsWidth;
		const float ItemPadding = 4;
		int TaskbarAreaWidth;
		readonly List<UITaskBarItem> Items;
		readonly Texture[] OnArrows;
		readonly Texture[] OffArrows;
		readonly Texture[] UpBrackets;
		readonly Texture[] DownBrackets;
		Font TextFont;

		public const int MaxItemLength = 256;

		float AutoScrollTo;
		float AutoScrollStart;
		float AutoScrollTime;

		public UITaskBar()
		{
			bAlwaysOnTop = true;
			bFlingAsScroll = true;

			Items = new List<UITaskBarItem>();
			OnArrows = new Texture[2];
			OnArrows[0] = ContentManager.cm.LoadResourceToTexture(Resource.Drawable.tb_left_on, "tb_left_on", null);
			OnArrows[1] = ContentManager.cm.LoadResourceToTexture(Resource.Drawable.tb_right_on, "tb_right_on", null);
			OffArrows = new Texture[2];
			OffArrows[0] = ContentManager.cm.LoadResourceToTexture(Resource.Drawable.tb_left_off, "tb_left_off", null);
			OffArrows[1] = ContentManager.cm.LoadResourceToTexture(Resource.Drawable.tb_right_off, "tb_right_off", null);
			UpBrackets = new Texture[2];
			UpBrackets[0] = ContentManager.cm.LoadResourceToTexture(Resource.Drawable.tb_bracket_left_up, "tb_bracket_left_up", null);
			UpBrackets[1] = ContentManager.cm.LoadResourceToTexture(Resource.Drawable.tb_bracket_right_up, "tb_bracket_right_up", null);
			DownBrackets = new Texture[2];
			DownBrackets[0] = ContentManager.cm.LoadResourceToTexture(Resource.Drawable.tb_bracket_left_down, "tb_bracket_left_down", null);
			DownBrackets[1] = ContentManager.cm.LoadResourceToTexture(Resource.Drawable.tb_bracket_right_down, "tb_bracket_right_down", null);

			TextFont = ContentManager.cm.GetFont("default");
		}

		public void ResetWithParent()
		{
			Bounds.X = Parent.Bounds.X;
			Bounds.Y = OnArrows[0].Height;
			Position.X = 0;
			Position.Y = Parent.Bounds.Y - Bounds.Y;
			TaskbarAreaWidth = (int)(Bounds.X - OnArrows[0].Width - OffArrows[0].Width);
			TotalItemsWidth = 0;
			foreach (UITaskBarItem item in Items) TotalItemsWidth += item.DisplayWidth;
			// account for the spacing between items
			if (Items.Count > 1) TotalItemsWidth += (Items.Count - 1) * ItemPadding;
		}

		public override void ProcessScreenChanged()
		{
			ResetWithParent();
			base.ProcessScreenChanged();
		}

		public override void Resize(float nw, float nh)
		{
			ResetWithParent();
		}

		public override bool ProcessInputEvent(EInputEvent ie, float x, float y, float px, float py)
		{
			if (base.ProcessInputEvent(ie, x, y, px, py))
			{
				// this will allow the user to scroll and interrupt the autoscroll behavior
				if (ie == EInputEvent.ScrollStart) AutoScrollTime = 0;

				return true;
			}

			switch (ie)
			{
				case EInputEvent.SingleTap:
					float sx = -ScrollX;
					foreach (UITaskBarItem item in Items)
					{
						if ((x >= sx) && (x <= sx + item.DisplayWidth))
						{
							item.Window.SetHidden(item.Window.bVisible);
							if (item.Window.bVisible) item.Window.BringToFront();
							break;
						}
						sx += item.DisplayWidth + ItemPadding;
					}
					return true;

				case EInputEvent.Scroll:
					if (AutoScrollTime > RPGlobals.g.RunningTime) return true;
					ScrollX -= x;
					if (ScrollX + TaskbarAreaWidth > TotalItemsWidth) ScrollX = TotalItemsWidth - TaskbarAreaWidth;
					if (ScrollX < 0) ScrollX = 0;
					return true;
			}

			return false;
		}

		public void AddWindow(UIWindow uiw)
		{
			UITaskBarItem item = new UITaskBarItem();
			item.Window = uiw;
			item.RealWidth = UpBrackets[0].Width + UpBrackets[1].Width;
			Vector2 se = TextFont.MeasureText("...");
			float ldt;
			item.DisplayText = TextFont.GetTextForLimit(uiw.Title, MaxItemLength - item.RealWidth - se.X, out ldt);
			if (item.DisplayText != uiw.Title)
			{
				item.DisplayText += "...";
				item.RealWidth += (float)Math.Floor(se.X);
				item.TextWidth = se.X;
			}
			item.RealWidth += (float)Math.Floor(ldt);
			item.TextWidth += ldt;
			Items.Insert(0, item);
			if (Items.Count > 1) TotalItemsWidth += ItemPadding;
			item.DisplayWidth = UpBrackets[0].Width + UpBrackets[1].Width;
			TotalItemsWidth += item.DisplayWidth;
			item.GrowthTime = RPGlobals.g.RunningTime + AnimationTimeFrame + TextFadeTimeFrame;

			if ((AutoScrollTo != 0) || (AutoScrollTime <= RPGlobals.g.RunningTime))
			{
				AutoScrollTo = 0;
				AutoScrollStart = ScrollX;
				AutoScrollTime = RPGlobals.g.RunningTime + AutoScrollTimeFrame;
			}
		}

		public void RemoveWindow(UIWindow uiw)
		{
			UITaskBarItem item = Items.Find(i => i.Window == uiw);
			item.ShrinkTime = RPGlobals.g.RunningTime + AnimationTimeFrame + TextFadeTimeFrame;
		}

		protected override void UpdateMe(float dt)
		{
			if (AutoScrollTime > 0)
			{
				ScrollX = InterpolationHelper.Smooth_Interp_Float(AutoScrollStart, AutoScrollTo, AutoScrollTime - AutoScrollTimeFrame, AutoScrollTime);
				if (AutoScrollTime < RPGlobals.g.RunningTime) AutoScrollTime = 0;
			}

			for (int i = 0; i < Items.Count; i++)
			{
				if (Items[i].GrowthTime > 0)
				{
					TotalItemsWidth -= Items[i].DisplayWidth;
					Items[i].DisplayWidth = UpBrackets[0].Width + UpBrackets[1].Width + InterpolationHelper.Linear_Interp_Float(0, Items[i].TextWidth, Items[i].GrowthTime - AnimationTimeFrame - TextFadeTimeFrame, Items[i].GrowthTime - TextFadeTimeFrame);
					TotalItemsWidth += Items[i].DisplayWidth;
					if (RPGlobals.g.RunningTime >= Items[i].GrowthTime) Items[i].GrowthTime = 0;
				}
				else if (Items[i].ShrinkTime > 0)
				{
					if (RPGlobals.g.RunningTime >= Items[i].ShrinkTime)
					{
						TotalItemsWidth -= Items[i].DisplayWidth + ItemPadding;
						if (TotalItemsWidth < 0) TotalItemsWidth = 0;
						Items.RemoveAt(i--);
						continue;
					}
					TotalItemsWidth -= Items[i].DisplayWidth;
					Items[i].DisplayWidth = UpBrackets[0].Width + UpBrackets[1].Width + InterpolationHelper.Linear_Interp_Float(Items[i].TextWidth, 0, Items[i].ShrinkTime - AnimationTimeFrame - TextFadeTimeFrame + TextFadeTimeFrame, Items[i].ShrinkTime);
					TotalItemsWidth += Items[i].DisplayWidth;
				}
			}
		}

		protected override void RenderMe(float px, float py)
		{
			Color color = new Color((byte)(UIManager.ui.UIColor.R * 0.25f), (byte)(UIManager.ui.UIColor.G * 0.25f), (byte)(UIManager.ui.UIColor.B * 0.25f));

			GL.Color4(UIManager.ui.UIColor.R, UIManager.ui.UIColor.G, UIManager.ui.UIColor.B, UIManager.ui.UIColor.A);

			// draw left arrow scroll hint
			if (ScrollX > 0) Renderer.r.DrawUI(OnArrows[0], px, py, 1, 1, 0);
			else Renderer.r.DrawUI(OffArrows[0], px, py, 1, 1, 0);

			// draw right arrow scroll hint
			if ((TotalItemsWidth - ScrollX) > TaskbarAreaWidth) Renderer.r.DrawUI(OnArrows[1], px + Bounds.X - OnArrows[1].Width, py, 1, 1, 0);
			else Renderer.r.DrawUI(OffArrows[1], px + Bounds.X - OffArrows[1].Width, py, 1, 1, 0);

			// this is a tracker in screen coordinates
			float DrawX = px + OnArrows[0].Width - ScrollX;
			// this is a tracker in taskbar local coordinates (area between arrows only)
			float PosX = 0;
			Texture[] Brackets;
			foreach (UITaskBarItem item in Items)
			{
				// this item starts past where we should be drawing, so stop drawing entirely
				if (PosX >= TaskbarAreaWidth + ScrollX) break;

				// this item's end is off-screen to the left, so don't draw and skip to the next item
				if ((PosX + item.DisplayWidth) < ScrollX)
				{
					DrawX += item.DisplayWidth;
					PosX += item.DisplayWidth;
					if (item != Items[0])
					{
						DrawX += ItemPadding;
						PosX += ItemPadding;
					}
					continue;
				}

				// only the first item doesn't get the padding in front
				if (item != Items[0])
				{
					DrawX += ItemPadding;
					PosX += ItemPadding;
				}

				Brackets = item.Window.bVisible ? DownBrackets : UpBrackets;

				if (item.GrowthTime > 0)
				{
					// draw left bracket
					Renderer.r.DrawUI(Brackets[0], DrawX, py, 1, 1, 0);
					// draw right bracket
					Renderer.r.DrawUI(Brackets[1], DrawX + item.DisplayWidth - Brackets[1].Width, py, 1, 1, 0);
					DrawX += Brackets[0].Width;
					PosX += Brackets[0].Width;
					// calculate how far into the DisplayText to render
					// this is the delay for starting each character
					float TextPerCharDelay = (AnimationTimeFrame + TextFadeTimeFrame) / item.DisplayText.Length;
					// the total amount of time the entire animation takes
					float AnimatingStartTime = item.GrowthTime - AnimationTimeFrame - TextFadeTimeFrame;
					// the amount of time since starting this
					float TimeAnimating = RPGlobals.g.RunningTime - AnimatingStartTime;
					int NumFullChars = 0;
					float dx = DrawX;
					for (int i = 0; i < item.DisplayText.Length; i++)
					{
						// if the time is before this letter could even start drawing, stop
						if (TimeAnimating < (i * TextPerCharDelay)) break;
						// this character is already full, account for it for drawing later
						if (TimeAnimating >= (i * TextPerCharDelay + TextFadeTimeFrame)) NumFullChars++;
						// we're in the middle of this character
						else
						{
							string s;
							Vector2 st;
							// before we do anything else, check to see if we have full characters to draw
							if (NumFullChars > 0)
							{
								s = item.DisplayText.Substring(0, NumFullChars);
								st = TextFont.MeasureText(s);
								GL.Color4(255, 255, 255, 255);
								Renderer.r.DrawText(s, TextFont, dx, py + (Bounds.Y - TextFont.CharHeight) / 2);
								dx += st.X;
								NumFullChars = 0;
							}

							s = item.DisplayText.Substring(i, 1);
							st = TextFont.MeasureText(s);
							Color cc = InterpolationHelper.Linear_Interp_Color(color, Color.White, i * TextPerCharDelay + AnimatingStartTime, i * TextPerCharDelay + AnimatingStartTime + TextFadeTimeFrame);
							GL.Color4(cc.R, cc.G, cc.B, cc.A);
							Renderer.r.DrawText(s, TextFont, dx, py + (Bounds.Y - TextFont.CharHeight) / 2);
							dx += st.X;
						}
					}

					// account for drawing the right bracket earlier
					DrawX += item.DisplayWidth - Brackets[0].Width;
					PosX += item.DisplayWidth - Brackets[0].Width;

					GL.Color4(UIManager.ui.UIColor.R, UIManager.ui.UIColor.G, UIManager.ui.UIColor.B, UIManager.ui.UIColor.A);
				}
				else if (item.ShrinkTime > 0)
				{
					// draw left bracket
					Renderer.r.DrawUI(Brackets[0], DrawX, py, 1, 1, 0);
					// draw right bracket
					Renderer.r.DrawUI(Brackets[1], DrawX + item.DisplayWidth - Brackets[1].Width, py, 1, 1, 0);
					DrawX += Brackets[0].Width;
					PosX += Brackets[0].Width;
					// this is the delay for starting each character
					float TextPerCharDelay = (AnimationTimeFrame + TextFadeTimeFrame) / item.DisplayText.Length;
					// the total amount of time the entire animation takes
					float AnimatingStartTime = item.ShrinkTime - AnimationTimeFrame - TextFadeTimeFrame;
					// the amount of time since starting this
					float TimeAnimating = RPGlobals.g.RunningTime - AnimatingStartTime;
					int NumFullChars = 0;
					float dx = DrawX;
					for (int i = 0; i < item.DisplayText.Length; i++)
					{
						// if the time is after this letter is done drawing, stop
						if (TimeAnimating >= ((item.DisplayText.Length - i - 1) * TextPerCharDelay + TextFadeTimeFrame)) break;
						// this character is still full, account for it for drawing later
						if (TimeAnimating < ((item.DisplayText.Length - i - 1) * TextPerCharDelay)) NumFullChars++;
						// we're in the middle of this character
						else
						{
							string s;
							Vector2 st;
							// before we do anything else, check to see if we have full characters to draw
							if (NumFullChars > 0)
							{
								s = item.DisplayText.Substring(0, NumFullChars);
								st = TextFont.MeasureText(s);
								GL.Color4(255, 255, 255, 255);
								Renderer.r.DrawText(s, TextFont, dx, py + (Bounds.Y - TextFont.CharHeight) / 2);
								dx += st.X;
								NumFullChars = 0;
							}

							s = item.DisplayText.Substring(i, 1);
							st = TextFont.MeasureText(s);
							Color cc = InterpolationHelper.Linear_Interp_Color(Color.White, color, (item.DisplayText.Length - i - 1) * TextPerCharDelay + AnimatingStartTime, (item.DisplayText.Length - i - 1) * TextPerCharDelay + AnimatingStartTime + TextFadeTimeFrame);
							GL.Color4(cc.R, cc.G, cc.B, cc.A);
							Renderer.r.DrawText(s, TextFont, dx, py + (Bounds.Y - TextFont.CharHeight) / 2);
							dx += st.X;
						}
					}

					// account for drawing the right bracket earlier
					DrawX += item.DisplayWidth - Brackets[0].Width;
					PosX += item.DisplayWidth - Brackets[0].Width;

					GL.Color4(UIManager.ui.UIColor.R, UIManager.ui.UIColor.G, UIManager.ui.UIColor.B, UIManager.ui.UIColor.A);
				}
				else
				{
					// draw left bracket
					Renderer.r.DrawUI(Brackets[0], DrawX, py, 1, 1, 0);
					DrawX += Brackets[0].Width;
					PosX += Brackets[0].Width;
					// draw text
					if (item.Window.bVisible) GL.Color4(255, 255, 255, 255);
					else GL.Color4(128, 128, 128, 255);
					float t = Renderer.r.DrawText(item.DisplayText, TextFont, DrawX, py + (Bounds.Y - TextFont.CharHeight) / 2);
					DrawX += t;
					PosX += t;
					// draw right bracket
					GL.Color4(UIManager.ui.UIColor.R, UIManager.ui.UIColor.G, UIManager.ui.UIColor.B, UIManager.ui.UIColor.A);
					Renderer.r.DrawUI(Brackets[1], DrawX, py, 1, 1, 0);
					DrawX += Brackets[1].Width;
					PosX += Brackets[1].Width;
				}
			}

			GL.Color4(color.R, color.G, color.B, color.A);
			Renderer.r.DrawUI(Renderer.r.White, px, py, Bounds.X / 2, Bounds.Y / 2, 0);
		}
	}
}

