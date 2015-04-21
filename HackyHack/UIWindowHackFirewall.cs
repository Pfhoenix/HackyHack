using OpenTK.Graphics.ES11;
using Android.Graphics;
using RPCoreLib;

namespace HackyHack
{
	public class UIWindowHackFirewall : UIWindow
	{
		struct FirewallCell
		{
			public bool Flashing;
			public float FlashStartTime;
			public float FlashEndTime;
			public string Value;
		}
		
		readonly Firewall Hacking;

		static Vector2 GridOrigin = new Vector2(3, 3);
		static readonly Vector2 CellPadding = new Vector2(3, 0);
		static readonly char[] CellValues = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
		static Vector2 CellCharSize;
		static Font CellFont;

		readonly FirewallCell[,] Cells;
		Vector2 CellSize;
		Vector2 GridSize;
		Vector2 BarSize;
		Vector2 BarOrigin;

		public UIWindowHackFirewall(Firewall fw)
		{
			if (CellFont == null)
			{
				CellFont = ContentManager.cm.GetFont("mono-large");
				CellCharSize = CellFont.MeasureText("00");
				GridOrigin.Y += TBBgTex.Height;
			}

			Title = "Firewall Bypass";

			Hacking = fw;
			if (Hacking.GeneralActivity == 1) Cells = new FirewallCell[12,9];
			else if (Hacking.GeneralActivity == 2) Cells = new FirewallCell[20,15];
			else if (Hacking.GeneralActivity == 3) Cells = new FirewallCell[36,27];
			CellSize = CellCharSize + CellPadding * 2;
			CellSize.X += 3;
			CellSize.Y += 3;
			GridSize = CellSize;
			GridSize.Multiply(Cells.GetLength(0), Cells.GetLength(1));
			BarSize = new Vector2(GridSize.X * 0.75f, CellCharSize.Y + 4);
			BarOrigin = new Vector2(GridOrigin.X + (GridSize.X - BarSize.X) / 2, GridOrigin.Y + GridSize.Y + 5);

			Bounds.X = GridOrigin.X * 2 + GridSize.X;
			Bounds.Y = GridOrigin.Y * 2 + GridSize.Y + BarSize.Y - 2;
		}

		protected override void RenderMe(float px, float py)
		{
			// draw all grid lines
			Color gridlinecolor = new Color(96,96,96);
			Vector2 ls, le;
			// vertical
			for (int i = 0; i <= Cells.GetLength(0); i++)
			{
				ls.X = GridOrigin.X + i * CellSize.X + Position.X;
				ls.Y = GridOrigin.Y + Position.Y;
				le.X = ls.X;
				le.Y = ls.Y + GridSize.Y;
				Renderer.r.DrawLine(ls.X, ls.Y, le.X, le.Y, 1, gridlinecolor);
			}
			// horizontal
			for (int i = 0; i <= Cells.GetLength(1); i++)
			{
				ls.X = GridOrigin.X + Position.X;
				ls.Y = GridOrigin.Y + i * CellSize.Y + Position.Y;
				le.X = ls.X + GridSize.X;
				le.Y = ls.Y;
				Renderer.r.DrawLine(ls.X, ls.Y, le.X, le.Y, 1, gridlinecolor);
			}

			// iterate over cells
			string s;
			for (int i = 0; i < Cells.GetLength(0); i++)
				for (int j = 0; j < Cells.GetLength(1); j++)
				{
					s = "" + CellValues[RPGlobals.g.RNG.Next(CellValues.Length)] + CellValues[RPGlobals.g.RNG.Next(CellValues.Length)];
					Renderer.r.DrawText(s, CellFont, Position.X + GridOrigin.X + i * CellSize.X + CellPadding.X, Position.Y + GridOrigin.Y + j * CellSize.Y + CellPadding.Y);
				}

			// draw progress bar
			ls.X = BarOrigin.X + Position.X;
			ls.Y = BarOrigin.Y + Position.Y;
			le.X = ls.X;
			le.Y = ls.Y + BarSize.Y;
			Renderer.r.DrawLine(ls.X, ls.Y, le.X, le.Y, 1, gridlinecolor);
			le.X = ls.X + BarSize.X;
			le.Y = ls.Y;
			Renderer.r.DrawLine(ls.X, ls.Y, le.X, le.Y, 1, gridlinecolor);
			le.Y = ls.Y + BarSize.Y;
			ls.Y += BarSize.Y;
			Renderer.r.DrawLine(ls.X, ls.Y, le.X, le.Y, 1, gridlinecolor);
			ls.X += BarSize.X;
			ls.Y = BarOrigin.Y + Position.Y;
			Renderer.r.DrawLine(ls.X, ls.Y, le.X, le.Y, 1, gridlinecolor);

			base.RenderMe(px, py);
		}
	}
}

