using System;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Platform;
using OpenTK.Platform.Android;

namespace HackyHack
{
	public class TestElement : UIElement
	{
		float TestRot;
		float TestX;
		float TestY;
		float Scale = 50;
		byte[] square_colors = {
			255, 255, 0, 255,
			0, 255, 255, 255,
			0, 0, 0, 0,
			255, 0, 255, 255,
		};

		public override bool ProcessInputEvent(EInputEvent ie, float x, float y, float px, float py)
		{
			if (base.ProcessInputEvent(ie, x, y, px, py)) return true;

			switch (ie)
			{
				case EInputEvent.SingleTap:
					TestX = x;
					TestY = y;
					FlingVelocity.Set(0, 0);
					return true;

				case EInputEvent.Scroll:
					TestX += x;
					TestY += y;
					if (TestX < 0) TestX = 0;
					else if (TestX > Parent.Bounds.X) TestX = Parent.Bounds.X;
					if (TestY < 0) TestY = 0;
					else if (TestY > Parent.Bounds.Y) TestY = Parent.Bounds.Y;
					return true;

				case EInputEvent.Scale:
					Scale *= x;
					if (Scale < 1) Scale = 1;
					else if (Scale > 500) Scale = 500;
					return true;
			}

			return false;
		}

		protected override void UpdateMe(float dt)
		{
			TestRot += 45 * dt;
		}

		protected override void RenderMe(float px, float py)
		{
			//GL.ColorPointer(4, All.UnsignedByte, 0, square_colors);
			//GL.EnableClientState(All.ColorArray);
			GL.Color4(1f, 0, 0, 1f);
			//Renderer.r.DrawSprite(TestX, TestY, Scale, TestRot);
		}
	}
}

