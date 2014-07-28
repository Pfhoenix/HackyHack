using System.Collections.Generic;
using OpenTK.Graphics.ES11;
using Android.Graphics;
using Android.Views;

namespace HackyHack
{
	public class UITextEntryBox : UIElement
	{
		protected Font TextFont;
		public bool bSingleLine;
		public int MaxChars;
		public string Text
		{
			get { return TextChars.ToString(); }
		}
		readonly List<char> TextChars;
		readonly Vector2 Padding;

		public string DisplayText;

		bool bDrawTextCursor;
		float TextCursorTimer;
		int TextCursorIndex;
		float TextCursorPos;

		public UITextEntryBox()
		{
			TextFont = UIManager.ui.UIMediumTextFont;
			bSingleLine = true;
			TextChars = new List<char>();
			Padding = new Vector2(4, 4);
			Bounds.X = 10;
			Bounds.Y = TextFont.CharHeight + Padding.Y;
		}

		public override void Resize(float nw, float nh)
		{
			Bounds.X = nw;
			if (bSingleLine) Bounds.Y = TextFont.CharHeight + Padding.Y;
			else
			{
				if (nh < TextFont.CharHeight + Padding.Y) Bounds.Y = TextFont.CharHeight + Padding.Y;
				Bounds.Y = (int)nh / (int)(TextFont.CharHeight + Padding.Y) * (TextFont.CharHeight + Padding.Y);
			}
		}

		public override void ProcessKeyInput(Keycode key, KeyEvent e)
		{
			char c = (char)e.UnicodeChar;
			if (c == '\n')
			{
				// end capturing text input
				UIManager.ui.HideKeyboard();
			}
			else if (c != '\0')
			{
				TextChars.Insert(TextCursorIndex++, c);
				Vector2 v = TextFont.MeasureChar(c);
				TextCursorPos += v.X;
			}
			else if (key == Keycode.Del)
			{
				if (TextCursorIndex > 0)
				{
					Vector2 v = TextFont.MeasureChar(TextChars[TextCursorIndex]);
					TextCursorPos -= v.X;
					TextChars.RemoveAt(TextCursorIndex--);
				}
			}
			else if (key == Keycode.Back)
			{
				// end capturing text input
				UIManager.ui.HideKeyboard();
			}
		}

		public override bool ProcessInputEvent(EInputEvent ie, float x, float y, float px, float py)
		{
			if (base.ProcessInputEvent(ie, x, y, px, py))
			{
				if (UIManager.ui.KeyInputTrapper != this) UIManager.ui.ShowKeyboard(this);
				// need to detect where in the control the user tapped
				// if before the start of the text, then move the text cursor there
				if (x <= Padding.X)
				{
					TextCursorIndex = 0;
					TextCursorPos = Padding.X;
				}
				// else need to move character by character until at the character the user tapped
				else
				{
					float cx = x - Padding.X;
					TextCursorPos = Padding.X;
					Vector2 v;
					for (TextCursorIndex = 0; TextCursorIndex < TextChars.Count; TextCursorIndex++)
					{
						v = TextFont.MeasureChar(TextChars[TextCursorIndex]);
						if (v.X >= cx) break;
						TextCursorPos += v.X;
					}
				}

				// force a drawing of the txt cursor this frame for immediate feedback to the user
				bDrawTextCursor = true;

				return true;
			}

			if (UIManager.ui.KeyInputTrapper == this) UIManager.ui.HideKeyboard();

			return false;
		}

		protected override void UpdateMe(float dt)
		{
			if (UIManager.ui.KeyInputTrapper == this)
			{
				TextCursorTimer += dt;
				if (TextCursorTimer >= 0.5f)
				{
					bDrawTextCursor = !bDrawTextCursor;
					TextCursorTimer -= 0.5f;
				}
			}

			base.UpdateMe(dt);
		}

		protected override void RenderMe(float px, float py)
		{
			Color color = new Color((byte)(UIManager.ui.UIColor.R * 0.25f), (byte)(UIManager.ui.UIColor.G * 0.25f), (byte)(UIManager.ui.UIColor.B * 0.25f));

			// draw frame
			Renderer.r.DrawLine(px, py, px + Bounds.X, py, 1, UIManager.ui.UIColor);
			Renderer.r.DrawLine(px, py, px, py + Bounds.Y, 1, UIManager.ui.UIColor);
			Renderer.r.DrawLine(px + Bounds.X, py, px + Bounds.X, py + Bounds.Y, 1, UIManager.ui.UIColor);
			Renderer.r.DrawLine(px, py + Bounds.Y, px + Bounds.X, py + Bounds.Y, 1, UIManager.ui.UIColor);

			// init scissor rect for text drawing
			Rect ScissorRect = new Rect();
			ScissorRect.Left = (int)(px + Padding.X / 2);
			ScissorRect.Top = (int)(py + Padding.Y / 2);
			ScissorRect.Right = (int)(px + Bounds.X - Padding.X / 2);
			ScissorRect.Bottom = (int)(py + Bounds.Y - Padding.Y / 2);
			//UIManager.ui.SetMaskRect(ScissorRect);

			// draw text cursor
			if (bDrawTextCursor) Renderer.r.DrawLine(ScissorRect.Left + TextCursorPos, ScissorRect.Top, ScissorRect.Left + TextCursorPos, ScissorRect.Bottom, 1, Color.White);

			// draw text
			GL.Color4(UIManager.ui.UITextColor.R, UIManager.ui.UITextColor.G, UIManager.ui.UITextColor.B, 255);
			Renderer.r.DrawText(TextChars, UIManager.ui.UIMediumTextFont, ScissorRect.Left, ScissorRect.Top);

			//UIManager.ui.UnsetMaskRect();

			// draw background
			GL.Color4(color.R, color.G, color.B, 255);
			Renderer.r.DrawUI(Renderer.r.White, px, py, Bounds.X / 2, Bounds.Y / 2, 0);
		}
	}
}
