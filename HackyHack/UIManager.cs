using System.Collections;
using System.Collections.Generic;
using Android.Graphics;
using Android.Views;
using Android.Views.InputMethods;
//using Android.InputMethodServices;


namespace HackyHack
{
	// this is purposefully lightweight, in order to reduce the memory footprint of the entire structure
	public sealed class UILinkedListNode : IEnumerable<UIElement>
	{
		public UIElement Element;
		public UILinkedListNode NextNode;

		public IEnumerator<UIElement> GetEnumerator()
		{
			UILinkedListNode lln = this;
			while (lln != null)
			{
				yield return lln.Element;
				lln = lln.NextNode;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public enum EInputEvent { Release = 0, SingleTap, LongPress, ScrollStart, Scroll, ScaleStart, Scale, Fling };

	public sealed class InputEventInfo
	{
		public EInputEvent InputEvent;
		public float X, Y;

		public InputEventInfo(EInputEvent ie, float x, float y)
		{
			Set(ie, x, y);
		}

		public void Set(EInputEvent ie, float x, float y)
		{
			InputEvent = ie;
			X = x;
			Y = y;
		}

		public void Clear()
		{
			InputEvent = EInputEvent.Release;
		}
	}

	public sealed class UIManager
	{
		readonly static UIManager _ui = new UIManager();
		public static UIManager ui
		{
			get { return _ui; }
		}

		public UIRoot Root;

		UILinkedListNode UINodePool;
		
		readonly Queue<InputEventInfo> InputEventQueue;
		readonly Stack<InputEventInfo> InputEventPool;
		InputEventInfo SpecialReleasePeek; // used to prevent redundant releasing prior to flinging

		public int FlingSlowdownScale = 5;
		readonly List<UIElement> DeleteList;

		public Color UIColor = new Color(0, 255, 0);
		public Color UITextColor = new Color(255, 255, 255);

		public Font UIMediumTextFont;

		Rect CurrentMaskRect;
		readonly Stack<Rect> MaskRectStack;

		InputMethodManager IMM;
		public bool bTrapKeyInput;
		public UIElement KeyInputTrapper;

		UIManager()
		{
			InputEventQueue = new Queue<InputEventInfo>(10);
			InputEventPool = new Stack<InputEventInfo>(10);
			DeleteList = new List<UIElement>();
			MaskRectStack = new Stack<Rect>(5);
		}

		public void InitForScreen(float x, float y)
		{
			UIMediumTextFont = ContentManager.cm.GetFont("slider");
			if (Root == null) Root = new UIRoot();
			ScreenChanged();
			IMM = (InputMethodManager)Globals.g.App.GetSystemService(InputMethod.ServiceInterface);
		}

		public void ScreenChanged()
		{
			if (Root != null) Root.ProcessScreenChanged();
		}

		public void DeleteElement(UIElement uie)
		{
			uie.bActive = false;
			uie.bAcceptsInput = false;
			uie.bVisible = false;
			DeleteList.Add(uie);
		}

		void ProcessDeleteList()
		{
			foreach (UIElement uie in DeleteList)
			{
				uie.Destroy();
			}

			DeleteList.Clear();
		}

		#region NODE POOL MANAGEMENT
		public UILinkedListNode ObtainNode()
		{
			if (UINodePool == null) return new UILinkedListNode();

			UILinkedListNode lln = UINodePool;
			UINodePool = lln.NextNode;
			return lln;
		}

		public void ReturnNode(UILinkedListNode lln)
		{
			lln.Element = null;
			lln.NextNode = UINodePool;
			UINodePool = lln;
		}
		#endregion

		#region INPUT EVENTS
		public void ProcessInputEvents()
		{
			InputEventInfo iei;
			while (InputEventQueue.Count > 0)
			{
				iei = InputEventQueue.Dequeue();

				// find the element to handle this event
				if (Root.CapturingInput != null)
				{
					Root.CapturingInput.ProcessInputEvent(iei.InputEvent, iei.X, iei.Y, 0, 0);
				}
				else if (iei.InputEvent != EInputEvent.Release) Root.FindChildForEvent(iei, 0, 0);

				iei.Clear();
				InputEventPool.Push(iei);
			}

			SpecialReleasePeek = null;
		}

		void HandleInputEvent(EInputEvent ie, float x, float y)
		{
			InputEventInfo iei;
			if (InputEventPool.Count == 0) iei = new InputEventInfo(ie, x, y);
			else
			{
				iei = InputEventPool.Pop();
				iei.Set(ie, x, y);
			}

			if (iei != null)
			{
				if (SpecialReleasePeek == null)
				{
					if (iei.InputEvent == EInputEvent.Release) SpecialReleasePeek = iei;
					InputEventQueue.Enqueue(iei);
				}
				else if (iei.InputEvent == EInputEvent.Fling)
				{
					SpecialReleasePeek.InputEvent = EInputEvent.Fling;
					SpecialReleasePeek.X = iei.X;
					SpecialReleasePeek.Y = iei.Y;
					iei.InputEvent = EInputEvent.Release;
					iei.X = 0;
					iei.Y = 0;
					InputEventQueue.Enqueue(iei);
				}
				else
				{
					SpecialReleasePeek = null;
					InputEventQueue.Enqueue(iei);
				}
			}
		}

		public void HandleSingleTap(float x, float y)
		{
			HandleInputEvent(EInputEvent.SingleTap, x, y);
		}

		public void HandleLongPress(float x, float y)
		{
			HandleInputEvent(EInputEvent.LongPress, x, y);
		}

		public void HandleScrollStart(float x, float y)
		{
			HandleInputEvent(EInputEvent.ScrollStart, x, y);
		}

		public void HandleScrollChange(float x, float y)
		{
			HandleInputEvent(EInputEvent.Scroll, x, y);
		}

		public void HandleScaleStart(float x, float y)
		{
			HandleInputEvent(EInputEvent.ScaleStart, x, y);
		}

		public void HandleScaleChange(float s)
		{
			HandleInputEvent(EInputEvent.Scale, s, 0);
		}

		public void HandleFling(float x, float y)
		{
			HandleInputEvent(EInputEvent.Fling, x, y);
		}

		public void HandleRelease()
		{
			HandleInputEvent(EInputEvent.Release, 0, 0);
		}

		public void HandleKeyInput(Keycode code, KeyEvent e)
		{
			if (KeyInputTrapper != null) KeyInputTrapper.ProcessKeyInput(code, e);
		}
		#endregion

		#region MASK MANAGEMENT
		public Rect GetCurrentMaskRect()
		{
			return CurrentMaskRect;
		}

		public void SetMaskRect(Rect r)
		{
			if (CurrentMaskRect == null) Renderer.r.EnableScissor();
			else MaskRectStack.Push(CurrentMaskRect);
			CurrentMaskRect = r;

			Renderer.r.SetScissor(r.Left, r.Bottom, r.Width(), r.Height());
		}

		public void UnsetMaskRect()
		{
			if (MaskRectStack.Count == 0)
			{
				CurrentMaskRect = null;
				Renderer.r.DisableScissor();
			}
			else
			{
				CurrentMaskRect = MaskRectStack.Pop();
				Renderer.r.SetScissor(CurrentMaskRect.Left, CurrentMaskRect.Top, CurrentMaskRect.Width(), CurrentMaskRect.Height());
			}
		}
		#endregion

		#region Keyboard Management
		public void ShowKeyboard(UIElement trapper)
		{
			bTrapKeyInput = true;
			KeyInputTrapper = trapper;
			if (Globals.g.App.Resources.Configuration.HardKeyboardHidden == Android.Content.Res.HardKeyboardHidden.No) return;
			IMM.ShowSoftInput(Globals.g.AGView, ShowFlags.Implicit);
		}

		public void HideKeyboard()
		{
			bTrapKeyInput = false;
			KeyInputTrapper = null;
			IMM.HideSoftInputFromWindow(Globals.g.AGView.WindowToken, HideSoftInputFlags.ImplicitOnly);
		}
		#endregion

		public void Update(float dt)
		{
			Root.Update(dt);
			ProcessDeleteList();
		}

		public void Render()
		{
			Root.Render(0, 0);
		}
	}
}

