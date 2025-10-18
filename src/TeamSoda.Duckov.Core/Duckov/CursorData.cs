using System;
using UnityEngine;

namespace Duckov;

[Serializable]
public class CursorData
{
	public Texture2D[] textures;

	public Vector2 hotspot;

	public float fps;

	public Texture2D texture
	{
		get
		{
			if (textures.Length == 0)
			{
				return null;
			}
			return textures[0];
		}
	}

	internal void Apply(int frame)
	{
		if (textures == null || textures.Length < 1)
		{
			Cursor.SetCursor(null, default(Vector2), CursorMode.Auto);
			return;
		}
		if (frame < 0)
		{
			int num = textures.Length;
			frame = (-frame / textures.Length + 1) * num + frame;
		}
		frame %= textures.Length;
		Cursor.SetCursor(textures[frame], hotspot, CursorMode.Auto);
	}
}
