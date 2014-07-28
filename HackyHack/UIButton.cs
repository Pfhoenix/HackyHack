using System;
using OpenTK.Graphics.ES11;
using Android.Graphics;

namespace HackyHack
{
	public class UIButton : UIElement
	{
		public delegate void Delegate_ButtonPressed(UIButton uib);
		public event Delegate_ButtonPressed Pressed;

		public Texture ButtonTex;
		public Color TexColor;
		
		public int NumFlashes;
		protected int FlashNum;
		protected float FlashTime;
		protected bool bFlash;

		public UIButton()
		{
			NumFlashes = 2;
		}

		public virtual void Setup(Texture t, Color c)
		{
			ButtonTex = t;
			Bounds.X = t.Width;
			Bounds.Y = t.Height;
			TexColor = new Color(c.ToArgb());
			TexColor.A = (byte)255;
		}

		public override bool ProcessInputEvent(EInputEvent ie, float x, float y, float px, float py)
		{
			if (ie == EInputEvent.SingleTap)
			{
				FlashNum = NumFlashes;
				if (Pressed != null) Pressed(this);
				else ButtonPressed();
			}

			return true;
		}

		public virtual void ButtonPressed()
		{
		}

		protected override void UpdateMe(float dt)
		{
			if (FlashNum > 0)
			{
				FlashTime -= dt;
				if (FlashTime <= 0)
				{
					bFlash = !bFlash;
					if (!bFlash) FlashNum--;
					FlashTime = 0.075f;
				}
			}
		}

		protected override void RenderMe(float px, float py)
		{
			if (ButtonTex == null) return;

			if (bFlash) GL.Color4(10, 10, 10, 1f);
			else GL.Color4(TexColor.R, TexColor.G, TexColor.B, TexColor.A);
			Renderer.r.DrawUI(ButtonTex, px, py, 1, 1, 0);
		}
	}
}

