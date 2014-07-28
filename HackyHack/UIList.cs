using System.Collections.Generic;
using OpenTK.Graphics.ES11;
using Android.Graphics;

namespace HackyHack
{
	public class UIListItem
	{
		public string DisplayText;
		public float DisplaySize;
	}


	public class UIVerticalList : UIList
	{
		protected float ItemHeight;

		public Color BackgroundColor;
		public Color ItemTextColor;

		public UIVerticalList(string fontname) : base(fontname)
		{
			ItemPadding.Set(4, 4);
			ItemHeight = TextFont.CharHeight + ItemPadding.Y * 2;
		}

		protected override void ProcessSingleTap(float x, float y, float px, float py)
		{
			float sy = -ScrollPos.Y;
			foreach (UIListItem item in Items)
			{
				if ((y >= sy) && (y <= sy + ItemHeight))
				{
					ItemTapped(item);
					break;
				}
				sy += ItemHeight;
			}
		}

		protected override void ProcessScroll(float x, float y)
		{
			ScrollPos.Y -= y;
			if (ScrollPos.Y + ItemScreenSpaceSize.Y > TotalItemsSize.Y) ScrollPos.Y = TotalItemsSize.Y - ItemScreenSpaceSize.Y;
			if (ScrollPos.Y < 0) ScrollPos.Y = 0;
		}

		protected override void RecalculateTotalItemsSize()
		{
			TotalItemsSize.X = 0;
			TotalItemsSize.Y = ItemHeight * Items.Count;
		}

		protected override void RenderItem(UIListItem item, float x, float y)
		{
			GL.Color4(ItemTextColor.R, ItemTextColor.G, ItemTextColor.B, 255);
			Renderer.r.DrawText(item.DisplayText, TextFont, x + ItemPadding.X, y + ItemPadding.Y);
		}

		protected override void RenderItems(float px, float py)
		{
			float ItemPos = 0;
			for (int i = 0; i < Items.Count; i++)
			{
				if (ItemPos >= ItemScreenSpaceSize.Y + ScrollPos.Y) break;
				if ((ItemPos + ItemHeight) < ScrollPos.Y)
				{
					ItemPos += ItemHeight;
					continue;
				}

				RenderItem(Items[i], px, py - ScrollPos.Y + ItemPos);

				ItemPos += ItemHeight;
			}
		}

		protected override void RenderBackground(float px, float py)
		{
			GL.Color4(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, 255);
			Renderer.r.DrawUI(Renderer.r.White, px, py, Bounds.X / 2, Bounds.Y / 2, 0);
		}
	}


	public abstract class UIList : UIElement
	{
		const float AutoScrollTimeFrame = 1f;

		protected List<UIListItem> Items;
		protected Font TextFont;
		protected Vector2 TotalItemsSize;
		protected Vector2 ItemScreenSpaceSize;
		public Vector2 ItemPadding;

		protected Vector2 ScrollPos;
		protected Vector2 AutoScrollTo;
		protected Vector2 AutoScrollStart;
		protected float AutoScrollTime;

		protected Rect ScissorRect;

		protected UIList(string fontname)
		{
			bFlingAsScroll = true;
			Items = new List<UIListItem>();
			TextFont = ContentManager.cm.GetFont(fontname);
			ItemPadding = new Vector2();
		}

		protected abstract void RecalculateTotalItemsSize();

		protected virtual void ResetWithParent()
		{
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

		protected virtual void ItemTapped(UIListItem Item)
		{
		}

		protected virtual void ProcessSingleTap(float x, float y, float px, float py)
		{
		}

		protected virtual void ProcessScroll(float x, float y)
		{
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
					ProcessSingleTap(x, y, px, py);
					return true;

					case EInputEvent.Scroll:
					if (AutoScrollTime > Globals.g.RunningTime) return true;
					ProcessScroll(x, y);
					return true;
			}

			return false;
		}

		protected override void UpdateMe(float dt)
		{
			if (AutoScrollTime > 0)
			{
				ScrollPos = Globals.g.Smooth_Interp_Vector(AutoScrollStart, AutoScrollTo, AutoScrollTime - AutoScrollTimeFrame, AutoScrollTime);
				if (AutoScrollTime < Globals.g.RunningTime) AutoScrollTime = 0;
			}
		}

		protected virtual void RenderItem(UIListItem item, float x, float y)
		{
		}

		protected abstract void RenderItems(float px, float py);
		protected abstract void RenderBackground(float px, float py);

		protected override void RenderMe(float px, float py)
		{
			ScissorRect.Left = (int)px;
			ScissorRect.Top = (int)py;
			ScissorRect.Right = (int)(px + Bounds.X);
			ScissorRect.Bottom = (int)(py + Bounds.Y);
			UIManager.ui.SetMaskRect(ScissorRect);
			RenderItems(px, py);
			RenderBackground(px, py);
			UIManager.ui.UnsetMaskRect();
		}
	}
}

