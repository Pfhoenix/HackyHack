using System;
using OpenTK.Platform.Android;
using Android.App;
using Android.Content;
using Android.Graphics;

namespace HackyHack
{
	public sealed class Globals
	{
		readonly static Globals globals = new Globals();
		public static Globals g
		{
			get { return globals; }
		}
		Globals() {}

		public Activity App;
		public string AppName;
		public float RunningTime;
		public Random RNG = new Random();
		public bool bQuit;


		public delegate Vector2 Delegate_Interpolation(Vector2 Start, Vector2 End, float StartTime, float EndTime);

		public Vector2 Linear_Interp_Vector(Vector2 Start, Vector2 End, float StartTime, float EndTime)
		{
			if (RunningTime <= StartTime) return new Vector2(Start);
			if (RunningTime >= EndTime) return new Vector2(End);
			return (End - Start) * (RunningTime - StartTime) / (EndTime - StartTime) + Start;
		}

		public Vector2 Smooth_Interp_Vector(Vector2 Start, Vector2 End, float StartTime, float EndTime)
		{
			if (RunningTime <= StartTime) return new Vector2(Start);
			if (RunningTime >= EndTime) return new Vector2(End);

			float x = (RunningTime - StartTime) / (EndTime - StartTime);
			return (End - Start) * (3 * x * x - 2 * x * x * x) + Start;
		}

		public float Linear_Interp_Float(float Start, float End, float StartTime, float EndTime)
		{
			if (RunningTime <= StartTime) return Start;
			if (RunningTime >= EndTime) return End;

			return (End - Start) * (RunningTime - StartTime) / (EndTime - StartTime) + Start;
		}

		public float Smooth_Interp_Float(float Start, float End, float StartTime, float EndTime)
		{
			if (RunningTime <= StartTime) return Start;
			if (RunningTime >= EndTime) return End;

			float x = (RunningTime - StartTime) / (EndTime - StartTime);
			return (End - Start) * (3 * x * x - 2 * x * x * x) + Start;
		}

		public Color Linear_Interp_Color(Color Start, Color End, float StartTime, float EndTime)
		{
			if (RunningTime <= StartTime) return new Color(Start.ToArgb());
			if (RunningTime >= EndTime) return new Color(End.ToArgb());

			Color c = new Color();
			float x = (RunningTime - StartTime) / (EndTime - StartTime);
			c.R = (byte)((End.R - Start.R) * x + Start.R);
			c.G = (byte)((End.G - Start.G) * x + Start.G);
			c.B = (byte)((End.B - Start.B) * x + Start.B);
			c.A = 255;

			return c;
		}

		public Color Smooth_Interp_Color(Color Start, Color End, float StartTime, float EndTime)
		{
			if (RunningTime <= StartTime) return new Color(Start.ToArgb());
			if (RunningTime >= EndTime) return new Color(End.ToArgb());

			Color c = new Color();
			float x = (RunningTime - StartTime) / (EndTime - StartTime);
			x = 3 * x * x - 2 * x * x * x;
			c.R = (byte)((End.R - Start.R) * x + Start.R);
			c.R = (byte)((End.G - Start.G) * x + Start.G);
			c.R = (byte)((End.B - Start.B) * x + Start.B);
			c.A = 255;

			return c;
		}

		public GLView1 AGView;
		public Context GameContext;
	}
}

