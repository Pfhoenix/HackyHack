using System;
using OpenTK.Platform.Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using RPCoreLib;

namespace HackyHack
{
	public sealed class Globals
	{
		static readonly Globals globals = new Globals();
		public static Globals g
		{
			get { return globals; }
		}
		Globals() {}

		public Network PlayerNetwork;
	}
}

