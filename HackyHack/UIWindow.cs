using System;
using OpenTK.Graphics.ES11;
using Android.Graphics;

namespace HackyHack
{
	public class UIWindow : UIElement
	{
		protected static Texture BgTex;
		protected static float UVwidth, UVheight; // because we're using an atlas texture, we need to know this
		protected static float UVppX, UVppY;	// precalculate UV space per texture pixel
		protected static Texture ULCornerTex, TBBgTex, LLCornerTex, LRCornerTex;
		protected Color bgcolor;
		protected UIButton CloseButton;

		protected bool bMoving;
		protected bool bResizingLeft;
		protected bool bResizingRight;

		protected const float AnimOpen_Step1 = 0.25f;
		protected const float AnimOpen_Step2 = 0.5f;
		protected const float AnimOpen_Step3 = 0.75f;
		protected const float AnimOpen_Step4 = 1.0f;

		protected const float AnimClose_Step1 = 0.25f;
		protected const float AnimClose_Step2 = 0.5f;
		protected const float AnimClose_Step3 = 0.75f;
		protected const float AnimClose_Step4 = 1.0f;

		protected bool bOpening;
		protected bool bClosing;
		protected float AnimStartTime = -1;

		public string Title = "Blah sse blah";
		protected Font TextFont;
		protected bool bMaximized;
		protected Vector2 RestorePosition;
		protected Vector2 RestoreSize;

		public UIWindow()
		{
			if (BgTex == null)
			{
				BgTex = ContentManager.cm.FindTexture("window_bg");
				UVwidth = BgTex.UVs[2];
				UVheight = BgTex.UVs[5];
				UVppX = UVwidth / BgTex.NativeInfo.Width;
				UVppY = UVheight / BgTex.NativeInfo.Height;

				ULCornerTex = ContentManager.cm.FindTexture("window_ul_corner");
				TBBgTex = ContentManager.cm.FindTexture("window_title_bg");
				LLCornerTex = ContentManager.cm.FindTexture("window_ll_corner");
				LRCornerTex = ContentManager.cm.FindTexture("window_lr_corner");
			}

			bgcolor = new Color();
			bgcolor.R = (byte)(Globals.g.RNG.Next(128) + 128);
			bgcolor.G = (byte)(Globals.g.RNG.Next(128) + 128);
			bgcolor.B = (byte)(Globals.g.RNG.Next(128) + 128);
			bgcolor.A = 255;

			Texture t = ContentManager.cm.FindTexture("window_ur_corner");
			CloseButton = new UIButton();
			CloseButton.Setup(t, bgcolor);
			CloseButton.Pressed += ClosePressed;
			AddChild(CloseButton);

			TextFont = ContentManager.cm.GetFont("slider");

			RestorePosition = new Vector2();
			RestoreSize = new Vector2();
		}

		public void Open()
		{
			if (bOpening || bClosing) return;

			bOpening = true;
			bAcceptsInput = false;
			AnimStartTime = Globals.g.RunningTime;
		}

		public void SetHidden(bool bH)
		{
			bVisible = !bH;
			bAcceptsInput = !bH;
		}

		public override bool FindChildForEvent(InputEventInfo iei, float px, float py)
		{
			if (!bAcceptsInput) return false;

			if (IsPointIn(iei.X, iei.Y, px, py))
			{
				if (Parent != null)	Parent.BringChildToFront(this);

				if (NumChildren == 0)
				{
					ProcessInputEvent(iei.InputEvent, iei.X, iei.Y, px + Position.X, py + Position.Y);
				}
				else
				{
					UILinkedListNode lln = Children;
					while (lln != null)
					{
						if (lln.Element.FindChildForEvent(iei, px + Position.X, py + Position.Y))
							return true;
						lln = lln.NextNode;
					}

					ProcessInputEvent(iei.InputEvent, iei.X, iei.Y, px + Position.X, py + Position.Y);
				}

				return true;
			}

			return false;
		}

		public override bool ProcessInputEvent(EInputEvent ie, float x, float y, float px, float py)
		{
			if ((ie == EInputEvent.ScrollStart) && !bMaximized)
			{
				if ((x >= px + ULCornerTex.Width) && (x <= px + Bounds.X - CloseButton.Bounds.X) && (y <= py + TBBgTex.Height))
				{
					// store the current position and size as restore point in case of fling later on
					RestorePosition.Set(Position);
					RestoreSize.Set(Bounds);
					bMoving = true;
				}
				// check for lower left resize corner
				else if ((x <= px + LLCornerTex.Width) && (y >= py + Bounds.Y - LLCornerTex.Height)) bResizingLeft = true;
				// check for lower right resize corner
				else if ((x >= px + Bounds.X - LRCornerTex.Width) && (y >= py + Bounds.Y - LRCornerTex.Height)) bResizingRight = true;
			}

			if (base.ProcessInputEvent(ie, x, y, px, py))
			{
				if (ie == EInputEvent.Release)
				{
					bResizingLeft = false;
					bResizingRight = false;
					bMoving = false;
				}
				return true;
			}

			switch (ie)
			{
				case EInputEvent.Scroll:
					if (bResizingLeft)
					{
						float bx = Bounds.X;
						Resize(Bounds.X - x, Bounds.Y + y);
						MoveTo(Position.X + bx - Bounds.X, Position.Y);
					}
					else if (bResizingRight) Resize(Bounds.X + x, Bounds.Y + y);
					else if (bMoving) MoveTo(Position.X + x, Position.Y + y);
					return true;

				case EInputEvent.Fling:
					if (y > 1000f)
					{
						if (bMaximized)
						{
							bMaximized = false;
							MoveTo(RestorePosition.X, RestorePosition.Y);
							Resize(RestoreSize.X, RestoreSize.Y);
						}
						else if (bMoving)
						{
							bMoving = false;
							SetHidden(true);
						}
					}
					else if (y < -1000f)
					{
						if (bMoving)
						{
							bMaximized = true;
							//RestorePosition.Set(Position);
							//RestoreSize.Set(Bounds);
							MoveTo(0, 0);
							Resize(Parent.Bounds.X, Parent.Bounds.Y - UIManager.ui.Root.Taskbar.Bounds.Y);
							bMoving = false;
						}
					}
					return true;
			}

			return false;
		}

		public override void ProcessScreenChanged()
		{
			MoveTo(Math.Max(Math.Min(Position.X, Parent.Bounds.X - 64), 0), Math.Max(Math.Min(Position.Y, Parent.Bounds.Y - 64), 0));
		}

		public override void Resize(float nw, float nh)
		{
			if (nw < 100) nw = 100;
			if (nh < 100) nh = 100;

			base.Resize(nw, nh);

			CloseButton.Position.X = Bounds.X - CloseButton.Bounds.X - 1;
		}

		public virtual void MoveTo(float nx, float ny)
		{
			Position.X = nx;
			Position.Y = ny;
			if (Position.X < 0) Position.X = 0;
			else if (Position.X > (Parent.Bounds.X - 25)) Position.X = Parent.Bounds.X - 25;
			if (Position.Y < 0) Position.Y = 0;
			else if (Position.Y > (Parent.Bounds.Y - 25)) Position.Y = Parent.Bounds.Y - 25;
		}

		public void ClosePressed(UIButton uib)
		{
			if (bOpening || bClosing) return;

			bClosing = true;
			bAcceptsInput = false;
			AnimStartTime = Globals.g.RunningTime;

			UIManager.ui.Root.Taskbar.RemoveWindow(this);
		}

		protected override void RenderMe(float px, float py)
		{
			// draw title
			GL.Color4(255, 255, 255, 255);
			Renderer.r.DrawText(Title, TextFont, px + ULCornerTex.Width, py + (TBBgTex.Height - TextFont.CharHeight) * 0.5f, px + Bounds.X - CloseButton.Bounds.X);

			GL.Color4(bgcolor.R, bgcolor.G, bgcolor.B, bgcolor.A);

			// render upper left corner
			Renderer.r.DrawUI(ULCornerTex, px, py, 1, 1, 0);

			// render titlebar background
			Renderer.r.DrawUI(TBBgTex, px + ULCornerTex.Width, py, Bounds.X - ULCornerTex.Width - CloseButton.Bounds.X, 1, 0);

			// render lower left corner
			Renderer.r.DrawUI(LLCornerTex, px, py + Bounds.Y - LLCornerTex.Height, 1, 1, 0);

			// render lower right corner
			Renderer.r.DrawUI(LRCornerTex, px + Bounds.X - LRCornerTex.Width, py + Bounds.Y - LRCornerTex.Height, 1, 1, 0);

			// draw lines for the bottom, left, and right frame (connects the corners)
			Renderer.r.DrawLine(px + LLCornerTex.Width, py + Bounds.Y - 1, px + Bounds.X - LRCornerTex.Width, py + Bounds.Y - 1, 1, bgcolor);
			Renderer.r.DrawLine(px + 1, py + ULCornerTex.Height, px + 1, py + Bounds.Y - LLCornerTex.Height, 1, bgcolor);
			Renderer.r.DrawLine(px + Bounds.X - 1, py + ULCornerTex.Height, px + Bounds.X - 1, py + Bounds.Y - LLCornerTex.Height, 1, bgcolor);

			// render the background
			int ipx = (int)px;
			int ipy = (int)py + TBBgTex.Height;
			int x = ipx;
			int y;
			int w;
			int h;
			bool bDoneX = false, bDoneY = false;

			while (!bDoneX)
			{
				y = ipy;

				// calculate the pixel width of this tile
				w = BgTex.Width - x % BgTex.Width;
				BgTex.UVs[0] = (BgTex.Width - w) / (float)BgTex.Width * UVwidth;
				// adjust for going beyond the border
				if ((x + w) >= (ipx + (int)Bounds.X))
				{
					w = ipx + (int)Bounds.X - x;
					// this allows us to short out of the entire tile process
					if (w <= 0) break;
					bDoneX = true;
					BgTex.UVs[2] = w / (float)BgTex.Width * UVwidth;
				}
				else BgTex.UVs[2] = UVwidth;

				BgTex.UVs[4] = BgTex.UVs[0];
				BgTex.UVs[6] = BgTex.UVs[2];

				while (!bDoneY)
				{
					// calculate the pixel height of this tile
					h = BgTex.Height - y % BgTex.Height;
					BgTex.UVs[1] = (BgTex.Height - h) / (float)BgTex.Height * UVheight;
					// adjust for going beyond the border
					if ((y + h) >= (ipy + (int)Bounds.Y - TBBgTex.Height))
					{
						bDoneY = true;
						h = ipy + (int)Bounds.Y - TBBgTex.Height - y;
						if (h <= 0) break;
						BgTex.UVs[5] = h / (float)BgTex.Height * UVheight;
					}
					else BgTex.UVs[5] = UVheight;

					BgTex.UVs[3] = BgTex.UVs[1];
					BgTex.UVs[7] = BgTex.UVs[5];

					// render this tile
					Renderer.r.DrawUI(BgTex, x, y, w / (float)BgTex.Width, h / (float)BgTex.Height, 0);

					y += h;
				}

				x += w;
				bDoneY = false;
			}
		}

		protected virtual void RenderOpening(float px, float py)
		{
			float AnimTime = Globals.g.RunningTime - AnimStartTime;

			if (AnimTime >= AnimOpen_Step4)
			{
				AnimStartTime = -1;
				bOpening = false;
				bAcceptsInput = true;
				base.Render(px - Position.X, py - Position.Y);
				return;
			}

			Color color = new Color(bgcolor.ToArgb());

			if (AnimTime <= AnimOpen_Step1)
			{
				// fade in upper left corner
				AnimTime = AnimTime / AnimOpen_Step1;
				GL.Color4((byte)(color.R * AnimTime), (byte)(color.G * AnimTime), (byte)(color.B * AnimTime), color.A);
				Renderer.r.DrawUI(ULCornerTex, px, py, 1, 1, 0);
			}
			else
			{
				GL.Color4(bgcolor.R, bgcolor.G, bgcolor.B, bgcolor.A);
				Renderer.r.DrawUI(ULCornerTex, px, py, 1, 1, 0);
				if (AnimTime <= AnimOpen_Step2)
				{
					// fade in and move the close button in upper right
					AnimTime = (AnimTime - AnimOpen_Step1) / (AnimOpen_Step2 - AnimOpen_Step1);
					GL.Color4((byte)(color.R * AnimTime), (byte)(color.G * AnimTime), (byte)(color.B * AnimTime), color.A);
					float start = px + ULCornerTex.Width - CloseButton.ButtonTex.Width;
					float end = px + CloseButton.Position.X;
					float l = start + (end - start) * (3 * AnimTime * AnimTime - 2 * AnimTime * AnimTime * AnimTime);
					Renderer.r.DrawUI(CloseButton.ButtonTex, l, py + CloseButton.Position.Y, 1, 1, 0);
					// fade in and draw the titlebar background behind the close button
					start += CloseButton.ButtonTex.Width;
					l -= CloseButton.ButtonTex.Width + start;
					Renderer.r.DrawUI(TBBgTex, start, py, l, 1, 0);
				}
				else
				{
					Renderer.r.DrawUI(CloseButton.ButtonTex, px + CloseButton.Position.X, py + CloseButton.Position.Y, 1, 1, 0);
					Renderer.r.DrawUI(TBBgTex, px + ULCornerTex.Width, py, Bounds.X - ULCornerTex.Width - CloseButton.Bounds.X, 1, 0);
					if (AnimTime <= AnimOpen_Step3)
					{
						// fade in and move the resize corners
						AnimTime = (AnimTime - AnimOpen_Step2) / (AnimOpen_Step3 - AnimOpen_Step2);
						color.R = (byte)(color.R * AnimTime);
						color.G = (byte)(color.G * AnimTime);
						color.B = (byte)(color.B * AnimTime);
						GL.Color4(color.R, color.G, color.B, color.A);
						float start = py;
						float end = py + Bounds.Y - LLCornerTex.Height;
						float l = start + (end - start) * (3 * AnimTime * AnimTime - 2 * AnimTime * AnimTime * AnimTime);
						Renderer.r.DrawUI(LLCornerTex, px, l, 1, 1, 0);
						Renderer.r.DrawUI(LRCornerTex, px + Bounds.X - LRCornerTex.Width, l, 1, 1, 0);
						// draw the frame borders connecting them
						start += ULCornerTex.Height;
						l -= ULCornerTex.Height;
						Renderer.r.DrawLine(px + 1, start, px + 1, l, 1, color);
						Renderer.r.DrawLine(px + Bounds.X - 1, start, px + Bounds.X - 1, l, 1, color);
					}
					else
					{
						Renderer.r.DrawUI(LLCornerTex, px, py + Bounds.Y - LLCornerTex.Height, 1, 1, 0);
						Renderer.r.DrawUI(LRCornerTex, px + Bounds.X - LRCornerTex.Width, py + Bounds.Y - LRCornerTex.Height, 1, 1, 0);
						Renderer.r.DrawLine(px + 1, py + ULCornerTex.Height, px + 1, py + Bounds.Y - LLCornerTex.Height, 1, bgcolor);
						Renderer.r.DrawLine(px + Bounds.X - 1, py + ULCornerTex.Height, px + Bounds.X - 1, py + Bounds.Y - LLCornerTex.Height, 1, bgcolor);
						// draw the bottom border line growing in from the corners
						AnimTime = (AnimTime - AnimOpen_Step3) / (AnimOpen_Step4 - AnimOpen_Step3);
						color.R = (byte)(color.R * AnimTime);
						color.G = (byte)(color.G * AnimTime);
						color.B = (byte)(color.B * AnimTime);
						float l = (Bounds.X - LLCornerTex.Width - LRCornerTex.Width) * 0.5f * (3 * AnimTime * AnimTime - 2 * AnimTime * AnimTime * AnimTime);
						float start = px + LLCornerTex.Width;
						int y = (int)(py + Bounds.Y - 1);
						Renderer.r.DrawLine(start, y, start + l, y, 1, color);
						start = px + Bounds.X - LRCornerTex.Width - l;
						Renderer.r.DrawLine(start, y, start + l, y, 1, color);
						// fade in the background as well
						GL.Color4(color.R, color.G, color.B, color.A);
						int ipx = (int)px;
						int ipy = (int)py + TBBgTex.Height;
						int x = ipx;
						int w;
						int h;
						bool bDoneX = false, bDoneY = false;

						while (!bDoneX)
						{
							y = ipy;

							// calculate the pixel width of this tile
							w = BgTex.Width - x % BgTex.Width;
							BgTex.UVs[0] = (BgTex.Width - w) / (float)BgTex.Width * UVwidth;
							// adjust for going beyond the border
							if ((x + w) >= (ipx + (int)Bounds.X))
							{
								w = ipx + (int)Bounds.X - x;
								// this allows us to short out of the entire tile process
								if (w <= 0) break;
								bDoneX = true;
								BgTex.UVs[2] = w / (float)BgTex.Width * UVwidth;
							}
							else BgTex.UVs[2] = UVwidth;

							BgTex.UVs[4] = BgTex.UVs[0];
							BgTex.UVs[6] = BgTex.UVs[2];

							while (!bDoneY)
							{
								// calculate the pixel height of this tile
								h = BgTex.Height - y % BgTex.Height;
								BgTex.UVs[1] = (BgTex.Height - h) / (float)BgTex.Height * UVheight;
								// adjust for going beyond the border
								if ((y + h) >= (ipy + (int)Bounds.Y - TBBgTex.Height))
								{
									bDoneY = true;
									h = ipy + (int)Bounds.Y - TBBgTex.Height - y;
									if (h <= 0) break;
									BgTex.UVs[5] = h / (float)BgTex.Height * UVheight;
								}
								else BgTex.UVs[5] = UVheight;

								BgTex.UVs[3] = BgTex.UVs[1];
								BgTex.UVs[7] = BgTex.UVs[5];

								// render this tile
								Renderer.r.DrawUI(BgTex, x, y, w / (float)BgTex.Width, h / (float)BgTex.Height, 0);

								y += h;
							}

							x += w;
							bDoneY = false;
						}
					}
				}
			}
		}

		protected virtual void RenderClosing(float px, float py)
		{
			float AnimTime = Globals.g.RunningTime - AnimStartTime;
			
			// going over means we render nothing, since we're closing anyways
			if (AnimTime >= AnimClose_Step4)
			{
				UIManager.ui.DeleteElement(this);
				return;
			}

			Color color = new Color(bgcolor.ToArgb());

			if (AnimTime > AnimClose_Step3)
			{
				// fade out upper left corner
				AnimTime = 1f - (AnimTime - AnimClose_Step3) / (AnimClose_Step4 - AnimOpen_Step3);
				GL.Color4((byte)(color.R * AnimTime), (byte)(color.G * AnimTime), (byte)(color.B * AnimTime), color.A);
				Renderer.r.DrawUI(ULCornerTex, px, py, 1, 1, 0);
			}
			else
			{
				GL.Color4(bgcolor.R, bgcolor.G, bgcolor.B, bgcolor.A);
				Renderer.r.DrawUI(ULCornerTex, px, py, 1, 1, 0);
				if (AnimTime > AnimClose_Step2)
				{
					// fade out and move the close button in upper right
					AnimTime = 1f - (AnimTime - AnimClose_Step2) / (AnimClose_Step3 - AnimClose_Step2);
					GL.Color4((byte)(color.R * AnimTime), (byte)(color.G * AnimTime), (byte)(color.B * AnimTime), color.A);
					float start = px + ULCornerTex.Width - CloseButton.ButtonTex.Width;
					float end = px + CloseButton.Position.X;
					float l = start + (end - start) * (3 * AnimTime * AnimTime - 2 * AnimTime * AnimTime * AnimTime);
					Renderer.r.DrawUI(CloseButton.ButtonTex, l, py + CloseButton.Position.Y, 1, 1, 0);
					// fade in and draw the titlebar background behind the close button
					start += CloseButton.ButtonTex.Width;
					l -= CloseButton.ButtonTex.Width + start;
					Renderer.r.DrawUI(TBBgTex, start, py, l, 1, 0);
				}
				else
				{
					Renderer.r.DrawUI(CloseButton.ButtonTex, px + CloseButton.Position.X, py + CloseButton.Position.Y, 1, 1, 0);
					Renderer.r.DrawUI(TBBgTex, px + ULCornerTex.Width, py, Bounds.X - ULCornerTex.Width - CloseButton.Bounds.X, 1, 0);
					if (AnimTime > AnimClose_Step1)
					{
						// fade in and move the resize corners
						AnimTime = 1f - (AnimTime - AnimClose_Step1) / (AnimClose_Step2 - AnimClose_Step1);
						color.R = (byte)(color.R * AnimTime);
						color.G = (byte)(color.G * AnimTime);
						color.B = (byte)(color.B * AnimTime);
						GL.Color4(color.R, color.G, color.B, color.A);
						float start = py;
						float end = py + Bounds.Y - LLCornerTex.Height;
						float l = start + (end - start) * (3 * AnimTime * AnimTime - 2 * AnimTime * AnimTime * AnimTime);
						Renderer.r.DrawUI(LLCornerTex, px, l, 1, 1, 0);
						Renderer.r.DrawUI(LRCornerTex, px + Bounds.X - LRCornerTex.Width, l, 1, 1, 0);
						// draw the frame borders connecting them
						start += ULCornerTex.Height;
						l -= ULCornerTex.Height;
						Renderer.r.DrawLine(px + 1, start, px + 1, l, 1, color);
						Renderer.r.DrawLine(px + Bounds.X - 1, start, px + Bounds.X - 1, l, 1, color);
					}
					else
					{
						Renderer.r.DrawUI(LLCornerTex, px, py + Bounds.Y - LLCornerTex.Height, 1, 1, 0);
						Renderer.r.DrawUI(LRCornerTex, px + Bounds.X - LRCornerTex.Width, py + Bounds.Y - LRCornerTex.Height, 1, 1, 0);
						Renderer.r.DrawLine(px + 1, py + ULCornerTex.Height, px + 1, py + Bounds.Y - LLCornerTex.Height, 1, bgcolor);
						Renderer.r.DrawLine(px + Bounds.X - 1, py + ULCornerTex.Height, px + Bounds.X - 1, py + Bounds.Y - LLCornerTex.Height, 1, bgcolor);
						// draw the bottom border line growing in from the corners
						AnimTime = 1f - AnimTime / AnimClose_Step1;
						color.R = (byte)(color.R * AnimTime);
						color.G = (byte)(color.G * AnimTime);
						color.B = (byte)(color.B * AnimTime);
						float l = (Bounds.X - LLCornerTex.Width - LRCornerTex.Width) * 0.5f * (3 * AnimTime * AnimTime - 2 * AnimTime * AnimTime * AnimTime);
						float start = px + LLCornerTex.Width;
						int y = (int)(py + Bounds.Y - 1);
						Renderer.r.DrawLine(start, y, start + l, y, 1, color);
						start = px + Bounds.X - LRCornerTex.Width - l;
						Renderer.r.DrawLine(start, y, start + l, y, 1, color);
						// fade in the background as well
						GL.Color4(color.R, color.G, color.B, color.A);
						int ipx = (int)px;
						int ipy = (int)py + TBBgTex.Height;
						int x = ipx;
						int w;
						int h;
						bool bDoneX = false, bDoneY = false;

						while (!bDoneX)
						{
							y = ipy;

							// calculate the pixel width of this tile
							w = BgTex.Width - x % BgTex.Width;
							BgTex.UVs[0] = (BgTex.Width - w) / (float)BgTex.Width * UVwidth;
							// adjust for going beyond the border
							if ((x + w) >= (ipx + (int)Bounds.X))
							{
								w = ipx + (int)Bounds.X - x;
								// this allows us to short out of the entire tile process
								if (w <= 0) break;
								bDoneX = true;
								BgTex.UVs[2] = w / (float)BgTex.Width * UVwidth;
							}
							else BgTex.UVs[2] = UVwidth;

							BgTex.UVs[4] = BgTex.UVs[0];
							BgTex.UVs[6] = BgTex.UVs[2];

							while (!bDoneY)
							{
								// calculate the pixel height of this tile
								h = BgTex.Height - y % BgTex.Height;
								BgTex.UVs[1] = (BgTex.Height - h) / (float)BgTex.Height * UVheight;
								// adjust for going beyond the border
								if ((y + h) >= (ipy + (int)Bounds.Y - TBBgTex.Height))
								{
									bDoneY = true;
									h = ipy + (int)Bounds.Y - TBBgTex.Height - y;
									if (h <= 0) break;
									BgTex.UVs[5] = h / (float)BgTex.Height * UVheight;
								}
								else BgTex.UVs[5] = UVheight;

								BgTex.UVs[3] = BgTex.UVs[1];
								BgTex.UVs[7] = BgTex.UVs[5];

								// render this tile
								Renderer.r.DrawUI(BgTex, x, y, w / (float)BgTex.Width, h / (float)BgTex.Height, 0);

								y += h;
							}

							x += w;
							bDoneY = false;
						}
					}
				}
			}
		}

		public override void Render(float psx, float psy)
		{
			if (bOpening) RenderOpening(psx + Position.X, psy + Position.Y);
			else if (bClosing) RenderClosing(psx + Position.X, psy + Position.Y);
			else base.Render(psx, psy);
		}
	}
}

