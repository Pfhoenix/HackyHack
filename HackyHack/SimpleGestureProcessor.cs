using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;


namespace HackyHack
{
	public class SimpleGestureProcessor : Java.Lang.Object, GestureDetector.IOnGestureListener, ScaleGestureDetector.IOnScaleGestureListener
	{
		GestureDetector gd;
		ScaleGestureDetector sgd;

		bool bTrappingInput;
		
		public delegate void Delegate_HandleSingleTap(float x, float y);
		public event Delegate_HandleSingleTap HandleSingleTap;

		public delegate void Delegate_HandleLongPress(float x, float y);
		public event Delegate_HandleLongPress HandleLongPress;

		public delegate void Delegate_HandleScrollStart(float dx, float dy);
		public event Delegate_HandleScrollStart HandleScrollStart;

		public delegate void Delegate_HandleScrollChange(float dx, float dy);
		public event Delegate_HandleScrollChange HandleScrollChange;

		public delegate void Delegate_HandleScaleStart(float dx, float dy);
		public event Delegate_HandleScaleStart HandleScaleStart;

		public delegate void Delegate_HandleScaleChange(float s);
		public event Delegate_HandleScaleChange HandleScaleChange;

		public delegate void Delegate_HandleFling(float vx, float y);
		public event Delegate_HandleFling HandleFling;

		public delegate void Delegate_HandleRelease();
		public event Delegate_HandleRelease HandleRelease;

		//------

		public SimpleGestureProcessor(Context context)
		{
			gd = new GestureDetector(context, this);
			sgd = new ScaleGestureDetector(context, this);
		}

		public bool OnTouchEvent(MotionEvent e)
		{
			// if two or more pointers are listed, then we're either starting, continuing, or ending scaling
			if (e.PointerCount > 1) return sgd.OnTouchEvent(e);
			// otherwise we're doing a single touch gesture
			else
			{
				if (e.Action == MotionEventActions.Up)
				{
					bTrappingInput = false;
					if (HandleRelease != null) HandleRelease();
				}
				return gd.OnTouchEvent(e);
			}
		}

		public bool OnDown(MotionEvent e)
		{
			return true;
		}

		public void OnShowPress(MotionEvent e)
		{
		}

		public bool OnSingleTapUp(MotionEvent e)
		{
			if (HandleSingleTap != null) HandleSingleTap(e.GetX(), e.GetY());
			return true;
		}

		public void OnLongPress(MotionEvent e)
		{
			if (HandleLongPress != null) HandleLongPress(e.GetX(), e.GetY());
		}

		public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
		{
			if (!bTrappingInput)
			{
				bTrappingInput = true;
				if (HandleScrollStart != null) HandleScrollStart(e1.GetX(), e1.GetY());
			}
			if (HandleScrollChange != null) HandleScrollChange(-distanceX, -distanceY);

			return true;
		}

		public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
		{
			if (HandleFling != null) HandleFling(velocityX, velocityY);

			return true;
		}

		public bool OnScale(ScaleGestureDetector sgd)
		{
			if (sgd.ScaleFactor != 0)
			{
				if (!bTrappingInput)
				{
					bTrappingInput = true;
				}
				if (HandleScaleChange != null) HandleScaleChange(sgd.ScaleFactor);
				return true;
			}

			return false;
		}

		public bool OnScaleBegin(ScaleGestureDetector sgd)
		{
			bTrappingInput = true;
			if (HandleScaleStart != null) HandleScaleStart(sgd.FocusX, sgd.FocusY);
			return true;
		}
		public void OnScaleEnd(ScaleGestureDetector sgd) { }
	}
}

