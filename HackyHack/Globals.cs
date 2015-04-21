
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

