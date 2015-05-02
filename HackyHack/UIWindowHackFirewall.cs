using System.Collections.Generic;
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
			public bool Valid;
			public bool Error;
		}
		
		readonly Firewall Hacking;

		static Vector2 GridOrigin = new Vector2(3, 3);
		static readonly Vector2 CellPadding = new Vector2(3, 0);
		static readonly char[] CellValues = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
		static Vector2 CellCharSize;
		static Font CellFont;
		static Font TextFont;
		static Color GridLineColor = new Color(96, 96, 96, 255);
		static Color FlashColor = new Color(160, 160, 160, 255);
		static Color ValidColor = new Color(96, 96, 190, 255);
		static Color ErrorColor = new Color(160, 96, 96, 255);

		readonly FirewallCell[,] Cells;
		Vector2 CellSize;
		Vector2 GridSize;
		Vector2 BarSize;
		Vector2 BarOrigin;

		readonly List<int> OpenCells;
		int NumCellsLocked;
		float BypassProgress;
		float ProgressPerCell = 0.25f;

		public UIWindowHackFirewall(Firewall fw)
		{
			if (CellFont == null)
			{
				CellFont = ContentManager.cm.GetFont("mono-large");
				CellCharSize = CellFont.MeasureText("00");
				GridOrigin.Y += TBBgTex.Height;
				TextFont = ContentManager.cm.GetFont("default");
			}

			Title = "Firewall Bypass";

			Hacking = fw;
			if (Hacking.Size == 1) Cells = new FirewallCell[6,4];
			else if (Hacking.Size == 2) Cells = new FirewallCell[10,6];
			else if (Hacking.Size == 3) Cells = new FirewallCell[14,8];
			CellSize = CellCharSize + CellPadding * 2;
			CellSize.X += 3;
			CellSize.Y += 3;
			GridSize = CellSize;
			GridSize.Multiply(Cells.GetLength(0), Cells.GetLength(1));
			BarSize = new Vector2(GridSize.X * 0.75f, CellCharSize.Y / 2);
			BarOrigin = new Vector2(GridOrigin.X + (GridSize.X - BarSize.X) / 2, GridOrigin.Y + GridSize.Y + TextFont.CharHeight + 5);

			Bounds.X = GridOrigin.X * 2 + GridSize.X;
			Bounds.Y = GridOrigin.Y * 2 + GridSize.Y + TextFont.CharHeight + BarSize.Y - 2;

			OpenCells = new List<int>(Cells.GetLength(0) * Cells.GetLength(1));
			for (int i = 0; i < OpenCells.Capacity; i++)
				OpenCells.Add(i);
		}

		public override bool ProcessInputEvent(EInputEvent ie, float x, float y, float px, float py)
		{
			if ((x >= px + GridOrigin.X) && (x <= px + GridOrigin.X + GridSize.X) &&
				(y >= py + GridOrigin.Y) && (y <= py + GridOrigin.Y + GridSize.Y))
			{
				int i = (int)(x - px - GridOrigin.X) / (int)CellSize.X;
				int j = (int)(y - py - GridOrigin.Y) / (int)CellSize.Y;
				if (Cells[i,j].Flashing)
				{
					if (Cells[i,j].Valid)
					{
						Cells[i,j].Flashing = false;
						Cells[i,j].Valid = false;
						OpenCells.Add(i + j * Cells.GetLength(0));
						NumCellsLocked--;
					}
					else if (Cells[i,j].Error)
					{
					}
					else
					{
						NumCellsLocked++;
						Cells[i,j].Valid = true;
						Cells[i,j].FlashStartTime = RPGlobals.g.RunningTime;
						Cells[i,j].FlashEndTime = RPGlobals.g.RunningTime + 2f;
					}
				}
				else if (!Cells[i,j].Error)
				{
					Cells[i,j].Error = true;
					OpenCells.Remove(i + j * Cells.GetLength(0));
				}

				return true;
			}

			return base.ProcessInputEvent(ie, x, y, px, py);
		}

		protected override void UpdateMe(float dt)
		{
			// flash a cell
			if ((OpenCells.Count > 0) && (RPGlobals.g.RNG.NextDouble() <= Hacking.ActivityLevel))
			{
				int cid = RPGlobals.g.RNG.Next() % OpenCells.Count;
				int c = OpenCells[cid];
				OpenCells.RemoveAt(cid);
				int w = c % Cells.GetLength(0);
				int h = c / Cells.GetLength(0);
				Cells[w,h].Flashing = true;
				Cells[w,h].FlashStartTime = RPGlobals.g.RunningTime;
				Cells[w,h].FlashEndTime = RPGlobals.g.RunningTime + Hacking.ConnectionValidationTimeMin + (float)RPGlobals.g.RNG.NextDouble() % Hacking.ConnectionValidationTimeRange;
			}

			if (NumCellsLocked > 0)
			{
				BypassProgress += dt * NumCellsLocked * ProgressPerCell;
				if (BypassProgress >= Hacking.TrafficComplexity) ClosePressed(null);
			}

			base.UpdateMe(dt);
		}

		protected override void RenderMe(float px, float py)
		{
			// draw all grid lines
			Vector2 ls, le;
			// vertical
			for (int i = 0; i <= Cells.GetLength(0); i++)
			{
				ls.X = GridOrigin.X + i * CellSize.X + Position.X;
				ls.Y = GridOrigin.Y + Position.Y;
				le.X = ls.X;
				le.Y = ls.Y + GridSize.Y;
				Renderer.r.DrawLine(ls.X, ls.Y, le.X, le.Y, 1, GridLineColor);
			}
			// horizontal
			for (int i = 0; i <= Cells.GetLength(1); i++)
			{
				ls.X = GridOrigin.X + Position.X;
				ls.Y = GridOrigin.Y + i * CellSize.Y + Position.Y;
				le.X = ls.X + GridSize.X;
				le.Y = ls.Y;
				Renderer.r.DrawLine(ls.X, ls.Y, le.X, le.Y, 1, GridLineColor);
			}

			// iterate over cells
			string s;
			for (int i = 0; i < Cells.GetLength(0); i++)
				for (int j = 0; j < Cells.GetLength(1); j++)
				{
					ls.X = Position.X + GridOrigin.X + i * CellSize.X;
					ls.Y = Position.Y + GridOrigin.Y + j * CellSize.Y;
					le.X = ls.X + CellSize.X - 1;
					le.Y = ls.Y + CellSize.Y - 1;
					if (Cells[i,j].Error)
					{
						s = "XX";
						GL.Color4(ErrorColor.R, ErrorColor.G, ErrorColor.B, 255);
					}
					else
					{
						s = "" + CellValues[RPGlobals.g.RNG.Next(CellValues.Length)] + CellValues[RPGlobals.g.RNG.Next(CellValues.Length)];
						GL.Color4(GridLineColor.R, GridLineColor.G, GridLineColor.B, 255);
					}
					Renderer.r.DrawText(s, CellFont, ls.X + CellPadding.X, ls.Y + CellPadding.Y);

					Color bgc = Color.Black;
					if (Cells[i,j].Flashing)
					{
						if (RPGlobals.g.RunningTime >= Cells[i,j].FlashEndTime)
						{
							Cells[i,j].Flashing = false;
							OpenCells.Add(i + j * Cells.GetLength(0));
							if (Cells[i,j].Valid)
							{
								Cells[i,j].Valid = false;
								NumCellsLocked--;
							}
						}
						else
						{
							if (Cells[i,j].Valid) bgc = InterpolationHelper.Linear_Interp_Color(ValidColor, bgc, Cells[i,j].FlashStartTime, Cells[i,j].FlashEndTime);
							else if (Cells[i,j].Error) bgc = InterpolationHelper.Linear_Interp_Color(ErrorColor, bgc, Cells[i,j].FlashStartTime, Cells[i,j].FlashEndTime);
							else bgc = InterpolationHelper.Linear_Interp_Color(FlashColor, bgc, Cells[i,j].FlashStartTime, Cells[i,j].FlashEndTime);
						}
					}

					GL.Color4(bgc.R, bgc.G, bgc.B, 255);
					Renderer.r.DrawUI(Renderer.r.White, ls.X, ls.Y, (CellSize.X - 1) / 2, (CellSize.Y - 1) / 2, 0);
				}

			// draw progress bar
			// frame
			ls.X = BarOrigin.X + Position.X;
			ls.Y = BarOrigin.Y + Position.Y;
			le.X = ls.X;
			le.Y = ls.Y + BarSize.Y;
			Renderer.r.DrawLine(ls.X, ls.Y, le.X, le.Y, 1, GridLineColor);
			le.X = ls.X + BarSize.X;
			le.Y = ls.Y;
			Renderer.r.DrawLine(ls.X, ls.Y, le.X, le.Y, 1, GridLineColor);
			le.Y = ls.Y + BarSize.Y;
			ls.Y += BarSize.Y;
			Renderer.r.DrawLine(ls.X, ls.Y, le.X, le.Y, 1, GridLineColor);
			ls.X += BarSize.X;
			ls.Y = BarOrigin.Y + Position.Y;
			Renderer.r.DrawLine(ls.X, ls.Y, le.X, le.Y, 1, GridLineColor);
			// text
			if (NumCellsLocked > 0) s = "Analyzing...";
			else s = "Waiting connections...";
			ls.X = BarOrigin.X + Position.X;
			ls.Y = BarOrigin.Y + Position.Y - TextFont.CharHeight;
			Renderer.r.DrawText(s, TextFont, ls.X, ls.Y);
			// progress bar
			if (BypassProgress > 0)
			{
				ls.Y = BarOrigin.Y + Position.Y;
				if (BypassProgress >= Hacking.TrafficComplexity) le.X = BarSize.X;
				else le.X = BypassProgress / Hacking.TrafficComplexity * BarSize.X;
				GL.Color4(ValidColor.R, ValidColor.G, ValidColor.B, 255);
				Renderer.r.DrawUI(Renderer.r.White, ls.X, ls.Y, le.X / 2, BarSize.Y / 2, 0);
			}

			base.RenderMe(px, py);
		}
	}
}

