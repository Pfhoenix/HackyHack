using Android.Views;

namespace HackyHack
{
	public class UIElement
	{
		public UIElement Parent;
		protected UILinkedListNode Children;
		int _numchildren;
		public int NumChildren
		{
			get { return _numchildren; }
		}

		public Vector2 Position = new Vector2();
		public Vector2 Bounds = new Vector2();
		public bool bActive;
		public bool bVisible;
		public bool bAcceptsInput;
		public bool bAlwaysOnTop;
		public UIElement CapturingInput;	// used when an element wants to capture a continuous stream of input

		public bool bFlingAsScroll;
		public Vector2 FlingVelocity = new Vector2();
		public double FlingLastTime;
		public double FlingLastMoveTime;
		EInputEvent LockInputTo;

		bool bDestroyed;


		public UIElement()
		{
			bActive = true;
			bVisible = true;
			bAcceptsInput = true;
		}

		#region CHILD LIST MANAGEMENT
		public void AddChild(UIElement uie)
		{
			if (bDestroyed) return;

			if (uie.Parent != null) uie.Parent.RemoveChild(uie);
			uie.Parent = this;
			UILinkedListNode lln = UIManager.ui.ObtainNode();
			lln.Element = uie;
			if ((Children == null) || !Children.Element.bAlwaysOnTop)
			{
				lln.NextNode = Children;
				Children = lln;
			}
			else
			{
				// find the node whose next node's element is not bAlwaysOnTop
				UILinkedListNode tlln = Children;
				while (tlln.NextNode != null)
				{
					if (!tlln.NextNode.Element.bAlwaysOnTop) break;
					tlln = tlln.NextNode;
				}
				lln.NextNode = tlln.NextNode;
				tlln.NextNode = lln;
			}
			_numchildren++;
		}

		public virtual void RemoveChild(UIElement uie)
		{
			if (bDestroyed) return;

			if (CapturingInput == uie) SetCaptureInput(null);

			if (Children == null) return;
			if (Children.Element == uie)
			{
				UILinkedListNode lln = Children;
				Children = lln.NextNode;
				UIManager.ui.ReturnNode(lln);
				_numchildren--;
			}
			else
			{
				UILinkedListNode lln = Children;
				while (lln.NextNode != null)
				{
					if (lln.NextNode.Element == uie)
					{
						UILinkedListNode tlln = lln.NextNode;
						lln.NextNode = tlln.NextNode;
						UIManager.ui.ReturnNode(tlln);
						_numchildren--;
						return;
					}

					lln = lln.NextNode;
				}
			}
		}

		public void ClearChildren(bool bDestroyChild)
		{
			UILinkedListNode lln;
			while (Children != null)
			{
				if (bDestroyChild)
				{
					Children.Element.Parent = null;
					Children.Element.Destroy();
				}
				lln = Children.NextNode;
				UIManager.ui.ReturnNode(Children);
				Children = lln;
			}
			_numchildren = 0;
		}

		public void BringToFront()
		{
			if (Parent != null) Parent.BringChildToFront(this);
		}

		public void BringChildToFront(UIElement uie)
		{
			if (Children == null) return;
			if (Children.Element == uie) return;

			UILinkedListNode lln = Children;
			if ((uie.bAlwaysOnTop && Children.Element.bAlwaysOnTop) || !Children.Element.bAlwaysOnTop)
			{
				while (lln.NextNode != null)
				{
					if (lln.NextNode.Element == uie)
					{
						UILinkedListNode tlln = lln.NextNode;
						lln.NextNode = tlln.NextNode;
						tlln.NextNode = Children;
						Children = tlln;
						return;
					}

					lln = lln.NextNode;
				}
			}
			else
			{
				// we need to find the end of the bAlwaysOnTop group as well as the node uie is in
				UILinkedListNode tlln = lln;
				while (lln.NextNode != null)
				{
					if (lln.Element.bAlwaysOnTop) tlln = lln;
					if (lln.NextNode.Element == uie) break;
					lln = lln.NextNode;
				}
				if (tlln != lln)
				{
					UILinkedListNode ttlln = lln.NextNode;
					lln.NextNode = ttlln.NextNode;
					ttlln.NextNode = tlln.NextNode;
					tlln.NextNode = ttlln;
				}
			}
		}
		#endregion

		public virtual void ProcessScreenChanged()
		{
			UILinkedListNode lln = Children;
			while (lln != null)
			{
				lln.Element.ProcessScreenChanged();
				lln = lln.NextNode;
			}
		}

		public virtual void Resize(float nw, float nh)
		{
			Bounds.X = nw;
			Bounds.Y = nh;
		}

		public virtual bool IsPointIn(float x, float y, float px, float py)
		{
			if (x < Position.X + px) return false;
			if (y < Position.Y + py) return false;
			if (x > (Position.X + px + Bounds.X)) return false;
			if (y > (Position.Y + py + Bounds.Y)) return false;

			return true;
		}

		#region INPUT PROCESSING
		public virtual bool ProcessInputEvent(EInputEvent ie, float x, float y, float px, float py)
		{
			if (LockInputTo != EInputEvent.Release)
			{
				if ((ie != LockInputTo) && ((LockInputTo == EInputEvent.Scroll) && (ie != EInputEvent.Fling)) && (ie != EInputEvent.Release)) return true;
			}

			if ((ie == EInputEvent.Fling) && bFlingAsScroll)
			{
				FlingVelocity.Add(x, y);
				FlingLastTime = Globals.g.RunningTime;
				FlingLastMoveTime = FlingLastTime;
				return true;
			}
			if ((ie == EInputEvent.ScrollStart) || (ie == EInputEvent.ScaleStart))
			{
				SetCaptureInput(this);
				if (ie == EInputEvent.ScrollStart) LockInputTo = EInputEvent.Scroll;
				else if (ie == EInputEvent.ScaleStart) LockInputTo = EInputEvent.Scale;
				return true;
			}
			if (ie == EInputEvent.Release)
			{
				LockInputTo = EInputEvent.Release;
				if (CapturingInput == this) SetCaptureInput(null);
				return true;
			}

			return false;
		}

		void AnimateScrollFling(float dt)
		{
			bool bDone = false;

			Vector2 FrameFlingVel = FlingVelocity * dt;

			if ((FrameFlingVel.X != 0) || (FrameFlingVel.Y != 0))
			{
				ProcessInputEvent(EInputEvent.Scroll, FrameFlingVel.X, FrameFlingVel.Y, 0, 0);
				FlingLastMoveTime = Globals.g.RunningTime;
			}
			else
			{
				if (Globals.g.RunningTime - FlingLastMoveTime >= 0.250) bDone = true;
			}

			if (bDone && (dt > 0))
			{
				FlingVelocity.Set(0, 0);
			}
			else
			{
				if (FlingVelocity.X > 0)
				{
					FlingVelocity.X -= FrameFlingVel.X * UIManager.ui.FlingSlowdownScale;
					if (FlingVelocity.X <= 0) FlingVelocity.X = 0;
				}
				else
				{
					FlingVelocity.X -= FrameFlingVel.X * UIManager.ui.FlingSlowdownScale;
					if (FlingVelocity.X >= 0) FlingVelocity.X = 0;
				}

				if (FlingVelocity.Y > 0)
				{
					FlingVelocity.Y -= FrameFlingVel.Y * UIManager.ui.FlingSlowdownScale;
					if (FlingVelocity.Y <= 0) FlingVelocity.Y = 0;
				}
				else
				{
					FlingVelocity.Y -= FrameFlingVel.Y * UIManager.ui.FlingSlowdownScale;
					if (FlingVelocity.Y >= 0) FlingVelocity.Y = 0;
				}

				if (!FlingVelocity.IsZero()) FlingLastTime = Globals.g.RunningTime;
			}
		}

		public virtual void SetCaptureInput(UIElement e)
		{
			CapturingInput = e;
			if (Parent != null) Parent.SetCaptureInput(e);
		}

		public virtual bool FindChildForEvent(InputEventInfo iei, float px, float py)
		{
			if (IsPointIn(iei.X, iei.Y, px, py))
			{
				if ((NumChildren == 0) && bAcceptsInput)
				{
					ProcessInputEvent(iei.InputEvent, iei.X, iei.Y, px + Position.X, py + Position.Y);
					return true;
				}

				UILinkedListNode lln = Children;
				while (lln != null)
				{
					if (lln.Element.FindChildForEvent(iei, px + Position.X, py + Position.Y))
						return true;
					lln = lln.NextNode;
				}

				if (bAcceptsInput) ProcessInputEvent(iei.InputEvent, iei.X, iei.Y, px + Position.X, py + Position.Y);
				return bAcceptsInput;
			}

			return false;
		}

		public virtual void ProcessKeyInput(Keycode key, KeyEvent e) { }
		#endregion
		
		#region UPDATING
		protected virtual void UpdateMe(float dt) { }

		public void Update(float dt)
		{
			if (!bActive) return;

			// we process flinging here so that children see an up-to-date position, if it's changing
			if (bFlingAsScroll && !FlingVelocity.IsZero()) AnimateScrollFling(dt);

			UILinkedListNode lln = Children;
			while (lln != null)
			{
				lln.Element.Update(dt);
				lln = lln.NextNode;
			}

			UpdateMe(dt);
		}
		#endregion

		#region RENDERING
		protected virtual void RenderMe(float px, float py)
		{
		}

		// psx and psy == Parent Screen X/Y
		public virtual void Render(float psx, float psy)
		{
			if (!bVisible) return;

			float cpsx = psx + Position.X;
			float cpsy = psy + Position.Y;

			if (Children != null)
			{

				UILinkedListNode lln = Children;
				while (lln != null)
				{
					lln.Element.Render(cpsx, cpsy);
					lln = lln.NextNode;
				}
			}

			RenderMe(cpsx, cpsy);
		}
		#endregion

		public virtual void Destroy()
		{
			bDestroyed = true;

			if (Parent != null)
			{
				Parent.RemoveChild(this);
				Parent = null;
			}

			SetCaptureInput(null);

			ClearChildren(true);
		}
	}
}
