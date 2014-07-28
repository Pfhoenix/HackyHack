using System;

namespace HackyHack
{
	public class TextureInfo
	{
		public readonly uint TexID; // OpenGL handle
		public readonly int Width;
		public readonly int Height;

		public TextureInfo(uint tid, int w, int h)
		{
			TexID = tid;
			Width = w;
			Height = h;
		}
	}

	public class Texture
	{
		public readonly string Name;
		public readonly TextureInfo NativeInfo;
		public int Width;
		public int Height;
		public float[] UVs;

		public Texture(string n, TextureInfo ni, float[] uvs)
		{
			Name = n;
			NativeInfo = ni;
			Width = ni.Width;
			Height = ni.Height;

			if (uvs == null)
			{
				UVs = new float[] {
					0, 0,
					1, 0,
					0, 1,
					1, 1
				};
			}
			else UVs = uvs;
		}

		public void SetUVsByCoords(int x, int y, int w, int h)
		{
			Width = w;
			Height = h;
			UVs[0] = x / (float)NativeInfo.Width;
			UVs[1] = y / (float)NativeInfo.Height;
			UVs[2] = UVs[0] + w / (float)NativeInfo.Width;
			UVs[3] = UVs[1];
			UVs[4] = UVs[0];
			UVs[5] = UVs[1] + h / (float)NativeInfo.Height;
			UVs[6] = UVs[2];
			UVs[7] = UVs[5];
		}
	}
}

